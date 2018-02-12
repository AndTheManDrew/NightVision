using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace NightVision
{
    public class CompProperties_NightVision : CompProperties
    {
        public Pawn parentPawnInt;
        public int numOfEyesWithNightVision = 0;
        public int numOfEyesWithPhotoSensitivity = 0;

        public CompProperties_NightVision()
        {
            compClass = typeof(Comp_NightVision);
        }
    }
}
