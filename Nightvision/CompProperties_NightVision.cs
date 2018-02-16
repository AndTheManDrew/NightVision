using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace NightVision
{
    public class CompProperties_NightVision : CompProperties
    {
        public SimpleCurve naturalGlowCurve = null;
        public bool naturalNightVision = false;

        public CompProperties_NightVision()
        {
            compClass = typeof(Comp_NightVision);
        }
    }
}
