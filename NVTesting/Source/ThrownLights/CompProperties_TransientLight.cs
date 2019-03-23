// Nightvision NVTesting CompProperties_TransientLight.cs
// 
// 19 03 2019
// 
// 19 03 2019

using RimWorld;
using Verse;

namespace NVTesting.ThrownLights
{
    public class CompProperties_TransientLight : CompProperties_Glower
    {
        public int ticksToGlow = 1000;
        public float finalGlowRadius = 10;

        public CompProperties_TransientLight() { }

        public CompProperties_TransientLight(CompProperties props)
        {
            if (props is CompProperties_TransientLight propsTl)
            {
             
                ticksToGlow     = propsTl.ticksToGlow;
                finalGlowRadius = propsTl.finalGlowRadius;
            }
        }
    }
}