// Nightvision NVExperiments Verb_ThrowLight.cs
// 
// 19 03 2019
// 
// 19 03 2019

using Verse;

namespace NVExperiments.ThrownLights
{
    public class Verb_ThrowLight : Verb_LaunchProjectile
    {
        public override void WarmupComplete()
        {
            base.WarmupComplete();
        }

        protected override bool TryCastShot()
        {
            return base.TryCastShot();
        }

        public override bool Available()
        {
            return base.Available();
        }

        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            return base.CanHitTargetFrom(root, targ);
        }
    }
}