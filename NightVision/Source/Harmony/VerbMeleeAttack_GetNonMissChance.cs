using Harmony;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace NightVision.Harmony {
    [HarmonyPatch(typeof(Verb_MeleeAttack), "GetNonMissChance")]
    public static class VerbMeleeAttack_GetNonMissChance
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        public static void GetNonMissChance_Postfix(ref Verb __instance, ref float __result, LocalTargetInfo target)
        {
            if (__result > 0.999f || !(__instance.CasterPawn is Pawn pawn) || !(pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp))
            {
                CurrentStrike.GlowFactor = CalcConstants.TrivialFactor;
                CurrentStrike.GlowDiff   = 0f;

                return;
            }

            float glow = pawn.Map != null? GlowFor.GlowAt(pawn.Map, target.Cell) : CalcConstants.TrivialGlow;

            if (!glow.GlowIsDarkOrBright())
            {
                CurrentStrike.GlowFactor = CalcConstants.TrivialFactor;
                CurrentStrike.GlowDiff   = 0f;

                return;
            }

            CurrentStrike.GlowFactor = comp.FactorFromGlow(glow);

            if (target.Thing is Pawn t_pawn)
            {
                CurrentStrike.GlowDiff = CurrentStrike.GlowFactor - GlowFor.FactorOrFallBack(t_pawn, glow);

                if (CurrentStrike.GlowDiff.IsNonTrivial() && CurrentStrike.SurpAtkSuccess)
                {
                    AccessTools.FieldRefAccess<Verb, bool>(__instance, "surpriseAttack") = true;
                    __result                                                             = 1f;

                    return;
                }
            }
            else
            {
                CurrentStrike.GlowDiff = 0;
            }

            __result = CombatHelpers.HitChanceGlowTransform(__result, CurrentStrike.GlowFactor);
        }
    }
}