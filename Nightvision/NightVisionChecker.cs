//using System;
//using System.Reflection;
//using RimWorld;
//using Verse;
//using Harmony;
//using System.Linq;
//using System.Collections.Generic;

//namespace NightVision
//{

//    /// <summary>
//    /// Holds all the night vision calculations that need to be performed on a pawn.
//    /// </summary>
//    static class NightVisionChecker
//    {

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="pawn"></param>
//        /// <returns></returns>
//        public static float? NightVisionFactor(Pawn pawn)
//        {
//            const string eyeTag = "SightSource";
//            int numPhotosensEyes = 0;
//            int numBionicEyes = 0;
//            float preglowFactor = 1f;
//            float lowerGlowBoundary = 0.3f;

//            int numOfPawnEyesNotMissing = pawn.RaceProps.body.GetPartsWithTag(eyeTag).Count();

//            foreach (Hediff hediff in pawn.health.hediffSet.hediffs.Where(hd =>
//            {
//                Log.Message("NVC_Predicate_1");
//                bool? res = hd.Part?.def.tags.Contains(eyeTag);
//                return res.GetValueOrDefault();
//            }))
//            {
//                if (hediff is Hediff_MissingPart)
//                {
//                    numOfPawnEyesNotMissing--;
//                }

//                else if (NightVisionMod.Instance.ListofNightVisionHediffDefs.Contains(hediff.def))
//                {
//                    numBionicEyes++;
//                }

//                else if (hediff.def.defName == "NV_TapetumL")
//                {
//                    numPhotosensEyes++;
//                }

//            }
//            float glowLevelAtPawn = GlowLevelAtPawn(pawn);
//            float? nullable_nightVisionFactor = NightVisionFactorResolver(pawn, numPhotosensEyes, numBionicEyes, numOfPawnEyesNotMissing, glowLevelAtPawn);
//            if (!nullable_nightVisionFactor.HasValue)
//            {
//                return null;
//            }

//            float nightVisionFactor = nullable_nightVisionFactor.Value;
//            if (nightVisionFactor < 1 && glowLevelAtPawn < lowerGlowBoundary && HasNightVisionGoggles(pawn))
//            {
//                nightVisionFactor = preglowFactor;
//            }

//            return nightVisionFactor;
//        }


//        //Overloaded for the explanation part, essentially the same as above but records each hediff or apparel that affects nightvision
//        public static float? NightVisionFactor(Pawn pawn, ref Dictionary<string,float> NV_effectors)
//        {
//            const string eyeTag = "SightSource";
//            int numPhotosensEyes = 0;
//            int numBionicEyes = 0;
//            float preglowFactor = 1f; 
//            float lowerGlowBoundary = 0.3f; //The glow level at which nightvision stuff starts i.e. light < 30%

//            int numOfPawnEyesNotMissing = pawn.RaceProps.body.GetPartsWithTag(eyeTag).Count();

//            foreach (Hediff hediff in pawn.health.hediffSet.hediffs.Where(hd =>
//            {
//                bool? res = hd.Part?.def.tags.Contains(eyeTag);
//                return res.GetValueOrDefault();
//            }))
//            {
//                if (hediff is Hediff_MissingPart)
//                {
//                    numOfPawnEyesNotMissing--;
//                }

//                else if (NightVisionMod.Instance.ListofNightVisionHediffDefs.Contains(hediff.def))
//                {
//                    numBionicEyes++;
//                }

//                else if (hediff.def.defName == "NV_TapetumL")
//                {
//                    numPhotosensEyes++;
//                }

//            }
//            float glowLevelAtPawn = GlowLevelAtPawn(pawn);
//            float? nullable_nightVisionFactor = NightVisionFactorResolver(pawn, numPhotosensEyes, numBionicEyes, numOfPawnEyesNotMissing, glowLevelAtPawn);
//            if (!nullable_nightVisionFactor.HasValue)
//            {
//                return null;
//            }
            
//            float nightVisionFactor = nullable_nightVisionFactor.Value;
            
//            string goggles = null;
//            if (nightVisionFactor < 1 && glowLevelAtPawn < lowerGlowBoundary &&  HasNightVisionGoggles(pawn, ref goggles))
//            {
//                float diff = preglowFactor - nightVisionFactor;
//                nightVisionFactor = preglowFactor;
//                NV_effectors.Add(goggles, diff);
//            }

