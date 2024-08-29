using UnityEngine;
using RWCustom;
using DevConsole;
using DevConsole.Commands;

namespace ExpeditionEnhanced
{
    using BepInEx.Logging;
    using ExampleContent;

    public class Console
    {
        public static void RegisterCommands()
        {
            new CommandBuilder("fracture")
                .Run(args =>
                {
                    ExampleBurdenHooks.GlobalSpikeEventDifficulty = 0;
                    ExampleBurdenHooks.SpikesLeft = 20;
                    ExampleBurdenHooks.SpikeEventCountdown = 20;
                    if (args.Length > 0)
                    {
                        if (float.TryParse(args[0], out float val))
                        {
                            ExampleBurdenHooks.GlobalSpikeEventDifficulty = val;
                        }
                        if (args.Length > 1)
                        {
                            if (int.TryParse(args[1], out int vall))
                            {
                                ExampleBurdenHooks.SpikesLeft = vall;
                            }
                        }
                    }
                    GameConsole.WriteLine("Watch your step.", new Color(0.41f, 0f, 0f));
                })
                .Help("fracture [difficulty?] [spikesleft?] - Starts a spike event, from the Marked burden. Only if it's enabled.")
                .Register();


            new CommandBuilder("boom")
                .RunGame((game, args) =>
                {
                    try
                    {
                        float mass = 5f * Random.value; 
                        if (args.Length > 0)
                        {
                            if (float.TryParse(args[0], out float val))
                            {
                                mass = val;
                            }
                        }
                        var player = game.Players[0].realizedCreature as Player;
                        player.room.AddObject(new VolatileBomb(GameConsole.TargetPos.Pos, new Vector2(Custom.RNV().x, Random.value) * (2f + Random.value * 3f + mass * 3f), mass * 1.25f));
                    }
                    catch { GameConsole.WriteLine("Failed to spawn VolatileBomb!"); }
                })
                .Register();

            Plugin.logger.LogMessage("DevConsole enabled, adding commands.");
        }
    }
}
