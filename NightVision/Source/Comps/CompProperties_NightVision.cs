// Nightvision NightVision CompProperties_NightVision.cs
// 
// 16 05 2018
// 
// 21 07 2018

using System;
using Verse;

namespace NightVision
{
    public class CompProperties_NightVision : CompProperties
    {
        public bool  CanCheat                = false;
        public float FullLightMultiplier     = Constants_Calculations.DefaultFullLightMultiplier;
        public bool  NaturalNightVision      = false;
        public bool  NaturalPhotosensitivity = false;
        public bool  ShouldShowInSettings    = true;

        public float ZeroLightMultiplier = Constants_Calculations.DefaultZeroLightMultiplier;

        public CompProperties_NightVision() => compClass = typeof(Comp_NightVision);

        public bool IsDefault()
            => Math.Abs(ZeroLightMultiplier    - Constants_Calculations.DefaultZeroLightMultiplier) < 0.1
               && Math.Abs(FullLightMultiplier - Constants_Calculations.DefaultFullLightMultiplier) < 0.1
               && !(NaturalNightVision || NaturalPhotosensitivity);
    }
}