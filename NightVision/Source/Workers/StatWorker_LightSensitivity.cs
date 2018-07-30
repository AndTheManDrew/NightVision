using System.Linq;
using System.Text;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace NightVision
    {
        [UsedImplicitly]
        public class StatWorker_LightSensitivity : StatWorker
            {

                public override string GetExplanationUnfinalized(
                    StatRequest         req,
                    ToStringNumberSense numberSense)
                    {
                        if (req.HasThing && req.Thing is Pawn pawn
                                         && pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
                            {
                                string result = comp.ExplanationBuilder(string.Empty, 1f, out bool usedApparelSetting);
                                if (usedApparelSetting)
                                    {
                                        var builder = new StringBuilder(result);
                                        builder.AppendLine("StatsReport_RelevantGear".Translate() + ":");
                                        foreach (Apparel app in comp.PawnsNVApparel ?? Enumerable.Empty<Apparel>())
                                            {
                                                if (Storage.NVApparel.TryGetValue(app.def,
                                                        out ApparelVisionSetting setting)
                                                    && setting.NullifiesPS)
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
                    StatDef             stat,
                    float               value,
                    ToStringNumberSense numberSense,
                    StatRequest         optionalReq)
                    {
                        if (optionalReq.HasThing && optionalReq.Thing is Pawn pawn
                                                 && pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
                            {
                                string result = $"x{comp.FullLightModifier + Constants.DefaultFullLightMultiplier:0%}";
                                return result;
                            }

                        return string.Empty;
                    }

                public override float GetValueUnfinalized(
                    StatRequest req,
                    bool        applyPostProcess = true)
                    {
                        if (req.HasThing && req.Thing is Pawn pawn
                                         && pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
                            {
                                return comp.FactorFromGlow(1f);
                            }

                        return Constants.DefaultFullLightMultiplier;
                    }

                public override bool IsDisabledFor(
                    Thing thing) =>
                            thing is Pawn pawn && pawn.GetComp<Comp_NightVision>() == null;

                
    }
    }