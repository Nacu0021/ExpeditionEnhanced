using UnityEngine;

namespace ExpeditionEnhanced.ExampleContent
{
    public class MakeAWish : CustomPerk
    {
        public override string ID => "unl-makeawish";
        public override string Name => "Make a Wish";
        public override string Description => "Throw pearls into the abyss to receive gifts";
        public override string ManualDescription => "Throw any pearl into a death pit in order to receive a random prize. Could be anything!";
        public override string SpriteName => "Symbol_Pearl";
        public override Color Color => new Color(0.8f, 0.8f, 0.8f);
        public override bool AlwaysUnlocked => true;
        public override CustomPerkType PerkType => CustomPerkType.Custom;
    }
}
