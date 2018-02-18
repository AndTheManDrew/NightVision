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
        // TODO Check there is a way to add an array from Xml, if not then change this to a list of somesort


        /// <summary>
        /// This array/list/whatever gives the pawn's races glow factors for zero light and full light.
        /// For humans, these values are 80% (0.8) in zero light and 100% (1) in full light (full light being 100% lit)
        /// The indexing is: [0] for zero light, [1] for full light
        /// So humans would have:  naturalGlowFactors: [ 0.8 , 1 ]
        /// </summary>
        public float[] naturalGlowfactors;
        public bool naturalNightVision = false;

        public CompProperties_NightVision()
        {
            compClass = typeof(Comp_NightVision);
        }
    }
}
