// Nightvision NightVision Harmony_Patches.cs
// 
// 30 03 2018
// 
// 21 07 2018

using System;
using System.Collections.Generic;
#if DEBUG
using System.Diagnostics;
#endif
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MissingAnnotation
// ReSharper disable RegionWithSingleElement

// ReSharper disable InconsistentNaming

namespace NightVision
{
    [StaticConstructorOnStartup]
    public static class NVHarmonyPatcher
    {
        #region Harmony Patcher

        static NVHarmonyPatcher()
        {
#if DEBUG
                        HarmonyInstance.DEBUG = true;
#endif
            HarmonyInstance harmony = HarmonyInstance.Create("drumad.rimworld.nightvision");

            // Targets
            //StatPart_Glow - The bits that actually affect move speed and work speed
            MethodInfo StatPart_Glow_FactorFromGlow =
                        AccessTools.Method(typeof(StatPart_Glow), "FactorFromGlow");

            MethodInfo StatPart_Glow_ExplanationPart = AccessTools.Method(
                                                                          typeof(StatPart_Glow),
                                                                          nameof(StatPart_Glow.ExplanationPart)
                                                                         );

            MethodInfo StatPart_Glow_ActiveFor = AccessTools.Method(typeof(StatPart_Glow), "ActiveFor");

            //HediffDef - so we can display the potential NV effect on the uninstalled bionic items
            MethodInfo HediffDef_SpecialDisplayStats =
                        AccessTools.Method(typeof(HediffDef), nameof(HediffDef.SpecialDisplayStats));

            //Hediff - to track hediffs
            MethodInfo Hediff_PostAdd     = AccessTools.Method(typeof(Hediff), nameof(Hediff.PostAdd));
            MethodInfo Hediff_PostRemoved = AccessTools.Method(typeof(Hediff), nameof(Hediff.PostRemoved));

            //HediffWithComps - because this class, derived from Hediff, doesn't use base.PostAdd
            MethodInfo HediffWithComps_PostAdd =
                        AccessTools.Method(typeof(HediffWithComps), nameof(HediffWithComps.PostAdd));
            // but it does use base.PostRemove
            //MethodInfo HediffWithComps_PostRemoved = AccessTools.Method(typeof(HediffWithComps), nameof(HediffWithComps.PostRemoved));

            //HealthTracker
            MethodInfo HealthTracker_AddHediff = AccessTools.Method(
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
            //MethodInfo HealthTracker_RemoveHediff = AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.RemoveHediff));

            //ApparelTracker
            MethodInfo ApparelTracker_Wear = AccessTools.Method(
                                                                typeof(Pawn_ApparelTracker),
                                                                nameof(Pawn_ApparelTracker.Wear)
                                                               );

            MethodInfo ApparelTracker_TryDrop = AccessTools.Method(
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

            MethodInfo ApparelTracker_Remove = AccessTools.Method(
                                                                  typeof(Pawn_ApparelTracker),
                                                                  nameof(Pawn_ApparelTracker.Remove)
                                                                 );

            MethodInfo ApparelTracker_TakeWearoutDamageForDay =
                        AccessTools.Method(typeof(Pawn_ApparelTracker), "TakeWearoutDamageForDay");

            //ThoughtWorker_Dark
            MethodInfo ThoughtWorker_Dark_CurrentStateInternal =
                        AccessTools.Method(typeof(ThoughtWorker_Dark), "CurrentStateInternal");


            // PawnRecentMemory
            MethodInfo PawnRecentMemory_RecentMemory = AccessTools.Method(
                                                                          typeof(PawnRecentMemory),
                                                                          nameof(PawnRecentMemory.RecentMemoryInterval)
                                                                         );

            //Pawn
            MethodInfo Pawn_getBodySize =
                        AccessTools.Property(typeof(Pawn), nameof(Pawn.BodySize)).GetGetMethod();

            //Patches
            Type thistype = typeof(NVHarmonyPatcher);
            //StatPart_Glow

            harmony.Patch(
                          StatPart_Glow_FactorFromGlow,
                          null,
                          new HarmonyMethod(thistype, nameof(NVHarmonyPatcher.FactorFromGlow_PostFix))
                         );

            harmony.Patch(
                          StatPart_Glow_ExplanationPart,
                          null,
                          new HarmonyMethod(thistype, nameof(NVHarmonyPatcher.ExplanationPart_PostFix))
                         );

            harmony.Patch(
                          StatPart_Glow_ActiveFor,
                          null,
                          new HarmonyMethod(thistype, nameof(NVHarmonyPatcher.ActiveFor_Postfix))
                         );

            //Hediff
            harmony.Patch(
                          Hediff_PostAdd,
                          null,
                          new HarmonyMethod(thistype, nameof(NVHarmonyPatcher.Hediff_PostAdd_Postfix))
                         );

            harmony.Patch(
                          Hediff_PostRemoved,
                          null,
                          new HarmonyMethod(thistype, nameof(NVHarmonyPatcher.Hediff_PostRemoved_Postfix))
                         );

            //HediffWithComps
            harmony.Patch(
                          HediffWithComps_PostAdd,
                          null,
                          new HarmonyMethod(thistype, nameof(NVHarmonyPatcher.HediffWithComps_PostAdd_Postfix))
                         );

            //HediffDef
            harmony.Patch(
                          HediffDef_SpecialDisplayStats,
                          null,
                          new HarmonyMethod(thistype, nameof(NVHarmonyPatcher.SpecialDisplayStats_Postfix))
                         );

            //HealthTracker
            harmony.Patch(
                          HealthTracker_AddHediff,
                          null,
                          new HarmonyMethod(thistype, nameof(NVHarmonyPatcher.AddHediff_Postfix))
                         );

            //ApparelTracker
            harmony.Patch(
                          ApparelTracker_Wear,
                          null,
                          new HarmonyMethod(thistype, nameof(NVHarmonyPatcher.Wear_Postfix))
                         );

            harmony.Patch(
                          ApparelTracker_TryDrop,
                          null,
                          new HarmonyMethod(thistype, nameof(NVHarmonyPatcher.TryDrop_Postfix))
                         );

            harmony.Patch(
                          ApparelTracker_Remove,
                          null,
                          new HarmonyMethod(thistype, nameof(NVHarmonyPatcher.Remove_Postfix))
                         );

            harmony.Patch(
                          ApparelTracker_TakeWearoutDamageForDay,
                          null,
                          new HarmonyMethod(thistype, nameof(NVHarmonyPatcher.TakeWearoutDamageForTheDay_Postfix))
                         );

            //ThoughtWorker_Dark
            harmony.Patch(
                          ThoughtWorker_Dark_CurrentStateInternal,
                          null,
                          new HarmonyMethod(thistype, nameof(NVHarmonyPatcher.CurrentStateInternal_Postfix))
                         );

            ////PawnRecentMemory
            harmony.Patch(
                          PawnRecentMemory_RecentMemory,
                          null,
                          null,
                          new HarmonyMethod(thistype, nameof(NVHarmonyPatcher.RecentMemory_Transpiler))
                         );

            //Pawn
            harmony.Patch(
                          Pawn_getBodySize,
                          null,
                          new HarmonyMethod(typeof(NVHarmonyPatcher), nameof(NVHarmonyPatcher.GetBodySize_Patch))
                         );


            ////Combat Extended Patch
            //                try
            //{
            //    PatchesForCE.ApplyCombatExtendedPatch(ref harmony);
            //}
            //catch (TypeLoadException) { }
#if DEBUG
                        HarmonyInstance.DEBUG = false;

#endif
        }

        #endregion

        #region HediffWithComps Patches

        public static void HediffWithComps_PostAdd_Postfix(
                        HediffWithComps __instance
                    )
        {
            if (__instance.pawn is Pawn pawn
                && pawn.Spawned /*&& pawn.RaceProps.Humanlike*/
                && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp
                        )
            {
#if DEBUG
                                Log.Message("NightVision.NVHarmonyPatcher.HediffWithComps_PostAdd_Postfix: " + pawn
                                                                                                             + ", "
                                                                                                             + __instance
                                                                                                                         .def);
#endif
                comp.CheckAndAddHediff(__instance, __instance.Part);
            }
        }

        #endregion

        #region HediffDef Patch

        /*
         *public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
           {
           if (this.stages != null && this.stages.Count == 1)
           {
           foreach (StatDrawEntry de in this.stages[0].SpecialDisplayStats())
           {
           yield return de;
           }
           }
           yield break;
           }
         */
        public static IEnumerable<StatDrawEntry> SpecialDisplayStats_Postfix(
                        IEnumerable<StatDrawEntry> statdrawentries,
                        HediffDef                  __instance
                    )
        {
            List<StatDrawEntry> statDrawEntryList = statdrawentries.ToList();

            foreach (StatDrawEntry sDE in statDrawEntryList)
            {
                yield return sDE;
            }

            if (Storage.HediffLightMods.TryGetValue(__instance, out Hediff_LightModifiers hlm)
                && hlm.HasModifier())
            {
                VisionType vt = hlm.Setting;

                if (vt < VisionType.NVCustom /*TODO review: && vt != VisionType.NVNone*/)
                {
                    yield return new StatDrawEntry(
                                                   StatCategoryDefOf.Basics,
                                                   "NVGrantsVisionType".Translate(),
                                                   vt.ToString().Translate(),
                                                   0,
                                                   hlm.AffectsEye ? "NVHediffQualifier".Translate() : ""
                                                  );
                }
                else
                {
                    //TODO Condense? need to review the code for the stats
                    yield return new StatDrawEntry(
                                                   StatCategoryDefOf.Basics,
                                                   "NVGrantsVisionType".Translate(),
                                                   vt.ToString(),
                                                   0,
                                                   hlm.AffectsEye ? "NVHediffQualifier".Translate() : ""
                                                  );

                    yield return new StatDrawEntry(
                                                   StatCategoryDefOf.Basics,
                                                   NightVisionStatDefOf.NightVision,
                                                   hlm[0],
                                                   StatRequest.ForEmpty(),
                                                   ToStringNumberSense.Offset
                                                  );

                    yield return new StatDrawEntry(
                                                   StatCategoryDefOf.Basics,
                                                   NightVisionStatDefOf.LightSensitivity,
                                                   hlm[1],
                                                   StatRequest.ForEmpty(),
                                                   ToStringNumberSense.Offset
                                                  );
                }
            }
        }

        #endregion

        #region Pawn_HealthTracker Patches

        //Don't think this is necessary anymore TODO review
        //public void AddHediff(Hediff hediff, BodyPartRecord part = null, DamageInfo? dinfo = null)  
        public static void AddHediff_Postfix(
                        Hediff             hediff,
                        BodyPartRecord     part,
                        Pawn_HealthTracker __instance
                    )
        {
            var pawn = Traverse.Create(__instance).Field("pawn")?.GetValue<Pawn>();

            if (pawn != null
                && pawn.Spawned
                && /*part != null && hediff is Hediff_MissingPart &&*/ /*pawn.RaceProps.Humanlike &&*/
                pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
#if DEBUG
                                Log.Message("NightVision.NVHarmonyPatcher.AddHediff_Postfix: " + pawn + ", "
                                            + hediff.def);

#endif
                comp.CheckAndAddHediff(hediff, part);
            }
        }
        // RemoveHediff calls hediff.PostRemoved - which we patch 
        //public void RemoveHediff(Hediff hediff)

        #endregion

        #region Pawn Patch

        //TODO - generalise this
        public static void GetBodySize_Patch(
                        ref Pawn  __instance,
                        ref float __result
                    )
        {
            var modextension = __instance?.def?.GetModExtension<Stealth_ModExtension>();

            if (modextension == null || !__instance.Spawned)
            {
                return;
            }

            if (__instance.Map.glowGrid.GameGlowAt(__instance.Position) > 0.3f)
            {
                return;
            }

            __result *= Mathf.Lerp(
                                   modextension.lowlightbodysizefactor,
                                   1,
                                   __instance.Map.glowGrid.GameGlowAt(__instance.Position) / 0.3f
                                  );
        }

        #endregion

        #region PawnRecentMemory Transpiler

        //Totally unnecessary overkill...but its my first, so wooop
        public static IEnumerable<CodeInstruction> RecentMemory_Transpiler(
                        IEnumerable<CodeInstruction> instructions,
                        ILGenerator                  il
                    )
        {
            List<CodeInstruction> codes = instructions.ToList();

            var inserts = new List<CodeInstruction>
                          {
                              new CodeInstruction(OpCodes.Ldarg_0),
                              new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRecentMemory), "pawn")),
                              new CodeInstruction(
                                                  OpCodes.Call,
                                                  AccessTools.Method(
                                                                     typeof(ThoughtWorker_TooBright),
                                                                     nameof(ThoughtWorker_TooBright.SetLastDarkTick)
                                                                    )
                                                 )
                          };

