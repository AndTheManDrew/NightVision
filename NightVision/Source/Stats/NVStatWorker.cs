﻿using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace NightVision {
    public class NVStatWorker : StatWorker
    {
        public float _glow;

        public FieldInfo _relevantField;

        public float DefaultStatValue;

        public string Acronym;

        public StatDef Stat => stat;

        public virtual float Glow
        {
            get
            {
                return _glow;
            }
            set
            {
                _glow = value;
            }
        }

        public virtual FieldInfo RelevantField
        {
            get
            {
                return _relevantField;
            }
            set
            {
                _relevantField = value;
            }
        }

        public override string GetExplanationUnfinalized(
            StatRequest         req,
            ToStringNumberSense numberSense
        )
        {
            if (req.Thing is Pawn pawn
                && pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                return StatReportFor_NightVision.CompleteStatReport(Stat, RelevantField, comp, Glow);
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
            return $"x{GetValueUnfinalized(optionalReq).ToStringPercent()}";
        }

        public override float GetValueUnfinalized(
            StatRequest req,
            bool        applyPostProcess = true
        )
        {
            if (req.Thing is Pawn pawn
                && pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                return comp.FactorFromGlow(Glow);
            }

            return DefaultStatValue;
        }

        public override bool IsDisabledFor(
            Thing thing
        )
        {
            return !(thing is Pawn pawn) || pawn.GetComp<Comp_NightVision>() == null;
        }

        public override bool ShouldShowFor(StatRequest req)
        {
            return req.HasThing && !IsDisabledFor(req.Thing);
        }

        #region Overrides of StatWorker

        public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
        {
            return "";

            //StringBuilder stringBuilder = new StringBuilder();
            //if (this.stat.parts != null)
            //{
            //    for (int index = 0; index < this.stat.parts.Count; ++index)
            //    {
            //        string str = this.stat.parts[index].ExplanationPart(req);
            //        if (!str.NullOrEmpty())
            //        {
            //            stringBuilder.AppendLine(str);
            //            stringBuilder.AppendLine();
            //        }
            //    }
            //}
            //if (this.stat.postProcessCurve != null)
            //{
            //    float num1 = this.GetValue(req, false);
            //    float num2 = this.GetValue(req, true);
            //    if (!Mathf.Approximately(num1, num2))
            //    {
            //        string str1 = this.ValueToString(num1, false, ToStringNumberSense.Absolute);
            //        string str2 = this.stat.ValueToString(num2, numberSense);
            //        stringBuilder.AppendLine("StatsReport_PostProcessed".Translate() + ": " + str1 + " => " + str2);
            //        stringBuilder.AppendLine();
            //    }
            //}
            //float statFactor = Find.Scenario.GetStatFactor(this.stat);
            //if ((double) statFactor != 1.0)
            //{
            //    stringBuilder.AppendLine("StatsReport_ScenarioFactor".Translate() + ": " + statFactor.ToStringPercent());
            //    stringBuilder.AppendLine();
            //}
            //stringBuilder.Append("StatsReport_FinalValue".Translate() + ": " + this.stat.ValueToString(finalVal, this.stat.toStringNumberSense));
            //return stringBuilder.ToString();
        }

        #endregion
    }
}