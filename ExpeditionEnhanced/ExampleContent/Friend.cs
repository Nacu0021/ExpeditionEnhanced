using Expedition;
using UnityEngine;
using MoreSlugcats;
using CreatureType = CreatureTemplate.Type;
using MSCCreatureType = MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType;

namespace ExpeditionEnhanced.ExampleContent
{
    public class Friend : CustomPerk
    {
        public override string ID => "unl-friend";
        public override string Name => "Friend";
        public override string Description => "Start the expedition with a lizard friend!";
        public override string ManualDescription => "Start the expedition with a friendly lizard, the type of lizard is different for all base slugcats.";
        public override string SpriteName => "FriendA";
        public override Color Color => new Color (1f, 0.529f, 0.913f);
        public override bool AlwaysUnlocked => true;
        public override CustomPerkType PerkType => CustomPerkType.OnStart;
        public override CreatureType StartCreature => CreatureType.PinkLizard;

        //Writing a custom OnStart. The spawning creature code is stolen from the original OnStart method, and then its added upon.
        public override void OnStart(Room room, WorldCoordinate position)
        {
            if (room.world.region == null) return;

            //Choosing a lizard type based on the slugcat youre playing (with default being the StartCreature which is PinkLizard)
            string region = room.world.region.name.ToLowerInvariant();
            bool water = region == "sl" || region == "ms" || region == "ds" || region == "vs" || region == "lm" || region == "ug";
            bool air = region == "uw" || region == "cc" || region == "si" || region == "lc" || region == "ss" || region == "rm" || region == "dm" || region == "cl";
            CreatureType friendType = air ? CreatureType.WhiteLizard : water ? (ModManager.MSC ? MSCCreatureType.EelLizard : CreatureType.Salamander) : StartCreature;

            if (ExpeditionData.slugcatPlayer == SlugcatStats.Name.Red)
            {
                friendType = water ? CreatureType.Salamander : CreatureType.CyanLizard;
            }
            else if (ExpeditionData.slugcatPlayer == SlugcatStats.Name.Night)
            {
                friendType = CreatureType.BlackLizard;
            }
            else if (ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
            {
                friendType = air ? CreatureType.CyanLizard : MSCCreatureType.EelLizard;
            }
            else if (ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                friendType = air ? CreatureType.WhiteLizard : water ? MSCCreatureType.EelLizard : StartCreature;
            }
            else if (ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
            {
                friendType = air ? CreatureType.WhiteLizard : water ? MSCCreatureType.EelLizard : MSCCreatureType.SpitLizard;
            }
            else if (ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Spear)
            {
                friendType = water ? CreatureType.Salamander : CreatureType.CyanLizard;
            }
            else if (ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
            {
                friendType = air ? CreatureType.CyanLizard : water ? MSCCreatureType.EelLizard : CreatureType.RedLizard;
            }
            else if (ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
            {
                friendType = MSCCreatureType.TrainLizard;
            }
            else if (ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Slugpup)
            {
                friendType = CreatureType.BlueLizard;
            }
            AbstractCreature player = room.game.Players[0];
            if (player != null)
            {
                AbstractCreature startCreature = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(friendType), null, position, room.game.GetNewID());
                room.abstractRoom.AddEntity(startCreature);
                //Making the liz befriend the player
                startCreature.state.socialMemory.GetOrInitiateRelationship(player.ID).InfluenceLike(10f);
                startCreature.state.socialMemory.GetOrInitiateRelationship(player.ID).InfluenceTempLike(10f);
                startCreature.state.socialMemory.GetOrInitiateRelationship(player.ID).InfluenceKnow(1f);
            }
        }
    }
}
