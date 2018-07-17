using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.AI;

namespace NightVision
{
    class JobDriver_DigNest : JobDriver
    {
        public override bool TryMakePreToilReservations() => false;

        protected override IEnumerable<Toil> MakeNewToils()
            {
                yield break;
            }
    }
}
