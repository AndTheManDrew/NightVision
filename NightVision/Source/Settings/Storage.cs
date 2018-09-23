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
        public static HashSet<ThingDef> AllEyeCoveringHeadgearDefs = new HashSet<ThingDef>();

        //AllEyeHediffs is a subset of AllSightAffectingHediffs
        public static HashSet<HediffDef> AllEyeHediffs            = new HashSet<HediffDef>();
        public static HashSet<HediffDef> AllSightAffectingHediffs = new HashSet<HediffDef>();
        public static bool               CustomCapsEnabled;
        public static bool NullRefWhenLoading = false;

        public static Dictionary<HediffDef, Hediff_LightModifiers> HediffLightMods =
                    new Dictionary<HediffDef, Hediff_LightModifiers>();

        public const float HighestCap = 2f;

        public const float LowestCap = 0.01f;

        public static FloatRange MultiplierCaps =
                    new FloatRange(Constants.DefaultMinCap, Constants.DefaultMaxCap);

        public static Dictionary<ThingDef, ApparelVisionSetting> NVApparel =
                    new Dictionary<ThingDef, ApparelVisionSetting>();

        public static bool NVEnabledForCE = true;


        public static Dictionary<ThingDef, Race_LightModifiers> RaceLightMods =
                    new Dictionary<ThingDef, Race_LightModifiers>();

        public static void ExposeSettings()
        {
            Scribe_Values.Look(ref Storage.CustomCapsEnabled, "CustomLimitsEnabled");

            if (Storage.CustomCapsEnabled)
            {
                Scribe_Values.Look(ref Storage.MultiplierCaps.min, "LowerLimit", 0.8f);
                Scribe_Values.Look(ref Storage.MultiplierCaps.max, "UpperLimit", 1.2f);
            }

            Scribe_Values.Look(ref Storage.NVEnabledForCE, "EnabledForCombatExtended", true);

            //cctor args because otherwise statics don't seem to load properly
            Scribe_Deep.Look(
                             ref LightModifiersBase.PSLightModifiers,
                             "photosensitivitymodifiers",
                             true,
                             false
                            );

            Scribe_Deep.Look(ref LightModifiersBase.NVLightModifiers, "nightvisionmodifiers", false, true);

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

            Scribes.LightModifiersDict(ref Storage.RaceLightMods,   "Races");
            Scribes.LightModifiersDict(ref Storage.HediffLightMods, "Hediffs");
            Scribes.ApparelDict(ref Storage.NVApparel);
        }

        public static void ResetAllSettings()
        {
            Log.Message("NightVision.Storage.ResetAllSettings: Defaulting Settings");

            Storage.CustomCapsEnabled = false;
            Storage.MultiplierCaps    = new FloatRange(Constants.DefaultMinCap, Constants.DefaultMaxCap);
            Storage.NVEnabledForCE    = true;

            LightModifiersBase.PSLightModifiers.Offsets =
                        LightModifiersBase.PSLightModifiers.DefaultOffsets.ToArray();

            LightModifiersBase.NVLightModifiers.Offsets =
                        LightModifiersBase.NVLightModifiers.DefaultOffsets.ToArray();

            Log.Message("NightVision.Storage.ResetAllSettings: Clearing Dictionaries");
            Storage.RaceLightMods              = null;
            Storage.HediffLightMods            = null;
            Storage.NVApparel                  = null;
            Storage.AllEyeCoveringHeadgearDefs = null;
            Storage.AllEyeHediffs              = null;
            Storage.AllSightAffectingHediffs   = null;
            Log.Message("NightVision.Storage.ResetAllSettings: Rebuilding Dictionaries");
            Initialiser.BuildDictionarys();
            SettingsCache.CacheInited = false;
            SettingsCache.DoPreWriteTasks();
            SettingsCache.Init();
        }
    }
}