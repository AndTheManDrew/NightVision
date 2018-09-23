using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace NightVision
{
    public static class Str
    {
        public static readonly string Effect = "NVEffects".Translate();

        public static readonly string NightVision = VisionType.NVNightVision.ToString().Translate();

        public static readonly string Photosens = VisionType.NVPhotosensitivity.ToString().Translate();

        public static string MaxAtGlow(float glow)
        {
            return "NVMaxAtGlow".Translate(glow.ToStringPercent());
        }
    }
}
