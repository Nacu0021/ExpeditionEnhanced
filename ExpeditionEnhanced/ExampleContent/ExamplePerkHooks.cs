using UnityEngine;
using RWCustom;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace ExpeditionEnhanced.ExampleContent
{
    public class ExamplePerkHooks
    {
        public static void Player_Jump(On.Player.orig_Jump orig, Player self)
        {
            int j = self.superLaunchJump;
            orig.Invoke(self);

            if (ExpeditionsEnhanced.ActiveContent("unl-spikes") && (self.flipFromSlide || (j == 20 && self.superLaunchJump == 0) || self.animation == Player.AnimationIndex.RocketJump))
            {
                var spikePos = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(self.room, self.bodyChunks[0].lastPos, self.bodyChunks[0].lastPos - new Vector2(0f, 1000f));
                if (spikePos.HasValue)
                {
                    Vector2 spikePosPos = self.room.MiddleOfTile(spikePos.Value) - new Vector2(0f, 10f);
                    self.room.AddObject(new ExampleBurdenHooks.GroundSpike(spikePosPos, UnityEngine.Random.Range(10, 15), self.mainBodyChunk.pos + (Vector2)Vector3.Slerp(Custom.DirVec(self.room.MiddleOfTile(spikePos.Value), self.mainBodyChunk.pos), Vector2.left, 0.4f + 0.2f * UnityEngine.Random.value) * UnityEngine.Random.Range(80, 160))
                    {
                        ignorePlayer = true,
                        killTagHolder = self.abstractCreature
                    });
                    self.room.AddObject(new ExampleBurdenHooks.GroundSpike(spikePosPos, UnityEngine.Random.Range(10, 15), self.mainBodyChunk.pos + (Vector2)Vector3.Slerp(Custom.DirVec(self.room.MiddleOfTile(spikePos.Value), self.mainBodyChunk.pos), Vector2.right, 0.4f + 0.2f * UnityEngine.Random.value) * UnityEngine.Random.Range(80, 160))
                    {
                        ignorePlayer = true,
                        killTagHolder = self.abstractCreature
                    });
                    self.room.AddObject(new ExampleBurdenHooks.GroundSpike(spikePosPos, UnityEngine.Random.Range(10, 18), self.mainBodyChunk.pos + Custom.DirVec(self.room.MiddleOfTile(spikePos.Value), self.mainBodyChunk.pos) * UnityEngine.Random.Range(100, 200))
                    {
                        ignorePlayer = true,
                        killTagHolder = self.abstractCreature
                    });
                }
            }
        }

        public static ConditionalWeakTable<Player, PlayerAndWeapon> TpWeapon = new ConditionalWeakTable<Player, PlayerAndWeapon>();

        public static void Player_ctor(On.Player.orig_ctor orig, Player player, AbstractCreature abstractCreature, World world)
        {
            orig.Invoke(player, abstractCreature, world);

            //Creating the saint tongue
            if (ExpeditionsEnhanced.ActiveContent("unl-stongue") && player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                player.tongue = new Player.Tongue(player, 0);
            }

            //Initializing the cooldown for thunder god
            if (ExpeditionsEnhanced.ActiveContent("unl-thundergod"))
            {
                if (!TpWeapon.TryGetValue(player, out _)) TpWeapon.Add(player, new(player));
            }
        }
        public static void Player_ThrowObject(ILContext il)
        {
            ILCursor c = new(il);
            if (c.TryGotoNext(
                x => x.MatchCallOrCallvirt<Player>("Blink")))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Action<Player, int>>((player, grasp) =>
                {
                    if (ExpeditionsEnhanced.ActiveContent("unl-thundergod"))
                    {
                        if (TpWeapon.TryGetValue(player, out var tp) && tp.ready)
                        {
                            tp.w = player.grasps[grasp].grabbed as Weapon;
                        }
                    }
                });
            }
            else Plugin.logger.LogError("The throw thing doesnt work mr man: " + il);
        }

        public static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            orig.Invoke(self, eu);

            if (TpWeapon.TryGetValue(self, out var tp))
            {
                tp.Update();
            }
        }

        public class PlayerAndWeapon
        {
            public Player self;
            public Weapon w;
            public bool ready;
            public int counter;
            public int maxCounter;

            public PlayerAndWeapon(Player p)
            {
                self = p;
                ready = true;
                maxCounter = 100;
            }

            public void Update()
            {
                if (self != null && self.mainBodyChunk != null && self.room != null)
                {
                    if (counter > 0)
                    {
                        counter--;
                        if (counter == 0)
                        {
                            ready = true;
                            self.room.AddObject(new MarkFlash("symbol_thundergod", self.firstChunk.pos + Custom.DirVec(self.mainBodyChunk.pos, self.firstChunk.pos) * self.firstChunk.rad * 4f, new Color(0.615f, 0.925f, 0.960f), 0.8f));
                        }
                    }
                }

                if (w != null && ready)
                {
                    bool less = Custom.DistLess(w.firstChunk.pos, self.mainBodyChunk.pos, 180f);
                    IntVector2 possum = self.room.GetTilePosition(w.firstChunk.pos);
                    if (w.room == null || w.room != self.room || ((less || possum.x < 0 || possum.x > self.room.TileWidth) && !self.input[0].thrw))
                    {
                        w = null;
                    }
                    else if (!self.input[0].thrw && possum.x >= 0 && possum.x < self.room.TileWidth)
                    {
                        float dist = Vector2.Distance(self.mainBodyChunk.pos, w.firstChunk.pos);
                        Vector2 playerPos = self.mainBodyChunk.pos;

                        self.room.AddObject(new StaticElectricty(self.mainBodyChunk.pos, w.firstChunk.pos, Mathf.Lerp(4f, 6f, UnityEngine.Random.value), 5f, new Color(0.615f, 0.925f, 0.960f), Vector2.Distance(self.mainBodyChunk.pos, w.firstChunk.pos) / 7f));
                        self.room.PlaySound(SoundID.Death_Lightning_Spark_Spontaneous, w.firstChunk.pos, 1f, Mathf.Lerp(0.9f, 1.25f, UnityEngine.Random.value));
                        self.room.AddObject(new Explosion.ExplosionLight(self.mainBodyChunk.pos, dist / 5f, 0.66f, 16, StaticElectricty.RandomizeColorABit(new Color(0.615f, 0.925f, 0.960f))));
                        
                        for (int i = 0; i < UnityEngine.Random.Range(5, 10); i++)
                        {
                            self.room.AddObject(new Spark(self.mainBodyChunk.pos, Custom.RNV() * dist / 50f, StaticElectricty.RandomizeColorABit(new Color(0.615f, 0.925f, 0.960f)), null, 20, 40));
                        }

                        foreach (var chunk in self.bodyChunks)
                        {
                            self.room.AddObject(new StaticElectricty(chunk.pos, w.firstChunk.pos, 1f, 1f, new Color(0.615f, 0.925f, 0.960f), dist / 5f));
                            chunk.HardSetPosition(w.firstChunk.pos);
                            chunk.vel *= 0f;
                        }

                        self.room.AddObject(new Explosion.ExplosionLight(self.mainBodyChunk.pos, dist / 3f, 0.66f, 12, StaticElectricty.RandomizeColorABit(new Color(0.615f, 0.925f, 0.960f))));

                        if (self.graphicsModule != null)
                        {
                            var grr = self.graphicsModule as PlayerGraphics;

                            foreach (var part in grr.bodyParts)
                            {
                                part.pos = w.firstChunk.pos;
                                part.lastPos = part.pos;
                            }

                            foreach (var part in grr.tail)
                            {
                                part.pos = w.firstChunk.pos;
                                part.lastPos = part.pos;
                            }
                        }

                        w.firstChunk.HardSetPosition(playerPos);
                        if (w.mode == Weapon.Mode.Thrown) 
                        {
                            w.throwDir = new IntVector2(-w.throwDir.x, -w.throwDir.y);
                            w.firstChunk.vel = -w.firstChunk.vel;
                            w.setRotation = new Vector2?(w.throwDir.ToVector2());
                        }
                        if (w is Spear spear && spear.stuckInObject != null)
                        {
                            foreach (BodyChunk chunj in spear.stuckInObject.bodyChunks)
                            {
                                chunj.HardSetPosition(playerPos);
                            }
                        }

                        ready = false;
                        w = null;
                        counter = maxCounter;
                    }
                    else if (UnityEngine.Random.value < 0.33f && !less)
                    {
                        self.room.AddObject(new StaticElectricty(self.mainBodyChunk.pos, w.firstChunk.pos, 1f, 1f, new Color(0.615f, 0.925f, 0.960f), Vector2.Distance(self.mainBodyChunk.pos, w.firstChunk.pos) / 10f));
                        self.room.PlaySound(SoundID.Spore_Bee_Spark, self.mainBodyChunk.pos, 0.6f, 1.25f);
                    }
                }
            }

            public class MarkFlash : CosmeticSprite
            {
                public string sprite;
                public Color color;
                public float life;
                public float maxLife;
                public bool shitGoBack;

                public MarkFlash(string spriteName, Vector2 pos, Color color, float lifeTime)
                {
                    this.pos = pos;
                    lastPos = pos;
                    this.color = color;
                    life = 0f;
                    maxLife = lifeTime;
                    sprite = spriteName;
                }

                public override void Update(bool eu)
                {
                    base.Update(eu);

                    life = shitGoBack ? Mathf.Max(0f, life - 0.12f) : Mathf.Min(maxLife, life + 0.12f);
                    if (life == maxLife)
                    {
                        shitGoBack = true;
                        for (int i = 0; i < UnityEngine.Random.Range(5, 10); i++)
                        {
                            room.AddObject(new Spark(pos + Custom.RNV() * 10f * UnityEngine.Random.value, Custom.RNV() * 8f * UnityEngine.Random.value, StaticElectricty.RandomizeColorABit(new Color(0.615f, 0.925f, 0.960f)), null, 15, 25));
                        }
                        room.AddObject(new Explosion.ExplosionLight(pos, 100f, 0.5f, 10, StaticElectricty.RandomizeColorABit(new Color(0.615f, 0.925f, 0.960f))));
                        room.PlaySound(SoundID.Spore_Bee_Spark, pos, 2f, 3f);
                    }
                    if (shitGoBack && life == 0f) Destroy();
                }

                public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
                {
                    sLeaser.sprites = new FSprite[1];
                    sLeaser.sprites[0] = new FSprite(sprite, true)
                    {
                        color = color,
                        alpha = 0
                    };
                    AddToContainer(sLeaser, rCam, null);
                    base.InitiateSprites(sLeaser, rCam);
                }

                public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
                {
                    base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

                    Vector2 poss = Vector2.Lerp(lastPos, pos, timeStacker);
                    sLeaser.sprites[0].SetPosition(poss - camPos);
                    sLeaser.sprites[0].alpha = Custom.LerpMap(life, 0f, maxLife, 0f, 1f);
                    sLeaser.sprites[0].scale = Custom.LerpMap(life * 0.66f, 0f, maxLife, 0.33f, 1f);
                }
            }
        }

        public class StaticElectricty : CosmeticSprite
        {
            public Color color;
            public Vector2 goalPos;
            public float inBetweenPoint;
            public float deviation;
            public float lifeTime;
            public float width;
            public float maxlifeTime;

            public StaticElectricty(Vector2 pos, Vector2 goalPos, float width, float lifeTime, Color color, float maxDeviation)
            {
                this.pos = pos;
                lastPos = pos;
                this.goalPos = goalPos;
                maxlifeTime = lifeTime;
                this.lifeTime = lifeTime;
                inBetweenPoint = Mathf.Lerp(0.2f, 0.8f, UnityEngine.Random.value);
                deviation = Mathf.Lerp(-maxDeviation, maxDeviation, UnityEngine.Random.value);
                this.width = width;
                this.color = RandomizeColorABit(color);
            }

            public static Color RandomizeColorABit(Color color, float factor1 = 0.1f, float factor2 = 0.1f)
            {
                var hslcolor = Custom.RGB2HSL(color);
                color = Custom.HSL2RGB(Custom.WrappedRandomVariation(hslcolor.x, factor1, factor2), hslcolor.y, Custom.ClampedRandomVariation(hslcolor.z, factor1, factor2));
                return color;
            }

            public override void Update(bool eu)
            {
                base.Update(eu);

                lifeTime = Mathf.Max(0f, lifeTime - 0.1f);
                if (lifeTime == 0f)
                {
                    Destroy();
                }
            }

            public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.sprites = new FSprite[2];
                sLeaser.sprites[0] = new FSprite("pixel", true)
                {
                    anchorX = 0f,
                    anchorY = 0.5f,
                    scaleY = width,
                    shader = rCam.game.rainWorld.Shaders["Hologram"]
                };
                sLeaser.sprites[1] = new FSprite("pixel", true)
                {
                    anchorX = 0f,
                    anchorY = 0.5f,
                    scaleY = width,
                    shader = rCam.game.rainWorld.Shaders["Hologram"]
                };
                AddToContainer(sLeaser, rCam, null);
            }

            public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                Vector2 deviousPos = Vector2.Lerp(pos, goalPos, inBetweenPoint) + Custom.PerpendicularVector(pos, goalPos) * deviation;
                float alpf = Custom.LerpMap(lifeTime, maxlifeTime, 0f, 1f, 0f, 0.9f);

                sLeaser.sprites[0].scaleX = Vector2.Distance(pos, deviousPos);
                sLeaser.sprites[1].scaleX = Vector2.Distance(goalPos, deviousPos);
                sLeaser.sprites[0].scaleY = width * alpf;
                sLeaser.sprites[1].scaleY = width * alpf;
                sLeaser.sprites[0].SetPosition(pos - camPos);
                sLeaser.sprites[1].SetPosition(goalPos - camPos);
                sLeaser.sprites[0].rotation = Custom.VecToDeg(Custom.DirVec(pos, deviousPos)) - 90;
                sLeaser.sprites[1].rotation = Custom.VecToDeg(Custom.DirVec(goalPos, deviousPos)) - 90;
                sLeaser.sprites[0].alpha = alpf;
                sLeaser.sprites[1].alpha = alpf;

                base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            }

            public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
            {
                sLeaser.sprites[0].color = color;
                sLeaser.sprites[1].color = color;
            }
        }

        //This is kinda required soo we know to what point the prize spawner should move (player's position)
        public static ConditionalWeakTable<DataPearl, Player> WishfulThinking = new ConditionalWeakTable<DataPearl, Player>();
        //Adding the pearl to wishful thinking
        public static void Player_ReleaseGrasp(On.Player.orig_ReleaseGrasp orig, Player player, int grasp)
        {
            if (ExpeditionsEnhanced.ActiveContent("unl-makeawish"))
            {
                if (player.grasps[grasp] != null && player.grasps[grasp].grabbed is DataPearl pearl)
                {
                    if (!WishfulThinking.TryGetValue(pearl, out _))
                    {
                        WishfulThinking.Add(pearl, player);
                    }
                }
            }
            orig.Invoke(player, grasp);
        }

        //Remove from wishful thinkinj
        public static void DataPearl_PickedUp(On.DataPearl.orig_PickedUp orig, DataPearl self, Creature upPicker)
        {
            orig.Invoke(self, upPicker);

            if (ExpeditionsEnhanced.ActiveContent("unl-makeawish"))
            {
                if (WishfulThinking.TryGetValue(self, out _)) WishfulThinking.Remove(self);
            }
        }

        //Spawn the prize spawner when in the death pit
        public static void DataPearl_Update(On.DataPearl.orig_Update orig, DataPearl self, bool eu)
        {
            if (ExpeditionsEnhanced.ActiveContent("unl-makeawish"))
            {
                if (WishfulThinking.TryGetValue(self, out var g) && g != null && self.room != null)
                {
                    if (self.firstChunk.pos.y < 0)
                    {
                        self.room.AddObject(new PrizeWoah(self.firstChunk.pos, g.mainBodyChunk.pos));
                        self.Destroy();
                    }
                }
            }

            orig.Invoke(self, eu);
        }

        //The prize spawner, really simple class (i stole code from something else i made heho) that just moves along a bezier curve and then spawns objects
        //Also shows how you can use CustomPerk.GetCorrectAPO in other places
        public class PrizeWoah : CosmeticSprite
        {
            public Vector2 startPos;
            public Vector2 playerPos;
            public float t;
            public AbstractPhysicalObject.AbstractObjectType prizeType;
            public int prizess;

            public static List<AbstractPhysicalObject.AbstractObjectType> prizes = new List<AbstractPhysicalObject.AbstractObjectType>
            {
                AbstractPhysicalObject.AbstractObjectType.FlareBomb,
                AbstractPhysicalObject.AbstractObjectType.PuffBall,
                AbstractPhysicalObject.AbstractObjectType.ScavengerBomb,
                AbstractPhysicalObject.AbstractObjectType.VultureMask,
                AbstractPhysicalObject.AbstractObjectType.Lantern,
                AbstractPhysicalObject.AbstractObjectType.Spear,
                //AbstractPhysicalObject.AbstractObjectType.KarmaFlower, 
                AbstractPhysicalObject.AbstractObjectType.Rock,
                new AbstractPhysicalObject.AbstractObjectType("FireEgg", false),
                new AbstractPhysicalObject.AbstractObjectType("LillyPuck", false),
            };

            public PrizeWoah(Vector2 startPos, Vector2 playerPos)
            {
                this.startPos = startPos;
                this.playerPos = playerPos;
                pos = startPos;
                lastPos = pos;
                t = 1f;
                prizess = UnityEngine.Random.Range(1, 4);
            }

            public override void Update(bool eu)
            {
                base.Update(eu);
                if (room == null) return;
                t = Mathf.Max(0f, t - 0.01f);
                Vector2 halfPos = new Vector2((startPos.x + playerPos.x) / 2f, playerPos.y + 40f);
                pos = Custom.Bezier(playerPos, halfPos, startPos, halfPos, t);

                room.AddObject(new CollectToken.TokenSpark(pos, Custom.RNV() * (10f * UnityEngine.Random.value), Color.white, false));
                if (UnityEngine.Random.value < 0.2f) room.AddObject(new Spark(pos, Custom.RNV() * (15f * UnityEngine.Random.value), Color.white, null, 20, 50));

                if (t == 0f)
                {
                    for (int i = 0; i < prizess; i++)
                    {
                        prizeType = prizes[UnityEngine.Random.Range(0, prizes.Count - (ModManager.MSC ? 0 : 2))];
                        var prize = EECustomPerk.GetCorrectAPO(prizeType, room, room.GetWorldCoordinate(pos));
                        prize.RealizeInRoom();
                        prize.realizedObject.firstChunk.lastPos = prize.realizedObject.firstChunk.pos;
                    }
                    for (int s = 0; s < UnityEngine.Random.Range(3, 9); s++)
                    {
                        room.AddObject(new Spark(pos, Custom.RNV() * 20f, Color.white, null, 30, 100));
                    }
                    for (int i = 0; i < UnityEngine.Random.Range(9, 16); i++)
                    {
                        room.AddObject(new CollectToken.TokenSpark(pos + Custom.RNV() * (25f * UnityEngine.Random.value), Custom.RNV() * (10f * UnityEngine.Random.value), Color.white, false));
                    }
                    room.PlaySound(SoundID.Moon_Wake_Up_Swarmer_Ping, pos, 1.2f, 0.9f + UnityEngine.Random.value / 8f);
                    Destroy();
                }
            }

            //The game throws an exception with no sLeaser.sprites
            public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                base.InitiateSprites(sLeaser, rCam);
                sLeaser.sprites = new FSprite[1];
                sLeaser.sprites[0] = new FSprite("pixel", true)
                {
                    alpha = 0f
                };
                AddToContainer(sLeaser, rCam, null);
            }
        }

        //A method used in various(2) places to check if the tongue can be shot
        public static bool Player_SaintTongueCheck(On.Player.orig_SaintTongueCheck orig, Player player)
        {
            if (ExpeditionsEnhanced.ActiveContent("unl-stongue") && player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                return player.Consious && player.tongue.mode == Player.Tongue.Mode.Retracted && player.bodyMode != Player.BodyModeIndex.CorridorClimb && !player.corridorDrop && player.bodyMode != Player.BodyModeIndex.ClimbIntoShortCut && player.bodyMode != Player.BodyModeIndex.WallClimb && player.bodyMode != Player.BodyModeIndex.Swimming && player.animation != Player.AnimationIndex.VineGrab && player.animation != Player.AnimationIndex.ZeroGPoleGrab;
            }

            return orig.Invoke(player);
        }

        //Checking for player tongue imput (the default, not the Remix old tongue controls)
        //((the Remix old tongue controls only require the SaintTongueCheck hook))
        public static void Player_ClassMechanicsSaint(On.Player.orig_ClassMechanicsSaint orig, Player player)
        {
            orig.Invoke(player);

            if (ExpeditionsEnhanced.ActiveContent("unl-stongue") && player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                if (!MMF.cfgOldTongue.Value && player.input[0].jmp && !player.input[1].jmp && !player.input[0].pckp && player.canJump <= 0 && player.bodyMode != Player.BodyModeIndex.Crawl && player.animation != Player.AnimationIndex.ClimbOnBeam && player.animation != Player.AnimationIndex.AntlerClimb && player.animation != Player.AnimationIndex.HangFromBeam && player.SaintTongueCheck())
                {
                    Vector2 vector = new Vector2(player.flipDirection, 0.7f);
                    Vector2 normalized = vector.normalized;
                    if (player.input[0].y > 0)
                    {
                        normalized = new Vector2(0f, 1f);
                    }
                    normalized = (normalized + player.mainBodyChunk.vel.normalized * 0.2f).normalized;
                    player.tongue.Shoot(normalized);
                }
            }
        }

        //Initializing the ropeSegments for the tongue sprite
        public static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig.Invoke(self, ow);

            if (ExpeditionsEnhanced.ActiveContent("unl-stongue") && self.player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                self.ropeSegments = new PlayerGraphics.RopeSegment[20];
                for (int k = 0; k < self.ropeSegments.Length; k++)
                {
                    self.ropeSegments[k] = new PlayerGraphics.RopeSegment(k, self);
                }
            }
        }

        //This dictionary exists to make sure we always get the correct index of our new tongue sprite
        public static Dictionary<PlayerGraphics, int> TongueSpriteIndex = [];

        //Making it so theres a correct amount of sprites, cause the tongue is a sprite on the player
        public static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            //Note: the game's code uses sLeaser.sprite[12] for saint as the tongue

            orig.Invoke(self, sLeaser, rCam);

            //We dont want to add a sprite if the player is saint, since they already have a tongue
            if (ExpeditionsEnhanced.ActiveContent("unl-stongue") && self.player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                //We're adding one sprite
                Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 1);

                //Set the value of this dictionary to the last sprite
                TongueSpriteIndex[self] = sLeaser.sprites.Length - 1;

                //We make the last sprite of the array the tongue mesh
                sLeaser.sprites[TongueSpriteIndex[self]] = TriangleMesh.MakeLongMesh(self.ropeSegments.Length - 1, false, true);

                //Manually add the sprites to a container here cause it creates 1 less headache
                //Remove it first, cause it was added earlier (in the orig method)
                sLeaser.sprites[TongueSpriteIndex[self]].RemoveFromContainer();
                //Then add it to where we want it to be 
                rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[TongueSpriteIndex[self]]);
            }
        }

        //Updating the tongue
        public static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig.Invoke(self, sLeaser, rCam, timeStacker, camPos);

            if (ExpeditionsEnhanced.ActiveContent("unl-stongue") && self.player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                //All of self is copied from the game's code
                Vector2 vector = Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timeStacker);
                Vector2 vector2 = Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timeStacker);
                float b = Mathf.Lerp(self.lastStretch, self.stretch, timeStacker);
                vector = Vector2.Lerp(self.ropeSegments[0].lastPos, self.ropeSegments[0].pos, timeStacker);
                vector += Custom.DirVec(Vector2.Lerp(self.ropeSegments[1].lastPos, self.ropeSegments[1].pos, timeStacker), vector) * 1f;
                float num7 = 0f;
                for (int k = 1; k < self.ropeSegments.Length; k++)
                {
                    float num8 = (float)k / (float)(self.ropeSegments.Length - 1);
                    if (k >= self.ropeSegments.Length - 2)
                    {
                        vector2 = new Vector2(sLeaser.sprites[9].x + camPos.x, sLeaser.sprites[9].y + camPos.y);
                    }
                    else
                    {
                        vector2 = Vector2.Lerp(self.ropeSegments[k].lastPos, self.ropeSegments[k].pos, timeStacker);
                    }
                    Vector2 a2 = Custom.PerpendicularVector((vector - vector2).normalized);
                    float d4 = 0.2f + 1.6f * Mathf.Lerp(1f, b, Mathf.Pow(Mathf.Sin(num8 * 3.1415927f), 0.7f));
                    Vector2 vector11 = vector - a2 * d4;
                    Vector2 vector12 = vector2 + a2 * d4;
                    float num9 = Mathf.Sqrt(Mathf.Pow(vector11.x - vector12.x, 2f) + Mathf.Pow(vector11.y - vector12.y, 2f));
                    if (!float.IsNaN(num9))
                    {
                        num7 += num9;
                    }
                    (sLeaser.sprites[TongueSpriteIndex[self]] as TriangleMesh).MoveVertice((k - 1) * 4, vector11 - camPos);
                    (sLeaser.sprites[TongueSpriteIndex[self]] as TriangleMesh).MoveVertice((k - 1) * 4 + 1, vector + a2 * d4 - camPos);
                    (sLeaser.sprites[TongueSpriteIndex[self]] as TriangleMesh).MoveVertice((k - 1) * 4 + 2, vector2 - a2 * d4 - camPos);
                    (sLeaser.sprites[TongueSpriteIndex[self]] as TriangleMesh).MoveVertice((k - 1) * 4 + 3, vector12 - camPos);
                    vector = vector2;
                }
                if (self.player.tongue.Free || self.player.tongue.Attached)
                {
                    sLeaser.sprites[TongueSpriteIndex[self]].isVisible = true;
                }
                else
                {
                    sLeaser.sprites[TongueSpriteIndex[self]].isVisible = false;
                }
            }
        }

        //Coloring the tongue
        public static void PlayerGraphics_ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig.Invoke(self, sLeaser, rCam, palette);

            if (ExpeditionsEnhanced.ActiveContent("unl-stongue") && self != null && self.player != null && self.player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint && TongueSpriteIndex.ContainsKey(self) && sLeaser.sprites[TongueSpriteIndex[self]] is TriangleMesh mesh && mesh.verticeColors != null)
            {
                //Relevant parts copied from the game's code
                float a = 0.95f;
                float b = 1f;
                float sl = 1f;
                float a2 = 0.75f;
                float b2 = 0.9f;
                for (int j = 0; j < mesh.verticeColors.Length; j++)
                {
                    float num2 = Mathf.Clamp(Mathf.Sin((float)j / (float)(mesh.verticeColors.Length - 1) * 3.1415927f), 0f, 1f);
                    mesh.verticeColors[j] = Color.Lerp(palette.fogColor, Custom.HSL2RGB(Mathf.Lerp(a, b, num2), sl, Mathf.Lerp(a2, b2, Mathf.Pow(num2, 0.15f))), 0.7f);
                }
            }
        }

        //Necessary rope segment updates for the tongue sprite
        public static void PlayerGraphics_MSCUpdate(On.PlayerGraphics.orig_MSCUpdate orig, PlayerGraphics self)
        {
            orig.Invoke(self);

            if (ExpeditionsEnhanced.ActiveContent("unl-stongue") && self.player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                //Relevant saint portion copied from the game's code
                self.lastStretch = self.stretch;
                self.stretch = self.RopeStretchFac;
                List<Vector2> list = new List<Vector2>();
                for (int j = self.player.tongue.rope.TotalPositions - 1; j > 0; j--)
                {
                    list.Add(self.player.tongue.rope.GetPosition(j));
                }
                list.Add(self.player.mainBodyChunk.pos);
                float num = 0f;
                for (int k = 1; k < list.Count; k++)
                {
                    num += Vector2.Distance(list[k - 1], list[k]);
                }
                float num2 = 0f;
                for (int l = 0; l < list.Count; l++)
                {
                    if (l > 0)
                    {
                        num2 += Vector2.Distance(list[l - 1], list[l]);
                    }
                    self.AlignRope(num2 / num, list[l]);
                }
                for (int m = 0; m < self.ropeSegments.Length; m++)
                {
                    self.ropeSegments[m].Update();
                }
                for (int n = 1; n < self.ropeSegments.Length; n++)
                {
                    self.ConnectRopeSegments(n, n - 1);
                }
                for (int num3 = 0; num3 < self.ropeSegments.Length; num3++)
                {
                    self.ropeSegments[num3].claimedForBend = false;
                }
            }
        }
    }
}
