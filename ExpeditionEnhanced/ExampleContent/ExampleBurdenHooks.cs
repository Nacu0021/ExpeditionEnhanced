using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using HUD;

namespace ExpeditionEnhanced.ExampleContent
{
    public class ExampleBurdenHooks
    {
        //public static ConditionalWeakTable<Player, List<CreatureSymbol>> SpiderSense = new ConditionalWeakTable<Player, List<CreatureSymbol>>();

        public static void Apply()
        {
            //Crippled burden
            IL.Player.TerrainImpact += Player_TerrainImpactIL;

            //Confused
            On.HUD.RainMeter.ctor += RainMeter_Ctor;
            On.HUD.RainMeter.Draw += RainMeter_Draw;
            On.HUD.Map.Draw += Map_Draw;

            //Splosh
            On.Player.checkInput += Player_checkInput;
        }

        //Just changing some numbers in the impact class lol
        //Im changing local variables so im using IL Hooks
        public static void Player_TerrainImpactIL(ILContext il)
        {
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdcR4(8),
                x => x.MatchStloc(3)
                ))
            {
                //We have to change 4 local values so like yeah,, annoying 
                c.Emit(OpCodes.Ldloc_0);
                c.EmitDelegate<Func<float, float>>((original) =>
                {
                    if (ExpeditionsEnhanced.ActiveContent("bur-crippled")) original *= 0.5f;
                    return original;
                });
                c.Emit(OpCodes.Stloc_0);

                c.Emit(OpCodes.Ldloc_1);
                c.EmitDelegate<Func<float, float>>((original) =>
                {
                    if (ExpeditionsEnhanced.ActiveContent("bur-crippled")) original *= 0.575f;
                    return original;
                });
                c.Emit(OpCodes.Stloc_1);

                c.Emit(OpCodes.Ldloc_2);
                c.EmitDelegate<Func<float, float>>((original) =>
                {
                    if (ExpeditionsEnhanced.ActiveContent("bur-crippled")) original *= 0.575f;
                    return original;
                });
                c.Emit(OpCodes.Stloc_2);

                c.Emit(OpCodes.Ldloc_3);
                c.EmitDelegate<Func<float, float>>((original) =>
                {
                    if (ExpeditionsEnhanced.ActiveContent("bur-crippled")) original *= 0.575f;
                    return original;
                });
                c.Emit(OpCodes.Stloc_3);
            }
            else Plugin.logger.LogError("UH OH player terrain impact 1 burden shitted " + il);

            //Part 2 of this circus
            ILCursor g = new(il);
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<Creature>("Stun")
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<Player>>((player) =>
                {
                    if (ExpeditionsEnhanced.ActiveContent("bur-crippled")) player.slowMovementStun = player.stun * 2;
                });
            }
            else Plugin.logger.LogError("UH OH player terrain impact 2 burden shitted " + il);
        }

        //Stop the rain meter from appearing and also from flashing at half the cycle time
        public static void RainMeter_Ctor(On.HUD.RainMeter.orig_ctor orig, RainMeter self, HUD.HUD hud, FContainer fContainer)
        {
            orig.Invoke(self, hud, fContainer);
            if (ExpeditionsEnhanced.ActiveContent("bur-confused")) self.halfTimeShown = true;
        }

        public static void RainMeter_Draw(On.HUD.RainMeter.orig_Draw orig, RainMeter self, float timeStacker)
        {
            orig.Invoke(self, timeStacker);
            if (ExpeditionsEnhanced.ActiveContent("bur-confused"))
            {
                for (int i = 0; i < self.circles.Length; i++)
                {
                    self.circles[i].sprite.alpha = 0f;
                }
            }
        }

        //Make the map invisible if confused burden
        public static void Map_Draw(On.HUD.Map.orig_Draw orig, Map self, float timeStacker)
        {
            if (ExpeditionsEnhanced.ActiveContent("bur-confused")) self.visible = false;
            orig.Invoke(self, timeStacker);
        }

        //Sploosh
        public static void Player_checkInput(On.Player.orig_checkInput orig, Player self)
        {
            orig.Invoke(self);

            if (self.room != null && self.input[0].mp && !self.input[1].mp) self.room.PlaySound(SoundID.Swollen_Water_Nut_Terrain_Impact, self.firstChunk);
        }
    }
}
