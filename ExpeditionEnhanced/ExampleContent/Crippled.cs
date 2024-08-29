using UnityEngine;

namespace ExpeditionEnhanced.ExampleContent
{
    using static ExampleBurdenHooks;
    public class Crippled : EECustomBurden
    {
        public override float ScoreMultiplier => 50f;
        public override string ID => "bur-crippled";
        public override string DisplayName => "LOST";
        public override string ManualDescription => "Slugcat is teleported to a random shelter in the current region every time it hibernates :((<LINE>How did this happen";
        public override Color Color => LOST_BLUE;
        public override bool UnlockedByDefault => true;
        public override string Description => ManualDescription;

        public static Color LOST_BLUE = new(124f / 255f, 183f / 255f, 242f / 255f);

        public override void ApplyHooks()
        {
            On.SaveState.BringUpToDate += SaveState_BringUpToDate;
        }
    }
}
