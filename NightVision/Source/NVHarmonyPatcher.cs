// Nightvision NightVision Harmony_Patches.cs
// 
// 30 03 2018
// 
// 21 07 2018

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using NightVision.Harmony.Manual;
using RimWorld;
using Verse;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MissingAnnotation
// ReSharper disable RegionWithSingleElement
// ReSharper disable All

// ReSharper disable InconsistentNaming

namespace NightVision
{
    [StaticConstructorOnStartup]
    public static class NVHarmonyPatcher
    {

        static NVHarmonyPatcher()
        {
#if DEBUG
                        HarmonyInstance.DEBUG = true;
#endif
            HarmonyInstance harmony = HarmonyInstance.Create("drumad.rimworld.nightvision");
            
            MethodInfo addHediffMethod = AccessTools.Method(
                                                                    typeof(Pawn_HealthTracker),
                                                                    nameof(Pawn_HealthTracker.AddHediff),
                                                                    new[]
                                                                    {
                                                                        typeof(Hediff),
                                                                        typeof(BodyPartRecord),
                                                                        typeof(DamageInfo),
                                                                        typeof(DamageWorker.DamageResult)
                                                                    }
                                                                   );
            
            MethodInfo tryDropMethod = AccessTools.Method(
                                                                   typeof(Pawn_ApparelTracker),
                                                                   nameof(Pawn_ApparelTracker.TryDrop),
                                                                   new[]
                                                                   {
                                                                       typeof(Apparel),
                                                                       typeof(Apparel).MakeByRefType(),
                                                                       typeof(IntVec3),
                                                                       typeof(bool)
                                                                   }
                                                                  );
            
            harmony.Patch(
                addHediffMethod,
                          null,
                          new HarmonyMethod(typeof(PawnHealthTracker_AddHediff), nameof(PawnHealthTracker_AddHediff.AddHediff_Postfix))
                         );
            
            harmony.Patch(
                tryDropMethod,
                          null,
                          new HarmonyMethod(typeof(ApparelTracker_TryDrop), nameof(ApparelTracker_TryDrop.Postfix))
                         );
            

            harmony.PatchAll(Assembly.GetExecutingAssembly());

#if DEBUG
                        HarmonyInstance.DEBUG = false;

#endif
        }
        
    }
}