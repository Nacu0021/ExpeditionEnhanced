using Expedition;
using ExpeditionEnhanced.ExampleContent;
using Menu;
using Modding.Expedition;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExpeditionEnhanced
{
    public class ExpeditionsEnhanced
    {
        //public static List<EECustomPerk> customPerks = [];
        //public static List<EECustomBurden> customBurdens = [];
        //public static int maxPerkPages = 0;
        //public static int currentPerkPage = 0;
        //public static int maxBurdenPages = 0;
        //public static int currentBurdenPage = 0;
        //public static int maxPPages = -1;
        //public static int maxBPages = -1;
        //public static bool burdenMode = false;

        //public static void RegisterExpeditionContent(params EECustomContent[] content)
        //{
        //    foreach (EECustomContent c in content)
        //    {
        //        //Annoying
        //        bool mscdotdotdot = false;
        //        foreach (var mod in ModManager.ActiveMods)
        //        {
        //            if (mod.id == "moreslugcats") mscdotdotdot = true;
        //        }
        //
        //        if (c is EECustomPerk perk)
        //        {
        //            if (customPerks.Any(x => x.ID == perk.ID)) throw new Exception("Perk with ID: " + perk.ID + " already exists! Cannot be added again!");
        //            if (!perk.ID.StartsWith("unl-")) throw new Exception(perk.Name + " perk's ID doesn't start with \"unl-\"");
        //            if (!mscdotdotdot && perk.MSCDependant)
        //            {
        //                Plugin.logger.LogMessage(perk.Name + " perk is dependant on MSC, but MSC is not enabled.");
        //                continue;
        //            }
        //            customPerks.Add(perk);
        //        } 
        //        else if (c is EECustomBurden burden)
        //        {
        //            if (customBurdens.Any(x => x.ID == burden.ID)) throw new Exception("Burden with ID: " + burden.ID + " already exists! Cannot be added again!");
        //            if (!burden.ID.StartsWith("bur-")) throw new Exception(burden.Name + " burden's ID doesn't start with \"bur-\"");
        //            if (!mscdotdotdot && burden.MSCDependant)
        //            {
        //                Plugin.logger.LogMessage(burden.Name + " burden is dependant on MSC, but MSC is not enabled.");
        //                continue;
        //            }
        //            customBurdens.Add(burden);
        //        }
        //    }
        //}

        public static bool ActiveContent(string ID)
        {
            return ModManager.Expedition && Custom.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains(ID);
            //if (ModManager.Expedition && Custom.rainWorld.ExpeditionMode)
            //{
            //    if (ExpeditionGame.activeUnlocks.Contains(ID)) return true;
            //
            //    // I dont think anyone would use this, it's just unnecessary checks
            //    //CustomContent perky = customPerks.FirstOrDefault(x => x.Name == nameOrID);
            //    //if (perky == null) perky = customBurdens.FirstOrDefault(x => x.Name == nameOrID);
            //    //if (perky != null && ExpeditionGame.activeUnlocks.Contains(perky.ID)) return true; 
            //}
            //return false;
        }

        public static void Apply()
        {
            //OnKill, OnHit, and OnStart functionality
            On.SocialEventRecognizer.Killing += SocialEventRecognizer_Killing;
            On.SocialEventRecognizer.WeaponAttack += SocialEventRecognizer_WeaponAttack;
            IL.Room.Loaded += Room_Loaded;

            //Fixing potential issue idk if its still in the game or not
            On.Expedition.ExpeditionCoreFile.FromString += ExpeditionCoreFile_FromString;

            //Adding custom perks to expedition's perk base
            //On.Expedition.ExpeditionProgression.SetupPerkGroups += ExpeditionProgression_SetupPerkGroups;
            //On.Expedition.ExpeditionProgression.UnlockSprite += ExpeditionProgression_UnlockSprite;
            //On.Expedition.ExpeditionProgression.UnlockName += ExpeditionProgression_UnlockName;
            //On.Expedition.ExpeditionProgression.UnlockDescription += ExpeditionProgression_UnlockDescription;
            //On.Expedition.ExpeditionProgression.UnlockColor += ExpeditionProgression_UnlockColor;

            //Adding custom burdens to expedition's burden base
            //On.Expedition.ExpeditionProgression.SetupBurdenGroups += ExpeditionProgression_SetupBurdenGroups;
            //On.Expedition.ExpeditionProgression.BurdenName += ExpeditionProgression_BurdenName;
            //On.Expedition.ExpeditionProgression.BurdenManualDescription += ExpeditionProgression_BurdenManualDescription;
            //On.Expedition.ExpeditionProgression.BurdenMenuColor += ExpeditionProgression_BurdenMenuColor;
            //On.Expedition.ExpeditionProgression.BurdenScoreMultiplier += ExpeditionProgression_BurdenScoreMultiplier;
            //On.Menu.UnlockDialog.UpdateBurdens += UnlockDialog_UpdateBurdens;
            //On.Menu.UnlockDialog.SetUpBurdenDescriptions += UnlockDialog_SetUpBurdenDescriptions;
            //On.Expedition.ExpeditionGame.SetUpBurdenTrackers += ExpeditionGame_SetUpBurdenTrackers;
            
            //Both
            //On.Expedition.ExpeditionProgression.CountUnlockables += ExpeditionProgression_CountUnlockables;

            //Manual business
            //On.Menu.ExpeditionManualDialog.PerkManualDescription += ExpeditionManualDialog_PerkManualDescription;
            //On.Menu.ExpeditionManualDialog.ctor += ExpeditionManualDialog_ctor;
            //On.Menu.ExpeditionManualDialog.GetManualPage += ExpeditionManualDialog_GetManualPage;

            //Ficksing clipping business and also perk & burden "pages"
            //On.Menu.UnlockDialog.ctor += UnlockDialog_ctor;
            //On.Menu.UnlockDialog.Singal += UnlockDialog_Singal;
            //IL.Menu.UnlockDialog.ctor += UnlockDialog_ctorIL;
            //On.Menu.UnlockDialog.GrafUpdate += UnlockDialog_GrafUpdate;
            //On.Menu.UnlockDialog.Update += UnlockDialog_Update;

            //Adding custom quest icons
            On.Menu.ProgressionPage.ctor += ProgressionPage_ctor;
        }

        public static void ProgressionPage_ctor(On.Menu.ProgressionPage.orig_ctor orig, ProgressionPage self, Menu.Menu menu, MenuObject owner, Vector2 pos)
        {
            orig.Invoke(self, menu, owner, pos);

            // Pretty and hardcoded
            int s = self.questButtons.Length;
            Array.Resize(ref self.questButtons, s + (ModManager.MSC ? 6 : 5));
            Vector2 size = new(50f, 50f);
            if (ModManager.MSC)
            {
                self.questButtons[s] = new QuestButton(menu, self, "", "qst1crippled", new Vector2(180f, 240f), size, "qst1crippled");
                self.questButtons[s].tick.SetElementByName("Symbol_Lost");
                self.questButtons[s + 1] = new QuestButton(menu, self, "", "qst2confused", new Vector2(1140f, 240f), size, "qst2confused");
                self.questButtons[s + 1].tick.SetElementByName("Symbol_Confused");
                self.questButtons[s + 2] = new QuestButton(menu, self, "", "qst3marked", new Vector2(180f, 180f), size, "qst3marked");
                self.questButtons[s + 2].tick.SetElementByName("Symbol_Spikes");
                self.questButtons[s + 3] = new QuestButton(menu, self, "", "qst4volatile", new Vector2(1140f, 180f), size, "qst4volatile");
                self.questButtons[s + 3].tick.SetElementByName("Symbol_Volatile");
                self.questButtons[s + 4] = new QuestButton(menu, self, "", "qst5silver", new Vector2(180f, 120f), size, "qst5silver");
                self.questButtons[s + 4].tick.SetElementByName("Symbol_Silver");
                self.questButtons[s + 5] = new QuestButton(menu, self, "", "qst6gold", new Vector2(1140f, 120f), size, "qst6gold");
                self.questButtons[s + 5].tick.SetElementByName("Symbol_Golden");
            }
            else
            {
                // We dont show the silver quest here, cause theres no Pursued
                self.questButtons[s] = new QuestButton(menu, self, "", "qst1crippled", new Vector2(180f, 255f), size, "qst1crippled");
                self.questButtons[s].tick.SetElementByName("Symbol_Lost");
                self.questButtons[s + 1] = new QuestButton(menu, self, "", "qst2confused", new Vector2(1140f, 255f), size, "qst2confused");
                self.questButtons[s + 1].tick.SetElementByName("Symbol_Confused");
                self.questButtons[s + 2] = new QuestButton(menu, self, "", "qst3marked", new Vector2(180f, 195f), size, "qst3marked");
                self.questButtons[s + 2].tick.SetElementByName("Symbol_Spikes");
                self.questButtons[s + 3] = new QuestButton(menu, self, "", "qst4volatile", new Vector2(1140f, 195f), size, "qst4volatile");
                self.questButtons[s + 3].tick.SetElementByName("Symbol_Volatile");
                self.questButtons[s + 4] = new QuestButton(menu, self, "", "qst6gold", new Vector2(660f, 105f), size, "qst6gold");
                self.questButtons[s + 4].tick.SetElementByName("Symbol_Golden");
            }

            for (int i = 0; i < (ModManager.MSC ? 6 : 5); i++)
            {
                if (ExpeditionData.completedQuests.Contains(self.questButtons[s + i].questKey))
                {
                    self.questButtons[s + i].tick.SetElementByName("tick");
                }
                self.questButtons[s + i].tick.alpha = 1f;
                self.subObjects.Add(self.questButtons[s + i]);
            }
        }

        //Call the OnStart function of all custom perks in the correct place
        public static void Room_Loaded(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdstr("unl-lantern")
                ))
            {
                c.Index--;
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc, 74);
                c.Emit(OpCodes.Ldloc, 77);
                c.EmitDelegate<Action<Room, WorldCoordinate, string>>((room, pos, id) =>
                {
                    if (CustomPerks.RegisteredPerks.Count > 0)
                    {
                        EECustomPerk drPerky = CustomPerks.RegisteredPerks.FirstOrDefault(
                            p => p is EECustomPerk x &&
                            x.ID == id &&
                            x.PerkType == EECustomPerk.CustomPerkType.OnStart &&
                            (x.StartItem != null || x.StartCreature != null) &&
                            x.StartObjectCount > 0) as EECustomPerk;

                        if (drPerky != null)
                        {
                            for (int i = 0; i < drPerky.StartObjectCount; i++)
                            {
                                drPerky.OnStart(room, pos);
                            }
                        }
                    }
                });
            }
            else Plugin.logger.LogError("Uh oh Room.Loaded IL code doesnt work (the probability of this error happening is like non existant actually, i guess only really when someone modifies the Room.Loaded method to have the same IL things but like why would you reference unl-lantern so yeah this will never be shown and like god damn congratulations if you ended up seeing this in the logs: " + il + " )");
        }

        //Call the OnKill function when a kill by a player happens
        public static void SocialEventRecognizer_Killing(On.SocialEventRecognizer.orig_Killing orig, SocialEventRecognizer self, Creature killer, Creature victim)
        {
            orig.Invoke(self, killer, victim);
            if (ModManager.Expedition && self.room.game.rainWorld.ExpeditionMode && killer is Player player && CustomPerks.RegisteredPerks.Count > 0)
            {
                for (int i = 0; i < ExpeditionGame.activeUnlocks.Count; i++)
                {
                    EECustomPerk perk = CustomPerks.RegisteredPerks.FirstOrDefault(
                        p => p is EECustomPerk x &&
                        x.ID == ExpeditionGame.activeUnlocks[i] &&
                        x.PerkType == EECustomPerk.CustomPerkType.OnKill) as EECustomPerk;

                    perk?.OnKill(self, player, victim);
                }
            }
        }

        //Call the OnHit of all burdens that use it
        public static void SocialEventRecognizer_WeaponAttack(On.SocialEventRecognizer.orig_WeaponAttack orig, SocialEventRecognizer self, PhysicalObject weapon, Creature thrower, Creature victim, bool hit)
        {
            orig.Invoke(self, weapon, thrower, victim, hit);
            if (ModManager.Expedition && self.room.game.rainWorld.ExpeditionMode && thrower is Player player && CustomPerks.RegisteredPerks.Count > 0)
            {
                for (int i = 0; i < ExpeditionGame.activeUnlocks.Count; i++)
                {
                    EECustomPerk perk = CustomPerks.RegisteredPerks.FirstOrDefault(
                        p => p is EECustomPerk x &&
                         x.ID == ExpeditionGame.activeUnlocks[i] &&
                        x.PerkType == EECustomPerk.CustomPerkType.OnAttack) as EECustomPerk;

                    perk?.OnAttack(self, weapon, player, victim, hit);
                }
            }
        }

        //Loading wanted (always active) custom perks 
        public static void ExpeditionCoreFile_FromString(On.Expedition.ExpeditionCoreFile.orig_FromString orig, ExpeditionCoreFile self, string saveString)
        {
            orig.Invoke(self, saveString);
        
            //foreach (EECustomPerk perk in customPerks)
            //{
            //    if (perk.UnlockedByDefault && !ExpeditionData.unlockables.Contains(perk.ID))
            //    {
            //        ExpeditionData.unlockables.Add(perk.ID);
            //    }
            //}
        
            //foreach (EECustomBurden burr in customBurdens)
            //{
            //    if (burr.AlwaysUnlocked && !ExpeditionData.unlockables.Contains(burr.ID))
            //    {
            //        ExpeditionData.unlockables.Add(burr.ID);
            //    }
            //}
        
            if (ExpeditionProgression.perkGroups.Count > 0)
            {
                //This just like fixes bs dont worry about it
                List<string> allContent = [];
                foreach (var kvp in ExpeditionProgression.perkGroups)
                {
                    allContent.AddRange(ExpeditionProgression.perkGroups[kvp.Key]);
                }
                foreach (var kvp in ExpeditionProgression.burdenGroups)
                {
                    allContent.AddRange(ExpeditionProgression.burdenGroups[kvp.Key]);
                }
                //List<string> allContent = [.. ExpeditionProgression.perkGroups["expedition"],
                //    .. ExpeditionProgression.perkGroups["expeditionenhanced"],
                //    .. ExpeditionProgression.burdenGroups["expedition"],
                //    .. ExpeditionProgression.burdenGroups["expeditionenhanced"]];
                //
                //if (ModManager.MSC)
                //{
                //    allContent.AddRange(ExpeditionProgression.perkGroups["moreslugcats"]);
                //    allContent.AddRange(ExpeditionProgression.burdenGroups["moreslugcats"]);
                //}
        
                foreach (KeyValuePair<SlugcatStats.Name, List<string>> keyValuePair2 in ExpeditionGame.allUnlocks)
                {
                balls:
                    foreach (var lue in keyValuePair2.Value)
                    {
                        if ((lue.StartsWith("unl") || lue.StartsWith("bur")) && !allContent.Contains(lue))
                        {
                            ExpeditionGame.allUnlocks[keyValuePair2.Key].Remove(lue);
                            Plugin.logger.LogMessage("MISSING UNLOCK, REMOVING " + lue + " FROM " + keyValuePair2.Key);
                            goto balls;
                        }
                    }
                }
            }
        }

        //Adding custom perks to expedition
       //public static void ExpeditionProgression_SetupPerkGroups(On.Expedition.ExpeditionProgression.orig_SetupPerkGroups orig)
       //{
       //    orig.Invoke();
       //
       //    if (customPerks.Count > 0)
       //    {
       //        List<string> perks = new List<string>();
       //        foreach (EECustomPerk perk in customPerks) perks.Add(perk.ID);
       //
       //        ExpeditionProgression.perkGroups.Add("expeditionenhanced", perks);
       //    }
       //}

        //public static void ExpeditionProgression_CountUnlockables(On.Expedition.ExpeditionProgression.orig_CountUnlockables orig)
        //{
        //    orig.Invoke();
        //
        //    //ExpeditionProgression.totalPerks += customPerks.Count;
        //    ExpeditionProgression.totalBurdens += customBurdens.Count;
        //}

        //public static string ExpeditionProgression_UnlockSprite(On.Expedition.ExpeditionProgression.orig_UnlockSprite orig, string key, bool alwaysShow)
        //{
        //    if (customPerks.Count > 0 && ExpeditionData.unlockables.Contains(key))
        //    {
        //        foreach (var perk in customPerks)
        //        {
        //            if (key == perk.ID) return perk.SpriteName;
        //        }
        //    }
        //
        //    return orig.Invoke(key, alwaysShow);
        //}

        //public static string ExpeditionProgression_UnlockName(On.Expedition.ExpeditionProgression.orig_UnlockName orig, string key)
        //{
        //    if (customPerks.Count > 0)
        //    {
        //        foreach (var perk in customPerks)
        //        {
        //            if (key == perk.ID) return perk.DisplayName;
        //        }
        //    }
        //
        //    return orig.Invoke(key);
        //}
        //
        //public static string ExpeditionProgression_UnlockDescription(On.Expedition.ExpeditionProgression.orig_UnlockDescription orig, string key)
        //{
        //    if (customPerks.Count > 0)
        //    {
        //        foreach (var perk in customPerks)
        //        {
        //            if (key == perk.ID) return perk.Description;
        //        }
        //    }
        //
        //    return orig.Invoke(key);
        //}
        //
        //public static Color ExpeditionProgression_UnlockColor(On.Expedition.ExpeditionProgression.orig_UnlockColor orig, string key)
        //{
        //    if (customPerks.Count > 0)
        //    {
        //        foreach (var perk in customPerks)
        //        {
        //            if (key == perk.ID) return perk.Color;
        //        }
        //    }
        //
        //    return orig.Invoke(key);
        //}
        //
        //public static string ExpeditionManualDialog_PerkManualDescription(On.Menu.ExpeditionManualDialog.orig_PerkManualDescription orig, ExpeditionManualDialog self, string key)
        //{
        //    if (customPerks.Count > 0)
        //    {
        //        foreach (var perk in customPerks)
        //        {
        //            if (key == perk.ID && ExpeditionData.unlockables.Contains(key)) return perk.ManualDescription;
        //        }
        //    }
        //
        //    return orig.Invoke(self, key);
        //}

        //Adding custom burdens to expedition
        //public static void ExpeditionProgression_SetupBurdenGroups(On.Expedition.ExpeditionProgression.orig_SetupBurdenGroups orig)
        //{
        //
        //    orig.Invoke();
        //
        //    if (customBurdens.Count > 0)
        //    {
        //        List<string> burs = new List<string>();
        //        foreach (EECustomBurden aaronburr in customBurdens) burs.Add(aaronburr.ID);
        //
        //        ExpeditionProgression.burdenGroups.Add("expeditionenhanced", burs);
        //    }
        //}
        //
        //public static float ExpeditionProgression_BurdenScoreMultiplier(On.Expedition.ExpeditionProgression.orig_BurdenScoreMultiplier orig, string key)
        //{
        //    if (customBurdens.Count > 0)
        //    {
        //        foreach (var aaronburr in customBurdens)
        //        {
        //            if (key == aaronburr.ID) return aaronburr.ScoreMultiplier;
        //        }
        //    }
        //
        //    return orig.Invoke(key);
        //}
        //
        //public static string ExpeditionProgression_BurdenName(On.Expedition.ExpeditionProgression.orig_BurdenName orig, string key)
        //{
        //    if (customBurdens.Count > 0)
        //    {
        //        foreach (var aaronburr in customBurdens)
        //        {
        //            if (key == aaronburr.ID) return aaronburr.Name;
        //        }
        //    }
        //
        //    return orig.Invoke(key);
        //}
        //
        //public static Color ExpeditionProgression_BurdenMenuColor(On.Expedition.ExpeditionProgression.orig_BurdenMenuColor orig, string key)
        //{
        //    if (customBurdens.Count > 0)
        //    {
        //        foreach (var aaronburr in customBurdens)
        //        {
        //            if (key == aaronburr.ID) return aaronburr.Color;
        //        }
        //    }
        //
        //    return orig.Invoke(key);
        //}
        //
        //public static string ExpeditionProgression_BurdenManualDescription(On.Expedition.ExpeditionProgression.orig_BurdenManualDescription orig, string key)
        //{
        //    if (customBurdens.Count > 0)
        //    {
        //        foreach (var aaronburr in customBurdens)
        //        {
        //            if (key == aaronburr.ID && ExpeditionData.unlockables.Contains(key)) return aaronburr.ManualDescription;
        //        }
        //    }
        //
        //    return orig.Invoke(key);
        //}
        //
        //public static void UnlockDialog_UpdateBurdens(On.Menu.UnlockDialog.orig_UpdateBurdens orig, UnlockDialog self)
        //{
        //    orig.Invoke(self);
        //
        //    if (customBurdens.Count > 0)
        //    {
        //        foreach (var aaronburr in customBurdens)
        //        {
        //            var grass = self.pages[0].subObjects.FirstOrDefault(x => x is BigSimpleButton b && b.signalText == aaronburr.ID) as BigSimpleButton;
        //            if (grass != null)
        //            {
        //                if (ExpeditionGame.activeUnlocks.Contains(aaronburr.ID))
        //                {
        //                    Vector3 vector = Custom.RGB2HSL(ExpeditionProgression.BurdenMenuColor(aaronburr.ID));
        //                    grass.labelColor = new HSLColor(vector.x, vector.y, vector.z);
        //                } 
        //                else
        //                {
        //                    grass.labelColor = new HSLColor(1f, 0f, 0.35f);
        //                }
        //            }
        //        }
        //    }
        //}
        //
        //public static void UnlockDialog_SetUpBurdenDescriptions(On.Menu.UnlockDialog.orig_SetUpBurdenDescriptions orig, UnlockDialog self)
        //{
        //
        //    orig.Invoke(self);
        //
        //    if (customBurdens.Count > 0)
        //    {
        //        foreach (var aaronburr in customBurdens)
        //        {
        //            self.burdenNames.Add(ExpeditionProgression.BurdenName(aaronburr.ID) + " +" + ExpeditionProgression.BurdenScoreMultiplier(aaronburr.ID).ToString() + "%");
        //            self.burdenDescriptions.Add(ExpeditionData.unlockables.Contains(aaronburr.ID) ? ExpeditionProgression.BurdenManualDescription(aaronburr.ID).WrapText(false, 600f, false) : "? ? ?");
        //        }
        //    }
        //}

        public static void ExpeditionGame_SetUpBurdenTrackers(On.Expedition.ExpeditionGame.orig_SetUpBurdenTrackers orig, RainWorldGame game)
        {
            orig.Invoke(game);

            if (game != null && ActiveContent("bur-marked")) ExpeditionGame.burdenTrackers.Add(new ExampleBurdenHooks.SpikeEventTracker(game));
        }

        //Below is logic for adding new pages to the manual, also rewriting the way base expedition does this a bit
        //public static void ExpeditionManualDialog_ctor(On.Menu.ExpeditionManualDialog.orig_ctor orig, ExpeditionManualDialog self, ProcessManager manager, Dictionary<string, int> topics)
        //{
        //    int extraPPageNum = 0;
        //    for (int i = 0; i < customPerks.Count; i++)
        //    {
        //        if (i % 4 == 0) extraPPageNum++;
        //    }
        //    int extraBPageNum = 0;
        //    for (int i = 0; i < customBurdens.Count; i++)
        //    {
        //        if (i % 4 == 0) extraBPageNum++;
        //    }
        //
        //    if (topics.ContainsKey("perks"))
        //    {
        //        topics["perks"] += extraPPageNum;
        //        if (maxPPages == -1) maxPPages = topics["perks"];
        //        if (topics["perks"] > maxPPages) topics["perks"] = maxPPages;
        //    }
        //    if (topics.ContainsKey("burdens"))
        //    {
        //        topics["burdens"] += extraBPageNum;
        //        if (maxBPages == -1) maxBPages = topics["burdens"];
        //        if (topics["burdens"] > maxBPages) topics["burdens"] = maxBPages;
        //    }
        //
        //    orig.Invoke(self, manager, topics);
        //}
        //
        ////Coding at 1 am is an experience for sure
        //public static void ExpeditionManualDialog_GetManualPage(On.Menu.ExpeditionManualDialog.orig_GetManualPage orig, ExpeditionManualDialog self, string topic, int pageNumber)
        //{
        //    orig.Invoke(self, topic, pageNumber);
        //
        //    if (customPerks.Count > 0 && topic == "perks")
        //    {
        //        if (self.currentTopicPage != null)
        //        {
        //            self.currentTopicPage.RemoveSprites();
        //            self.pages[1].RemoveSubObject(self.currentTopicPage);
        //        }
        //
        //        int num = ModManager.MSC ? 4 : 2;
        //        int extraPageNum = self.topics["perks"] - num;
        //
        //        if (pageNumber == 0) self.currentTopicPage = new PerkManualPage(self, self.pages[1]);
        //        else if (pageNumber == 1) self.currentTopicPage = new PerkManualPageTwo(self, self.pages[1]);
        //        if (ModManager.MSC)
        //        {
        //            if (pageNumber == 2) self.currentTopicPage = new PerkManualPageThree(self, self.pages[1]);
        //            else if (pageNumber == 3) self.currentTopicPage = new PerkManualPageFour(self, self.pages[1]);
        //        }
        //
        //        //I SWEAR this makes sense just dont think about it
        //        for (int i = 0; i < extraPageNum; i++)
        //        {
        //            if (pageNumber == i + num) 
        //            {
        //                List<string> perks = new();
        //                for (int j = i * 4; j < Mathf.Min((i + 1) * 4, customPerks.Count); j++)
        //                {
        //                    perks.Add(customPerks[j].ID);
        //                }
        //
        //                self.currentTopicPage = new CustomPerkManualPage(self, self.pages[1], perks);
        //            }
        //        }
        //
        //        self.pages[1].subObjects.Add(self.currentTopicPage);
        //    }
        //
        //    if (customBurdens.Count > 0 && topic == "burdens")
        //    {
        //        if (self.currentTopicPage != null)
        //        {
        //            self.currentTopicPage.RemoveSprites();
        //            self.pages[1].RemoveSubObject(self.currentTopicPage);
        //        }
        //
        //        int num = 1;
        //        int extraPageNum = self.topics["burdens"] - num;
        //
        //        if (pageNumber == 0) self.currentTopicPage = new BurdenManualPage(self, self.pages[1]);
        //
        //        for (int i = 0; i < extraPageNum; i++)
        //        {
        //            if (pageNumber == i + num)
        //            {
        //                List<string> burdens = new();
        //                for (int j = i * 4; j < Mathf.Min((i + 1) * 4, customBurdens.Count); j++)
        //                {
        //                    burdens.Add(customBurdens[j].ID);
        //                }
        //
        //                self.currentTopicPage = new CustomBurdenManualPage(self, self.pages[1], burdens);
        //            }
        //        }
        //
        //        self.pages[1].subObjects.Add(self.currentTopicPage);
        //    }
        //}

        //Mostly copied from base game
        //public class CustomPerkManualPage : ManualPage
        //{
        //    public FSprite headingSeparator;
        //    public FSprite[] sprites;
        //
        //    public CustomPerkManualPage(Menu.Menu menu, MenuObject owner, List<string> perks) : base(menu, owner)
        //    {
        //        this.topicName = "CUSTOM PERKS";
        //        MenuLabel menuLabel = new MenuLabel(menu, owner, this.topicName, new Vector2(15f + (menu as ExpeditionManualDialog).contentOffX, 475f), default(Vector2), true, null);
        //        menuLabel.label.alignment = FLabelAlignment.Left;
        //        this.subObjects.Add(menuLabel);
        //        this.headingSeparator = new FSprite("pixel", true);
        //        this.headingSeparator.scaleX = 594f;
        //        this.headingSeparator.scaleY = 2f;
        //        this.headingSeparator.color = new Color(0.7f, 0.7f, 0.7f);
        //        this.Container.AddChild(this.headingSeparator);
        //        this.sprites = new FSprite[perks.Count];
        //        for (int i = 0; i < this.sprites.Length; i++)
        //        {
        //            string key = perks[i];
        //            this.sprites[i] = new FSprite(ExpeditionProgression.UnlockSprite(key, true), true);
        //            this.sprites[i].color = Color.Lerp(ExpeditionProgression.UnlockColor(key), new Color(0.8f, 0.8f, 0.8f), 0.25f);
        //            this.Container.AddChild(this.sprites[i]);
        //            MenuLabel menuLabel2 = new MenuLabel(menu, owner, ExpeditionProgression.UnlockName(key), new Vector2(120f + (menu as ExpeditionManualDialog).contentOffX, 410f - (float)(120 * i)), default(Vector2), true, null);
        //            menuLabel2.label.color = this.sprites[i].color;
        //            menuLabel2.label.alignment = FLabelAlignment.Left;
        //            this.subObjects.Add(menuLabel2);
        //            string[] array = Regex.Split((menu as ExpeditionManualDialog).PerkManualDescription(key).WrapText(false, 450f + (menu as ExpeditionManualDialog).wrapTextMargin, false), "\n");
        //            for (int j = 0; j < array.Length; j++)
        //            {
        //                MenuLabel menuLabel3 = new MenuLabel(menu, owner, array[j], new Vector2(120f + (menu as ExpeditionManualDialog).contentOffX, 395f - 120f * (float)i - 15f * (float)j), default(Vector2), false, null);
        //                menuLabel3.label.SetAnchor(0f, 1f);
        //                menuLabel3.label.color = new Color(0.7f, 0.7f, 0.7f);
        //                this.subObjects.Add(menuLabel3);
        //            }
        //        }
        //    }
        //
        //    public override void GrafUpdate(float timeStacker)
        //    {
        //        base.GrafUpdate(timeStacker);
        //        this.headingSeparator.x = base.page.pos.x + 295f + (this.menu as ExpeditionManualDialog).contentOffX;
        //        this.headingSeparator.y = base.page.pos.y + 450f;
        //        for (int i = 0; i < this.sprites.Length; i++)
        //        {
        //            this.sprites[i].x = base.page.pos.x + 60f + (this.menu as ExpeditionManualDialog).contentOffX;
        //            this.sprites[i].y = base.page.pos.y + 390f - 120f * (float)i;
        //        }
        //    }
        //
        //    public override void RemoveSprites()
        //    {
        //        base.RemoveSprites();
        //        this.headingSeparator.RemoveFromContainer();
        //        for (int i = 0; i < this.sprites.Length; i++)
        //        {
        //            this.sprites[i].RemoveFromContainer();
        //        }
        //    }
        //}

        //public class CustomBurdenManualPage : ManualPage
        //{
        //    public FSprite headingSeparator;
        //
        //    public CustomBurdenManualPage(Menu.Menu menu, MenuObject owner, List<string> burdens) : base(menu, owner)
        //    {
        //        this.topicName = menu.Translate((menu as ExpeditionManualDialog).TopicName((menu as ExpeditionManualDialog).currentTopic));
        //        MenuLabel menuLabel = new MenuLabel(menu, owner, this.topicName, new Vector2(15f + (menu as ExpeditionManualDialog).contentOffX, 475f), default(Vector2), true, null);
        //        menuLabel.label.alignment = FLabelAlignment.Left;
        //        this.subObjects.Add(menuLabel);
        //        this.headingSeparator = new FSprite("pixel", true);
        //        this.headingSeparator.scaleX = 594f;
        //        this.headingSeparator.scaleY = 2f;
        //        this.headingSeparator.color = new Color(0.7f, 0.7f, 0.7f);
        //        this.Container.AddChild(this.headingSeparator);
        //        for (int i = 0; i < burdens.Count; i++)
        //        {
        //            string key = burdens[i];
        //            MenuLabel name = new MenuLabel(menu, owner, ExpeditionProgression.BurdenName(key) + " +" + ExpeditionProgression.BurdenScoreMultiplier(key).ToString() + "%", new Vector2(35f + (menu as ExpeditionManualDialog).contentOffX, 410f - (float)(120 * i)), default, true, null);
        //            name.label.alignment = FLabelAlignment.Left;
        //            name.label.color = ExpeditionProgression.BurdenMenuColor(key);
        //            this.subObjects.Add(name);
        //
        //            string[] array2 = Regex.Split(ExpeditionProgression.BurdenManualDescription(key).WrapText(false, 500f + (menu as ExpeditionManualDialog).wrapTextMargin, false), "\n");
        //            for (int j = 0; j < array2.Length; j++)
        //            {
        //                MenuLabel menuLabel4 = new MenuLabel(menu, owner, array2[j], new Vector2(35f + (menu as ExpeditionManualDialog).contentOffX, name.pos.y - 15f - 15f * (float)j), default(Vector2), false, null);
        //                menuLabel4.label.SetAnchor(0f, 1f);
        //                menuLabel4.label.color = new Color(0.7f, 0.7f, 0.7f);
        //                this.subObjects.Add(menuLabel4);
        //            }
        //        }
        //    }
        //    
        //    public override void GrafUpdate(float timeStacker)
        //    {
        //        base.GrafUpdate(timeStacker);
        //        this.headingSeparator.x = base.page.pos.x + 295f + (this.menu as ExpeditionManualDialog).contentOffX;
        //        this.headingSeparator.y = base.page.pos.y + 450f;
        //    }
        //
        //    public override void RemoveSprites()
        //    {
        //        base.RemoveSprites();
        //        this.headingSeparator.RemoveFromContainer();
        //    }
        //}

        //Initializing the perk pages and adding page control buttons
       //public static void UnlockDialog_ctor(On.Menu.UnlockDialog.orig_ctor orig, UnlockDialog self, ProcessManager manager, ChallengeSelectPage owner)
       //{
       //    orig.Invoke(self, manager, owner);
       //    maxPerkPages = 0;
       //    maxBurdenPages = 0;
       //    for (int i = ModManager.MSC ? 16 : 8; i < self.perkButtons.Count; i++)
       //    {
       //        if (i % 8 == 0) maxPerkPages++;
       //    }
       //    for (int i = 0; i < (ModManager.MSC ? 4 : 3) + customBurdens.Count; i++)
       //    {
       //        if (i % 4 == 0) maxBurdenPages++;
       //    }
       //
       //    if (maxPerkPages > 1)
       //    {
       //        SymbolButton perkLeft = new SymbolButton(self, self.pages[0], "Big_Menu_Arrow", "CUSTOMPERKPAGE_LEFT", new Vector2(403f, ModManager.MSC ? 407f : 407f + 75f));
       //        perkLeft.symbolSprite.rotation = 270f;
       //        perkLeft.size = new Vector2(30f, 30f);
       //        perkLeft.roundedRect.size = perkLeft.size;
       //        perkLeft.symbolSprite.scale = 0.5f;
       //        self.pages[0].subObjects.Add(perkLeft);
       //
       //        SymbolButton perkRight = new SymbolButton(self, self.pages[0], "Big_Menu_Arrow", "CUSTOMPERKPAGE_RIGHT", new Vector2(937f, ModManager.MSC ? 407f : 407f + 75f));
       //        perkRight.symbolSprite.rotation = 90f;
       //        perkRight.size = new Vector2(30f, 30f);
       //        perkRight.roundedRect.size = perkRight.size;
       //        perkRight.symbolSprite.scale = 0.5f;
       //        self.pages[0].subObjects.Add(perkRight);
       //    }
       //
       //    if (maxBurdenPages > 1)
       //    {
       //        SymbolButton burdenLeft = new SymbolButton(self, self.pages[0], "Big_Menu_Arrow", "CUSTOMBURDENPAGE_LEFT", new Vector2(300f, 300f));
       //        burdenLeft.symbolSprite.rotation = 270f;
       //        burdenLeft.size = new Vector2(40f, 40f);
       //        burdenLeft.roundedRect.size = burdenLeft.size;
       //        burdenLeft.symbolSprite.scale = 0.8f;
       //        self.pages[0].subObjects.Add(burdenLeft);
       //
       //        SymbolButton burdenRight = new SymbolButton(self, self.pages[0], "Big_Menu_Arrow", "CUSTOMBURDENPAGE_RIGHT", new Vector2(1030f, 300f));
       //        burdenRight.symbolSprite.rotation = 90f;
       //        burdenRight.size = new Vector2(40f, 40f);
       //        burdenRight.roundedRect.size = burdenRight.size;
       //        burdenRight.symbolSprite.scale = 0.8f;
       //        self.pages[0].subObjects.Add(burdenRight);
       //    }
       //
       //    ConstructPerkPage(self);
       //    ConstructBurdenPage(self);
       //}
       //
       ////Rearranging the custom perks to be a max of 8 at any moment
       //public static void ConstructPerkPage(UnlockDialog self)
       //{
       //    int emescee = ModManager.MSC ? 16 : 8;
       //    for (int i = emescee; i < self.perkButtons.Count; i++) //Start from the perks this mod adds
       //    {
       //        self.perkButtons[i].pos = new(10000f, 10000f);
       //        self.perkButtons[i].lastPos = self.perkButtons[i].pos;
       //    }
       //
       //    for (int i = emescee + currentPerkPage * 8; i < Mathf.Min(emescee + currentPerkPage * 8 + 8, self.perkButtons.Count); i++)
       //    {
       //        self.perkButtons[i].pos = new(450f + 60f * (i - emescee - currentPerkPage * 8), 610f - 75f * (ModManager.MSC ?  2 : 1) - 63f);
       //        self.perkButtons[i].lastPos = self.perkButtons[i].pos;
       //    }
       //}
       //
       ////Rearranging the custom burdens to be a max of 4 at any moment
       //public static void ConstructBurdenPage(UnlockDialog self)
       //{
       //    int emescee = ModManager.MSC ? 4 : 3;
       //    for (int i = 0; i < emescee + customBurdens.Count; i++)
       //    {
       //        var buton = GetBurdenButton(self, i, emescee);
       //        if (buton != null) buton.pos = new Vector2(10000f, 10000f);
       //        buton.lastPos = buton.pos;
       //    }
       //    
       //    for (int i = currentBurdenPage * 4; i < Mathf.Min(currentBurdenPage * 4 + 4, emescee + customBurdens.Count); i++)
       //    {
       //        var buton = GetBurdenButton(self, i, emescee);
       //        if (buton != null) buton.pos = new(355f + 170f * (i - currentBurdenPage * 4), 295f);
       //        buton.lastPos = buton.pos;
       //    }
       //}
       //
       //public static BigSimpleButton GetBurdenButton(UnlockDialog self, int i, int emescee)
       //{
       //    BigSimpleButton burdenButone = null;
       //    if (i == 0) burdenButone = self.blindedBurden;
       //    else if (i == 1) burdenButone = self.doomedBurden;
       //    else if (i == 2) burdenButone = self.huntedBurden;
       //    else if (ModManager.MSC && i == 3) burdenButone = self.pursuedBurden;
       //    else
       //    {
       //        burdenButone = self.pages[0].subObjects.FirstOrDefault(x => x is BigSimpleButton b && b.signalText == customBurdens[i - emescee].ID) as BigSimpleButton;
       //    }
       //    return burdenButone;
       //}
       //
       //public static void UnlockDialog_Singal(On.Menu.UnlockDialog.orig_Singal orig, UnlockDialog self, MenuObject sender, string message)
       //{
       //    orig.Invoke(self, sender, message);
       //
       //    if (message == "CUSTOMPERKPAGE_LEFT")
       //    {
       //        currentPerkPage--;
       //        if (currentPerkPage < 0) currentPerkPage = maxPerkPages - 1;
       //        ConstructPerkPage(self);
       //        self.PlaySound(SoundID.MENU_Checkbox_Check);
       //    }
       //
       //    if (message == "CUSTOMPERKPAGE_RIGHT")
       //    {
       //        currentPerkPage++;
       //        if (currentPerkPage > maxPerkPages - 1) currentPerkPage = 0;
       //        ConstructPerkPage(self);
       //        self.PlaySound(SoundID.MENU_Checkbox_Check);
       //    }
       //
       //    if (message == "CUSTOMBURDENPAGE_LEFT")
       //    {
       //        currentBurdenPage--;
       //        if (currentBurdenPage < 0) currentBurdenPage = maxBurdenPages - 1;
       //        ConstructBurdenPage(self);
       //        self.PlaySound(SoundID.MENU_Checkbox_Check);
       //    }
       //
       //    if (message == "CUSTOMBURDENPAGE_RIGHT")
       //    {
       //        currentBurdenPage++;
       //        if (currentBurdenPage > maxBurdenPages - 1) currentBurdenPage = 0;
       //        ConstructBurdenPage(self);
       //        self.PlaySound(SoundID.MENU_Checkbox_Check);
       //    }
       //}
       //
       ////Adding a background and border to the perk and burden text so it never obstructs stuff
       //public static void UnlockDialog_ctorIL(ILContext il)
       //{
       //    ILCursor c = new(il);
       //
       //    if (c.TryGotoNext(x => x.MatchStfld("Menu.UnlockDialog", "perkNameLabel")))
       //    {
       //        c.Index--;
       //        c.Emit(OpCodes.Ldarg_0);
       //        c.EmitDelegate<Action<UnlockDialog>>((self) =>
       //        {
       //            self.pages[0].subObjects.Add(new Backgroinde(self, self.pages[0], default));
       //        });
       //    }
       //    else Plugin.logger.LogError("Oopsie unlock dialog Il hook total utter failure " + il);
       //
       //    ILCursor g = new(il);
       //
       //    if (g.TryGotoNext(x => x.MatchCallOrCallvirt<Menu.UnlockDialog>("UpdateBurdens")))
       //    {
       //        g.Emit(OpCodes.Ldarg_0);
       //        g.EmitDelegate<Action<UnlockDialog>>((self) =>
       //        {
       //            for (int i = 0; i < customBurdens.Count; i++)
       //            {
       //                var buton = new BigSimpleButton(self, self.pages[0], ExpeditionProgression.BurdenName(customBurdens[i].ID), customBurdens[i].ID, new Vector2(680f - (ModManager.MSC ? 325f : 248f), 310f) + new Vector2(170 * (i + 4f), -15f), new Vector2(150f, 50f), FLabelAlignment.Center, true);
       //                buton.buttonBehav.greyedOut = !ExpeditionData.unlockables.Contains(customBurdens[i].ID);
       //                self.pages[0].subObjects.Add(buton);
       //            }
       //        });
       //    }
       //    else Plugin.logger.LogError("Oopsie unlock dialog Il hook TWO total utter failure " + il);
       //}
       //
       ////Dynamic positioning for the background box and text
       //public static void UnlockDialog_GrafUpdate(On.Menu.UnlockDialog.orig_GrafUpdate orig, UnlockDialog self, float timeStacker)
       //{
       //    orig.Invoke(self, timeStacker);
       //
       //    //This is very messy yes.
       //    Backgroinde backgroiun = self.pages[0].subObjects.FirstOrDefault(x => x is Backgroinde) as Backgroinde;
       //    if (backgroiun != null)
       //    {
       //        foreach (FSprite sprite in backgroiun.boxSprites)
       //        {
       //            sprite.isVisible = self.perkNameLabel.text != "";
       //        }
       //        if (self.perkNameLabel.text != "")
       //        {
       //            int grug = burdenMode ? 5 : 0;
       //            self.perkNameLabel.pos = new Vector2(683f + grug, 523.5f); //Middle row default pos
       //            for (int i = 0; i < self.perkButtons.Count; i++)
       //            {
       //                if (self.perkButtons[i].IsMouseOverMe)
       //                {
       //                    self.perkNameLabel.pos -= new Vector2(0, i > 7 && i < 16 ? 75f : 0f);
       //                    self.perkNameLabel.lastPos = self.perkNameLabel.pos;
       //                }
       //            }
       //            self.perkDescLabel.pos = new Vector2(683f + grug, self.perkNameLabel.pos.y - 35f);
       //            self.perkDescLabel.lastPos = self.perkDescLabel.pos;
       //
       //            Vector2 pos = self.perkNameLabel.pos + (burdenMode ? new(-52f, 0f) : new(2f, 0f));
       //            if (burdenMode)
       //            {
       //                backgroiun.boxSprites[0].scaleX = 600f;
       //                backgroiun.boxSprites[1].scaleX = 600f;
       //                backgroiun.boxSprites[2].scaleX = 600f;
       //            } 
       //            else
       //            {
       //                backgroiun.boxSprites[0].scaleX = 500f;
       //                backgroiun.boxSprites[1].scaleX = 500f;
       //                backgroiun.boxSprites[2].scaleX = 500f;
       //            }
       //            float numb = backgroiun.boxSprites[0].scaleX;
       //            float numb2 = burdenMode ? 50f : 0f;
       //            backgroiun.boxSprites[0].SetPosition(pos + new Vector2(-numb / 2f + numb2, -55f));
       //            backgroiun.boxSprites[1].SetPosition(pos + new Vector2(-numb / 2f + numb2, 20f));
       //            backgroiun.boxSprites[2].SetPosition(pos + new Vector2(-numb / 2f + numb2, -55f));
       //            backgroiun.boxSprites[3].SetPosition(pos + new Vector2(-numb / 2f + numb2, -55f));
       //            backgroiun.boxSprites[4].SetPosition(pos + new Vector2(numb / 2f + numb2 - 2f, -55f));
       //        }
       //    }
       //}
       //
       //public static void UnlockDialog_Update(On.Menu.UnlockDialog.orig_Update orig, UnlockDialog self)
       //{
       //    orig.Invoke(self);
       //    if (customBurdens.Count > 0)
       //    {
       //        burdenMode = false;
       //        int gruh = (ModManager.MSC ? 4 : 3);
       //        for (int i = gruh; i < gruh + customBurdens.Count; i++)
       //        {
       //            var grass = self.pages[0].subObjects.FirstOrDefault(x => x is BigSimpleButton b && b.signalText == customBurdens[i - gruh].ID) as BigSimpleButton;
       //            if (grass != null && (grass.Selected || grass.IsMouseOverMe))
       //            {
       //                self.perkNameLabel.text = self.burdenNames[i + (ModManager.MSC ? 0 : 1)];
       //                self.perkDescLabel.text = self.burdenDescriptions[i + (ModManager.MSC ? 0 : 1)];
       //                burdenMode = true;
       //            }
       //        }
       //        if (self.blindedBurden.Selected || self.blindedBurden.IsMouseOverMe ||
       //            self.doomedBurden.Selected || self.doomedBurden.IsMouseOverMe ||
       //            self.huntedBurden.Selected || self.huntedBurden.IsMouseOverMe ||
       //            (self.pursuedBurden != null && self.pursuedBurden.Selected) || (self.pursuedBurden != null && self.pursuedBurden.IsMouseOverMe)) burdenMode = true;
       //    }
       //}

       //public class Backgroinde : PositionedMenuObject
       //{
       //    public FSprite[] boxSprites = new FSprite[5];
       //
       //    public Backgroinde(Menu.Menu menu, MenuObject owner, Vector2 pos) : base(menu, owner, pos)
       //    {
       //        boxSprites = new FSprite[5];
       //        boxSprites[0] = new FSprite("pixel", true)
       //        {
       //            anchorX = 0,
       //            anchorY = 0,
       //            scaleX = 500,
       //            scaleY = 75,
       //            color = new Color(0.01f, 0.01f, 0.01f, 0.75f)
       //        };
       //        boxSprites[1] = new FSprite("pixel", true)
       //        {
       //            anchorX = 0,
       //            anchorY = 0,
       //            scaleX = 500,
       //        };
       //        boxSprites[2] = new FSprite("pixel", true)
       //        {
       //            anchorX = 0,
       //            anchorY = 0,
       //            scaleX = 500,
       //        };
       //        boxSprites[3] = new FSprite("pixel", true)
       //        {
       //            anchorX = 0,
       //            anchorY = 0,
       //            scaleY = 75,
       //        };
       //        boxSprites[4] = new FSprite("pixel", true)
       //        {
       //            anchorX = 0,
       //            anchorY = 0,
       //            scaleY = 75,
       //        };
       //        for (int i = 0; i < boxSprites.Length; i++)
       //        {
       //            Container.AddChild(boxSprites[i]);
       //            if (i > 0)
       //            {
       //                boxSprites[i].scaleX += 1f;
       //                boxSprites[i].scaleY += 1f;
       //                boxSprites[i].color = Color.white;
       //                boxSprites[i].shader = menu.manager.rainWorld.Shaders["MenuText"];
       //            }
       //        }
       //    }
       //
       //    public override void RemoveSprites()
       //    {
       //        base.RemoveSprites();
       //        for (int i = 0; i < boxSprites.Length; i++)
       //        {
       //            boxSprites[i].RemoveFromContainer();
       //        }
       //    }
       //}
    }
}
