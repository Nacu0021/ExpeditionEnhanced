using UnityEngine;
using RWCustom;

namespace ExpeditionEnhanced.ExampleContent
{
    public class Leeching : EECustomPerk
    {
        public override string ID => "unl-leeching";
        public override string DisplayName => "Leeching";
        public override string Description => "Gain food upon killing creatures";
        public override string ManualDescription => "When killing a creature, gain a quarter pip of food. Bigger creatures killed yield more food!";
        public override string SpriteName => "Kill_Leech";
        public override Color Color => new Color(0.68235296f, 0.15686275f, 0.11764706f);
        public override bool UnlockedByDefault => true;
        public override CustomPerkType PerkType => CustomPerkType.OnKill; //This means the OnKill function triggers whenever a player kills something

        //You need to override this if you want the OnKill perk type to do anything.
        public override void OnKill(SocialEventRecognizer socialEventRecognizer, Player player, Creature victim)
        {
            if (player == null || victim == null || player.room == null || victim.room == null) return;

            //Logic for giving the player quarter food pips based on the creature's weight.
            if (!victim.Template.smallCreature) 
            {
                int num = Mathf.Clamp(Mathf.CeilToInt(victim.TotalMass * 2f), 1, 16);
                for (int i = 0; i < num; i++)
                {
                    //player.AddQuarterFood(); //If you didnt want any of the cosmetic stuff this is essentially the same

                    if (player.room == victim.room) player.room.AddObject(new LeechParticle(player, victim));
                    else player.AddQuarterFood();
                }
            }
        }

        //Cosmetic class, not really necessary but i want to be slightly fancy
        public class LeechParticle : CosmeticSprite
        {
            public Player leech;
            public Creature victim;
            public float time;
            public float offset;
            public float speed;
            public int startTime;
            public Room origRoom;
            public Vector2 onBezierPos;
            public Vector2 lastLastPos;
            public Vector2 lastLastLastPos; //YES this is necessary (its not)

            public LeechParticle(Player leech, Creature victim)
            {
                this.leech = leech;
                this.victim = victim;
                time = 1f;
                startTime = (int)Mathf.Lerp(0, 20, Random.value);
                offset = Mathf.Lerp(-50f, 50f, Random.value);
                speed = Mathf.Lerp(0.6f, 1.7f, Random.value);
                origRoom = leech.room;
                pos = victim.mainBodyChunk.pos;
                lastPos = victim.mainBodyChunk.pos;
                lastLastPos = victim.mainBodyChunk.pos;
                lastLastLastPos = victim.mainBodyChunk.pos;
            }

            public override void Update(bool eu)
            {
                if (leech == null || victim == null)
                {
                    Destroy();
                    return;
                }
                room = leech.room;
                if (room != origRoom)
                {
                    Destroy();
                    return;
                }
                if (lastLastLastPos != lastLastPos) lastLastLastPos = lastLastPos;
                if (lastPos != lastLastPos) lastLastPos = lastPos;
                base.Update(eu);
                //if (startTime > 0)
                //{
                //    startTime--;
                //    if (startTime == 0) room.PlaySound(SoundID.Leech_Detatch_Player, victim.mainBodyChunk, false, 1.5f, 1.5f);
                //    return;
                //}
                if (time > 0f) time = Mathf.Max(time - 0.05f * speed, 0f);
                else
                {
                    Destroy();
                }

                Vector2 halfPoint = (leech.mainBodyChunk.pos + victim.mainBodyChunk.pos) / 2f + Custom.PerpendicularVector(victim.mainBodyChunk.pos, leech.mainBodyChunk.pos) * offset;
                onBezierPos = Custom.Bezier(leech.mainBodyChunk.pos, halfPoint, victim.mainBodyChunk.pos, halfPoint, time);
                pos = onBezierPos;
            }

            public override void Destroy()
            {
                if (leech != null)
                {
                    room?.PlaySound(SoundID.Leech_Attatch_Player, leech.mainBodyChunk, false, 1f + Random.value / 3f, 1f + Random.value / 3f);
                    leech.AddQuarterFood();
                }
                base.Destroy();
            }

            public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.sprites = new FSprite[1];
                TriangleMesh.Triangle[] trianglage = 
                { 
                    new TriangleMesh.Triangle(0, 1, 2), 
                    new TriangleMesh.Triangle(1, 2, 3), 
                    new TriangleMesh.Triangle(2, 3, 4), 
                };
                sLeaser.sprites[0] = new TriangleMesh("Futile_White", trianglage, false);
                sLeaser.sprites[0].alpha = 0.65f;

                AddToContainer(sLeaser, rCam, null);
            }

            public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
                Vector2 vector2 = Vector2.Lerp(lastLastPos, lastPos, timeStacker);
                Vector2 vector3 = Vector2.Lerp(lastLastLastPos, lastLastPos, timeStacker);
                Vector2 perp = Custom.PerpendicularVector(lastPos, pos);
                Vector2 perp2 = Custom.PerpendicularVector(lastLastPos, lastPos);

                TriangleMesh mesh = sLeaser.sprites[0] as TriangleMesh;

                mesh.MoveVertice(0, vector - perp * 0.5f - camPos);
                mesh.MoveVertice(1, vector + perp * 0.5f - camPos);
                mesh.MoveVertice(2, vector2 - perp2 * 1.25f - camPos);
                mesh.MoveVertice(3, vector2 + perp2 * 1.25f - camPos);
                mesh.MoveVertice(4, vector3 - camPos);

                base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            }

            public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
            {
                sLeaser.sprites[0].color = Color.red;
            }
        }
    }
}
