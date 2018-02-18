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
    public class HediffComp_NightVision : HediffComp
    {
        public HediffCompProperties_NightVision Props => (HediffCompProperties_NightVision)props;
        public bool grantsNightVision;
        public bool grantsPhotosensitivity;
    }

    public class HediffCompProperties_NightVision : HediffCompProperties
    {
        public bool grantsNightVision = false;
        public bool grantsPhotosensitivity = false;
        public SimpleCurve nightVisionCurve = null;
        public HediffCompProperties_NightVision()
        {
            compClass = typeof(HediffComp_NightVision);
        }
    }
}

