// Nightvision NVTesting CompProps_ChangeableProjectile_Thrown.cs
// 
// 19 03 2019
// 
// 19 03 2019

using Verse;

namespace NVTesting.ThrownLights
{
    public class CompProps_ChangeableProjectile_Thrown : CompProperties
    {
        public CompProps_ChangeableProjectile_Thrown()
        {
            compClass = typeof(Comp_ChangeableProjectile_Thrown);
        }

        public int maxAmmo = 5;
    }
}