// Nightvision NVTesting MovingGlowFlooder_Patch.cs
// 
// 20 03 2019
// 
// 20 03 2019

using System.Diagnostics;
using Harmony;
using Verse;

namespace NVTesting.ThrownLights
{
    [HarmonyPatch(typeof(GlowGrid), nameof(GlowGrid.GlowGridUpdate_First))]
    public static class MovingGlowFlooder_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(bool ___glowGridDirty, bool __state)
        {
            if (___glowGridDirty)
            {
                for (var index = MovingGlowFlooder.ActiveGlowFlooders.Count - 1; index >= 0; index--)
                {
                    WeakReference<MovingGlowFlooder> activeGlowFlooder = MovingGlowFlooder.ActiveGlowFlooders[index];

                    if (activeGlowFlooder.IsAlive)
                    {
                        activeGlowFlooder.Target.RestoreOriginalGlow();
                        __state = true;
                    }
                    else
                    {
                        MovingGlowFlooder.ActiveGlowFlooders.Remove(activeGlowFlooder);
                    }
                    
                }
            }
        }

        [HarmonyPostfix]
        public static void Postfix(bool __state)
        {
            if (__state)
            {
                foreach (WeakReference<MovingGlowFlooder> activeGlowFlooder in MovingGlowFlooder.ActiveGlowFlooders)
                {
                    if (activeGlowFlooder.IsAlive)
                    {
                        activeGlowFlooder.Target.ReapplyGlow();
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(SectionLayer_LightingOverlay), nameof(SectionLayer_LightingOverlay.Regenerate))]
    public static class LightingOverlayRegenerate_Patch
    {
        public static Stopwatch _timer;
        public static Stopwatch Timer
        {
            get
            {
                if (_timer == null)
                {
                    _timer = new Stopwatch();
                }

                return _timer;
            }
        }

        [HarmonyPrefix]
        public static void Prefix()
        {
            Timer.Start();
        }

        [HarmonyPostfix]
        public static void Postfix()
        {
            Timer.Stop();
        }
    }
}