// Nightvision NightVision StatReportFor_NightVision_Combat.cs
// 
// 25 10 2018
// 
// 06 12 2018

using System.Text;
using NightVision;
using RimWorld;
using Verse;

using Mod = NightVision.Mod;

public static class StatReportFor_NightVision_Combat
{
    #region  Members

    public static string CombatPart(Pawn pawn, Comp_NightVision comp)
    {
        var stringbuilder = new StringBuilder();

        float nvFactor     = comp.FactorFromGlow(glow: 0);
        float psFactor     = comp.FactorFromGlow(glow: 1);
        float pawnDodgeVal = pawn.GetStatValue(stat: Defs_Rimworld.MeleeDodgeStat);
        float meleeHit     = pawn.GetStatValue(stat: Defs_Rimworld.MeleeHitStat, applyPostProcess: true);

        stringbuilder.AppendLine(value: Str_Combat.LMDef);

        stringbuilder.AppendLine(value: Str_Combat.AnimalAndMechNote);

        //Hit Chance
        stringbuilder.AppendLine();

        stringbuilder.AppendLine(
            value: Str_Combat.HitChanceTitle.PadLeft(totalWidth: 20, paddingChar: '-').PadRight(totalWidth: 30, paddingChar: '-')
        );

        stringbuilder.AppendLine(value: Str_Combat.HitChanceHeader());


        if (Storage_Combat.RangedHitEffectsEnabled.Value)
        {
            stringbuilder.AppendLine(value: Str_Combat.ShootTargetAtGlow());


            for (var i = 1; i <= 4; i++)
            {
                float hit = ShotReport.HitFactorFromShooter(caster: pawn, distance: i * 5);

                stringbuilder.AppendLine(
                    value: Str_Combat.ShotChanceTransform(
                        distance: i * 5,
                        hitChance: hit,
                        nvResult: CombatHelpers.HitChanceGlowTransform(hitChance: hit, attGlowFactor: nvFactor),
                        psResult: CombatHelpers.HitChanceGlowTransform(hitChance: hit, attGlowFactor: psFactor)
                    )
                );
            }

            stringbuilder.AppendLine();
        }

        if (!Storage_Combat.MeleeHitEffectsEnabled.Value)
        {
            return stringbuilder.ToString();
        }

        var caps = Mod.Store.MultiplierCaps;
        
        stringbuilder.AppendLine(value: Str_Combat.StrikeTargetAtGlow());


        stringbuilder.AppendLine(
            value: Str_Combat.StrikeChanceTransform(
                hitChance: meleeHit,
                nvResult: CombatHelpers.HitChanceGlowTransform(hitChance: meleeHit, attGlowFactor: nvFactor),
                psResult: CombatHelpers.HitChanceGlowTransform(hitChance: meleeHit, attGlowFactor: psFactor)
            )
        );

        stringbuilder.AppendLine();

        stringbuilder.AppendLine(
            value: Str_Combat.SurpriseAttackTitle.PadLeft(totalWidth: 20, paddingChar: '-').PadRight(totalWidth: 30, paddingChar: '-')
        );

        stringbuilder.AppendLine(value: Str_Combat.SurpriseAtkDesc());
        stringbuilder.AppendLine(value: Str_Combat.SurpriseAtkChance());
        float nvSAtk = CombatHelpers.SurpriseAttackChance(atkGlowFactor: nvFactor, defGlowFactor: caps.min);
        float psSAtk = CombatHelpers.SurpriseAttackChance(atkGlowFactor: psFactor, defGlowFactor: caps.min);
        stringbuilder.AppendLine();
        stringbuilder.AppendLine(value: Str_Combat.SurpriseAtkCalcHeader());

        stringbuilder.AppendLine(
            value: Str_Combat.SurpriseAtkCalcRow(glow: 0f, atkGlowF: nvFactor, defGlowF: caps.min, chance: nvSAtk)
        );

        if (pawnDodgeVal.IsTrivial())
        {
            if (nvSAtk.IsNonTrivial())
            {
                nvSAtk = CombatHelpers.SurpriseAttackChance(atkGlowFactor: nvFactor, defGlowFactor: 1f);
                stringbuilder.AppendLine(value: Str_Combat.SurpriseAtkCalcRow(glow: 0f, atkGlowF: nvFactor, defGlowF: 1f, chance: nvSAtk));

                if (nvSAtk.IsNonTrivial())
                {
                    nvSAtk = CombatHelpers.SurpriseAttackChance(atkGlowFactor: nvFactor, defGlowFactor: caps.max);

                    stringbuilder.AppendLine(
                        value: Str_Combat.SurpriseAtkCalcRow(glow: 0f, atkGlowF: nvFactor, defGlowF: caps.max, chance: nvSAtk)
                    );
                }
            }
        }

        stringbuilder.AppendLine(value: Str_Combat.SurpriseAtkCalcRow(glow: 1f, atkGlowF: psFactor, defGlowF: 1f, chance: psSAtk));

        if (pawnDodgeVal.IsTrivial())
        {
            psSAtk = CombatHelpers.SurpriseAttackChance(atkGlowFactor: psFactor, defGlowFactor: caps.min);

            stringbuilder.AppendLine(
                value: Str_Combat.SurpriseAtkCalcRow(
                    glow: caps.min,
                    atkGlowF: psFactor,
                    defGlowF: caps.min,
                    chance: psSAtk
                )
            );
        }


        stringbuilder.AppendLine();

        stringbuilder.AppendLine(
            value: Str_Combat.DodgeTitle.PadLeft(totalWidth: 20, paddingChar: '-').PadRight(totalWidth: 30, paddingChar: '-')
        );

        stringbuilder.AppendLine(value: Str_Combat.Dodge());
        stringbuilder.AppendLine();

        stringbuilder.AppendLine(value: Str_Combat.DodgeCalcHeader());

        stringbuilder.AppendLine(
            value: Str_Combat.DodgeCalcRow(
                glow: 0f,
                atkGlowF: caps.min,
                defGlowF: nvFactor,
                dodge: pawnDodgeVal,
                newDodge: CombatHelpers.DodgeChanceFunction(orgDodge: pawnDodgeVal, glowFactorDelta: caps.min - nvFactor)
            )
        );

        if (pawnDodgeVal.IsNonTrivial())
        {
            stringbuilder.AppendLine(
                value: Str_Combat.DodgeCalcRow(
                    glow: 0f,
                    atkGlowF: 1f,
                    defGlowF: nvFactor,
                    dodge: pawnDodgeVal,
                    newDodge: CombatHelpers.DodgeChanceFunction(orgDodge: pawnDodgeVal, glowFactorDelta: 1f - nvFactor)
                )
            );

            stringbuilder.AppendLine(
                value: Str_Combat.DodgeCalcRow(
                    glow: 0f,
                    atkGlowF: caps.max,
                    defGlowF: nvFactor,
                    dodge: pawnDodgeVal,
                    newDodge: CombatHelpers.DodgeChanceFunction(orgDodge: pawnDodgeVal, glowFactorDelta: caps.max - nvFactor)
                )
            );

            stringbuilder.AppendLine(
                value: Str_Combat.DodgeCalcRow(
                    glow: 1f,
                    atkGlowF: caps.min,
                    defGlowF: psFactor,
                    dodge: pawnDodgeVal,
                    newDodge: CombatHelpers.DodgeChanceFunction(orgDodge: pawnDodgeVal, glowFactorDelta: caps.min - psFactor)
                )
            );
        }

        stringbuilder.AppendLine(
            value: Str_Combat.DodgeCalcRow(
                glow: 1f,
                atkGlowF: 1f,
                defGlowF: psFactor,
                dodge: pawnDodgeVal,
                newDodge: CombatHelpers.DodgeChanceFunction(orgDodge: pawnDodgeVal, glowFactorDelta: 1f - psFactor)
            )
        );


        return stringbuilder.ToString();
    }

