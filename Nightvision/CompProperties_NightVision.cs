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
        /// <summary>
        /// These floats give the pawn's racial work & move speed multipliers for zero light and full light.
        /// For humans, these values are 0.8 (x 80%) in zero light and 1.0 ( x100% ) in full light (full light being 100% lit)
        /// </summary>
        public float zeroLightMultplier = NightVisionSettings.DefaultZeroLightMultiplier;
        public float fullLightMultiplier = NightVisionSettings.DefaultFullLightMultiplier;
        public bool naturalNightVision = false;
        

        public CompProperties_NightVision()
        {
            compClass = typeof(Comp_NightVision);
        }
    }
}
