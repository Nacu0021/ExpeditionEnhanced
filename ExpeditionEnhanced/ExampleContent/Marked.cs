using UnityEngine;

namespace ExpeditionEnhanced.ExampleContent
{
    public class Marked : CustomBurden
    {
        public override float ScoreMultiplier => 80f;
        public override string ID => "bur-marked";
        public override string Name => "MARKED";
        public override string ManualDescription => "Causes spontaneous rumbles to happen below the slugcat's feet, best not to know what causes them. Tread carefully.";
        public override Color Color => new Color(0.41f, 0f, 0f);
        public override bool AlwaysUnlocked => true;
    }
}
