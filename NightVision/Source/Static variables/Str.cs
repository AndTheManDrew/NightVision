using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace NightVision
{
    public static class Str
    {
        public const string PhotoSTag = "Photosensitivity";

        public static readonly string Darkness = "NightVisionDarkness".Translate();
        public static readonly string Bright = "NightVisionBright".Translate();

        public static readonly string Effect = "NVEffects".Translate();

        public static readonly string NightVision = VisionType.NVNightVision.ToString().Translate();

        public static readonly string Photosens = VisionType.NVPhotosensitivity.ToString().Translate();

        public static string MaxAtGlow(float glow)
        {
            return "NVMaxAtGlow".Translate(glow.ToStringPercent());
        }

        public static string ShootTargetAtGlow(float glow) => "NightVisionShootTargetAtGlow".Translate(glow.ToStringPercent());

        public static string HitChanceTransform(float distance, float hitChance, float result) => "NightVisionHitChanceTransform".Translate(
            $"{distance.ToString() + ',',-5}{hitChance.ToStringPercent(), 5}", $"{result.ToStringPercent(),5}");

        public static string StrikeChanceTransform(float hitChance, float result) => "NightVisionStrikeChanceTransform".Translate($"{hitChance.ToStringPercent(), 20}", result.ToStringPercent());

        public static string StrikeTargetAtGlow(float glow) => "NightVisionStrikeTargetAtGlow".Translate(glow.ToStringPercent());

        public static string SurpriseAtkChance(float glow, StatDef stat) => "NightVisionSurpriseAtkChance".Translate(glow.ToStringPercent(), stat.label);

        public static string SurpriseAtkDesc() => "NightVisionSurpriseAtkDesc".Translate();

        public static string Combat(bool isDark) => "NightVisionCombat".Translate(isDark? Darkness : Bright);

        public static string SurpriseAtkDemo(StatDef stat, float atkStatVal, float defStatValue, float glow, float sAtkChance)
            => "NightVisionSurpriseAtkDemo"
                        .Translate(
                            atkStatVal.ToStringPercent(),
                            stat.label,
                            glow.ToStringPercent(),
                            defStatValue.ToStringPercent(),
                            sAtkChance.ToStringPercent()
                        );


        public static string SurpriseAtkDemoExt(StatDef stat, float atkStatVal, float defStatVal, float sAtk) => "NightVisionSurpriseAtkDemoExt"
                    .Translate(
                        defStatVal.ToStringPercent(),
                        stat.label,
                        sAtk.ToStringPercent());

        public static string Dodge(StatDef stat) => "NightVisionDodge".Translate(stat.label);

        public static string DodgeDemo(StatDef stat, float defStatVal, float atkStatVal, StatDef dodgeStat, float dodgeChance, float result) => "NightVisionDodgeDemo"
                    .Translate(
                        dodgeChance.ToStringPercent(),
                        defStatVal.ToStringPercent(),
                        stat.LabelCap,
                        atkStatVal.ToStringPercent(),
                        result.ToStringPercent()


                        );

        public static string AimFactorFromLight(float glowAtTarget, float result) => "NightVisionAimFactorFromLight".Translate(glowAtTarget.ToStringPercent(),  result.ToStringPercent()).CapitalizeFirst();

        public const string Alabel = "{0:+#;-#;0}%";
        public const string BodyKey = "WholeBody";
        public const string Maxline = " ({0}{1}: {2,6: +#0.0%;-#0.0%;+0.0%})";
        public const string ModifierLine = "{0}: {1,6:+#0.0%;-#0.0%;0.0%}";
        public const string MultiplierLine = "{0}: {1,6:x#0.0%;x#0.0%;x0.0%}";
        public const string XLabel = "x{0: #0}%";

        public static readonly string FullLabel =
                    "NVFullLabel".Translate() + " = {0:+#;-#;0}%";

        public static readonly string FullMultiLabel = "NVFullLabel".Translate() + " = x{0:##}%";

        public static readonly string ZeroLabel =
                    "NVZeroLabel".Translate() + " = {0:+#;-#;0}%";

        public static readonly string ZeroMultiLabel = "NVZeroLabel".Translate() + " = x{0:##}%";
        public static readonly string ExpIntro = "{3} " + Str.Effect + Maxline;

        public static string RangedCooldown(float glow, int skill, float result) => "NightVisionRangedCooldown".Translate(glow.ToStringPercent(), skill, result.ToStringPercent());

        public static string RangedCooldownDemo (float glow, float result)=> "NightVisionRangedCooldownDemo".Translate(glow.ToStringPercent(), result.ToStringPercent());
    }
}
