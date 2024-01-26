using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using HUD;
using UnityEngine;
using RWCustom;
using System.Collections.Generic;
using static Expedition.ExpeditionGame;
using System.Linq;
using DevConsole;

namespace ExpeditionEnhanced.ExampleContent
{
    public class ExampleBurdenHooks
    {
        public static int SpikesLeft;
        public static bool firstSpike;
        public static int SpikeEventCountdown;
        public static float GlobalSpikeEventDifficulty;
        public static float LocalSpikeEventDifficulty;

        public static void Apply()
        {
            //Crippled burden
            CrippledMechanics.Apply();

            //Confused
            On.HUD.RainMeter.ctor += RainMeter_Ctor;
            On.HUD.RainMeter.Draw += RainMeter_Draw;
            On.HUD.Map.Draw += Map_Draw;
            On.SaveState.BringUpToDate += SaveState_BringUpToDate;
            //Splosh
            On.Player.checkInput += Player_checkInput;

            //Marked
            On.ProcessManager.PostSwitchMainProcess += ProcessManager_PostSwitchMainProcess;

            //Volatile
            On.Creature.Die += Creature_Die;
            //Cosmetics for existing boom items
            On.ExplosiveSpear.ApplyPalette += ExplosiveSpear_ApplyPalette;
            IL.ExplosiveSpear.Explode += ExplosiveSpear_ExplodeIL;
            IL.ScavengerBomb.Explode += ScavengerBomb_ExplodeIL;
            On.ScavengerBomb.InitiateSprites += ScavengerBomb_InitiateSprites;
            On.ScavengerBomb.DrawSprites += ScavengerBomb_DrawSprites;
            On.ScavengerBomb.ApplyPalette += ScavengerBomb_ApplyPalette;
            On.ScavengerBomb.UpdateColor += ScavengerBomb_UpdateColor;
            On.ScavengerBomb.Thrown += ScavengerBomb_Thrown;
        }

