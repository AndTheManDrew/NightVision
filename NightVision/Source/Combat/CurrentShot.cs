using Verse;

namespace NightVision {
    public static class CurrentShot {
        public static float GlowFactor = Constants_Calculations.TrivialFactor;
        public static Verb  Verb;
        public static Thing Caster;
        public static float OriginalHitOnStandardTarget;
        public static float ModifiedHitOnStandardTarget;

        public static bool NoShot => Verb == null || Caster == null;

        public static void ClearLastShot()
        {
            GlowFactor = Constants_Calculations.TrivialFactor;
            Verb       = null;
            Caster     = null;
        }

        public static float PseudoMultiplier()
        {
            return ModifiedHitOnStandardTarget / OriginalHitOnStandardTarget;
        }
    }
}