using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace NightVision
{
    /// <summary>
    /// More comps and props and comps and props and comps and props
    /// </summary>
    public class Comp_NightVisionHediff : HediffComp
    {
        public CompProperties_NightVisionHediff Props => (CompProperties_NightVisionHediff)props;

    }

    public class CompProperties_NightVisionHediff : HediffCompProperties
    {
        public bool grantsNightVision = false;
        public bool grantsPhotosensitivity = false;
        public SimpleCurve nightVisionCurve = null;
        public CompProperties_NightVisionHediff()
        {
            compClass = typeof(Comp_NightVisionHediff);
        }
    }
}

