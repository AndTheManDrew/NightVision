using System.Linq;
using UnityEngine;
using Verse;

namespace NightVision {
    public static class HediffTab {
        private static Vector2 _hediffScrollPosition = Vector2.zero;
        private static int? _numberOfCustomHediffs;


        public static void Clear()
        {
            NightVision.HediffTab._hediffScrollPosition  = Vector2.zero;
            NightVision.HediffTab._numberOfCustomHediffs = null;
        }


        public static void DrawTab(Rect inRect)
        {
            int hediffcount = SettingsCache.GetAllHediffs.Count;

            if (_numberOfCustomHediffs == null)
            {
                _numberOfCustomHediffs =
                            Storage.HediffLightMods.Count(hlm => hlm.Value.IntSetting == VisionType.NVCustom);
            }

            inRect = inRect.AtZero();

            SettingsHelpers.DrawLightModifiersHeader(
                ref inRect,
                "NVHediffs".Translate(),
                "NVHediffNote".Translate() + " " + "NVHediffNoteCont".Translate()
            );

            float num = inRect.y + 3f;

            var viewRect = new Rect(
                inRect.x,
                inRect.y,
                inRect.width * 0.9f,
                hediffcount
                * (DrawConst.RowHeight + DrawConst.RowGap)
                + (float) _numberOfCustomHediffs * 100f
            );

            var rowRect = new Rect(inRect.x + 6f, num, inRect.width - 12f, DrawConst.RowHeight);
            Widgets.BeginScrollView(inRect, ref _hediffScrollPosition, viewRect);

            for (var i = 0; i < hediffcount; i++)
            {
                HediffDef hediffdef = SettingsCache.GetAllHediffs[i];
                rowRect.y = num;

                if (Storage.HediffLightMods.TryGetValue(hediffdef, out Hediff_LightModifiers hediffmods))
                {
                    _numberOfCustomHediffs +=
                                SettingsHelpers.DrawLightModifiersRow(
                                    hediffdef,
                                    hediffmods,
                                    rowRect,
                                    ref num,
                                    false
                                );
                }
                else
                {
                    Hediff_LightModifiers temp = Storage.AllEyeHediffs.Contains(hediffdef)
                                ? new Hediff_LightModifiers {AffectsEye = true}
                                : new Hediff_LightModifiers();

                    _numberOfCustomHediffs +=
                                SettingsHelpers.DrawLightModifiersRow(
                                    hediffdef,
                                    temp,
                                    rowRect,
                                    ref num,
                                    false
                                );

                    if (temp.IntSetting != VisionType.NVNone)
                    {
                        temp.InitialiseNewFromSettings(hediffdef);
                        Storage.HediffLightMods[hediffdef] = temp;
                    }
                }

                num += DrawConst.RowHeight + DrawConst.RowGap;

                if (i < hediffcount)
                {
                    Widgets.DrawLineHorizontal(
                        rowRect.x     + 6f,
                        num           - (DrawConst.RowGap / 2 - 0.5f),
                        rowRect.width - 12f
                    );
                }
            }

            Widgets.EndScrollView();
        }
    }
}