using UnityEngine;


namespace ExpeditionEnhanced.ExampleContent
{
    public class Confused : CustomBurden
    {
        public override float ScoreMultiplier => 40f;
        public override string ID => "bur-confused";
        public override string Name => "CONFUSED";
        public override string ManualDescription => "Slugcat hit its head on a rock earlier, now its memory seems to be working funny. What time is it again?";
        public override Color Color => new Color(1f, 0.949f, 0.25f);
        public override bool AlwaysUnlocked => true;
    }
}
