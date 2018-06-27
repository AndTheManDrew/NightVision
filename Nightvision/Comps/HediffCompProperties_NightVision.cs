using System;
using JetBrains.Annotations;
using NightVision.LightModifiers;
using Verse;

namespace NightVision.Comps
{
    public class HediffCompProperties_NightVision : HediffCompProperties
        {
            public Hediff_LightModifiers LightModifiers;
        public bool GrantsNightVision = false;
        public bool GrantsPhotosensitivity = false;
        public float ZeroLightMod = default;
        public float FullLightMod = default;

        public bool IsDefault() => Math.Abs(ZeroLightMod) < 0.001f && Math.Abs(FullLightMod) < 0.001f
                                                                   && !(GrantsNightVision || GrantsPhotosensitivity);

        
        [UsedImplicitly]
        public HediffCompProperties_NightVision() => compClass = typeof(HediffComp_NightVision);
    }
}
