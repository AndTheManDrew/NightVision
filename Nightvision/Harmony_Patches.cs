using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using Verse;
using Harmony;
using System.Diagnostics;

namespace Nightvision
{

    /// <summary>
    /// -Improve the nightvision check 
    ///     - potentially build a def list/dict/C# thingy at startup that can be checked against for bionic eyes
    ///     -or use something like hediffs[i].Part.HasTag("SightSource")
    /// But what about cost? As Rimworld has a limited effective light level sys, I think balance should be around 50% i.e. for lightlevels > artificial
    ///malus for photosensitive
    ///^ To not punish players from lighting their base as normal -- as in both photosensitive&normal&nightvision pawns should have == work speed in 50% light
    /// </summary>


    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        #region Applying Harmony Patches
        static HarmonyPatches()
        {


            var harmony = HarmonyInstance.Create("atmd.nightvision.for.rimworld");

            {
                //Both Patches hit StatPart_Glow methods
                Type type = typeof(StatPart_Glow);

                //Vanilla method:
                //public override void TransformValue(StatRequest req, ref float val)
                var transform_value_vanilla_method = type.GetMethod("TransformValue", BindingFlags.Public | BindingFlags.Instance);
                harmony.Patch(transform_value_vanilla_method, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("TransformValue_Prefix")), new HarmonyMethod(typeof(HarmonyPatches).GetMethod("TransformValue_Postfix")));

                //Vanilla method:
                //public override string ExplanationPart(StatRequest req)
                var explanation_part_vanilla_method = type.GetMethod("ExplanationPart", BindingFlags.Public | BindingFlags.Instance);
                harmony.Patch(explanation_part_vanilla_method, null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("ExplanationPart_Postfix")));

            }

            var methods = harmony.GetPatchedMethods();
            foreach (var method in methods)
            {
                Log.Message(method.ToString());
            }
        }
        #endregion

        //#region Debugger?
        ////Directly copied from DoctorVanGogh on Discord
        //[HarmonyPatch(typeof(ATMD_Nightvision.HarmonyPatches), nameof(Has_NightVision))]
        //class Foo
        //{

        //    public static void Prefix(out object __state)
        //    {
        //        __state = Stopwatch.StartNew();
        //    }

        //    public static void Postfix(object __state, object __instance)
        //    {
        //        Stopwatch sw = (Stopwatch)__state;

        //        sw.Stop();
        //        Log.Message($"{__instance.GetType()}: {sw.ElapsedMilliseconds} ms");
        //    }
        //}
        //#endregion

        #region Transfrom_Value Patches
        /* 		public override void TransformValue(StatRequest req, ref float val)
         *           {
         *               if (req.HasThing && this.ActiveFor(req.Thing)) 
         *               {
         *                   val *= this.FactorFromGlow(req.Thing);
         *               }
         *           } 
         */

        public static void TransformValue_Prefix(out float __state, ref float val)
        {
            __state = val;
            return;
        }

        public static void TransformValue_Postfix(float __state, ref float val, ref StatRequest req)
        {
            //If the factor from glow was 1, and left the initial val unchanged, then do nothing
            //This should catch if req.HasThing is false, but if implementing photosensitivity then will need to include check
            if (__state == val)
            {
                return;
            }

            //Could be useful to determine glow grid result -- also consider the explanation part which has glow grid in percent
            float factorfromglow = val / __state;
            Log.Message("Factor from glow was: " + factorfromglow.ToString());

            //Shouldn't need to check req.HasThing -- for now
            Pawn pawn = req.Thing as Pawn;
            //This next check might catch req with no Thing??
            if (pawn != null && pawn.RaceProps.Humanlike)
            {
                int num_NV_eyes = NightVisionChecker.AmountOfNightVision(pawn);
                //Derived from y = mx + c  ==> out = numNV * (pre - post)/2 + post
                val = val + (__state - val) * num_NV_eyes / 2;
            }
            return;
        }

        #endregion


        #region ExplanationPart Patches

        //public override string ExplanationPart(StatRequest req)
        //{
        //    if (req.HasThing && this.ActiveFor(req.Thing))
        //    {
        //        return "StatsReport_LightMultiplier".Translate(new object[]
        //        {
        //            this.GlowLevel(req.Thing).ToStringPercent()
        //        }) + ": x" + this.FactorFromGlow(req.Thing).ToStringPercent();
        //    }
        //    return null;
        //}
        public static void ExplanationPart_Postfix(ref StatRequest req, ref string __result/*, ref StatPart_Glow __instance*/)
        {
            //Log.Message("ExplanationPart returned: " + __result);
            if (__result == null){
                return;
            }
            //Log.Message("ExplanationPartPatch passed null check");
            //read string and extract the glow factor, then use that as base for calculations
            Pawn pawn = req.Thing as Pawn;
            //Log.Message("ExplanationPartPatch being called for: " + pawn.ToString());
            if (pawn != null && pawn.RaceProps.Humanlike)
            {
                int num_NV_eyes = NightVisionChecker.AmountOfNightVision(pawn);

                if (num_NV_eyes == 0)
                {
                    return;
                }

                string[] tempArray = __result.Split(':');
                char[] charsToTrim = { '%', ' ', 'x' };
                tempArray[1] = tempArray[1].Trim(charsToTrim);

                float.TryParse(tempArray[1], out float temp);

                //Log.Message("Parsed Glow Factor from explanationpart: " + temp.ToString());

                // temp == 0 implies TryParse failed as lower limit of glowfactor == 80
                //temp == 100, nothing for this to do
                if (temp == 0f || temp == 100f) { return; }

                temp = (temp + num_NV_eyes*0.5f*(100f - temp));

                //Should probably add a translate function here, and think of a better way of expressing it
                __result = tempArray[0] + ": x" + temp.ToString() + "% with night vision.";
                return;
                

                    
            }
        }

        #endregion


    }
}
