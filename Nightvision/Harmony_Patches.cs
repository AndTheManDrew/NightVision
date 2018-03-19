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
            if (req.HasThing)
            {
                if (req.Thing is Pawn pawn && pawn.RaceProps.Humanlike)
                {
                    float glowat = pawn.Map.glowGrid.GameGlowAt(pawn.Position, false);
                    val = __state * pawn.GetComp<Comp_NightVision>()?.FactorFromGlow(glowat) ?? val;
                    return;
                    
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

            Pawn pawn = req.Thing as Pawn;
            // TODO Not sure this next check is necessary, so adding an else clause which prints to Log
            if (pawn != null && pawn.RaceProps.Humanlike)
            {
                float glowat = pawn.Map.glowGrid.GameGlowAt(pawn.Position, false);
                //If glow is outside NV range, do nothing
                if (glowat > 0.3f & glowat < 0.7f)
                {
                    return;
                }

                if (pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
                {
                    string[] tempArray = __result.Split(':');
                    
                    float temp = 100f * comp.FactorFromGlow(glowat);

                    //Should probably add a translate function here, and think of a better way of expressing it
                    string resultString = $"{tempArray[0]}: x{temp.ToString()}% with night vision from: \n" + comp.NVEffectorsAsListStr.ToStringSafeEnumerable();
                    __result = resultString;
                }
                
                return;
                

                    
            }
            Log.Message("NV: Exp.Patch: Pawn was null or not humanlike but vanilla explanation part was not null");
        }

        #endregion
        
        //TODO Add patch for Psych Glow

        //TODO Add patch for Combat Extended ShiftVecReportFor
    }
}
