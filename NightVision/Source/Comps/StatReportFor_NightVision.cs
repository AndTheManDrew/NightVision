using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RimWorld;
using Verse;

namespace NightVision {
    public  static class StatReportFor_NightVision {


        public static string CompleteStatReport(StatWorker_NightVisionBase worker, Comp_NightVision comp)
        {
            return BasicExplanation(worker.Glow, out bool UsedApparel, comp) + (UsedApparel? ApparelPart(worker, comp) + CombatPart() : CombatPart());
        }
        
        public static string ShortStatReport(float glow, Comp_NightVision comp)
        {
            return BasicExplanation(glow, out _, comp, true);
        }

        private static string ApparelPart(StatWorker_NightVisionBase worker, Comp_NightVision comp)
        {
            var builder = new StringBuilder();
                builder.AppendLine("StatsReport_RelevantGear".Translate());

                foreach (Apparel app in comp.PawnsNVApparel ?? Enumerable.Empty<Apparel>())
                {
                    if (Storage.NVApparel.TryGetValue(
                            app.def,
                            out ApparelVisionSetting setting
                        )
                        && (bool)worker.RelevantField.GetValue(setting))
                    {
                        builder.AppendLine(app.LabelCap);
                    }
                }

            return builder.ToString();
        }

        private static string CombatPart()
        {
            return string.Empty;
        }
        
