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
        //public static SimpleCurve vanillaGlowCurve;

        #region Applying Harmony Patches
        static HarmonyPatches()
        {


            var harmony = HarmonyInstance.Create("atmd.nightvision.for.rimworld");

            {
                //All harmony stuff hits StatPart_Glow methods
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
            //float factorfromglow = val / __state;
            
            if (req.HasThing)
            {
                Log.Message("HP_1");
                if (req.Thing is Pawn pawn && pawn.RaceProps.Humanlike)
                {
                    //Potentially pass __state to NightVisionChecker to save calculating glowfactorforNormal ?
                    float? NV_factorfromGlow = NightVisionChecker.NightVisionFactor(pawn);

                    if (!NV_factorfromGlow.HasValue)
                    {
                        Log.Message("HP_1A");
                        return;
                    }

                    val = __state * NV_factorfromGlow.Value;
                    return;


                    //Depreciated:
                    //Derived from y = mx + c  ==> out = numNV * (pre - post)/2 + post
                    //val = val + (__state - val) * num_NV_eyes / 2;
                }
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
            //Not sure this next check is necessary, so adding an else clause which prints to Log
            if (pawn != null && pawn.RaceProps.Humanlike)
            {
                string[] tempArray = __result.Split(':');
                char[] charsToTrim = { '%', ' ', 'x' };
                tempArray[1] = tempArray[1].Trim(charsToTrim);

                float.TryParse(tempArray[1], out float temp);

                // temp == 0 implies TryParse failed (or grabbed wrong thing) as lower limit of glowfactor == 80
                if ( temp == 0f) { Log.Message("NightVision: TryParse failed in explanationpart postfix");  return; }

                float? NV_factorfromGlow = NightVisionChecker.NightVisionFactor(pawn);

                if (!NV_factorfromGlow.HasValue)
                {
                    return;
                }

                temp = 100f * NV_factorfromGlow.Value;

                //Should probably add a translate function here, and think of a better way of expressing it
                string resultString = $"{tempArray[0]}: x{temp.ToString()}% with night vision from: \n TESTESTESTEST";
                __result = resultString;
                return;
                

                    
            }
        }

        #endregion
        
    }
}
