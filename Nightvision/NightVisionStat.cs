using System.Linq;
using System.Text;
using JetBrains.Annotations;
using NightVision.Comps;
using RimWorld;
using Verse;

namespace NightVision
    {
        [DefOf]
        [UsedImplicitly]
        public static class NightVisionStatDefOf
            {
                [UsedImplicitly] public static StatDef NightVision      = StatDef.Named("NightVision");
                [UsedImplicitly] public static StatDef LightSensitivity = StatDef.Named("LightSensitivity");
            }

        [UsedImplicitly]
        public class StatWorker_NightVision : StatWorker
            {
                public override string GetExplanationUnfinalized(
                    StatRequest         req,
                    ToStringNumberSense numberSense)
                    {
                        if (req.HasThing && req.Thing is Pawn pawn
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
                                                if (NightVisionSettings.NVApparel.TryGetValue(app.def,
                                                        out ApparelSetting setting)
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
                    StatDef             stat,
                    float               value,
                    ToStringNumberSense numberSense,
                    StatRequest         optionalReq)
                    {
                        if (optionalReq.HasThing)
                            {
                                if (optionalReq.Thing is Pawn pawn
                                    && pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
                                    {
                                        return
                                                    $"x{comp.ZeroLightModifier + NightVisionSettings.DefaultZeroLightMultiplier:0%}";
                                    }

                                if (optionalReq.Thing is Apparel apparel
                                    && NightVisionSettings.NVApparel.TryGetValue(apparel.def,
                                        out ApparelSetting apparelSetting)
                                    && apparelSetting.GrantsNV)
                                    {
                                        return "NVGiveNV".Translate();
                                    }
                                
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
                                return comp.FactorFromGlow(0f);
                            }

                        return NightVisionSettings.DefaultZeroLightMultiplier;
                    }

                public override bool IsDisabledFor(
                    Thing thing) =>
                            thing is Pawn pawn && pawn.GetComp<Comp_NightVision>() == null;
            }

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
                                        builder.AppendLine();
                                        builder.AppendLine();
                                        builder.AppendLine("StatsReport_RelevantGear".Translate() + ":");
                                        foreach (Apparel app in comp.PawnsNVApparel ?? Enumerable.Empty<Apparel>())
                                            {
                                                if (NightVisionSettings.NVApparel.TryGetValue(app.def,
                                                        out ApparelSetting setting)
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
                                string result =
                                            $"x{comp.FullLightModifier + NightVisionSettings.DefaultFullLightMultiplier:0%}";
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

                        return NightVisionSettings.DefaultFullLightMultiplier;
                    }

                public override bool IsDisabledFor(
                    Thing thing) =>
                            thing is Pawn pawn && pawn.GetComp<Comp_NightVision>() == null;
            }
    }