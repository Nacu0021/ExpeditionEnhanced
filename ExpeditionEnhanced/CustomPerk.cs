using UnityEngine;
using RWCustom;
using MoreSlugcats;
using System;
using ItemType = AbstractPhysicalObject.AbstractObjectType;
using MSCItemType = MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType;

namespace ExpeditionEnhanced
{
    public abstract class CustomPerk : CustomContent
    {
        /// <summary>Name of the sprite used for displaying the perk in the select menu and in game.</summary>
        public abstract string SpriteName { get; }

        /// <summary>The description of a perk shown in the select menu.</summary>
        public abstract string Description { get; }

        /// <summary>The type of the custom perk. 
        /// OnStart - Spawns an item/creature in the first shelter of an expedition.
        /// OnKill - Does something whenever a creature is killed.
        /// Custom - Entirely custom logic. Helped by the use of ExpeditonEnhanced.ActiveContent.</summary>
        public virtual CustomPerkType PerkType { get; } = CustomPerkType.Custom;

        /// <summary>Specifies what item is spawned with an OnStart perk.</summary>
        public virtual ItemType StartItem { get; }

        /// <summary>Specifies what creature is spawned with an OnStart perk.</summary>
        public virtual CreatureTemplate.Type StartCreature { get; }

        /// <summary>Specifies the amount of items/creatures spawned with an OnStart perk.</summary>
        public virtual int StartObjectCount { get; } = 1;

        public class CustomPerkType : ExtEnum<CustomPerkType>
        {
            public static readonly CustomPerkType Custom = new CustomPerkType("Custom", true);
            public static readonly CustomPerkType OnStart = new CustomPerkType("OnStart", true);
            public static readonly CustomPerkType OnKill = new CustomPerkType("OnKill", true);

            public CustomPerkType(string value, bool register = false) : base(value, register) { }
        }

        public CustomPerk() { }

        /// <summary>Called at the start of an expedition in the starting shelter. Can be overridden for custom logic.</summary>
        public virtual void OnStart(Room room, WorldCoordinate position)
        {
            if (StartItem != null)
            {
                AbstractPhysicalObject startItem = GetCorrectAPO(StartItem, room, position);
                room.abstractRoom.entities.Add(startItem);
                startItem.Realize();
            } 
            else if (StartCreature != null)
            {
                AbstractCreature startCreature = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(StartCreature), null, position, room.game.GetNewID());
                room.abstractRoom.AddEntity(startCreature);
            } else
            {
                throw new Exception($"{ID}.OnStart: StartItem and StartCreature missing. How did this happen...");
            }
        }

        /// <summary>Called whenever a player kills something. Meant to be overridden.</summary>
        public virtual void OnKill(SocialEventRecognizer socialEventRecognizer, Player player, Creature victim)
        {
        }

        /// <summary>Some object types require their own APO's else they dont work, this method fetches those.</summary>
        public static AbstractPhysicalObject GetCorrectAPO(ItemType type, Room room, WorldCoordinate position)
        {
            AbstractPhysicalObject obj = new AbstractPhysicalObject(room.world, type, null, position, room.game.GetNewID());
            //Mfw ExtEnums dont support switch statements D:
            if (type == ItemType.Spear)
            {
                return new AbstractSpear(room.world, null, position, room.game.GetNewID(), false);
            }
            else if (type == ItemType.EggBugEgg)
            {
                return new EggBugEgg.AbstractBugEgg(room.world, null, position, room.game.GetNewID(), Mathf.Lerp(-0.15f, 0.1f, Custom.ClampedRandomVariation(0.5f, 0.5f, 2f)));
            }
            else if (type == MSCItemType.FireEgg)
            {
                return new FireEgg.AbstractBugEgg(room.world, null, position, room.game.GetNewID(), Mathf.Lerp(0.35f, 0.6f, Custom.ClampedRandomVariation(0.5f, 0.5f, 2f)));
            }
            else if (type == ItemType.BubbleGrass)
            {
                return new BubbleGrass.AbstractBubbleGrass(room.world, null, position, room.game.GetNewID(), 1f, -1, -1, null);
            }
            else if (type == ItemType.DataPearl)
            {
                return new DataPearl.AbstractDataPearl(room.world, type, null, position, room.game.GetNewID(), -1, -1, null, DataPearl.AbstractDataPearl.DataPearlType.Misc);
            }
            else if (type == ItemType.PebblesPearl)
            {
                return   new PebblesPearl.AbstractPebblesPearl(room.world, null, position, room.game.GetNewID(), -1, -1, null, 0, 0);
            }
            else if (type == MSCItemType.LillyPuck)
            {
                return new LillyPuck.AbstractLillyPuck(room.world, null, position, room.game.GetNewID(), 3, -1, -1, null);
            }
            else if (type == ItemType.SeedCob)
            {
                return new SeedCob.AbstractSeedCob(room.world, null, position, room.game.GetNewID(), -1, -1, false, null);
            }
            else if (type == ItemType.SporePlant)
            {
                return new SporePlant.AbstractSporePlant(room.world, null, position, room.game.GetNewID(), -1, -1, null, false, true);
            }
            else if (type == ItemType.WaterNut)
            {
                return new WaterNut.AbstractWaterNut(room.world, null, position, room.game.GetNewID(), -1, -1, null, false);
            }
            else if (AbstractConsumable.IsTypeConsumable(type))
            {
                return new AbstractConsumable(room.world, type, null, position, room.game.GetNewID(), -1, -1, null);
            }
            else if (type == ItemType.OverseerCarcass)
            {
                return new OverseerCarcass.AbstractOverseerCarcass(room.world, null, position, room.game.GetNewID(), new Color(1f, 0.8f, 0.3f), 1);
            }
            else if (type == MSCItemType.JokeRifle)
            {
                return new JokeRifle.AbstractRifle(room.world, null, position, room.game.GetNewID(), JokeRifle.AbstractRifle.AmmoType.Rock);
            }
            else if (type == ItemType.VultureMask)
            {
                return new VultureMask.AbstractVultureMask(room.world, null, position, room.game.GetNewID(), UnityEngine.Random.Range(0, 4000), false);
            }

            return obj;
        }
    }
}
