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
        public float FullLightMultiplier     = CalcConstants.DefaultFullLightMultiplier;
        public bool  NaturalNightVision      = false;
        public bool  NaturalPhotosensitivity = false;
        public bool  ShouldShowInSettings    = true;

        public float ZeroLightMultiplier = CalcConstants.DefaultZeroLightMultiplier;

        public CompProperties_NightVision() => compClass = typeof(Comp_NightVision);

        public bool IsDefault()
            => Math.Abs(ZeroLightMultiplier    - CalcConstants.DefaultZeroLightMultiplier) < 0.1
               && Math.Abs(FullLightMultiplier - CalcConstants.DefaultFullLightMultiplier) < 0.1
               && !(NaturalNightVision || NaturalPhotosensitivity);
    }
}