            int jumpToInsertsIndex = -1, jumpPastInsertsIndex = -1;

            for (var i = 2; i < codes.Count - 2; i++)
            {
                if (codes[i].opcode        == OpCodes.Brfalse
                    && codes[i - 1].opcode == OpCodes.Callvirt
                    && codes[i - 2].opcode == OpCodes.Callvirt)
                {
                    jumpToInsertsIndex = i;
                }
                else if (codes[i].opcode == OpCodes.Stfld && codes[i + 1].opcode != OpCodes.Ret)
                {
                    jumpPastInsertsIndex = i + 1;
                }

                if (jumpToInsertsIndex > 0 && jumpPastInsertsIndex > 0)
                {
                    break;
                }
            }

            //Add a new branch after end of org if clause that jumps past our code
            inserts.Insert(0, new CodeInstruction(OpCodes.Br, codes[jumpToInsertsIndex].operand));
            var   inserted      = false;
            var   relabeled     = false;
            Label landInInserts = il.DefineLabel();
            //landing point
            inserts[1].labels.Add(landInInserts);

            foreach (CodeInstruction code in codes)
            {
                if (inserted == false && codes.IndexOf(code) == jumpPastInsertsIndex)
                {
                    foreach (CodeInstruction insert in inserts)
                    {
                        yield return insert;
                    }

                    inserted = true;
                }
                else if (relabeled == false && code == codes[jumpToInsertsIndex])
                {
                    code.operand = landInInserts;
                    relabeled    = true;
                }

                yield return code;
            }
        }

