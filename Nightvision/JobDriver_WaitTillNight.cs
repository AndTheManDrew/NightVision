using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.AI;

namespace NightVision
{
    //class JobDriver_WaitTillNight : JobDriver
    //    {
    //        private const TargetIndex WaitSpot = TargetIndex.A;
    //        private const int WaitingTicks = 10000;
    //    public override bool TryMakePreToilReservations()
    //        {
    //            return this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job, 1, -1, null);
    //        }

    //    protected override IEnumerable<Toil> MakeNewToils()
    //        {
    //            yield return Toils_Goto.GotoCell(WaitSpot, PathEndMode.OnCell);
    //            yield return Toils_General.WaitWith(WaitSpot, WaitingTicks, false, true);
    //        }
    //}
}
