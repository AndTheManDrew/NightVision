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
            float      rowHeight = DrawConst.GenRowHeight;

            var rowRect = new Rect(
                inRect.width  * 0.05f,
                inRect.height * 0.05f,
                inRect.width  * 0.9f,
                rowHeight
            );

            Text.Anchor = TextAnchor.MiddleLeft;

            Widgets.Label(rowRect, "NVVanillaMultiExp".Translate());
            rowRect.y += rowHeight + DrawConst.RowGap;
            Widgets.DrawLineHorizontal(rowRect.x + 24f, rowRect.y, rowRect.width - 48f);
            rowRect.y += DrawConst.RowGap;

            //Night Vision Settings

            Widgets.Label(
                rowRect,
                "NVMoveWorkSpeedMultipliers"
                            .Translate(VisionType.NVNightVision.ToString().Translate())
                            .ToLower()
                            .CapitalizeFirst()
            );

            rowRect.y += rowHeight + DrawConst.RowGap;

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
                CalcConstants.DefaultZeroLightMultiplier,
                CalcConstants.NVDefaultOffsets[0],
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
                CalcConstants.DefaultFullLightMultiplier,
                CalcConstants.NVDefaultOffsets[1],
                (float) SettingsCache.MinCache,
                (float) SettingsCache.MaxCache,
                IndicatorTex.DefIndicator
            );

            rowRect.y += rowHeight * 2f;
            Widgets.DrawLineHorizontal(rowRect.x + 24f, rowRect.y, rowRect.width - 48f);
            rowRect.y += DrawConst.RowGap;

            //Photosensitivity settings

            Widgets.Label(
                rowRect,
                "NVMoveWorkSpeedMultipliers"
                            .Translate(VisionType.NVPhotosensitivity.ToString().Translate())
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
                CalcConstants.DefaultZeroLightMultiplier,
                CalcConstants.PSDefaultOffsets[0],
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
                CalcConstants.DefaultFullLightMultiplier,
                CalcConstants.PSDefaultOffsets[1],
                (float) SettingsCache.MinCache,
                (float) SettingsCache.MaxCache,
                IndicatorTex.DefIndicator
            );

            rowRect.y += rowHeight * 2f;
            Widgets.DrawLineHorizontal(rowRect.x + 24f, rowRect.y, rowRect.width - 48f);
            rowRect.y += DrawConst.RowGap;


            if (Settings.CEDetected)
            {
                Widgets.CheckboxLabeled(rowRect, "EnableNVForCE".Translate(), ref Storage.NVEnabledForCE);
                rowRect.y += rowHeight * 2f;
                Widgets.DrawLineHorizontal(rowRect.x + 24f, rowRect.y, rowRect.width - 48f);
                rowRect.y += DrawConst.RowGap;
            }


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
                    //Log.Message("NightVision.DrawSettings.DrawTab: NVConfirm");

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