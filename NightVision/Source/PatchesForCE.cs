using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using CombatExtended;
using Harmony;
using Verse;

namespace NightVision {
        //public static class PatchesForCE {
                

        //        /// <summary>
        //        ///     Should only be executed in try-catch(TypeLoadException) block
        //        ///     See: https://stackoverflow.com/questions/3346740/typeloadexception-is-not-caught-by-try-catch
        //        /// </summary>
        //        [MethodImpl(MethodImplOptions.NoInlining)]
        //        public static void ApplyCombatExtendedPatch(
        //            ref HarmonyInstance harmony)
        //            {
        //                //ModMetaData m.Name is defined in the About.xml file of the mod, therefore no need to account for steamID messes
        //                if (ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Combat Extended"))
        //                    {
        //                        MethodInfo Verb_LaunchProjectileCE_ShiftVecReportFor =
        //                                    AccessTools.Method(typeof(Verb_LaunchProjectileCE),
        //                                        nameof(Verb_LaunchProjectileCE.ShiftVecReportFor));
        //                        harmony.Patch(Verb_LaunchProjectileCE_ShiftVecReportFor,
        //                            null,
        //                            new HarmonyMethod(typeof(NVHarmonyPatcher), nameof(ShiftVecReportFor_Postfix)));
        //                        //Checked by mod settings window: if true will display checkbox for NVEnabledForCE
        //                        Settings.CEDetected = true;
        //                        Log.Message("Night Vision detected Combat Extended and patched: "
        //                                    + Verb_LaunchProjectileCE_ShiftVecReportFor.Name);
        //                    }
        //            }

        //        /// <summary>
        //        ///     CE's lighting shift (= 1 - glow) is linear function between points (glow = 1, shift = 0) & (glow = 0, shift = 1)
        //        ///     All this method does is transform the shift function to a linear function between points:
        //        ///     (glow = 1, shift = - modifier at 100% light)
        //        ///     (glow = 0, shift = 1 - modifier at 0% light)
        //        ///     where, with default settings,
        //        ///     modifier at 100%  less or  = 0   , therefore same or worse
        //        ///     modifier at 0% light greater or = 0, therefore same or better
        //        /// </summary>
        //        public static void ShiftVecReportFor_Postfix(
        //            ref ShiftVecReport      __result,
        //            Verb_LaunchProjectileCE instance)
        //            {
        //                if (NVEnabledForCE && instance.caster is Pawn pawn
        //                                   && pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
        //                    {
        //                        __result.lightingShift =
        //                                    __result.lightingShift
        //                                    * (1 + comp.FullLightModifier - comp.ZeroLightModifier)
        //                                    - comp.FullLightModifier;
        //                    }
        //            }
        //    }
    }