    public static string RangedCoolDown(Pawn pawn, int skillLevel)
    {
        var   stringBuilder = new StringBuilder();
        float glow          = GlowFor.GlowAt(thing: pawn);
        float glowFactor    = GlowFor.FactorOrFallBack(pawn: pawn, glow: glow);

        stringBuilder.AppendLine(
            value: Str_Combat.RangedCooldown(
                glow: glow,
                skill: skillLevel,
                result: CombatHelpers.RangedCooldownMultiplier(skill: skillLevel, glowFactor: glowFactor)
            )
        );

        glow       = 1f;
        glowFactor = GlowFor.FactorOrFallBack(pawn: pawn, glow: glow);

        stringBuilder.AppendLine(
            value: Str_Combat.RangedCooldownDemo(
                glow: glow,
                result: CombatHelpers.RangedCooldownMultiplier(skill: skillLevel, glowFactor: glowFactor)
            )
        );

        glow       = 0f;
        glowFactor = GlowFor.FactorOrFallBack(pawn: pawn, glow: glow);

        stringBuilder.AppendLine(
            value: Str_Combat.RangedCooldownDemo(
                glow: glow,
                result: CombatHelpers.RangedCooldownMultiplier(skill: skillLevel, glowFactor: glowFactor)
            )
        );

        return stringBuilder.ToString();
    }

    #endregion
}