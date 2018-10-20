// Nightvision NightVision StatReportFor_NightVision.cs
// 
// 20 10 2018
// 
// 20 10 2018

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace NightVision
{
    public static class StatReportFor_NightVision
    {
        public static string CompleteStatReport(NVStatWorker worker, Comp_NightVision comp, Pawn pawn)
        {
            float   glow           = worker.Glow;
            StatDef stat           = worker.Stat;
            float   factorFromGlow = comp.FactorFromGlow(glow: worker.Glow);

            return BasicExplanation(glow: glow, usedApparelSetting: out bool UsedApparel, comp: comp)
                   + FinalValue(stat: stat, value: factorFromGlow)
                   + (UsedApparel
                               ? ApparelPart(worker: worker, comp: comp)
                                 + CombatPart(pawn: pawn, factorFromGlow: factorFromGlow, glow: glow, stat: stat)
                               : CombatPart(pawn: pawn, factorFromGlow: factorFromGlow, glow: glow, stat: stat));
        }

        public static string ShortStatReport(float glow, Comp_NightVision comp)
        {
            return BasicExplanation(glow: glow, usedApparelSetting: out _, comp: comp, needsFinalValue: true);
        }

        private static string FinalValue(StatDef stat, float value)
        {
            return "StatsReport_FinalValue".Translate() + ": " + stat.ValueToString(val: value, numberSense: stat.toStringNumberSense) + "\n\n";
        }

        private static string ApparelPart(NVStatWorker worker, Comp_NightVision comp)
        {
            var builder = new StringBuilder();
            builder.AppendLine(value: "StatsReport_RelevantGear".Translate());

            foreach (Apparel app in comp.PawnsNVApparel ?? Enumerable.Empty<Apparel>())
            {
                if (Storage.NVApparel.TryGetValue(key: app.def, value: out ApparelVisionSetting setting)
                    && (bool) worker.RelevantField.GetValue(obj: setting))
                {
                    builder.AppendLine(value: app.LabelCap);
                }
            }

            return builder.ToString();
        }

        public static string RangedCoolDown(Pawn pawn, int skillLevel)
        {
            var   stringBuilder = new StringBuilder();
            float glow          = GlowFor.GlowAt(thing: pawn);
            float glowFactor    = GlowFor.FactorOrFallBack(pawn: pawn, glow: glow);

            stringBuilder.AppendLine(
                value: Str.RangedCooldown(
                    glow: glow,
                    skill: skillLevel,
                    result: CombatHelpers.RangedCooldownMultiplier(skill: skillLevel, glowFactor: glowFactor)
                )
            );

            glow       = 1f;
            glowFactor = GlowFor.FactorOrFallBack(pawn: pawn, glow: glow);

            stringBuilder.AppendLine(
                value: Str.RangedCooldownDemo(glow: glow, result: CombatHelpers.RangedCooldownMultiplier(skill: skillLevel, glowFactor: glowFactor))
            );

            glow       = 0f;
            glowFactor = GlowFor.FactorOrFallBack(pawn: pawn, glow: glow);

            stringBuilder.AppendLine(
                value: Str.RangedCooldownDemo(glow: glow, result: CombatHelpers.RangedCooldownMultiplier(skill: skillLevel, glowFactor: glowFactor))
            );

            return stringBuilder.ToString();
        }


        private static string CombatPart(Pawn pawn, float factorFromGlow, float glow, StatDef stat)
        {
            var     stringbuilder = new StringBuilder();

            stringbuilder.AppendLine(new string('-', 20));
            stringbuilder.AppendLine(Str.Combat(glow.GlowIsDarkness()));
            stringbuilder.AppendLine();
            stringbuilder.AppendLine(value: Str.ShootTargetAtGlow(glow: glow));


            for (var i = 1; i <= 4; i++)
            {
                float hit = ShotReport.HitFactorFromShooter(caster: pawn, distance: i * 5);

                stringbuilder.AppendLine(
                    value: Str.HitChanceTransform(
                        distance: i * 5,
                        hitChance: hit,
                        result: CombatHelpers.HitChanceGlowTransform(hitChance: hit, attGlowFactor: factorFromGlow)
                    )
                );
            }

            stringbuilder.AppendLine();
            stringbuilder.AppendLine(value: Str.StrikeTargetAtGlow(glow: glow));
            float meleeHit = pawn.GetStatValue(stat: RwDefs.MeleeHitStat, applyPostProcess: true);

            stringbuilder.AppendLine(
                value: Str.StrikeChanceTransform(
                    hitChance: meleeHit,
                    result: CombatHelpers.HitChanceGlowTransform(hitChance: meleeHit, attGlowFactor: factorFromGlow)
                )
            );

            stringbuilder.AppendLine();
            stringbuilder.AppendLine(value: Str.SurpriseAtkDesc());
            stringbuilder.AppendLine(value: Str.SurpriseAtkChance(glow,  stat));
            float sAtk = CombatHelpers.SurpriseAttackChance(atkGlowFactor: factorFromGlow, defGlowFactor: Storage.MultiplierCaps.min);

            stringbuilder.AppendLine(
                value: Str.SurpriseAtkDemo(
                    stat: stat,
                    atkStatVal: factorFromGlow,
                    defStatValue: Storage.MultiplierCaps.min,
                    glow: glow,
                    sAtkChance: sAtk
                )
            );

            if (sAtk.IsNonTrivial())
            {
                sAtk = CombatHelpers.SurpriseAttackChance(atkGlowFactor: factorFromGlow, defGlowFactor: 1f);
                stringbuilder.AppendLine(value: Str.SurpriseAtkDemoExt(stat: stat, atkStatVal: factorFromGlow, defStatVal: 1f, sAtk: sAtk));

                if (sAtk.IsNonTrivial())
                {
                    sAtk = CombatHelpers.SurpriseAttackChance(atkGlowFactor: factorFromGlow, defGlowFactor: Storage.MultiplierCaps.max);

                    stringbuilder.AppendLine(
                        value: Str.SurpriseAtkDemoExt(stat: stat, atkStatVal: factorFromGlow, defStatVal: Storage.MultiplierCaps.max, sAtk: sAtk)
                    );
                }
            }


            stringbuilder.AppendLine();
            stringbuilder.AppendLine(value: Str.Dodge(stat: stat));
            float pawnDodgeVal = pawn.GetStatValue(stat: RwDefs.MeleeDodgeStat);

            stringbuilder.AppendLine(
                value: Str.DodgeDemo(
                    stat: stat,
                    defStatVal: factorFromGlow,
                    atkStatVal: 1f,
                    dodgeStat: RwDefs.MeleeDodgeStat,
                    dodgeChance: pawnDodgeVal,
                    result: CombatHelpers.DodgeChanceFunction(orgDodge: pawnDodgeVal, glowFactorDelta: 1f - factorFromGlow)
                )
            );

            return stringbuilder.ToString();
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
            );

            StringBuilder psexplanation = new StringBuilder().AppendFormat(
                format: Str.ExpIntro,
                "",
                Str.MaxAtGlow(glow: glow),
                caps[3],
                Str.Photosens
            );


            explanation.AppendLine();

            #region Adding Default Values

            if (lowLight)
            {
                basevalue = CalcConstants.DefaultFullLightMultiplier
                            + (CalcConstants.DefaultZeroLightMultiplier - CalcConstants.DefaultFullLightMultiplier) * (0.3f - glow) / 0.3f;

                if (comp.ApparelGrantsNV)
                {
                    foundSomething = true;
                }
            }
            else
            {
                basevalue = CalcConstants.DefaultFullLightMultiplier;

                if (comp.ApparelNullsPS)
                {
                    foundSomething = true;
                }
            }

            explanation.AppendFormat(format: Str.MultiplierLine, arg0: "StatsReport_BaseValue".Translate(), arg1: basevalue).AppendLine();

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
                        digits: CalcConstants.NumberOfDigits,
                        mode: CalcConstants.Rounding
                    );

                    StringToAppend = string.Format(
                        format: "  " + Str.ModifierLine,
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
                            effect         = (float) Math.Round(value: effect, digits: CalcConstants.NumberOfDigits, mode: CalcConstants.Rounding);
                            StringToAppend = string.Format(format: "  " + Str.ModifierLine, arg0: hediffDef.LabelCap, arg1: effect);

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

                explanation.AppendLine();

                var needed = true;

                if (!comp.CanCheat)
                {
                    if (sum - CalcConstants.NVEpsilon > caps[0] || sum + CalcConstants.NVEpsilon < caps[1])
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

                    if (lowLight && comp.ApparelGrantsNV && sum + CalcConstants.NVEpsilon < caps[2])
                    {
                        AppendPreSumIfNeeded(needed: ref needed);
                        explanation.Append(value: "NVGearPresent".Translate(arg1: $"{basevalue + caps[2]:0%}"));
                        usedApparelSetting = true;
                        sum                = caps[2];
                    }
                    else if (comp.ApparelNullsPS && sum + CalcConstants.NVEpsilon < 0)
                    {
                        AppendPreSumIfNeeded(needed: ref needed);
                        explanation.Append(value: "PSGearPresent".Translate(arg1: $"{CalcConstants.DefaultFullLightMultiplier:0%}"));
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
    }
}