﻿using JetBrains.Annotations;
using Verse;

namespace NightVision.Comps
    {
        public class Comp_NightVisionApparel : ThingComp
            {
                [UsedImplicitly]
                public CompProperties_NightVisionApparel Props => (CompProperties_NightVisionApparel) props;
            }

        public class CompProperties_NightVisionApparel : CompProperties
            {
                public bool GrantsNightVision         = false;
                public bool NullifiesPhotosensitivity = false;

                [UsedImplicitly]
                public CompProperties_NightVisionApparel() => compClass = typeof(Comp_NightVisionApparel);
            }
    }