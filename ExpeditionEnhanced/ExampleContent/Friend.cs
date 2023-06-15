using Expedition;
using UnityEngine;
using MoreSlugcats;

namespace ExpeditionEnhanced.ExampleContent
{
    public class Friend : CustomPerk
    {
        public override string ID => "unl-friend";
        public override string Name => "Friend";
        public override string Description => "Start the expedition with a lizard friend!";
        public override string ManualDescription => "Start the expedition with a friendly lizard, the type of lizard is different for all base slugcats.";
        public override string SpriteName => "FriendA";
        public override Color Color => Color.magenta;
        public override bool AlwaysUnlocked => true;
        public override CustomPerkType PerkType => CustomPerkType.OnStart;
        public override CreatureTemplate.Type StartCreature => CreatureTemplate.Type.PinkLizard;

        //Writing a custom OnStart. The spawning creature code is stolen from the original OnStart method, and then its added upon.
        public override void OnStart(Room room, WorldCoordinate position)
        {
            //Choosing a lizard type based on the slugcat youre playing (with default being the StartCreature which is PinkLizard)
            CreatureTemplate.Type friendType = StartCreature;
            if (ExpeditionData.slugcatPlayer == SlugcatStats.Name.Red)
            {
                friendType = CreatureTemplate.Type.CyanLizard;
            }
            else if (ExpeditionData.slugcatPlayer == SlugcatStats.Name.Yellow)
            {
                friendType = CreatureTemplate.Type.BlueLizard;
            }
            else if (ExpeditionData.slugcatPlayer == SlugcatStats.Name.Night)
            {
                friendType = CreatureTemplate.Type.BlackLizard;
            }
            else if (ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
            {
                friendType = MoreSlugcatsEnums.CreatureTemplateType.EelLizard;
            }
            else if (ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                friendType = MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard;
            }
            else if (ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
            {
                friendType = MoreSlugcatsEnums.CreatureTemplateType.SpitLizard;
            }
            else if (ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Spear)
            {
                friendType = CreatureTemplate.Type.WhiteLizard;
            }
            else if (ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
            {
                friendType = CreatureTemplate.Type.RedLizard;
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