        public static void ScavengerBomb_Thrown(On.ScavengerBomb.orig_Thrown orig, ScavengerBomb self, Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
        {
            orig.Invoke(self, thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);

            if (ExpeditionsEnhanced.ActiveContent("bur-volatile") || ExpeditionsEnhanced.ActiveContent("unl-explosivedamage"))
            {
                self.rotationSpeed = 20f * (UnityEngine.Random.value < 0.5f ? -1 : 1);
            }
        }

        public static void ScavengerBomb_InitiateSprites(On.ScavengerBomb.orig_InitiateSprites orig, ScavengerBomb self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            if (ExpeditionsEnhanced.ActiveContent("bur-volatile") || ExpeditionsEnhanced.ActiveContent("unl-explosivedamage"))
            {
                self.explodeColor = Volatile.VOLATILE_PINK;
                sLeaser.sprites = new FSprite[6];
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(self.abstractPhysicalObject.ID.RandomSeed);
                sLeaser.sprites[0] = new FSprite("Futile_White")
                {
                    scale = (self.firstChunk.rad + 0.75f) / 10f,
                    shader = rCam.game.rainWorld.Shaders["JaggedCircle"],
                    alpha = Mathf.Lerp(0.05f, 0.1f, UnityEngine.Random.value)
                };
                UnityEngine.Random.state = state;
                TriangleMesh.Triangle[] tris = [new TriangleMesh.Triangle(0, 1, 2)];
                for (int i = 1; i < 6; i++)
                {
                    sLeaser.sprites[i] = new TriangleMesh("Futile_White", tris, true, false);
                }
                self.AddToContainer(sLeaser, rCam, null);
            }
            else orig.Invoke(self, sLeaser, rCam);
        }

        public static void ScavengerBomb_DrawSprites(On.ScavengerBomb.orig_DrawSprites orig, ScavengerBomb self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (ExpeditionsEnhanced.ActiveContent("bur-volatile") || ExpeditionsEnhanced.ActiveContent("unl-explosivedamage"))
            {
                Vector2 vector = Vector2.Lerp(self.firstChunk.lastPos, self.firstChunk.pos, timeStacker);
                if (self.vibrate > 0)
                {
                    vector += Custom.DegToVec(UnityEngine.Random.value * 360f) * 2f * UnityEngine.Random.value;
                }
                float vector2 = Custom.VecToDeg(Vector3.Slerp(self.lastRotation, self.rotation, timeStacker));
                sLeaser.sprites[0].SetPosition(vector - camPos);
                sLeaser.sprites[0].rotation = vector2;
                Color color = Color.Lerp(Volatile.VOLATILE_PINK, self.color, Mathf.Pow(UnityEngine.Random.value, self.ignited ? 3f : 30f));

                for (int i = 0; i < 4; i++)
                {
                    Vector2 ror = Custom.DegToVec(vector2 + 90 * i);
                    VolatileBomb.MoveTriangle(sLeaser.sprites[i + 1] as TriangleMesh, vector + ror * sLeaser.sprites[0].scale - camPos, 3, 10, 1f, ror, true);
                    (sLeaser.sprites[i + 1] as TriangleMesh).verticeColors[0] = self.color;
                    (sLeaser.sprites[i + 1] as TriangleMesh).verticeColors[1] = color;
                    (sLeaser.sprites[i + 1] as TriangleMesh).verticeColors[2] = self.color;
                }

                if (self.mode == Weapon.Mode.Thrown)
                {
                    sLeaser.sprites[5].isVisible = true;
                    Vector2 vector3 = Vector2.Lerp(self.tailPos, self.firstChunk.lastPos, timeStacker);
                    Vector2 a = Custom.PerpendicularVector((vector - vector3).normalized);
                    (sLeaser.sprites[5] as TriangleMesh).MoveVertice(0, vector + a * 2f - camPos);
                    (sLeaser.sprites[5] as TriangleMesh).MoveVertice(1, vector - a * 2f - camPos);
                    (sLeaser.sprites[5] as TriangleMesh).MoveVertice(2, vector3 - camPos);
                    (sLeaser.sprites[5] as TriangleMesh).verticeColors[0] = self.color;
                    (sLeaser.sprites[5] as TriangleMesh).verticeColors[1] = self.color;
                    (sLeaser.sprites[5] as TriangleMesh).verticeColors[2] = self.explodeColor;
                }
                else
                {
                    sLeaser.sprites[5].isVisible = false;
                }

                if (self.blink > 0)
                {
                    if (self.blink > 1 && UnityEngine.Random.value < 0.5f)
                    {
                        self.UpdateColor(sLeaser, self.blinkColor);
                    }
                    else
                    {
                        self.UpdateColor(sLeaser, self.color);
                    }
                }
                else if (sLeaser.sprites[0].color != self.color)
                {
                    self.UpdateColor(sLeaser, self.color);
                }
                if (self.slatedForDeletetion || self.room != rCam.room)
                {
                    sLeaser.CleanSpritesAndRemove();
                }
            }
            else orig.Invoke(self, sLeaser, rCam, timeStacker, camPos);
        }

        public static void ScavengerBomb_ApplyPalette(On.ScavengerBomb.orig_ApplyPalette orig, ScavengerBomb self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (ExpeditionsEnhanced.ActiveContent("bur-volatile") || ExpeditionsEnhanced.ActiveContent("unl-explosivedamage"))
            {
                self.color = palette.blackColor;
                self.UpdateColor(sLeaser, self.color);
            }
            else orig.Invoke(self, sLeaser, rCam, palette);
        }

        public static void ScavengerBomb_UpdateColor(On.ScavengerBomb.orig_UpdateColor orig, ScavengerBomb self, RoomCamera.SpriteLeaser sLeaser, Color col)
        {
            if (ExpeditionsEnhanced.ActiveContent("bur-volatile") || ExpeditionsEnhanced.ActiveContent("unl-explosivedamage"))
            {
                sLeaser.sprites[0].color = col;
                for (int i = 0; i < 4; i++)
                {
                    (sLeaser.sprites[i + 1] as TriangleMesh).verticeColors[0] = col;
                    (sLeaser.sprites[i + 1] as TriangleMesh).verticeColors[2] = col;
                }
            }
            else orig.Invoke(self, sLeaser, col);
        }

        public static void ScavengerBomb_ExplodeIL(ILContext il)
        {
            ILCursor c = new(il);
            ILCursor l = new(il); // Label cursor
            ILLabel lable = l.DefineLabel();
            if (l.TryGotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt<Room>("ScreenMovement")
                ))
            {
                l.Index -= 8;
                l.MarkLabel(lable);
                Plugin.logger.LogMessage("halo1");
            }
            else Plugin.logger.LogMessage("ScavengerBomb_ExplodeIL LABEL FAILED " + il);

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchNewobj<Explosion>(),
                x => x.MatchCallOrCallvirt<Room>("AddObject")
                ))
            {
                Plugin.logger.LogMessage("halo2");
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc_0);
                c.EmitDelegate<Func<ScavengerBomb, Vector2, bool>>((self, vector) =>
                {
                    if (ExpeditionsEnhanced.ActiveContent("bur-volatile") || ExpeditionsEnhanced.ActiveContent("unl-explosivedamage"))
                    {
                        VolatileBomb.VolatileEffect(self.room, vector, 9f);
                        return true;
                    }
                    return false;
                });
                c.Emit(OpCodes.Brtrue, lable);
                Plugin.logger.LogMessage("halo3");
            }
            else Plugin.logger.LogMessage("ScavengerBomb_ExplodeIL CONTENT FAILED " + il);
        }

        public static void ExplosiveSpear_ExplodeIL(ILContext il)
        {
            ILCursor c = new(il);
            ILCursor l = new(il); // Label cursor
            ILLabel lable = l.DefineLabel();
            if (l.TryGotoNext(
                x => x.MatchStloc(5)
                ))
            {
                l.Index -= 1;
                l.MarkLabel(lable);
            }
            else Plugin.logger.LogMessage("ExplosiveSpear_ExplodeIL LABEL FAILED " + il);

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchNewobj<Explosion>(),
                x => x.MatchCallOrCallvirt<Room>("AddObject")
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc_0);
                c.EmitDelegate<Func<ExplosiveSpear, Vector2, bool>>((self, vector) =>
                {
                    if (ExpeditionsEnhanced.ActiveContent("bur-volatile") || ExpeditionsEnhanced.ActiveContent("unl-explosivedamage"))
                    {
                        VolatileBomb.VolatileEffect(self.room, vector, 5f);
                        return true;
                    }
                    return false;
                });
                c.Emit(OpCodes.Brtrue, lable);
            }
            else Plugin.logger.LogMessage("ExplosiveSpear_ExplodeIL CONTENT FAILED " + il);
        }

        public static void ExplosiveSpear_ApplyPalette(On.ExplosiveSpear.orig_ApplyPalette orig, ExplosiveSpear self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig.Invoke(self, sLeaser, rCam, palette);

            if (ExpeditionsEnhanced.ActiveContent("bur-volatile") || ExpeditionsEnhanced.ActiveContent("unl-explosivedamage"))
            {
                self.redColor = Color.Lerp(Volatile.VOLATILE_PINK, palette.blackColor, 0f + 0.7f * palette.darkness);
            }
        }

        public static void Creature_Die(On.Creature.orig_Die orig, Creature self)
        {
            if (!self.dead && ExpeditionsEnhanced.ActiveContent("bur-volatile"))
            {
                float mass = self.TotalMass;
                float div = Mathf.Lerp(Mathf.Max(0.1f, mass * 0.33f), mass, UnityEngine.Random.value);
                int i = 0;
                while (mass > 0f)
                {
                    float d;
                    if (mass - div > 0f)
                    {
                        mass -= div;
                        d = mass;
                    }
                    else
                    {
                        d = Mathf.Abs(mass - div);
                        mass = 0f;
                    }
                    switch (i)
                    {
                        case 0:
                            d *= 1.33f;
                            break;
                        case 1:
                            d *= 0.8f;
                            break;
                        case 2:
                            d *= 0.5f;
                            break;
                        default:
                            d *= 0.25f;
                            break;
                    }
                    self.room.AddObject(new Spark(self.mainBodyChunk.pos, Custom.RNV() * 10f * UnityEngine.Random.value, self.room.game.cameras[0].currentPalette.blackColor, null, 40, 70));
                    self.room.AddObject(new VolatileBomb(self.mainBodyChunk.pos, Vector2.ClampMagnitude(self.mainBodyChunk.vel, 10f) + (new Vector2(Custom.RNV().x, 0.2f + UnityEngine.Random.value * 1.25f) * (2f + UnityEngine.Random.value * 4f + d * 3f)), d * 1.3f));
                    i++;
                }
                self.room.PlaySound(SoundID.Spear_Dislodged_From_Creature, self.DangerPos, 1f, 0.8f);
            }

            orig.Invoke(self);
        }

        // Random shelter in region
        public static void SaveState_BringUpToDate(On.SaveState.orig_BringUpToDate orig, SaveState self, RainWorldGame game)
        {
            orig.Invoke(self, game);

            if (ExpeditionsEnhanced.ActiveContent("bur-confused"))
            {
                List<int> shelters = game.world.shelters.ToList();
                // Removing current shelter from evaluation
                int shelterToRemove = -1;

                if (shelters.Count > 1)
                {
                    foreach (int i in shelters)
                    {
                        if (RainWorld.roomIndexToName.ContainsKey(i))
                        {
                            if (RainWorld.roomIndexToName[i] == self.denPosition) shelterToRemove = i;
                        }
                    }
                }
                if (shelterToRemove != -1) shelters.Remove(shelterToRemove);

                int randoe = shelters[UnityEngine.Random.Range(0, shelters.Count)];
                if (RainWorld.roomIndexToName.ContainsKey(randoe))
                {
                    self.denPosition = RainWorld.roomIndexToName[randoe];
                }
            }
        }

        //Stop the rain meter from appearing and also from flashing at half the cycle time
        public static void RainMeter_Ctor(On.HUD.RainMeter.orig_ctor orig, RainMeter self, HUD.HUD hud, FContainer fContainer)
        {
            orig.Invoke(self, hud, fContainer);
            if (ExpeditionsEnhanced.ActiveContent("bur-confused")) self.halfTimeShown = true;
        }

        public static void RainMeter_Draw(On.HUD.RainMeter.orig_Draw orig, RainMeter self, float timeStacker)
        {
            orig.Invoke(self, timeStacker);
            if (ExpeditionsEnhanced.ActiveContent("bur-confused"))
            {
                for (int i = 0; i < self.circles.Length; i++)
                {
                    self.circles[i].sprite.alpha = 0f;
                }
            }
        }

        //Make the map invisible if confused burden
        public static void Map_Draw(On.HUD.Map.orig_Draw orig, Map self, float timeStacker)
        {
            if (ExpeditionsEnhanced.ActiveContent("bur-confused") && self.hud.rainWorld.processManager.currentMainLoop is not Menu.FastTravelScreen) self.visible = false;
            orig.Invoke(self, timeStacker);
        }

        //Sploosh
        public static void Player_checkInput(On.Player.orig_checkInput orig, Player self)
        {
            orig.Invoke(self);

            if (ExpeditionsEnhanced.ActiveContent("bur-confused") && self.room != null && self.input[0].mp && !self.input[1].mp) self.room.PlaySound(SoundID.Swollen_Water_Nut_Terrain_Impact, self.firstChunk);
        }

        //Marked burden
        //Setting up spike event variables
        public static void ProcessManager_PostSwitchMainProcess(On.ProcessManager.orig_PostSwitchMainProcess orig, ProcessManager self, ProcessManager.ProcessID ID)
        {
            orig.Invoke(self, ID);

            if (ExpeditionsEnhanced.ActiveContent("bur-marked") && self.currentMainLoop is RainWorldGame game)
            {
                // Randomise
                GlobalSpikeEventDifficulty = Custom.LerpMap(game.rainWorld.progression.currentSaveState.cycleNumber, 0, 20, 0f, 1f);
                SpikesLeft = 10 + (int)(20 * GlobalSpikeEventDifficulty);
                SpikeEventCountdown = UnityEngine.Random.Range(800, 1200);
                Plugin.logger.LogMessage(GlobalSpikeEventDifficulty + " " + SpikesLeft);
            }
        }

        //Spike event logic
        public class SpikeEventTracker : BurdenTracker
        {
            public bool EventActive;
            public int spikeCounter;
            public int maxSpikes;
            public int rumble;
            public bool reverse;
            public DisembodiedLoopEmitter emitter;

            public SpikeEventTracker(RainWorldGame game) : base(game)
            {
                this.game = game;
                spikeCounter = 0;
            }

            public override void Update()
            {
                base.Update();
                if (game == null) return;
                if (game.Players.Count < 1) return;

                if (SpikeEventCountdown-- > -1)
                {
                    if (SpikeEventCountdown == 0)
                    {
                        EventActive = true;
                        firstSpike = true;
                        maxSpikes = SpikesLeft;
                    }
                }

                if (EventActive)
                {
                    rumble = reverse ? Mathf.Max(rumble - 1, 0) : Mathf.Min(rumble + 1, 200);

                    for (int i = 0; i < game.cameras.Length; i++)
                    {
                        game.cameras[i].ScreenMovement(null, Custom.RNV() * 0.1f, rumble / 1200f);
                    }

                    //if (!reverse && rumble < 100) return;
                    if (game.cameras[0].room == null) return;

                    emitter ??= game.cameras[0].room.PlayDisembodiedLoop(Plugin.WarningSound, 0f, 1f, 0f);
                    emitter.volume = Mathf.InverseLerp(0, 300, rumble);
                    emitter.pan = Mathf.Clamp(emitter.pan += Mathf.Lerp(0.033f, 0.066f, UnityEngine.Random.value) * (UnityEngine.Random.value < 0.5f ? -1f : 1f), -1f, 1f);
                    emitter.alive = true;

                    if (emitter.room != null && emitter.room != game.cameras[0].room)
                    {
                        emitter.RemoveFromRoom();
                        game.cameras[0].room.AddObject(emitter);
                    }

                    if (spikeCounter > 0) spikeCounter--;
                    if (SpikesLeft == 0) reverse = true;
                    if (spikeCounter == 0 && SpikesLeft > 0)
                    {
                        SpawnSpike();
                    }

                    if (SpikesLeft == 0 && rumble == 0)
                    {
                        ResetEvent();
                    }
                }
            }

            public void ResetEvent()
            {
                EventActive = false;
                LocalSpikeEventDifficulty = 0f;
                maxSpikes = 0;
                SpikesLeft = 10 + (int)(20 * GlobalSpikeEventDifficulty);
                SpikeEventCountdown = UnityEngine.Random.Range(1000, 3500 - (int)(GlobalSpikeEventDifficulty * 1000));
                Plugin.logger.LogMessage(GlobalSpikeEventDifficulty + " " + SpikesLeft);
                reverse = false;
                emitter.alive = false; 
                emitter.Destroy();
                emitter = null;
            }

            // This is such a mess oh my gog
            public void SpawnSpike()
            {
                if (game.Players[0] != null && game.Players[0].realizedCreature != null && !game.Players[0].realizedCreature.inShortcut && !game.Players[0].realizedCreature.room.abstractRoom.shelter) 
                {
                    Player player = game.Players[0].realizedCreature as Player;
                    List<Vector2> tiles = new List<Vector2>();
                    float diff = Mathf.Max(GlobalSpikeEventDifficulty, LocalSpikeEventDifficulty);
                    for (int i = 0; i < 4; i++) // 4 possible random tile checks per spike
                    {
                        Vector2? v = SharedPhysics.ExactTerrainRayTracePos(player.room, player.mainBodyChunk.pos,
                            player.mainBodyChunk.pos + Custom.RNV() * Mathf.Lerp(100, 400, diff)); // The harder the event, the longer they can get
                        if (v.HasValue && !Custom.DistLess(v.Value, player.mainBodyChunk.pos, 40f))
                        {
                            Vector2 terrainDir = Vector2.zero;
                            for (int t = 0; t < Custom.fourDirections.Length; t++)
                            {
                                if (player.room.GetTile(player.room.GetTilePosition(v.Value + Custom.fourDirections[t].ToVector2() * 30f)).Solid)
                                {
                                    terrainDir = Custom.fourDirections[t].ToVector2(); //I dont know if this works
                                    //Plugin.logger.LogMessage("works"); //Oh okay
                                    break;
                                }
                            }
                            tiles.Add(v.Value + terrainDir * 10f);
                        }
                    }
                     
                    if (tiles.Count > 0)
                    {
                        // Actually spawn the spike
                        int spikess = UnityEngine.Random.value < Mathf.Pow(diff, 3f + (diff * 3.33f)) ? 2 : 1;
                        for (int s = 0; s < spikess; s++)
                        {
                            if (tiles.Count == 0) break;
                            Vector2 remove = tiles[UnityEngine.Random.Range(0, tiles.Count)];
                            player.room.AddObject(new GroundSpike(remove, firstSpike ? 100 : UnityEngine.Random.Range((int)Mathf.Lerp(60f, 40f, diff), 76), player));
                            tiles.Remove(remove);
                        }
                        firstSpike = false;
                        SpikesLeft--;
                        spikeCounter = (int)Mathf.Lerp(100f, 30f, diff) + UnityEngine.Random.Range(5, 31);

                        LocalSpikeEventDifficulty += UnityEngine.Random.Range(0.3f, 0.7f) / Mathf.Max(1f, maxSpikes);
                    }
                }
            }
        }

        public class GroundSpike : CosmeticSprite
        {
            public Vector2 root;
            public Vector2 dir;
            public Vector2 goal;
            public Vector2 targetBehaviorFac;
            public float dist;
            public float progress;
            public float thickness;
            public float progIncrease;
            public float lifeTime;
            public float warningFade;
            public float warningPause;
            public int rumble;
            public int maxRumble;
            public int testRumble;
            public int cosmeticCounter;
            public bool reverse;
            public Player target;
            public Color color;
            public BodyChunk piercedChunk;
            public Vector2 piercedPos;
            public bool ignorePlayer;
            public AbstractCreature killTagHolder;

            public GroundSpike(Vector2 position, int rumble, Player player) : this(position, rumble, player.mainBodyChunk.pos)
            {
                target = player;
            }

            public GroundSpike(Vector2 position, int rumble, Vector2 goalPos)
            {
                pos = position;
                lastPos = pos;
                root = pos;
                this.rumble = rumble;
                maxRumble = rumble;
                progress = 0f;
                goal = goalPos;
                dir = Custom.DirVec(root, goal);
                dist = Vector2.Distance(pos, goal);
                thickness = Mathf.Lerp(2.8f, 3.33f, UnityEngine.Random.value);
                cosmeticCounter = 1;
                lifeTime = 1f;
                targetBehaviorFac = Vector2.zero;
                progIncrease = 0.05f;
                color = new Color(0.41f, 0f, 0f);
                testRumble = (int)(maxRumble / (2f * thickness / 3f));
            }

            public override void Update(bool eu)
            {
                base.Update(eu);

                if (rumble > 0)
                {
                    if (cosmeticCounter > 0)
                    {
                        cosmeticCounter--;
                        if (cosmeticCounter == 0)
                        {
                            if (UnityEngine.Random.value < 0.3f) room.AddObject(new CosmeticRubble(root + dir * 10f, dir * 8f + Custom.RNV() * 8f, 0.4f + 0.6f * UnityEngine.Random.value, 7.5f + UnityEngine.Random.value * 10f));
                            else room.AddObject(new Spark(root, dir * 6f + Custom.RNV() * 5f, Color.Lerp(new Color(0.01f, 0.01f, 0.01f), color, UnityEngine.Random.value), null, 13, 20));

                            cosmeticCounter = 4;
                        }
                    }

                    if (warningPause > 0)
                    {
                        warningPause--;
                        return;
                    }

                    rumble--;

                    // Player tracking
                    if (rumble > testRumble)
                    {
                        if (target != null && !target.inShortcut)
                        {
                            goal = target.mainBodyChunk.pos;
                            targetBehaviorFac.x += target.mainBodyChunk.vel.x;
                            dir = Custom.DirVec(pos, goal + targetBehaviorFac);
                            dist = Vector2.Distance(pos, goal + targetBehaviorFac);
                        }

                        if (rumble == testRumble + 1) // The last frame of this happening
                        {
                            progIncrease = 15f / dist; // So the speed of it coming out is always the same essentially

                            // Go a bit ahead of your goal, the shorter the spike, the more it go (for coolness)
                            goal += targetBehaviorFac + dir * (Mathf.Lerp(0f, 40f, Mathf.Max(GlobalSpikeEventDifficulty, LocalSpikeEventDifficulty)) + Custom.LerpMap(Vector2.Distance(pos, goal), 40f, 300f, 50f, 10f)) * (0.75f + UnityEngine.Random.value * 0.75f);
                        }
                    }
                    else
                    {
                        // Warning
                        float rumbleFac = Custom.LerpMap(rumble, testRumble, 0f, 0f, 1f);
                        if (rumble == (int)(testRumble / 2))
                        {
                            room.PlaySound(SoundID.King_Vulture_Tusk_Aim_Beep, root, 1.3f, 1f);
                        }
                        warningFade = Mathf.Sin(rumbleFac * Mathf.PI);
                        if (rumble == 1) warningPause = Mathf.Pow(maxRumble, 0.7f);
                    }

                    if (rumble == 0)
                    {
                        // Extend the spike
                        if (!ignorePlayer) room.ScreenMovement(pos, Vector2.zero, 0.0045f * Mathf.Min(dist, 200f));
                        room.PlaySound(SoundID.Slugcat_Throw_Spear, pos, 1f, 0.75f);
                        room.PlaySound(Plugin.SpikeSound, pos);

                        for (int r = 0; r < UnityEngine.Random.Range(2, 6); r++)
                        {
                            room.AddObject(new CosmeticRubble(root + dir * 10f, dir * 5f + Custom.RNV() * (5f + 8f * UnityEngine.Random.value), 0.4f + 0.6f * UnityEngine.Random.value, 7.5f + UnityEngine.Random.value * 10f));
                        }

                        if (target != null && target.inShortcut) SpikesLeft++; //So you cant cheese it
                    }
                    progress = Custom.LerpMap(rumble, maxRumble, 0, 0, 0.1f);
                    if (goal != null) pos = Vector2.Lerp(root, goal, Custom.LerpQuadEaseOut(0f, 1f, progress));
                    return;
                }

                if (goal != null) pos = Vector2.Lerp(root, goal, Custom.LerpQuadEaseOut(0f, 1f, progress));
                progress = Mathf.Min(1f, progress + (0.033f + progIncrease) * (reverse ? -1 : 1));
                lifeTime = Mathf.Max(0, lifeTime - 0.0075f * UnityEngine.Random.value);

                if (!reverse && piercedChunk == null && lastPos != pos)
                {
                    SharedPhysics.CollisionResult result = SharedPhysics.TraceProjectileAgainstBodyChunks(null, room, lastPos, ref pos, killTagHolder != null ? 7f : 3f, 1, null, false);

                    if (result.hitSomething)
                    {
                        if (result.obj != null && result.chunk != null && result.obj is Creature c && !(ignorePlayer && c is Player))
                        {
                            piercedChunk = result.chunk;
                            piercedPos = result.collisionPoint;
                            if (killTagHolder != null) c.SetKillTag(killTagHolder);
                            c.Violence(null, dir, result.chunk, null, Creature.DamageType.Stab, 1f, 40);
                            room.PlaySound(SoundID.Spear_Stick_In_Creature, piercedPos, 1f, 0.8f + 0.5f * UnityEngine.Random.value);

                            if (room.BeingViewed)
                            {
                                for (int i = 0; i < 5; i++)
                                {
                                    room.AddObject(new WaterDrip(piercedPos, Custom.RNV() * 5f + dir * 4f, false));
                                }
                            }
                        }
                    }
                }

                if (piercedChunk != null && piercedChunk.owner.room == room && !(piercedChunk.owner is Creature cr && cr.enteringShortCut != null))
                {
                    piercedChunk.MoveFromOutsideMyUpdate(eu, piercedPos);
                    piercedChunk.vel *= 0f;
                }

                if (lifeTime == 0f)
                {
                    reverse = true;
                    if (piercedPos != Vector2.zero)
                    {
                        piercedChunk = null;
                        room.PlaySound(SoundID.Spear_Dislodged_From_Creature, piercedPos, 1f, 0.8f + 0.5f * UnityEngine.Random.value);
                        for (int i = 0; i < 3; i++)
                        {
                            room.AddObject(new WaterDrip(piercedPos, Custom.RNV() * 5f - dir * 4f, false));
                        }
                        piercedPos = Vector2.zero;
                    }
                    if (progress <= 0f) Destroy();
                }
            }

            public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.sprites = new FSprite[3];
                TriangleMesh.Triangle[] spike = new TriangleMesh.Triangle[] {new (0, 1, 2) };

                sLeaser.sprites[0] = new TriangleMesh("Futile_White", spike, true);
                sLeaser.sprites[1] = new FSprite("Circle20")
                {
                    scale = 1 / 20f * thickness * 2f
                };
                sLeaser.sprites[2] = new FSprite("pixel")
                {
                    anchorX = 0.5f,
                    anchorY = 0f,
                    shader = rCam.game.rainWorld.Shaders["Hologram"]
                };

                base.InitiateSprites(sLeaser, rCam);
                AddToContainer(sLeaser, rCam, null);
            }

            public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

                Vector2 positio = Vector2.Lerp(lastPos, pos, timeStacker);
                Vector2 perp = Custom.PerpendicularVector(dir);
                float phrogFac = Custom.LerpQuadEaseOut(0f, thickness, progress); //frog.

                TriangleMesh spikeMesh = sLeaser.sprites[0] as TriangleMesh;
                spikeMesh.MoveVertice(0, root + perp * phrogFac - camPos);
                spikeMesh.MoveVertice(1, positio - camPos);
                spikeMesh.MoveVertice(2, root - perp * phrogFac - camPos);

                sLeaser.sprites[1].SetPosition(root - camPos);

                sLeaser.sprites[2].alpha = warningFade;
                sLeaser.sprites[2].scaleY = Vector2.Distance(pos, goal);
                sLeaser.sprites[2].SetPosition(root - camPos);
                sLeaser.sprites[2].rotation = Custom.AimFromOneVectorToAnother(root, goal);
            }

            public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
            {
                TriangleMesh spikeMesh = sLeaser.sprites[0] as TriangleMesh;
                spikeMesh.verticeColors[0] = palette.blackColor;
                spikeMesh.verticeColors[1] = color;
                spikeMesh.verticeColors[2] = palette.blackColor;
                sLeaser.sprites[1].color = palette.blackColor;
                sLeaser.sprites[2].color = Color.red;
            }

            public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
            {
                if (newContatiner == null)
                {
                    newContatiner = rCam.ReturnFContainer("Midground");
                }
                newContatiner.AddChild(sLeaser.sprites[1]);
                newContatiner.AddChild(sLeaser.sprites[0]);
                newContatiner.AddChild(sLeaser.sprites[2]);
            }
        }

        public class CosmeticRubble : CosmeticSprite
        {
            public int spriteType;
            public float scale;
            public float rot;
            public float lastRot;
            public float rotSpeed;
            public bool right;
            public SharedPhysics.TerrainCollisionData scratchTerrainCollisionData = new SharedPhysics.TerrainCollisionData();

            public CosmeticRubble(Vector2 position, Vector2 velocity, float scale, float rotSpeed = 10f)
            {
                pos = position;
                lastPos = pos;
                vel = velocity;
                spriteType = UnityEngine.Random.Range(1, 15);
                this.scale = scale;
                right = UnityEngine.Random.value < 0.5f;
                this.rotSpeed = rotSpeed;
            }

            public override void Update(bool eu)
            {
                base.Update(eu);
                lastRot = rot;
                rot += rotSpeed * (right ? 1 : -1);
                vel.y -= room.gravity;
                scale = Mathf.Max(0f, scale - 0.0033f);
                if (scale == 0f) Destroy();

                SharedPhysics.TerrainCollisionData terrainCollisionData = scratchTerrainCollisionData.Set(this.pos, this.lastPos, this.vel, scale, new IntVector2(0, 0), false);
                terrainCollisionData = SharedPhysics.VerticalCollision(room, terrainCollisionData);
                terrainCollisionData = SharedPhysics.HorizontalCollision(room, terrainCollisionData);
                pos = terrainCollisionData.pos;
                vel = terrainCollisionData.vel;
                if (terrainCollisionData.contactPoint.y != 0) 
                { 
                    vel *= 0.33f; 
                    rotSpeed *= 0.9f;
                }
            }

            public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.sprites = new FSprite[1];
                sLeaser.sprites[0] = new FSprite("Pebble" + spriteType)
                {
                    scale = scale
                };

                AddToContainer(sLeaser, rCam, null);
                base.InitiateSprites(sLeaser, rCam);
            }

            public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

                Vector2 position = Vector2.Lerp(lastPos, pos, timeStacker);

                sLeaser.sprites[0].SetPosition(position - camPos);
                sLeaser.sprites[0].scale = scale;
                sLeaser.sprites[0].rotation = Mathf.Lerp(lastRot, rot, timeStacker);
            }

            public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
            {
                sLeaser.sprites[0].color = palette.blackColor;
            }
        }
    }
}