//            return nightVisionFactor;
//        }


//        private static float? NightVisionFactorResolver(Pawn pawn, int numPhotosensEyes, int numBionicEyes, int numOfPawnEyesNotMissing, float glowLevelAtPawn)
//        {
//            float nightVisionFactor;
//            const float glowfactorforBionic = 1f;
            
//            //This might need to be a parameter or something if race specific night vision is implemented
//            float glowfactorforNormal = 1f;

//            //Number of normal eyes
//            int numRemainderEyes = numOfPawnEyesNotMissing - numBionicEyes - numPhotosensEyes;

//            //Pass this as param in signiture...
//            ////Getting glow factors for photosensitive and normal eyes
//            //glowLevelAtPawn = pawn.Map.glowGrid.GameGlowAt(pawn.Position);

//            //Don't do anything if glowlevel is outside the range that NightVision applies to
//            //simply as a computation saving device: the calculations below would work
//            //but the harmony patch for explanation part would append an irrelevant string
//            if (glowLevelAtPawn > 0.3f & glowLevelAtPawn < 0.7f)
//            {
//                return null;
//            }

//            //Get the Photosensitive glowfactor
//            float glowfactorforPhotosens = 0f;
//            if (numPhotosensEyes > 0)
//            {
//                glowfactorforPhotosens = NightVisionMod.Instance.GlowCurveforPhotosensitivity.Evaluate(glowLevelAtPawn);
//            }


//            if (numRemainderEyes < 0)
//            {
//                //Negative nums are inconvenient, needs to be zero for multiplication later
//                Log.Message("Calculated remainderEyes is less than zero, setting to zero. ");
//                numRemainderEyes = 0;
//            }
//            // glowfactorforNormal init. as 1, which is good except when glow < 30%
//            else if (numRemainderEyes > 0 && glowLevelAtPawn < 0.3f)
//            {
//                glowfactorforNormal = NightVisionMod.Instance.VanillaGlowCurve.Evaluate(glowLevelAtPawn);
//            }


//            //Nightvision Factor calculations:
//            //Doesn't penalise for missing eyes; leaving that to Rimworld's sight capacity 
//            nightVisionFactor = numBionicEyes * glowfactorforBionic
//                                    + numPhotosensEyes * glowfactorforPhotosens
//                                    + numRemainderEyes * glowfactorforNormal;
//            nightVisionFactor /= numOfPawnEyesNotMissing;

//            return nightVisionFactor;
//        }

        
//        /// <summary>
//        /// Check apparel to see if any has NightVision comp
//        /// </summary>
//        private static bool HasNightVisionGoggles(Pawn pawn)
//        {
//            List<Apparel> applist = pawn.apparel.WornApparel;
//            if (!applist.NullOrEmpty() 
//                    && applist.Exists(
//                        app => app.TryGetComp<Comp_NightVisionApparel>() != null
//                            && app.TryGetComp<Comp_NightVisionApparel>().GrantsNightVision()))
//            {
//                return true;
//            }
//            return false;
//        }


//        /// <summary>
//        /// Check pawn's apparel to see if any has NightVision comp, if successful sets the ref string to the apparel's name
//        /// </summary>
//        /// <param name="pawn"></param>
//        /// <param name="goggles">string to be changed to nightvision apparel's name, if it exists; otherwise leaves it alone</param>
//        /// <returns>True if pawn has nightvision apparel</returns>
//        private static bool HasNightVisionGoggles(Pawn pawn, ref string goggles)
//        {
//            List<Apparel> applist = pawn.apparel.WornApparel;
//            if (!applist.NullOrEmpty())
//            {
//                var applabel = (from app in applist
//                               where app.TryGetComp<Comp_NightVisionApparel>() != null
//                               && app.GetComp<Comp_NightVisionApparel>().GrantsNightVision()
//                               select app.def.label).First();                               //More than one? Doesn't matter

//                if (applabel != null)
//                {
                    
//                    goggles = applabel;
//                    return true;
//                }
                
//            }
//            return false;
//        }
        

//        private static float GlowLevelAtPawn(Pawn pawn)
//        {
//            return pawn.Map.glowGrid.GameGlowAt(pawn.Position);
//        }
//    }
//}

