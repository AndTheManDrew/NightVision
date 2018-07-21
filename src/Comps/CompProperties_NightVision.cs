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

                /// <summary>
                ///     These floats give the pawn's racial work & move speed multipliers for zero light and full light.
                ///     For humans, these values are 0.8 (x 80%) in zero light and 1.0 ( x100% ) in full light (full light being 100% lit)
                /// </summary>
                public float ZeroLightMultiplier = Constants.DefaultZeroLightMultiplier;

                public CompProperties_NightVision() => compClass = typeof(Comp_NightVision);

                public bool IsDefault() =>
                            Math.Abs(ZeroLightMultiplier - Constants.DefaultZeroLightMultiplier) < 0.1
                            && Math.Abs(FullLightMultiplier - Constants.DefaultFullLightMultiplier) < 0.1
                            && !(NaturalNightVision || NaturalPhotosensitivity);
            }
    }