using UnityEngine;

namespace ExpeditionEnhanced
{
    public abstract class CustomContent
    {
        /// <summary>The id of a perk/burden. Used when checking whether its active.</summary>
        public abstract string ID { get; }

        /// <summary>The name of a perk/burden. Shown in the select menu and manual.</summary>
        public abstract string Name { get; }

        /// <summary>The description of a perk/burden shown in the manual.</summary>
        public abstract string ManualDescription { get; }

        /// <summary>Color of the perk/burden. Shown in the select menu, manual, and in game.</summary>
        public abstract Color Color { get; }

        /// <summary>Whether a perk/burden is unlocked from the start. Leave false if you want it to be unlocked via a quest/other way.</summary>
        public virtual bool AlwaysUnlocked { get; } = false;

        /// <summary>Whether a perk/burden requires MSC to work properly.</summary>
        public virtual bool MSCDependant { get; } = false;

        public CustomContent() { }
    }
}
