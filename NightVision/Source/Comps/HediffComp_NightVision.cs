// Nightvision NightVision HediffComp_NightVision.cs
// 
// 16 05 2018
// 
// 21 07 2018

using JetBrains.Annotations;
using Verse;

namespace NightVision
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
                                case VisionType.NVNightVision:      return "NVGiveNV".Translate();
                                case VisionType.NVPhotosensitivity: return "NVGivePS".Translate();
                                case VisionType.NVCustom:
                                    return "NVZeroLabel".Translate() + $" = {Props.LightModifiers[0]:+#;-#;0}%" + " | "
                                           + "NVFullLabel".Translate() + $" = {Props.LightModifiers[1]:+#;-#;0}%";
                                default: return string.Empty;
                            }
                    }
            }
    }