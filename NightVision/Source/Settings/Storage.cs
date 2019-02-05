// Nightvision NightVision Storage.cs
// 
// 03 08 2018
// 
// 24 10 2018

using System.Collections.Generic;
using System.Linq;
using Verse;

namespace NightVision
{
    public static class Storage
    {
        public const float HighestCap = 2f;

        public const  float             LowestCap                  = 0.01f;
        public static HashSet<ThingDef> AllEyeCoveringHeadgearDefs = new HashSet<ThingDef>();

        //AllEyeHediffs is a subset of AllSightAffectingHediffs
        public static HashSet<HediffDef> AllEyeHediffs            = new HashSet<HediffDef>();
        public static HashSet<HediffDef> AllSightAffectingHediffs = new HashSet<HediffDef>();
        public static bool               CustomCapsEnabled;
        public static bool               NullRefWhenLoading = false;

        public static Dictionary<HediffDef, Hediff_LightModifiers> HediffLightMods = new Dictionary<HediffDef, Hediff_LightModifiers>();

        public static FloatRange MultiplierCaps = new FloatRange(
            min: Constants_Calculations.DefaultMinCap,
            max: Constants_Calculations.DefaultMaxCap
        );

        public static Dictionary<ThingDef, ApparelVisionSetting> NVApparel = new Dictionary<ThingDef, ApparelVisionSetting>();

        public static bool NVEnabledForCE = true;


        public static Dictionary<ThingDef, Race_LightModifiers> RaceLightMods = new Dictionary<ThingDef, Race_LightModifiers>();

        public static void ExposeSettings()
        {
            Scribe_Values.Look(value: ref CustomCapsEnabled, label: "CustomLimitsEnabled");

            if (CustomCapsEnabled)
            {
                if (Scribe.mode == LoadSaveMode.Saving)
                {
                    if (MultiplierCaps.min > MultiplierCaps.max)
                    {
                        float temp = MultiplierCaps.max;
                        MultiplierCaps.max = MultiplierCaps.min;
                        MultiplierCaps.min = temp;
                    }
                }

                Scribe_Values.Look(value: ref MultiplierCaps.min, label: "LowerLimit", defaultValue: 0.8f);
                Scribe_Values.Look(value: ref MultiplierCaps.max, label: "UpperLimit", defaultValue: 1.2f);

                if (Scribe.mode == LoadSaveMode.LoadingVars)
                {
                    if (MultiplierCaps.min > MultiplierCaps.max)
                    {
                        float temp = MultiplierCaps.max;
                        MultiplierCaps.max = MultiplierCaps.min;
                        MultiplierCaps.min = temp;
                    }
                }
            }

            Scribe_Values.Look(value: ref NVGameComponent.FlareRaidIsEnabled, label: "flareRaidEnabled");

            Storage_Combat.LoadSaveCommit();

            Scribe_Values.Look(value: ref NVEnabledForCE, label: "EnabledForCombatExtended", defaultValue: true);

            //cctor args because otherwise statics don't seem to load properly
            Scribe_Deep.Look(target: ref LightModifiersBase.PSLightModifiers, label: "photosensitivitymodifiers", true, false);

            Scribe_Deep.Look(target: ref LightModifiersBase.NVLightModifiers, label: "nightvisionmodifiers", false, true);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (LightModifiersBase.PSLightModifiers == null)
                {
                    LightModifiersBase.PSLightModifiers = new LightModifiersBase
                                                          {
                                                              Offsets = Constants_Calculations.PSDefaultOffsets.ToArray(), Initialised = true
                                                          };
                }

                if (LightModifiersBase.NVLightModifiers == null)
                {
                    LightModifiersBase.NVLightModifiers = new LightModifiersBase
                                                          {
                                                              Offsets = Constants_Calculations.NVDefaultOffsets.ToArray(), Initialised = true
                                                          };
                }
            }

            Scribes.LightModifiersDict(dictionary: ref RaceLightMods,   label: "Races");
            Scribes.LightModifiersDict(dictionary: ref HediffLightMods, label: "Hediffs");
            Scribes.ApparelDict(dictionary: ref NVApparel);
        }

        public static void ResetAllSettings()
        {
            Log.Message(text: "NightVision: Defaulting Settings");

            CustomCapsEnabled = false;

            MultiplierCaps = new FloatRange(
                min: Constants_Calculations.DefaultMinCap,
                max: Constants_Calculations.DefaultMaxCap
            );

            NVEnabledForCE                              = true;
            NVGameComponent.FlareRaidIsEnabled          = true;
            LightModifiersBase.PSLightModifiers.Offsets = LightModifiersBase.PSLightModifiers.DefaultOffsets.ToArray();

            LightModifiersBase.NVLightModifiers.Offsets = LightModifiersBase.NVLightModifiers.DefaultOffsets.ToArray();

            Storage_Combat.ResetCombatSettings();

            Log.Message(text: "NightVision.Storage.ResetAllSettings: Clearing Dictionaries");
            RaceLightMods              = null;
            HediffLightMods            = null;
            NVApparel                  = null;
            AllEyeCoveringHeadgearDefs = null;
            AllEyeHediffs              = null;
            AllSightAffectingHediffs   = null;
            Log.Message(text: "NightVision.Storage.ResetAllSettings: Rebuilding Dictionaries");
            Initialiser.FindDefsToAddNightVisionTo();
            SettingsCache.CacheInited = false;
            SettingsCache.DoPreWriteTasks();
            SettingsCache.Init();
            FieldClearer.ResetSettingsDependentFields();
        }
    }
}