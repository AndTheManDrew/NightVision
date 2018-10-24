// Nightvision NightVision CombatHelpers.cs
// 
// 26 09 2018
// 
// 20 10 2018

using System;
using UnityEngine;
using Verse;

namespace NightVision
{
    [NVHasSettingsDependentField]
    public static class CombatHelpers
    {
        [NVSettingsDependentField]
        public static float _attXCoeff = Storage_Combat.HitCurviness.Value / Storage.MultiplierCaps.Span;

        public static float DodgeXCoeff
        {
            get
            {
                if (_dodgeXCoeff < 0)
                {
                    _dodgeXCoeff = Storage_Combat.DodgeCurviness.Value / Storage.MultiplierCaps.Span;
                }
                return _dodgeXCoeff;
            }
            set
            {
                _dodgeXCoeff = value;
            }
        }

        public static float AttXCoeff
        {
            get
            {
                if (_attXCoeff < -1)
                {
                    _attXCoeff = Storage_Combat.HitCurviness.Value / Storage.MultiplierCaps.Span;;
                }

                return _attXCoeff;
            }
            set
            {
                _attXCoeff = value;
            }
        }

        public static float ChanceOfSurpriseAttFactor
        {
            get
            {
                if (_chanceOfSurpriseAttFactor < -1)
                {
                    _chanceOfSurpriseAttFactor = Storage_Combat.SurpriseAttackMultiplier.Value;
                }
                return _chanceOfSurpriseAttFactor;
            }
            set
            {
                _chanceOfSurpriseAttFactor = value;
            }
        }

        public static float RangedCooldownMultiplierBad
        {
            get
            {
                if (_rangedCooldownMultiplierBad < -1)
                {
                    if (Storage_Combat.RangedCooldownLinkedToCaps.Value)
                    {
                        _rangedCooldownMultiplierBad = 1 / Storage.MultiplierCaps.min;
                    }
                    else
                    {
                        _rangedCooldownMultiplierBad = Storage_Combat.RangedCooldownMinAndMax.Value.max / 100f;
                    }
                }
                return _rangedCooldownMultiplierBad;
            }
            set
            {
                RangedCooldownMultiplierBad = value;
            }
        }

        public static float HitChanceGlowTransform(float hitChance, float attGlowFactor)
        {
            return 1 / (1 + (1 / hitChance - 1) * (float) Math.Exp(d: -1 * AttXCoeff * (attGlowFactor - 1)));
        }

        public static string NightVisionTooltipElement(Thing target)
        {
            var result = "";

            if (CurrentShot.NoShot || CurrentShot.Verb.verbProps.forcedMissRadius > 0.5f || CurrentShot.GlowFactor.FactorIsTrivial())
            {
                return result;
            }

            result += "   " + Str_Combat.AimFactorFromLight(glowAtTarget: GlowFor.GlowAt(thing: target), result: CurrentShot.PseudoMultiplier());

            return result;
        }

        public static float AdjustCooldownForGlow(float rangedCooldown, Pawn pawn)
        {
            if (pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                float glow = pawn.Map.glowGrid.GameGlowAt(c: pawn.Position);
                
                if (glow.GlowIsDarkOrBright())
                {
                    float glF = comp.FactorFromGlow(glow: glow);

                    int skillLevel = pawn.skills.GetSkill(skillDef: Defs_Rimworld.ShootSkill).Level;

                    return rangedCooldown * RangedCooldownMultiplier(skill: skillLevel, glowFactor: glF);
                }
            }

            return rangedCooldown;
        }

        #region Ranged

        public static float GlowFactorForPawnAtTarget(Pawn pawn, LocalTargetInfo target, Comp_NightVision comp)
        {
            return comp.FactorFromGlow(glow: GlowFor.GlowAt(map: pawn.Map, pos: target.Cell));
        }

        [NVSettingsDependentField]
        public static float _rangedCooldownMultiplierBad; 
        [NVSettingsDependentField]
        public static float _rangedCooldownMultiplierGood;

        public static float RangedCooldownMultiplierGood
        {
            get
            {
                if (_rangedCooldownMultiplierGood < -1)
                {
                    if (Storage_Combat.RangedCooldownLinkedToCaps.Value)
                    {
                        _rangedCooldownMultiplierGood = 1 / Storage.MultiplierCaps.max;
                    }
                    else
                    {
                        _rangedCooldownMultiplierGood = Storage_Combat.RangedCooldownMinAndMax.Value.min / 100f;
                    }
                }

                return _rangedCooldownMultiplierGood;
            }
        }


        /// <summary>
        ///     if glow factor is &lt; 1f then result is &gt; 1f and tends towards 1f as skill tends towards 20.
        ///     if glow factor is &gt; 1f then result is &lt; 1f and tends towards reciprocal of max glow factor as skill tends
        ///     towards 20.
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="glowFactor"></param>
        /// <returns></returns>
        public static float RangedCooldownMultiplier(int skill, float glowFactor)
        {
            if (glowFactor < 1f - Constants_Calculations.NVEpsilon)
            {
                return 1 + (1 - glowFactor) * (RangedCooldownMultiplierBad) * (1 - (float) Math.Sqrt(d: 0.05f * skill));
            }

            if (glowFactor > 1f + Constants_Calculations.NVEpsilon)
            {
                return 1 + (1 - glowFactor) * (RangedCooldownMultiplierGood) * (float) Math.Sqrt(d: 0.05f * skill);
            }

            return 1;
        }

        #endregion

        #region Melee

        public static float SurpriseAttackChance(float atkGlowFactor, float defGlowFactor)
        {
            return SurpriseAttackChance(glowFactorDelta: atkGlowFactor - defGlowFactor);
        }

        /// <summary>
        ///     Surprise attack chance can never be negative
        /// </summary>
        /// <param name="glowFactorDelta">attacker's - defender's</param>
        /// <returns></returns>
        public static float SurpriseAttackChance(float glowFactorDelta)
        {
            return Mathf.Clamp01(value: glowFactorDelta) * ChanceOfSurpriseAttFactor;
        }

        [NVSettingsDependentField]
        public static float _dodgeXCoeff = Storage_Combat.DodgeCurviness.Value / Storage.MultiplierCaps.Span;


        /// <param name="orgDodge">defenders dodge chance</param>
        /// <param name="glowFactorDelta">AttGlowFactor - DefGlowFactor</param>
        /// <returns></returns>
        public static float DodgeChanceFunction(float orgDodge, float glowFactorDelta)
        {
            if (glowFactorDelta.IsTrivial())
            {
                return orgDodge;
            }
            return 2 * orgDodge / (1 + (float) Math.Exp(d: DodgeXCoeff * glowFactorDelta));
        }

        #endregion
        [NVSettingsDependentField]
        public static float _chanceOfSurpriseAttFactor = Storage_Combat.SurpriseAttackMultiplier.Value;
    }
}