using System;
using System.Reflection;
using RimWorld;
using Verse;
using Harmony;

namespace Nightvision
{
    static class NightVisionChecker {
        //Not sure how efficient the following code is. An alternative might be to build a list of bionic eye hediff defs on game load then test against that
        public static int AmountOfNightVision(Pawn pawn)
        {
            //Log.Message("Has_NightVision has been called");

            int num_NV_eye = 0;
            string tag = "SightSource";
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                //Log.Message(hediff.ToString());
                if (hediff is Hediff_AddedPart && hediff.def.addedPartProps.isBionic && hediff.Part.def.tags.Contains(tag))
                {
                    num_NV_eye++;
                }
            }
            return num_NV_eye;
        }
    }
}

