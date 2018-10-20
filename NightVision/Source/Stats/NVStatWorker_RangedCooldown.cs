// Nightvision NightVision NVStatWorker_RangedCooldown.cs
// 
// 17 10 2018
// 
// 17 10 2018

using System;
using RimWorld;
using Verse;

namespace NightVision
{
    public class NVStatWorker_RangedCooldown : NVStatWorker
    {

        public SkillDef DerivedFrom = RwDefs.ShootSkill;
        #region Overrides of NVStatWorker

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            if (req.Thing is Pawn pawn)
            {
                int skillLevel = pawn.skills.GetSkill(DerivedFrom).Level;

                return StatReportFor_NightVision.RangedCoolDown(pawn, skillLevel);
            }
            return String.Empty;
        }

        public override string GetStatDrawEntryLabel(StatDef statDef, float value, ToStringNumberSense numberSense, StatRequest optionalReq)
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

            return CalcConstants.TrivialFactor;
        }

        #region Overrides of StatWorker

        public override void FinalizeValue(StatRequest req, ref float val, bool applyPostProcess)
        {

            Log.Message(new string('-', 20));
            Log.Message($"NVStatWorker_RangedCooldown");
            Log.Message($"FinalizeValue");
            
            Log.Message($"val = {val}");
            
            base.FinalizeValue(req, ref val, applyPostProcess);
            Log.Message($"after base:");
            
            Log.Message($"val = {val}");
            Log.Message(new string('-', 20));
        }

        #endregion

        public override bool IsDisabledFor(Thing thing)
        {
            return base.IsDisabledFor(thing) || !(thing is Pawn pawn && !pawn.skills.GetSkill(DerivedFrom).TotallyDisabled);
        }


        #endregion

    }
}