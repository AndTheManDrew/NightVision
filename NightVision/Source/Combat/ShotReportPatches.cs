using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using RimWorld;
using Verse;
// ReSharper disable InconsistentNaming

namespace NightVision
{
    //ShotReport Patches


    public static class CurrentShot {
        public static float CurrentLightFactor = 1f;
        public static Verb CurrentVerb;
        public static Thing CurrentCaster;

        public static bool NoShot => CurrentVerb == null || CurrentCaster == null;

        public static void ClearLastShot()
        {
            CurrentLightFactor = 1f;
            CurrentVerb = null;
            CurrentCaster = null;
        }
    }

    [HarmonyPatch(typeof(ShotReport), nameof(ShotReport.HitReportFor))]
    public static class HitReportFor_Patches
    {
        [HarmonyPostfix]
        public static void HitReportFor_Postfix(
                        ref ShotReport __result,
                        Thing           caster,
                        Verb            verb,
                        LocalTargetInfo target
                    )
        {
            

            if (caster is Pawn pawn && pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                CurrentShot.CurrentCaster      = caster;
                CurrentShot.CurrentVerb        = verb;
                CurrentShot.CurrentLightFactor = CombatHelpers.ShootGlowFactor(pawn, target, comp);
                //Log.Message($"NightVision.ShotReport_HitReportFor_Patches.HitReportFor_Postfix: Stored data for: {pawn} shooting {target.Thing?.def.defName} with glow factor {CurrentShot.CurrentLightFactor}.");
            }
            else
            {
                CurrentShot.ClearLastShot();
            }
        }
    }

    [HarmonyPatch(typeof(ShotReport), nameof(ShotReport.AimOnTargetChance_StandardTarget))]
    [HarmonyPatch(MethodType.Getter)]
    public static class AimOnTarget_StandardTargetPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
                        ref ShotReport __instance,
                        ref float __result
                    )
        {
            if (!CurrentShot.NoShot)
            {
                __result *= CurrentShot.CurrentLightFactor;
            }
        }
    }
    
}
