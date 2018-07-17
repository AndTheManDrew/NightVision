using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NightVision.LightModifiers;

namespace NightVision.Utilities
{
        static class Classifier
            {

                public static List<float> ZeroLightTurningPoints;
                public static List<float> FullLightTurningPoint;



                /// <summary>
                /// Three classifications available: none, nightvision, photosensitivity.
                /// For zero light: [none &lt; nightvision &lt; photosensitivity]
                /// For full light: [photosensitivity &lt; none = nightvision]
                /// </summary>
                /// <param name="modifier"></param>
                /// <param name="isZeroLightMod"></param>
                /// <returns></returns>
                public static LightModifiersBase.Options ClassifyModifier(float modifier, bool isZeroLightMod)
                    {
                        if (Math.Abs(modifier) < 0.005 || (isZeroLightMod && modifier < 0.005))
                            {
                                return LightModifiersBase.Options.NVNone;
                            }

                        if (isZeroLightMod)
                            {
                                if (ZeroLightTurningPoints == null)
                                    {
                                        ZeroLightTurningPoints = OffsetsList(true);
                                    }

                                if (ZeroLightTurningPoints.Count == 0 || modifier + 0.001f < ZeroLightTurningPoints[0])
                                    {
                                        return LightModifiersBase.Options.NVNone;
                                    }

                                if (modifier + 0.001f < ZeroLightTurningPoints[1])
                                    {
                                        return LightModifiersBase.Options.NVNightVision;
                                    }
                                return LightModifiersBase.Options.NVPhotosensitivity;

                            }

                        if (FullLightTurningPoint == null)
                            {
                                FullLightTurningPoint = OffsetsList(false);
                            }

                        if (FullLightTurningPoint.Count == 0 || modifier - 0.001f > FullLightTurningPoint[0])
                            {
                                return LightModifiersBase.Options.NVNone;
                            }

                        return LightModifiersBase.Options.NVPhotosensitivity;



                    }
                
                
                // Pretty sure there are better ways of doing this but eh
                private static List<float> OffsetsList(bool forZeroLight)
                    {
                        int  offsetIndex = forZeroLight? 0 : 1;
                        List<float> result = new List<float>
                        {           
                                    (float)Math.Round(LightModifiersBase.NVLightModifiers[offsetIndex], 2, MidpointRounding.AwayFromZero),
                                    (float)Math.Round(LightModifiersBase.PSLightModifiers[offsetIndex], 2, MidpointRounding.AwayFromZero)
                        };
            
            
                        for (int i = result.Count - 1; i >= 0; i--)
                            {
                                if (Math.Abs(result[i]) < 0.001f
                                        || forZeroLight && result[i] + 0.001f < 0
                                        || !forZeroLight && result[i] - 0.001f > 0)
                                    {
                                        result.RemoveAt(i);
                                    }
                            }

                        if (result.Count == 0)
                            {
                                return result;
                            }
                        
                        
                        if (Math.Abs(result[0] - result[1]) < 0.005f)
                            {
                                result[0] = LightModifiersBase.PSLightModifiers[offsetIndex] / 2;
                            }

                        result.Sort();

                        if (forZeroLight)
                            {
                                if (result.Count < 2)
                                    {
                                        result.Insert(0, result[1] / 2);
                                    }
                                result = new List<float>(){(float)Math.Round(result[0] / 2,2), (float)Math.Round((result[0] + result[1])/2,2)};

                            }
                        else
                        {
                            result = new List<float>(){(float)Math.Round(result[0] / 2, 2)};
                        }

                        return result;

                    }
        
            }
    }
