using JetBrains.Annotations;
using Verse;

namespace NightVision.Comps
{
    /// <summary>
    /// More comps and props and comps and props and comps and props
    /// </summary>
    public class HediffComp_NightVision : HediffComp
    {
        internal bool AffectsEyes = false;

        [UsedImplicitly]
        public HediffCompProperties_NightVision Props
        {
            get { return (HediffCompProperties_NightVision) props; }
        }


        public override string CompTipStringExtra
        {
            get { return base.CompTipStringExtra; }
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            if (Pawn is Pawn pawn && pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                comp.CheckAndAddHediff(parent, parent.Part);
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
        }
        public override void CompPostMerged(Hediff other)
        {
            base.CompPostMerged(other);
        }
    }
    
}

