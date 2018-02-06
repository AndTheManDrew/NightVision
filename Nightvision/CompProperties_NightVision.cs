using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Nightvision
{
    public class CompProperties_NightVision : CompProperties
    {
        public bool seeindark = false;
        public bool photosensitive = false;
        public bool shadedeyes = false;

        public CompProperties_NightVision()
        {
            compClass = typeof(Comp_NightVision);
        }
    }
}
