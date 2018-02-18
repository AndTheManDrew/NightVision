using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace NightVision
{


    class Comp_NightVision : ThingComp
    {

        private const string eyeTag = "SightSource";
        private const float zeroglownightVisionBonusPerEye = 0.1f;
        private const float fullglownightVisionBonusPerEye = 0f;
        private const float glowFactorCap = 1.2f;
        private const float glowFactorFloor = 0.8f;
        private const float nightVisionZeroLightCap = 1f;

        private Pawn parentPawnInt;
        private SimpleCurve pawnsGlowCurve;
        private List<BodyPartRecord> raceSightParts;
        private HediffSet pawnHediffSet;
        private float[] naturalGlowfactors;

        public bool NaturalNightvision => Props.naturalNightVision;

        public CompProperties_NightVision Props => (CompProperties_NightVision)props;
        public List<Hediff> NV_hediffs = new List<Hediff>();
        public Pawn ParentPawn
        {
            get
            {
                if (parentPawnInt == null)
                {
                    parentPawnInt = parent as Pawn;
                }
                return parentPawnInt;
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
                    raceSightParts = /*(List<BodyPartRecord>)*/ParentPawn.RaceProps.body.AllParts.Where(part => part.def.tags.Contains(eyeTag)).ToList();
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
        public SimpleCurve PawnsGlowCurve
        {
            get
            {
                if (pawnsGlowCurve == null)
                {
                    pawnsGlowCurve = new SimpleCurve();

                    if (Props.naturalGlowfactors != null)
                    {
                        naturalGlowfactors = new float[2] { Props.naturalGlowfactors[0] , Props.naturalGlowfactors[1] };
                        pawnsGlowCurve.SetPoints(
                            new CurvePoint[]
                            {
                                new CurvePoint(0f, naturalGlowfactors[0]),     
                                new CurvePoint(0.3f,1f),
                                new CurvePoint(0.7f, 1f),
                                new CurvePoint(1f, naturalGlowfactors[1]),
                            });
                    }
                    else if (Props.naturalNightVision)
                    {
                        naturalGlowfactors = new float[2] { 1f, 1f };
                        pawnsGlowCurve.SetPoints(
                            new CurvePoint[]
                            {
                                new CurvePoint(0f, naturalGlowfactors[0]),
                                new CurvePoint(0.3f,1f),
                                new CurvePoint(0.7f, 1f),
                                new CurvePoint(1f, naturalGlowfactors[1]),
                            });
                    }
                    else
                    {
                        pawnsGlowCurve.SetPoints(NightVisionMod.Instance.HumanGlowCurve.ToList());
                        //Just checking if the curves are the same object or not
                        Log.Message("Human glow curve before editting pawns glow curve: ");
                        Log.Message(NightVisionMod.Instance.HumanGlowCurve.ToStringSafeEnumerable());
                        pawnsGlowCurve.Add(0.3f, 0f);
                        Log.Message("and after editing pawns glow curve: ");
                        Log.Message(NightVisionMod.Instance.HumanGlowCurve.ToStringSafeEnumerable());
                        Log.Message("Removing added point from pawns glow curve.");
                        pawnsGlowCurve.RemovePointNear(new CurvePoint(0.3f, 0f));
                        naturalGlowfactors = new float[2] { pawnsGlowCurve.Evaluate(0f), pawnsGlowCurve.Evaluate(1f) };
                    }
                }
                return pawnsGlowCurve;
            }
            set => pawnsGlowCurve = value;
        }

        

        public override void CompTickRare()
        {
            Update();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (PawnsGlowCurve == null)
            {
                return;
            }
            Update();
        }
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
        }
        #region Eye Checker
        /// <summary>
        /// Checks the eyes and updates the comp's count of nightvision, photosensitive, and normal eyes.
        /// Also adds any relevant hediffs to the corresponding list that is referenced in the params
        /// </summary>
        /// <param name="NV_hediffs">Reference to List of nightvision hediffs pawn has</param>
        /// <param name="PS_hediffs">Reference to List of photosensitive hediffs pawn has</param>
        public void Update( /*ref List<Hediff> NV_hediffs ref List<Hediff> PS_hediffs*/ )
        {
            int num_NVeyes = 0;
            int num_PSeyes = 0;
            int total_num_eyes = RaceSightParts.Count();
            Log.Message("1");
            NV_hediffs.Clear();
            List<Hediff> hedifflist = PawnHediffSet.hediffs;
            Log.Message("2");
            if (hedifflist == null)
            {
                return;
            }
            Log.Message("3");
            foreach (Hediff hediff in hedifflist.Where(hd => RaceSightParts.Contains(hd.Part)))
            {
                //REMOVE THIS
                Log.Message($"NV: matched {ParentPawn}'s hediff {hediff.Label} to race bodypart {RaceSightParts.Find(rSP => rSP == hediff.Part).def.label}");
                //REMOVE THIS

                //Check if the eye is missing
                if (hediff is Hediff_MissingPart)
                {
                    Log.Message(ParentPawn.LabelShort + "has a missing eye");
                    total_num_eyes--;
                }
                //Check if the eye has nightvision
                else if (NightVisionMod.Instance.ListofNightVisionHediffDefs.Contains(hediff.def))
                {
                    NV_hediffs.Add(hediff);
                    num_NVeyes++;

                }
                //Check if the eye is photosensitive
                else if (NightVisionMod.Instance.ListofPhotosensitiveHediffDefs.Contains(hediff.def))
                {
                    NV_hediffs.Add(hediff);
                    //PS_hediffs.Add(hediff);
                    num_PSeyes++;
                }
                //Check if the eye has been replaced by something that is not an implant
                else if (hediff is Hediff_AddedPart && (hediff.def.addedPartProps?.isSolid ?? false))
                {
                    total_num_eyes--;
                }

            }
            //Shouldn't have negative eyes
            int num_NaturalEyes = Math.Max(0, total_num_eyes - (num_NVeyes + num_PSeyes));
            Log.Message("4");
            //Calc the multiplier for zero light

            //Get the modifier for each eye from race
            float ZeroGl_NaturalEyeMod = (naturalGlowfactors[0] - 0.8f) / 2f;
            //Sum all factors and add to base value (0.8f == 80%)
            //Start at 0.8: considered starting at 1f but then natural eyes would have to be neg. which would
            //have lead to the odd situation of a pawn with only one eye that was bionic having better nightvision
            //than a pawn with two eyes, one normal and one bionic
            Log.Message("5");
            float ZeroGlowfactor = 0.8f + (num_PSeyes*NightVisionMod.Instance.PhotosensitiveGlowfactorsPerEye[0]
                                                + num_NVeyes*zeroglownightVisionBonusPerEye     
                                                + num_NaturalEyes * ZeroGl_NaturalEyeMod);

            //Calc the multiplier for max light
            Log.Message("6");
            //Get the modifier for each eye from race
            float MaxGL_NaturalEyeMod = (naturalGlowfactors[1] - 1f) / 2f;
            //Start from 1f, same as above, except we ignore NightVision eyes (i.e. bionics)
            float MaxGlowFactor = 1f + (num_PSeyes * NightVisionMod.Instance.PhotosensitiveGlowfactorsPerEye[1]
                                                + num_NaturalEyes * MaxGL_NaturalEyeMod);
            Log.Message("7");

            //Check apparel
            if (ZeroGlowfactor < 1f && HasNVGoggles())
            {
                ZeroGlowfactor = 1f;
            }

            if (MaxGlowFactor < 1f && HasShades())
            {
                MaxGlowFactor = 1f;
            }
            Log.Message("8");
            pawnsGlowCurve.SetPoints(
                            new CurvePoint[]
                            {
                                new CurvePoint(0f, ZeroGlowfactor),
                                new CurvePoint(0.3f,1f),
                                new CurvePoint(0.7f, 1f),
                                new CurvePoint(1f, MaxGlowFactor),
                            });




        }
        #endregion


        public bool HasNVGoggles()
        {
            List<Apparel> applist = ParentPawn.apparel.WornApparel;
            if (!applist.NullOrEmpty()
                    && applist.Exists(
                        app => app.TryGetComp<Comp_NightVisionApparel>() != null
                            && app.TryGetComp<Comp_NightVisionApparel>().Props.grantsNightVision))
            {
                return true;
            }
            return false;
        }
        //public bool HasNVGogglesEx(ref Dictionary<string, float> NV_effects)
        //{
        //    return false;
        //}

        public bool HasShades()
        {
            List<Apparel> applist = ParentPawn.apparel.WornApparel;
            if (!applist.NullOrEmpty()
                    && applist.Exists(
                        app => app.TryGetComp<Comp_NightVisionApparel>() != null
                            && app.TryGetComp<Comp_NightVisionApparel>().Props.nullifiesPhotosensitivity))
            {
                return true;
            }
            return false;
        }

        //public bool HasShadesEx(ref Dictionary<string, float> NV_effects)
        //{
        //    return false;
        //}
    }
}
