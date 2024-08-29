using BepInEx;
using System.Security.Permissions;
using System.Security;

#pragma warning disable CS0618
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace ExpeditionEnhanced
{
    using ExampleContent;
    using Modding.Expedition;

    [BepInPlugin("nacu.expeditionenhanced", "Expedition Enhanced", "2.1")]
    public class Plugin : BaseUnityPlugin
    {
        public static bool AppliedAlreadyDontDoItAgainPlease;
        internal static BepInEx.Logging.ManualLogSource logger;

        public static SoundID WarningSound;
        public static SoundID SpikeSound;
        public static SoundID LongBeep;

        public void Awake()
        {
            WarningSound = new SoundID("ee_event_warning_rumble", true);
            SpikeSound = new SoundID("ee_ground_spike_launch", true);
            LongBeep = new SoundID("ee_bomb_long_beep", true);
        }

        public void OnEnable()
        {
            logger = Logger;
            On.RainWorld.OnModsInit += OnModsInit;
        }

        public void OnDisable()
        {
            logger = null;
        }

        public static void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld raingame)
        {
            orig(raingame);

            if (!AppliedAlreadyDontDoItAgainPlease)
            {
                AppliedAlreadyDontDoItAgainPlease = true;

                Futile.atlasManager.LoadAtlas("Atlases/expeditionsenhanced");

                ExpeditionsEnhanced.Apply();
                //ExpeditionsEnhanced.RegisterExpeditionContent( new Gourmet(), new Leeching(), new Friend(), new SaintTongue(), new MakeAWish(), new ThunderGod(), new GroundSpikes(), new ExplosiveDamage(), //Perks
                //                                              new Crippled(), new Confused(), new Marked(), new Volatile() ); //Burdens
                //ExamplePerkHooks.Apply();
                ExampleBurdenHooks.Apply();

                CustomPerks.Register( new Gourmet(), new Leeching(), new Friend(), new SaintTongue(), new MakeAWish(), new ThunderGod(), new GroundSpikes(), new ExplosiveDamage() );
                CustomBurdens.Register( new Crippled(), new Confused(), new Marked(), new Volatile() );

                //ChallengeHooks.Apply();

                //Custom DevConsole commands. Soft dependency
                try { Console.RegisterCommands(); }
                catch { logger.LogMessage("DevConsole disabled, skipping adding commands."); }
            }
        }
    }
}