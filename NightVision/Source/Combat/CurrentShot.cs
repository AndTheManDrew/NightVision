using Verse;

namespace NightVision {
    public static class CurrentShot {
        public static float GlowFactor = CalcConstants.TrivialFactor;
        public static Verb  Verb;
        public static Thing Caster;
        public static float OriginalHitOnStandardTarget;
        public static float ModifiedHitOnStandardTarget;

        public static bool NoShot => Verb == null || Caster == null;

        public static void ClearLastShot()
        {
            GlowFactor = CalcConstants.TrivialFactor;
            Verb       = null;
            Caster     = null;
        }

        public static float PseudoMultiplier()
        {
            return ModifiedHitOnStandardTarget / OriginalHitOnStandardTarget;
        }
    }
}