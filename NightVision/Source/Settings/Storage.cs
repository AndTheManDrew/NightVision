// Nightvision NightVision Storage.cs
// 
// 21 07 2018
// 
// 21 07 2018

using System.Collections.Generic;
using System.Linq;
using Verse;

namespace NightVision
    {
        internal static class Storage
            {
                public static bool NVEnabledForCE = true;
                public static HashSet<ThingDef> AllEyeCoveringHeadgearDefs = new HashSet<ThingDef>();

                //AllEyeHediffs is a subset of AllSightAffectingHediffs
                public static HashSet<HediffDef> AllEyeHediffs            = new HashSet<HediffDef>();
                public static HashSet<HediffDef> AllSightAffectingHediffs = new HashSet<HediffDef>();
                public static bool               CustomCapsEnabled;

                public static Dictionary<HediffDef, Hediff_LightModifiers> HediffLightMods =
                            new Dictionary<HediffDef, Hediff_LightModifiers>();

                public const float HighestCap = 2f;

                public const float LowestCap = 0.01f;

                public static FloatRange MultiplierCaps =
                            new FloatRange(Constants.DefaultMinCap, Constants.DefaultMaxCap);

                public static Dictionary<ThingDef, ApparelVisionSetting> NVApparel =
                            new Dictionary<ThingDef, ApparelVisionSetting>();


                public static Dictionary<ThingDef, Race_LightModifiers> RaceLightMods =
                            new Dictionary<ThingDef, Race_LightModifiers>();

                public static void ExposeSettings()
                    {
                        Scribe_Values.Look(ref CustomCapsEnabled, "CustomLimitsEnabled");
                        if (CustomCapsEnabled)
                            {
                                Scribe_Values.Look(ref MultiplierCaps.min, "LowerLimit", 0.8f);
                                Scribe_Values.Look(ref MultiplierCaps.max, "UpperLimit", 1.2f);
                            }

                        Scribe_Values.Look(ref NVEnabledForCE, "EnabledForCombatExtended", true);
                        Scribe_Deep.Look(ref LightModifiersBase.PSLightModifiers, "photosensitivitymodifiers");
                        Scribe_Deep.Look(ref LightModifiersBase.NVLightModifiers, "nightvisionmodifiers");
                        if (Scribe.mode == LoadSaveMode.LoadingVars)
                            {
                                if (LightModifiersBase.PSLightModifiers == null)
                                    {
                                        LightModifiersBase.PSLightModifiers = new LightModifiersBase
                                        {
                                            Offsets     = Constants.PSDefaultOffsets.ToArray(),
                                            Initialised = true
                                        };
                                    }

                                if (LightModifiersBase.NVLightModifiers == null)
                                    {
                                        LightModifiersBase.NVLightModifiers = new LightModifiersBase
                                        {
                                            Offsets     = Constants.NVDefaultOffsets.ToArray(),
                                            Initialised = true
                                        };
                                    }
                            }

                        Scribes.LightModifiersDict(ref RaceLightMods,   "Race");
                        Scribes.LightModifiersDict(ref HediffLightMods, "Hediffs");
                        Scribes.ApparelDict(ref NVApparel);
                    }
            }
    }