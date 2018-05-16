﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace NightVision.Comps
{
    public class Comp_NightVision : ThingComp
    {
        #region Constants
        public const string EyeTag = "SightSource";
        public const string BrainTag = "ConsciousnessSource";
        public const string BodyKey = "WholeBody";
        
        public const float MinGlowNoGlow = 0.3f;
        public const float MaxGlowNoGlow = 0.7f;
        
        private const string ModifierLine = "    {0} : {1,6:+#0.0%;-#0.0%;0%}";
        private const string MultiplierLine = "    {0} : {1,6:x#0.0%;x#0.0%;x0%}";
        private const string Maxline = "     {0} {1} : {2,6:x#0.0%;x#0.0%;x0%}";
        #endregion

        #region Private Fields
        private int _numRemainingEyes = -1;
        private Pawn _intParentPawn;
        private List<BodyPartRecord> _raceSightParts;
        private Race_LightModifiers _naturalLightModifiers;
        internal float[] HediffMods = new float[2];
        internal float[] PshediffMods = new float[2];
        internal float[] NvhediffMods = new float[2];
        private string _brainName;

        private bool _hediffsNeedChecking;
        private bool _apparelNeedsChecking;
        private float _zeroLightModifier = -1;
        private float _fullLightModifier = -1;
        private List<Hediff> _pawnsHediffs;

        private static LightModifiers NVModifiers;
        #endregion

        #region Public Fields
        [UsedImplicitly]
        public CompProperties_NightVision Props
        {
            get { return (CompProperties_NightVision) props; }
        }

        public List<Apparel> PawnsNVApparel = new List<Apparel>();
        public Dictionary<string, List<HediffDef>> PawnsNVHediffs = new Dictionary<string, List<HediffDef>>();
        public bool ApparelGrantsNV;
        public bool ApparelNullsPS;
        #endregion

        #region Pawn Details

        public Pawn ParentPawn
        {
            get { return _intParentPawn ?? (_intParentPawn = parent as Pawn); }
        }
        
        public List<Hediff> PawnHediffs
        {
            get { return _pawnsHediffs ?? (_pawnsHediffs = ParentPawn.health?.hediffSet?.hediffs); }
        }
        public List<BodyPartRecord> RaceSightParts
        {
            get
            {
                if (_raceSightParts.NullOrEmpty())
                {
                    _raceSightParts = ParentPawn.RaceProps.body.AllParts.Where(part => part.def.tags.Contains(EyeTag)).ToList();
                    if (_raceSightParts.NullOrEmpty())
                    {
                        Log.Message($"{ParentPawn.LabelShort}'s race has no eyes. The NightVision Comp should not be attached.");
                        Log.Message("Creating an empty list for their races eyes but NV will not have any effect");
                        return new List<BodyPartRecord>();
                    }
                }
                return _raceSightParts;
            }
        }

        /// <summary>
        /// Get: returns the number of pawns RACE's eyes if private float numRemainingEyes is negative
        /// Set: If the value is less than 0; will set = 0;
        /// </summary>
        public int NumberOfRemainingEyes
        {
            get
            {
                if (_numRemainingEyes < 0)
                {
                    _numRemainingEyes = RaceSightParts.Count;
                }
                return _numRemainingEyes;
            }
            set
            {
                _numRemainingEyes = (value < 0)? 0 : value;
            }
        }

        #endregion

        #region Natural Glow Mods from race

        /// <summary>
        /// The glow mods of the pawns races eyes
        /// Null Checks, if fails gets eye glow mods normalised for race's number of eyes
        /// </summary>
        public Race_LightModifiers NaturalLightModifiers
        {
            get
            {
                if (_naturalLightModifiers == null)
                {
                    _naturalLightModifiers = NightVisionSettings.GetRaceNightVisionMod(ParentPawn.def, RaceSightParts.Count);
                    _zeroLightModifier = -1f;
                    _fullLightModifier = -1f;
                }
                return _naturalLightModifiers;
            }
        }
        #endregion

        #region Modifier Calculators

        private float CalcZeroLightModifier()
        {
            float mod = NightVisionSettings.DefaultZeroLightMultiplier;
            byte setting = (byte)NaturalLightModifiers.Setting;
            switch (setting)
            {
                
                default:
                    mod += Mathf.Clamp(
                                NvhediffMods[0],
                                Math.Min(0f, LightModifiers.NvZeroLightMod),
                                Math.Max(0f, LightModifiers.NvZeroLightMod))
                            + Mathf.Clamp(
                                PshediffMods[0],
                                Math.Min(0f, LightModifiers.PsZeroLightMod),
                                Math.Max(0f, LightModifiers.PsZeroLightMod))
                            + HediffMods[0];
                    break;
                case 1: //Nightvision
                    mod += Mathf.Clamp(
                                NvhediffMods[0] + NaturalLightModifiers.ZeroLight * _numRemainingEyes,
                                Math.Min(0f, LightModifiers.NvZeroLightMod),
                                Math.Max(0f, LightModifiers.NvZeroLightMod))
                            + Mathf.Clamp(
                                PshediffMods[0],
                                Math.Min(0f, LightModifiers.PsZeroLightMod),
                                Math.Max(0f, LightModifiers.PsZeroLightMod))
                            + HediffMods[0];
                    break;
                case 2: // Photosensitive
                    mod += Mathf.Clamp(
                                NvhediffMods[0],
                                Math.Min(0f, LightModifiers.NvZeroLightMod),
                                Math.Max(0f, LightModifiers.NvZeroLightMod))
                            + Mathf.Clamp(
                                PshediffMods[0] + NaturalLightModifiers.ZeroLight * _numRemainingEyes,
                                Math.Min(0f, LightModifiers.PsZeroLightMod),
                                Math.Max(0f, LightModifiers.PsZeroLightMod))
                            + HediffMods[0];
                    break;
                case 3: //Custom
                    mod += Mathf.Clamp(
                                NvhediffMods[0],
                                Math.Min(0f, LightModifiers.NvZeroLightMod),
                                Math.Max(0f, LightModifiers.NvZeroLightMod))
                            + Mathf.Clamp(
                                PshediffMods[0],
                                Math.Min(0f, LightModifiers.PsZeroLightMod),
                                Math.Max(0f, LightModifiers.PsZeroLightMod))
                            + HediffMods[0] + NaturalLightModifiers.ZeroLight * _numRemainingEyes;
                    break;
            }
            return (float)Math.Round(
                                        Mathf.Clamp(mod,
                                                    NightVisionSettings.MultiplierCaps.min,
                                                    NightVisionSettings.MultiplierCaps.max)
                                            - NightVisionSettings.DefaultZeroLightMultiplier,
                                        2);
        }

        private float CalcFullLightModifier()
        {
            float mod = NightVisionSettings.DefaultFullLightMultiplier;
            byte setting = (byte)NaturalLightModifiers.Setting;
            switch (setting)
            {
                default:
                    mod += Mathf.Clamp(
                                NvhediffMods[1],
                                Math.Min(0f, LightModifiers.nvFullLightMod),
                                Math.Max(0f, LightModifiers.nvFullLightMod))
                            + Mathf.Clamp(
                                PshediffMods[1],
                                Math.Min(0f, LightModifiers.PsFullLightMod),
                                Math.Max(0f, LightModifiers.PsFullLightMod))
                            + HediffMods[1];
                    break;
                case 1: //Nightvision
                    mod += Mathf.Clamp(
                                NvhediffMods[1] + NaturalLightModifiers.FullLight * _numRemainingEyes,
                                Math.Min(0f, LightModifiers.nvFullLightMod),
                                Math.Max(0f, LightModifiers.nvFullLightMod))
                            + Mathf.Clamp(
                                PshediffMods[1],
                                Math.Min(0f, LightModifiers.PsFullLightMod),
                                Math.Max(0f, LightModifiers.PsFullLightMod))
                            + HediffMods[1];
                    break;
                case 2: // Photosensitive
                    mod += Mathf.Clamp(
                                NvhediffMods[1],
                                Math.Min(0f, LightModifiers.nvFullLightMod),
                                Math.Max(0f, LightModifiers.nvFullLightMod))
                            + Mathf.Clamp(
                                PshediffMods[1] + NaturalLightModifiers.FullLight * _numRemainingEyes,
                                Math.Min(0f, LightModifiers.PsFullLightMod),
                                Math.Max(0f, LightModifiers.PsFullLightMod))
                            + HediffMods[1];
                    break;
                case 3: //Custom
                    mod += Mathf.Clamp(
                                NvhediffMods[1],
                                Math.Min(0f, LightModifiers.nvFullLightMod),
                                Math.Max(0f, LightModifiers.nvFullLightMod))
                            + Mathf.Clamp(
                                PshediffMods[1],
                                Math.Min(0f, LightModifiers.PsFullLightMod),
                                Math.Max(0f, LightModifiers.PsFullLightMod))
                            + HediffMods[1] + NaturalLightModifiers.FullLight * _numRemainingEyes;
                    break;
            }
            return (float)Math.Round(
                                        Mathf.Clamp(mod,
                                                    NightVisionSettings.MultiplierCaps.min,
                                                    NightVisionSettings.MultiplierCaps.max) 
                                            - NightVisionSettings.DefaultFullLightMultiplier,
                                        2);
        }
        #endregion

        #region Light Modifiers

        public float ZeroLightModifier
        {
            get
            {
                //true on init, hediffs changed, apparel changed or settings changed
                if (_zeroLightModifier < -0.99f)
                {
                    //Also gets calculated every rare tick
                    if (_apparelNeedsChecking)
                    {
                        QuickRecheckApparel();
                        _apparelNeedsChecking = false;
                    }
                    //Also gets calculated every rare tick
                    if (_hediffsNeedChecking)
                    {
                        CalculateHediffMod();
                        _hediffsNeedChecking = false;
                    }
                    //Also gets calculated every rare tick
                    _zeroLightModifier = CalcZeroLightModifier();

                }
                if (ApparelGrantsNV && _zeroLightModifier + 0.001f < LightModifiers.NvZeroLightMod)
                {
                    return LightModifiers.NvZeroLightMod;
                }
                return _zeroLightModifier;
            }
        }
        public float FullLightModifier
        {
            get
            {
                //true on init, hediffs changed, apparel changed or settings changed
                if (_fullLightModifier < - 0.99f)
                {
                    //Also gets calculated every rare tick
                    if (_apparelNeedsChecking)
                    {
                        QuickRecheckApparel();
                        _apparelNeedsChecking = false;
                    }
                    //Also gets calculated every rare tick
                    if (_hediffsNeedChecking)
                    {
                        CalculateHediffMod();
                        _hediffsNeedChecking = false;
                    }
                    //Also gets calculated every rare tick
                    _fullLightModifier = CalcFullLightModifier();
                }
                if (ApparelNullsPS && _fullLightModifier + 0.001f < 0f)
                {
                    return 0f;
                }
                return _fullLightModifier;
            }
        }
        #endregion

        #region Output to harmony patches
        /// <summary>
        /// To calculate the effects of light on movement speed and work speed:
        /// Returns a multiplier
        /// </summary>
        public float FactorFromGlow(float glow)
        {
            //If glow is approx. 0%
            if (Math.Abs(glow) < 0.005)
            {
                return (float)Math.Round(NightVisionSettings.DefaultZeroLightMultiplier + ZeroLightModifier);
            }
            //If glow is approx. 100% and the pawns full light modifier is not approx 0

            if (Math.Abs(glow - 1f) < 0.005)
            {
                if (Math.Abs(FullLightModifier) > 0.005f)
                {
                    return (float)Math.Round(NightVisionSettings.DefaultFullLightMultiplier + FullLightModifier);
                }
                return 1f;
            }
            //Else linear interpolation

            if (glow < MinGlowNoGlow)
            {
                return (float)Math.Round(1f + (MinGlowNoGlow - glow) * (ZeroLightModifier - 0.2f) / 0.3f, 2);
            }

            if (glow > MaxGlowNoGlow && Math.Abs(FullLightModifier) > 0.01f)
            {
                return (float)Math.Round(1f + (glow - MaxGlowNoGlow) * FullLightModifier / 0.3f, 2);
            }
            return 1f;
        }

        /// <summary>
        /// For the pawn's stat inspect tab; incredibly fat
        /// </summary>
        /// <param name="result"></param>
        /// <param name="glow"></param>
        /// <param name="usedApparelSetting"></param>
        /// <returns></returns>
        public string ExplanationBuilder(string result, float glow, out bool usedApparelSetting)
        {
            float nvsum = 0f;
            float pssum = 0f;
            float sum = 0f;
            float[] caps = LightModifiers.GetCapsAtGlow(glow);
            bool foundSomething = false;
            float effect;
            float basevalue = 0f;
            bool lowLight = glow < 0.3f;
            usedApparelSetting = false;

            StringBuilder explanation = new StringBuilder(result);
            StringBuilder nvexplanation = new StringBuilder().Append(LightModifiers.Options.NVNightVision.ToString().Translate());
            StringBuilder psexplanation = new StringBuilder().AppendFormat(LightModifiers.Options.NVPhotosensitivity.ToString().Translate());

            explanation.AppendLine();
            if (lowLight)
            {
                effect = NightVisionSettings.DefaultFullLightMultiplier + (NightVisionSettings.DefaultZeroLightMultiplier -  NightVisionSettings.DefaultFullLightMultiplier) * (0.3f - glow) / 0.3f;
                if (Math.Abs(effect) >= 0.005)
                {
                    explanation.AppendFormat(MultiplierLine, "StatsReport_BaseValue".Translate(), effect);
                    explanation.AppendLine();
                    basevalue = effect;
                }
                if (ApparelGrantsNV)
                {
                    foundSomething = true;
                }
                
            }
            else
            {
                explanation.AppendFormat(MultiplierLine, "StatsReport_BaseValue".Translate(), 1f);
                explanation.AppendLine();
                basevalue = NightVisionSettings.DefaultFullLightMultiplier;
                if (ApparelNullsPS)
                {
                    foundSomething = true;
                }
            }
            if (!NaturalLightModifiers.IsNone())
            {
                effect = NaturalLightModifiers.GetEffectAtGlow(glow);
                if (Math.Abs(effect) >= 0.005)
                {
                    foundSomething = true;
                    switch (NaturalLightModifiers.Setting)
                    {
                        case LightModifiers.Options.NVNightVision:
                            nvsum += effect * NumberOfRemainingEyes;
                            nvexplanation.AppendFormat(ModifierLine, ParentPawn.def.LabelCap + " x" + NumberOfRemainingEyes, effect * NumberOfRemainingEyes);
                            break;
                        case LightModifiers.Options.NVPhotosensitivity:
                            pssum += effect * NumberOfRemainingEyes;
                            psexplanation.AppendFormat(ModifierLine, ParentPawn.def.LabelCap + " x" + NumberOfRemainingEyes, effect * NumberOfRemainingEyes);
                            break;
                        case LightModifiers.Options.NVCustom:
                            sum += effect * NumberOfRemainingEyes;
                            explanation.AppendFormat(ModifierLine, ParentPawn.def.LabelCap + " x" + NumberOfRemainingEyes, effect * NumberOfRemainingEyes);
                            break;
                    }
                    explanation.AppendLine();
                }
            }
            foreach (List<HediffDef> value in PawnsNVHediffs.Values)
            {
                if (value.NullOrEmpty())
                {
                    continue;
                }
                foreach (var hediffDef in value)
                {
                    if (NightVisionSettings.HediffLightMods.TryGetValue(hediffDef, out LightModifiers hediffSetting) && !hediffSetting.IsNone())
                    {
                        effect = hediffSetting.GetEffectAtGlow(glow);
                        if (Math.Abs(effect) > 0.005)
                        {
                            foundSomething = true;
                            switch (hediffSetting.Setting)
                            {
                                case LightModifiers.Options.NVNightVision:
                                    nvsum += effect;
                                    nvexplanation.AppendFormat(ModifierLine, hediffDef.LabelCap, effect);
                                    break;
                                case LightModifiers.Options.NVPhotosensitivity:
                                    pssum += effect;
                                    psexplanation.AppendFormat(ModifierLine, hediffDef.LabelCap, effect);
                                    break;
                                case LightModifiers.Options.NVCustom:
                                    sum += effect;
                                    explanation.AppendFormat(ModifierLine, hediffDef.LabelCap, effect);
                                    break;
                            }
                            explanation.AppendLine();
                        }
                    }
                }
            }
            
            if (foundSomething)
            {

                if (Math.Abs(nvsum) > 0.005f)
                {
                    explanation.AppendLine();
                    explanation.Append(nvexplanation);
                    if (Math.Abs(nvsum) > Math.Abs(caps[2]))
                    {
                        explanation.AppendLine();
                        explanation.AppendFormat(Maxline, "", "max".Translate(), caps[2]);
                    }
                    explanation.AppendLine();
                }
                if (Math.Abs(pssum) > 0.005f)
                {
                    explanation.AppendLine();
                    explanation.Append(psexplanation);
                    if (Math.Abs(pssum) > Math.Abs(caps[3]))
                    {
                        explanation.AppendLine();
                        explanation.AppendFormat(Maxline, "", "max".Translate(), caps[3]);
                    }
                    explanation.AppendLine();
                }
                sum += pssum + nvsum;
                if (Math.Abs(sum) > 0.005f)
                {
                    explanation.AppendLine();
                    explanation.AppendFormat(ModifierLine, "NVTotal".Translate() + " " + "NVmodifier".Translate().ToUpper(), sum);
                    explanation.AppendLine();
                    explanation.AppendFormat(MultiplierLine, "multiplier".Translate() + ":", sum + basevalue);

                    explanation.AppendLine();
                }
                if ((sum - 0.001f > caps[0])|| (sum + 0.001f < caps[1]))
                {
                    explanation.AppendLine();
                    explanation.AppendFormat(Maxline, "NVTotal".Translate(), "max".Translate(), (sum + basevalue) > caps[0] ? caps[0] : caps[1]);

                    explanation.AppendLine();
                }
                if (lowLight & ApparelGrantsNV & sum + 0.001f < caps[2])
                {
                    explanation.AppendLine();
                    explanation.Append("NVGearPresent".Translate($"{basevalue + caps[2]:0%}"));
                    usedApparelSetting = true;
                }
                else if (ApparelNullsPS &  sum + 0.001f < 0)
                {
                    explanation.AppendLine();
                    explanation.Append("PSGearPresent".Translate($"{NightVisionSettings.DefaultFullLightMultiplier:0%}"));
                    usedApparelSetting = true;
                }

                //explanation.AppendFormat("    {0}  {1,6:x#0.#%;x#0.#%;x0%}", "NVTotal".Translate(), sum);

                return explanation.ToString();
            }
            return result;
        }

        /// <summary>
        /// For ThoughtWorker_Dark patch
        /// </summary>
        /// <returns></returns>
        public LightModifiers.Options PsychDark()
        {
            if (ZeroLightModifier < 0.01f)
            {
                return LightModifiers.Options.NVNone;
            }

            //if nightvis and photosens have equal bonuses at zero % light
            if (Math.Abs(LightModifiers.NvZeroLightMod - LightModifiers.PsZeroLightMod) < 0.01f)
            {
                //if equal to photosensitivity bonus ( or nightvis bonus)
                if (Math.Abs(ZeroLightModifier - LightModifiers.PsZeroLightMod) < 0.01f)
                {
                    return LightModifiers.Options.NVPhotosensitivity;
                }
                //otherwise if greater than min 0.2 (equivalent to 100% mv&wrk speed at 0% light) or half the photosens bonus 

                if (ZeroLightModifier > Math.Min(LightModifiers.DefaultNVZero, LightModifiers.NvZeroLightMod / 2) - 0.01f)
                {
                    return LightModifiers.Options.NVNightVision;
                }
            }
            else
            {
                float lower = Math.Min(LightModifiers.NvZeroLightMod, LightModifiers.PsZeroLightMod);
                float upper = Math.Max(LightModifiers.NvZeroLightMod, LightModifiers.PsZeroLightMod);
                //if greater than the midpoint between the two bonuses
                if (ZeroLightModifier > ((lower + upper) / 2) - 0.01f)
                {
                    return LightModifiers.Options.NVPhotosensitivity;
                }
                //if greater than the midpoint between no bonus and the lower bonus
                if (ZeroLightModifier > (lower / 2) - 0.01f)
                {
                    return LightModifiers.Options.NVNightVision;
                }

            }
            return LightModifiers.Options.NVNone;
        }
        #endregion

        #region Dirtifier
        public void SetDirty()
        {
            _naturalLightModifiers = null;
            _zeroLightModifier = -1f;
            _fullLightModifier = -1f;
            _apparelNeedsChecking = true;
            _hediffsNeedChecking = true;
        }
        #endregion

        #region Comp overrides
        
        public override void CompTickRare()
        {
            if (!ParentPawn.Spawned || ParentPawn.Dead)
            {
                return;
            }
            if (_apparelNeedsChecking)
            {
                QuickRecheckApparel();
                _apparelNeedsChecking = false;
            }
            if (_hediffsNeedChecking)
            {
                CalculateHediffMod();
                _hediffsNeedChecking = false;
            }
            if (_fullLightModifier < -0.99f)
            {
                _fullLightModifier = CalcFullLightModifier();
            }
            if (_zeroLightModifier < -0.99f)
            {
                _zeroLightModifier = CalcZeroLightModifier();
            }
            if (NightVisionSettings.LogPawnComps)
            {
                Log.Message(new string('*', 30));
                Log.Message("NightVisionComp: " + ParentPawn.NameStringShort);
                Log.Message("Hediffs: ");
                foreach (var hedifflist in PawnsNVHediffs)
                {
                    Log.Message(new string('-', 20));
                    Log.Message(hedifflist.Key + "has:");
                    foreach (var hediff in hedifflist.Value)
                    {
                        Log.Message(hediff.LabelCap);
                    }
                }
                Log.Message(new string('-', 20));
                Log.Message("NumberRemainingEyes: " + NumberOfRemainingEyes);
                Log.Message("HediffLightModifiers: zero = " + HediffMods[0] + PshediffMods[0] + NvhediffMods[0] + ", full = " + HediffMods[0] + PshediffMods[0] + NvhediffMods[0]);
                Log.Message("NaturalLightModifiers: zero = " + NaturalLightModifiers.ZeroLight + ", full = " + NaturalLightModifiers.FullLight);
                Log.Message("Total Glow Mods: zero = " + ZeroLightModifier + ", full = " + FullLightModifier);
                Log.Message(new string('*', 30));
            }
            
        }

        //Note: don't save this comp so this only gets called when spawning new pawn
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            InitPawnsHediffsAndCountEyes();
            InitPawnsApparel();
        }
        //props aren't actually used here, they are loaded in the dictionary on start-up
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            //Note: Pawn.Dead is equivalent to ParentPawn.health.Dead, therefore null check
            if (parent == null || !ParentPawn.Spawned || ParentPawn.health == null || ParentPawn.Dead)
            {
                return;
            }
            InitPawnsHediffsAndCountEyes();
            InitPawnsApparel();
        }
        #endregion

        #region Initialisers
        /// <summary>
        /// Builds a dictionary to store all the possibly relevant hediffs:
        /// <para>Keys: Names of all sight parts, brain and "wholebody" for hediffs that do not affect a specific part</para>
        /// <para>Values: List of all the hediff defs that affect that part</para>
        /// </summary>
        private void InitPawnsHediffsAndCountEyes()
        {
            if (!ParentPawn.Spawned || ParentPawn.health == null || ParentPawn.Dead)
            {
                return;
            }
            NumberOfRemainingEyes = RaceSightParts.Count;
            PawnsNVHediffs.Clear();
            for (int i = 0; i < NumberOfRemainingEyes; i++)
            {
                PawnsNVHediffs[RaceSightParts[i].def.defName] = new List<HediffDef>();
            }
            //not really necessary to do this any more
            _brainName = ParentPawn.RaceProps.body.GetPartsWithTag(BrainTag).First().def.defName;
            PawnsNVHediffs[_brainName] = new List<HediffDef>();
            PawnsNVHediffs[BodyKey] = new List<HediffDef>();
            if (!PawnHediffs.NullOrEmpty())
            {
                foreach (var hediff in PawnHediffs)
                {
                    CheckAndAddHediff(hediff, hediff.Part);
                }
            }
            CalculateHediffMod();
        }

        public void InitPawnsApparel()
        {
            if (!ParentPawn.Spawned || ParentPawn.health == null || ParentPawn.Dead)
            {
                return;
            }
            ApparelGrantsNV = false;
            ApparelNullsPS = false;
            PawnsNVApparel = new List<Apparel>();
            if (!(ParentPawn.apparel?.WornApparel is List<Apparel> pawnsApparel))
            {
                return;
            }
            foreach (var apparel in pawnsApparel)
            {
                if (NightVisionSettings.NVApparel.TryGetValue(apparel.def, out ApparelSetting value))
                {
                    ApparelGrantsNV |= value.GrantsNV;
                    ApparelNullsPS |= value.NullifiesPS;
                    PawnsNVApparel.Add(apparel);
                }
                else if (NightVisionSettings.AllEyeCoveringHeadgearDefs.Contains(apparel.def))
                {
                    PawnsNVApparel.Add(apparel);
                }
            }
        }
        #endregion

        #region Hediff Updates
        public void RemoveHediff(Hediff hediff, BodyPartRecord part = null)
        {
            if (!ParentPawn.Spawned || ParentPawn.health == null || ParentPawn.Dead)
            {
                return;
            }
            if (part != null && PawnsNVHediffs.ContainsKey(part.def.defName) && PawnsNVHediffs[part.def.defName].Remove(hediff.def))
            {
                CalculateHediffMod();
                if(part.def.tags.Contains(EyeTag) && (hediff is Hediff_MissingPart || (hediff.def.addedPartProps is AddedBodyPartProps abpp && abpp.isSolid)))
                {
                    NumberOfRemainingEyes++;
                }
            }
            else if (PawnsNVHediffs[BodyKey].Remove(hediff.def))
            {
                CalculateHediffMod();
            }
        }
        public void CheckAndAddHediff(Hediff hediff, BodyPartRecord part = null)
        {
            if (ParentPawn.Dead || !ParentPawn.Spawned)
            {
                return;
            }
            if (part != null)
            {
                string partName = part.def.defName;
                if (PawnsNVHediffs.TryGetValue(partName, out List<HediffDef> tempPartsHediffDefs))
                {
                    //Categorise the hediff:
                    //MissingPart overrides everything
                    bool removedPart = false;

                    if (hediff is Hediff_MissingPart)
                    {
                        removedPart = true;
                        tempPartsHediffDefs = new List<HediffDef>
                        {
                            HediffDefOf.MissingBodyPart
                        };
                    }
                    else
                    {
                        //Check if there is a setting for it
                        HediffDef hediffDef = hediff.def;
                        if (NightVisionSettings.HediffLightMods.ContainsKey(hediffDef))
                        {
                            if (hediffDef.addedPartProps is AddedBodyPartProps abpp && abpp.isSolid)
                            {
                                removedPart = true;
                                tempPartsHediffDefs = new List<HediffDef>
                                {
                                    hediffDef
                                };  
                            }
                            else if (!tempPartsHediffDefs.Contains(hediffDef))
                            {
                                tempPartsHediffDefs.Add(hediffDef);
                            }
                        }
                        //Last two checks are in case the user changes settings later
                        //Is it a solid replacement?
                        else if (hediffDef.addedPartProps is AddedBodyPartProps abpp && abpp.isSolid)
                        {
                            removedPart = true;
                            tempPartsHediffDefs = new List<HediffDef>
                            {
                                hediffDef
                            };
                        }
                        //Check if it is a valid hediff
                        else if (NightVisionSettings.AllSightAffectingHediffs.Contains(hediffDef))
                        {
                             if (!tempPartsHediffDefs.Contains(hediffDef))
                             {
                                tempPartsHediffDefs.Add(hediffDef);
                             }
                        }
                    }
                    //remove an eye
                    if (removedPart && part.def.tags.Contains(EyeTag))
                    {
                        NumberOfRemainingEyes--;
                    }
                    PawnsNVHediffs[partName] = tempPartsHediffDefs;
                    CalculateHediffMod();
                }
                else if (NightVisionSettings.AllSightAffectingHediffs.Contains(hediff.def))
                {
                    PawnsNVHediffs[partName] = new List<HediffDef>
                            {
                                hediff.def
                            };
                }
            }
            else if (NightVisionSettings.AllSightAffectingHediffs.Contains(hediff.def))
            {
                 if (PawnsNVHediffs.TryGetValue(BodyKey, out List<HediffDef> value) && !value.Contains(hediff.def))
                 {
                    PawnsNVHediffs[BodyKey].Add(hediff.def);
                    CalculateHediffMod();
                 }
            }
        }

        #endregion

        #region Hediff GlowMod Calculator
        private void CalculateHediffMod()
        {
            float zeroMod= 0f;
            float fullMod = 0f;
            float psZeroMod = 0f;
            float psFullMod = 0f;
            float nvZeroMod = 0f;
            float nvFullMod = 0f;
            foreach (List<HediffDef> value in PawnsNVHediffs.Values)
            {
                if (value.NullOrEmpty())
                {
                    continue;
                }
                foreach (var hediffDef in value)
                {
                    if (NightVisionSettings.HediffLightMods.TryGetValue(hediffDef, out LightModifiers hediffSetting))
                    {
                        switch (hediffSetting.Setting)
                        {
                            case LightModifiers.Options.NVNone:
                                continue;
                            case LightModifiers.Options.NVCustom:
                                zeroMod += hediffSetting.ZeroLight;
                                fullMod += hediffSetting.FullLight;
                                continue;
                            case LightModifiers.Options.NVNightVision:
                                nvZeroMod += hediffSetting.ZeroLight;
                                nvFullMod += hediffSetting.FullLight;
                                continue;
                            case LightModifiers.Options.NVPhotosensitivity:
                                psZeroMod += hediffSetting.ZeroLight;
                                psFullMod += hediffSetting.FullLight;
                                continue;
                            default:
                                continue;
                        }
                    }
                }
            }

            HediffMods[0] = zeroMod;
            NvhediffMods[0] = nvZeroMod;
            PshediffMods[0] = psZeroMod;

            HediffMods[1] = fullMod;
            NvhediffMods[1] = nvFullMod;
            PshediffMods[1] = psFullMod;

            _zeroLightModifier = -1f;
            _fullLightModifier = -1f;
            _hediffsNeedChecking = false;
        }
        #endregion

        #region Apparel Update

        public void CheckAndAddApparel(Apparel apparel)
        {
            if (ParentPawn.Dead || !ParentPawn.Spawned || apparel == null)
            {
                return;
            }
            if (NightVisionSettings.NVApparel.TryGetValue(apparel.def, out ApparelSetting value))
            {
                ApparelGrantsNV |= value.GrantsNV;
                ApparelNullsPS |= value.NullifiesPS;
                PawnsNVApparel.Add(apparel);
            }
            //In case the user changes settings in game
            else if (NightVisionSettings.AllEyeCoveringHeadgearDefs.Contains(apparel.def))
            {
                PawnsNVApparel.Add(apparel);
            }
        }

        public void RemoveApparel(Apparel apparel)
        {
            if (ParentPawn.Dead || !ParentPawn.Spawned || apparel == null)
            {
                return;
            }
            if (PawnsNVApparel.Contains(apparel))
            {
                PawnsNVApparel.Remove(apparel);

                if (!(ApparelGrantsNV || ApparelNullsPS))
                {
                    return;
                }
                QuickRecheckApparel();
            }
        }

        /// <summary>
        /// This only works if PawnsNVApparel is correct
        /// </summary>
        public void QuickRecheckApparel()
        {
            ApparelGrantsNV = false;
            ApparelNullsPS = false;
            foreach (var apparel in PawnsNVApparel)
            {
                if (!NightVisionSettings.NVApparel.TryGetValue(apparel.def, out ApparelSetting value))
                {
                    continue;
                }
                ApparelGrantsNV |= value.GrantsNV;
                ApparelNullsPS |= value.NullifiesPS;
            }
            _apparelNeedsChecking = false;
        }
        #endregion
        
    }
}