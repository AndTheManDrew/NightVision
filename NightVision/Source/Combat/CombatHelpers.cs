using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace NightVision
{
    public static class CombatHelpers
    {
        #region Ranged
        [TweakValue("NightVision", 0, 10)]
        public static float GlowFactorAimExponent = 1;

        [TweakValue("NightVision", 0, 3)]
        public static int ShootCalcFunction = (int) Functions_ShootCalc.Power;
        
        public enum Functions_ShootCalc
        {
            None = 0,
            Power = 1,
            SqRoot,
            Exponential
        }

        public static float CalcShootGlowFactor(float factorFromGlow)
        {
            switch (ShootCalcFunction)
            {
                    case (int)Functions_ShootCalc.Power:
                        return Math.Min((float) Math.Pow(factorFromGlow, GlowFactorAimExponent), 0.99f);
                    case (int)Functions_ShootCalc.Exponential: break;
                    case (int)Functions_ShootCalc.SqRoot: break;
                    default: return factorFromGlow;
            }
            return Math.Min((float) Math.Pow(factorFromGlow, GlowFactorAimExponent), 0.99f);
        }
        
        
        
        
        public static float ShootGlowFactor(
                        Pawn            pawn,
                        LocalTargetInfo target,
                        Comp_NightVision comp
                    )
        {
            return CalcShootGlowFactor(comp.FactorFromGlow(pawn.Map.glowGrid.GameGlowAt(target.Cell)));
        }

        public static float ShootGlowFactor(
                        Pawn             pawn,
                        Thing  target,
                        Comp_NightVision comp,
                        out float        glow
                    )
        {
            glow = pawn.Map.glowGrid.GameGlowAt(target.Position);
            return CalcShootGlowFactor(comp.FactorFromGlow(glow));
        }





        public static float AdjustCooldownForGlow(
            float rangedCooldown,
            Pawn  pawn
        )
        {
            if (pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                float glow = pawn.Map.glowGrid.GameGlowAt(pawn.Position);

                if (glow < 0.2999 || glow > 0.7001)
                {
                    var glF = comp.FactorFromGlow(glow);

                    float adjusted = rangedCooldown / glF;

                    if (adjusted < rangedCooldown - 0.00001f)
                    {
                        adjusted = Mathf.Lerp(adjusted, rangedCooldown, pawn.skills.GetSkill(SkillDefOf.Shooting).Level / Constants.ShootSkillCooldownLimit);
                    }

                    return adjusted;
                }
            }

            return rangedCooldown;
        }
        #endregion

        #region Melee
        
        #endregion
    }
}
