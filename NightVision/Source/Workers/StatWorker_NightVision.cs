using System.Linq;
using System.Text;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace NightVision
{
    [UsedImplicitly]
    public class StatWorker_NightVision : StatWorker
    {
        public override string GetExplanationUnfinalized(
                        StatRequest         req,
                        ToStringNumberSense numberSense
                    )
        {
            if (req.HasThing
                && req.Thing is Pawn pawn
                && pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                string result = comp.ExplanationBuilder(string.Empty, 0f, out bool usedApparelSetting);

                if (usedApparelSetting)
                {
                    var builder = new StringBuilder(result);
                    builder.AppendLine();
                    builder.AppendLine();
                    builder.AppendLine("StatsReport_RelevantGear".Translate());

                    foreach (Apparel app in comp.PawnsNVApparel ?? Enumerable.Empty<Apparel>())
                    {
                        if (Storage.NVApparel.TryGetValue(
                                                          app.def,
                                                          out ApparelVisionSetting setting
                                                         )
                            && setting.GrantsNV)
                        {
                            builder.AppendLine(app.LabelCap);
                        }
                    }

                    return builder.ToString();
                }

                return result;
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
            if (optionalReq.HasThing)
            {
                if (optionalReq.Thing is Pawn pawn
                    && pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
                {
                    return $"x{comp.FactorFromGlow(0f):0.0%}";
                }

                //if (optionalReq.Thing is Apparel apparel
                //    && Storage.NVApparel.TryGetValue(
                //                                     apparel.def,
                //                                     out ApparelVisionSetting apparelSetting
                //                                    )
                //    && apparelSetting.GrantsNV)
                //{
                //    return "NVGiveNV".Translate();
                //}
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
                return comp.FactorFromGlow(0f);
            }

            return Constants.DefaultZeroLightMultiplier;
        }

        public override bool IsDisabledFor(
                        Thing thing
                    )
            => thing is Pawn pawn && pawn.GetComp<Comp_NightVision>() == null;
    }
}