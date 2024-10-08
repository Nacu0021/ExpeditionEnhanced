﻿using UnityEngine;

namespace ExpeditionEnhanced.ExampleContent
{
    internal class ThunderGod : EECustomPerk
    {
        public override string ID => "unl-thundergod";
        public override string DisplayName => "Flying Thunder Cat";
        public override string Description => "Hold the throw button when throwing something to swap places with it!";
        public override string ManualDescription => "Allows the player to swap places with any thrown object by holding the throw button after throwing, then releasing it.";
        public override string SpriteName => "symbol_thundergod";
        public override Color Color => new Color(0.615f, 0.925f, 0.960f);
        public override bool UnlockedByDefault => true;
        public override CustomPerkType PerkType => CustomPerkType.Custom;

        public override void ApplyHooks()
        {
            IL.Player.ThrowObject += ExamplePerkHooks.Player_ThrowObject;
            On.Player.GrabUpdate += ExamplePerkHooks.Player_GrabUpdate;
        }
    }
}
