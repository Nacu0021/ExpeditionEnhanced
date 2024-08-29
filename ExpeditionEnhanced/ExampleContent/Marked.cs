using Expedition;
using UnityEngine;

namespace ExpeditionEnhanced.ExampleContent
{
    using static ExampleBurdenHooks;
    public class Marked : EECustomBurden
    {
        public override float ScoreMultiplier => 80f;
        public override string ID => "bur-marked";
        public override string DisplayName => "MARKED";
        public override string ManualDescription => "Causes spontaneous rumbles to happen below the slugcat's feet, best not to know what causes them. Tread carefully.";
        public override Color Color => new Color(0.41f, 0f, 0f);
        public override bool UnlockedByDefault => true;
        public override string Description => ManualDescription;

        public override ExpeditionGame.BurdenTracker CreateTracker(RainWorldGame game)
        {
            return new ExampleBurdenHooks.SpikeEventTracker(game);
        }

        public override void ApplyHooks()
        {
            On.ProcessManager.PostSwitchMainProcess += ProcessManager_PostSwitchMainProcess;
        }
    }
}
