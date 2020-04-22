// Nightvision NightVision Storage.cs
// 
// 03 08 2018
// 
// 24 10 2018

using System;
using System.Collections.Generic;
using System.Linq;

using Verse;

namespace NightVision
{
    public class Storage
    {
        public const float HighestCap = 2f;

        public const float LowestCap = 0.01f;
        public HashSet<ThingDef> AllEyeCoveringHeadgearDefs = new HashSet<ThingDef>();

        //AllEyeHediffs is a subset of AllSightAffectingHediffs
        public HashSet<HediffDef> AllEyeHediffs = new HashSet<HediffDef>();
        public HashSet<HediffDef> AllSightAffectingHediffs = new HashSet<HediffDef>();
        public bool CustomCapsEnabled;
        public bool NullRefWhenLoading = false;

        public Dictionary<HediffDef, Hediff_LightModifiers> HediffLightMods =
            new Dictionary<HediffDef, Hediff_LightModifiers>();

        public FloatRange MultiplierCaps = new FloatRange(
            min: Constants.DEFAULT_MIN_CAP,
            max: Constants.DEFAULT_MAX_CAP
        );

        public Dictionary<ThingDef, ApparelVisionSetting> NVApparel = new Dictionary<ThingDef, ApparelVisionSetting>();

        public bool NVEnabledForCE = true;


        public Dictionary<ThingDef, Race_LightModifiers>
            RaceLightMods = new Dictionary<ThingDef, Race_LightModifiers>();

        public void ExposeSettings()
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

            //combatStore.LoadSaveCommit();

            Scribe_Values.Look(value: ref NVEnabledForCE, label: "EnabledForCombatExtended", defaultValue: true);

            //cctor args because otherwise statics don't seem to load properly
            Scribe_Deep.Look(target: ref LightModifiersBase.PSLightModifiers, label: "photosensitivitymodifiers", true,
                false);

            Scribe_Deep.Look(target: ref LightModifiersBase.NVLightModifiers, label: "nightvisionmodifiers", false,
                true);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (LightModifiersBase.PSLightModifiers == null)
                {
                    LightModifiersBase.PSLightModifiers = new LightModifiersBase
                    {
                        Offsets = Constants.PSDefaultOffsets.ToArray(), Initialised = true
                    };
                }

                if (LightModifiersBase.NVLightModifiers == null)
                {
                    LightModifiersBase.NVLightModifiers = new LightModifiersBase
                    {
                        Offsets = Constants.NVDefaultOffsets.ToArray(), Initialised = true
                    };
                }
            }

            var nullRef = Scribes.LightModifiersDict(dictionary: ref RaceLightMods, label: "Races");
            nullRef |= Scribes.LightModifiersDict(dictionary: ref HediffLightMods, label: "Hediffs");
            nullRef |= Scribes.ApparelDict(dictionary: ref NVApparel);
            NullRefWhenLoading = nullRef;
            //FieldClearer.ResetSettingsDependentFields();
        }

        public void ResetAllSettings()
        {
            Log.Message(text: "NightVision: Defaulting Settings");

            CustomCapsEnabled = false;

            MultiplierCaps = new FloatRange(
                min: Constants.DEFAULT_MIN_CAP,
                max: Constants.DEFAULT_MAX_CAP
            );

            NVEnabledForCE = true;
            NVGameComponent.FlareRaidIsEnabled = true;
            LightModifiersBase.PSLightModifiers.Offsets = LightModifiersBase.PSLightModifiers.DefaultOffsets.ToArray();

            LightModifiersBase.NVLightModifiers.Offsets = LightModifiersBase.NVLightModifiers.DefaultOffsets.ToArray();

            Settings.CombatStore.LoadDefaultSettings();

            Log.Message(text: "NightVision.Storage.ResetAllSettings: Clearing Dictionaries");
            RaceLightMods = null;
            HediffLightMods = null;
            NVApparel = null;
            AllEyeCoveringHeadgearDefs = null;
            AllEyeHediffs = null;
            AllSightAffectingHediffs = null;
            Log.Message(text: "NightVision.Storage.ResetAllSettings: Rebuilding Dictionaries");
            var initialiser = new Initialiser();
            initialiser.FindDefsToAddNightVisionTo();

            Settings.Cache.Reset();
            FieldClearer.ResetSettingsDependentFields();
        }

        public void SetMinMultiplierCap(float newMin)
        {
            MultiplierCaps.min = (float) Math.Round(newMin / 100, Constants.NUMBER_OF_DIGITS);
        }

        public void SetMaxMultiplierCap(float newMax)
        {
            MultiplierCaps.max = (float) Math.Round(newMax / 100, Constants.NUMBER_OF_DIGITS);
        }

        public float ClampToMultipliers(float val)
        {
            if (val < MultiplierCaps.min - Constants.NV_EPSILON)
            {
                return MultiplierCaps.min;
            }

            if (val > MultiplierCaps.max + Constants.NV_EPSILON)
            {
                return MultiplierCaps.max;
            }

            return val;
        }
    }
}