        #endregion

        #region StatPart_Glow Patches

        ////The bits that actually effect the gameplay
#if DEBUG
                public static           double    TotalGlFactorNanoSec;
                public static           long      TotalTicks;
                private static readonly Stopwatch GlfactorTimer = new Stopwatch();
                private static          int       GlfactorTicks;
#endif
        public static void FactorFromGlow_PostFix(
                        Thing     t,
                        ref float __result
                    )
        {
#if DEBUG
                        GlfactorTimer.Start();
#endif
            if (t is Pawn pawn && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                float glowat = pawn.Map.glowGrid.GameGlowAt(pawn.Position);
                __result = comp.FactorFromGlow(glowat);
            }

#if DEBUG
                        GlfactorTimer.Stop();
                        if (Find.TickManager.TicksGame - GlfactorTicks > 600)
                            {
                                int elapsedTicks = Find.TickManager.TicksGame - GlfactorTicks;
                                GlfactorTicks = Find.TickManager.TicksGame;
                                TotalGlFactorNanoSec += GlfactorTimer.ElapsedMilliseconds * 1000000;
                                TotalTicks += elapsedTicks;
                                GlfactorTimer.Reset();
                            }
#endif
        }

        public static void ExplanationPart_PostFix(
                        ref StatRequest req,
                        ref string      __result
                    )
        {
            if (!__result.NullOrEmpty()
                && req.Thing is Pawn pawn
                && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                float glowat = pawn.Map.glowGrid.GameGlowAt(pawn.Position);

                if (glowat < 0.3f || glowat > 0.7f)
                {
                    __result = "StatsReport_LightMultiplier".Translate(glowat.ToStringPercent())
                               + ":";

                    __result = comp.ExplanationBuilder(__result, glowat, out bool _, true);
                }
            }
        }

