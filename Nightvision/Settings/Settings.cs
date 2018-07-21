// Nightvision NightVision Settings.cs
// 
// 21 07 2018
// 
// 21 07 2018

using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace NightVision
    {
        public class Settings : ModSettings
            {
                public static Tab  _tab;
                public static bool CEDetected = false;


                [UsedImplicitly] public static Settings        Instance;
                public static readonly         List<TabRecord> TabsList = new List<TabRecord>();

                [UsedImplicitly]
                public Settings() => Instance = this;


                public override void ExposeData()
                    {
                        base.ExposeData();
                        Storage.ExposeSettings();
                    }

                public static void DoSettingsWindowContents(
                    Rect inRect)
                    {
                        TabsList.Clear();
                        TabsList.Add(new TabRecord("General", delegate { _tab = Tab.General; }, _tab == Tab.General));
                        TabsList.Add(new TabRecord("Races",   delegate { _tab = Tab.Races; },   _tab == Tab.Races));
                        TabsList.Add(new TabRecord("Apparel", delegate { _tab = Tab.Apparel; }, _tab == Tab.Apparel));
                        TabsList.Add(new TabRecord("Bionics", delegate { _tab = Tab.Bionics; }, _tab == Tab.Bionics));

                        if (Prefs.DevMode)
                            {
                                TabsList.Add(new TabRecord("Debug", delegate { _tab = Tab.Debug; }, _tab == Tab.Debug));
                            }

                        SettingsCache.Init();

                        inRect.yMin += 32f;
                        Widgets.DrawMenuSection(inRect);
                        TabDrawer.DrawTabs(inRect, TabsList, 1);

                        inRect = inRect.ContractedBy(17f);
                        GUI.BeginGroup(inRect);
                        GameFont   font   = Text.Font;
                        TextAnchor anchor = Text.Anchor;
                        Text.Font   = GameFont.Small;
                        Text.Anchor = TextAnchor.MiddleCenter;
                        switch (_tab)
                            {
                                default:
                                    DrawSettings.GeneralTab(inRect);
                                    break;
                                case Tab.Races:
                                    DrawSettings.RaceTab(inRect);
                                    break;
                                case Tab.Apparel:
                                    DrawSettings.ApparelTab(inRect);
                                    break;
                                case Tab.Bionics:
                                    DrawSettings.HediffTab(inRect);
                                    break;
                                case Tab.Debug:
                                    DrawSettings.DebugTab(inRect);
                                    break;
                            }

                        Text.Font   = font;
                        Text.Anchor = anchor;
                        GUI.EndGroup();
                    }
            }
    }