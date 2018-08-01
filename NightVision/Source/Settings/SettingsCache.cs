using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace NightVision
{
    public static class SettingsCache
    {
        [CanBeNull]
        private static List<HediffDef> _allHediffsCache;

        public static bool CacheInited;

        [CanBeNull]
        private static List<ThingDef> _headgearCache;

        public static float? MaxCache;

        public static float? MinCache;
        public static float? NVFullCache;
        public static float? NVZeroCache;
        public static float? PSFullCache;
        public static float? PSZeroCache;

        [NotNull]
        public static List<ThingDef> GetAllHeadgear
        {
            get
            {
                if (SettingsCache._headgearCache == null || SettingsCache._headgearCache.Count == 0)
                {
                    SettingsCache._headgearCache = new List<ThingDef>(Storage.AllEyeCoveringHeadgearDefs);

                    foreach (ThingDef appareldef in Storage.NVApparel.Keys)
                    {
                        int appindex = SettingsCache._headgearCache.IndexOf(appareldef);

                        if (appindex > 0)
                        {
                            SettingsCache._headgearCache.RemoveAt(appindex);
                            SettingsCache._headgearCache.Insert(0, appareldef);
                        }
                    }
                }

                return SettingsCache._headgearCache;
            }
        }

        [NotNull]
        public static List<HediffDef> GetAllHediffs
        {
            get
            {
                if (SettingsCache._allHediffsCache == null || SettingsCache._allHediffsCache.Count == 0)
                {
                    SettingsCache._allHediffsCache = new List<HediffDef>(Storage.AllSightAffectingHediffs);

                    foreach (HediffDef hediffdef in Storage.HediffLightMods.Keys)
                    {
                        int appindex = SettingsCache._allHediffsCache.IndexOf(hediffdef);

                        if (appindex > 0)
                        {
                            SettingsCache._allHediffsCache.RemoveAt(appindex);
                            SettingsCache._allHediffsCache.Insert(0, hediffdef);
                        }
                    }
                }

                return SettingsCache._allHediffsCache;
            }
        }

        public static void Init()
        {
            if (SettingsCache.CacheInited)
            {
                return;
            }

            SettingsCache.MinCache    = (float) Math.Round(Storage.MultiplierCaps.min * 100);
            SettingsCache.MaxCache    = (float) Math.Round(Storage.MultiplierCaps.max * 100);
            SettingsCache.NVZeroCache = SettingsHelpers.ModToMultiPercent(LightModifiersBase.NVLightModifiers[0], true);

            SettingsCache.NVFullCache =
                        SettingsHelpers.ModToMultiPercent(LightModifiersBase.NVLightModifiers[1], false);

            SettingsCache.PSZeroCache = SettingsHelpers.ModToMultiPercent(LightModifiersBase.PSLightModifiers[0], true);

            SettingsCache.PSFullCache =
                        SettingsHelpers.ModToMultiPercent(LightModifiersBase.PSLightModifiers[1], false);

            SettingsCache.CacheInited = true;
        }

        /// <summary>
        ///     Sets new settings
        ///     Clears all cached stuff
        ///     Runs when opening the settings menu and when closing it
        /// </summary>
        public static void DoPreWriteTasks()
        {
            // this check is required because this method is run on opening the menu
            if (SettingsCache.CacheInited)
            {
                Storage.MultiplierCaps.min = SettingsCache.MinCache != null && Storage.CustomCapsEnabled
                            ? (float) Math.Round((float) SettingsCache.MinCache / 100, 2)
                            : Storage.MultiplierCaps.min;

                Storage.MultiplierCaps.max = SettingsCache.MaxCache != null && Storage.CustomCapsEnabled
                            ? (float) Math.Round((float) SettingsCache.MaxCache / 100, 2)
                            : Storage.MultiplierCaps.max;

                LightModifiersBase.NVLightModifiers.Offsets = new[]
                                                              {
                                                                  SettingsCache.NVZeroCache != null
                                                                              ? SettingsHelpers.MultiPercentToMod(
                                                                                                                  (float
                                                                                                                  ) SettingsCache
                                                                                                                              .NVZeroCache,
                                                                                                                  true
                                                                                                                 )
                                                                              : LightModifiersBase.NVLightModifiers[0],
                                                                  SettingsCache.NVFullCache != null
                                                                              ? SettingsHelpers.MultiPercentToMod(
                                                                                                                  (float
                                                                                                                  ) SettingsCache
                                                                                                                              .NVFullCache,
                                                                                                                  false
                                                                                                                 )
                                                                              : LightModifiersBase.NVLightModifiers[1]
                                                              };

                LightModifiersBase.PSLightModifiers.Offsets = new[]
                                                              {
                                                                  SettingsCache.PSZeroCache != null
                                                                              ? SettingsHelpers.MultiPercentToMod(
                                                                                                                  (float
                                                                                                                  ) SettingsCache
                                                                                                                              .PSZeroCache,
                                                                                                                  true
                                                                                                                 )
                                                                              : LightModifiersBase.PSLightModifiers[0],
                                                                  SettingsCache.PSFullCache != null
                                                                              ? SettingsHelpers.MultiPercentToMod(
                                                                                                                  (float
                                                                                                                  ) SettingsCache
                                                                                                                              .PSFullCache,
                                                                                                                  false
                                                                                                                 )
                                                                              : LightModifiersBase.PSLightModifiers[1]
                                                              };

                Classifier.ZeroLightTurningPoints = null;
                Classifier.FullLightTurningPoint  = null;

                SettingsCache.MinCache    = null;
                SettingsCache.MaxCache    = null;
                SettingsCache.NVZeroCache = null;
                SettingsCache.NVFullCache = null;
                SettingsCache.PSZeroCache = null;
                SettingsCache.PSFullCache = null;
            }

            SettingsCache.CacheInited = false;
            SettingsCache._allHediffsCache?.Clear();
            SettingsCache._headgearCache?.Clear();

            SettingsHelpers.TipStringHolder.Clear();
            DrawSettings.Clear();

            if (Current.ProgramState == ProgramState.Playing)
            {
                SettingsCache.SetDirtyAllComps();
            }
        }


        /// <summary>
        ///     So that the comps will update with the new settings, sets all the comps dirty
        /// </summary>
        public static void SetDirtyAllComps()
        {
            foreach (Pawn pawn in PawnsFinder.AllMaps_Spawned)
            {
                if (pawn == null)
                {
                    continue;
                }

                Log.Message($"Found {pawn}");

                if (pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
                {
                    comp.SetDirty();
                    Log.Message($"Set {pawn}'s comp to dirty");
                }
            }
        }
    }
}