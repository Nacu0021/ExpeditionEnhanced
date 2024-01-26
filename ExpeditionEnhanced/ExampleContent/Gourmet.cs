using UnityEngine;
using ItemType = AbstractPhysicalObject.AbstractObjectType;

namespace ExpeditionEnhanced.ExampleContent
{
    public class Gourmet : CustomPerk
    {
        public override string ID => "unl-gourmet"; //Needs to start with "unl-"
        public override string Name => "Gourmet"; //Display name for the perk select menu and the manual
        public override string Description => "Start the expedition with 6 random food items!"; //Description for the perk select menu
        public override string ManualDescription => "Start the expedition with 6 random food items, a yummy meal that might help on the first cycle.";
        public override string SpriteName => "Symbol_Gourmet"; //Sprite name used for the perk display in various places
        public override Color Color => new Color (208f / 255f, 165f / 255f, 67f / 255f); //Color in the perk select menu and manual
        public override bool AlwaysUnlocked => true;
        public override CustomPerkType PerkType => CustomPerkType.OnStart; //Essentially what the perk does, this one spawns an item/creature at the start of an expedition
        public override ItemType StartItem => ItemType.DangleFruit; //Cant be empty if perk type == OnStart
        public override int StartObjectCount => 6; //The amount of spawned items/creatures

        public override void OnStart(Room room, WorldCoordinate position)
        {
            AbstractPhysicalObject startItem = GetCorrectAPO(FoodTypes[Random.Range(0, FoodTypes.Length - (ModManager.MSC ? 0 : 4))], room, position);
            room.abstractRoom.entities.Add(startItem);
            startItem.Realize();
        }

        public static ItemType[] FoodTypes =
        {
            ItemType.DangleFruit,
            ItemType.EggBugEgg,
            ItemType.WaterNut,
            ItemType.SlimeMold,
            ItemType.Mushroom,
            ItemType.JellyFish,
            new("GooieDuck", false),
            new("LillyPuck", false),
            new("LillyPuck", false),
            new("GlowWeed", false)
        };
    }
}