        /// <summary>
        ///     For the pawn's stat inspect tab. Cleaned up a bit, still about as elegant as a panda doing the can-can
        /// </summary>
        /// <param name="result"></param>
        /// <param name="glow"></param>
        /// <param name="usedApparelSetting">if apparel had an effect</param>
        /// <param name="comp"></param>
        /// <param name="needsFinalValue">if RW will finalise value or we need to add it</param>
        /// <returns></returns>
        private static string BasicExplanation(
            float            glow,
            out bool         usedApparelSetting,
            Comp_NightVision comp,
            bool             needsFinalValue = false
        )
        {
            var     nvsum          = 0f;
            var     pssum          = 0f;
            var     sum            = 0f;
            float[] caps           = LightModifiersBase.GetCapsAtGlow(glow);
            var     foundSomething = false;
            float   effect;
            var     basevalue = 0f;
            bool    lowLight  = glow < 0.3f;
            usedApparelSetting = false;


            var explanation = new StringBuilder();

            StringBuilder nvexplanation = new StringBuilder().AppendFormat(Constants.ExpIntro, "", Str.MaxAtGlow(glow), caps[2], Str.NightVision);

            StringBuilder psexplanation = new StringBuilder().AppendFormat(Constants.ExpIntro, "", Str.MaxAtGlow(glow), caps[3], Str.Photosens);


            explanation.AppendLine();

            #region Adding Default Values

            if (lowLight)
            {
                basevalue = Constants.DefaultFullLightMultiplier
                            + (Constants.DefaultZeroLightMultiplier - Constants.DefaultFullLightMultiplier)
                            * (0.3f                                 - glow)
                            / 0.3f;
                if (comp.ApparelGrantsNV)
                {
                    foundSomething = true;
                }
            }
            else
            {
                basevalue = Constants.DefaultFullLightMultiplier;
                if (comp.ApparelNullsPS)
                {
                    foundSomething = true;
                }
            }
                    
            explanation.AppendFormat(
                Constants.MultiplierLine,
                "StatsReport_BaseValue".Translate(),
                basevalue
            ).AppendLine();

            #endregion

            string StringToAppend;

            if (comp.NaturalLightModifiers.HasAnyModifier() && comp.NumberOfRemainingEyes > 0)
            {
                effect = comp.NaturalLightModifiers.GetEffectAtGlow(glow);

                if (Math.Abs(effect) >= 0.005)
                {
                    foundSomething = true;
                    var NumToAdd = (float) Math.Round(effect * comp.NumberOfRemainingEyes, Constants.NumberOfDigits, Constants.Rounding);
                    StringToAppend = string.Format("  " + Constants.ModifierLine, $"{comp.ParentPawn.def.LabelCap} {comp.RaceSightParts.First().LabelShort} x{comp.NumberOfRemainingEyes}", effect * comp.NumberOfRemainingEyes);

                    switch (comp.NaturalLightModifiers.Setting)
                    {
                        case VisionType.NVNightVision:
                            nvsum += NumToAdd;
                            nvexplanation.AppendLine(StringToAppend);

                            break;
                        case VisionType.NVPhotosensitivity:
                            pssum += NumToAdd;
                            psexplanation.AppendLine(StringToAppend);

                            break;
                        case VisionType.NVCustom:
                            sum += NumToAdd;
                            explanation.AppendLine(StringToAppend);

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
                    if (Storage.HediffLightMods.TryGetValue(
                        hediffDef,
                        out Hediff_LightModifiers hediffSetting
                    ))
                    {
                        effect = hediffSetting.GetEffectAtGlow(glow, comp.EyeCount);

                        if (Math.Abs(effect) > 0.005)
                        {
                            foundSomething = true;
                            effect         = (float) Math.Round(effect, Constants.NumberOfDigits, Constants.Rounding);
                            StringToAppend = string.Format("  " + Constants.ModifierLine, hediffDef.LabelCap, effect);

                            switch (hediffSetting.IntSetting)
                            {
                                case VisionType.NVNightVision:
                                    nvsum += effect;
                                    nvexplanation.AppendLine(StringToAppend);

                                    break;
                                case VisionType.NVPhotosensitivity:
                                    pssum += effect;
                                    psexplanation.AppendLine(StringToAppend);

                                    break;
                                case VisionType.NVCustom:
                                    sum += effect;
                                    explanation.AppendLine(StringToAppend);

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
                    Constants.MultiplierLine,
                    "NVTotal".Translate()
                    + " "
                    + "NVMultiplier".Translate(),
                    sum + basevalue
                );

                explanation.AppendLine();

                needed = false;
            }

            if (foundSomething)
            {
                if (Math.Abs(nvsum) > 0.005f)
                {
                    explanation.Append(nvexplanation);
                    explanation.AppendLine();
                }

                if (Math.Abs(pssum) > 0.005f)
                {
                    explanation.Append(psexplanation);
                    explanation.AppendLine();
                }

                sum += pssum + nvsum;

                explanation.AppendFormat(
                    Constants.ModifierLine,
                    "NVTotal".Translate() + " " + "NVModifier".Translate(),
                    sum
                );

                explanation.AppendLine();

                explanation.AppendLine();

                var needed = true;

                if (!comp.CanCheat)
                {
                    if (sum - 0.001f > caps[0] || sum + 0.001f < caps[1])
                    {
                        AppendPreSumIfNeeded(ref needed);

                        explanation.AppendFormat(
                            Constants.Maxline,
                            "NVTotal".Translate() + " ",
                            "max".Translate(),
                            sum > caps[0] ? caps[0] : caps[1]
                        );

                        explanation.AppendLine();
                    }

                    if (lowLight && comp.ApparelGrantsNV && sum + 0.001f < caps[2])
                    {
                        AppendPreSumIfNeeded(ref needed);
                        explanation.Append("NVGearPresent".Translate($"{basevalue + caps[2]:0%}"));
                        usedApparelSetting = true;
                        sum                = caps[2];
                    }
                    else if (comp.ApparelNullsPS && sum + 0.001f < 0)
                    {
                        AppendPreSumIfNeeded(ref needed);
                        explanation.Append("PSGearPresent".Translate($"{Constants.DefaultFullLightMultiplier:0%}"));
                        usedApparelSetting = true;
                        sum                = 0;
                    }
                }

                explanation.AppendLine();

                if (needsFinalValue)
                {
                    sum += basevalue;

                    explanation.AppendFormat(
                        Constants.MultiplierLine,
                        "NVStatReport_FinalMulti".Translate(),
                        sum > caps[0] + basevalue ? caps[0] + basevalue :
                        sum < caps[1] + basevalue ? caps[1] + basevalue : sum
                    );
                }

                return explanation.ToString();
            }

            //Fallback 
            if (needsFinalValue)
            {
                return comp.FactorFromGlow(glow).ToStringPercent();
            }

            return string.Empty;
        }
    }
}