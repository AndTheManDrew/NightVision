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

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        public const string eyeTag = "SightSource";
        public const float minGlowNoGlow = 0.3f;
        public const float maxGlowNoGlow = 0.7f;

        #region Applying Harmony Patches
        static HarmonyPatches()
        {


            var harmony = HarmonyInstance.Create("nightvision.rimworld.mod");

            

            // Targets
            MethodInfo StatPart_Glow_FactorFromGlow = AccessTools.Method(typeof(StatPart_Glow), "FactorFromGlow");
            MethodInfo Hediff_PostAdd = AccessTools.Method(typeof(Hediff), nameof(Hediff.PostAdd));
            MethodInfo Hediff_PostRemoved = AccessTools.Method(typeof(Hediff), nameof(Hediff.PostRemoved));
            MethodInfo HediffWithComps_PostAdd = AccessTools.Method(typeof(HediffWithComps), nameof(HediffWithComps.PostAdd));
            //MethodInfo HediffWithComps_PostRemoved = AccessTools.Method(typeof(HediffWithComps), nameof(HediffWithComps.PostRemoved));
            MethodInfo HealthTracker_AddHediff = AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.AddHediff), new Type[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo) });
            MethodInfo ApparelTracker_Wear = AccessTools.Method(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Wear));
            MethodInfo ApparelTracker_TryDrop = typeof(Pawn_ApparelTracker).GetMethods(AccessTools.all).Where(methodInfo => methodInfo.Name == "TryDrop" && methodInfo.GetParameters().Where(pi => pi.ParameterType == typeof(IntVec3)).Any()).First();
            MethodInfo ApparelTracker_Remove = AccessTools.Method(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Remove));
            MethodInfo ApparelTracker_TakeWearoutDamageForDay = AccessTools.Method(typeof(Pawn_ApparelTracker), "TakeWearoutDamageForDay");


            //var transform_value_vanilla_method = typeof(StatPart_Glow).GetMethod("TransformValue", BindingFlags.Public | BindingFlags.Instance);
            //harmony.Patch(transform_value_vanilla_method, new HarmonyMethod(harmonytype.GetMethod("TransformValue_Prefix")), new HarmonyMethod(harmonytype.GetMethod("TransformValue_Postfix")));
            //var explanation_part_vanilla_method = type.GetMethod("ExplanationPart", BindingFlags.Public | BindingFlags.Instance);
            //harmony.Patch(explanation_part_vanilla_method, null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("ExplanationPart_Postfix")));

            //Patches
            Type harmonytype = typeof(HarmonyPatches);
            harmony.Patch(StatPart_Glow_FactorFromGlow, null, new HarmonyMethod(harmonytype.GetMethod("FactorFromGlow_PostFix")));
            harmony.Patch(Hediff_PostAdd, null, new HarmonyMethod(harmonytype, nameof(HediffWithComps_PostAdd_Postfix)));
            harmony.Patch(Hediff_PostRemoved, null, new HarmonyMethod(harmonytype, nameof(Hediff_PostRemoved_Postfix)));
            harmony.Patch(HealthTracker_AddHediff, null, new HarmonyMethod(harmonytype, nameof(AddHediff_Postfix)));
            harmony.Patch(ApparelTracker_Wear, null, new HarmonyMethod(harmonytype, nameof(Wear_Postfix)));
            harmony.Patch(ApparelTracker_TryDrop, null, new HarmonyMethod(harmonytype, nameof(TryDrop_Postfix)));
            harmony.Patch(ApparelTracker_Remove, null, new HarmonyMethod(harmonytype, nameof(Remove_Postfix)));
            harmony.Patch(ApparelTracker_TakeWearoutDamageForDay, null, new HarmonyMethod(harmonytype, nameof(TakeWearoutDamageForTheDay_Postfix)));
            harmony.Patch(HediffWithComps_PostAdd, null, new HarmonyMethod(harmonytype, nameof(HediffWithComps_PostAdd_Postfix)));


            var meths = harmony.GetPatchedMethods();
            Log.Message(meths.Count().ToString());
            foreach (MethodInfo mi in meths)
            {
                Log.Message(mi.Name);
                Log.Message(mi.ToString());
            }
            
        }
        #endregion


        #region StatPart_Glow Patches
        //TODO Patch Explanation Part
        public static void FactorFromGlow_PostFix(Thing t, ref float __result)
        {
            if (t is Pawn pawn && pawn.RaceProps.Humanlike && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                float glowat = pawn.Map.glowGrid.GameGlowAt(pawn.Position, false);
                __result = comp.FactorFromGlow(glowat);
            }
        }

        #endregion

        #region Hediff Patches
        //public virtual void PostAdd(DamageInfo? dinfo)
        public static void Hediff_PostAdd_Postfix(Hediff __instance)
        {
            Log.Message("Hediff_PostAdd_Postfix: " + __instance.Label);
            if (__instance?.pawn is Pawn pawn && pawn.RaceProps.Humanlike && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                Log.Message("NightVision: Hediff_PostAdd_postfix: " + pawn.NameStringShort + " - " + __instance.def.defName);

                comp.CheckAndAddHediff(__instance, __instance.Part);
            }
        }
        //public virtual void PostRemoved()
        public static void Hediff_PostRemoved_Postfix(Hediff __instance)
        {

            Log.Message("Hediff_PostRemoved_Postfix: " + __instance.Label);
            if (__instance?.pawn is Pawn pawn && pawn.RaceProps.Humanlike && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                Log.Message("NightVision: Hediff_PostRemoved_postfix: " + pawn.NameStringShort + " - " + __instance.def.defName);

                comp.RemoveHediff(__instance, __instance.Part);
            }
        }

        #endregion

        #region HediffWithComps Patches
        public static void HediffWithComps_PostAdd_Postfix(HediffWithComps __instance)
        {
            Log.Message("HediffWithComps_PostAdd_Postfix: " + __instance.Label);
            if (__instance.pawn is Pawn pawn && pawn.RaceProps.Humanlike && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                Log.Message("NightVision: HediffWithComps_PostAdd_postfix: " + pawn.NameStringShort + " - " + __instance.def.defName);

                comp.CheckAndAddHediff(__instance, __instance.Part);
            }
        }
        #endregion
        #region Pawn_HealthTracker Patches

        // I am unsure why this is necessary but the Hediff patches above do not seem to catch Hediff_MissingPart: at least not when added in devmode
        //public void AddHediff(Hediff hediff, BodyPartRecord part = null, DamageInfo? dinfo = null)  
        public static void AddHediff_Postfix(Hediff hediff, BodyPartRecord part, Pawn_HealthTracker __instance)
        {

            Pawn pawn = Traverse.Create(__instance).Field("pawn")?.GetValue<Pawn>();
            Log.Message("AddHediff_Postfix: " + pawn?.Label + " - " + hediff.Label + " - " + part?.def.defName);
            if (pawn != null && part != null && hediff is Hediff_MissingPart && pawn.RaceProps.Humanlike && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                Log.Message("NightVision: AddHediff_Postfix: " + pawn.NameStringShort + " - " + hediff.def.defName);

                comp.CheckAndAddHediff(hediff, part);
            }
        }

        #endregion

        #region Pawn_ApparelTracker Patches

        //public void Wear(Apparel newApparel, bool dropReplacedApparel = true)
        //public bool TryDrop(Apparel ap, out Apparel resultingAp, IntVec3 pos, bool forbid = true)
        //public void Remove(Apparel ap)
        //private void TakeWearoutDamageForDay(Thing ap)
        public static void Wear_Postfix(Apparel newApparel, Pawn_ApparelTracker __instance)
        {
            if (__instance?.pawn is Pawn pawn && pawn.RaceProps.Humanlike)
            {
                Log.Message("Night Vision: Pawn_ApparelTracker.Wear_Postfix called for: " + pawn.NameStringShort + " to wear " + newApparel.Label);
                if (pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
                {
                    comp.CheckAndAddApparel(newApparel);
                }
                //else
                //{
                //    //TODO Reconsider this: Should this be placed in an injector somewhere, activated on game load
                //    ThingComp thingComp = (ThingComp)Activator.CreateInstance(typeof(Comp_NightVision));
                //    thingComp.parent = pawn;
                //    pawn.AllComps.Add(thingComp);
                //    thingComp.Initialize(new CompProperties_NightVision());
                //    ((Comp_NightVision)thingComp).CheckAndAddApparel(newApparel);
                //    Log.Message("Night Vision: Made a new comp for " + pawn.NameStringShort);
                //}
            }
        }
        public static void TryDrop_Postfix(Apparel ap, Pawn_ApparelTracker __instance, bool __result)
        {
            if (__result && __instance?.pawn is Pawn pawn && pawn.RaceProps.Humanlike && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                Log.Message("NightVision: ApparelTracker_tryDrop_Postfix: " + pawn.NameStringShort + " - " + ap.Label);

                comp.RemoveApparel(ap);
            }
        }
        public static void Remove_Postfix(Apparel ap, Pawn_ApparelTracker __instance)
        {
            if (__instance?.pawn is Pawn pawn && pawn.RaceProps.Humanlike && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                Log.Message("NightVision: ApparelTracker_Remove_Postfix: " + pawn.NameStringShort + " - " + ap.Label);

                comp.RemoveApparel(ap);
            }
        }

        public static void TakeWearoutDamageForTheDay_Postfix(Thing ap, Pawn_ApparelTracker __instance)
        {
            if (ap.Destroyed && __instance?.pawn is Pawn pawn && pawn.RaceProps.Humanlike && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                Log.Message("NightVision: ApparelTracker_TakeWearoutDamageForTheDay_Postfix: " + pawn.NameStringShort + " - " + ap.Label);
                if (ap is Apparel apparel)
                {
                    comp.RemoveApparel(apparel);
                }
            }
        }
        #endregion

        //TODO Add patch for Psych Glow

        //TODO Add patch for Combat Extended ShiftVecReportFor
    }
    }
