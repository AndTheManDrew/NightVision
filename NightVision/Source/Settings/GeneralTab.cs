using System.Diagnostics;
using UnityEngine;
using Verse;

namespace NightVision {
    public class GeneralTab {
        private bool _askToConfirmReset;
        private Stopwatch confirmTimer = new Stopwatch();

        public  void Clear()
        {
            _askToConfirmReset = false;
            confirmTimer.Reset();
        }

        public void DrawTab(Rect inRect)
        {
            TextAnchor anchor    = Text.Anchor;
            float      rowHeight = Constants_Draw.GenRowHeight;
            var cache = Mod.Cache;

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

            cache.NVZeroCache = Widgets.HorizontalSlider(
                rowRect,
                (float) cache.NVZeroCache,
                (float) cache.MinCache,
                cache.MaxCache.Value,
                true,
                string.Format(
                    Str.ZeroMultiLabel,
                    cache.NVZeroCache
                ),
                string.Format(
                    Str.XLabel,
                    cache.MinCache
                ),
                string.Format(
                    Str.XLabel,
                    cache.MaxCache
                ),
                1
            );

            SettingsHelpers.DrawIndicator(
                rowRect,
                Constants_Calculations.DefaultZeroLightMultiplier,
                Constants_Calculations.NVDefaultOffsets[0],
                (float) cache.MinCache,
                (float) cache.MaxCache,
                IndicatorTex.DefIndicator
            );

            rowRect.y += rowHeight * 1.5f;

            cache.NVFullCache = Widgets.HorizontalSlider(
                rowRect,
                (float) cache.NVFullCache,
                (float) cache.MinCache,
                (float) cache.MaxCache,
                true,
                string.Format(
                    Str.FullMultiLabel,
                    cache.NVFullCache
                ),
                string.Format(
                    Str.XLabel,
                    cache.MinCache
                ),
                string.Format(
                    Str.XLabel,
                    cache.MaxCache
                ),
                1
            );

            SettingsHelpers.DrawIndicator(
                rowRect,
                Constants_Calculations.DefaultFullLightMultiplier,
                Constants_Calculations.NVDefaultOffsets[1],
                (float) cache.MinCache,
                (float) cache.MaxCache,
                IndicatorTex.DefIndicator
            );

            rowRect.y += rowHeight * 2f;
            Widgets.DrawLineHorizontal(rowRect.x + 24f, rowRect.y, rowRect.width - 48f);
            rowRect.y += Constants_Draw.RowGap;

            // Photosensitivity settings

            Widgets.Label(
                rowRect,
                "NightVisionPSSettingsDesc".Translate()
                            .ToLower()
                            .CapitalizeFirst()
            );

            rowRect.y += rowHeight * 1.5f;

            cache.PSZeroCache = Widgets.HorizontalSlider(
                rowRect,
                (float) cache.PSZeroCache,
                (float) cache.MinCache,
                (float) cache.MaxCache,
                true,
                string.Format(
                    Str.ZeroMultiLabel,
                    cache.PSZeroCache
                ),
                string.Format(
                    Str.XLabel,
                    cache.MinCache
                ),
                string.Format(
                    Str.XLabel,
                    cache.MaxCache
                ),
                1
            );

            SettingsHelpers.DrawIndicator(
                rowRect,
                Constants_Calculations.DefaultZeroLightMultiplier,
                Constants_Calculations.PSDefaultOffsets[0],
                (float) cache.MinCache,
                (float) cache.MaxCache,
                IndicatorTex.DefIndicator
            );

            rowRect.y += rowHeight * 1.5f;

            cache.PSFullCache = Widgets.HorizontalSlider(
                rowRect,
                (float) cache.PSFullCache,
                (float) cache.MinCache,
                (float) cache.MaxCache,
                true,
                string.Format(
                    Str.FullMultiLabel,
                    cache.PSFullCache
                ),
                string.Format(
                    Str.XLabel,
                    cache.MinCache
                ),
                string.Format(
                    Str.XLabel,
                    cache.MaxCache
                ),
                1
            );

            SettingsHelpers.DrawIndicator(
                rowRect,
                Constants_Calculations.DefaultFullLightMultiplier,
                Constants_Calculations.PSDefaultOffsets[1],
                (float) cache.MinCache,
                (float) cache.MaxCache,
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
                    Mod.Store.ResetAllSettings();
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