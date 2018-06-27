using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using CombatExtended;
using Harmony;
using NightVision.Comps;
using NightVision.LightModifiers;
using RimWorld;
using Verse;

// ReSharper disable InconsistentNaming

namespace NightVision
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        #region Applying Harmony Patches
        static HarmonyPatches()
        {
            var harmony = HarmonyInstance.Create("nightvision.rimworld.mod");

        // Targets
            //StatPart_Glow
            MethodInfo StatPart_Glow_FactorFromGlow = AccessTools.Method(typeof(StatPart_Glow), "FactorFromGlow");
            MethodInfo StatPart_Glow_ExplanationPart = AccessTools.Method(typeof(StatPart_Glow), nameof(StatPart_Glow.ExplanationPart));

            //Hediff
            MethodInfo Hediff_PostAdd = AccessTools.Method(typeof(Hediff), nameof(Hediff.PostAdd));
            MethodInfo Hediff_PostRemoved = AccessTools.Method(typeof(Hediff), nameof(Hediff.PostRemoved));

            //HediffWithComps
            MethodInfo HediffWithComps_PostAdd = AccessTools.Method(typeof(HediffWithComps), nameof(HediffWithComps.PostAdd));
            //MethodInfo HediffWithComps_PostRemoved = AccessTools.Method(typeof(HediffWithComps), nameof(HediffWithComps.PostRemoved));

            //HealthTracker
            MethodInfo HealthTracker_AddHediff = AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.AddHediff), new[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo), typeof(DamageWorker.DamageResult) });
            //MethodInfo HealthTracker_RemoveHediff = AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.RemoveHediff));

            //ApparelTracker
            MethodInfo ApparelTracker_Wear = AccessTools.Method(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Wear));
            MethodInfo ApparelTracker_TryDrop = AccessTools.Method(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.TryDrop), new []{typeof(Apparel), typeof(Apparel).MakeByRefType(), typeof(IntVec3), typeof(bool)});
            MethodInfo ApparelTracker_Remove = AccessTools.Method(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Remove));
            MethodInfo ApparelTracker_TakeWearoutDamageForDay = AccessTools.Method(typeof(Pawn_ApparelTracker), "TakeWearoutDamageForDay");

            //ThoughtWorker_Dark
            MethodInfo ThoughtWorker_Dark_CurrentStateInternal = AccessTools.Method(typeof(ThoughtWorker_Dark), "CurrentStateInternal");

            Log.Message("ApparelTrackerPatchMI: " + ApparelTracker_TryDrop, true);

        //Patches
            Type thistype = typeof(HarmonyPatches);
            //StatPart_Glow
            Log.Message("1");
            harmony.Patch(StatPart_Glow_FactorFromGlow, null, new HarmonyMethod(thistype, nameof(FactorFromGlow_PostFix)));
            Log.Message("2");
            harmony.Patch(StatPart_Glow_ExplanationPart, null, new HarmonyMethod(thistype, nameof(ExplanationPart_PostFix)));
            Log.Message("3");
            //Hediff
            harmony.Patch(Hediff_PostAdd, null, new HarmonyMethod(thistype, nameof(Hediff_PostAdd_Postfix)));
            Log.Message("4");
            harmony.Patch(Hediff_PostRemoved, null, new HarmonyMethod(thistype, nameof(Hediff_PostRemoved_Postfix)));
            Log.Message("5");
            //HediffWithComps
            harmony.Patch(HediffWithComps_PostAdd, null, new HarmonyMethod(thistype, nameof(HediffWithComps_PostAdd_Postfix)));
            Log.Message("6");
            //HealthTracker
            harmony.Patch(HealthTracker_AddHediff, null, new HarmonyMethod(thistype, nameof(AddHediff_Postfix)));
            Log.Message("7");
            //ApparelTracker
            harmony.Patch(ApparelTracker_Wear, null, new HarmonyMethod(thistype, nameof(Wear_Postfix)));
            Log.Message("8");
            harmony.Patch(ApparelTracker_TryDrop, null, new HarmonyMethod(thistype, nameof(TryDrop_Postfix)));
            Log.Message("9");
            harmony.Patch(ApparelTracker_Remove, null, new HarmonyMethod(thistype, nameof(Remove_Postfix)));
            Log.Message("10");
            harmony.Patch(ApparelTracker_TakeWearoutDamageForDay, null, new HarmonyMethod(thistype, nameof(TakeWearoutDamageForTheDay_Postfix)));
            //ThoughtWorker_Dark
            harmony.Patch(ThoughtWorker_Dark_CurrentStateInternal, null, new HarmonyMethod(thistype, nameof(CurrentStateInternal_Postfix)));
            
        //Combat Extended Patch
            try
            {
                ApplyCombatExtendedPatch(ref harmony);
            }
            catch (TypeLoadException)
            {
            }
            
            
        }
        #endregion

        #region StatPart_Glow Patches
        //The bits that actually effect the gameplay
        public static double TotalGlFactorNanoSec;
        public static Int64 TotalTicks;
        private static Stopwatch GlfactorTimer = new Stopwatch();
        private static int GlfactorTicks;
        public static void FactorFromGlow_PostFix(Thing t, ref float __result)
        {
            GlfactorTimer.Start();
            if (t is Pawn pawn /*&& pawn.RaceProps.Humanlike  -- Not necessary? Statpart_glow.ActiveFor() should filter*/ && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                float glowat = pawn.Map.glowGrid.GameGlowAt(pawn.Position);
                __result = comp.FactorFromGlow(glowat);
            }
            //TODO Remove timers for release
            GlfactorTimer.Stop();
            if (Find.TickManager.TicksGame - GlfactorTicks > 600)
            {
                int elapsedTicks = Find.TickManager.TicksGame - GlfactorTicks;
                //Log.Message($"FactorFromGlow_PostFix Timer: {((glfactorTimer.Elapsed.TotalMilliseconds * 1000000) / elapsedTicks):#.0 ns} per tick in the last {elapsedTicks} ticks");
                GlfactorTicks = Find.TickManager.TicksGame;
                TotalGlFactorNanoSec += GlfactorTimer.ElapsedMilliseconds * 1000000;
                TotalTicks += elapsedTicks;
                GlfactorTimer.Reset();

            }
        }
        
        public static void ExplanationPart_PostFix(ref StatRequest req, ref string __result)
        {
            if (!__result.NullOrEmpty() && req.Thing is Pawn pawn && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                float glowat = pawn.Map.glowGrid.GameGlowAt(pawn.Position);
                if (glowat < 0.3f || glowat > 0.7f)
                {
                    __result = comp.ExplanationBuilder(__result, glowat, out bool _);
                }
            }
        }

        #endregion

        #region Hediff Patches
        //public virtual void PostAdd(DamageInfo? dinfo)
        public static void Hediff_PostAdd_Postfix(Hediff __instance)
        {
            if (__instance?.pawn is Pawn pawn && pawn.Spawned /*&& pawn.RaceProps.Humanlike*/ && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                comp.CheckAndAddHediff(__instance, __instance.Part);
            }
        }
        //public virtual void PostRemoved()
        //For hediffs that are removed in Pawn_HealthTracker.HealthTick (removes them directly from list, then calls Hediff.PostRemoved)
        public static void Hediff_PostRemoved_Postfix(Hediff __instance)
        {
            if (__instance.pawn is Pawn pawn && pawn.Spawned /*&& pawn.RaceProps.Humanlike*/ && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                comp.RemoveHediff(__instance, __instance.Part);
            }
        }

        #endregion

        #region HediffWithComps Patches
        public static void HediffWithComps_PostAdd_Postfix(HediffWithComps __instance)
        {
            if (__instance.pawn is Pawn pawn && pawn.Spawned /*&& pawn.RaceProps.Humanlike*/ && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                comp.CheckAndAddHediff(__instance, __instance.Part);
            }
        }
        #endregion

        #region Pawn_HealthTracker Patches

        //public void AddHediff(Hediff hediff, BodyPartRecord part = null, DamageInfo? dinfo = null)  
        public static void AddHediff_Postfix(Hediff hediff, BodyPartRecord part, Pawn_HealthTracker __instance)
        {

            Pawn pawn = Traverse.Create(__instance).Field("pawn")?.GetValue<Pawn>();
            if (pawn != null &&  pawn.Spawned && /*part != null && hediff is Hediff_MissingPart &&*/ /*pawn.RaceProps.Humanlike &&*/ pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                comp.CheckAndAddHediff(hediff, part);
            }
        }
        //public void RemoveHediff(Hediff hediff)
        //public static void RemoveHediff_Postfix(Hediff hediff, Pawn_HealthTracker __instance)
        //{
        //    Pawn pawn = Traverse.Create(__instance).Field("pawn")?.GetValue<Pawn>();
        //    Log.Message("RemoveHediff_Postfix called on: " + pawn?.Label?? " null " + "'s hediff " + hediff.Label + " && part " + hediff.Part?.def.defName ?? " null ");
        //    if (pawn != null && pawn.Spawned && /*part != null && hediff is Hediff_MissingPart &&*/ /*pawn.RaceProps.Humanlike &&*/ pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
        //    {
        //        Log.Message("RemoveHediff_Postfix: Calling RemoveHediff with: " + pawn.NameStringShort + " & " + hediff.def.defName);

        //        comp.RemoveHediff(hediff, hediff.Part);
        //    }
        //}

        #endregion

        #region Pawn_ApparelTracker Patches

        //public void Wear(Apparel newApparel, bool dropReplacedApparel = true)
        //public bool TryDrop(Apparel ap, out Apparel resultingAp, IntVec3 pos, bool forbid = true)
        //public void Remove(Apparel ap)
        //private void TakeWearoutDamageForDay(Thing ap)
        public static void Wear_Postfix(Apparel newApparel, Pawn_ApparelTracker __instance)
        {
            if (__instance?.pawn is Pawn pawn && pawn.Spawned && pawn.RaceProps.Humanlike)
            {
                if (pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
                {
                    comp.CheckAndAddApparel(newApparel);
                }
            }
        }
        public static void TryDrop_Postfix(Apparel ap, Pawn_ApparelTracker __instance, ref bool __result)
        {
            if (__result && __instance?.pawn is Pawn pawn && pawn.Spawned && pawn.RaceProps.Humanlike && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                comp.RemoveApparel(ap);
            }
        }
        public static void Remove_Postfix(Apparel ap, Pawn_ApparelTracker __instance)
        {
            if (__instance?.pawn is Pawn pawn && pawn.RaceProps.Humanlike && pawn.Spawned && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                comp.RemoveApparel(ap);
            }
        }

        public static void TakeWearoutDamageForTheDay_Postfix(Thing ap, Pawn_ApparelTracker __instance)
        {
            if (ap.Destroyed && __instance.pawn is Pawn pawn && pawn.RaceProps.Humanlike && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
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
        public static void CurrentStateInternal_Postfix(Pawn p, ref ThoughtState __result)
        {
            if (__result.Active)
            {
                if (p.GetComp<Comp_NightVision>() is Comp_NightVision comp)
                {
                    switch (comp.PsychDark())
                    {
                        default:
                            return;
                        case LightModifiersBase.Options.NVNightVision:
                            __result = ThoughtState.Inactive;
                            return;
                        case LightModifiersBase.Options.NVPhotosensitivity:
                            __result = ThoughtState.ActiveAtStage(PhotosensDarkThoughtStage, LightModifiersBase.Options.NVPhotosensitivity.ToString().Translate());
                            return;
                    }
                }
            }
        }
        #endregion

        #region Combat Extended Patch

        //Inspired by Pick Up And Haul's implementation, which is more elegant
        /// <summary>
        /// Should only be executed in try-catch(TypeLoadException) block
        /// See: https://stackoverflow.com/questions/3346740/typeloadexception-is-not-caught-by-try-catch
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ApplyCombatExtendedPatch(ref HarmonyInstance harmony)
        {
            //ModMetaData m.Name is defined in the About.xml file of the mod, therefore no need to account for steamID messes
            if (ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Combat Extended"))
            {
                MethodInfo Verb_LaunchProjectileCE_ShiftVecReportFor = AccessTools.Method(typeof(Verb_LaunchProjectileCE), nameof(Verb_LaunchProjectileCE.ShiftVecReportFor));
                harmony.Patch(Verb_LaunchProjectileCE_ShiftVecReportFor, null, new HarmonyMethod(typeof(HarmonyPatches), nameof(ShiftVecReportFor_Postfix)));
                //Checked by mod settings window: if true will display checkbox for NVEnabledForCE
                NightVisionSettings.CEDetected = true;
                Log.Message("Night Vision detected Combat Extended and patched: " + Verb_LaunchProjectileCE_ShiftVecReportFor.Name);
            }
        }

        //Can be changed in the mod settings
        public static bool NVEnabledForCE = true;

        /// <summary>
        /// CE's lighting shift (= 1 - glow) is linear function between points (glow = 1, shift = 0) & (glow = 0, shift = 1)
        /// All this method does is transform the shift function to a linear function between points: 
        ///     (glow = 1, shift = - modifier at 100% light) 
        ///     (glow = 0, shift = 1 - modifier at 0% light)
        /// where, with default settings, 
        ///     modifier at 100%  less or  = 0   , therefore same or worse
        ///     modifier at 0% light greater or = 0, therefore same or better
        /// </summary>
        public static void ShiftVecReportFor_Postfix(ref ShiftVecReport __result, Verb_LaunchProjectileCE instance)
        {
            if (NVEnabledForCE && instance.caster is Pawn pawn && pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                __result.lightingShift = (__result.lightingShift * (1 + comp.FullLightModifier - comp.ZeroLightModifier)) - comp.FullLightModifier;
            }
        }

        #endregion

    }
}
