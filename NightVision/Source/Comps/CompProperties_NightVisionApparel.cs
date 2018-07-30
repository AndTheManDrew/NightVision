using JetBrains.Annotations;
using Verse;

namespace NightVision
    {
        public class CompProperties_NightVisionApparel : CompProperties
            {
                public bool GrantsNightVision         = false;
                public bool NullifiesPhotosensitivity = false;
                public ApparelVisionSetting AppVisionSetting;
                [UsedImplicitly]
                public CompProperties_NightVisionApparel() => compClass = typeof(Comp_NightVisionApparel);
            }
    }