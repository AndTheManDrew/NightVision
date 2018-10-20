// Nightvision NightVision StatPartGlow_ActiveFor.cs
// 
// 17 10 2018
// 
// 17 10 2018

using Harmony;
using RimWorld;
using Verse;

namespace NightVision.Harmony
{
    [HarmonyPatch(typeof(StatPart_Glow), "ActiveFor")]
    public static class StatPartGlow_ActiveFor
    {
        public static void Postfix(
            ref Thing t,
            ref bool  __result
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