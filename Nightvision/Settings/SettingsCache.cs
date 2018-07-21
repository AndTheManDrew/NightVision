using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace NightVision
    {
        public static class SettingsCache
            {
                [CanBeNull] public static List<HediffDef> AllHediffsCache;
                public static             bool            CacheInited;
                [CanBeNull] public static List<ThingDef>  HeadgearCache;
                public static             float?          MaxCache;

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
                                if (HeadgearCache == null)
                                    {
                                        HeadgearCache = new List<ThingDef>(Storage.AllEyeCoveringHeadgearDefs);
                                        foreach (ThingDef appareldef in Storage.NVApparel.Keys)
                                            {
                                                int appindex = HeadgearCache.IndexOf(appareldef);
                                                if (appindex > 0)
                                                    {
                                                        HeadgearCache.RemoveAt(appindex);
                                                        HeadgearCache.Insert(0, appareldef);
                                                    }
                                            }
                                    }

                                return HeadgearCache;
                            }
                    }

                [NotNull]
                public static List<HediffDef> GetAllHediffs
                    {
                        get
                            {
                                if (AllHediffsCache == null)
                                    {
                                        AllHediffsCache = new List<HediffDef>(Storage.AllSightAffectingHediffs);
                                        foreach (HediffDef hediffdef in Storage.HediffLightMods.Keys)
                                            {
                                                int appindex = AllHediffsCache.IndexOf(hediffdef);
                                                if (appindex > 0)
                                                    {
                                                        AllHediffsCache.RemoveAt(appindex);
                                                        AllHediffsCache.Insert(0, hediffdef);
                                                    }
                                            }
                                    }

                                return AllHediffsCache;
                            }
                    }

                public static void Init()
                    {
                        if (CacheInited)
                            {
                                return;
                            }

                        MinCache    = (float) Math.Round(Storage.MultiplierCaps.min * 100);
                        MaxCache    = (float) Math.Round(Storage.MultiplierCaps.max * 100);
                        NVZeroCache = SettingsHelpers.ModToMultiPercent(LightModifiersBase.NVLightModifiers[0], true);
                        NVFullCache = SettingsHelpers.ModToMultiPercent(LightModifiersBase.NVLightModifiers[1], false);
                        PSZeroCache = SettingsHelpers.ModToMultiPercent(LightModifiersBase.PSLightModifiers[0], true);
                        PSFullCache = SettingsHelpers.ModToMultiPercent(LightModifiersBase.PSLightModifiers[1], false);
                        CacheInited = true;
                    }

                /// <summary>
                ///     Sets new settings
                ///     Clears all cached stuff
                ///     Runs when opening the settings menu and when closing it
                /// </summary>
                public static void DoPreWriteTasks()
                    {
                        // this check is required because this method is run on opening the menu
                        if (CacheInited)
                            {
                                Storage.MultiplierCaps.min = MinCache != null
                                            ? (float) Math.Round((float) MinCache / 100, 2)
                                            : Storage.MultiplierCaps.min;
                                Storage.MultiplierCaps.max = MaxCache != null
                                            ? (float) Math.Round((float) MaxCache / 100, 2)
                                            : Storage.MultiplierCaps.max;

                                LightModifiersBase.NVLightModifiers.Offsets = new[]
                                {
                                    NVZeroCache != null
                                                ? SettingsHelpers.MultiPercentToMod((float) NVZeroCache, true)
                                                : LightModifiersBase.NVLightModifiers[0],
                                    NVFullCache != null
                                                ? SettingsHelpers.MultiPercentToMod((float) NVFullCache, false)
                                                : LightModifiersBase.NVLightModifiers[1]
                                };
                                LightModifiersBase.PSLightModifiers.Offsets = new[]
                                {
                                    PSZeroCache != null
                                                ? SettingsHelpers.MultiPercentToMod((float) PSZeroCache, true)
                                                : LightModifiersBase.PSLightModifiers[0],
                                    PSFullCache != null
                                                ? SettingsHelpers.MultiPercentToMod((float) PSFullCache, false)
                                                : LightModifiersBase.PSLightModifiers[1]
                                };

                                Classifier.ZeroLightTurningPoints = null;
                                Classifier.FullLightTurningPoint  = null;

                                MinCache    = null;
                                MaxCache    = null;
                                NVZeroCache = null;
                                NVFullCache = null;
                                PSZeroCache = null;
                                PSFullCache = null;
                            }

                        CacheInited     = false;
                        AllHediffsCache = null;
                        HeadgearCache   = null;

                        SettingsHelpers.TipStringHolder.Clear();
                        DrawSettings._numberOfCustomRaces   = null;
                        DrawSettings._numberOfCustomHediffs = null;
                        if (Current.ProgramState == ProgramState.Playing)
                            {
                                SetDirtyAllComps();
                            }

                        DrawSettings._allPawns = null;
                        DrawSettings._maxY     = 0f;
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