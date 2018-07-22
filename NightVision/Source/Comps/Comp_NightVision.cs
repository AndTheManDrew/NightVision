// Nightvision NightVision Comp_NightVision.cs
// 
// 16 05 2018
// 
// 21 07 2018

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using Debug = System.Diagnostics.Debug;

namespace NightVision
    {
        public class Comp_NightVision : ThingComp
            {
                public bool ApparelGrantsNV;
                public bool ApparelNullsPS;

                public int lastDarkTick;

                public   List<Apparel>                       PawnsNVApparel = new List<Apparel>();
                public   Dictionary<string, List<HediffDef>> PawnsNVHediffs = new Dictionary<string, List<HediffDef>>();
                internal float[]                             HediffMods     = new float[2];
                internal float[]                             NvhediffMods   = new float[2];
                internal float[] PshediffMods = new float[2];

                private bool  _apparelNeedsChecking;
                private float _fullLightModifier = -1;


                private bool                _hediffsNeedChecking;
                private Pawn                _intParentPawn;
                private Race_LightModifiers _naturalLightModifiers;

                private int                  _numRemainingEyes = -1;
                private List<Hediff>         _pawnsHediffs;
                private List<BodyPartRecord> _raceSightParts;
                private float                _zeroLightModifier = -1;

                /// <summary>
                ///     The glow mods of the pawns races eyes
                ///     Null Checks, if fails gets eye glow mods normalised for race's number of eyes
                /// </summary>
                public Race_LightModifiers NaturalLightModifiers
                    {
                        get
                            {
                                if (_naturalLightModifiers == null)
                                    {
                                        _naturalLightModifiers = Storage.RaceLightMods[ParentPawn.def];
                                        if (_naturalLightModifiers == null)
                                            {
                                                _naturalLightModifiers =
                                                            new Race_LightModifiers(ParentPawn.def);
                                                Storage.RaceLightMods[ParentPawn.def] = _naturalLightModifiers;
                                            }

                                        _zeroLightModifier = -1f;
                                        _fullLightModifier = -1f;
                                    }

                                return _naturalLightModifiers;
                            }
                    }

                private bool CanCheat => Props.CanCheat;

                public int TicksSinceLastDark => Find.TickManager.TicksGame - lastDarkTick;

                [UsedImplicitly]
                public CompProperties_NightVision Props => (CompProperties_NightVision) props;
        
                public Pawn ParentPawn => _intParentPawn ?? (_intParentPawn = parent as Pawn);
        
                private List<Hediff> PawnHediffs =>
                            _pawnsHediffs ?? (_pawnsHediffs = ParentPawn?.health?.hediffSet?.hediffs);

                private int EyeCount => RaceSightParts.Count;
        
                public List<BodyPartRecord> RaceSightParts
                    {
                        get
                            {
                                if (_raceSightParts.NullOrEmpty())
                                    {
                                        _raceSightParts = ParentPawn?
                                                          .RaceProps.body.GetPartsWithTag(Constants.EyeTag)
                                                          .ToList();
                                        
                                        if (_raceSightParts.NullOrEmpty())
                                            {
                                                Log.Message(
                                                    $"{ParentPawn?.LabelShort}'s race has no eyes. The NightVision Comp should not be attached.");
                                                Log.Message(
                                                    "Creating an empty list for their races eyes but NV will not have any effect");
                                                return new List<BodyPartRecord>();
                                            }
                                    }
                                
                                return _raceSightParts;
                            }
                    }

                /// <summary>
                ///     Get: returns the number of pawns RACE's eyes if private float numRemainingEyes is negative
                ///     Set: If the value is less than 0 then will set = 0;
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
                        set => _numRemainingEyes = value < 0 ? 0 : value;
                    }

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

                                if (ApparelGrantsNV && _zeroLightModifier + 0.001f
                                    < LightModifiersBase.NVLightModifiers[0])
                                    {
                                        return LightModifiersBase.NVLightModifiers[0];
                                    }

                                return _zeroLightModifier;
                            }
                    }

                public float FullLightModifier
                    {
                        get
                            {
                                //true on init, hediffs changed, apparel changed or settings changed
                                if (_fullLightModifier < -0.99f)
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

                public void SetDirty()
                    {
                        _naturalLightModifiers = null;
                        _zeroLightModifier     = -1f;
                        _fullLightModifier     = -1f;
                        _apparelNeedsChecking  = true;
                        _hediffsNeedChecking   = true;
                    }

                /// <summary>
                ///     To calculate the effects of light on movement speed and work speed:
                /// </summary>
                /// <param name="glow">light level as float in [0,1]</param>
                /// <returns>a multiplier: (default + light offsets)</returns>
                public float FactorFromGlow(
                    float glow)
                    {
                        //If glow is approx. 0%
                        if (glow < 0.001f)
                            {
                                return (float) Math.Round(Constants.DefaultZeroLightMultiplier + ZeroLightModifier, 2);
                            }
                        //If glow is approx. 100% and the pawns full light modifier is not approx 0

                        if (Math.Abs(glow - 1f) < 0.001)
                            {
                                if (Math.Abs(FullLightModifier) > 0.005f)
                                    {
                                        return (float) Math.Round(
                                            Constants.DefaultFullLightMultiplier + FullLightModifier,
                                            2);
                                    }

                                return 1f;
                            }
                        //Else linear interpolation

                        if (glow < Constants.MinGlowNoGlow)
                            {
                                return (float) Math.Round(
                                    1f + (Constants.MinGlowNoGlow - glow) * (ZeroLightModifier - 0.2f) / 0.3f,
                                    2);
                            }

                        if (glow > Constants.MaxGlowNoGlow && Math.Abs(FullLightModifier) > 0.01f)
                            {
                                return (float) Math.Round(
                                    1f + (glow - Constants.MaxGlowNoGlow) * FullLightModifier / 0.3f,
                                    2);
                            }

                        return 1f;
                    }

                /// <summary>
                ///     For the pawn's stat inspect tab. fat TODO split?
                /// </summary>
                /// <param name="result"></param>
                /// <param name="glow"></param>
                /// <param name="usedApparelSetting">if apparel had an effect</param>
                /// <param name="needsFinalValue">if RW will finalise value or we need to add it</param>
                /// <returns></returns>
                public string ExplanationBuilder(
                    string   result,
                    float    glow,
                    out bool usedApparelSetting,
                    bool     needsFinalValue = false)
                    {
                        var     nvsum          = 0f;
                        var     pssum          = 0f;
                        var     sum            = 0f;
                        float[] caps           = LightModifiersBase.GetCapsAtGlow(glow);
                        var     foundSomething = false;
                        float   effect;
                        var     basevalue = 0f;
                        bool    lowLight  = glow < 0.3f;
                        usedApparelSetting = false;

                        var explanation = new StringBuilder(result);
                        StringBuilder nvexplanation = new StringBuilder().AppendLine(
                            VisionType.NVNightVision.ToString().Translate() + " " + "NVEffects".Translate()
                            + string.Format(Constants.Maxline,
                                "",
                                "NVMaxAtGlow".Translate(glow.ToStringPercent()),
                                caps[2]));
                        StringBuilder psexplanation = new StringBuilder().AppendLine(
                            VisionType.NVPhotosensitivity.ToString().Translate() + " " + "NVEffects".Translate()
                            + string.Format(Constants.Maxline,
                                "",
                                "NVMaxAtGlow".Translate(glow.ToStringPercent()),
                                caps[3]));

                        explanation.AppendLine();
                        if (lowLight)
                            {
                                effect = Constants.DefaultFullLightMultiplier
                                         + (Constants.DefaultZeroLightMultiplier - Constants.DefaultFullLightMultiplier)
                                         * (0.3f - glow) / 0.3f;
                                if (Math.Abs(effect) >= 0.005)
                                    {
                                        explanation.AppendFormat(Constants.MultiplierLine,
                                            "StatsReport_BaseValue".Translate(),
                                            effect);
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
                                explanation.AppendFormat(Constants.MultiplierLine,
                                    "StatsReport_BaseValue".Translate(),
                                    Constants.DefaultFullLightMultiplier);
                                explanation.AppendLine();
                                basevalue = Constants.DefaultFullLightMultiplier;
                                if (ApparelNullsPS)
                                    {
                                        foundSomething = true;
                                    }
                            }

                        //Possibly a redundant check
                        if (NaturalLightModifiers.HasModifier())
                            {
                                effect = NaturalLightModifiers.GetEffectAtGlow(glow);
                                if (Math.Abs(effect) >= 0.005)
                                    {
                                        foundSomething = true;
                                        switch (NaturalLightModifiers.Setting)
                                            {
                                                //TODO consider iterating over the racesightparts and returning the custom label of each part
                                                case VisionType.NVNightVision:
                                                    nvsum += (float) Math.Round(effect * NumberOfRemainingEyes,
                                                        2,
                                                        MidpointRounding.AwayFromZero);
                                                    nvexplanation.AppendFormat("  " + Constants.ModifierLine,
                                                        RaceSightParts.First().LabelShort + " x"
                                                                                          + NumberOfRemainingEyes,
                                                        effect * NumberOfRemainingEyes);
                                                    break;
                                                case VisionType.NVPhotosensitivity:
                                                    pssum += (float) Math.Round(effect * NumberOfRemainingEyes,
                                                        2,
                                                        MidpointRounding.AwayFromZero);
                                                    psexplanation.AppendFormat("  " + Constants.ModifierLine,
                                                        RaceSightParts.First().LabelShort + " x"
                                                                                          + NumberOfRemainingEyes,
                                                        effect * NumberOfRemainingEyes);
                                                    break;
                                                case VisionType.NVCustom:
                                                    sum += (float) Math.Round(effect * NumberOfRemainingEyes,
                                                        2,
                                                        MidpointRounding.AwayFromZero);
                                                    explanation.AppendFormat("  " + Constants.ModifierLine,
                                                        RaceSightParts.First().LabelShort + " x"
                                                                                          + NumberOfRemainingEyes,
                                                        effect * NumberOfRemainingEyes);
                                                    break;
                                            }
                                    }
                            }

                        foreach (List<HediffDef> value in PawnsNVHediffs.Values)
                            {
                                if (value.NullOrEmpty())
                                    {
                                        continue;
                                    }

                                foreach (HediffDef hediffDef in value)
                                    {
                                        if (Storage.HediffLightMods.TryGetValue(hediffDef,
                                            out Hediff_LightModifiers hediffSetting))
                                            {
                                                effect = hediffSetting.GetEffectAtGlow(glow, EyeCount);
                                                if (Math.Abs(effect) > 0.005)
                                                    {
                                                        foundSomething = true;
                                                        switch (hediffSetting.IntSetting)
                                                            {
                                                                case VisionType.NVNightVision:
                                                                    nvsum += (float) Math.Round(effect,
                                                                        2,
                                                                        MidpointRounding.AwayFromZero);
                                                                    nvexplanation.AppendFormat(
                                                                        "  " + Constants.ModifierLine,
                                                                        hediffDef.LabelCap,
                                                                        effect);
                                                                    nvexplanation.AppendLine();
                                                                    break;
                                                                case VisionType.NVPhotosensitivity:
                                                                    pssum += (float) Math.Round(effect,
                                                                        2,
                                                                        MidpointRounding.AwayFromZero);
                                                                    psexplanation.AppendFormat(
                                                                        "  " + Constants.ModifierLine,
                                                                        hediffDef.LabelCap,
                                                                        effect);
                                                                    psexplanation.AppendLine();
                                                                    break;
                                                                case VisionType.NVCustom:
                                                                    sum += (float) Math.Round(effect,
                                                                        2,
                                                                        MidpointRounding.AwayFromZero);
                                                                    explanation.AppendFormat(
                                                                        "  " + Constants.ModifierLine,
                                                                        hediffDef.LabelCap,
                                                                        effect);
                                                                    explanation.AppendLine();
                                                                    break;
                                                            }
                                                    }
                                            }
                                    }
                            }

                        if (foundSomething)
                            {
                                if (Math.Abs(nvsum) > 0.005f)
                                    {
                                        explanation.Append(nvexplanation);
                                        explanation.AppendLine();
                                    }

                                if (Math.Abs(pssum) > 0.005f)
                                    {
                                        explanation.Append(psexplanation);
                                        explanation.AppendLine();
                                    }

                                sum += pssum + nvsum;
                                if (Math.Abs(sum) > 0.005f)
                                    {
                                        explanation.AppendFormat(Constants.ModifierLine,
                                            "NVTotal".Translate() + " " + "NVModifier".Translate(),
                                            sum);
                                        explanation.AppendLine();
                                        explanation.AppendLine();
                                    }

                                if (!CanCheat)
                                    {
                                        if (sum - 0.001f > caps[0] || sum + 0.001f < caps[1])
                                            {
                                                explanation.AppendFormat(Constants.Maxline,
                                                    "NVTotal".Translate() + " ",
                                                    "max".Translate(),
                                                    sum > caps[0] ? caps[0] : caps[1]);

                                                explanation.AppendLine();
                                            }

                                        if (lowLight & ApparelGrantsNV & sum + 0.001f < caps[2])
                                            {
                                                explanation.Append(
                                                    "NVGearPresent".Translate($"{basevalue + caps[2]:0%}"));
                                                usedApparelSetting = true;
                                                sum                = caps[2];
                                            }
                                        else if (ApparelNullsPS & sum + 0.001f < 0)
                                            {
                                                explanation.Append(
                                                    "PSGearPresent".Translate(
                                                        $"{Constants.DefaultFullLightMultiplier:0%}"));
                                                usedApparelSetting = true;
                                                sum                = 0;
                                            }
                                    }

                                explanation.AppendLine();
                                if (needsFinalValue)
                                    {
                                        sum += basevalue;
                                        explanation.AppendFormat(Constants.MultiplierLine,
                                            "StatsReport_FinalValue".Translate(),
                                            sum > caps[0] + basevalue ? caps[0] + basevalue :
                                            sum < caps[1] + basevalue ? caps[1] + basevalue : sum);
                                    }

                                return explanation.ToString();
                            }

                        return result;
                    }

                #region Thoughts
                /// <summary>
                ///     For ThoughtWorker_Dark patch
                /// </summary>
                /// <returns></returns>
                public VisionType PsychDark() => Classifier.ClassifyModifier(ZeroLightModifier, true);

                /// <summary>
                ///     For ThoughtWorker_TooBright
                /// </summary>
                public bool PsychBright() =>
                            TicksSinceLastDark > Constants.ThoughtActiveTicksPast && IsOversensitiveToLight();

                private bool IsOversensitiveToLight()
                    {
                        return Classifier.ClassifyModifier(FullLightModifier, false) == VisionType.NVPhotosensitivity;
                    }

                #endregion

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

                        if (DrawSettings.LogPawnComps)
                            {
                                Log.Message(new string('*', 30));
                                Log.Message("NightVisionComp: " + ParentPawn.Name);
                                Log.Message("Hediffs: ");
                                foreach (KeyValuePair<string, List<HediffDef>> hedifflist in PawnsNVHediffs)
                                    {
                                        Log.Message(new string('-', 20));
                                        Log.Message(hedifflist.Key + "has:");
                                        foreach (HediffDef hediff in hedifflist.Value)
                                            {
                                                Log.Message(hediff.LabelCap);
                                            }
                                    }

                                Log.Message(new string('-', 20));
                                Log.Message("NumberRemainingEyes: " + NumberOfRemainingEyes);
                                Log.Message("HediffLightModifiers: zero = " + HediffMods[0] + PshediffMods[0]
                                            + NvhediffMods[0] + ", full = " + HediffMods[0] + PshediffMods[0]
                                            + NvhediffMods[0]);
                                Log.Message("NaturalLightModifiers: zero = " + NaturalLightModifiers[0] + ", full = "
                                            + NaturalLightModifiers[1]);
                                Log.Message("Total Glow Mods: zero = " + ZeroLightModifier + ", full = "
                                            + FullLightModifier);
                                Log.Message(new string('*', 30));
                            }
                    }

                //Note: we don't save this comp so this only gets called when spawning new pawn, i think
                public override void PostSpawnSetup(
                    bool respawningAfterLoad)
                    {
                        base.PostSpawnSetup(respawningAfterLoad);
                        InitPawnsHediffsAndCountEyes();
                        InitPawnsApparel();
                    }

                public override void Initialize(
                    CompProperties props)
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

                public void RemoveHediff(
                    Hediff         hediff,
                    BodyPartRecord part = null)
                    {
                        if (!ParentPawn.Spawned || ParentPawn.health == null || ParentPawn.Dead)
                            {
                                return;
                            }

                        if (part != null && PawnsNVHediffs.ContainsKey(part.Label)
                                         && PawnsNVHediffs[part.Label].Remove(hediff.def))
                            {
                                if (part.def.tags.Contains(Constants.EyeTag)
                                    && (hediff is Hediff_MissingPart
                                        || hediff.def.addedPartProps is AddedBodyPartProps abpp && abpp.solid))
                                    {
                                        NumberOfRemainingEyes++;
                                    }

                                CalculateHediffMod();
                            }
                        else if (PawnsNVHediffs[Constants.BodyKey].Remove(hediff.def))
                            {
                                CalculateHediffMod();
                            }
                    }

                public void CheckAndAddHediff(
                    Hediff         hediff,
                    BodyPartRecord part = null)
                    {
                        if (ParentPawn.Dead || !ParentPawn.Spawned)
                            {
                                return;
                            }

                        if (part != null)
                            {
                                string partName = part.Label;
                                if (PawnsNVHediffs.TryGetValue(partName, out List<HediffDef> tempPartsHediffDefs))
                                    {
                                        //Categorise the hediff:
                                        //MissingPart overrides everything
                                        var removedPart = false;

                                        if (hediff is Hediff_MissingPart)
                                            {
                                                removedPart         = true;
                                                tempPartsHediffDefs = new List<HediffDef> {HediffDefOf.MissingBodyPart};
                                            }
                                        else
                                            {
                                                //Check if there is a setting for it
                                                HediffDef hediffDef = hediff.def;
                                                if (Storage.HediffLightMods.ContainsKey(hediffDef))
                                                    {
                                                        if (hediffDef.addedPartProps is AddedBodyPartProps abpp
                                                            && abpp.solid)
                                                            {
                                                                removedPart         = true;
                                                                tempPartsHediffDefs = new List<HediffDef> {hediffDef};
                                                            }
                                                        else if (!tempPartsHediffDefs.Contains(hediffDef))
                                                            {
                                                                tempPartsHediffDefs.Add(hediffDef);
                                                            }
                                                    }
                                                //Last two checks are in case the user changes settings later
                                                //Is it a solid replacement?
                                                else if (hediffDef.addedPartProps is AddedBodyPartProps abpp
                                                         && abpp.solid)
                                                    {
                                                        removedPart         = true;
                                                        tempPartsHediffDefs = new List<HediffDef> {hediffDef};
                                                    }
                                                //Check if it is a valid hediff
                                                else if (Storage.AllSightAffectingHediffs.Contains(hediffDef))
                                                    {
                                                        if (!tempPartsHediffDefs.Contains(hediffDef))
                                                            {
                                                                tempPartsHediffDefs.Add(hediffDef);
                                                            }
                                                    }
                                            }

                                        //remove an eye
                                        if (removedPart && part.def.tags.Contains(Constants.EyeTag))
                                            {
                                                NumberOfRemainingEyes--;
                                            }

                                        PawnsNVHediffs[partName] = tempPartsHediffDefs;
                                        CalculateHediffMod();
                                    }
                                else if (Storage.AllSightAffectingHediffs.Contains(hediff.def))
                                    {
                                        PawnsNVHediffs[partName] = new List<HediffDef> {hediff.def};
                                    }
                            }
                        else if (Storage.AllSightAffectingHediffs.Contains(hediff.def))
                            {
                                if (PawnsNVHediffs.TryGetValue(Constants.BodyKey, out List<HediffDef> value)
                                    && !value.Contains(hediff.def))
                                    {
                                        PawnsNVHediffs[Constants.BodyKey].Add(hediff.def);
                                        CalculateHediffMod();
                                    }
                            }
                    }

                public void CheckAndAddApparel(
                    Apparel apparel)
                    {
                        if (ParentPawn.Dead || !ParentPawn.Spawned || apparel == null)
                            {
                                return;
                            }

                        if (Storage.NVApparel.TryGetValue(apparel.def, out ApparelVisionSetting value))
                            {
                                ApparelGrantsNV |= value.GrantsNV;
                                ApparelNullsPS  |= value.NullifiesPS;
                                PawnsNVApparel.Add(apparel);
                            }
                        //In case the user changes settings in game
                        else if (Storage.AllEyeCoveringHeadgearDefs.Contains(apparel.def))
                            {
                                PawnsNVApparel.Add(apparel);
                            }
                    }

                public void RemoveApparel(
                    Apparel apparel)
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

                private void CalculateHediffMod()
                    {
                        var zeroMod   = 0f;
                        var fullMod   = 0f;
                        var psZeroMod = 0f;
                        var psFullMod = 0f;
                        var nvZeroMod = 0f;
                        var nvFullMod = 0f;
                        int eyeCount  = RaceSightParts.Count;
                        foreach (List<HediffDef> value in PawnsNVHediffs.Values)
                            {
                                if (value.NullOrEmpty())
                                    {
                                        continue;
                                    }

                                foreach (HediffDef hediffDef in value)
                                    {
                                        if (Storage.HediffLightMods.TryGetValue(hediffDef,
                                            out Hediff_LightModifiers hediffSetting))
                                            {
                                                int eyeNormalisingFactor = hediffSetting.AffectsEye ? eyeCount : 1;
                                                switch (hediffSetting.Setting)
                                                    {
                                                        case VisionType.NVNone: continue;
                                                        case VisionType.NVCustom:
                                                            zeroMod += hediffSetting[0] / eyeNormalisingFactor;
                                                            fullMod += hediffSetting[1] / eyeNormalisingFactor;
                                                            continue;
                                                        case VisionType.NVNightVision:
                                                            nvZeroMod += hediffSetting[0] / eyeNormalisingFactor;
                                                            nvFullMod += hediffSetting[1] / eyeNormalisingFactor;
                                                            continue;
                                                        case VisionType.NVPhotosensitivity:
                                                            psZeroMod += hediffSetting[0] / eyeNormalisingFactor;
                                                            psFullMod += hediffSetting[1] / eyeNormalisingFactor;
                                                            continue;
                                                        default: continue;
                                                    }
                                            }
                                    }
                            }

                        HediffMods[0]   = zeroMod;
                        NvhediffMods[0] = nvZeroMod;
                        PshediffMods[0] = psZeroMod;

                        HediffMods[1]   = fullMod;
                        NvhediffMods[1] = nvFullMod;
                        PshediffMods[1] = psFullMod;

                        _zeroLightModifier   = -1f;
                        _fullLightModifier   = -1f;
                        _hediffsNeedChecking = false;
                    }

                private float CalcZeroLightModifier()
                    {
                        float mod     = Constants.DefaultZeroLightMultiplier;
                        var   setting = (byte) NaturalLightModifiers.IntSetting;
                        switch (setting)
                            {
                                default:
                                    mod += Mathf.Clamp(NvhediffMods[0],
                                               Math.Min(0f, LightModifiersBase.NVLightModifiers[0]),
                                               Math.Max(0f, LightModifiersBase.NVLightModifiers[0]))
                                           + Mathf.Clamp(PshediffMods[0],
                                               Math.Min(0f, LightModifiersBase.PSLightModifiers[0]),
                                               Math.Max(0f, LightModifiersBase.PSLightModifiers[0])) + HediffMods[0];
                                    break;
                                case 1: //Nightvision
                                    mod += Mathf.Clamp(NvhediffMods[0] + NaturalLightModifiers[0] * _numRemainingEyes,
                                               Math.Min(0f, LightModifiersBase.NVLightModifiers[0]),
                                               Math.Max(0f, LightModifiersBase.NVLightModifiers[0]))
                                           + Mathf.Clamp(PshediffMods[0],
                                               Math.Min(0f, LightModifiersBase.PSLightModifiers[0]),
                                               Math.Max(0f, LightModifiersBase.PSLightModifiers[0])) + HediffMods[0];
                                    break;
                                case 2: // Photosensitive
                                    mod += Mathf.Clamp(NvhediffMods[0],
                                               Math.Min(0f, LightModifiersBase.NVLightModifiers[0]),
                                               Math.Max(0f, LightModifiersBase.NVLightModifiers[0])) + Mathf.Clamp(
                                               PshediffMods[0] + NaturalLightModifiers[0] * _numRemainingEyes,
                                               Math.Min(0f, LightModifiersBase.PSLightModifiers[0]),
                                               Math.Max(0f, LightModifiersBase.PSLightModifiers[0])) + HediffMods[0];
                                    break;
                                case 3: //Custom
                                    mod += Mathf.Clamp(NvhediffMods[0],
                                               Math.Min(0f, LightModifiersBase.NVLightModifiers[0]),
                                               Math.Max(0f, LightModifiersBase.NVLightModifiers[0]))
                                           + Mathf.Clamp(PshediffMods[0],
                                               Math.Min(0f, LightModifiersBase.PSLightModifiers[0]),
                                               Math.Max(0f, LightModifiersBase.PSLightModifiers[0])) + HediffMods[0]
                                           + NaturalLightModifiers[0] * _numRemainingEyes;
                                    break;
                            }

                        if (CanCheat)
                            {
                                return (float) Math.Round(mod - Constants.DefaultZeroLightMultiplier, 2);
                            }

                        return (float) Math.Round(
                            Mathf.Clamp(mod, Storage.MultiplierCaps.min, Storage.MultiplierCaps.max)
                            - Constants.DefaultZeroLightMultiplier,
                            2);
                    }

                private float CalcFullLightModifier()
                    {
                        float mod     = Constants.DefaultFullLightMultiplier;
                        var   setting = (byte) NaturalLightModifiers.IntSetting;
                        switch (setting)
                            {
                                default:
                                    mod += Mathf.Clamp(NvhediffMods[1],
                                               Math.Min(0f, LightModifiersBase.NVLightModifiers[1]),
                                               Math.Max(0f, LightModifiersBase.NVLightModifiers[1]))
                                           + Mathf.Clamp(PshediffMods[1],
                                               Math.Min(0f, LightModifiersBase.PSLightModifiers[1]),
                                               Math.Max(0f, LightModifiersBase.PSLightModifiers[1])) + HediffMods[1];
                                    break;
                                case 1: //Nightvision
                                    mod += Mathf.Clamp(NvhediffMods[1] + NaturalLightModifiers[1] * _numRemainingEyes,
                                               Math.Min(0f, LightModifiersBase.NVLightModifiers[1]),
                                               Math.Max(0f, LightModifiersBase.NVLightModifiers[1]))
                                           + Mathf.Clamp(PshediffMods[1],
                                               Math.Min(0f, LightModifiersBase.PSLightModifiers[1]),
                                               Math.Max(0f, LightModifiersBase.PSLightModifiers[1])) + HediffMods[1];
                                    break;
                                case 2: // Photosensitive
                                    mod += Mathf.Clamp(NvhediffMods[1],
                                               Math.Min(0f, LightModifiersBase.NVLightModifiers[1]),
                                               Math.Max(0f, LightModifiersBase.NVLightModifiers[1])) + Mathf.Clamp(
                                               PshediffMods[1] + NaturalLightModifiers[1] * _numRemainingEyes,
                                               Math.Min(0f, LightModifiersBase.PSLightModifiers[1]),
                                               Math.Max(0f, LightModifiersBase.PSLightModifiers[1])) + HediffMods[1];
                                    break;
                                case 3: //Custom
                                    mod += Mathf.Clamp(NvhediffMods[1],
                                               Math.Min(0f, LightModifiersBase.NVLightModifiers[1]),
                                               Math.Max(0f, LightModifiersBase.NVLightModifiers[1]))
                                           + Mathf.Clamp(PshediffMods[1],
                                               Math.Min(0f, LightModifiersBase.PSLightModifiers[1]),
                                               Math.Max(0f, LightModifiersBase.PSLightModifiers[1])) + HediffMods[1]
                                           + NaturalLightModifiers[1] * _numRemainingEyes;
                                    break;
                            }

                        if (CanCheat)
                            {
                                return (float) Math.Round(mod - Constants.DefaultFullLightMultiplier, 2);
                            }

                        return (float) Math.Round(
                            Mathf.Clamp(mod, Storage.MultiplierCaps.min, Storage.MultiplierCaps.max)
                            - Constants.DefaultFullLightMultiplier,
                            2);
                    }

                /// <summary>
                ///     Builds a dictionary to store all the possibly relevant hediffs:
                ///     <para>Keys: initially .Label of all sight parts & catch-all "wholebody" but will add other parts at runtime if necessary</para>
                ///     <para>Values: List of all the hediff defs that affect that part</para>
                /// </summary>
                private void InitPawnsHediffsAndCountEyes()
                    {
                        if (!ParentPawn.Spawned || ParentPawn.health == null || ParentPawn.Dead)
                            {
                                return;
                            }

                        NumberOfRemainingEyes = RaceSightParts.Count;
                        PawnsNVHediffs.Clear();
                        for (var i = 0; i < NumberOfRemainingEyes; i++)
                            {
                                PawnsNVHediffs[RaceSightParts[i].Label] = new List<HediffDef>();
                            }
                        
                        PawnsNVHediffs[Constants.BodyKey] = new List<HediffDef>();
                        if (!PawnHediffs.NullOrEmpty())
                            {
                                foreach (Hediff hediff in PawnHediffs)
                                    {
                                        CheckAndAddHediff(hediff, hediff.Part);
                                    }
                            }

                        CalculateHediffMod();
                    }

                private void InitPawnsApparel()
                    {
                        if (!ParentPawn.Spawned || ParentPawn.health == null || ParentPawn.Dead)
                            {
                                return;
                            }

                        ApparelGrantsNV = false;
                        ApparelNullsPS  = false;
                        PawnsNVApparel  = new List<Apparel>();
                        if (!(ParentPawn.apparel?.WornApparel is List<Apparel> pawnsApparel))
                            {
                                return;
                            }

                        foreach (Apparel apparel in pawnsApparel)
                            {
                                if (Storage.NVApparel.TryGetValue(apparel.def, out ApparelVisionSetting value))
                                    {
                                        ApparelGrantsNV |= value.GrantsNV;
                                        ApparelNullsPS  |= value.NullifiesPS;
                                        PawnsNVApparel.Add(apparel);
                                    }
                                else if (Storage.AllEyeCoveringHeadgearDefs.Contains(apparel.def))
                                    {
                                        PawnsNVApparel.Add(apparel);
                                    }
                            }
                    }

                /// <summary>
                ///     This only works if PawnsNVApparel is correct
                /// </summary>
                private void QuickRecheckApparel()
                    {
                        ApparelGrantsNV = false;
                        ApparelNullsPS  = false;
                        foreach (Apparel apparel in PawnsNVApparel)
                            {
                                if (!Storage.NVApparel.TryGetValue(apparel.def, out ApparelVisionSetting value))
                                    {
                                        continue;
                                    }

                                ApparelGrantsNV |= value.GrantsNV;
                                ApparelNullsPS  |= value.NullifiesPS;
                            }

                        _apparelNeedsChecking = false;
                    }
            }
    }