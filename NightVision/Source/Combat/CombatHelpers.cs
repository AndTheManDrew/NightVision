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
    public static class CombatHelpers
    {
        public static float AttXCoeff = 2 / Storage.MultiplierCaps.Span;

        public static float HitChanceGlowTransform(float hitChance, float attGlowFactor)
        {
#if DEBUG

            Log.Message(text: new string(c: '-', count: 20));
            Log.Message(text: "HitChanceGlowTransform");
            Log.Message(text: $"attGlowFactor = {attGlowFactor}");
            Log.Message(text: $"hitChance = {hitChance}");
            float finalHitChance = 1 / (1 + (1 / hitChance - 1) * (float) Math.Exp(d: -1 * AttXCoeff * (attGlowFactor - 1)));
            Log.Message(text: $"finalHitChance = {finalHitChance}");
            Log.Message(text: new string(c: '-', count: 20));
#endif
            return 1 / (1 + (1 / hitChance - 1) * (float) Math.Exp(d: -1 * AttXCoeff * (attGlowFactor - 1)));
        }

        public static string NightVisionTooltipElement(Thing target)
        {
            var result = "";

            if (CurrentShot.NoShot || CurrentShot.Verb.verbProps.forcedMissRadius > 0.5f || CurrentShot.GlowFactor.FactorIsTrivial())
            {
                return result;
            }

            result += "   " + Str.AimFactorFromLight(glowAtTarget: GlowFor.GlowAt(thing: target), result: CurrentShot.PseudoMultiplier());

            return result;
        }

        public static float AdjustCooldownForGlow(float rangedCooldown, Pawn pawn)
        {
            if (pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                float glow = pawn.Map.glowGrid.GameGlowAt(c: pawn.Position);

                Log.Message(text: new string(c: '-', count: 20));
                Log.Message(text: "VerbProperties_AdjustedCooldown");
                Log.Message(text: "AdjustCooldownForGlow");
                Log.Message(text: new string(c: '-', count: 20));

                if (glow.GlowIsDarkOrBright())
                {
                    float glF = comp.FactorFromGlow(glow: glow);

                    int skillLevel = pawn.skills.GetSkill(skillDef: RwDefs.ShootSkill).Level;

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
            if (glowFactor < 1f - CalcConstants.NVEpsilon)
            {
                return 1 + (1 - glowFactor) * (1 / Storage.MultiplierCaps.min) * (1 - (float) Math.Sqrt(d: 0.05f * skill));
            }

            if (glowFactor > 1f + CalcConstants.NVEpsilon)
            {
                return 1 + (1 - glowFactor) * (1 / Storage.MultiplierCaps.max) * (float) Math.Sqrt(d: 0.05f * skill);
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
            return Mathf.Clamp01(value: glowFactorDelta) * CurrentStrike.ChanceOfSurpriseAttFactor;
        }

        public static float DodgeXCoeff = 5 / Storage.MultiplierCaps.Span;


        /// <param name="orgDodge">defenders dodge chance</param>
        /// <param name="glowFactorDelta">AttGlowFactor - DefGlowFactor</param>
        /// <returns></returns>
        public static float DodgeChanceFunction(float orgDodge, float glowFactorDelta)
        {
            if (glowFactorDelta.ApproxZero())
            {
                return 0;
            }
            return 2 * orgDodge / (1 + (float) Math.Exp(d: DodgeXCoeff * glowFactorDelta));
        }

        #endregion
    }
}