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
                public float FullLightMultiplier     = Constants.DefaultFullLightMultiplier;
                public bool  NaturalNightVision      = false;
                public bool  NaturalPhotosensitivity = false;
                public bool  ShouldShowInSettings    = true;

                public float ZeroLightMultiplier = Constants.DefaultZeroLightMultiplier;

                public CompProperties_NightVision() => compClass = typeof(Comp_NightVision);

                public bool IsDefault() =>
                            Math.Abs(ZeroLightMultiplier - Constants.DefaultZeroLightMultiplier) < 0.1
                            && Math.Abs(FullLightMultiplier - Constants.DefaultFullLightMultiplier) < 0.1
                            && !(NaturalNightVision || NaturalPhotosensitivity);
            }
    }