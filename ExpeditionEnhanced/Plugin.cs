using BepInEx;
using System.Security.Permissions;
using System.Security;

#pragma warning disable CS0618
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace ExpeditionEnhanced
{
    using ExampleContent;

    [BepInPlugin("nacu.expeditionenhanced", "Expedition Enhanced", "1.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static bool AppliedAlreadyDontDoItAgainPlease;
        internal static BepInEx.Logging.ManualLogSource logger;

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

                ExpeditionsEnhanced.Apply();
                ExpeditionsEnhanced.RegisterExpeditionContent( new BlueFruit(), new Leeching(), new Friend(), new SaintTongue(), new ExplosiveDamage(), new MakeAWish(), //Perks
                                                              new Crippled(), new Confused() ); //Burdens
                ExamplePerkHooks.Apply();
                ExampleBurdenHooks.Apply();
            }
        }
    }
}