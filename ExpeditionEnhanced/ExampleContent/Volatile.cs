using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace ExpeditionEnhanced.ExampleContent
{
    using static ExampleBurdenHooks;
    public class Volatile : EECustomBurden
    {
        public override float ScoreMultiplier => 70f;
        public override string ID => "bur-volatile";
        public override string DisplayName => "VOLATILE";
        public override string ManualDescription => "Causes all creatures to drop deadly explosives upon death.";
        public override Color Color => VOLATILE_PINK;
        public override bool UnlockedByDefault => true;
        public override string Description => ManualDescription;

        public static Color VOLATILE_PINK = new(0.929f, 0.176f, 0.466f);
        public static Color VOLATILE_PINK2 = new(0.941f, 0.705f, 0.796f);

        public override void ApplyHooks()
        {
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
    }

    public class VolatileBomb : CosmeticSprite
    {
        public float rad;
        public float rot;
        public float yRot;
        public float lastRot;
        public float lastYRot;
        public float rotSpeed;
        public float yRotSpeed;
        public bool yRotDir;
        public float g;
        //public float lastBlink;
        //public float blink;
        //public float blinkRate;
        //public int blinks;
        public SharedPhysics.TerrainCollisionData scratchTerrainCollisionData = new SharedPhysics.TerrainCollisionData();
        public IntVector2 lastContactPoint;
        public int spikes;
        //public bool blinked;
        public float finalBeep;
        public float life;
        public PositionedSoundEmitter longBeep;
        public float airFriction;
        public float waterFriction;

        public float submersion
        {
            get
            {
                if (room == null)
                {
                    return 0f;
                }
                if (room.waterInverted)
                {
                    return 1f - Mathf.InverseLerp(pos.y - rad, pos.y + rad, room.FloatWaterLevel(pos.x));
                }
                float num = room.FloatWaterLevel(pos.x);
                if (ModManager.MMF && !MMF.cfgVanillaExploits.Value && num > (room.abstractRoom.size.y + 20) * 20f)
                {
                    return 1f;
                }
                return Mathf.InverseLerp(pos.y - rad, pos.y + rad, num);
            }
        }

        public VolatileBomb(Vector2 pos, Vector2 vel, float scaleFac)
        {
            this.pos = pos;
            lastPos = pos;
            this.vel = vel;
            rad = 3f + scaleFac;
            g = 0.7f;
            lastContactPoint = new IntVector2(0, 0);
            spikes = 4 + 2;
            yRotSpeed = Mathf.Lerp(4f, 9f, Random.value);
            rotSpeed = Mathf.Lerp(4f, 9f, Random.value);
            yRotDir = Random.value < 0.5f;
            life = Mathf.Lerp(0.85f, 1.25f, Random.value) * (scaleFac/3f);
            airFriction = 0.99f;
            waterFriction = 0.95f;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            vel *= Mathf.Lerp(airFriction, waterFriction, submersion);
            vel -= new Vector2(0f, g);

            // Rotations
            lastRot = rot;
            lastYRot = yRot;

            yRot += yRotSpeed;
            if (yRot > 180)
            {
                yRot = 0;
                lastYRot = 0;
            }
            rot += rotSpeed;
            if (rot > 360)
            {
                rot = 0;
                lastRot = 0;
            }
            yRotSpeed = Mathf.Max(0f, yRotSpeed - 0.001f);
            rotSpeed = Mathf.Max(0f, rotSpeed - 0.001f);

            // Collisions
            SharedPhysics.TerrainCollisionData collision = scratchTerrainCollisionData.Set(pos, lastPos, vel, rad, new IntVector2(0, 0), false);
            collision = SharedPhysics.VerticalCollision(room, collision);
            collision = SharedPhysics.HorizontalCollision(room, collision);

            // bounce
            if (lastContactPoint != collision.contactPoint)
            {
                if (collision.contactPoint.x != 0) vel.x = -vel.x;
                if (collision.contactPoint.y != 0) vel.y = -vel.y;
                rotSpeed *= 0.9f;
                yRotSpeed *= 0.9f;
                room.PlaySound(SoundID.Slugcat_Floor_Impact_Stealthy, pos, 2f, 0.8f + 0.2f * Random.value);
            }

            //if (collision.contactPoint.y != 0)
            //{
            //    vel.y *= 0.96f;
            //    vel.x *= 0.98f;
            //}

            lastContactPoint = collision.contactPoint;

            // Beeping flashing
            // Going from 0 to 360 again to make work with Mathf.Sin easier
            //lastBlink = blink;
            //blink += blinkRate;
            //
            //if (!blinked && blink >= 90)
            //{
            //    if (blinks == 0)
            //    {
            //        if (longBeep == null)
            //        {
            //            blinkRate = 0f;
            //            blinks = -1;
            //        }
            //    }
            //    else
            //    {
            //        room.PlaySound(Plugin.Beep, pos, 0.3f, 0.8f + 0.1f * Random.value);
            //        blinked = true;
            //        blinks--;
            //    }
            //}
            //if (blink >= 360 && longBeep == null)
            //{
            //    blink = 0;
            //    lastBlink = 0;
            //    blinkRate += 20 / (rad/3f);
            //    blinked = false;
            //}

            if (life > 0f)
            {
                life -= 0.022f;
            }
            else if (finalBeep == 0f)
            {
                longBeep = new PositionedSoundEmitter(pos, 0.3f, 0.8f);
                room.PlaySound(Plugin.LongBeep, longBeep, true, 0.3f, 0.8f + 0.1f * Random.value, false);
                longBeep.requireActiveUpkeep = true;
                finalBeep = 1f;
            }

            if (finalBeep > 0f && longBeep != null)
            {
                longBeep.pos = pos;
                //longBeep.volume = Mathf.InverseLerp(1f, 0f, finalBeep);
                longBeep.alive = true;
                longBeep.pitch = Custom.LerpMap(finalBeep, 0.5f, 0f, 0.8f, 1.3f);
                finalBeep = Mathf.Max(0f, finalBeep - 0.025f);
                if (finalBeep == 0f)
                {
                    longBeep.volume = 0f;
                    longBeep.alive = false;
                    Boom();
                }
            }
        }

        public void Boom()
        {
            room.AddObject(new Explosion(room, null, pos, 12, 30f + rad * 12.5f, 2.33f + rad * 0.9f, rad * 0.6f, rad * 3f, 0f, null, 1f, 40, 0f));
            VolatileEffect(room, pos, rad);
            room.ScreenMovement(new Vector2?(pos), default, 0.15f * rad);
            room.PlaySound(SoundID.Fire_Spear_Explode, pos, 0.7f, (0.6f + 0.2f * Random.value) * Custom.LerpMap(rad, 3f, 10f, 2.5f, 0.3f));
            room.PlaySound(SoundID.Bomb_Explode, pos, 0.5f, 0.5f);
            Destroy();
        }

        public static void VolatileEffect(Room room, Vector2 pos, float rad)
        {
            room.AddObject(new ImpactEffect(pos, rad * 1.5f, 12));
            room.AddObject(new ExplosionSpikes(room, pos, Random.Range(5, 10), rad * 7.5f, 12, rad * 3f, rad * 10f, Volatile.VOLATILE_PINK));
            for (int i = 0; i < (int)(Random.Range(5, 16) * (rad/2f)); i++)
            {
                room.AddObject(new CollectToken.TokenSpark(pos, Custom.RNV() * (15f + rad) * Random.value * (rad / 2f), ImpactEffect.FlamboColor(Random.value), room.PointSubmerged(pos)));
            }
            for (int i = 0; i < (int)(Random.Range(2, 6) * (rad / 2f)); i++)
            {
                room.AddObject(new Spark(pos, Custom.RNV() * (10f + rad) * Random.value * (rad / 2f), ImpactEffect.FlamboColor(Random.value), null, 20, 50));
            }
            room.AddObject(new ShockWave(pos, rad * 20f, 0.05f, 8));
            room.AddObject(new SootMark(room, pos, rad * 10f, false));
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1 + spikes];
            sLeaser.sprites[0] = new FSprite("Futile_White")
            {
                scale = 1f / 4f * rad,
                shader = rCam.game.rainWorld.Shaders["JaggedCircle"],
                alpha = Mathf.Lerp(0.05f, 0.1f, Random.value)
            };
            // Spikes
            for (int i = 0; i < spikes; i++)
            {
                sLeaser.sprites[i + 1] = new TriangleMesh("Futile_White", [new(0, 3, 4), new(0, 1, 3), new(1, 2, 3)], true);
            }

            AddToContainer(sLeaser, rCam, null);
            base.InitiateSprites(sLeaser, rCam);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            Vector2 position = Vector2.Lerp(lastPos, pos, timeStacker);
            float routatioune = Mathf.Lerp(lastRot, rot, timeStacker);
            Vector2 rotototo = Custom.DegToVec(routatioune);

            sLeaser.sprites[0].SetPosition(position - camPos);
            sLeaser.sprites[0].rotation = routatioune;

            for (int i = 0; i < spikes; i++)
            {
                if (i < spikes - 2)
                {
                    float deg = Mathf.Lerp(lastYRot, yRot, timeStacker) + 90 * i;
                    float sinFac = Mathf.Sin(deg * (yRotDir ? 1f : -1f) * Mathf.Deg2Rad);
                    Vector2 possum = Custom.DegToVec(routatioune + 90) * sinFac * rad * 1.2f;
                    MoveTriangle(sLeaser.sprites[i + 1] as TriangleMesh, position + possum - camPos, rad * 0.8f, rad * 1.8f, Mathf.Abs(sinFac), Custom.DegToVec((rot + (sinFac < 0f ? 270f : 90f))), false);

                    // Making sure the spikes are correctly behind the ball
                    if (deg > 90 && deg < 270)
                    {
                        if (rCam.ReturnFContainer("Midground").GetChildIndex(sLeaser.sprites[i + 1]) > rCam.ReturnFContainer("Midground").GetChildIndex(sLeaser.sprites[0]))
                        {
                            sLeaser.sprites[i + 1].MoveBehindOtherNode(sLeaser.sprites[0]);
                        }
                    }
                    else if (rCam.ReturnFContainer("Midground").GetChildIndex(sLeaser.sprites[i + 1]) < rCam.ReturnFContainer("Midground").GetChildIndex(sLeaser.sprites[0]))
                    {
                        sLeaser.sprites[i + 1].MoveInFrontOfOtherNode(sLeaser.sprites[0]);
                    }
                }
                else
                {
                    int g = i == 5 ? 1 : -1;
                    MoveTriangle(sLeaser.sprites[i + 1] as TriangleMesh, position + rotototo * g * rad * 1f - camPos, rad * 0.8f, rad * 1.8f, 1f, rotototo * g, false);
                }

                if (finalBeep > 0f)
                {
                    float bling = Mathf.InverseLerp(1f, 0f, finalBeep);//;finalBeep > 0f ? Mathf.InverseLerp(1f, 0f, finalBeep) : Mathf.Sin(blink * Mathf.Deg2Rad);
                    Color c = Color.Lerp(rCam.currentPalette.blackColor, Volatile.VOLATILE_PINK, bling);
                    (sLeaser.sprites[i + 1] as TriangleMesh).verticeColors[1] = c;
                    (sLeaser.sprites[i + 1] as TriangleMesh).verticeColors[3] = c;
                }
            }
        }

        public static void MoveTriangle(TriangleMesh triangle, Vector2 pos, float width, float height, float f, Vector2 rotation, bool three)
        {
            Vector2 perp = Custom.PerpendicularVector(rotation);
            Vector2 rot = (rotation * height * f);
            // Tip
            if (three)
            {
                triangle.MoveVertice(0, pos - perp * width);
                triangle.MoveVertice(1, pos + rot);
                triangle.MoveVertice(2, pos + perp * width);
                return;
            }
            triangle.MoveVertice(0, pos - perp * width);
            triangle.MoveVertice(2, pos + rot);
            triangle.MoveVertice(4, pos + perp * width);
            triangle.MoveVertice(3, Vector2.Lerp(pos + perp * width, pos + rot, 0.33f));
            triangle.MoveVertice(1, Vector2.Lerp(pos - perp * width, pos + rot, 0.33f));
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            sLeaser.sprites[0].color = palette.blackColor;
            for (int i = 0; i < spikes; i++)
            {
                (sLeaser.sprites[i + 1] as TriangleMesh).verticeColors[0] = palette.blackColor;
                (sLeaser.sprites[i + 1] as TriangleMesh).verticeColors[1] = palette.blackColor;
                (sLeaser.sprites[i + 1] as TriangleMesh).verticeColors[2] = Volatile.VOLATILE_PINK;
                (sLeaser.sprites[i + 1] as TriangleMesh).verticeColors[3] = palette.blackColor;
                (sLeaser.sprites[i + 1] as TriangleMesh).verticeColors[4] = palette.blackColor;
            }
        }
    }

    public class ImpactEffect : CosmeticSprite
    {
        public float rad;
        public int lifeTime;
        public int maxLifeTime;
        public float scaleFac;
        public float grug;

        public ImpactEffect(Vector2 pos, float rad, int lifeTime)
        {
            this.pos = pos;
            this.rad = rad;
            this.lifeTime = lifeTime;
            maxLifeTime = lifeTime;
            grug = 0.1f * rad;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            scaleFac = Custom.LerpMap(lifeTime, maxLifeTime, maxLifeTime / 10f, 0f, rad, 0.2f);

            if (lifeTime <= 0)
            {
                Destroy();
            }
            lifeTime--;
        }

        public static Color FlamboColor(float t)
        {
            return Color.Lerp(Volatile.VOLATILE_PINK, Volatile.VOLATILE_PINK2, t);
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[2];
            sLeaser.sprites[0] = new FSprite("Futile_White", true);
            sLeaser.sprites[0].shader = room.game.rainWorld.Shaders["VectorCircle"];
            sLeaser.sprites[0].alpha = 0f;
            sLeaser.sprites[0].scale = 0f;
            sLeaser.sprites[1] = new FSprite("Futile_White", true);
            sLeaser.sprites[1].shader = room.game.rainWorld.Shaders["FlatLightBehindTerrain"];
            sLeaser.sprites[1].alpha = 0.133f;
            sLeaser.sprites[1].scale = 0f;

            AddToContainer(sLeaser, rCam, null);
            base.InitiateSprites(sLeaser, rCam);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            Vector2 position = Vector2.Lerp(lastPos, pos, timeStacker);

            float t = Custom.LerpMap(scaleFac, rad, 0f, 0f, 0.33f);
            Color c = FlamboColor(Custom.LerpMap(lifeTime, maxLifeTime, maxLifeTime / 10f, 1f, 0f));

            sLeaser.sprites[0].scale = scaleFac;
            sLeaser.sprites[0].alpha = t;
            sLeaser.sprites[0].color = c;
            sLeaser.sprites[0].SetPosition(position - camPos);
            sLeaser.sprites[1].scale = scaleFac;
            sLeaser.sprites[1].color = c;
            sLeaser.sprites[1].alpha = t;
            sLeaser.sprites[1].SetPosition(position - camPos);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (newContatiner == null)
            {
                newContatiner = rCam.ReturnFContainer("Midground");
            }
            newContatiner.AddChild(sLeaser.sprites[0]);
            rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[1]);
        }
    }
}
