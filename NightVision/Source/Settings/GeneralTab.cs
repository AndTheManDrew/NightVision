using System.Diagnostics;
using UnityEngine;
using Verse;

namespace NightVision {
    public static class GeneralTab {
        public static bool _askToConfirmReset;
        public static Stopwatch confirmTimer = new Stopwatch();

        public static void Clear()
        {
            NightVision.GeneralTab._askToConfirmReset = false;
            NightVision.GeneralTab.confirmTimer.Reset();
        }

        public static void DrawTab(Rect inRect)
        {
            TextAnchor anchor    = Text.Anchor;
            float      rowHeight = Constants_Draw.GenRowHeight;

            var rowRect = new Rect(
                inRect.width  * 0.05f,
                inRect.height * 0.05f,
                inRect.width  * 0.9f,
                rowHeight
            );

            Text.Anchor = TextAnchor.MiddleLeft;

            Widgets.Label(rowRect, "NVVanillaMultiExp".Translate());
            rowRect.y += rowHeight + Constants_Draw.RowGap;
            Widgets.DrawLineHorizontal(rowRect.x + 24f, rowRect.y, rowRect.width - 48f);
            rowRect.y += Constants_Draw.RowGap;

            //Night Vision Settings

            Widgets.Label(
                rowRect,
                "NightVisionNVSettingDesc".Translate()
                            .ToLower()
                            .CapitalizeFirst()
            );

            rowRect.y += rowHeight + Constants_Draw.RowGap;

            SettingsCache.NVZeroCache = Widgets.HorizontalSlider(
                rowRect,
                (float) SettingsCache.NVZeroCache,
                (float) SettingsCache.MinCache,
                (float) SettingsCache.MaxCache,
                true,
                string.Format(
                    Str.ZeroMultiLabel,
                    SettingsCache.NVZeroCache
                ),
                string.Format(
                    Str.XLabel,
                    SettingsCache.MinCache
                ),
                string.Format(
                    Str.XLabel,
                    SettingsCache.MaxCache
                ),
                1
            );

            SettingsHelpers.DrawIndicator(
                rowRect,
                Constants_Calculations.DefaultZeroLightMultiplier,
                Constants_Calculations.NVDefaultOffsets[0],
                (float) SettingsCache.MinCache,
                (float) SettingsCache.MaxCache,
                IndicatorTex.DefIndicator
            );

            rowRect.y += rowHeight * 1.5f;

            SettingsCache.NVFullCache = Widgets.HorizontalSlider(
                rowRect,
                (float) SettingsCache.NVFullCache,
                (float) SettingsCache.MinCache,
                (float) SettingsCache.MaxCache,
                true,
                string.Format(
                    Str.FullMultiLabel,
                    SettingsCache.NVFullCache
                ),
                string.Format(
                    Str.XLabel,
                    SettingsCache.MinCache
                ),
                string.Format(
                    Str.XLabel,
                    SettingsCache.MaxCache
                ),
                1
            );

            SettingsHelpers.DrawIndicator(
                rowRect,
                Constants_Calculations.DefaultFullLightMultiplier,
                Constants_Calculations.NVDefaultOffsets[1],
                (float) SettingsCache.MinCache,
                (float) SettingsCache.MaxCache,
                IndicatorTex.DefIndicator
            );

            rowRect.y += rowHeight * 2f;
            Widgets.DrawLineHorizontal(rowRect.x + 24f, rowRect.y, rowRect.width - 48f);
            rowRect.y += Constants_Draw.RowGap;

            //Photosensitivity settings

            Widgets.Label(
                rowRect,
                "NightVisionPSSettingsDesc".Translate()
                            .ToLower()
                            .CapitalizeFirst()
            );

            rowRect.y += rowHeight * 1.5f;

            SettingsCache.PSZeroCache = Widgets.HorizontalSlider(
                rowRect,
                (float) SettingsCache.PSZeroCache,
                (float) SettingsCache.MinCache,
                (float) SettingsCache.MaxCache,
                true,
                string.Format(
                    Str.ZeroMultiLabel,
                    SettingsCache.PSZeroCache
                ),
                string.Format(
                    Str.XLabel,
                    SettingsCache.MinCache
                ),
                string.Format(
                    Str.XLabel,
                    SettingsCache.MaxCache
                ),
                1
            );

            SettingsHelpers.DrawIndicator(
                rowRect,
                Constants_Calculations.DefaultZeroLightMultiplier,
                Constants_Calculations.PSDefaultOffsets[0],
                (float) SettingsCache.MinCache,
                (float) SettingsCache.MaxCache,
                IndicatorTex.DefIndicator
            );

            rowRect.y += rowHeight * 1.5f;

            SettingsCache.PSFullCache = Widgets.HorizontalSlider(
                rowRect,
                (float) SettingsCache.PSFullCache,
                (float) SettingsCache.MinCache,
                (float) SettingsCache.MaxCache,
                true,
                string.Format(
                    Str.FullMultiLabel,
                    SettingsCache.PSFullCache
                ),
                string.Format(
                    Str.XLabel,
                    SettingsCache.MinCache
                ),
                string.Format(
                    Str.XLabel,
                    SettingsCache.MaxCache
                ),
                1
            );

            SettingsHelpers.DrawIndicator(
                rowRect,
                Constants_Calculations.DefaultFullLightMultiplier,
                Constants_Calculations.PSDefaultOffsets[1],
                (float) SettingsCache.MinCache,
                (float) SettingsCache.MaxCache,
                IndicatorTex.DefIndicator
            );

            rowRect.y += rowHeight * 2f;
            Widgets.DrawLineHorizontal(rowRect.x + 24f, rowRect.y, rowRect.width - 48f);
            rowRect.y += Constants_Draw.RowGap;

            
            Widgets.CheckboxLabeled(rowRect, "NightVisionFlareRaidEnabled".Translate(), ref NVGameComponent.FlareRaidIsEnabled);
            rowRect.y += rowHeight * 2f;
            Widgets.DrawLineHorizontal(rowRect.x + 24f, rowRect.y, rowRect.width - 48f);
            rowRect.y += Constants_Draw.RowGap;
 


            if (_askToConfirmReset)
            {
                if (!confirmTimer.IsRunning)
                {
                    confirmTimer.Start();
                }

                Color color = GUI.color;
                GUI.color = Color.red;

                if (Widgets.ButtonText(rowRect, "NVConfirmReset".Translate()) && confirmTimer.ElapsedMilliseconds > 500)
                {
                    Storage.ResetAllSettings();
                    confirmTimer.Reset();
                }

                GUI.color = color;

                if (confirmTimer.ElapsedMilliseconds > 5000)
                {
                    _askToConfirmReset = false;
                    confirmTimer.Reset();
                }
            }
            else
            {
                _askToConfirmReset = Widgets.ButtonText(rowRect, "NVReset".Translate());
            }

            Text.Anchor = anchor;
        }
    }
}