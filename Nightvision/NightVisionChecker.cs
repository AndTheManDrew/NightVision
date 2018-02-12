using System;
using System.Reflection;
using RimWorld;
using Verse;
using Harmony;

namespace NightVision
{
    static class NightVisionChecker {
        //Not sure how efficient the following code is. An alternative might be to build a list of bionic eye hediff defs on game load then test against that
        public static int AmountOfNightVision(Pawn pawn)
        {
            //Log.Message("Has_NightVision has been called");

            int num_NV_eye = 0;
            //string tag = "SightSource";
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                //Log.Message(hediff.ToString());
                if (NightVisionMod.Instance.listofNightVisionHediffDefs.Contains(hediff.def))
                {
                    num_NV_eye++;
                }
            }
            //Next bit is just a test: will implement differently, add comps directly to pawns
            if (num_NV_eye < 2
                && pawn.apparel.WornApparel != null
                && pawn.apparel.WornApparel.Exists(app => app.def.HasComp(typeof(Comp_NightVisionApparel))))
            {
                num_NV_eye = 2;
            }
            return num_NV_eye;
        }
    }
}

