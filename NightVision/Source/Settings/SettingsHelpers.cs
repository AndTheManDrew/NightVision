using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace NightVision
{
    internal static class SettingsHelpers
    {
        private static Dictionary<Def, string> _tipStringHolder;

        public static Dictionary<Def, string> TipStringHolder
        {
            get
                => SettingsHelpers._tipStringHolder
                   ?? (SettingsHelpers._tipStringHolder = new Dictionary<Def, string>());
            set => SettingsHelpers._tipStringHolder = value;
        }

        public static float ModToMultiPercent(
                        float mod,
                        bool  isZeroLight
                    )
            => (float) Math.Round(
                                  100f
                                  * (mod
                                     + (isZeroLight
                                                 ? Constants.DefaultZeroLightMultiplier
                                                 : Constants.DefaultFullLightMultiplier))
                                 );

        public static float MultiPercentToMod(
                        float multipercent,
                        bool  isZeroLight
                    )
            => (float) Math.Round(multipercent / 100f, Constants.NumberOfDigits)
               - (isZeroLight
                           ? Constants.DefaultZeroLightMultiplier
                           : Constants.DefaultFullLightMultiplier);

        public static void DrawIndicator(
                        Rect      rowRect,
                        float     baseVal,
                        float     modVal,
                        float     min,
                        float     max,
                        Texture2D indicator
                    )
        {
            float posOfDefault = rowRect.x
                                 + 6f
                                 + (rowRect.width            - 12f)
                                 * ((baseVal + modVal) * 100 - min)
                                 / (max - min);

            rowRect.position = new Vector2(
                                           posOfDefault - 0.5f           * Constants.IndicatorSize,
                                           rowRect.y    + rowRect.height * 0.95f
                                          );

            rowRect.width  = Constants.IndicatorSize;
            rowRect.height = Constants.IndicatorSize;
            Widgets.DrawTextureFitted(rowRect, indicator, 1);
        }

        private static void DrawIndicators(
                        Rect               zeroRect,
                        Rect               fullRect,
                        LightModifiersBase lightModifiers,
                        float              minZero,
                        float              maxZero,
                        float              minFull,
                        float              maxFull
                    )
        {
            //int eyeCount = lightModifiers is Race_LightModifiers rlm ? rlm.EyeCount : 1; 
            //Draw indicators on zero light rect
            SettingsHelpers.DrawIndicator(
                                          zeroRect,
                                          Constants.DefaultZeroLightMultiplier,
                                          LightModifiersBase.PSLightModifiers[0],
                                          minZero,
                                          maxZero,
                                          IndicatorTex.PsIndicator
                                         );

            SettingsHelpers.DrawIndicator(
                                          zeroRect,
                                          Constants.DefaultZeroLightMultiplier,
                                          LightModifiersBase.NVLightModifiers[0],
                                          minZero,
                                          maxZero,
                                          IndicatorTex.NvIndicator
                                         );

            SettingsHelpers.DrawIndicator(
                                          zeroRect,
                                          Constants.DefaultZeroLightMultiplier,
                                          lightModifiers.DefaultOffsets[0],
                                          minZero,
                                          maxZero,
                                          IndicatorTex.DefIndicator
                                         );

            //Draw indicators on full light rect
            SettingsHelpers.DrawIndicator(
                                          fullRect,
                                          Constants.DefaultFullLightMultiplier,
                                          LightModifiersBase.PSLightModifiers[1],
                                          minFull,
                                          maxFull,
                                          IndicatorTex.PsIndicator
                                         );

            SettingsHelpers.DrawIndicator(
                                          fullRect,
                                          Constants.DefaultFullLightMultiplier,
                                          LightModifiersBase.NVLightModifiers[1],
                                          minFull,
                                          maxFull,
                                          IndicatorTex.NvIndicator
                                         );

            SettingsHelpers.DrawIndicator(
                                          fullRect,
                                          Constants.DefaultFullLightMultiplier,
                                          lightModifiers.DefaultOffsets[1],
                                          minFull,
                                          maxFull,
                                          IndicatorTex.DefIndicator
                                         );
        }

        public static string GetTipString(
                        Def                def,
                        LightModifiersBase lightModifiers
                    )
        {
            if (SettingsHelpers.TipStringHolder == null)
            {
                SettingsHelpers.TipStringHolder = new Dictionary<Def, string>();
            }

            if (SettingsHelpers.TipStringHolder.TryGetValue(def, out string tip))
            {
                return tip;
            }

            string result = /*Confusing as it shows givenv or null ps and doesnt get updated immediately((ThingDef)def).DescriptionDetailed ??*/ def.description ?? def.LabelCap;

            if (lightModifiers is Hediff_LightModifiers hediffMods)
            {
                if (hediffMods.AffectsEye)
                {
                    result += "\n" + "NVHediffQualifier".Translate();
                }

                if (hediffMods.AutoAssigned)
                {
                    result += "\n" + "NVHediffAutoAssigned".Translate();
                }
                else if (Math.Abs(lightModifiers.DefaultOffsets[0])    > 0.001f
                         || Math.Abs(lightModifiers.DefaultOffsets[1]) > 0.001f)
                {
                    result += "\n"
                              + "NVLoadedFromFile".Translate(
                                                             Hediff_LightModifiers.GetSetting(hediffMods).ToString().Translate(),
                                                             lightModifiers.DefaultOffsets.ToStringSafeEnumerable()
                                                            );
                }
            }
            else if (lightModifiers is Race_LightModifiers rlm
                     && (Math.Abs(lightModifiers.DefaultOffsets[0])    > 0.001f
                         || Math.Abs(lightModifiers.DefaultOffsets[1]) > 0.001f))
            {
                result += "\n"
                          + "NVLoadedFromFile".Translate(
                                                         Race_LightModifiers.GetSetting(rlm).ToString().Translate(),
                                                         rlm.DefaultOffsets.ToStringSafeEnumerable()
                                                        );
            }

            SettingsHelpers.TipStringHolder[def] = result;

            return result;
        }

        public static void DrawLightModifiersHeader(
                        ref Rect inRect,
                        string   label,
                        string   tooltip
                    )
        {
            var headerRect = new Rect(inRect.x + 6f, inRect.y, inRect.width - 12f, inRect.height * 0.1f);

            float labelwidth = headerRect.width * 0.3f;
            var   labelRect  = new Rect(headerRect.x, headerRect.y, labelwidth, headerRect.height);
            float xSettings  = headerRect.x + labelwidth + 1f + headerRect.width * 0.05f;

            var settingsRect = new Rect(
                                        xSettings,
                                        headerRect.y,
                                        headerRect.xMax - xSettings,
                                        headerRect.height
                                       );

            GameFont font = Text.Font;
            Text.Font = GameFont.Medium;
            Widgets.Label(labelRect, label);
            TooltipHandler.TipRegion(labelRect, new TipSignal(tooltip));
            Widgets.Label(settingsRect.TopPart(0.6f), "NVGlowModOptions".Translate());
            float num = settingsRect.width / 3;
            settingsRect       = settingsRect.BottomPart(0.4f);
            Text.Font          = GameFont.Tiny;
            settingsRect.width = num;

            Widgets.Label(
                          settingsRect,
                          new GUIContent(
                                         "default".Translate().ToLower().CapitalizeFirst(),
                                         IndicatorTex.DefIndicator
                                        )
                         );

            settingsRect.x += num;

            Widgets.Label(
                          settingsRect,
                          new GUIContent(
                                         "NVNightVision".Translate().ToLower().CapitalizeFirst(),
                                         IndicatorTex.NvIndicator
                                        )
                         );

            settingsRect.x += num;

            Widgets.Label(
                          settingsRect,
                          new GUIContent(
                                         "NVPhotosensitivity".Translate().ToLower().CapitalizeFirst(),
                                         IndicatorTex.PsIndicator
                                        )
                         );

            Text.Font = font;

            Widgets.DrawLineHorizontal(
                                       headerRect.x                           + 10f,
                                       headerRect.yMax + Constants.RowGap / 4 - 0.5f,
                                       headerRect.width                       - 20f
                                      );

            inRect.yMin += headerRect.height + Constants.RowGap / 2;
        }

        /// <summary>
        ///     0 if LightModifiers is unchanged, 1 if LightModifiers changed to custom, -1 if LightModifiers changed from custom
        /// </summary>
        /// <param name="num">the y-coord of the rect: is increased if rect needs more space</param>
        public static int DrawLightModifiersRow(
                        Def                def,
                        LightModifiersBase lightMods,
                        Rect               rowRect,
                        ref float          num,
                        bool               isRace
                    )
        {
            var   result     = 0;
            float labelwidth = rowRect.width * 0.3f;
            var   labelRect  = new Rect(rowRect.x, rowRect.y, labelwidth, rowRect.height);

            float buttonwidth = rowRect.width * 0.14f;
            float buttongap   = rowRect.width * 0.025f;
            float x           = rowRect.x + labelwidth + buttongap;

            Widgets.DrawAltRect(labelRect.ContractedBy(2f));
            Widgets.Label(labelRect, def.LabelCap);

            TooltipHandler.TipRegion(
                                     labelRect,
                                     new TipSignal(
                                                   SettingsHelpers.GetTipString(def, lightMods),
                                                   labelRect.GetHashCode()
                                                  )
                                    );

            Widgets.DrawLineVertical(x, rowRect.y, rowRect.height);

            //LightModifiers.Options =  enum: default = 0; nightvis = 1; photosens = 2; custom = 3
            for (var i = 0; i < 4; i++)
            {
                var iOption = (VisionType) Enum.ToObject(typeof(VisionType), i);
                x += buttongap;
                var buttonRect = new Rect(x, rowRect.y + 6f, buttonwidth, rowRect.height - 12f);

                if (iOption == lightMods.Setting)
                {
                    Color color = GUI.color;
                    GUI.color = Color.yellow;
                    Widgets.DrawBox(buttonRect.ExpandedBy(2f));
                    GUI.color = color;
                    Widgets.DrawAtlas(buttonRect, Widgets.ButtonSubtleAtlas);
                    Widgets.Label(buttonRect, iOption.ToString().Translate());
                }
                else
                {
                    bool changesetting =
                                Widgets.ButtonText(buttonRect, iOption.ToString().Translate());

                    if (changesetting)
                    {
                        if (lightMods.IsCustom())
                        {
                            result = -1;
                        }

                        lightMods.ChangeSetting(iOption);

                        if (lightMods.IsCustom())
                        {
                            result = 1;
                        }
                    }
                }

                x += buttonwidth;
            }

            if (lightMods.IsCustom())
            {
                num += Constants.RowHeight + Constants.RowGap;

                var topRect = new Rect(
                                       labelRect.xMax + 2 * buttongap,
                                       num,
                                       rowRect.width - labelRect.width - 60f,
                                       Constants.RowHeight / 2
                                      );

                var bottomRect = new Rect(
                                          labelRect.xMax                  + 2                * buttongap,
                                          topRect.yMax                    + Constants.RowGap * 2,
                                          rowRect.width - labelRect.width - 60f,
                                          Constants.RowHeight / 2
                                         );

                var explanationRect = new Rect(
                                               labelRect.xMax - labelRect.width * 0.95f,
                                               num,
                                               labelRect.width * 0.9f,
                                               Constants.RowHeight
                                              );

                Widgets.DrawLineVertical(
                                         labelRect.xMax          + buttongap,
                                         rowRect.y               + rowRect.height,
                                         Constants.RowHeight * 2 - Constants.RowGap * 0.5f
                                        );


                var zeroModAsPercent = (float) Math.Round(
                                                          (lightMods.Offsets[0]
                                                           + (isRace ? Constants.DefaultZeroLightMultiplier : 0))
                                                          * 100
                                                         );

                var fullModAsPercent = (float) Math.Round(
                                                          (lightMods.Offsets[1]
                                                           + (isRace ? Constants.DefaultFullLightMultiplier : 0))
                                                          * 100
                                                         );

                if (isRace)
                {
                    GameFont font = Text.Font;
                    Text.Font = GameFont.Tiny;

                    Widgets.Label(
                                  explanationRect,
                                  "NVMoveWorkSpeedMultipliers".Translate("").Trim().CapitalizeFirst()
                                  + "\n"
                                  + "NVRaceQualifier"
                                    .Translate()
                                    .CapitalizeFirst()
                                 );

                    Text.Font = font;
                    float min;
                    float max;

                    if (((Race_LightModifiers) lightMods).CanCheat)
                    {
                        min = (float) Math.Round(Storage.LowestCap  * 100);
                        max = (float) Math.Round(Storage.HighestCap * 100);
                    }
                    else
                    {
                        min = (float) SettingsCache.MinCache;
                        max = (float) SettingsCache.MaxCache;
                    }

                    zeroModAsPercent = Widgets.HorizontalSlider(
                                                                topRect,
                                                                zeroModAsPercent,
                                                                min,
                                                                max,
                                                                true,
                                                                string.Format(
                                                                              Constants.ZeroMultiLabel,
                                                                              zeroModAsPercent
                                                                             ),
                                                                string.Format(Constants.XLabel, min),
                                                                string.Format(Constants.XLabel, max),
                                                                1
                                                               );

                    fullModAsPercent = Widgets.HorizontalSlider(
                                                                bottomRect,
                                                                fullModAsPercent,
                                                                min,
                                                                max,
                                                                true,
                                                                string.Format(
                                                                              Constants.FullMultiLabel,
                                                                              fullModAsPercent
                                                                             ),
                                                                string.Format(Constants.XLabel, min),
                                                                string.Format(Constants.XLabel, max),
                                                                1
                                                               );

                    SettingsHelpers.DrawIndicators(topRect, bottomRect, lightMods, min, max, min, max);
                }
                else
                {
                    GameFont font = Text.Font;
                    Text.Font = GameFont.Tiny;

                    Widgets.Label(
                                  explanationRect,
                                  "NVMoveWorkSpeedModifiers".Translate(def.LabelCap).CapitalizeFirst()
                                 );

                    Text.Font = font;

                    zeroModAsPercent = Widgets.HorizontalSlider(
                                                                topRect,
                                                                zeroModAsPercent,
                                                                (float) SettingsCache.MinCache - 80,
                                                                (float) SettingsCache.MaxCache - 80,
                                                                true,
                                                                string.Format(Constants.ZeroLabel, zeroModAsPercent),
                                                                string.Format(
                                                                              Constants.Alabel,
                                                                              SettingsCache.MinCache - 80
                                                                             ),
                                                                string.Format(
                                                                              Constants.Alabel,
                                                                              SettingsCache.MaxCache - 80
                                                                             ),
                                                                1
                                                               );


                    fullModAsPercent = Widgets.HorizontalSlider(
                                                                bottomRect,
                                                                fullModAsPercent,
                                                                (float) SettingsCache.MinCache - 100,
                                                                (float) SettingsCache.MaxCache - 100,
                                                                true,
                                                                string.Format(Constants.FullLabel, fullModAsPercent),
                                                                string.Format(
                                                                              Constants.Alabel,
                                                                              SettingsCache.MinCache - 100
                                                                             ),
                                                                string.Format(
                                                                              Constants.Alabel,
                                                                              SettingsCache.MaxCache - 100
                                                                             ),
                                                                1
                                                               );

                    SettingsHelpers.DrawIndicators(
                                                   topRect,
                                                   bottomRect,
                                                   lightMods,
                                                   (float) SettingsCache.MinCache,
                                                   (float) SettingsCache.MaxCache,
                                                   (float) SettingsCache.MinCache,
                                                   (float) SettingsCache.MaxCache
                                                  );
                }

                if (!Mathf.Approximately(zeroModAsPercent / 100, lightMods.Offsets[0]))
                {
                    lightMods[0] = zeroModAsPercent / 100
                                   - (isRace ? Constants.DefaultZeroLightMultiplier : 0);
                }

                if (!Mathf.Approximately(fullModAsPercent / 100, lightMods.Offsets[1]))
                {
                    lightMods[1] = fullModAsPercent / 100
                                   - (isRace ? Constants.DefaultFullLightMultiplier : 0);
                }

                num += Constants.RowHeight * 0.9f /*+ rowGap*/;
            }

            return result;
        }
    }
}