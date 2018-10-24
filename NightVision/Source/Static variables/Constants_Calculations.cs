using System;

namespace NightVision {
    public static class Constants_Calculations {
        public static readonly float[] NVDefaultOffsets = {0.2f, 0f};
        public static readonly float[] PSDefaultOffsets = {0.4f, -0.2f};
        public const float MaxGlowNoGlow = 0.7f;
        public const float MinGlowNoGlow = 0.3f;
        public const int NumberOfDigits = 3;
        public const MidpointRounding Rounding = MidpointRounding.ToEven;
        public const float TrivialGlow = 0.5f;
        public const float TrivialFactor = 1f;
        public const float NVEpsilon = 0.001f;
        public const float DefaultFullLightMultiplier = 1f;
        public const float DefaultMaxCap = 1.2f;
        public const float DefaultMinCap = 0.8f;
        public const float DefaultZeroLightMultiplier = 0.8f;
        public const int ThoughtActiveTicksPast = 240;
    }
}