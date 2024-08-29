using UnityEngine;

namespace ExpeditionEnhanced.ExampleContent
{
    using static ExampleBurdenHooks;
    public class Confused : EECustomBurden
    {
        public override float ScoreMultiplier => 40f;
        public override string ID => "bur-confused";
        public override string DisplayName => "CONFUSED";
        public override string ManualDescription => "Due to an earlier incident, slugcat can't view the map or check the cycle timer.";
        public override Color Color => new Color(1f, 0.949f, 0.25f);
        public override bool UnlockedByDefault => true;
        public override string Description => ManualDescription;

        public override void ApplyHooks()
        {
            On.HUD.RainMeter.ctor += RainMeter_Ctor;
            On.HUD.RainMeter.Draw += RainMeter_Draw;
            On.HUD.Map.Draw += Map_Draw;
            //Splosh
            On.Player.checkInput += Player_checkInput;
        }
    }
}
