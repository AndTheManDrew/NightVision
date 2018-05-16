using System;
using Verse;

namespace NightVision.Comps
{
    public class CompProperties_NightVision : CompProperties
    {
        /// <summary>
        /// These floats give the pawn's racial work & move speed multipliers for zero light and full light.
        /// For humans, these values are 0.8 (x 80%) in zero light and 1.0 ( x100% ) in full light (full light being 100% lit)
        /// </summary>
        public float ZeroLightMultiplier = NightVisionSettings.DefaultZeroLightMultiplier;
        public float FullLightMultiplier = NightVisionSettings.DefaultFullLightMultiplier;
        public bool NaturalNightVision = false;
        public bool NaturalPhotosensitivity = false;

        public bool IsDefault()
            {
                return Math.Abs(ZeroLightMultiplier - NightVisionSettings.DefaultZeroLightMultiplier) < 0.1 
                       && Math.Abs(FullLightMultiplier - NightVisionSettings.DefaultFullLightMultiplier) < 0.1
                       && !(NaturalNightVision || NaturalPhotosensitivity);
            }

        public CompProperties_NightVision()
        {
            compClass = typeof(Comp_NightVision);
        }
    }
}
