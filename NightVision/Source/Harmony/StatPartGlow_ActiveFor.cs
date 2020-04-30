// Nightvision NightVision StatPartGlow_ActiveFor.cs
// 
// 17 10 2018
// 
// 17 10 2018

#if HARM12
using Harmony;
#else
using HarmonyLib;
#endif
using RimWorld;
using Verse;

namespace NightVision.Harmony
{
    [HarmonyPatch(typeof(StatPart_Glow), "ActiveFor")]
    public static class StatPartGlow_ActiveFor
    {
        [HarmonyPostfix]
        public static void Postfix(
            ref Thing t,
            ref bool __result
        )
        {
            if (__result || !t.Spawned) { }
            else
            {
                if (t is Pawn pawn && pawn.TryGetComp<Comp_NightVision>() != null)
                {
                    __result = true;
                }
            }
        }
    }
}