        public static void ActiveFor_Postfix(
                        ref Thing t,
                        ref bool  __result
                    )
        {
            if (__result || !t.Spawned) { }
            else
            {
                if (t is Pawn pawn && pawn.TryGetComp<Comp_NightVision>() != null)
                {
                    __result = true;
                }
            }
        }

        #endregion

        #region Hediff Patches

        //public virtual void PostAdd(DamageInfo? dinfo)
        public static void Hediff_PostAdd_Postfix(
                        Hediff __instance
                    )
        {
            if (__instance?.pawn is Pawn pawn
                && pawn.Spawned /*&& pawn.RaceProps.Humanlike*/
                && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision
                            comp)
            {
#if DEBUG
                                Log.Message("NightVision.NVHarmonyPatcher.Hediff_PostAdd_Postfix: " + pawn + ", "
                                            + __instance.def);

#endif
                comp.CheckAndAddHediff(__instance, __instance.Part);
            }
        }

        //public virtual void PostRemoved()
        //
        public static void Hediff_PostRemoved_Postfix(
                        Hediff __instance
                    )
        {
            if (__instance?.pawn is Pawn pawn
                && pawn.Spawned /*&& pawn.RaceProps.Humanlike*/
                && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision
                            comp)
            {
#if DEBUG
                                Log.Message("NightVision.NVHarmonyPatcher.Hediff_PostRemoved_Postfix: " + pawn + ", "
                                            + __instance.def);

#endif
                comp.RemoveHediff(__instance, __instance.Part);
            }
        }

