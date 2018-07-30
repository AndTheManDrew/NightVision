// Nightvision NightVision Comp_NightVisionApparel.cs
// 
// 16 05 2018
// 
// 21 07 2018

using System.Text;
using JetBrains.Annotations;
using Verse;

namespace NightVision
    {
        public class Comp_NightVisionApparel : ThingComp
            {

                [UsedImplicitly]
                public CompProperties_NightVisionApparel Props => (CompProperties_NightVisionApparel) props;

        public override string GetDescriptionPart()
        {
            StringBuilder result = new StringBuilder(base.GetDescriptionPart());
            if (this.Props?.AppVisionSetting?.GrantsNV == true)
                {
                    result.AppendLine("NVGiveNV".Translate());
                }
            if (Props?.AppVisionSetting?.NullifiesPS == true)
                {
                    result.AppendLine("NVNullPS".Translate());
                }

            return result.ToString().Trim();
        }

        public override string CompInspectStringExtra()
            {
                StringBuilder result = new StringBuilder(base.CompInspectStringExtra());
                if (this.Props?.AppVisionSetting?.GrantsNV == true)
                {
                    result.AppendLine("NVGiveNV".Translate());
                }
                if (Props?.AppVisionSetting?.NullifiesPS == true)
                    {
                        result.AppendLine("NVNullPS".Translate());
                    }
            return result.ToString().Trim();
        }
    }
    }