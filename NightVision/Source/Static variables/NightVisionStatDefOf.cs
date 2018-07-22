// Nightvision NightVision NightVisionStat.cs
// 
// 01 05 2018
// 
// 21 07 2018

using JetBrains.Annotations;
using RimWorld;

namespace NightVision
    {
        [DefOf]
        [UsedImplicitly]
        public static class NightVisionStatDefOf
            {
                [UsedImplicitly] public static StatDef LightSensitivity = StatDef.Named("LightSensitivity");
                [UsedImplicitly] public static StatDef NightVision      = StatDef.Named("NightVision");
            }
    }