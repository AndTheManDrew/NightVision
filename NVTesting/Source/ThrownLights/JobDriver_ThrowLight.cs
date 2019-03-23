// Nightvision NVTesting JobDriver_ThrowLight.cs
// 
// 19 03 2019
// 
// 19 03 2019

using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace NVTesting.ThrownLights
{
    public class JobDriver_ThrowLight : JobDriver_AttackStatic
    {
        private bool finishedThrowing;
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
            yield return new Toil
                         {
                             initAction = delegate()
                             {
                                 this.pawn.pather.StopDead();
                             },
                             tickAction = delegate()
                             {
                                 if (!base.TargetA.IsValid)
                                 {
                                     base.EndJobWith(JobCondition.Succeeded);
                                     return;
                                 }
                                 if (base.TargetA.HasThing)
                                 {
                                     if (!base.TargetA.Thing.Destroyed)
                                     {
                                         base.EndJobWith(JobCondition.Incompletable);
                                         return;
                                     }
                                 }

                                 if (finishedThrowing && !pawn.stances.FullBodyBusy)
                                 {
                                     EndJobWith(JobCondition.Succeeded);
                                 }
                                 if (!this.pawn.stances.FullBodyBusy)
                                 {

                                     // TODO less hardcoding
                                     CompEquipable_SecondaryThrown comp = pawn.equipment.AllEquipmentListForReading
                                                 .Find(th => th.def == DefOfs.ThrowingTorch)?.GetComp<CompEquipable_SecondaryThrown>();

                                     if (comp?.PrimaryVerb.TryStartCastOn(TargetA, false, false) == true)
                                     {
                                         finishedThrowing = true;
                                         
                                     }
                                     else if(job.endIfCantShootTargetFromCurPos && (comp.PrimaryVerb == null || !comp.PrimaryVerb.CanHitTargetFrom(this.pawn.Position, base.TargetA)))
                                     {
                                         EndJobWith(JobCondition.Incompletable);
                                     }
                                 }
                             },
                             defaultCompleteMode = ToilCompleteMode.Never
                         };
            yield break;
        }
        
    }

    
}