using System;
using JetBrains.Annotations;
using Verse;

namespace NightVision.Comps
{
    public class HediffCompProperties_NightVision : HediffCompProperties
    {
        public bool GrantsNightVision = false;
        public bool GrantsPhotosensitivity = false;
        public float ZeroLightMod = default(float);
        public float FullLightMod = default(float);

        public bool IsDefault() => Math.Abs(ZeroLightMod) < 0.001f && Math.Abs(FullLightMod) < 0.001f
                                                                   && !(GrantsNightVision || GrantsPhotosensitivity);

        
        [UsedImplicitly]
        public HediffCompProperties_NightVision() => compClass = typeof(HediffComp_NightVision);
    }
}
