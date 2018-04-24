using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace NightVision
{
    public interface INVSaveCheck
    {
        bool ShouldBeSaved();
    }
    /// <summary>
    /// For storing night vision settings for items of apparel
    /// </summary>
    public class ApparelSetting : IExposable, INVSaveCheck
    {
        #region Fields
        //Current Settings
        internal bool NullifiesPS = false;
        internal bool GrantsNV = false;
        //Settings in xml defs
        private bool compNullifiesPS = false;
        private bool compGrantsNV = false;
        #endregion

        #region Constructors And comp attacher
        /// <summary>
        /// Parameterless Constructor: necessary for RW scribe system
        /// </summary>
        public ApparelSetting() { }

        /// <summary>
        /// Manual Constructor: for when user instantiates new setting in mod settings
        /// </summary>
        internal ApparelSetting(bool nullPS, bool giveNV)
        {
            NullifiesPS = nullPS;
            GrantsNV = giveNV;
        }

        /// <summary>
        /// Constructor for Dictionary builder
        /// </summary>
        internal ApparelSetting(CompProperties_NightVisionApparel compprops)
        {
            compNullifiesPS = compprops.nullifiesPhotosensitivity;
            compGrantsNV = compprops.grantsNightVision;
            NullifiesPS = compNullifiesPS;
            GrantsNV = compGrantsNV;
        }

        /// <summary>
        /// Dictionary builder attaches the comp settings to preexisting entries
        /// </summary>
        internal void AttachComp(CompProperties_NightVisionApparel compprops)
        {
            compNullifiesPS = compprops.nullifiesPhotosensitivity;
            compGrantsNV = compprops.grantsNightVision;
        }
        #endregion

        #region Equality, Redundancy, and INVSaveCheck checks
        internal bool Equals(ApparelSetting other) => this.GrantsNV == other.GrantsNV && this.NullifiesPS == other.NullifiesPS;

        /// <summary>
        /// Check to see if this setting should be removed from the dictionary, i.e. current and def values are all false
        /// </summary>
        internal bool IsRedundant() => !(this.GrantsNV || this.NullifiesPS) && !(this.compGrantsNV || this.compNullifiesPS);
        /// <summary>
        /// Check to see if this setting should be saved, i.e. current and def values are all false,
        /// or current values are equal to def values
        /// </summary>
        /// <returns></returns>
        public bool ShouldBeSaved()
        {
            return !(IsRedundant() || (this.GrantsNV == this.compGrantsNV && this.NullifiesPS == this.compNullifiesPS));
        }
        #endregion

        #region Expose Data
        public void ExposeData()
        {
            Scribe_Values.Look(ref NullifiesPS, "nullifiesphotosens", compNullifiesPS);
            Scribe_Values.Look(ref GrantsNV, "grantsnightvis", compGrantsNV);
        }
        #endregion
    }

    /// <summary>
    /// For storing modifiers to work and move speed light multipliers. 
    /// Used to store settings for: hediffs, races, and individual pawns
    /// </summary>
    public class GlowMods : IExposable, INVSaveCheck
    {
        #region Default Constants
        internal const float DefaultNVZero = 0.2f;
        internal const float DefaultNVFull = 0.0f;
        internal const float DefaultPSZero = 0.4f;
        internal const float DefaultPSFull = -0.2f;
        #endregion

        internal enum Options : byte { NVNone = 0, NVNightVision = 1, NVPhotosensitivity = 2, NVCustom = 3}

        #region Static Fields

        protected internal static float nvZeroLightMod = DefaultNVZero;
        protected internal static float nvFullLightMod = DefaultNVFull;
        protected internal static float psZeroLightMod = DefaultPSZero;
        protected internal static float psFullLightMod = DefaultPSFull;

        protected static int NVVersion = 0;
        protected static int PSVersion = 0;

        #endregion

        #region Instance Fields
        protected float? zeroLightMod = null;
        protected float? fullLightMod = null;

        internal bool LoadedFromFile = false;
        internal Options fileSetting = Options.NVNone;
        internal float DefaultZeroLightMod = 0.0f;
        internal float DefaultFullLightMod = 0.0f;

        internal Options Setting;
        #endregion

        #region Get And Set Static Fields
        //Seperate get and set as get needs to be virtual so EyeGlowMods can override
        //and set needs to be static so it can be accessed from Settings
        internal virtual float NVZeroLightMod
        {
            get => nvZeroLightMod;
        }
        internal static float SetNVZeroLightMod
        {
            set { nvZeroLightMod = RoundAndCheck(value, true); NVVersion++; }
        }
        internal virtual float NVFullLightMod
        {
            get => nvFullLightMod;
        }
        internal static float SetNVFullLightMod
        {
            set { nvFullLightMod = RoundAndCheck(value, false); NVVersion++; }
        }
        internal virtual float PSZeroLightMod
        {
            get => psZeroLightMod;
        }
        internal static float SetPSZeroLightMod
        {
            set { psZeroLightMod = RoundAndCheck(value, true); PSVersion++; }
        }
        internal virtual float PSFullLightMod
        {
            get => psFullLightMod;
        }
        internal static float SetPSFullLightMod
        {
            set { psFullLightMod = RoundAndCheck(value, false); PSVersion++; }
        }
        #endregion
        
        #region Get And Set Instance Fields
        internal float ZeroLight
        {
            get
            {
                switch (Setting)
                {
                    default:
                    case Options.NVNone:
                        return DefaultZeroLightMod;
                    case Options.NVNightVision:
                        return NVZeroLightMod;
                    case Options.NVPhotosensitivity:
                        return PSZeroLightMod;
                    case Options.NVCustom:
                        return zeroLightMod ?? DefaultZeroLightMod;
                }
            }
            set
            {
                zeroLightMod = RoundAndCheck(value, true);
                Setting = Options.NVCustom;
            }
        }

        internal float FullLight
        {
            get
            {
                switch (Setting)
                {
                    
                    default:
                    case Options.NVNone:
                        return DefaultFullLightMod;
                    case Options.NVNightVision:
                        return NVFullLightMod;
                    case Options.NVPhotosensitivity:
                        return PSFullLightMod;
                    case Options.NVCustom:
                        return fullLightMod ?? DefaultFullLightMod;
                }
            }
            set
            {
                fullLightMod = RoundAndCheck(value, false);
                Setting = Options.NVCustom;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Parameterless constructor
        /// </summary>
        public GlowMods()
        {
            zeroLightMod = null;
            fullLightMod = null;
            Setting = Options.NVNone;
        }
        /// <summary>
        /// Manual Contructor
        /// </summary>
        /// <param name="zerolight"></param>
        /// <param name="fulllight"></param>
        internal GlowMods(float zerolight, float fulllight)
        {
            Setting = Options.NVCustom;
            zeroLightMod = zerolight;
            fullLightMod = fulllight;
        }
        internal GlowMods(Options setting)
        {
            zeroLightMod = null;
            fullLightMod = null;
            Setting = setting;
        }
        /// <summary>
        /// Constructor for Races: NOT eye normalised
        /// </summary>
        /// <param name="compprops"></param>
        internal GlowMods(CompProperties_NightVision compprops)
        {
            LoadedFromFile = true;
            if (compprops == null)
            {
                LoadedFromFile = false;
                Setting = Options.NVNone;
            }
            else if (compprops.naturalNightVision)
            {
                Setting = Options.NVNightVision;
                fileSetting = Setting;
            }
            else if (compprops.naturalPhotosensitivity)
            {
                Setting = Options.NVPhotosensitivity;
                fileSetting = Setting;
            }
            else
            {
                fileSetting = Options.NVCustom;
                DefaultZeroLightMod = RoundAndCheck(compprops.zeroLightMultiplier - NightVisionSettings.DefaultZeroLightMultiplier, true);
                DefaultFullLightMod = RoundAndCheck(compprops.fullLightMultiplier - NightVisionSettings.DefaultFullLightMultiplier, false);
                Setting = fileSetting;
                zeroLightMod = DefaultZeroLightMod;
                fullLightMod = DefaultFullLightMod;
            }
        }
        /// <summary>
        /// Constructor for Hediffs that are not eye normalised
        /// </summary>
        /// <param name="compprops"></param>
        internal GlowMods(HediffCompProperties_NightVision compprops)
        {
            LoadedFromFile = true;
            if (compprops == null)
            {
                LoadedFromFile = false;
                Setting = Options.NVNone;
            }
            else if (compprops.grantsNightVision)
            {
                Setting = Options.NVNightVision;
                fileSetting = Setting;
            }
            else if (compprops.grantsPhotosensitivity)
            {
                Setting = Options.NVPhotosensitivity;
                fileSetting = Setting;
            }
            else
            {
                fileSetting = Options.NVCustom;
                DefaultZeroLightMod = RoundAndCheck(compprops.zeroLightMod, true);
                DefaultFullLightMod = RoundAndCheck(compprops.fullLightMod, false);
                Setting = fileSetting;
                zeroLightMod = DefaultZeroLightMod;
                fullLightMod = DefaultFullLightMod;
            }
        }
        #endregion

        #region Misc Helpers
        internal static float RoundAndCheck(float desiredValue, bool forZeroLight, int numNormalise = 1)
        {
            float resultingMultiplier = (forZeroLight ? NightVisionSettings.DefaultZeroLightMultiplier : NightVisionSettings.DefaultFullLightMultiplier)
                                + desiredValue;
            return (float)Math.Round((resultingMultiplier < 0.01f ? (-1f / numNormalise) + 0.01f : desiredValue / numNormalise), 2);
        }

        internal float GetEffectAtGlow(float glow)
        {
            float effect = 0f;
            if (glow < 0.3f)
            {
                effect = ZeroLight * (0.3f - glow) / 0.3f;
            }
            else
            {
                effect = FullLight * (glow - 0.7f) / 0.3f;
            }
            return effect;
        }
        #endregion

        #region Change Setting And Attach Comp
        internal void ChangeSetting(Options newsetting)
        {
            if(Setting != newsetting)
            {
                if (newsetting == Options.NVCustom && (zeroLightMod == null || fullLightMod == null))
                {
                    switch(Setting)
                    {
                        case Options.NVNightVision:
                            zeroLightMod = NVZeroLightMod;
                            fullLightMod = NVFullLightMod;
                            break;
                        case Options.NVPhotosensitivity:
                            zeroLightMod = PSZeroLightMod;
                            fullLightMod = PSFullLightMod;
                            break;
                        default:
                        case Options.NVNone:
                            zeroLightMod = 0f;
                            fullLightMod = 0f;
                            break;
                    }
                }
                
                Setting = newsetting;
            }
        }

        internal void AttachComp(CompProperties_NightVision compprops)
        {
            LoadedFromFile = true;
            if (compprops == null)
            {
                LoadedFromFile = false;
            }
            else if (compprops.naturalNightVision)
            {
                fileSetting = Options.NVNightVision;
            }
            else if (compprops.naturalPhotosensitivity)
            {
                fileSetting = Options.NVPhotosensitivity;
            }
            else
            {
                fileSetting = Options.NVCustom;
                //No need to normalise  for case when inherited by eyeglowmods as race comps are always glowmods
                DefaultZeroLightMod = RoundAndCheck(compprops.zeroLightMultiplier - NightVisionSettings.DefaultZeroLightMultiplier, true);
                DefaultFullLightMod = RoundAndCheck(compprops.fullLightMultiplier - NightVisionSettings.DefaultFullLightMultiplier, false);
            }
        }

        internal void AttachComp(HediffCompProperties_NightVision compprops)
        {
            LoadedFromFile = true;
            if (compprops == null)
            {
                LoadedFromFile = false;
            }
            else if (compprops.grantsNightVision)
            {
                fileSetting = Options.NVNightVision;
            }
            else if (compprops.grantsPhotosensitivity)
            {
                fileSetting = Options.NVPhotosensitivity;
            }
            else
            {
                fileSetting = Options.NVCustom;
                //Need to normalise for two eyes to ensure installing two of the hediff will not produce a negative multiplier
                //Note default value for NumOfEyesNormalisedFor is 2 - hopefully won't cause an exception
                //What about three or more eyed races?? have to limit the actual calculation in comp_NightVision
                int numNormalise = ((EyeGlowMods)this)?.NumOfEyesNormalisedFor ?? 1;
                DefaultZeroLightMod = RoundAndCheck(compprops.zeroLightMod, true, numNormalise);
                DefaultFullLightMod = RoundAndCheck(compprops.fullLightMod, false, numNormalise);
            }
        }
        #endregion

        #region Simple Checkers
        internal bool NoCustomSettings()
        {
            return zeroLightMod == null && fullLightMod == null;
        }
        internal bool IsCustom()
        {
            return Setting == Options.NVCustom;
        }
        internal bool IsNone()
        {
            return Setting == Options.NVNone;
        }

        internal bool IsRedundant()
        {
            return (!LoadedFromFile && ((Setting == Options.NVCustom && Math.Abs(zeroLightMod ?? 0) < 0.001 && Math.Abs(fullLightMod ?? 0) < 0.001) || Setting == Options.NVNone));
        }
        #endregion

        #region Save and load members
        public bool ShouldBeSaved()
        {
            return !(IsRedundant() || (LoadedFromFile && Setting == fileSetting && (fileSetting != Options.NVCustom ||
                            ((zeroLightMod == null || Mathf.Approximately((float)zeroLightMod, DefaultZeroLightMod))
                                    && (fullLightMod == null || Mathf.Approximately((float)fullLightMod, DefaultFullLightMod))))));
        }

        public void ExposeData()
        {
            //Only want to save the setting if it is different from the setting on file
            //so that changing the setting on file (i.e. in the mod) changes the setting in game if user has not altered it
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                if(!LoadedFromFile || (LoadedFromFile && Setting != fileSetting))
                {
                    Scribe_Values.Look(ref Setting, "setting", fileSetting);
                }
            }
            else
            {
                Scribe_Values.Look(ref Setting, "setting", fileSetting);
            }
            if (Setting == Options.NVCustom)
            {
                if (Scribe.mode == LoadSaveMode.Saving)
                {
                    if (!LoadedFromFile
                            ||(LoadedFromFile && fileSetting != Options.NVCustom)
                            || (LoadedFromFile && fileSetting == Options.NVCustom
                                && ((zeroLightMod != null && !Mathf.Approximately((float)zeroLightMod, DefaultZeroLightMod))
                                    || (fullLightMod != null && !Mathf.Approximately((float)fullLightMod, DefaultFullLightMod)))))
                    {
                        Scribe_Values.Look(ref zeroLightMod, "zerolight", DefaultZeroLightMod);
                        Scribe_Values.Look(ref fullLightMod, "fulllight", DefaultFullLightMod);
                    }
                }
                else
                {
                    Scribe_Values.Look(ref zeroLightMod, "zerolight", DefaultZeroLightMod);
                    Scribe_Values.Look(ref fullLightMod, "fulllight", DefaultFullLightMod);
                }
            }
        }

        internal static void SaveOrLoadSettings()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                nvFullLightMod = RoundAndCheck(nvFullLightMod, false);
                nvZeroLightMod = RoundAndCheck(nvZeroLightMod, true);
                psFullLightMod = RoundAndCheck(psFullLightMod, false);
                psZeroLightMod = RoundAndCheck(psZeroLightMod, true);
            }
            Scribe_Values.Look(ref nvZeroLightMod, "NightVisionZeroLightModifier", DefaultNVZero);
            Scribe_Values.Look(ref nvFullLightMod, "NightVisionFullLightModifier", DefaultNVFull);
            Scribe_Values.Look(ref psZeroLightMod, "PhotosensitivityZeroLightModifier", DefaultPSZero);
            Scribe_Values.Look(ref psFullLightMod, "PhotosensitivityFullLightModifier", DefaultPSFull);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                nvFullLightMod = RoundAndCheck(nvFullLightMod, false);
                nvZeroLightMod = RoundAndCheck(nvZeroLightMod, true);
                psFullLightMod = RoundAndCheck(psFullLightMod, false);
                psZeroLightMod = RoundAndCheck(psZeroLightMod, true);
            }
        }
        #endregion
    }

    /// <summary>
    /// As GlowMods, but, for standard settings (NV & PS), divides the results by the set number of eyes
    /// </summary>
    public class EyeGlowMods : GlowMods
    {
        #region Instance Fields
        private float? nvZeroLightEyeMod;
        private float? nvFullLightEyeMod;
        private float? psZeroLightEyeMod;
        private float? psFullLightEyeMod;

        //Bi-Retinal Normative
        internal int NumOfEyesNormalisedFor = 2;

        //Used to check for changes to general NV and PS multipliers
        internal int EyeNVVersion;
        internal int EyePSVersion;
        #endregion

        #region Get Overrides
        internal override float NVZeroLightMod
        {
            get
            {
                if (nvZeroLightEyeMod == null || EyeNVVersion != GlowMods.NVVersion)
                {
                    FetchAndNormaliseNVMods();
                    EyeNVVersion = NVVersion;
                }
                return (float)nvZeroLightEyeMod;
            }
        }
        internal override float NVFullLightMod
        {
            get
            {
                if (nvFullLightEyeMod == null || EyeNVVersion != GlowMods.NVVersion)
                {
                    FetchAndNormaliseNVMods();
                    EyeNVVersion = NVVersion;
                }
                return (float)nvFullLightEyeMod;
            }
        }
        internal override float PSZeroLightMod
        {
            get
            {
                if (psZeroLightEyeMod == null || EyePSVersion != GlowMods.PSVersion)
                {
                    FetchAndNormalisePSMods();
                    EyePSVersion = PSVersion;
                }
                return (float)psZeroLightEyeMod;
            }
        }
        internal override float PSFullLightMod
        {
            get
            {
                if (psFullLightEyeMod == null || EyePSVersion != GlowMods.PSVersion)
                {
                    FetchAndNormalisePSMods();
                    EyePSVersion = PSVersion;
                }
                return (float)psFullLightEyeMod;
            }
        }
        #endregion

        #region Constructors
        internal EyeGlowMods(GlowMods racegm, int numeyes)
        {
            this.Setting = racegm.Setting;
            NumOfEyesNormalisedFor = numeyes;
            if (this.Setting == Options.NVCustom)
            {
                zeroLightMod = RoundAndCheck(racegm.ZeroLight, true, numeyes);
                fullLightMod = RoundAndCheck(racegm.FullLight, false, numeyes);
            }
        }
        
        internal EyeGlowMods(HediffCompProperties_NightVision compprops) : base(compprops) {}

        internal EyeGlowMods(Options setting) : base(setting) { }

        public EyeGlowMods() : base() { }
        #endregion

        #region Fetching from base class
        private void FetchAndNormaliseNVMods()
        {
            //Fetches values from base class
            nvZeroLightEyeMod = RoundAndCheck(nvZeroLightMod, true, NumOfEyesNormalisedFor);
            nvFullLightEyeMod = RoundAndCheck(nvFullLightMod, false, NumOfEyesNormalisedFor);
        }

        private void FetchAndNormalisePSMods()
        {
            //Fetches values from base class
            psZeroLightEyeMod = RoundAndCheck(GlowMods.psZeroLightMod, true, NumOfEyesNormalisedFor);
            psFullLightEyeMod = RoundAndCheck(GlowMods.psFullLightMod, false, NumOfEyesNormalisedFor);
        }
        #endregion
    }
}
