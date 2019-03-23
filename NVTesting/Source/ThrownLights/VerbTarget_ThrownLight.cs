using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace NVTesting.ThrownLights
{
    class VerbTarget_ThrownLight : Command_VerbTarget
    {
        public Comp_ChangeableProjectile_Thrown comp;

        private static TargetingParameters tParams = new TargetingParameters()
                                                     {
                                                         canTargetBuildings = false, canTargetPawns = false, mapObjectTargetsMustBeAutoAttackable = false, canTargetLocations = true, neverTargetDoors = true
                                                     };

        public override void GizmoUpdateOnMouseover()
        {
            base.GizmoUpdateOnMouseover();
        }

        public override void MergeWith(Gizmo other)
        {
            base.MergeWith(other);
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
        {
            if (comp == null)
            {
                return base.GizmoOnGUI(topLeft, maxWidth);
            }
            GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth);
            Text.Font = GameFont.Tiny;
            Rect rect = new Rect(topLeft.x  + 52f, topLeft.y +5f, this.GetWidth(maxWidth) - 10f, 18f);
            Widgets.Label(rect, $"{comp.ammo}/{comp.Props.maxAmmo}");

            return result;
        }

        public override void ProcessInput(Event ev)
        {

            
            if (this.CurActivateSound != null)
            {
                this.CurActivateSound.PlayOneShotOnCamera(null);
            }
            
            // Mostly a copy of Command_VerbTarget.ProcessInput
            SoundDefOf.Tick_Tiny.PlayOneShotOnCamera(null);
            Targeter targeter = Find.Targeter;
            
            //TODO remove merging
            if (verb.CasterIsPawn && targeter.targetingVerb != null && targeter.targetingVerb.verbProps == this.verb.verbProps)
            {
                Log.Message($"Ba");
                
            }
            else
            {
                // Find.Targeter.BeginTargeting(this.verb); - TODO any reason RW doesn't use targeter?


                Find.Targeter.BeginTargeting(tParams, OrderPawnThrowLight, verb.CasterPawn);

            }
        }

        public void OrderPawnThrowLight(LocalTargetInfo localTargetInfo)
        {
            Job    job = new Job(DefOfs.ThrowLight);
            job.verbToUse             = verb;
            job.targetA               = localTargetInfo;
            job.maxNumStaticAttacks = 1;
            job.endIfCantShootTargetFromCurPos = true;
            verb.CasterPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }
    }
}
