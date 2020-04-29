using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace NVExperiments
{
        [StaticConstructorOnStartup]
    public class NViHarmonyPatcher
    {
        static NViHarmonyPatcher()
            {
                var harmony = new Harmony("drumad.rimworld.mod.nvtesting");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                //TODO potentially throw error if nightvision is not present
                harmony.Patch(
                    typeof(SymbolResolver_RandomMechanoidGroup)
                                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                                .First(mi => mi.HasAttribute<CompilerGeneratedAttribute>()
                                             && mi.ReturnType == typeof(bool) && mi.GetParameters().Length == 1
                                             && mi.GetParameters()[0].ParameterType == typeof(PawnKindDef)),
                    null,
                    new HarmonyMethod(typeof(NViHarmonyPatcher), nameof(MechanoidsFixerAncient)));
                //TODO Check if below is still required in 1.1
                //harmony.Patch(
                //    typeof(CompSpawnerMechanoidsOnDamaged)
                //                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                //                .First(mi => mi.HasAttribute<CompilerGeneratedAttribute>()
                //                             && mi.ReturnType == typeof(bool) && mi.GetParameters().Length == 1
                //                             && mi.GetParameters()[0].ParameterType == typeof(PawnKindDef)),
                //    null,
                //    new HarmonyMethod(typeof(NVHarmonyPatcher), nameof(MechanoidsFixer)));
            }


        //Stolen from Androids mod by Chjees; written by erdelf. My thanks to them both.

        public static void MechanoidsFixerAncient(
            ref bool    __result,
            PawnKindDef kind)
            {
                if (kind.race.HasModExtension<Stalker_ModExtension>())
                    {
                        __result = false;
                    }
            }

        public static void MechanoidsFixer(
            ref bool    __result,
            PawnKindDef def)
            {
                if (def.race.HasModExtension<Stalker_ModExtension>())
                    {
                        __result = false;
                    }
            }
    }

    

        

        


}
