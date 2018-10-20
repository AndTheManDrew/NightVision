// Nightvision NightVision NightVisionStat.cs
// 
// 01 05 2018
// 
// 21 07 2018

using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace NightVision
{
    [DefOf]
    [UsedImplicitly]
    public static class NVDefOf
    {
        [UsedImplicitly]
        public static StatDef LightSensitivity = StatDef.Named("LightSensitivity");

        [UsedImplicitly]
        public static StatDef NightVision = StatDef.Named("NightVision");

        [UsedImplicitly]
        public static RecipeDef ExtractTapetumLucidum;
        
    }
}