        #endregion

        #region Pawn_ApparelTracker Patches

        //public void Wear(Apparel newApparel, bool dropReplacedApparel = true)
        //public bool TryDrop(Apparel ap, out Apparel resultingAp, IntVec3 pos, bool forbid = true)
        //public void Remove(Apparel ap)
        //private void TakeWearoutDamageForDay(Thing ap)
        public static void Wear_Postfix(
                        Apparel             newApparel,
                        Pawn_ApparelTracker __instance
                    )
        {
            if (__instance?.pawn is Pawn pawn && pawn.Spawned && pawn.RaceProps.Humanlike)
            {
                if (pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
                {
                    comp.CheckAndAddApparel(newApparel);
                }
            }
        }

        public static void TryDrop_Postfix(
                        Apparel             ap,
                        Pawn_ApparelTracker __instance,
                        ref bool            __result
                    )
        {
            if (__result
                && __instance?.pawn is Pawn pawn
                && pawn.Spawned
                && pawn.RaceProps.Humanlike
                && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                comp.RemoveApparel(ap);
            }
        }

        public static void Remove_Postfix(
                        Apparel             ap,
                        Pawn_ApparelTracker __instance
                    )
        {
            if (__instance?.pawn is Pawn pawn
                && pawn.RaceProps.Humanlike
                && pawn.Spawned
                && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                comp.RemoveApparel(ap);
            }
        }

        public static void TakeWearoutDamageForTheDay_Postfix(
                        Thing               ap,
                        Pawn_ApparelTracker __instance
                    )
        {
            if (ap.Destroyed
                && __instance.pawn is Pawn pawn
                && pawn.RaceProps.Humanlike
                && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                if (ap is Apparel apparel)
                {
                    comp.RemoveApparel(apparel);
                }
            }
        }

        #endregion

        #region ThoughtWorker_Dark Patches

        private const int PhotosensDarkThoughtStage = 1;

        public static void CurrentStateInternal_Postfix(
                        Pawn             p,
                        ref ThoughtState __result
                    )
        {
            if (__result.Active)
            {
                if (p.GetComp<Comp_NightVision>() is Comp_NightVision comp)
                {
                    switch (comp.PsychDark)
                    {
                        default: return;
                        case VisionType.NVNightVision:
                            __result = ThoughtState.Inactive;

                            return;
                        case VisionType.NVPhotosensitivity:

                            __result = ThoughtState.ActiveAtStage(
                                                                  NVHarmonyPatcher.PhotosensDarkThoughtStage,
                                                                  VisionType.NVPhotosensitivity.ToString().Translate()
                                                                 );

                            return;
                    }
                }
            }
        }

        #endregion
    }
}