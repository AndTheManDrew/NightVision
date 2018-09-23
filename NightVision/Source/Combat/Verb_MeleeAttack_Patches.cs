using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using RimWorld;
using Verse;

namespace NightVision
{

    public static class MeleeVariables
    {
        public static float CurrentGlowFactor = 1;
        public static float CurrentGlowDiff = 0;
        
        //equiv. to 5% chance per 10% glow factor difference; e.g. diff = 0.1 ==> chance = 0.1*0.5 = 0.05
        [TweakValue("NV", 0, 0.5f)]
        public static float ChanceOfSurpriseAttFactor = 0.5f; 

        [TweakValue("NV", 0.1f, 1)]
        public static float MeleeAttackExp = 1;
        [TweakValue("NV", 0.1f, 1)]
        public static float MeleeDodgeFactorExp = 1;
        [TweakValue("NV", 0.1f, 1)]
        public static float MeleeDodgeDiffExp = 1;
    }
    [HarmonyPatch(typeof(Verb_MeleeAttack), "GetNonMissChance")]
    public static class GetNonMissChance_Patch
    {
        [HarmonyPostfix]
        public static void GetNonMissChance_Postfix(ref Verb __instance, ref float __result, LocalTargetInfo target)
        {
            if (__result > 0.9999 || !(__instance.CasterPawn is Pawn pawn) || !(pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp))
            {
                MeleeVariables.CurrentGlowFactor = 1f;
                return;
            }
            float glow = pawn.Map.glowGrid.GameGlowAt(target.Cell);

            if (glow > Constants.MinGlowNoGlow - 0.001f && glow < Constants.MaxGlowNoGlow + 0.001f)
            {
                MeleeVariables.CurrentGlowFactor = 1f;
                return;
            }
            
            MeleeVariables.CurrentGlowFactor = comp.FactorFromGlow(glow);

            if (target.Thing is Pawn t_pawn)
            {
                float diffGF = MeleeVariables.CurrentGlowFactor - (t_pawn.GetComp<Comp_NightVision>()?.FactorFromGlow(glow) ?? 1f);

                if (diffGF > 0.0001 && Rand.Chance(diffGF * MeleeVariables.ChanceOfSurpriseAttFactor))
                {
                    AccessTools.FieldRefAccess<Verb, bool>(__instance, "surpriseAttack") = true;
                    __result = 1f;
                    return;
                }
            }
            __result = Math.Min(__result *(float)Math.Pow(MeleeVariables.CurrentGlowFactor, MeleeVariables.MeleeAttackExp), 0.99f);
        }
    }

    [HarmonyPatch(typeof(Verb_MeleeAttack), "GetDodgeChance")]
    public static class GetDodgeChance_Patch
    {
        [HarmonyPostfix]
        public static void GetDodgeChance_Postfix(
                        Verb            __instance,
                        ref float       __result,
                        LocalTargetInfo target
                    )
        {
            Pawn a_pawn = __instance.CasterPawn;
            Pawn t_pawn = target.Thing as Pawn;
            if (__result < 0.0001 || a_pawn == null && t_pawn == null)
            {
                return;
            }

            float glow = a_pawn?.Map.glowGrid.GameGlowAt(target.Cell) ?? t_pawn.Map.glowGrid.GameGlowAt(target.Cell);

            if (glow > 0.299999 && glow < 0.69999)
            {
                return;
            }
            float defFactor = t_pawn?.GetComp<Comp_NightVision>()?.FactorFromGlow(glow) ?? 1f;
            float attFactor = MeleeVariables.CurrentGlowFactor;

            //TODO Use stored diff
            __result = (float)(__result * Math.Pow(defFactor, MeleeVariables.MeleeDodgeFactorExp) * Math.Pow((1 + defFactor - attFactor), MeleeVariables.MeleeDodgeDiffExp));

        }
    }
}
