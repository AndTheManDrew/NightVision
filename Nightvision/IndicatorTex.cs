using UnityEngine;
using Verse;

namespace NightVision
{
    [StaticConstructorOnStartup]
    public class IndicatorTex
    {
        public static readonly Texture2D PsIndicator = ContentFinder<Texture2D>.Get("UI/Indicators/PSarrow");
        public static readonly Texture2D NvIndicator = ContentFinder<Texture2D>.Get("UI/Indicators/NVarrow");
        public static readonly Texture2D DefIndicator = ContentFinder<Texture2D>.Get("UI/Indicators/DefaultArrow");
    }
}
