using System;
using JetBrains.Annotations;
using NightVision.LightModifiers;
using Verse;

namespace NightVision.Comps
{
    /// <summary>
    /// More comps and props and comps and props and comps and props
    /// </summary>
    public class HediffComp_NightVision : HediffComp
    {
        //TODO: Consider moving all hediff harmony patches here
        //internal bool AffectsEyes = false;
        [UsedImplicitly]
        public HediffCompProperties_NightVision Props => (HediffCompProperties_NightVision) props;


        public override string CompTipStringExtra => TipString();

        private string TipString()
            {
                switch (Props.LightModifiers.Setting)
                    {
                        //TODO Review returning empty & expand explaination
                        case LightModifiersBase.Options.NVNightVision: return "NVNightVision".Translate();
                        case LightModifiersBase.Options.NVPhotosensitivity: return "NVPhotosensitivity".Translate();
                        case LightModifiersBase.Options.NVCustom: return "NVZeroLabel".Translate() + $" = {Props.LightModifiers[0]:+#;-#;0}%" + " | " + "NVFullLabel".Translate()+ $" = {Props.LightModifiers[1]:+#;-#;0}%";
                        default: return String.Empty;
                    }
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

