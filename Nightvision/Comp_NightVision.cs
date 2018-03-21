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

        public const float lowerLightLimitForGlowMod = 0.3f;
        public const float upperLightLimitForGlowMod = 0.7f;

        private bool UpdateIsDirty = false;
        private bool GlowModIsDirty = false;
        private float zeroLightFactor;
        private float fullLightFactor;
        private Pawn intParentPawn;
        private List<BodyPartRecord> raceSightParts;
        private HediffSet pawnHediffSet;
        private float[] naturalGlowMods = new float[2];


        public CompProperties_NightVision Props => (CompProperties_NightVision)props;

        /// <summary>
        /// A string list of NV stuff; adds without checking, uses List.Distinct() to trim.
        /// TODO Add an id? to each item so List.Distinct() doesn't remove legitimate dupes?
        /// </summary>
        public List<String> NVEffectorsAsListStr = new List<String>();


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

        public HediffSet PawnHediffSet
        {
            get
            {
                if (pawnHediffSet == null)
                {
                    pawnHediffSet = ParentPawn.health.hediffSet;
                }
                return pawnHediffSet;
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

        public float[] NaturalGlowModsPerEye
        {
            get
            {
                if (naturalGlowMods.Length == 0 || GlowModIsDirty)
                {
                    IntRange temp = NightVisionSettings.RaceNightVisionFactors[this.ParentPawn.def];
                    naturalGlowMods[0] = CalcModifierFromFactor(temp.min);
                    naturalGlowMods[1] = CalcModifierFromFactor(temp.max);
                }
                return naturalGlowMods;
            }
        }


        public float ZeroLightFactor
        {
            get
            {
                if (zeroLightFactor == 0)
                {
                    Update();
                }
                return zeroLightFactor;
            }
            set
            {
                zeroLightFactor = value;
            }
        }
        public float FullLightFactor
        {
            get
            {
                if (fullLightFactor == 0)
                {
                    Update();
                }
                return fullLightFactor;
            }
            set
            {
                fullLightFactor = value;
            }
        }

        public float FactorFromGlow(float glow)
        {
            if (glow < lowerLightLimitForGlowMod)
            {
                return Mathf.Lerp(ZeroLightFactor, 1f, glow / lowerLightLimitForGlowMod);
            }
            else if (glow > upperLightLimitForGlowMod && FullLightFactor != 1f)
            {
                return Mathf.Lerp(1f, FullLightFactor, (glow - upperLightLimitForGlowMod) / (1f - upperLightLimitForGlowMod));
            }

            return 1f;
        }

        public void SetDirty()
        {
            UpdateIsDirty = true;
            GlowModIsDirty = true;
        }

        #region Comp overrides


        public override void CompTickRare()
        {
            // Anti Crytosleep casket check
            if (!ParentPawn.Spawned)
            {
                return;
            }
            Update();
        }
        

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            Update();
        }
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
        }
        #endregion

        #region Update Comp
        /// <summary>
        /// Checks the eyes and updates the pawns glow curves
        /// Also adds any relevant hediffs to the  list for explanation part
        /// </summary>
        public void Update()
        {
            int num_NVeyes = 0;
            int num_PSeyes = 0;
            int total_num_eyes = RaceSightParts.Count();
            if (total_num_eyes <= 0)
            {
                return;
            }
            
            List<Hediff> hedifflist = PawnHediffSet.hediffs;
            
            if (hedifflist.NullOrEmpty())
            {
                if (zeroLightFactor == 0f || fullLightFactor == 0f || UpdateIsDirty)
                {
                    zeroLightFactor = NightVisionSettings.RaceNightVisionFactors[this.ParentPawn.def].min;
                    fullLightFactor = NightVisionSettings.RaceNightVisionFactors[this.ParentPawn.def].max;
                }
                //TODO Does this need to check Photosensitivity?
                NVApparelCheck(true, true);
                return;
            }

            //TODO Consider iterating over the RaceSightParts instead: potentially avoiding edge case where two hediffs apply to the same eye
            //part in racesightparts (select hediff where hediff.part == part).first? or some form of choose best hediff?
            
            foreach (Hediff hediff in hedifflist.Where(hd => RaceSightParts.Contains(hd.Part)))
            {
                //Check if the eye is missing
                if (hediff is Hediff_MissingPart)
                {
                    total_num_eyes--;
                }
                //Check if the eye has nightvision
                else if (NightVisionSettings.NightVisionHediffDefs.Contains(hediff.def))
                {
                    NVEffectorsAsListStr.Add(hediff.Label);
                    num_NVeyes++;

                }
                //Check if the eye is photosensitive
                else if (NightVisionSettings.PhotosensitiveHediffDefs.Contains(hediff.def))
                {
                    NVEffectorsAsListStr.Add(hediff.Label);
                    num_PSeyes++;
                }
                //Check if the eye has been replaced by something that is not a solid bionic
                else if (hediff is Hediff_AddedPart && (hediff.def.addedPartProps?.isSolid ?? false))
                {
                    total_num_eyes--;
                }

            }

            //Shouldn't have negative eyes
            int num_NaturalEyes = Math.Max(0, total_num_eyes - (num_NVeyes + num_PSeyes));
            
            //Calc the multiplier for zero light
            
            //Sum all factors and add to base value (0.8f == 80%)
            //Start at 0.8: considered starting at 1f but then natural eyes would have to be neg. which would
            //have lead to the odd situation of a pawn with only one eye that was bionic having better nightvision
            //than a pawn with two eyes, one normal and one bionic

            ZeroLightFactor = 0.8f + (num_PSeyes* CalcModifierFromFactor(Controller.Settings.PhotosensitiveLightFactors.min)
                                                + num_NVeyes*CalcModifierFromFactor(Controller.Settings.BionicLightFactors.min)     
                                                + num_NaturalEyes * NaturalGlowModsPerEye[0]);

            //Calc the multiplier for max light
            
            //Start from 1f, same as above, except we ignore NightVision eyes (i.e. bionics)
            FullLightFactor = 1f + (num_PSeyes * CalcModifierFromFactor(Controller.Settings.PhotosensitiveLightFactors.max)
                                                + num_NaturalEyes * NaturalGlowModsPerEye[1]);


            //Call apparel checker
            NVApparelCheck(ZeroLightFactor < 1f, FullLightFactor < 1f);

            NVEffectorsAsListStr = NVEffectorsAsListStr.Distinct().ToList();


        }
        #endregion

        #region Apparel Checkers
        //public void 

            /// <summary>
            /// Apparel checker. Checks apparel for nightvision granters / photosensitivity nullifiers.
            /// Void: Edits the parent comp directly: specifically the parent pawn's glow curve.
            /// Only necessary if either max glow factor / zero glow factor are less than one
            /// Does nothing if both params are false
            /// </summary>
            /// <param name="checkNV">If max glow factor is less than one</param>
            /// <param name="checkPS">If zero glow factor is less than one</param>
        public void NVApparelCheck(bool checkNV = true, bool checkPS = true)
        {
            if (!checkNV & !checkPS)
            {
                return;
            }
            if (ParentPawn.apparel != null && ParentPawn.apparel.WornApparelCount > 0)
            {
                bool hasNVApparel = false;
                foreach (KeyValuePair<ThingDef, ApparelSetting> kvp in ParentPawn.apparel.WornApparel.Where(app => NightVisionSettings.NVApparel.ContainsKey(app.def))
                    .Select(app => { return new KeyValuePair<ThingDef, ApparelSetting>(app.def, NightVisionSettings.NVApparel[app.def]); }))
                {
                    if (checkNV && kvp.Value.GrantsNV)
                    {
                        NVEffectorsAsListStr.Add(kvp.Key.LabelCap);
                        if (ZeroLightFactor < 1f )
                        {
                            ZeroLightFactor = 1f;
                            hasNVApparel = true;
                        }
                    }
                    if (checkPS && kvp.Value.NullifiesPS)
                    {
                        NVEffectorsAsListStr.Add(kvp.Key.LabelCap);

                        if (FullLightFactor < 1f)
                        {
                            FullLightFactor = 1f;
                            hasNVApparel = true;
                        }
                    }
                }
                if (hasNVApparel)
                {
                    NVEffectorsAsListStr = NVEffectorsAsListStr.Distinct().ToList();
                    return;
                }
            }
            return;
        }

        #endregion
        public float CalcModifierFromFactor(int factor)
        {
            return (((float)factor / 100) - 0.8f) / 2f;
        }
    }


}
