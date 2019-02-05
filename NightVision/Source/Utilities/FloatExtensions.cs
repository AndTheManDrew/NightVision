using System;
using Verse;

namespace NightVision
{
    /// <summary>
    /// This Mod rounds to two decimal places, equivalent to a whole percentile
    /// </summary>
    public static class FloatExtensions
    {
        public static bool ApproxEq<T>(this T value, T other)
        {
            if (value is float figure && other is float otherFigure)
            {
                return figure.ApproxEq(otherFigure);
            }

            if (value is FloatRange range && other is FloatRange otherRange)
            {
                return range.min.ApproxEq(otherRange.min) && range.max.ApproxEq(otherRange.max);
            }

            return value.Equals(other);
        }


        public static bool GlowIsDarkOrBright(this float glow)
        {
            return glow < Constants_Calculations.MinGlowNoGlow - Constants_Calculations.NVEpsilon || glow > Constants_Calculations.MaxGlowNoGlow + Constants_Calculations.NVEpsilon;
        }

        public static bool GlowIsDarkness(this float glow)
        {
            return glow < Constants_Calculations.MinGlowNoGlow - Constants_Calculations.NVEpsilon;
        }

        public static bool GlowIsBright(this float glow)
        {
            return glow > Constants_Calculations.MaxGlowNoGlow + Constants_Calculations.NVEpsilon;
        }

        public static bool GlowIsTrivial(this float glow)
        {
            return !glow.GlowIsDarkOrBright();
        }

        public static bool FactorIsNonTrivial(this float factor)
        {
            return !factor.FactorIsTrivial();
        }

        public static bool FactorIsTrivial(this float factor)
        {
            return factor.ApproxEq(Constants_Calculations.TrivialFactor);
        }

        public static bool IsTrivial(this float value)
        {
            return Math.Abs(value) < Constants_Calculations.NVEpsilon;
        }

        public static bool ApproxEq(this float value, float other)
        {
            return (value - other).IsTrivial();
        }

        public static bool IsNonTrivial(this float value)
        {
            return !value.IsTrivial();
        }

        public static string ToStringSignedWholePercent(this float value)
        {
            return value.ToString("+0%;-0%;0%");
        }
    }
}
