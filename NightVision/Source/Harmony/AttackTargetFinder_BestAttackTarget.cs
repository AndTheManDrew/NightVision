// Nightvision NightVision AttackTargetFinder_BestAttackTarget.cs
// 
// 20 10 2018
// 
// 20 10 2018

using Harmony;
using Verse;
using Verse.AI;

namespace NightVision.Harmony
{
    [HarmonyPatch(typeof(AttackTargetFinder), nameof(AttackTargetFinder.BestAttackTarget))]
    public static class AttackTargetFinder_BestAttackTarget
    {
        public static void Prefix(IAttackTargetSearcher searcher, ref float maxDist)
        {
            if (maxDist.IsNonTrivial() && searcher is Pawn pawn)
            {
                float glowFactor = GlowFor.FactorOrFallBack(pawn);

                if (glowFactor.FactorIsNonTrivial())
                {
                    maxDist = maxDist * glowFactor * glowFactor * glowFactor;
                }
            }
        }
    }
}