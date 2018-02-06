using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Nightvision
{
    class Comp_NightVision : ThingComp
    {
        private Pawn parentPawnInt = null;

        public int numOfEyesWithNightVision = 0;
        public int numOfEyesWithPhotoSensitivity = 0;

        public CompProperties_NightVision Props
        {
            get
            {
                return (CompProperties_NightVision)props;
            }
        }

        public void UpdateNightVision()
        {
            int num_NV_eye = 0;
            string tag = "SightSource";
            foreach (Hediff hediff in parentPawnInt.health.hediffSet.hediffs)
            {
                //Log.Message(hediff.ToString());
                if (hediff is Hediff_AddedPart && hediff.def.addedPartProps.isBionic && hediff.Part.def.tags.Contains(tag))
                {
                    num_NV_eye++;
                }
            }
            this.numOfEyesWithNightVision = num_NV_eye;
        }

        public void UpdatePhotoSensitivity()
        {
            int num_PS_eye = 0;
            string tag = "SightSource";
            foreach (Hediff hediff in parentPawnInt.health.hediffSet.hediffs)
            {
                //Log.Message(hediff.ToString());
                num_PS_eye = PhotoSensitivityEyeCounter(num_PS_eye, tag, hediff);
            }
            this.numOfEyesWithPhotoSensitivity = num_PS_eye;
        }

        private static int PhotoSensitivityEyeCounter(int num_PS_eye, string tag, Hediff hediff)
        {
            if (hediff is Hediff_AddedPart && hediff.def.HasComp(typeof(Comp_GrantsPhotosensitivity)) && hediff.Part.def.tags.Contains(tag))
            {
                num_PS_eye++;
            }

            return num_PS_eye;
        }
    }
}
