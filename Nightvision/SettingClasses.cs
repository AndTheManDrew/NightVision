using JetBrains.Annotations;
using NightVision.Comps;
using Verse;

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
        internal bool NullifiesPS;
        internal bool GrantsNV;
        //Settings in xml defs
        internal bool CompNullifiesPS;
        internal bool CompGrantsNV;
        #endregion

        #region Constructors And comp attacher
        /// <summary>
        /// Parameterless Constructor: necessary for RW scribe system
        /// </summary>
        [UsedImplicitly]
        public ApparelSetting() { }

        /// <summary>
        /// Manual Constructor: for when user instantiates new setting in mod settings
        /// </summary>
        internal ApparelSetting(bool nullPS, bool giveNv)
        {
            NullifiesPS = nullPS;
            GrantsNV = giveNv;
        }

        /// <summary>
        /// Constructor for Dictionary builder
        /// </summary>
        internal ApparelSetting(CompProperties_NightVisionApparel compprops)
        {
            CompNullifiesPS = compprops.NullifiesPhotosensitivity;
            CompGrantsNV = compprops.GrantsNightVision;
            NullifiesPS = CompNullifiesPS;
            GrantsNV = CompGrantsNV;
        }

        /// <summary>
        /// Dictionary builder attaches the comp settings to preexisting entries
        /// </summary>
        internal void AttachComp(CompProperties_NightVisionApparel compprops)
        {
            CompNullifiesPS = compprops.NullifiesPhotosensitivity;
            CompGrantsNV = compprops.GrantsNightVision;
        }
        #endregion

        #region Equality, Redundancy, and INVSaveCheck checks
        internal bool Equals(ApparelSetting other) => (GrantsNV == other.GrantsNV) && (NullifiesPS == other.NullifiesPS);

        /// <summary>
        /// Check to see if this setting should be removed from the dictionary, i.e. current and def values are all false
        /// </summary>
        internal bool IsRedundant() => !(GrantsNV || NullifiesPS) && !(CompGrantsNV || CompNullifiesPS);
        /// <summary>
        /// Check to see if this setting should be saved, i.e. current and def values are all false,
        /// or current values are equal to def values
        /// </summary>
        /// <returns></returns>
        public bool ShouldBeSaved()
        {
            return !(IsRedundant() || ((GrantsNV == CompGrantsNV) && (NullifiesPS == CompNullifiesPS)));
        }
        #endregion

        #region Expose Data
        public void ExposeData()
        {
            Scribe_Values.Look(ref NullifiesPS, "nullifiesphotosens", CompNullifiesPS);
            Scribe_Values.Look(ref GrantsNV, "grantsnightvis", CompGrantsNV);
        }
        #endregion
    }

    ///// <summary>
    ///// For storing modifiers to work and move speed light multipliers. 
    ///// Used to store settings for: hediffs, races, and individual pawns
    ///// </summary>
    //public class LightModifiers : IExposable, INVSaveCheck
    //{
    //    #region Default Constants
    //    internal const float DefaultNVZero = 0.2f;
    //    internal const float DefaultNVFull = 0.0f;
    //    internal const float DefaultPSZero = 0.4f;
    //    internal const float DefaultPSFull = -0.2f;
    //    #endregion

    //    public enum Options : byte
    //        {
    //            NVNone = 0,

    //            NVNightVision = 1,

    //            NVPhotosensitivity = 2,

    //            NVCustom = 3
    //        }

    //    #region Static Fields

    //    protected internal static float NvZeroLightMod = DefaultNVZero;
    //    protected internal static float nvFullLightMod = DefaultNVFull;
    //    protected internal static float PsZeroLightMod = DefaultPSZero;
    //    protected internal static float PsFullLightMod = DefaultPSFull;

    //    protected static int NVVersion;
    //    protected static int PSVersion;

    //    #endregion

    //    #region Instance Fields
    //    protected float? ZeroLightMod;
    //    protected float? FullLightMod;

    //    internal bool LoadedFromFile;
    //    internal Options FileSetting = Options.NVNone;
    //    private float _defaultZeroMod;
    //    private float _defaultFullMod;
    //    internal float DefaultZeroMod
    //    {
    //        get
    //        {
    //            switch (FileSetting)
    //            {
    //                default:
    //                    return 0f;
    //                case Options.NVNightVision:
    //                    return NVZeroLightMod;
    //                case Options.NVPhotosensitivity:
    //                    return PSZeroLightMod;
    //                case Options.NVCustom:
    //                    return _defaultZeroMod;
    //            }
    //        }
    //        set => _defaultZeroMod = value;
    //    }
    //    internal float DefaultFullMod
    //    {
    //        get
    //        {
    //            switch (FileSetting)
    //            {
    //                default:
    //                    return 0f;
    //                case Options.NVNightVision:
    //                    return NVFullLightMod;
    //                case Options.NVPhotosensitivity:
    //                    return PSFullLightMod;
    //                case Options.NVCustom:
    //                    return _defaultFullMod;
    //            }
    //        }
    //        set => _defaultFullMod = value;
    //    }


    //    internal Options Setting;
    //    #endregion

    //    #region Get And Set Static Fields
    //    // Seperate get and set as get needs to be virtual so EyeLightModifiers can override
    //    // and set needs to be static so it can be accessed from Settings
    //    internal virtual float NVZeroLightMod
    //    {
    //        get => NvZeroLightMod;
    //    }
    //    internal static float SetNVZeroLightMod
    //    {
    //        set { NvZeroLightMod = RoundAndCheck(value, true); NVVersion++; }
    //    }
    //    internal virtual float NVFullLightMod
    //    {
    //        get => nvFullLightMod;
    //    }
    //    internal static float SetNVFullLightMod
    //    {
    //        set { nvFullLightMod = RoundAndCheck(value, false); NVVersion++; }
    //    }
    //    internal virtual float PSZeroLightMod
    //    {
    //        get => PsZeroLightMod;
    //    }
    //    internal static float SetPSZeroLightMod
    //    {
    //        set { PsZeroLightMod = RoundAndCheck(value, true); PSVersion++; }
    //    }
    //    internal virtual float PSFullLightMod
    //    {
    //        get => PsFullLightMod;
    //    }
    //    internal static float SetPSFullLightMod
    //    {
    //        set { PsFullLightMod = RoundAndCheck(value, false); PSVersion++; }
    //    }
    //    #endregion
        
    //    #region Get And Set Instance Fields
    //    internal float ZeroLight
    //    {
    //        get
    //        {
    //            switch (Setting)
    //            {
    //                default:
    //                    return DefaultZeroMod;
    //                case Options.NVNightVision:
    //                    return NVZeroLightMod;
    //                case Options.NVPhotosensitivity:
    //                    return PSZeroLightMod;
    //                case Options.NVCustom:
    //                    return ZeroLightMod ?? DefaultZeroMod;
    //            }
    //        }
    //        set
    //        {
    //            ZeroLightMod = RoundAndCheck(value, true);
    //            Setting = Options.NVCustom;
    //        }
    //    }

    //    internal float FullLight
    //    {
    //        get
    //        {
    //            switch (Setting)
    //            {
                    
    //                default:
    //                    return DefaultFullMod;
    //                case Options.NVNightVision:
    //                    return NVFullLightMod;
    //                case Options.NVPhotosensitivity:
    //                    return PSFullLightMod;
    //                case Options.NVCustom:
    //                    return FullLightMod ?? DefaultFullMod;
    //            }
    //        }
    //        set
    //        {
    //            FullLightMod = RoundAndCheck(value, false);
    //            Setting = Options.NVCustom;
    //        }
    //    }

    //    #endregion

    //    #region Constructors

    //    /// <summary>
    //    /// Parameterless constructor
    //    /// </summary>
    //    public LightModifiers()
    //    {
    //        ZeroLightMod = null;
    //        FullLightMod = null;
    //        Setting = Options.NVNone;
    //    }
    //    /// <summary>
    //    /// Manual Contructor
    //    /// </summary>
    //    /// <param name="zerolight"></param>
    //    /// <param name="fulllight"></param>
    //    internal LightModifiers(float zerolight, float fulllight)
    //    {
    //        Setting = Options.NVCustom;
    //        ZeroLightMod = zerolight;
    //        FullLightMod = fulllight;
    //    }
    //    internal LightModifiers(Options setting)
    //    {
    //        ZeroLightMod = null;
    //        FullLightMod = null;
    //        Setting = setting;
    //    }
    //    /// <summary>
    //    /// Constructor for Races: NOT eye normalised
    //    /// </summary>
    //    /// <param name="compprops"></param>
    //    internal LightModifiers(CompProperties_NightVision compprops)
    //    {
    //        LoadedFromFile = true;
    //        if (compprops == null)
    //        {
    //            LoadedFromFile = false;
    //            Setting = Options.NVNone;
    //        }
    //        else if (compprops.NaturalNightVision)
    //        {
    //            Setting = Options.NVNightVision;
    //            FileSetting = Setting;
    //        }
    //        else if (compprops.NaturalPhotosensitivity)
    //        {
    //            Setting = Options.NVPhotosensitivity;
    //            FileSetting = Setting;
    //        }
    //        else
    //        {
    //            FileSetting = Options.NVCustom;
    //            DefaultZeroMod = RoundAndCheck(compprops.ZeroLightMultiplier - NightVisionSettings.DefaultZeroLightMultiplier, true);
    //            DefaultFullMod = RoundAndCheck(compprops.FullLightMultiplier - NightVisionSettings.DefaultFullLightMultiplier, false);
    //            Setting = FileSetting;
    //            ZeroLightMod = DefaultZeroMod;
    //            FullLightMod = DefaultFullMod;
    //        }
    //    }
    //    /// <summary>
    //    /// Constructor for Hediffs: if eye hediff then this will be called from within the EyeLightModifiers class
    //    /// - in that case custom settings should NOT be normalised
    //    /// </summary>
    //    /// <param name="compprops"></param>
    //    internal LightModifiers(HediffCompProperties_NightVision compprops)
    //    {
    //        LoadedFromFile = true;
    //        if (compprops == null)
    //        {
    //            LoadedFromFile = false;
    //            Setting = Options.NVNone;
    //        }
    //        else if (compprops.GrantsNightVision)
    //        {
    //            Setting = Options.NVNightVision;
    //            FileSetting = Setting;
    //        }
    //        else if (compprops.GrantsPhotosensitivity)
    //        {
    //            Setting = Options.NVPhotosensitivity;
    //            FileSetting = Setting;
    //        }
    //        else
    //        {
    //            FileSetting = Options.NVCustom;
    //            DefaultZeroMod = RoundAndCheck(compprops.ZeroLightMod, true);
    //            DefaultFullMod = RoundAndCheck(compprops.FullLightMod, false);
    //            Setting = FileSetting;
    //        }
    //    }
    //    #endregion

    //    #region Misc Helpers
    //    internal static float RoundAndCheck(float desiredValue, bool forZeroLight, int numNormalise = 1)
    //    {
    //        float resultingMultiplier = (forZeroLight ? NightVisionSettings.DefaultZeroLightMultiplier : NightVisionSettings.DefaultFullLightMultiplier)
    //                            + desiredValue;
    //        return (float)Math.Round((resultingMultiplier < 0.01f ? (-1f / numNormalise) + 0.01f : desiredValue / numNormalise), 2);
    //    }

    //    internal float GetEffectAtGlow(float glow)
    //    {
    //        if (Math.Abs(glow) < 0.01f)
    //        {
    //            return ZeroLight;
    //        }

    //        if(Math.Abs(glow - 1f) < 0.01f)
    //        {
    //            return FullLight;
    //        }

    //        if (glow < 0.3f)
    //        {
    //            return ZeroLight * (0.3f - glow) / 0.3f;
    //        }

    //        return FullLight * (glow - 0.7f) / 0.3f;
    //    }

    //    /// <returns>[maxcap, mincap, nvcap, pscap]</returns>
    //    internal static float[] GetCapsAtGlow(float glow)
    //    {
    //        float mincap;
    //        float maxcap;
    //        float nvcap;
    //        float pscap;
    //        if (glow < 0.3f)
    //        {
    //            mincap = (NightVisionSettings.MultiplierCaps.min - NightVisionSettings.DefaultZeroLightMultiplier)* (0.3f - glow) / 0.3f;
    //            maxcap = (NightVisionSettings.MultiplierCaps.max - NightVisionSettings.DefaultZeroLightMultiplier) * (0.3f - glow) / 0.3f;
    //            pscap = PsZeroLightMod * (0.3f - glow) / 0.3f;
    //            nvcap = NvZeroLightMod * (0.3f - glow) / 0.3f;
    //        }
    //        else
    //        {
    //            mincap = (NightVisionSettings.MultiplierCaps.min - NightVisionSettings.DefaultFullLightMultiplier) * (glow-0.7f) / 0.3f;
    //            maxcap = (NightVisionSettings.MultiplierCaps.max - NightVisionSettings.DefaultFullLightMultiplier) * (glow-0.7f) / 0.3f;
    //            pscap = PsFullLightMod * (glow - 0.7f) / 0.3f;
    //            nvcap = nvFullLightMod * (glow - 0.7f) / 0.3f;
    //        }
    //        return new[] { maxcap, mincap, nvcap, pscap };
    //    }
    //    #endregion

    //    #region Change Setting And Attach Comp
    //    internal void ChangeSetting(Options newsetting)
    //    {
    //        if(Setting != newsetting)
    //        {
    //            if ((newsetting == Options.NVCustom) && ((ZeroLightMod == null) || (FullLightMod == null)))
    //            {
    //                switch(Setting)
    //                {
    //                    case Options.NVNightVision:
    //                        ZeroLightMod = NVZeroLightMod;
    //                        FullLightMod = NVFullLightMod;
    //                        break;
    //                    case Options.NVPhotosensitivity:
    //                        ZeroLightMod = PSZeroLightMod;
    //                        FullLightMod = PSFullLightMod;
    //                        break;
    //                    default:
    //                        ZeroLightMod = 0f;
    //                        FullLightMod = 0f;
    //                        break;
    //                }
    //            }
                
    //            Setting = newsetting;
    //        }
    //    }
    //    /// <summary>
    //    /// For attaching xml race settings to existing settings
    //    /// </summary>
    //    /// <param name="compprops"></param>
    //    internal void AttachComp(CompProperties_NightVision compprops)
    //    {
    //        LoadedFromFile = true;
    //        if (compprops == null)
    //        {
    //            LoadedFromFile = false;
    //        }
    //        else if (compprops.NaturalNightVision)
    //        {
    //            FileSetting = Options.NVNightVision;
    //        }
    //        else if (compprops.NaturalPhotosensitivity)
    //        {
    //            FileSetting = Options.NVPhotosensitivity;
    //        }
    //        else
    //        {
    //            FileSetting = Options.NVCustom;
    //            DefaultZeroMod = RoundAndCheck(compprops.ZeroLightMultiplier - NightVisionSettings.DefaultZeroLightMultiplier, true);
    //            DefaultFullMod = RoundAndCheck(compprops.FullLightMultiplier - NightVisionSettings.DefaultFullLightMultiplier, false);
    //        }
    //    }
    //    /// <summary>
    //    /// For attaching xml hediff settings to existing settings
    //    /// </summary>
    //    /// <param name="compprops"></param>
    //    internal void AttachComp(HediffCompProperties_NightVision compprops)
    //    {
    //        LoadedFromFile = true;
    //        if (compprops == null)
    //        {
    //            LoadedFromFile = false;
    //        }
    //        else if (compprops.GrantsNightVision)
    //        {
    //            FileSetting = Options.NVNightVision;
    //        }
    //        else if (compprops.GrantsPhotosensitivity)
    //        {
    //            FileSetting = Options.NVPhotosensitivity;
    //        }
    //        else
    //        {
    //            FileSetting = Options.NVCustom;
    //            DefaultZeroMod = RoundAndCheck(compprops.ZeroLightMod, true);
    //            DefaultFullMod = RoundAndCheck(compprops.FullLightMod, false);
    //        }
    //    }
    //    #endregion

    //    #region Simple Checkers

    //    internal bool IsCustom()
    //    {
    //        return Setting == Options.NVCustom;
    //    }
    //    internal bool IsNone()
    //    {
    //        return Setting == Options.NVNone;
    //    }

    //    internal bool IsRedundant()
    //    {
    //        return (!LoadedFromFile && (((Setting == Options.NVCustom) && (Math.Abs(ZeroLightMod ?? 0) < 0.001) && (Math.Abs(FullLightMod ?? 0) < 0.001)) || (Setting == Options.NVNone)));
    //    }
    //    #endregion

    //    #region Save and load members
    //    public bool ShouldBeSaved()
    //    {
    //        return !(IsRedundant() || (LoadedFromFile && (Setting == FileSetting) && ((FileSetting != Options.NVCustom) ||
    //                        (((ZeroLightMod == null) || Mathf.Approximately((float)ZeroLightMod, DefaultZeroMod))
    //                                && ((FullLightMod == null) || Mathf.Approximately((float)FullLightMod, DefaultFullMod))))));
    //    }

    //    public void ExposeData()
    //    {
    //        //Only want to save the setting if it is different from the setting on file
    //        //so that changing the setting on file (i.e. in the mod) changes the setting in game if user has not altered it
    //        if (Scribe.mode == LoadSaveMode.Saving)
    //        {
    //            if(!LoadedFromFile || (LoadedFromFile && (Setting != FileSetting)))
    //            {
    //                Scribe_Values.Look(ref Setting, "setting", FileSetting);
    //            }
    //        }
    //        else
    //        {
    //            Scribe_Values.Look(ref Setting, "setting", FileSetting);
    //        }
    //        if (Setting == Options.NVCustom)
    //        {
    //            if (Scribe.mode == LoadSaveMode.Saving)
    //            {
    //                if (!LoadedFromFile
    //                        ||(LoadedFromFile && (FileSetting != Options.NVCustom))
    //                        || (LoadedFromFile && (FileSetting == Options.NVCustom)
    //                            && (((ZeroLightMod != null) && !Mathf.Approximately((float)ZeroLightMod, DefaultZeroMod))
    //                                || ((FullLightMod != null) && !Mathf.Approximately((float)FullLightMod, DefaultFullMod)))))
    //                {
    //                    Scribe_Values.Look(ref ZeroLightMod, "zerolight", DefaultZeroMod);
    //                    Scribe_Values.Look(ref FullLightMod, "fulllight", DefaultFullMod);
    //                }
    //            }
    //            else
    //            {
    //                Scribe_Values.Look(ref ZeroLightMod, "zerolight", DefaultZeroMod);
    //                Scribe_Values.Look(ref FullLightMod, "fulllight", DefaultFullMod);
    //            }
    //        }
    //    }

    //    internal static void SaveOrLoadSettings()
    //    {
    //        if (Scribe.mode == LoadSaveMode.Saving)
    //        {
    //            nvFullLightMod = RoundAndCheck(nvFullLightMod, false);
    //            NvZeroLightMod = RoundAndCheck(NvZeroLightMod, true);
    //            PsFullLightMod = RoundAndCheck(PsFullLightMod, false);
    //            PsZeroLightMod = RoundAndCheck(PsZeroLightMod, true);
    //        }
    //        Scribe_Values.Look(ref NvZeroLightMod, "NightVisionZeroLightModifier", DefaultNVZero);
    //        Scribe_Values.Look(ref nvFullLightMod, "NightVisionFullLightModifier", DefaultNVFull);
    //        Scribe_Values.Look(ref PsZeroLightMod, "PhotosensitivityZeroLightModifier", DefaultPSZero);
    //        Scribe_Values.Look(ref PsFullLightMod, "PhotosensitivityFullLightModifier", DefaultPSFull);

    //        if (Scribe.mode == LoadSaveMode.LoadingVars)
    //        {
    //            nvFullLightMod = RoundAndCheck(nvFullLightMod, false);
    //            NvZeroLightMod = RoundAndCheck(NvZeroLightMod, true);
    //            PsFullLightMod = RoundAndCheck(PsFullLightMod, false);
    //            PsZeroLightMod = RoundAndCheck(PsZeroLightMod, true);
    //        }
    //    }
    //    #endregion
    //}

    ///// <summary>
    ///// As LightModifiers, but, for standard settings (NV & PS), divides the results by the set number of eyes
    ///// </summary>
    //public class EyeLightModifiers : LightModifiers
    //{
    //    #region Instance Fields
    //    private float? _nvZeroLightEyeMod;
    //    private float? _nvFullLightEyeMod;
    //    private float? _psZeroLightEyeMod;
    //    private float? _psFullLightEyeMod;

    //    //Bi-Retinal Normative
    //    internal int NumOfEyesNormalisedFor = 2;

    //    //Used to check for changes to general NV and PS multipliers
    //    internal int EyeNVVersion;
    //    internal int EyePSVersion;
    //    #endregion

    //    #region Get Overrides
    //    internal override float NVZeroLightMod
    //    {
    //        get
    //        {
    //            if ((_nvZeroLightEyeMod != null) && (EyeNVVersion == NVVersion))
    //            {
    //                return (float) _nvZeroLightEyeMod;
    //            }
    //            FetchAndNormaliseNVMods();
    //            EyeNVVersion = NVVersion;
    //            return (float)_nvZeroLightEyeMod;
    //        }
    //    }
    //    internal override float NVFullLightMod
    //    {
    //        get
    //        {
    //            if ((_nvFullLightEyeMod != null) && (EyeNVVersion == NVVersion))
    //                {
    //                    return (float) _nvFullLightEyeMod;
    //                }

    //            FetchAndNormaliseNVMods();
    //            EyeNVVersion = NVVersion;
    //            return (float)_nvFullLightEyeMod;
    //        }
    //    }
    //    internal override float PSZeroLightMod
    //    {
    //        get
    //        {
    //            if ((_psZeroLightEyeMod != null) && (EyePSVersion == PSVersion))
    //                {
    //                    return (float) _psZeroLightEyeMod;
    //                }

    //            FetchAndNormalisePSMods();
    //            EyePSVersion = PSVersion;
    //            return (float)_psZeroLightEyeMod;
    //        }
    //    }
    //    internal override float PSFullLightMod
    //    {
    //        get
    //        {
    //            if ((_psFullLightEyeMod != null) && (EyePSVersion == PSVersion))
    //                {
    //                    return (float) _psFullLightEyeMod;
    //                }

    //            FetchAndNormalisePSMods();
    //            EyePSVersion = PSVersion;
    //            return (float)_psFullLightEyeMod;
    //        }
    //    }
    //    #endregion

    //    #region Constructors
    //    internal EyeLightModifiers(LightModifiers racegm, int numeyes)
    //    {
    //        Setting = racegm.Setting;
    //        NumOfEyesNormalisedFor = numeyes;
    //        if (Setting == Options.NVCustom)
    //        {
    //            ZeroLightMod = RoundAndCheck(racegm.ZeroLight, true, numeyes);
    //            FullLightMod = RoundAndCheck(racegm.FullLight, false, numeyes);
    //        }
    //        if (racegm.LoadedFromFile)
    //        {
    //            LoadedFromFile = true;
    //            FileSetting = racegm.FileSetting;
    //        }
    //    }
        
    //    internal EyeLightModifiers(LightModifiers hediffMod)
    //    {
    //        Setting = hediffMod.Setting;
    //        if (Setting == Options.NVCustom)
    //        {
    //            ZeroLightMod = RoundAndCheck(hediffMod.ZeroLight, true);
    //            FullLightMod = RoundAndCheck(hediffMod.FullLight, false);
    //        }
    //    }

    //    internal EyeLightModifiers(HediffCompProperties_NightVision compprops) : base(compprops) {}

    //    internal EyeLightModifiers(Options setting) : base(setting) { }

    //    public EyeLightModifiers()
    //    { }
    //    #endregion

    //    #region Fetching from base class
    //    private void FetchAndNormaliseNVMods()
    //    {
    //        //Fetches values from base class
    //        _nvZeroLightEyeMod = RoundAndCheck(NvZeroLightMod, true, NumOfEyesNormalisedFor);
    //        _nvFullLightEyeMod = RoundAndCheck(nvFullLightMod, false, NumOfEyesNormalisedFor);
    //    }

    //    private void FetchAndNormalisePSMods()
    //    {
    //        //Fetches values from base class
    //        _psZeroLightEyeMod = RoundAndCheck(PsZeroLightMod, true, NumOfEyesNormalisedFor);
    //        _psFullLightEyeMod = RoundAndCheck(PsFullLightMod, false, NumOfEyesNormalisedFor);
    //    }
    //    #endregion
    //}
}
