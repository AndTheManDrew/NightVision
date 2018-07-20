using JetBrains.Annotations;
using NightVision.LightModifiers;
using Verse;

namespace NightVision.Comps
    {
        /// <summary>
        ///     More comps and props and comps and props and comps and props
        /// </summary>
        public class HediffComp_NightVision : HediffComp
            {
                [UsedImplicitly]
                public HediffCompProperties_NightVision Props => (HediffCompProperties_NightVision) props;


                public override string CompTipStringExtra => TipString();

                private string TipString()
                    {
                        switch (Props.LightModifiers.Setting)
                            {
                                //TODO Review returning empty & expand explaination
                                case LightModifiersBase.Options.NVNightVision:      return "NVGiveNV".Translate();
                                case LightModifiersBase.Options.NVPhotosensitivity: return "NVGivePS".Translate();
                                case LightModifiersBase.Options.NVCustom:
                                    return "NVZeroLabel".Translate() + $" = {Props.LightModifiers[0]:+#;-#;0}%" + " | "
                                           + "NVFullLabel".Translate() + $" = {Props.LightModifiers[1]:+#;-#;0}%";
                                default: return string.Empty;
                            }
                    }

                //Need to use harmony patches instead of overriding these methods as the part is not assigned for added or missing parts until
                //after these methods are called ==> Comp_NightVision associating the hediffs with the whole body

                //public override void CompPostMake()
                //{
                //    base.CompPostMake();
                //    if (Pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
                //    {
                //        comp.CheckAndAddHediff(parent, parent.Part);
                //    }
                //}

                //public override void CompPostPostRemoved()
                //{
                //    base.CompPostPostRemoved();
                //    if (Pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
                //        {
                //            comp.CheckAndAddHediff(parent, parent.Part);
                //        }
                //}
                //public override void CompPostMerged(Hediff other)
                //{
                //    base.CompPostMerged(other);
                //}
            }
    }