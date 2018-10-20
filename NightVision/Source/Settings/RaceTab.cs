using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace NightVision {
    public static class RaceTab {
        private static int?    _numberOfCustomRaces;
        private static Vector2 _raceScrollPosition = Vector2.zero;

        public static void Clear()
        {
            NightVision.RaceTab._numberOfCustomRaces = null;
            NightVision.RaceTab._raceScrollPosition = Vector2.zero;
        }


        public static void DrawTab(Rect inRect)
        {
            int raceCount = Storage.RaceLightMods.Count;

            if (_numberOfCustomRaces == null)
            {
                _numberOfCustomRaces =
                            Storage.RaceLightMods.Count(rlm => rlm.Value.IntSetting == VisionType.NVCustom);
            }

            inRect = inRect.AtZero();
            SettingsHelpers.DrawLightModifiersHeader(ref inRect, "NVRaces".Translate(), "NVRaceNote".Translate());

            //#region Tweaks

            //Constants.RowHeight = Widgets.HorizontalSlider(new Rect(inRect.x, inRect.y, inRect.width, Constants.RowHeight),
            //    Constants.RowHeight,
            //    -100f,
            //    +100f,
            //    true,
            //    $"Tweak RowHeight: {Constants.RowHeight}");
            //inRect.y += 50f;


            //#endregion
            float num = inRect.y + 3f;

            var viewRect = new Rect(
                inRect.x,
                inRect.y,
                inRect.width * 0.9f,
                raceCount
                * (DrawConst.RowHeight + DrawConst.RowGap)
                + (float) _numberOfCustomRaces * 100f
            );

            var rowRect = new Rect(inRect.x + 6f, num, inRect.width - 12f, DrawConst.RowHeight);
            Widgets.BeginScrollView(inRect, ref _raceScrollPosition, viewRect);
            var count = 0;

            foreach (KeyValuePair<ThingDef, Race_LightModifiers> kvp in Storage.RaceLightMods)
            {
                Color givenColor = GUI.color;
                rowRect.y = num;

                if (!kvp.Value.ShouldShowInSettings)
                {
                    if (!Prefs.DevMode)
                    {
                        continue;
                    }

                    GUI.color = Color.grey;
                    Widgets.Label(rowRect.TopPartPixels(20f), "NVDevModeOnly".Translate());
                    rowRect.y += 20;
                    num       += 20;
                }

                _numberOfCustomRaces +=
                            SettingsHelpers.DrawLightModifiersRow(kvp.Key, kvp.Value, rowRect, ref num, true);

                if (!kvp.Value.ShouldShowInSettings)
                {
                    GUI.color = givenColor;
                }

                count++;
                num += DrawConst.RowHeight + DrawConst.RowGap;

                if (count < raceCount)
                {
                    Widgets.DrawLineHorizontal(rowRect.x + 6f, num - 5.5f, rowRect.width - 12f);
                }
            }

            Widgets.EndScrollView();
        }
    }
}