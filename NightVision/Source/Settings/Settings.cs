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
        private static Tab  _tab;
        public static bool CEDetected = false;


        [UsedImplicitly]
        public static Settings Instance;

        private static readonly List<TabRecord> TabsList = new List<TabRecord>();

        [UsedImplicitly]
        public Settings() => Settings.Instance = this;


        public override void ExposeData()
        {
            base.ExposeData();
            Storage.ExposeSettings();
        }

        public static void DoSettingsWindowContents(
                        Rect inRect
                    )
        {
            Settings.TabsList.Clear();

            Settings.TabsList.Add(
                                  new TabRecord(
                                                "NVGeneralTab".Translate(),
                                                delegate
                                                {
                                                    Settings._tab = Tab.General;
                                                },
                                                Settings._tab == Tab.General
                                               )
                                 );

            Settings.TabsList.Add(
                                  new TabRecord(
                                                "NVRaces".Translate(),
                                                delegate
                                                {
                                                    Settings._tab = Tab.Races;
                                                },
                                                Settings._tab == Tab.Races
                                               )
                                 );

            Settings.TabsList.Add(
                                  new TabRecord(
                                                "NVApparel".Translate(),
                                                delegate
                                                {
                                                    Settings._tab = Tab.Apparel;
                                                },
                                                Settings._tab == Tab.Apparel
                                               )
                                 );

            Settings.TabsList.Add(
                                  new TabRecord(
                                                "NVHediffs".Translate(),
                                                delegate
                                                {
                                                    Settings._tab = Tab.Bionics;
                                                },
                                                Settings._tab == Tab.Bionics
                                               )
                                 );

            if (Prefs.DevMode)
            {
                Settings.TabsList.Add(
                                      new TabRecord(
                                                    "NVDebugTab".Translate(),
                                                    delegate
                                                    {
                                                        Settings._tab = Tab.Debug;
                                                    },
                                                    Settings._tab == Tab.Debug
                                                   )
                                     );
            }

            SettingsCache.Init();

            inRect.yMin += 32f;
            Widgets.DrawMenuSection(inRect);
            TabDrawer.DrawTabs(inRect, Settings.TabsList, 1);

            inRect = inRect.ContractedBy(17f);
            GUI.BeginGroup(inRect);
            GameFont   font   = Text.Font;
            TextAnchor anchor = Text.Anchor;
            Text.Font   = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;

            switch (Settings._tab)
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