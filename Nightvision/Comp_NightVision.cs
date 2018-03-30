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
        public const string eyeTag = "SightSource";
        public const string brainTag = "ConsciousnessSource";
        public const string bodyKey = "WholeBody";
        
        public const float lowerLightLimitForGlowMod = 0.3f;
        public const float upperLightLimitForGlowMod = 0.7f;

        private float numRemainingEyes = -1;
        private Pawn intParentPawn;
        private List<BodyPartRecord> raceSightParts;

        private List<Hediff> pawnsHediffs;

        public CompProperties_NightVision Props => (CompProperties_NightVision)props;


        public List<Apparel> PawnsNVApparel = new List<Apparel>();
        //Should store all sight sources in here
        public Dictionary<string, List<HediffDef>> PawnsNVHediffs = new Dictionary<string, List<HediffDef>>();

        private EyeGlowMods naturalGlowMods;
        private GlowMods HediffMods = new GlowMods();
        private string brainName;

        private bool glowmodschanged = false;
        private bool ApparelGrantsNV = false;
        private bool ApparelNullsPS = false;
        private float zeroLightMultiplier;
        private float fullLightMultiplier;


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
        public float NumberOfRemainingEyes
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

        /// <summary>
        /// The glow mods of the pawns races eyes
        /// Null Checks and Check for if it has been set: if either is false, gets default race value divided by race's number of eyes
        /// </summary>
        public EyeGlowMods NaturalGlowMods
        {
            get
            {
                if (naturalGlowMods == null || naturalGlowMods.IsNotSet())
                {
                    naturalGlowMods = new EyeGlowMods(NightVisionSettings.GetRaceNightVisionMod(ParentPawn.def), RaceSightParts.Count);
                }
                return naturalGlowMods;
            }
            private set
            {
                if (naturalGlowMods.Equals(value))
                {
                    return;
                }
                value = naturalGlowMods;
            }
        }


        public float ZeroLightMultiplier
        {
            get
            {
                if (glowmodschanged || zeroLightMultiplier == 0)
                {
                    zeroLightMultiplier =  (float)Math.Round(NightVisionSettings.DefaultZeroLightMultiplier + HediffMods.ZeroLight + (NaturalGlowMods.ZeroLight * NumberOfRemainingEyes), 2, MidpointRounding.AwayFromZero);
                    
                }
                //Default FULL light multiplier is NOT a mistake ( it == 1f i.e. night vision)
                if (ApparelGrantsNV && zeroLightMultiplier < NightVisionSettings.DefaultFullLightMultiplier)
                {
                    return NightVisionSettings.DefaultFullLightMultiplier;
                }
                return zeroLightMultiplier;
            }
        }
        public float FullLightMultiplier
        {
            get
            {
                if (glowmodschanged || fullLightMultiplier == 0)
                {
                    fullLightMultiplier = (float)Math.Round(NightVisionSettings.DefaultFullLightMultiplier + HediffMods.FullLight + (NaturalGlowMods.FullLight * NumberOfRemainingEyes), 2, MidpointRounding.AwayFromZero);
                }
                if (ApparelNullsPS && fullLightMultiplier < NightVisionSettings.DefaultFullLightMultiplier)
                {
                    return NightVisionSettings.DefaultFullLightMultiplier;
                }
                return fullLightMultiplier;
            }
        }

        public float FactorFromGlow(float glow)
        {
            if (glow < lowerLightLimitForGlowMod)
            {
                return Mathf.Lerp(ZeroLightMultiplier, 1f, glow / lowerLightLimitForGlowMod);
            }
            else if (glow > upperLightLimitForGlowMod && FullLightMultiplier != 1f)
            {
                return Mathf.Lerp(1f, FullLightMultiplier, (glow - upperLightLimitForGlowMod) / (1f - upperLightLimitForGlowMod));
            }
            return 1f;
        }

        public void SetDirty()
        {
            NaturalGlowMods.Reset();
            glowmodschanged = true;
        }

        #region Comp overrides


        public override void CompTickRare()
        {
            // Anti Crytosleep casket check
            if (!ParentPawn.Spawned /*|| this.Active == false*/)
            {
                return;
            }
            Log.Message("NightVisionComp: " + ParentPawn.NameStringShort);
            Log.Message("RaceSightParts: " + RaceSightParts.ToStringSafeEnumerable());
            Log.Message("PawnsNVHediffs: " + PawnsNVHediffs.ToStringFullContents());
            Log.Message("NumberRemainingEyes: " + NumberOfRemainingEyes);
            Log.Message("HediffGlowMods: " + HediffMods.ZeroLight + ", " + HediffMods.FullLight);
            Log.Message("NaturalGlowMods: " + NaturalGlowMods.ZeroLight + ", " + NaturalGlowMods.FullLight);

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
        
        public void RemoveHediff(Hediff hediff, BodyPartRecord part = null)
        {
            if (ParentPawn.Dead || !ParentPawn.Spawned)
            {
                return;
            }
            if (part != null && PawnsNVHediffs.ContainsKey(part.def.defName) && PawnsNVHediffs[part.def.defName].Remove(hediff.def))
            {
                CalculateHediffMod();
                //TODO Check removing bionic eye works with this
                if(part.def.tags.Contains(eyeTag) && (hediff is Hediff_MissingPart || (hediff.def.addedPartProps is AddedBodyPartProps abpp && abpp.isSolid)))
                {
                    numRemainingEyes++;
                }
            }
            else if (PawnsNVHediffs[bodyKey].Remove(hediff.def))
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
                Log.Message("Night Vision: Called CheckAndAddHediff on: " + part + " with " + hediff);
                string partName = part.def.defName;
                if (PawnsNVHediffs.TryGetValue(partName, out List<HediffDef> partsHediffDefs))
                {
                    //Categorise the hediff:
                    //MissingPart overrides everything
                    bool removedPart = false;

                    if (hediff is Hediff_MissingPart)
                    {
                        removedPart = true;
                        partsHediffDefs = new List<HediffDef>
                        {
                            HediffDefOf.MissingBodyPart
                        };
                    }
                    else
                    {
                        //Check if there is a setting for it
                        HediffDef hediffDef = hediff.def;
                        if (NightVisionSettings.NVHediffs.ContainsKey(hediffDef))
                        {
                            if (hediffDef.addedPartProps is AddedBodyPartProps abpp && abpp.isSolid)
                            {
                                removedPart = true;
                                partsHediffDefs = new List<HediffDef>
                                {
                                    hediffDef
                                };  
                            }
                            else if (!partsHediffDefs.Contains(hediffDef))
                            {
                                partsHediffDefs.Add(hediffDef);
                            }
                        }
                        //Last two checks are in case the user changes settings later
                        //Is it a solid replacement?
                        else if (hediffDef.addedPartProps is AddedBodyPartProps abpp && abpp.isSolid)
                        {
                            removedPart = true;
                            partsHediffDefs = new List<HediffDef>
                            {
                                hediffDef
                            };
                        }
                        //Rule stuff out
                        else if (!(hediff is Hediff_Injury) && !(hediff is Hediff_Pregnant) && !(hediff is Hediff_HeartAttack)
                                && !(hediff is Hediff_Alcohol) && !(hediff is Hediff_Hangover))
                        {
                             if (!partsHediffDefs.Contains(hediffDef))
                             {
                                partsHediffDefs.Add(hediffDef);
                             }
                        }
                    }
                    //remove an eye if the part is not the brain
                    if (removedPart && partName != brainName)
                    {
                        numRemainingEyes--;
                    }
                CalculateHediffMod();
                }
                else
                {
                    Log.Message("...but did not add to NVHediffs dictionary.");
                }
            }
            //TODO Maybe not include this check?
            else if (!(hediff is Hediff_Injury) && !(hediff is Hediff_Pregnant) && !(hediff is Hediff_HeartAttack)
                                && !(hediff is Hediff_Alcohol) && !(hediff is Hediff_Hangover))
            {
                 if (PawnsNVHediffs.TryGetValue(bodyKey, out List<HediffDef> value) && !value.Contains(hediff.def))
                 {
                    PawnsNVHediffs[bodyKey].Add(hediff.def);
                    CalculateHediffMod();
                 }
            }
        }
        private void CalculateHediffMod()
        {
            float zerolightmod = new float();
            float fulllightmod = new float();
            foreach (List<HediffDef> value in PawnsNVHediffs.Values)
            {
                if (!value.NullOrEmpty())
                {
                    for (int i = 0; i < value.Count; i++ )
                    {
                        if (NightVisionSettings.NVHediffs.TryGetValue(value[i], out FloatRange hediffSetting))
                        {
                            zerolightmod += hediffSetting.min;
                            fulllightmod += hediffSetting.max;
                        }
                    }
                }
            }
            if (HediffMods.IsNotSet() 
                || !Mathf.Approximately(HediffMods.ZeroLight, zerolightmod) 
                || !Mathf.Approximately(HediffMods.FullLight, fulllightmod))
            {
                HediffMods.ZeroLight = zerolightmod;
                HediffMods.FullLight = fulllightmod;
                glowmodschanged = true;
            }
        }

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
            if (HediffMods.IsNotSet())
            {
                CalculateHediffMod();
            }
        }

        public void InitPawnsApparel()
        {
            if (ParentPawn.Dead || !ParentPawn.Spawned)
            {
                return;
            }
            ApparelGrantsNV = false;
            ApparelNullsPS = false;
            PawnsNVApparel.Clear();
            if (ParentPawn.apparel?.WornApparel is List<Apparel> pawnsApparel)
            {
                for (int i = 0; i < pawnsApparel.Count; i++)
                {
                    if (NightVisionSettings.NVApparel.TryGetValue(pawnsApparel[i].def, out ApparelSetting value))
                    {
                        ApparelGrantsNV |= value.GrantsNV;
                        ApparelNullsPS |= value.NullifiesPS;
                        PawnsNVApparel.Add(pawnsApparel[i]);
                        if (ApparelGrantsNV && ApparelNullsPS)
                        {
                            break;
                        }
                    }
                }
            }
        }

        public void CheckAndAddApparel(Apparel apparel)
        {
            if (ParentPawn.Dead || !ParentPawn.Spawned || apparel == null || (ApparelGrantsNV && ApparelNullsPS))
            {
                return;
            }
            if (NightVisionSettings.NVApparel.TryGetValue(apparel.def, out ApparelSetting value))
            {
                ApparelGrantsNV |= value.GrantsNV;
                ApparelNullsPS |= value.NullifiesPS;
                PawnsNVApparel.Add(apparel);
            }
        }
        public void RemoveApparel(Apparel apparel)
        {
            if (ParentPawn.Dead || !ParentPawn.Spawned || apparel == null)
            {
                return;
            }
            if (PawnsNVApparel.Contains(apparel) && !(ApparelGrantsNV || ApparelNullsPS))
            {
                PawnsNVApparel.Remove(apparel);
                Log.Message("Apparel was in PawnsNVApparel but pawn did not have any flags? " + ParentPawn.NameStringShort + " - " + apparel.Label);
                Log.Message("Were the settings changed?");
                return;
            }
            if (!(ApparelGrantsNV || ApparelNullsPS))
            {
                return;
            }
            if (PawnsNVApparel.Contains(apparel))
            {
                PawnsNVApparel.Remove(apparel);
                ApparelGrantsNV = false;
                ApparelNullsPS = false;
                //TODO redesign
                for (int i = PawnsNVApparel.Count -1; i >= 0; i--)
                {
                    if (NightVisionSettings.NVApparel.TryGetValue(PawnsNVApparel[i].def, out ApparelSetting value))
                    {
                        ApparelGrantsNV |= value.GrantsNV;
                        ApparelNullsPS |= value.NullifiesPS;
                    }
                    else
                    {
                        PawnsNVApparel.RemoveAt(i);
                    }
                }
            }
        }
        //public void RecheckPawns
    }


}
