using Verse;

namespace NightVision {
    public static class CurrentStrike
    {
        public static float GlowFactor = CalcConstants.TrivialFactor;
        public static float GlowDiff;

        public static bool SurpAtkSuccess => Rand.Chance(CombatHelpers.SurpriseAttackChance(GlowDiff));

        //equiv. to 5% chance per 10% glow factor difference; e.g. diff = 0.1 ==> chance = 0.1*0.5 = 0.05
        [TweakValue("NV", 0, 0.5f)]
        public static float ChanceOfSurpriseAttFactor = 0.5f;
    }
}