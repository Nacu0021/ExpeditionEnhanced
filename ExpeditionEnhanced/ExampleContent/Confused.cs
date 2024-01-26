using UnityEngine;

namespace ExpeditionEnhanced.ExampleContent
{
    public class Confused : CustomBurden
    {
        public override float ScoreMultiplier => 60f;
        public override string ID => "bur-confused";
        public override string Name => "CONFUSED";
        public override string ManualDescription => "Due to an earlier incident, slugcat can't view the map or check the cycle timer, and is also teleported to a random shelter each time it hibernates :((";
        public override Color Color => new Color(1f, 0.949f, 0.25f);
        public override bool AlwaysUnlocked => true;
    }
}
