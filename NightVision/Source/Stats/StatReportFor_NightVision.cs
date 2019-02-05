// Nightvision NightVision StatReportFor_NightVision.cs
// 
// 25 10 2018
// 
// 06 12 2018

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RimWorld;
using Verse;

namespace NightVision
{
    public static class StatReportFor_NightVision
    {
        #region  Members

        public static string CompleteStatReport(StatDef stat, FieldInfo relevantField, Comp_NightVision comp, float relevantGlow)
        {
            float factorFromGlow = comp.FactorFromGlow(glow: relevantGlow);

            return BasicExplanation(glow: relevantGlow, usedApparelSetting: out bool UsedApparel, comp: comp)
                   + FinalValue(stat: stat, value: factorFromGlow)
                   + (relevantField != null && UsedApparel ? ApparelPart(relevantField: relevantField, comp: comp) : "");
        }

        public static string ShortStatReport(float glow, Comp_NightVision comp)
        {
            return BasicExplanation(glow: glow, usedApparelSetting: out _, comp: comp, needsFinalValue: true);
        }


        private static string ApparelPart(FieldInfo relevantField, Comp_NightVision comp)
        {
            var builder = new StringBuilder();
            builder.AppendLine(value: "StatsReport_RelevantGear".Translate());

            foreach (Apparel app in comp.PawnsNVApparel ?? Enumerable.Empty<Apparel>())
            {
                if (Storage.NVApparel.TryGetValue(key: app.def, value: out ApparelVisionSetting setting)
                    && (bool) relevantField.GetValue(obj: setting))
                {
                    builder.AppendLine(value: app.LabelCap);
                }
            }

            return builder.ToString();
        }


