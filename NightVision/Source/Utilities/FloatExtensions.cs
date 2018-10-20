using System;

namespace NightVision
{
    /// <summary>
    /// This Mod rounds to two decimal places, equivalent to a whole percentile
    /// </summary>
    public static class FloatExtensions
    {
        public static bool GlowIsDarkOrBright(this float glow)
        {
            return glow < CalcConstants.MinGlowNoGlow - CalcConstants.NVEpsilon || glow > CalcConstants.MaxGlowNoGlow + CalcConstants.NVEpsilon;
        }

        public static bool GlowIsDarkness(this float glow)
        {
            return glow < CalcConstants.MinGlowNoGlow - CalcConstants.NVEpsilon;
        }

        public static bool GlowIsBright(this float glow)
        {
            return glow > CalcConstants.MaxGlowNoGlow + CalcConstants.NVEpsilon;
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
            return factor.ApproxEq(CalcConstants.TrivialFactor);
        }

        public static bool ApproxZero(this float value)
        {
            return Math.Abs(value) < CalcConstants.NVEpsilon;
        }

        public static bool ApproxEq(this float value, float other)
        {
            return (value - other).ApproxZero();
        }

        public static bool IsNonTrivial(this float value)
        {
            return !value.ApproxZero();
        }
    }
}
