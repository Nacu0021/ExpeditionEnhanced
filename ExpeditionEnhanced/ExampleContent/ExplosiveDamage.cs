using UnityEngine;

namespace ExpeditionEnhanced.ExampleContent
{
    public class ExplosiveDamage : CustomPerk
    {
        public override string ID => "unl-boomdamage";
        public override string Name => "Explosive damage";
        public override string Description => "Your attacks explode!";
        public override string ManualDescription => "All instances of damage dealt by the player cause a small explosion. It's strength depends on the weapon.";
        public override string SpriteName => "OutlawA";
        public override Color Color => new Color(1f, 0.5f, 0.5f);
        public override bool AlwaysUnlocked => true;
        public override CustomPerkType PerkType => CustomPerkType.Custom;
    }
}
