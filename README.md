# Expeditions Enhanced


A content mod and a framework for the Expedition gamemode in Rain World.

![image](https://github.com/user-attachments/assets/b30b9fca-6908-4d49-ac19-e8f2692225ae)


On it's own the mod adds 8 new perks, and 4 new burdens. All were designed to be fun and a good example of how to add your own stuff!

## Adding custom content
**IMPORTANT** - This tutorial is now outdated, adding custom content is very similar but now you have to inherit from EECustomPerk and EECustomBurden, also use Modding.Expedition.CustomPerks/CustomBurdens.Register, and also in general cross examine it with the official support for adding perks/burdens


Some basic C# and Rain World modding knowledge is required, and I'll assume it's known.  
Remember to reference `ExpeditionEnhanced.dll` in your project!  
Also feel free to poke around the source code and example content in the [Example Content directory](ExpeditionEnhanced/ExampleContent) for any reason.

### Custom Perks
Start by creating a class that inherits from `CustomPerk`. Then implement all necessary/wanted abstract/virtual properties.  
For example:
```csharp
public class BlueFruitMeal : CustomPerk
{
    //The ID with which you'll be referring to the perk. Needs to start with "unl-"
    public override string ID => "unl-bluefrutmeal"; 
    
    //Display name for the perk select menu and the manual
    public override string Name => "Blue Fruit"; 
    
    //Description for the perk select menu
    public override string Description => "Start the expedition with a Blue Fruit meal!";
    
    //Description for the expedition manual
    public override string ManualDescription => "Start the expedition with 3 Blue Fruits, a yummy meal that might help on the first cycle.";
    
    //Sprite name used for the perk display in various places
    public override string SpriteName => "Symbol_DangleFruit";
    
    //Color the perk uses in various places
    public override Color Color => Color.blue; 
    
    //Whether the perk is always unlocked or not (false by default)
    public override bool AlwaysUnlocked => true;
    
    //Essentially what the perk does, this one spawns an item/creature at the start of an expedition
    public override CustomPerkType PerkType => CustomPerkType.OnStart;
    
    //The properties below are only used if the PerkType is set to OnStart
    
    //Specifying what item is spawned at the start
    public override AbstractPhysicalObject.AbstractObjectType StartItem => AbstractPhysicalObject.AbstractObjectType.DangleFruit;
    //If you want to spawn a creature instead, override `CreatureTemplate.Type StartCreature`
    
    //The amount of spawned items/creatures
    public override int StartObjectCount => 3;
}
```
There are three types of Custom Perks this framework allows you to use.
+ OnStart - Spawns an item/creature in the first shelter of an expedition.  
If used, it's necessary to override the `StartItem` or `StartCreature` properties, with an optional `StartObjectCount`.  
You can override the `OnStart` method to write custom logic. [Like in the example Friend perk.](ExpeditionEnhanced/ExampleContent/Friend.cs)
+ OnKill - Does something whenever a creature is killed.  
You can override the `OnKill` method to write custom logic. [Like in the example Leeching perk](ExpeditionEnhanced/ExampleContent/Leeching.cs)
+ OnAttack - Does something whenever a creature is damaged, or a weapon flies near it. 
You can override the `OnAttack` method to write custom logic. [Like in the example Explosive Damage perk](ExpeditionEnhanced/ExampleContent/ExplosiveDamage.cs)
+ Custom - Entirely custom logic. Your friend here will be using the `ExpeditionsEnhanced.ActiveContent` method.  
On its own the perk does nothing, but [you can write hooks checking whether the perk is active in other places.](https://github.com/Nacu0021/ExpeditionEnhanced/blob/master/ExpeditionEnhanced/ExampleContent/ExamplePerkHooks.cs??plain=1#L34)  
For a more complex example showcasing how to add exclusive slugcat features for every slugcat, [look at how the example SaintTongue perk does it](https://github.com/Nacu0021/ExpeditionEnhanced/blob/master/ExpeditionEnhanced/ExampleContent/ExamplePerkHooks.cs??plain=1#L200).

note: It's possible to use custom logic for every type of perk, the two first perk types just do some of the dirty work for you.

### Custom Burdens
Similar to Custom Perks, create a class that inherits from `CustomBurden`. Then implement all necessary/wanted abstract/virtual properties.  
```csharp
public class Confusedd : CustomBurden
{
    //The % amount of how much the burden increases the expedition's score.
    public override float ScoreMultiplier => 40f;
    
    //The ID with which you'll be referring to the burden. Needs to start with "bur-"
    public override string ID => "bur-confusedd";
    
    //The rest is explained above, in the Custom Perks section
    public override string Name => "CONFUSED";
    public override string ManualDescription => "Slugcat hit its head on a rock earlier, now its memory seems to be working funny. What time is it again?";
    public override Color Color => new Color(1f, 0.949f, 0.25f);
    public override bool AlwaysUnlocked => true;
}
```
For custom burdens, it's necessary to write custom logic. Use the `ExpeditionsEnhanced.ActiveContent` method to easily check whether a specific burden is active.
[Here's an example of how you might use it.](https://github.com/Nacu0021/ExpeditionEnhanced/blob/master/ExpeditionEnhanced/ExampleContent/ExampleBurdenHooks.cs#L106)

### Adding custom content to the game
**This step is extremely necessary**


In your plugins `OnEnable`, or `RainWorld.OnModsInit`, or wherever, call the `ExpeditionEnhanced.ExpeditionsEnhanced.RegisterExpeditionContent` method, with your custom perks/burdens as the arguments.
```csharp
ExpeditionsEnhanced.RegisterExpeditionContent( new BlueFruitMeal(), new Confusedd() ); //Adding our example content to Expedition properly
```
The content will automatically get added to the perk/burden select page, and the expedition manual.
