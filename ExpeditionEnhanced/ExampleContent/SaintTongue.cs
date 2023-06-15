using UnityEngine;

namespace ExpeditionEnhanced.ExampleContent
{
    internal class SaintTongue : CustomPerk
    {
        public override string ID => "unl-stongue";
        public override string Name => "Saint's Tongue";
        public override string Description => "Allows the player to use Saint's tongue ability";
        public override string ManualDescription => "Allows any slugcat to use Saint's tongue, which can get you out of sticky situations, and allows you to zoop around!";
        public override string SpriteName => "Kill_Slugcat";
        public override Color Color => new(0.372f, 0.752f, 0.098f); //Saint colore :))
        public override bool AlwaysUnlocked => true;
        public override bool MSCDependant => true;
        public override CustomPerkType PerkType => CustomPerkType.Custom;
    }
}
