using System.Collections.Generic;
using System.Linq;
using Harmony;
using RimWorld;
using Verse;

namespace NightVision.Harmony {
    [HarmonyPatch(typeof(HediffDef), nameof(HediffDef.SpecialDisplayStats))]
    public static class HediffDef_SpecialDisplayStats {
        public static IEnumerable<StatDrawEntry> Postfix(
            IEnumerable<StatDrawEntry> statdrawentries,
            HediffDef                  __instance
        )
        {
            List<StatDrawEntry> statDrawEntryList = statdrawentries.ToList();

            foreach (StatDrawEntry sDE in statDrawEntryList)
            {
                yield return sDE;
            }

            if (Storage.HediffLightMods.TryGetValue(__instance, out Hediff_LightModifiers hlm)
                && hlm.HasAnyModifier())
            {
                VisionType vt = hlm.Setting;

                if (vt < VisionType.NVCustom)
                {
                    yield return new StatDrawEntry(
                        Defs_Rimworld.BasicStats,
                        "NVGrantsVisionType".Translate(),
                        vt.ToString().Translate(),
                        0,
                        hlm.AffectsEye ? "NVHediffQualifier".Translate() : ""
                    );
                }
                else
                {
                    yield return new StatDrawEntry(
                        Defs_Rimworld.BasicStats,
                        "NVGrantsVisionType".Translate(),
                        vt.ToString(),
                        0,
                        hlm.AffectsEye ? "NVHediffQualifier".Translate() : ""
                    );

                    yield return new StatDrawEntry(
                        Defs_Rimworld.BasicStats,
                        Defs_NightVision.NightVision,
                        hlm[0],
                        StatRequest.ForEmpty(),
                        ToStringNumberSense.Offset
                    );

                    yield return new StatDrawEntry(
                        Defs_Rimworld.BasicStats,
                        Defs_NightVision.LightSensitivity,
                        hlm[1],
                        StatRequest.ForEmpty(),
                        ToStringNumberSense.Offset
                    );
                }
            }
        }
    }
}