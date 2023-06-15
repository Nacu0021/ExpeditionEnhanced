using UnityEngine;

namespace ExpeditionEnhanced.ExampleContent
{
    public class Crippled : CustomBurden
    {
        public override float ScoreMultiplier => 30f;
        public override string ID => "bur-crippled";
        public override string Name => "CRIPPLED";
        public override string ManualDescription => "Limits the slugcat's fall tolerance, causing easier deaths and heavier ground impact repercussions. Watch your step.";
        public override Color Color => new Color(0.098f, 0.4f, 0.168f);
        public override bool AlwaysUnlocked => true;
    }
}
