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
        private Pawn parentPawnInt;
        private SimpleCurve pawnsGlowCurve;
        private SimpleCurve naturalGlowCurve;
        private const string eyeTag = "SightSource";
        private List<BodyPartRecord> raceSightParts;
        private HediffSet pawnHediffSet;

        public int numOfEyesWithNightVision;
        public int numOfEyesWithPhotoSensitivity;
        public int totalnumOfEyesNotMissing;

        public bool NaturalNightvision => Props.naturalNightVision;

        public CompProperties_NightVision Props => (CompProperties_NightVision)props;

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
                    raceSightParts = (List<BodyPartRecord>)ParentPawn.RaceProps.body.AllParts.Where(part => part.def.tags.Contains(eyeTag));
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
                    pawnsGlowCurve = Props.naturalGlowCurve ?? NightVisionMod.Instance.HumanGlowCurve;
                }
                return pawnsGlowCurve;
            }
            set => pawnsGlowCurve = value;
        }

        

        public override void CompTickRare()
        {
            UpdateEyeHediffs();
        }

        public override void Initialize(CompProperties props)
        {
            //Add race check for naturalGlowCurve??
            //Add check for NaturalNightVision??
            //Add check for races norm num of eyes - make a list of them
            //Grab Pawns hediff set
            
        }
        #region Eye Checker

        /// <summary>
        /// Checks the eyes and updates the comp's count of nightvision, photosensitive, and normal eyes.
        /// </summary>
        public void UpdateEyeHediffs()
        {
            int num_NVeyes = 0;
            int num_PSeyes = 0;
            int total_num_eyes = RaceSightParts.Count();

            List<Hediff> hedifflist = PawnHediffSet.hediffs;
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
                    num_NVeyes++;

                }
                //Check if the eye is photosensitive
                else if (NightVisionMod.Instance.ListofPhotosensitiveHediffDefs.Contains(hediff.def))
                {
                    num_PSeyes++;
                }

            }
            //Update the fields of the comp
            numOfEyesWithNightVision = num_NVeyes;
            numOfEyesWithPhotoSensitivity = num_PSeyes;
            totalnumOfEyesNotMissing = total_num_eyes;
        }
        /// <summary>
        /// Checks the eyes and updates the comp's count of nightvision, photosensitive, and normal eyes.
        /// Also adds any relevant hediffs to the corresponding list that is referenced in the params
        /// </summary>
        /// <param name="NV_hediffs">Reference to List of nightvision hediffs pawn has</param>
        /// <param name="PS_hediffs">Reference to List of photosensitive hediffs pawn has</param>
        public void UpdateEyeHediffsAndExplain(ref List<Hediff> NV_hediffs, ref List<Hediff> PS_hediffs)
        {
            int num_NVeyes = 0;
            int num_PSeyes = 0;
            int total_num_eyes = RaceSightParts.Count();

            List<Hediff> hedifflist = PawnHediffSet.hediffs;
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
                    PS_hediffs.Add(hediff);
                    num_PSeyes++;
                }

            }
            ////Update the fields of the comp
            //numOfEyesWithNightVision = num_NVeyes;
            //numOfEyesWithPhotoSensitivity = num_PSeyes;
            //totalnumOfEyesNotMissing = total_num_eyes;

            //This should update the pawns simple curve
            int numNormaleyes = total_num_eyes - (num_NVeyes + num_PSeyes);
            float pawnglowfactorFullLight = NightVisionMod.Instance.
            
        }
        #endregion


        public float? FactorFromNightVisionExpanded(ref Dictionary<string, float> NV_effects)
        {
            return 1f;
        }

        public float? FactorFromNightVision()
        {
            //called from outside the comp by the harmony patches
            //calls HasNightVisionApparel if glowfactor is less than one
            return null;
        }

        public bool HasNightVisionApparel()
        {
            //Include check if glow factor is less than one: check for nightvision granters and photosensitive nullify
            return false;
        }

        public bool HasNightVisionApparelExpanded(ref Dictionary<string, float> NV_effects)
        {
            return false;
        }
    }
}
