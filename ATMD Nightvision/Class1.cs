using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using Verse;
using Harmony;
using System.Diagnostics;

namespace ATMD_Nightvision
{

    /// <summary>
    /// A prefix harmony patch of StatPart_Glow.FactorFromGlow + shoddy method to work out if a pawn has bionic eyes:
    /// To be improved:
    /// -switch to postfix prefix set, prefix storing ref value then postfix comparing
    /// -Improve the nightvision check 
    ///     - potentially build a def list/dict/C# thingy at startup that can be checked against for bionic eyes
    ///     -or use something like hediffs[i].Part.HasTag("SightSource")
    /// But what about cost? As Rimworld has a limited effective light level sys, I think balance should be around 50% i.e. for lightlevels > artificial
    ///malus for photosensitive
    ///^ To not punish players from lighting their base as normal -- as in both photosensitive&normal&nightvision pawns should have == work speed in 50% light
    ///This would require added complexity as factorfromglow (I believe) does not differentiate at higher glow levels
    /// </summary>
 

    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {

        static HarmonyPatches()
        {


            var harmony = HarmonyInstance.Create("atmd.nightvision.for.rimworld");

            {

                Type type = typeof(StatPart_Glow);
                MethodInfo patch_target = type.GetMethod("factorfromglow", BindingFlags.NonPublic | BindingFlags.Instance);
                var prefix = typeof(HarmonyPatches).GetMethod("FactorfromGlow_Prefix");
                var postfix = typeof(HarmonyPatches).GetMethod("FactorfromGlow_Postfix");
                harmony.Patch(patch_target, new HarmonyMethod( prefix), new HarmonyMethod(postfix));

            }
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        var methods = harmony.GetPatchedMethods();
        foreach (var method in methods)
            {
                Log.Message(method.ToString());
            }
        }

        #region Debugger?
        //Directly copied from DoctorVanGogh on Discord
        [HarmonyPatch(typeof(ATMD_Nightvision.HarmonyPatches), nameof(Has_NightVision))]
        class Foo
        {

            public static void Prefix(out object __state)
            {
                __state = Stopwatch.StartNew();
            }

            public static void Postfix(object __state, object __instance)
            {
                Stopwatch sw = (Stopwatch)__state;

                sw.Stop();
                Log.Message($"{__instance.GetType()}: {sw.ElapsedMilliseconds} ms");
            }
        }
        #endregion

        #region FactorfromGlowPatches

        [HarmonyPrefix]
        public static bool FactorfromGlow_Prefix(out float __state, ref float __result, ref Thing t)
        {
            __state = __result;
            return true;
        }
        [HarmonyPostfix]
        public static void FactorfromGlow_Postfix(float __state, ref float __result, ref Thing t)
        {
            //Adding a factor from glow checker. NEED TO REMOVE THIS LEAST PEOPLE GET ALL PISSY

            float factorfromglow = __result / __state;
            Log.Message("Factor from glow was: " + factorfromglow.ToString());



            //If the factor from glow was 1, and left the initial val unchanged, then do nothing
            //NOTE: This will need to be changed if photo-sensitivity is implemented
            if (__state == __result)
            {
                return;
            }
            Pawn pawn = t as Pawn;
            if (pawn != null && pawn.RaceProps.Humanlike)
            {
                int num_NV_eyes = Has_NightVision(pawn);
                //Derived from y = mx + c  :::> out = numNV * (pre - post)/2 + post
                __result = __result + (__state - __result)*num_NV_eyes/2;
            }
            return;
        }

        #endregion


        //Not sure how efficient the following code is. An alternative might be to build a list of bionic eye hediff defs on game load then test against that
        public static int Has_NightVision(Pawn pawn)
        {
            //Log.Message("Has_NightVision has been called");

                int num_NV_eye = 0;
                string tag = "SightSource";
                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    //Log.Message(hediff.ToString());
                    if (hediff is Hediff_AddedPart && hediff.def.addedPartProps.isBionic && hediff.Part.def.tags.Contains(tag))
                    {
                        num_NV_eye++;
                    }
                }
                return num_NV_eye;
            }
        }
    }
