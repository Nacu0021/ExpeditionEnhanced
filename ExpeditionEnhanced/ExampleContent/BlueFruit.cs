using Modding.Expedition;
using UnityEngine;

namespace ExpeditionEnhanced.ExampleContent
{
    public class BlueFruit : EECustomPerk
    {
        public override string ID => "unl-blufrut"; //Needs to start with "unl-"
        public override string DisplayName => "Blue Fruit"; //Display name for the perk select menu and the manual
        public override string Description => "Start the expedition with a Blue Fruit meal!"; //Description for the perk select menu
        public override string ManualDescription => "Start the expedition with 3 Blue Fruits, a yummy meal that might help on the first cycle.";
        public override string SpriteName => "Symbol_DangleFruit"; //Sprite name used for the perk display in various places
        public override Color Color => Color.blue; //Color in the perk select menu and manual
        public override bool UnlockedByDefault => true;
        public override CustomPerkType PerkType => CustomPerkType.OnStart; //Essentially what the perk does, this one spawns an item/creature at the start of an expedition
        public override AbstractPhysicalObject.AbstractObjectType StartItem => AbstractPhysicalObject.AbstractObjectType.DangleFruit; //Specifying what item is spawned at the start
        //public override CreatureTemplate.Type StartCreature => CreatureTemplate.Type.Hazer; //Specifying what creature is spawned at the start. Can only be either the item or creature.
        public override int StartObjectCount => 3; //The amount of spawned items/creatures
    }
}
