using UnityEngine;

namespace ExpeditionEnhanced.ExampleContent
{
    internal class GroundSpikes : EECustomPerk
    {
        public override string ID => "unl-spikes";
        public override string DisplayName => "Ground Spikes";
        public override string Description => "Spawn spikes after pouncing";
        public override string ManualDescription => "Pouncing and other movement techniques spawn two spikes where the player stood before jumping.";
        public override string SpriteName => "Symbol_Spikes";
        public override Color Color => new Color(0.41f, 0f, 0f);
        public override CustomPerkType PerkType => CustomPerkType.Custom;

        public override void ApplyHooks()
        {
            On.Player.Jump += ExamplePerkHooks.Player_Jump;
        }
    }
}
