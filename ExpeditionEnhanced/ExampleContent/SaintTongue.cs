using UnityEngine;

namespace ExpeditionEnhanced.ExampleContent
{
    internal class SaintTongue : EECustomPerk
    {
        public override string ID => "unl-stongue";
        public override string DisplayName => "Saint's Tongue";
        public override string Description => "Allows the player to use Saint's tongue ability";
        public override string ManualDescription => "Allows any slugcat to use Saint's tongue, which can get you out of sticky situations, and allows you to zoop around!";
        public override string SpriteName => "Kill_Slugcat";
        public override Color Color => new(0.372f, 0.752f, 0.098f); //Saint colore :))
        public override bool UnlockedByDefault => true;
        public override CustomPerkType PerkType => CustomPerkType.Custom;

        public override bool AvailableForSlugcat(SlugcatStats.Name name)
        {
            return ModManager.MSC;
        }

        public override void ApplyHooks()
        {
            On.Player.ctor += ExamplePerkHooks.Player_ctor;
            On.Player.SaintTongueCheck += ExamplePerkHooks.Player_SaintTongueCheck;
            On.Player.ClassMechanicsSaint += ExamplePerkHooks.Player_ClassMechanicsSaint;
            On.PlayerGraphics.ctor += ExamplePerkHooks.PlayerGraphics_ctor;
            On.PlayerGraphics.InitiateSprites += ExamplePerkHooks.PlayerGraphics_InitiateSprites;
            On.PlayerGraphics.DrawSprites += ExamplePerkHooks.PlayerGraphics_DrawSprites;
            On.PlayerGraphics.ApplyPalette += ExamplePerkHooks.PlayerGraphics_ApplyPalette;
            On.PlayerGraphics.MSCUpdate += ExamplePerkHooks.PlayerGraphics_MSCUpdate;
        }
    }
}
