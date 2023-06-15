using UnityEngine;
using RWCustom;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ExpeditionEnhanced.ExampleContent
{
    public class ExamplePerkHooks
    {
        public static void Apply()
        {
            //Explosive damage perk
            On.SocialEventRecognizer.WeaponAttack += SocialEventRecognizer_WeaponAttack;

            //Make a wish perk
            On.Player.ReleaseGrasp += Player_ReleaseGrasp;
            On.DataPearl.PickedUp += DataPearl_PickedUp;
            On.DataPearl.Update += DataPearl_Update;

            //Saint tongue perk
            On.Player.ctor += Player_ctor;
            On.Player.SaintTongueCheck += Player_SaintTongueCheck;
            On.Player.ClassMechanicsSaint += Player_ClassMechanicsSaint;
            On.PlayerGraphics.ctor += PlayerGraphics_ctor;
            On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            On.PlayerGraphics.ApplyPalette += PlayerGraphics_ApplyPalette;
            On.PlayerGraphics.MSCUpdate += PlayerGraphics_MSCUpdate;
        }

        //I dont know if this is fun and/or balanced
        public static void SocialEventRecognizer_WeaponAttack(On.SocialEventRecognizer.orig_WeaponAttack orig, SocialEventRecognizer self, PhysicalObject weapon, Creature thrower, Creature victim, bool hit)
        {
            orig.Invoke(self, weapon, thrower, victim, hit);

            if (ExpeditionsEnhanced.ActiveContent("unl-boomdamage") && weapon != null && thrower != null && victim != null && thrower is Player && hit)
            {
                //Make sure its an actual hit (no phantom explosions), and that bombs dont create splosions
                if (weapon is not ScavengerBomb and not SingularityBomb)
                {
                    //Strength based on the weapon type
                    float factor = 2f;
                    if (weapon is Spear s)
                    {
                        factor = 2.4f;
                        if (s is ExplosiveSpear || s is ElectricSpear)
                        {
                            factor = 3f;
                        }
                        factor *= s.spearDamageBonus;
                    }
                    if (weapon is Bullet) factor = 0.2f;

                    //Stolen code from ScavengerBomb.Explode with tweaked values
                    Vector2 pos = weapon.firstChunk.pos;
                    self.room.AddObject(new Explosion(self.room, weapon, pos, 7, 30f * factor, factor * 2.5f, 0.15f * factor, 20f * factor, 0f, thrower, 0.3f * factor, 10f * factor, 0.1f * factor));
                    self.room.AddObject(new Explosion.ExplosionLight(pos, 30f * factor, 1f, 3, Color.white));
                    self.room.AddObject(new ExplosionSpikes(self.room, pos, (int)(3 * factor), 30f * factor, 9f, 7f, 10f * factor, new Color(0.01f, 0.01f, 0.01f)));
                    self.room.AddObject(new ShockWave(pos, 30f * factor, 0.04f * factor, 5, false));

                    for (int j = 0; j < 5; j++)
                    {
                        self.room.AddObject(new Spark(pos, Custom.RNV() * Mathf.Lerp(5f, 10f, UnityEngine.Random.value) * factor + Custom.RNV() * 10f * UnityEngine.Random.value, Color.Lerp(Color.gray, Color.white, UnityEngine.Random.value), null, 10 * (int)factor, 15 * (int)factor));
                    }
                    self.room.PlaySound(SoundID.Fire_Spear_Explode, pos, 0.4f, 1.1f - (0.15f * factor));
                }
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
                AbstractPhysicalObject.AbstractObjectType.KarmaFlower,
                AbstractPhysicalObject.AbstractObjectType.Rock,
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
                        prizeType = prizes[UnityEngine.Random.Range(0, prizes.Count)];
                        var prize = CustomPerk.GetCorrectAPO(prizeType, room, room.GetWorldCoordinate(pos));
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



        //Creating the saint tongue
        public static void Player_ctor(On.Player.orig_ctor orig, Player player, AbstractCreature abstractCreature, World world)
        {
            orig.Invoke(player, abstractCreature, world);

            if (ExpeditionsEnhanced.ActiveContent("unl-stongue") && player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                player.tongue = new Player.Tongue(player, 0);
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
        public static Dictionary<PlayerGraphics, int> TongueSpriteIndex = new();

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
                if (TongueSpriteIndex.ContainsKey(self)) TongueSpriteIndex[self] = sLeaser.sprites.Length - 1;
                else TongueSpriteIndex.Add(self, sLeaser.sprites.Length - 1);

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

            if (ExpeditionsEnhanced.ActiveContent("unl-stongue") && self.player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint && TongueSpriteIndex.ContainsKey(self))
            {
                //Relevant parts copied from the game's code
                float a = 0.95f;
                float b = 1f;
                float sl = 1f;
                float a2 = 0.75f;
                float b2 = 0.9f;
                for (int j = 0; j < (sLeaser.sprites[TongueSpriteIndex[self]] as TriangleMesh).verticeColors.Length; j++)
                {
                    float num2 = Mathf.Clamp(Mathf.Sin((float)j / (float)((sLeaser.sprites[TongueSpriteIndex[self]] as TriangleMesh).verticeColors.Length - 1) * 3.1415927f), 0f, 1f);
                    (sLeaser.sprites[TongueSpriteIndex[self]] as TriangleMesh).verticeColors[j] = Color.Lerp(palette.fogColor, Custom.HSL2RGB(Mathf.Lerp(a, b, num2), sl, Mathf.Lerp(a2, b2, Mathf.Pow(num2, 0.15f))), 0.7f);
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
