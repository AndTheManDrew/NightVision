// Nightvision NightVision Constants.cs
// 
// 22 07 2018
// 
// 02 08 2018

using System;
using RimWorld;
using Verse;

namespace NightVision
{
    public static class Constants
    {
        public const           string           Alabel                     = "{0:+#;-#;0}%";
        public const           string           BodyKey                    = "WholeBody";
        public const           float            DefaultFullLightMultiplier = 1f;
        public const           float            DefaultMaxCap              = 1.2f;
        public const           float            DefaultMinCap              = 0.8f;
        public const           float            DefaultZeroLightMultiplier = 0.8f;
        public const           float            GenRowHeight               = 30f;
        public const           float            IndicatorSize              = 12f;
        public const           float            MaxGlowNoGlow              = 0.7f;
        public const           string           Maxline                    = " ({0}{1}: {2,6: +#0.0%;-#0.0%;+0.0%})";
        public const           float            MinGlowNoGlow              = 0.3f;
        public const           string           ModifierLine               = "{0}: {1,6:+#0.0%;-#0.0%;0.0%}";
        public const           string           MultiplierLine             = "{0}: {1,6:x#0.0%;x#0.0%;x0.0%}";
        public const           int              NumberOfDigits             = 3;
        public const           MidpointRounding Rounding                   = MidpointRounding.ToEven;
        public const           float            RowGap                     = 10f;
        public const           int              ThoughtActiveTicksPast     = 240;
        public const           string           XLabel                     = "x{0: #0}%";
        public const float ShootSkillCooldownLimit = 14;
        public static readonly BodyPartTagDef   EyeTag                     = BodyPartTagDefOf.SightSource;

        public static readonly string FullLabel =
                    "NVFullLabel".Translate() + " = {0:+#;-#;0}%";

        public static readonly string  FullMultiLabel   = "NVFullLabel".Translate() + " = x{0:##}%";
        public static readonly float[] NVDefaultOffsets = {0.2f, 0f};
        public static readonly float[] PSDefaultOffsets = {0.4f, -0.2f};

        public static readonly string ZeroLabel =
                    "NVZeroLabel".Translate() + " = {0:+#;-#;0}%";

        public static readonly  string ZeroMultiLabel = "NVZeroLabel".Translate() + " = x{0:##}%";
        public /*const*/ static float  RowHeight      = 45f;


        public static readonly string ExpIntro = "{3} " + Str.Effect + Maxline;

    }
}