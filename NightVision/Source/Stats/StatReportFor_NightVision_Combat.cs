using System.Text;
using NightVision;
using RimWorld;
using Verse;

static public class StatReportFor_NightVision_Combat {
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
            value: Str_Combat.RangedCooldownDemo(glow: glow, result: CombatHelpers.RangedCooldownMultiplier(skill: skillLevel, glowFactor: glowFactor))
        );

        glow       = 0f;
        glowFactor = GlowFor.FactorOrFallBack(pawn: pawn, glow: glow);

        stringBuilder.AppendLine(
            value: Str_Combat.RangedCooldownDemo(glow: glow, result: CombatHelpers.RangedCooldownMultiplier(skill: skillLevel, glowFactor: glowFactor))
        );

        return stringBuilder.ToString();
    }

    public static string CombatPart(Pawn pawn, Comp_NightVision comp)
    {
        var stringbuilder = new StringBuilder();

        float nvFactor     = comp.FactorFromGlow(0);
        float psFactor     = comp.FactorFromGlow(1);
        float pawnDodgeVal = pawn.GetStatValue(stat: Defs_Rimworld.MeleeDodgeStat);
        float meleeHit     = pawn.GetStatValue(stat: Defs_Rimworld.MeleeHitStat, applyPostProcess: true);

        stringbuilder.AppendLine(Str_Combat.LMDef);

        stringbuilder.AppendLine(Str_Combat.AnimalAndMechNote);

        //Hit Chance
        stringbuilder.AppendLine();
        stringbuilder.AppendLine(Str_Combat.HitChanceTitle.PadLeft(20, '-').PadRight(30, '-'));
        stringbuilder.AppendLine(Str_Combat.HitChanceHeader());


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
                        psResult: CombatHelpers.HitChanceGlowTransform(hit,            psFactor)
                    )
                );
            }
            stringbuilder.AppendLine();
        }

        if (Storage_Combat.MeleeHitEffectsEnabled.Value)
        {
            stringbuilder.AppendLine(value: Str_Combat.StrikeTargetAtGlow());


        stringbuilder.AppendLine(
            value: Str_Combat.StrikeChanceTransform(
                hitChance: meleeHit,
                CombatHelpers.HitChanceGlowTransform(hitChance: meleeHit, attGlowFactor: nvFactor),
                CombatHelpers.HitChanceGlowTransform(meleeHit,            psFactor)
            )
        );
        stringbuilder.AppendLine();
        stringbuilder.AppendLine(Str_Combat.SurpriseAttackTitle.PadLeft(20, '-').PadRight(30, '-'));
        stringbuilder.AppendLine(value: Str_Combat.SurpriseAtkDesc());
        stringbuilder.AppendLine(value: Str_Combat.SurpriseAtkChance());
        float nvSAtk = CombatHelpers.SurpriseAttackChance(atkGlowFactor: nvFactor, defGlowFactor: Storage.MultiplierCaps.min);
        float psSAtk = CombatHelpers.SurpriseAttackChance(psFactor,                Storage.MultiplierCaps.min);
        stringbuilder.AppendLine();
        stringbuilder.AppendLine(Str_Combat.SurpriseAtkCalcHeader());

        stringbuilder.AppendLine(
            value: Str_Combat.SurpriseAtkCalcRow(0f, nvFactor, Storage.MultiplierCaps.min, nvSAtk)
        );

        if (pawnDodgeVal.IsTrivial())
        {
            if (nvSAtk.IsNonTrivial())
            {
                nvSAtk = CombatHelpers.SurpriseAttackChance(atkGlowFactor: nvFactor, defGlowFactor: 1f);
                stringbuilder.AppendLine(
                    value: Str_Combat.SurpriseAtkCalcRow(0f, nvFactor, 1f, nvSAtk)
                );

                if (nvSAtk.IsNonTrivial())
                {
                    nvSAtk = CombatHelpers.SurpriseAttackChance(nvFactor, Storage.MultiplierCaps.max);
                    stringbuilder.AppendLine(Str_Combat.SurpriseAtkCalcRow(0f, nvFactor, Storage.MultiplierCaps.max, nvSAtk));
                }
                    
            }
        }

        stringbuilder.AppendLine(
            value: Str_Combat.SurpriseAtkCalcRow(1f, psFactor, 1f, psSAtk)
        );

        if (pawnDodgeVal.IsTrivial())
        {
            psSAtk = CombatHelpers.SurpriseAttackChance(psFactor, Storage.MultiplierCaps.min);
            stringbuilder.AppendLine(Str_Combat.SurpriseAtkCalcRow(Storage.MultiplierCaps.min, psFactor, Storage.MultiplierCaps.min, psSAtk));

        }



        stringbuilder.AppendLine();
        stringbuilder.AppendLine(Str_Combat.DodgeTitle.PadLeft(20, '-').PadRight(30, '-'));
        stringbuilder.AppendLine(value: Str_Combat.Dodge());
        stringbuilder.AppendLine();

        stringbuilder.AppendLine(value: Str_Combat.DodgeCalcHeader());

        stringbuilder.AppendLine(Str_Combat.DodgeCalcRow(0f, Storage.MultiplierCaps.min, nvFactor, pawnDodgeVal, CombatHelpers.DodgeChanceFunction(pawnDodgeVal, Storage.MultiplierCaps.min - nvFactor))
        );
        if (pawnDodgeVal.IsNonTrivial())
        {
            stringbuilder.AppendLine(Str_Combat.DodgeCalcRow(0f, 1f, nvFactor, pawnDodgeVal, CombatHelpers.DodgeChanceFunction(pawnDodgeVal, 1f - nvFactor))
            );

            stringbuilder.AppendLine(Str_Combat.DodgeCalcRow(0f, Storage.MultiplierCaps.max, nvFactor, pawnDodgeVal, CombatHelpers.DodgeChanceFunction(pawnDodgeVal, Storage.MultiplierCaps.max - nvFactor))
            );

            stringbuilder.AppendLine(Str_Combat.DodgeCalcRow(1f, Storage.MultiplierCaps.min, psFactor, pawnDodgeVal, CombatHelpers.DodgeChanceFunction(pawnDodgeVal, Storage.MultiplierCaps.min - psFactor))
            );
        }
        stringbuilder.AppendLine(Str_Combat.DodgeCalcRow(1f, 1f, psFactor, pawnDodgeVal, CombatHelpers.DodgeChanceFunction(pawnDodgeVal, 1f - psFactor))
        );
        }
        
            
        return stringbuilder.ToString();
    }
}