﻿// Nightvision NightVision NVStatWorker_RangedCooldown.cs
// 
// 17 10 2018
// 
// 17 10 2018

using JetBrains.Annotations;
using RimWorld;
using System;
using Verse;

namespace NightVision
{

    [UsedImplicitly]
    public class NVStatWorker_RangedCooldown : NVStatWorker
    {

        public SkillDef DerivedFrom = Defs_Rimworld.ShootSkill;
        #region Overrides of NVStatWorker

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            if (req.Thing is Pawn pawn)
            {
                int skillLevel = pawn.skills.GetSkill(DerivedFrom).Level;

                return StatReportFor_NightVision_Combat.RangedCoolDown(pawn, skillLevel);
            }
            return String.Empty;
        }

        public override string GetStatDrawEntryLabel(StatDef statDef, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized = true)
        {
            return $"x{GetValueUnfinalized(optionalReq).ToStringPercent()}";
        }

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            if (req.Thing is Pawn pawn)
            {
                float glowFactor = GlowFor.FactorOrFallBack(pawn);

                if (glowFactor.FactorIsNonTrivial())
                {
                    return CombatHelpers.RangedCooldownMultiplier(pawn.skills.GetSkill(DerivedFrom).Level, glowFactor);
                }
            }

            return Constants.TRIVIAL_FACTOR;
        }

        #region Overrides of StatWorker

        public override void FinalizeValue(StatRequest req, ref float val, bool applyPostProcess)
        {

            base.FinalizeValue(req, ref val, applyPostProcess);
        }

        #endregion

        public override bool IsDisabledFor(Thing thing)
        {
            return base.IsDisabledFor(thing) || !(thing is Pawn pawn && !pawn.skills.GetSkill(DerivedFrom).TotallyDisabled);
        }

        public override bool ShouldShowFor(StatRequest req)
        {
            return base.ShouldShowFor(req) || !Settings.CombatStore.RangedCooldownEffectsEnabled.Value;
        }

        #endregion

    }
}