using Verse;

namespace NightVision {
    public static class CurrentStrike
    {
        public static float GlowFactor = Constants_Calculations.TrivialFactor;
        public static float GlowDiff;

        public static bool SurpAtkSuccess
        {
            get
            {
                if (CombatHelpers.ChanceOfSurpriseAttFactor.ApproxEq(0))
                {
                    return false;
                }
                return Rand.Chance(CombatHelpers.SurpriseAttackChance(GlowDiff));
            }
        }
    }
}