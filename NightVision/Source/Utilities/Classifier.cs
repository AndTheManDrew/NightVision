// Nightvision NightVision Classifier.cs
// 
// 03 07 2018
// 
// 21 07 2018

using System;
using System.Collections.Generic;
#if DEBUG
using Verse;
#endif
namespace NightVision
{
    internal static class Classifier
    {
        public static List<float> FullLightTurningPoint;
        public static List<float> ZeroLightTurningPoints;


        /// <summary>
        ///     Three classifications available: none, nightvision, photosensitivity.
        ///     For zero light: [none &lt; nightvision &lt; photosensitivity]
        ///     For full light: [photosensitivity &lt; none = nightvision]
        /// </summary>
        /// <param name="modifier"></param>
        /// <param name="isZeroLightMod"></param>
        /// <returns></returns>
        public static VisionType ClassifyModifier(
                        float modifier,
                        bool  isZeroLightMod
                    )
        {
            //If the effect is negligible or is <= 0 for Zero mod
            //The latter because we don't have a thought that corresponds to worse night vision then default
            if (Math.Abs(modifier) < 0.005 || isZeroLightMod && modifier < 0.005)
            {
                return VisionType.NVNone;
            }

            if (isZeroLightMod)
            {
                if (Classifier.ZeroLightTurningPoints == null)
                {
                    Classifier.ZeroLightTurningPoints = Classifier.OffsetsList(true);
                }

                if (Classifier.ZeroLightTurningPoints.Count == 0
                    || modifier + 0.001f                    < Classifier.ZeroLightTurningPoints[0])
                {
                    return VisionType.NVNone;
                }

                if (modifier + 0.001f < Classifier.ZeroLightTurningPoints[1])
                {
                    return VisionType.NVNightVision;
                }

                return VisionType.NVPhotosensitivity;
            }

            if (Classifier.FullLightTurningPoint == null)
            {
                Classifier.FullLightTurningPoint = Classifier.OffsetsList(false);
            }

            if (Classifier.FullLightTurningPoint.Count == 0 || modifier - 0.001f > Classifier.FullLightTurningPoint[0])
            {
                return VisionType.NVNone;
            }

            return VisionType.NVPhotosensitivity;
        }


        // Pretty sure there are better ways of doing this but eh
        private static List<float> OffsetsList(
                        bool forZeroLight
                    )
        {
            //for accessing the offsets [zerolightmod, fulllightmod]
            int offsetIndex = forZeroLight ? 0 : 1;

            var result = new List<float>
                         {
                             (float) Math.Round(
                                                LightModifiersBase.NVLightModifiers[offsetIndex],
                                                2,
                                                Constants.Rounding
                                               ),
                             (float) Math.Round(
                                                LightModifiersBase.PSLightModifiers[offsetIndex],
                                                2,
                                                Constants.Rounding
                                               )
                         };
#if DEBUG
                        Log.Message($"NightVision.Classifier.OffsetsList: {result.ToStringSafeEnumerable()}");
#endif


            for (int i = result.Count - 1; i >= 0; i--)
            {
                if (Math.Abs(result[i]) < 0.001f
                    || forZeroLight  && result[i] + 0.001f < 0
                    || !forZeroLight && result[i] - 0.001f > 0)
                {
                    result.RemoveAt(i);
                }
            }

            if (result.Count == 0)
            {
                return result;
            }


            if (result.Count >= 2 && Math.Abs(result[0] - result[1]) < 0.005f)
            {
                result[0] = LightModifiersBase.PSLightModifiers[offsetIndex] / 2;
            }
#if DEBUG
                        Log.Message($"NightVision.Classifier.OffsetsList: 112 {result.ToStringSafeEnumerable()}");
#endif

            result.Sort();

            if (forZeroLight)
            {
                if (result.Count < 2)
                {
                    result.Insert(0, result[1] / 2);
                }

                result = new List<float>
                         {
                             (float) Math.Round(result[0]               / 2, 2),
                             (float) Math.Round((result[0] + result[1]) / 2, 2)
                         };
            }
            else
            {
                result = new List<float> {(float) Math.Round(result[0] / 2, 2)};
            }

            return result;
        }
    }
}