        /// <summary>
        ///     For the pawn's stat inspect tab. Cleaned up a bit, still about as elegant as a panda doing the can-can
        /// </summary>
        /// <param name="result"></param>
        /// <param name="glow"></param>
        /// <param name="usedApparelSetting">if apparel had an effect</param>
        /// <param name="comp"></param>
        /// <param name="needsFinalValue">if final value is added externally or we need to add it</param>
        /// <returns></returns>
        private static string BasicExplanation(float glow, out bool usedApparelSetting, Comp_NightVision comp, bool needsFinalValue = false)
        {
            var     nvsum          = 0f;
            var     pssum          = 0f;
            var     sum            = 0f;
            float[] caps           = LightModifiersBase.GetCapsAtGlow(glow: glow);
            var     foundSomething = false;
            float   effect;
            var     basevalue = 0f;
            bool    lowLight  = glow.GlowIsDarkness();
            usedApparelSetting = false;


            var explanation = new StringBuilder();

            StringBuilder nvexplanation = new StringBuilder().AppendFormat(
                format: Str.ExpIntro,
                "",
                Str.MaxAtGlow(glow: glow),
                caps[2],
                Str.NightVision
            ).AppendLine();

            StringBuilder psexplanation = new StringBuilder().AppendFormat(
                format: Str.ExpIntro,
                "",
                Str.MaxAtGlow(glow: glow),
                caps[3],
                Str.Photosens
            ).AppendLine();


            explanation.AppendLine();

            #region Adding Default Values

            if (lowLight)
            {
                basevalue = Constants_Calculations.DefaultFullLightMultiplier
                            + (Constants_Calculations.DefaultZeroLightMultiplier - Constants_Calculations.DefaultFullLightMultiplier)
                            * (0.3f                                              - glow)
                            / 0.3f;

                if (comp.ApparelGrantsNV)
                {
                    foundSomething = true;
                }
            }
            else
            {
                basevalue = Constants_Calculations.DefaultFullLightMultiplier;

                if (comp.ApparelNullsPS)
                {
                    foundSomething = true;
                }
            }

            explanation.AppendFormat(format: "  " + Str.MultiplierLine, arg0: "StatsReport_BaseValue".Translate(), arg1: basevalue).AppendLine()
                        .AppendLine();

            #endregion

            string StringToAppend;

            if (comp.NaturalLightModifiers.HasAnyModifier() && comp.NumberOfRemainingEyes > 0)
            {
                effect = comp.NaturalLightModifiers.GetEffectAtGlow(glow: glow);

                if (effect.IsNonTrivial())
                {
                    foundSomething = true;

                    var NumToAdd = (float) Math.Round(
                        value: effect * comp.NumberOfRemainingEyes,
                        digits: Constants_Calculations.NumberOfDigits,
                        mode: Constants_Calculations.Rounding
                    );

                    StringToAppend = string.Format(
                        format: "    " + Str.ModifierLine,
                        arg0: $"{comp.ParentPawn.def.LabelCap} {comp.RaceSightParts.First().LabelShort} x{comp.NumberOfRemainingEyes}",
                        arg1: effect * comp.NumberOfRemainingEyes
                    );

                    switch (comp.NaturalLightModifiers.Setting)
                    {
                        case VisionType.NVNightVision:
                            nvsum += NumToAdd;
                            nvexplanation.AppendLine(value: StringToAppend);

                            break;
                        case VisionType.NVPhotosensitivity:
                            pssum += NumToAdd;
                            psexplanation.AppendLine(value: StringToAppend);

                            break;
                        case VisionType.NVCustom:
                            sum += NumToAdd;
                            explanation.AppendLine(value: StringToAppend);

                            break;
                    }
                }
            }

            foreach (List<HediffDef> value in comp.PawnsNVHediffs.Values)
            {
                if (value.NullOrEmpty())
                {
                    continue;
                }

                foreach (HediffDef hediffDef in value)
                {
                    if (Storage.HediffLightMods.TryGetValue(key: hediffDef, value: out Hediff_LightModifiers hediffSetting))
                    {
                        effect = hediffSetting.GetEffectAtGlow(glow: glow, numOfEyesNormalisedFor: comp.EyeCount);

                        if (effect.IsNonTrivial())
                        {
                            foundSomething = true;

                            effect = (float) Math.Round(
                                value: effect,
                                digits: Constants_Calculations.NumberOfDigits,
                                mode: Constants_Calculations.Rounding
                            );

                            StringToAppend = string.Format(format: "    " + Str.ModifierLine, arg0: hediffDef.LabelCap, arg1: effect);

                            switch (hediffSetting.IntSetting)
                            {
                                case VisionType.NVNightVision:
                                    nvsum += effect;
                                    nvexplanation.AppendLine(value: StringToAppend);

                                    break;
                                case VisionType.NVPhotosensitivity:
                                    pssum += effect;
                                    psexplanation.AppendLine(value: StringToAppend);

                                    break;
                                case VisionType.NVCustom:
                                    sum += effect;
                                    explanation.AppendLine(value: StringToAppend);

                                    break;
                            }
                        }
                    }
                }
            }

            void AppendPreSumIfNeeded(ref bool needed)
            {
                if (!needed)
                {
                    return;
                }

                explanation.AppendFormat(
                    format: Str.MultiplierLine,
                    arg0: "NVTotal".Translate() + " " + "NVMultiplier".Translate(),
                    arg1: sum                         + basevalue
                );

                explanation.AppendLine();

                needed = false;
            }

            if (foundSomething)
            {
                if (nvsum.IsNonTrivial())
                {
                    explanation.Append(value: nvexplanation);
                    explanation.AppendLine();
                }

                if (pssum.IsNonTrivial())
                {
                    explanation.Append(value: psexplanation);
                    explanation.AppendLine();
                }

                sum += pssum + nvsum;

                explanation.AppendFormat(format: Str.ModifierLine, arg0: "NVTotal".Translate() + " " + "NVModifier".Translate(), arg1: sum);

                explanation.AppendLine();


                var needed = true;

                if (!comp.CanCheat)
                {
                    if (sum - Constants_Calculations.NVEpsilon > caps[0] || sum + Constants_Calculations.NVEpsilon < caps[1])
                    {
                        AppendPreSumIfNeeded(needed: ref needed);

                        explanation.AppendFormat(
                            format: Str.Maxline,
                            arg0: "NVTotal".Translate() + " ",
                            arg1: "max".Translate(),
                            arg2: sum > caps[0] ? caps[0] : caps[1]
                        );

                        explanation.AppendLine();
                    }

                    if (lowLight && comp.ApparelGrantsNV && sum + Constants_Calculations.NVEpsilon < caps[2])
                    {
                        AppendPreSumIfNeeded(needed: ref needed);
                        explanation.Append(value: "NVGearPresent".Translate(arg1: $"{basevalue + caps[2]:0%}"));
                        usedApparelSetting = true;
                        sum                = caps[2];
                    }
                    else if (comp.ApparelNullsPS && sum + Constants_Calculations.NVEpsilon < 0)
                    {
                        AppendPreSumIfNeeded(needed: ref needed);
                        explanation.Append(value: "PSGearPresent".Translate(arg1: $"{Constants_Calculations.DefaultFullLightMultiplier:0%}"));
                        usedApparelSetting = true;
                        sum                = 0;
                    }
                }

                explanation.AppendLine();

                if (needsFinalValue)
                {
                    sum += basevalue;

                    explanation.AppendFormat(
                        format: Str.MultiplierLine,
                        arg0: "NVStatReport_FinalMulti".Translate(),
                        arg1: sum > caps[0] + basevalue ? caps[0] + basevalue :
                        sum       < caps[1] + basevalue ? caps[1] + basevalue : sum
                    );
                }

                return explanation.ToString();
            }

            //Fallback 
            if (needsFinalValue)
            {
                return comp.FactorFromGlow(glow: glow).ToStringPercent();
            }

            return string.Empty;
        }

        private static string FinalValue(StatDef stat, float value)
        {
            return "StatsReport_FinalValue".Translate() + ": " + stat.ValueToString(val: value, numberSense: stat.toStringNumberSense) + "\n\n";
        }

        #endregion
    }
}