using RimWorld;
using UnityEngine;
using Verse;

namespace NVIncidents
{
    public class GameCondition_Darkness : GameCondition
    {
        public override float SkyTargetLerpFactor(Map map)
        {
            return 1f;
        }

        // Token: 0x06000D5A RID: 3418 RVA: 0x00074058 File Offset: 0x00072458
        public override SkyTarget? SkyTarget(Map map)
        {
            SkyColorSet colorSet     = new SkyColorSet(new Color(SkyColorR, SkyColorG, SkyColorB), new Color(SkyShadowR, SkyShadowG, SkyShadowB), new Color(SkyOverlayR, SkyOverlayG, SkyOverlayB), SkySaturation);
            float       glow         = 0f;
            return new SkyTarget?(new SkyTarget(glow, colorSet, 1f, 1f));
        }

        [TweakValue("NightVision", 0 , 1)]
        public static float SkyColorR = 1;
        [TweakValue("NightVision", 0 , 1)]
        public static float SkyColorG = 1;
        [TweakValue("NightVision", 0 , 1)]
        public static float SkyColorB = 1;

        [TweakValue("NightVision", 0 , 1)]
        public static float SkyShadowR = 0.5f;
        [TweakValue("NightVision", 0 , 1)]
        public static float SkyShadowG = 0.5f   ;
        [TweakValue("NightVision", 0 , 1)]
        public static float SkyShadowB = 0.5f;

        [TweakValue("NightVision", 0 , 1)]
        public static float SkyOverlayR = 0;
        [TweakValue("NightVision", 0 , 1)]
        public static float SkyOverlayG = 0;
        [TweakValue("NightVision", 0 , 1)]
        public static float SkyOverlayB = 0;

        [TweakValue("NightVision", 0, 1)]
        public static float SkySaturation = 0.2f;
    }
}
