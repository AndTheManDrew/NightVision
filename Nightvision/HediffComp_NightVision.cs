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
    }

    public class HediffCompProperties_NightVision : HediffCompProperties
    {
        public bool grantsNightVision = false;
        public bool grantsPhotosensitivity = false;
        public float zeroLightMod = default(float);
        public float fullLightMod = default(float);
        public HediffCompProperties_NightVision()
        {
            compClass = typeof(HediffComp_NightVision);
        }
    }
}

