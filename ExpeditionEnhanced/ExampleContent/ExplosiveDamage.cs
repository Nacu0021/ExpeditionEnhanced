using UnityEngine;
using MoreSlugcats;

namespace ExpeditionEnhanced.ExampleContent
{
    public class ExplosiveDamage : EECustomPerk
    {
        public override string ID => "unl-explosivedamage";
        public override string DisplayName => "Explosive Damage";
        public override string Description => "Your attacks explode!";
        public override string ManualDescription => "All instances of damage dealt by the player cause a small explosion. It's strength depends on the weapon.";
        public override string SpriteName => "Symbol_Volatile";//"OutlawA";
        public override Color Color => Volatile.VOLATILE_PINK; //new Color(1f, 0.5f, 0.5f);
        public override CustomPerkType PerkType => CustomPerkType.OnAttack;

        //I dont know if this is fun and/or balanced (i still dont)
        public override void OnAttack(SocialEventRecognizer socialEventRecognizer, PhysicalObject weapon, Player thrower, Creature victim, bool actuallyHit)
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
                socialEventRecognizer.room.AddObject(new Explosion(socialEventRecognizer.room, weapon, pos, 7, 30f * factor, factor * 2.5f, 0.3f * factor, 20f * factor, 0f, thrower, 0.3f * factor, 10f * factor, 0.1f * factor));
                //socialEventRecognizer.room.AddObject(new Explosion.ExplosionLight(pos, 30f * factor, 1f, 3, Color.white));
                //socialEventRecognizer.room.AddObject(new ExplosionSpikes(socialEventRecognizer.room, pos, (int)(3 * factor), 30f * factor, 9f, 7f, 10f * factor, new Color(0.01f, 0.01f, 0.01f)));
                //socialEventRecognizer.room.AddObject(new ShockWave(pos, 30f * factor, 0.04f * factor, 5, false));
                //for (int j = 0; j < 5; j++)
                //{
                //    socialEventRecognizer.room.AddObject(new Spark(pos, Custom.RNV() * Mathf.Lerp(5f, 10f, UnityEngine.Random.value) * factor + Custom.RNV() * 10f * UnityEngine.Random.value, Color.Lerp(Color.gray, Color.white, UnityEngine.Random.value), null, 10 * (int)factor, 15 * (int)factor));
                //}

                // now does the volatile bomb effect :))
                VolatileBomb.VolatileEffect(socialEventRecognizer.room, pos, 2f + factor / 2f);
                socialEventRecognizer.room.PlaySound(SoundID.Fire_Spear_Explode, pos, 0.4f, 1.1f - (0.15f * factor));
            }
        }
    }
}
