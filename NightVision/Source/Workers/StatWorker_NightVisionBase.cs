using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using RimWorld;
using Verse;

namespace NightVision {
    public class StatWorker_NightVisionBase : StatWorker
    {
        public float Glow;

        public FieldInfo RelevantField;

        public float DefaultStatValue;
        

        public override string GetExplanationUnfinalized(
            StatRequest         req,
            ToStringNumberSense numberSense
        )
        {
            if (req.HasThing
                && req.Thing is Pawn pawn
                && pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                return StatReportFor_NightVision.CompleteStatReport(this, comp);
            }

            return string.Empty;
        }

        public override string GetStatDrawEntryLabel(
            StatDef             statDef,
            float               value,
            ToStringNumberSense numberSense,
            StatRequest         optionalReq
        )
        {
            if (optionalReq.HasThing
                && optionalReq.Thing is Pawn pawn
                && pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                string result = $"x{comp.FactorFromGlow(Glow):0.0%}";

                return result;
            }

            return string.Empty;
        }

        public override float GetValueUnfinalized(
            StatRequest req,
            bool        applyPostProcess = true
        )
        {
            if (req.HasThing
                && req.Thing is Pawn pawn
                && pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                return comp.FactorFromGlow(Glow);
            }

            return DefaultStatValue;
        }

        public override bool IsDisabledFor(
            Thing thing
        )
            => thing is Pawn pawn && pawn.GetComp<Comp_NightVision>() == null;
    }
}