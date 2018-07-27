using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace NightVision
{
    [UsedImplicitly]
    public class ThoughtWorker_TooBright : ThoughtWorker
    {
        public ThoughtWorker_TooBright() { }

        protected override ThoughtState CurrentStateInternal(
            Pawn p)
            {
                return p.Awake() && p.GetComp<Comp_NightVision>() is Comp_NightVision comp && comp.PsychBright();
            }

        public static void SetLastDarkTick(Pawn pawn)
            {
                if (pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
                    {
                        comp.lastDarkTick = Find.TickManager.TicksGame;
                    }
            }
    }
}
