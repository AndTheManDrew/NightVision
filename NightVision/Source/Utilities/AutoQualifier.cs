// Nightvision NightVision AutoQualifier.cs
// 
// 26 06 2018
// 
// 21 07 2018

using Verse;

namespace NightVision
{
    internal static class AutoQualifier
    {
        internal static VisionType? HediffCheck(
                        HediffDef hediffDef
                    )
        {
            if (hediffDef.addedPartProps is AddedBodyPartProps abpp && abpp.partEfficiency > 1.0f)
            {
                return VisionType.NVNightVision;
            }

            return null;
        }
    }
}