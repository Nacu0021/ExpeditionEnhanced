using UnityEngine;

namespace ExpeditionEnhanced.ExampleContent
{
    public class MakeAWish : EECustomPerk
    {
        public override string ID => "unl-makeawish";
        public override string DisplayName => "Make a Wish";
        public override string Description => "Throw pearls into the abyss to receive gifts";
        public override string ManualDescription => "Throw any pearl into a death pit in order to receive a random prize. Could be anything!";
        public override string SpriteName => "Symbol_Pearl";
        public override Color Color => new Color(0.8f, 0.8f, 0.8f);
        public override bool UnlockedByDefault => true;
        public override CustomPerkType PerkType => CustomPerkType.Custom;

        public override void ApplyHooks()
        {
            On.Player.ReleaseGrasp += ExamplePerkHooks.Player_ReleaseGrasp;
            On.DataPearl.PickedUp += ExamplePerkHooks.DataPearl_PickedUp;
            On.DataPearl.Update += ExamplePerkHooks.DataPearl_Update;
        }
    }
}
