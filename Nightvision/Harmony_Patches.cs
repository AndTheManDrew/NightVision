using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using Verse;
using Harmony;
using System.Diagnostics;

namespace NightVision
{
    //TODO Add an injector?? Doesn't seem to need it

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        public const string eyeTag = "SightSource";

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
            MethodInfo HealthTracker_AddHediff = AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.AddHediff), new Type[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo) });
            //MethodInfo HealthTracker_RemoveHediff = AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.RemoveHediff));

            //ApparelTracker
            MethodInfo ApparelTracker_Wear = AccessTools.Method(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Wear));
            MethodInfo ApparelTracker_TryDrop = typeof(Pawn_ApparelTracker).GetMethods(AccessTools.all).Where(methodInfo => methodInfo.Name == "TryDrop" && methodInfo.GetParameters().Where(pi => pi.ParameterType == typeof(IntVec3)).Any()).First();
            MethodInfo ApparelTracker_Remove = AccessTools.Method(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Remove));
            MethodInfo ApparelTracker_TakeWearoutDamageForDay = AccessTools.Method(typeof(Pawn_ApparelTracker), "TakeWearoutDamageForDay");

            //ThoughtWorker_Dark
            MethodInfo ThoughtWorker_Dark_CurrentStateInternal = AccessTools.Method(typeof(ThoughtWorker_Dark), "CurrentStateInternal");


        //Patches
            Type thistype = typeof(HarmonyPatches);
            //StatPart_Glow
            harmony.Patch(StatPart_Glow_FactorFromGlow, null, new HarmonyMethod(thistype, nameof(FactorFromGlow_PostFix)));
            harmony.Patch(StatPart_Glow_ExplanationPart, null, new HarmonyMethod(thistype, nameof(ExplanationPart_PostFix)));
            //Hediff
            harmony.Patch(Hediff_PostAdd, null, new HarmonyMethod(thistype, nameof(HediffWithComps_PostAdd_Postfix)));
            harmony.Patch(Hediff_PostRemoved, null, new HarmonyMethod(thistype, nameof(Hediff_PostRemoved_Postfix)));
            //HediffWithComps
            harmony.Patch(HediffWithComps_PostAdd, null, new HarmonyMethod(thistype, nameof(HediffWithComps_PostAdd_Postfix)));
            //HealthTracker
            harmony.Patch(HealthTracker_AddHediff, null, new HarmonyMethod(thistype, nameof(AddHediff_Postfix)));
            //ApparelTracker
            harmony.Patch(ApparelTracker_Wear, null, new HarmonyMethod(thistype, nameof(Wear_Postfix)));
            harmony.Patch(ApparelTracker_TryDrop, null, new HarmonyMethod(thistype, nameof(TryDrop_Postfix)));
            harmony.Patch(ApparelTracker_Remove, null, new HarmonyMethod(thistype, nameof(Remove_Postfix)));
            harmony.Patch(ApparelTracker_TakeWearoutDamageForDay, null, new HarmonyMethod(thistype, nameof(TakeWearoutDamageForTheDay_Postfix)));
            //ThoughtWorker_Dark
            harmony.Patch(ThoughtWorker_Dark_CurrentStateInternal, null, new HarmonyMethod(thistype, nameof(CurrentStateInternal_Postfix)));

            var meths = harmony.GetPatchedMethods();
            Log.Message(meths.Count().ToString());
            foreach (MethodInfo mi in meths)
            {
                Log.Message("Night Vision patched: " + mi.Name);
            }
            
        }
        #endregion

        #region StatPart_Glow Patches
        //The bits that actually effect the game
        public static double TotalGlFactorNanoSec = 0f;
        public static Int64 TotalTicks = 0;
        public static Stopwatch glfactorTimer = new Stopwatch();
        public static int glfactorTicks = 0;
        public static void FactorFromGlow_PostFix(Thing t, ref float __result)
        {
            glfactorTimer.Start();
            if (t is Pawn pawn /*&& pawn.RaceProps.Humanlike  -- Not necessary? Statpart_glow.ActiveFor() should filter*/ && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                float glowat = pawn.Map.glowGrid.GameGlowAt(pawn.Position, false);
                __result = comp.FactorFromGlow(glowat);
            }
            //TODO Remove timers for release
            glfactorTimer.Stop();
            if (Find.TickManager.TicksGame - glfactorTicks > 600)
            {
                int elapsedTicks = Find.TickManager.TicksGame - glfactorTicks;
                Log.Message($"FactorFromGlow_PostFix Timer: {((glfactorTimer.Elapsed.TotalMilliseconds * 1000000) / elapsedTicks):#.0 ns} per tick in the last {elapsedTicks} ticks");
                glfactorTicks = Find.TickManager.TicksGame;
                TotalGlFactorNanoSec += glfactorTimer.ElapsedMilliseconds * 1000000;
                TotalTicks += elapsedTicks;
                glfactorTimer.Reset();

            }
        }
        
        public static void ExplanationPart_PostFix(ref StatRequest req, ref string __result)
        {
            if (!__result.NullOrEmpty() && req.Thing is Pawn pawn && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                float glowat = pawn.Map.glowGrid.GameGlowAt(pawn.Position, false);
                if (glowat < 0.3f || glowat > 0.7f)
                {
                    __result = comp.ExplanationBuilder(__result, glowat);
                }
            }
        }

        #endregion

        #region Hediff Patches
        //public virtual void PostAdd(DamageInfo? dinfo)
        public static void Hediff_PostAdd_Postfix(Hediff __instance)
        {
            Log.Message("Hediff_PostAdd_Postfix called on: " + __instance.Label);
            if (__instance?.pawn is Pawn pawn && pawn.Spawned /*&& pawn.RaceProps.Humanlike*/ && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                Log.Message("Hediff_PostAdd_postfix: Calling CheckAndAddHediff with: " + pawn.NameStringShort + " - " + __instance.def.defName);

                comp.CheckAndAddHediff(__instance, __instance.Part);
            }
        }
        //public virtual void PostRemoved()
        //For hediffs that are removed in Pawn_HealthTracker.HealthTick (removes them directly from list, then calls Hediff.PostRemoved)
        public static void Hediff_PostRemoved_Postfix(Hediff __instance)
        {

            Log.Message("Hediff_PostRemoved_Postfix: called on: " + __instance.Label);
            if (__instance.pawn is Pawn pawn && pawn.Spawned /*&& pawn.RaceProps.Humanlike*/ && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                Log.Message("NightVision: Calling RemoveHediff with: " + pawn.NameStringShort + " - " + __instance.def.defName);

                comp.RemoveHediff(__instance, __instance.Part);
            }
        }

        #endregion

        #region HediffWithComps Patches
        public static void HediffWithComps_PostAdd_Postfix(HediffWithComps __instance)
        {
            if (__instance.pawn is Pawn pawn && pawn.Spawned /*&& pawn.RaceProps.Humanlike*/ && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                Log.Message("HediffWithComps_PostAdd_postfix: Calling CheckAndAddHediff on: " + pawn.NameStringShort + " with hediff " + __instance.Label + " and part " + __instance.Part?.def?.defName ?? " null");

                comp.CheckAndAddHediff(__instance, __instance.Part);
            }
        }
        #endregion

        #region Pawn_HealthTracker Patches

        //public void AddHediff(Hediff hediff, BodyPartRecord part = null, DamageInfo? dinfo = null)  
        public static void AddHediff_Postfix(Hediff hediff, BodyPartRecord part, Pawn_HealthTracker __instance)
        {

            Pawn pawn = Traverse.Create(__instance).Field("pawn")?.GetValue<Pawn>();
            Log.Message("AddHediff_Postfix called on: " + pawn?.Label ?? " null " + "'s hediff " + hediff.Label + " && part " + part?.def.defName?? " null ");
            if (pawn != null &&  pawn.Spawned && /*part != null && hediff is Hediff_MissingPart &&*/ /*pawn.RaceProps.Humanlike &&*/ pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                Log.Message("AddHediff_Postfix: Calling CheckAndAddHediff with: " + pawn.NameStringShort + " & " + hediff.def.defName);

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
        public static void TryDrop_Postfix(Apparel ap, Pawn_ApparelTracker __instance, bool __result)
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
                Log.Message("ApparelTracker.Remove_Postfix: " + pawn.NameStringShort + " - " + ap.Label);

                comp.RemoveApparel(ap);
            }
        }

        public static void TakeWearoutDamageForTheDay_Postfix(Thing ap, Pawn_ApparelTracker __instance)
        {
            if (ap.Destroyed && __instance.pawn is Pawn pawn && pawn.RaceProps.Humanlike && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                Log.Message("ApparelTracker.TakeWearoutDamageForTheDay_Postfix: " + pawn.NameStringShort + " - " + ap.Label);
                if (ap is Apparel apparel)
                {
                    comp.RemoveApparel(apparel);
                }
            }
        }
        #endregion

        #region ThoughtWorker_Dark Patches
        public const int PhotosensDarkThoughtStage = 1;
        public static void CurrentStateInternal_Postfix(Pawn p, ref ThoughtState __result)
        {
            if (__result.Active)
            {
                if (p.GetComp<Comp_NightVision>() is Comp_NightVision comp)
                {
                    switch (comp.NVPsychDark())
                    {
                        default:
                        case GlowMods.Options.NVNone:
                            return;
                        case GlowMods.Options.NVNightVision:
                            __result = ThoughtState.Inactive;
                            return;
                        case GlowMods.Options.NVPhotosensitivity:
                            __result = ThoughtState.ActiveAtStage(PhotosensDarkThoughtStage, GlowMods.Options.NVPhotosensitivity.ToString().Translate());
                            return;
                    }
                }
            }
        }
        #endregion

        //TODO Add patch for Combat Extended ShiftVecReportFor
    }
}
