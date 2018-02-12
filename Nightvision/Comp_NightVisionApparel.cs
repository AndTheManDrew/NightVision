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

        //private bool isInitialized;
        //public bool nullifiesPhotoSensitivity;
        //public bool grantsNightVision;

        //public void Initialize()
        //{
        //    if (CompProperties_NightVisionApparel = null)
        //    if (!isInitialized)
        //    {
        //        isInitialized = true;

        //    }
        //}
    }

    public class CompProperties_NightVisionApparel : CompProperties
    {
        public bool nullifiesPhotoSensitivity = false;
        public bool grantsNightVision = false;
        public CompProperties_NightVisionApparel()
        {
            compClass = typeof(Comp_NightVisionApparel);
        }
    }
}

