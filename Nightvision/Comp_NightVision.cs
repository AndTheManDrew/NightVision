using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace NightVision
{
    class Comp_NightVision : ThingComp
    {
        private Pawn parentPawnInt;

        public int numOfEyesWithNightVision;
        public int numOfEyesWithPhotoSensitivity;

        public CompProperties_NightVision Props => (CompProperties_NightVision)props;

        private Pawn ParentPawn
        {
            get
            {
                if (parentPawnInt == null)
                {
                    parentPawnInt = parent as Pawn;
                }
                return parentPawnInt;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            UpdateNightVisionAndPhotosensitivity();
        }

        public override void CompTickRare()
        {
            UpdateNightVisionAndPhotosensitivity();
        }

        public void UpdateNightVisionAndPhotosensitivity()
        {
            int num_NV_eye = 0;
            int num_PS_eye = 0;
            parentPawnInt.health.hediffSet.hediffs.ForEach
                (hd =>
                {
                    if (NightVisionMod.Instance.listofNightVisionHediffDefs.Contains(hd.def)) { num_NV_eye++; }
                    else if (NightVisionMod.Instance.listofPhotoSensitiveHediffDefs.Contains(hd.def)) { num_PS_eye++; }
                    return;
                });

            this.numOfEyesWithNightVision = num_NV_eye;
            this.numOfEyesWithPhotoSensitivity = num_PS_eye;
        }

    }
}
