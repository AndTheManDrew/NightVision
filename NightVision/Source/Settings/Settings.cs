// Nightvision NightVision Settings.cs
// 
// 03 08 2018
// 
// 16 10 2018

using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace NightVision
{
    public class Settings : ModSettings
    {
        private Tab  _tab;
        public  bool CEDetected = false;


        public Storage Store;

        public SettingsCache Cache;
        

        private readonly List<TabRecord> TabsList = new List<TabRecord>();

        [UsedImplicitly]
        public Settings()
        {
            Store = new Storage();
            Cache = new SettingsCache();
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Store.ExposeSettings();
        }

        public void InitialiseWindow(Rect inRect)
        {
            TabsList.Clear();
            TabsList.Add(
                new TabRecord(
                    "NVGeneralTab".Translate(),
                    delegate
                    {
                        _tab = Tab.General;
                    },
                    () => _tab == Tab.General
                )
            );

            TabsList.Add(
                new TabRecord(
                    "NVCombat".Translate(),
                    delegate
                    {
                        _tab = Tab.Combat;
                    },
                    () => _tab == Tab.Combat
                )
            );

            TabsList.Add(
                new TabRecord(
                    "NVRaces".Translate(),
                    delegate
                    {
                        _tab = Tab.Races;
                    },
                    () => _tab == Tab.Races
                )
            );

            TabsList.Add(
                new TabRecord(
                    "NVApparel".Translate(),
                    delegate
                    {
                        _tab = Tab.Apparel;
                    },
                    () => _tab == Tab.Apparel
                )
            );

            TabsList.Add(
                new TabRecord(
                    "NVHediffs".Translate(),
                    delegate
                    {
                        _tab = Tab.Bionics;
                    },
                    () => _tab == Tab.Bionics
                )
            );

            if (Prefs.DevMode)
            {
                TabsList.Add(
                    new TabRecord(
                        "NVDebugTab".Translate(),
                        delegate
                        {
                            _tab = Tab.Debug;
                        },
                        () => _tab == Tab.Debug
                    )
                );
            }

            Cache.Init();

            inRect.yMin += 32f;
            menuRect = inRect;
            tabRect = inRect.ContractedBy(17f);
            
            
        }

        private Rect lastRect;

        private bool initialised;
        private Rect menuRect;
        private Rect tabRect;
        

        public /*static*/ void DoSettingsWindowContents(Rect inRect)
        {
            if (!initialised || lastRect != inRect)
            {
                lastRect = inRect;
                InitialiseWindow(inRect);
                initialised = true;
            }
            
            Widgets.DrawMenuSection(menuRect);
            TabDrawer.DrawTabs(menuRect, TabsList, 1);

            GUI.BeginGroup(tabRect);
            var   font   = Text.Font;
            var anchor = Text.Anchor;
            Text.Font   = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;

            switch (_tab)
            {
                default:
                    GeneralTab.DrawTab(tabRect);

                    break;
                case Tab.Combat:
                    CombatTab.DrawTab(tabRect);
                    break;
                case Tab.Races:
                    RaceTab.DrawTab(tabRect);

                    break;
                case Tab.Apparel:
                    ApparelTab.DrawTab(tabRect);

                    break;
                case Tab.Bionics:
                    HediffTab.DrawTab(tabRect);

                    break;
                case Tab.Debug:
                    DebugTab.DrawTab(tabRect);

                    break;
            }

            Text.Font   = font;
            Text.Anchor = anchor;
            GUI.EndGroup();
        }

        public void ClearDrawVariables()
        {
            DebugTab.Clear();
            ApparelTab.Clear();
            RaceTab.Clear();
            GeneralTab.Clear();
            CombatTab.Clear();
            initialised = false;
        }
    }
}