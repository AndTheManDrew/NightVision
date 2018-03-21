using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace NightVision
{
    class Comp_NightVisionApparel : ThingComp
    {   
        public CompProperties_NightVisionApparel Props => (CompProperties_NightVisionApparel)props;

    }

    public class CompProperties_NightVisionApparel : CompProperties
    {
        public bool nullifiesPhotosensitivity = false;
        public bool grantsNightVision = false;
        public CompProperties_NightVisionApparel()
        {
            compClass = typeof(Comp_NightVisionApparel);
        }
    }
}

