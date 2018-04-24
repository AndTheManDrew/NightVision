using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace NightVision
{
    class Comp_NightVision : ThingComp
    {
        #region Constants
        public const string eyeTag = "SightSource";
        public const string brainTag = "ConsciousnessSource";
        public const string bodyKey = "WholeBody";
        
        public const float minGlowNoGlow = 0.3f;
        public const float maxGlowNoGlow = 0.7f;
        
        private const string modifierLine = "    {0}  {1,6:+#0.#%;-#0.#%;0%}";
        private const string multiplierLine = "    {0}  {1,6:x#0.#%;x#0.#%;x0%}";
        #endregion

        #region Private Fields
        private int numRemainingEyes = -1;
        private Pawn intParentPawn;
        private List<BodyPartRecord> raceSightParts;
        private EyeGlowMods naturalGlowMods;
        private GlowMods GlowModsFromHediffs = new GlowMods(GlowMods.Options.NVCustom);
        private string brainName;

        private bool hediffsNeedChecking = false;
        private bool apparelNeedsChecking = false;
        private bool glowmodschanged = false;
        private bool ApparelGrantsNV = false;
        private bool ApparelNullsPS = false;
        private float zeroLightModifier;
        private float fullLightModifier;
        private List<Hediff> pawnsHediffs;
        #endregion

        #region Public Fields
        public CompProperties_NightVision Props => (CompProperties_NightVision)props;
        public List<Apparel> PawnsNVApparel = new List<Apparel>();
        public Dictionary<string, List<HediffDef>> PawnsNVHediffs = new Dictionary<string, List<HediffDef>>();
        #endregion

        #region Pawn Details

        public Pawn ParentPawn
        {
            get
            {
                if (intParentPawn == null)
                {
                    intParentPawn = parent as Pawn;
                }
                return intParentPawn;
            }
        }

        public List<Hediff> PawnHediffs
        {
            get
            {
                if (pawnsHediffs == null)
                {
                    pawnsHediffs = new List<Hediff>();
                    pawnsHediffs = ParentPawn.health?.hediffSet?.hediffs;
                }
                return pawnsHediffs;
            }
        }
        public List<BodyPartRecord> RaceSightParts
        {
            get
            {
                if (raceSightParts.NullOrEmpty())
                {
                    raceSightParts = ParentPawn.RaceProps.body.AllParts.Where(part => part.def.tags.Contains(eyeTag)).ToList();
                    if (raceSightParts.NullOrEmpty())
                    {
                        Log.Message(ParentPawn.LabelShort + "'s race has no eyes. The NightVision Comp should not be attached.");
                        Log.Message("Creating an empty list for their races eyes but NV will not have any effect");
                        return new List<BodyPartRecord>();
                    }
                }
                return raceSightParts;
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
                if (numRemainingEyes < 0)
                {
                    numRemainingEyes = RaceSightParts.Count;
                }
                return numRemainingEyes;
            }
            set
            {
                numRemainingEyes = (value < 0)? 0 : value;
            }
        }

        #endregion

        #region Natural Glow Mods from race
        /// <summary>
        /// The glow mods of the pawns races eyes
        /// Null Checks and Check for if it has been set: if either is false, gets eye glow mods normalised for race's number of eyes
        /// </summary>
        public EyeGlowMods NaturalGlowMods
        {
            get
            {
                if (naturalGlowMods == null)
                {
                    naturalGlowMods = NightVisionSettings.GetRaceNightVisionMod(ParentPawn.def, RaceSightParts.Count);
                    Log.Message("GetRaceNightVisionMod for: " + ParentPawn.NameStringShort + " returned " + naturalGlowMods.ZeroLight + " & " + naturalGlowMods.FullLight);
                }
                return naturalGlowMods;
            }
            private set
            {
                value = naturalGlowMods;
            }
        }
        #endregion

        #region Modifier Calculators
        public float ZeroLightModifier
        {
            get
            {
                if (glowmodschanged || zeroLightModifier == 0)
                {
                    zeroLightModifier =  (float)Math.Round(Mathf.Clamp(
                                                        NightVisionSettings.DefaultZeroLightMultiplier + GlowModsFromHediffs.ZeroLight + (NaturalGlowMods.ZeroLight * NumberOfRemainingEyes)
                                                    , NightVisionSettings.MultiplierCaps.min, NightVisionSettings.MultiplierCaps.max)
                                                        - NightVisionSettings.DefaultZeroLightMultiplier
                                                            , 2);

                }
                //Default FULL light multiplier is NOT a mistake ( it == 1f i.e. night vision)
                if (ApparelGrantsNV && zeroLightModifier < GlowMods.DefaultNVZero)
                {
                    return GlowMods.DefaultNVZero;
                }
                return zeroLightModifier;
            }
        }
        public float FullLightModifier
        {
            get
            {
                if (glowmodschanged || fullLightModifier == 0)
                {
                    fullLightModifier = (float)Math.Round(Mathf.Clamp(
                                                        NightVisionSettings.DefaultFullLightMultiplier + GlowModsFromHediffs.FullLight + (NaturalGlowMods.FullLight * NumberOfRemainingEyes)
                                                    , NightVisionSettings.MultiplierCaps.min, NightVisionSettings.MultiplierCaps.max)
                                                        - NightVisionSettings.DefaultFullLightMultiplier
                                                            , 2);
                }
                if (ApparelNullsPS && fullLightModifier < 0f)
                {
                    return 0f;
                }
                return fullLightModifier;
            }
        }
        #endregion

        #region Output to harmony patches
        /// <summary>
        /// To calculate the effects of light on movement speed and work speed
        /// </summary>
        public float FactorFromGlow(float glow)
        {
            if (glow < minGlowNoGlow)
            {
                return (float)Math.Round(1f + (minGlowNoGlow - glow) * (ZeroLightModifier - 0.2f) / 0.3f, 2);
            }
            else if (glow > maxGlowNoGlow && FullLightModifier != 1f)
            {
                return (float)Math.Round(1f + (glow - maxGlowNoGlow) * FullLightModifier / 0.3f, 2);
            }
            return 1f;
        }

        /// <summary>
        /// For the pawn's stat inspect tab
        /// </summary>
        /// <param name="__result"></param>
        /// <param name="glow"></param>
        /// <returns></returns>
        public string ExplanationBuilder(string __result, float glow)
        {
            //TODO Remove sum for release
            float sum = 0f;
            bool foundSomething = false;
            StringBuilder explanation = new StringBuilder(__result);
            explanation.AppendLine();
            float effect = 0f;
            if (glow < 0.3f)
            {
                effect = NightVisionSettings.DefaultFullLightMultiplier + (NightVisionSettings.DefaultZeroLightMultiplier -  NightVisionSettings.DefaultFullLightMultiplier) * (0.3f - glow) / 0.3f;
                if (Math.Abs(effect) >= 0.01)
                {
                    explanation.AppendFormat(multiplierLine, "NVBase".Translate(), effect);
                    explanation.AppendLine();
                    sum += effect;
                }
            }
            else
            {
                explanation.AppendFormat(multiplierLine, "NVBase".Translate(), 1f);
                explanation.AppendLine();
                sum += 1f;
            }
            effect = 0f;
            if (!NaturalGlowMods.IsNone())
            {
                effect = NaturalGlowMods.GetEffectAtGlow(glow);
                if (Math.Abs(effect) >= 0.01)
                {
                    foundSomething = true;
                    sum += effect * NumberOfRemainingEyes;
                    explanation.AppendFormat(modifierLine, ParentPawn.def.LabelCap + " x" + NumberOfRemainingEyes, effect * NumberOfRemainingEyes);
                    explanation.AppendLine();
                }
            }
            foreach (List<HediffDef> value in PawnsNVHediffs.Values)
            {
                if (!value.NullOrEmpty())
                {
                    for (int i = 0; i < value.Count; i++)
                    {
                        if (NightVisionSettings.HediffGlowMods.TryGetValue(value[i], out GlowMods hediffSetting) && !hediffSetting.IsNone())
                        {
                            effect = hediffSetting.GetEffectAtGlow(glow);
                            if (Math.Abs(effect) >= 0.01)
                            {
                                foundSomething = true;
                                sum += effect;
                                explanation.AppendFormat(modifierLine, value[i].LabelCap, effect);
                                explanation.AppendLine();
                            }
                        }
                    }
                }
            }
            if (foundSomething)
            {
                //TODO remove for release
                explanation.AppendLine();
                explanation.AppendFormat("    {0}  {1,6:x#0.#%;x#0.#%;x0%}", "NVTotal".Translate(), sum);

                return explanation.ToString();
            }
            return __result;
        }

        /// <summary>
        /// For ThoughtWorker_Dark patch
        /// </summary>
        /// <returns></returns>
        public GlowMods.Options NVPsychDark()
        {
            if (ZeroLightModifier < 0.01f)
            {
                return GlowMods.Options.NVNone;
            }
            else
            {
                //if nightvis and photosens have equal bonuses at zero % light
                if (Math.Abs(GlowMods.nvZeroLightMod - GlowMods.psZeroLightMod) < 0.01f)
                {
                    //if equal to photosensitivity bonus ( or nightvis bonus)
                    if (Math.Abs(ZeroLightModifier - GlowMods.psZeroLightMod) < 0.01f)
                    {
                        return GlowMods.Options.NVPhotosensitivity;
                    }
                    //otherwise if greater than min 0.2 (equivalent to 100% mv&wrk speed at 0% light) or half the photosens bonus 
                    else if (ZeroLightModifier > Math.Min(GlowMods.DefaultNVZero, GlowMods.nvZeroLightMod / 2) - 0.01f)
                    {
                        return GlowMods.Options.NVNightVision;
                    }
                }
                else
                {
                    float lower = Math.Min(GlowMods.nvZeroLightMod, GlowMods.psZeroLightMod);
                    float upper = Math.Max(GlowMods.nvZeroLightMod, GlowMods.psZeroLightMod);
                    //if greater than the midpoint between the two bonuses
                    if (ZeroLightModifier > ((lower + upper) / 2) - 0.01f)
                    {
                        return GlowMods.Options.NVPhotosensitivity;
                    }
                    //if greater than the midpoint between no bonus and the lower bonus
                    if (ZeroLightModifier > (lower / 2) - 0.01f)
                    {
                        return GlowMods.Options.NVNightVision;
                    }

                }
            }
            return GlowMods.Options.NVNone;
        }
        #endregion

        public void SetDirty()
        {
            naturalGlowMods = null;
            glowmodschanged = true;
            apparelNeedsChecking = true;
            hediffsNeedChecking = true;
        }

        #region Comp overrides

        //TODO Remove this before release
        public override void CompTickRare()
        {
            // Anti Crytosleep casket check
            if (!ParentPawn.Spawned)
            {
                return;
            }
            if (apparelNeedsChecking)
            {
                RecheckApparel();
                apparelNeedsChecking = false;
            }
            if (hediffsNeedChecking)
            {
                CalculateHediffMod();
                hediffsNeedChecking = false;
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
                Log.Message("HediffGlowMods: zero = " + GlowModsFromHediffs.ZeroLight + ", full = " + GlowModsFromHediffs.FullLight);
                Log.Message("NaturalGlowMods: zero = " + NaturalGlowMods.ZeroLight + ", full = " + NaturalGlowMods.FullLight);
                Log.Message("Total Glow Mods: zero = " + ZeroLightModifier + ", full = " + FullLightModifier);
                Log.Message(new string('*', 30));
            }
            
        }


        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            InitPawnsHediffsAndCountEyes();
            InitPawnsApparel();
        }
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
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
            if (ParentPawn.Dead || !ParentPawn.Spawned)
            {
                return;
            }
            NumberOfRemainingEyes = RaceSightParts.Count;
            PawnsNVHediffs.Clear();

            for (int i = 0; i < NumberOfRemainingEyes; i++)
            {
                PawnsNVHediffs[RaceSightParts[i].def.defName] = new List<HediffDef>();
            }
            brainName = ParentPawn.RaceProps.body.GetPartsWithTag(brainTag).First().def.defName;
            PawnsNVHediffs[brainName] = new List<HediffDef>();
            PawnsNVHediffs[bodyKey] = new List<HediffDef>();
            if (!PawnHediffs.NullOrEmpty())
            {
                for (int i = 0; i < PawnHediffs.Count; i++)
                {
                    CheckAndAddHediff(PawnHediffs[i], PawnHediffs[i].Part);
                }
            }

            CalculateHediffMod();
        }

        public void InitPawnsApparel()
        {
            if (ParentPawn.Dead || !ParentPawn.Spawned)
            {
                return;
            }
            ApparelGrantsNV = false;
            ApparelNullsPS = false;
            PawnsNVApparel = new List<Apparel>();
            if (ParentPawn.apparel?.WornApparel is List<Apparel> pawnsApparel)
            {
                for (int i = 0; i < pawnsApparel.Count; i++)
                {
                    if (NightVisionSettings.NVApparel.TryGetValue(pawnsApparel[i].def, out ApparelSetting value))
                    {
                        ApparelGrantsNV |= value.GrantsNV;
                        ApparelNullsPS |= value.NullifiesPS;
                        PawnsNVApparel.Add(pawnsApparel[i]);
                    }
                    else if (NightVisionSettings.AllEyeCoveringHeadgearDefs.Contains(pawnsApparel[i].def))
                    {
                        PawnsNVApparel.Add(pawnsApparel[i]);
                    }
                }
            }
        }
        #endregion

        #region Hediff Updates
        public void RemoveHediff(Hediff hediff, BodyPartRecord part = null)
        {
            if (ParentPawn.Dead || !ParentPawn.Spawned)
            {
                return;
            }
            if (part != null && PawnsNVHediffs.ContainsKey(part.def.defName) && PawnsNVHediffs[part.def.defName].Remove(hediff.def))
            {
                CalculateHediffMod();
                if(part.def.tags.Contains(eyeTag) && (hediff is Hediff_MissingPart || (hediff.def.addedPartProps is AddedBodyPartProps abpp && abpp.isSolid)))
                {
                    NumberOfRemainingEyes++;
                }
            }
            else if (PawnsNVHediffs[bodyKey].Remove(hediff.def))
            {
                CalculateHediffMod();
            }
        }
        //TODO rethink this: what about hediffs that are applied to parts not in PawnsNVHediff keys
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
                        if (NightVisionSettings.HediffGlowMods.ContainsKey(hediffDef))
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
                    if (removedPart && part.def.tags.Contains(eyeTag))
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
                 if (PawnsNVHediffs.TryGetValue(bodyKey, out List<HediffDef> value) && !value.Contains(hediff.def))
                 {
                    PawnsNVHediffs[bodyKey].Add(hediff.def);
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
                //Log.Message("Hediff calc: " + value.ToStringSafeEnumerable());
                if (!value.NullOrEmpty())
                {
                    for (int i = 0; i < value.Count; i++ )
                    {
                        Log.Message("Non empty hediff list in pawn dictionary: " + value.ToStringSafeEnumerable());
                        if (NightVisionSettings.HediffGlowMods.TryGetValue(value[i], out GlowMods hediffSetting))
                        {
                            Log.Message("HEdiffModCalc found glow mod hediff: " + value[i].defName);
                            Log.Message(hediffSetting.Setting.ToString());
                            switch (hediffSetting.Setting)
                            {
                                case GlowMods.Options.NVNone:
                                    continue;
                                case GlowMods.Options.NVCustom:
                                    zeroMod += hediffSetting.ZeroLight;
                                    fullMod += hediffSetting.FullLight;
                                    continue;
                                case GlowMods.Options.NVNightVision:
                                    nvZeroMod += hediffSetting.ZeroLight;
                                    nvFullMod += hediffSetting.FullLight;
                                    continue;
                                case GlowMods.Options.NVPhotosensitivity:
                                    psZeroMod += hediffSetting.ZeroLight;
                                    psFullMod += hediffSetting.FullLight;
                                    continue;
                                default:
                                    continue;
                            }
                        }
                    }
                }
            }
            //Clamp the values for NV & PS so they do not go above or below the values set in the settings menu
            GlowModsFromHediffs.ZeroLight = 
                                zeroMod
                                + Mathf.Clamp(nvZeroMod, Math.Min(GlowMods.nvZeroLightMod, 0f), Math.Max(GlowMods.nvZeroLightMod, 0f))
                                + Mathf.Clamp(psZeroMod, Math.Min(GlowMods.psZeroLightMod, 0f), Math.Max(GlowMods.psZeroLightMod, 0f));
            GlowModsFromHediffs.FullLight = 
                                fullMod
                                + Mathf.Clamp(nvFullMod, Math.Min(GlowMods.nvFullLightMod, 0f), Math.Max(GlowMods.nvFullLightMod, 0f))
                                + Mathf.Clamp(psFullMod, Math.Min(GlowMods.psFullLightMod, 0f), Math.Max(GlowMods.psFullLightMod, 0f));
            glowmodschanged = true;
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
                ApparelGrantsNV = false;
                ApparelNullsPS = false;
                //TODO redesign: what about when the user changes settings mid-game??
                RecheckApparel();
            }
        }

        public void RecheckApparel()
        {
            for (int i = PawnsNVApparel.Count - 1; i >= 0; i--)
            {
                if (NightVisionSettings.NVApparel.TryGetValue(PawnsNVApparel[i].def, out ApparelSetting value))
                {
                    ApparelGrantsNV |= value.GrantsNV;
                    ApparelNullsPS |= value.NullifiesPS;
                }
            }
        }
        #endregion
        
    }


}
