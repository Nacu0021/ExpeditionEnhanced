using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using HUD;
using UnityEngine;
using RWCustom;
using System.Collections.Generic;
using static Expedition.ExpeditionGame;
using System.Linq;

namespace ExpeditionEnhanced.ExampleContent
{
    public class CrippledMechanics
    {
        public static Dictionary<PlayerState, float> PlayerCripple = new();

        public static void Apply()
        {
            IL.Player.TerrainImpact += Player_TerrainImpactIL;
            IL.Player.ThrowObject += Player_ThrowObject;
            On.PlayerState.ctor += PlayerState_ctor;
            On.SaveState.SessionEnded += SaveState_SessionEnded;
            On.Player.Update += Player_Update;
        }

        //Im changing local variables so im using IL Hooks
        public static void Player_TerrainImpactIL(ILContext il)
        {
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdcR4(8),
                x => x.MatchStloc(3)
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldarg_3);
                // Flop
                c.EmitDelegate<Action<Player, float>>((player, speed) =>
                {
                    if (ExpeditionsEnhanced.ActiveContent("bur-crippled") && speed > 20f && player.playerState != null)
                    {
                        if (CrippledMechanics.PlayerCripple.TryGetValue(player.playerState, out _))
                        {
                            float increase = Mathf.Min(0.2f, speed / UnityEngine.Random.Range(190, 230));
                            CrippledMechanics.PlayerCripple[player.playerState] += increase;
                            player.slowMovementStun = (int)(speed * (1f + CrippledMechanics.PlayerCripple[player.playerState]));
                            //Plugin.logger.LogMessage("Cripple for: " + increase);
                            //Plugin.logger.LogMessage("Cripple: " + CrippledMechanics.PlayerCripple[player.playerState]);
                            player.room.PlaySound(SoundID.Slime_Mold_Terrain_Impact, player.mainBodyChunk.pos, Mathf.Max(0.5f, CrippledMechanics.PlayerCripple[player.playerState] * 2f), 0.5f);
                            for (int i = 0; i < Mathf.Min(CrippledMechanics.PlayerCripple[player.playerState] * 20, 10); i++)
                            {
                                player.room.AddObject(new Spark(player.bodyChunks[1].pos, Custom.RNV() * 5f * UnityEngine.Random.value, Color.white, null, 10, 25));
                            }
                        }
                    }
                });

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
        }


        public static void Player_Update(On.Player.orig_Update orig, Player player, bool eu)
        {
            orig.Invoke(player, eu);

            if (ExpeditionsEnhanced.ActiveContent("bur-crippled") && PlayerCripple.TryGetValue(player.playerState, out float cripple))
            {
                if (cripple > 1f) player.Die();
                if (player.aerobicLevel > (1f - Mathf.Pow(cripple, 1.8f)))
                {
                    player.Stun((int)(cripple * 100));
                    player.slowMovementStun = (int)(player.stun * 1.5f);
                }
            }
        }

        public static void SaveState_SessionEnded(On.SaveState.orig_SessionEnded orig, SaveState self, RainWorldGame game, bool survived, bool newMalnourished)
        {
            if (ExpeditionsEnhanced.ActiveContent("bur-crippled")) PlayerCripple.Clear();
            orig.Invoke(self, game, survived, newMalnourished);
        }

        public static void Player_ThrowObject(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt<Creature>("ReleaseGrasp")
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Action<Player, int>>((player, grasp) =>
                {
                    if (ExpeditionsEnhanced.ActiveContent("bur-crippled") && PlayerCripple.TryGetValue(player.playerState, out float cripple))
                    {
                        float velocityMarkiplier = Mathf.Min(1f, 1.6f - cripple);
                        foreach (BodyChunk chunk in player.grasps[grasp].grabbed.bodyChunks)
                        {
                            chunk.vel *= velocityMarkiplier;
                        }
                        if (player.grasps[grasp].grabbed is Spear s) s.spearDamageBonus *= Custom.LerpMap(cripple, 0f, 1f, 1f, 0.66f);
                    }
                });
            }
        }

        public static void PlayerState_ctor(On.PlayerState.orig_ctor orig, PlayerState self, AbstractCreature crit, int playerNumber, SlugcatStats.Name slugcatCharacter, bool isGhost)
        {
            orig.Invoke(self, crit, playerNumber, slugcatCharacter, isGhost);

            PlayerCripple[self] = 0f;
        }
    }
}
