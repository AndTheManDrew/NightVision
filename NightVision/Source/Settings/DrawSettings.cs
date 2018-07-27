using System;
using System.Collections.Generic;
using System.Linq;
using NightVision;
using RimWorld;
using UnityEngine;
using Verse;

internal static class DrawSettings
    {
        public static List<Pawn> _allPawns;
        public static Vector2    _apparelScrollPosition = Vector2.zero;
        public static Vector2    _debugScrollPos        = Vector2.zero;
        public static Vector2    _hediffScrollPosition  = Vector2.zero;
        public static float      _maxY;
        public static int?       _numberOfCustomHediffs;
        public static int?       _numberOfCustomRaces;
        public static Vector2    _raceScrollPosition = Vector2.zero;

        public static bool LogPawnComps;

        public static void GeneralTab(
            Rect inRect)
            {
                //TODO GeneralTab: Add reset defaults and thought settings
                //TODO GeneralTab: Move cap and multiplier settings to their own tab
                TextAnchor anchor    = Text.Anchor;
                float      rowHeight = Constants.RowHeight * 0.6f;
                var rowRect = new Rect(inRect.width * 0.05f,
                    inRect.height * 0.05f,
                    inRect.width * 0.9f,
                    rowHeight);
                Text.Anchor = TextAnchor.MiddleLeft;

                Widgets.Label(rowRect, "NVVanillaMultiExp".Translate());
                rowRect.y += rowHeight + Constants.RowGap;
                Widgets.DrawLineHorizontal(rowRect.x + 24f, rowRect.y, rowRect.width - 48f);
                rowRect.y += Constants.RowGap;

                //Night Vision Settings

                Widgets.Label(rowRect,
                    "NVMoveWorkSpeedMultipliers"
                                .Translate(VisionType.NVNightVision.ToString().Translate())
                                .ToLower()
                                .CapitalizeFirst());
                rowRect.y += rowHeight + Constants.RowGap;
                SettingsCache.NVZeroCache = Widgets.HorizontalSlider(rowRect,
                    (float) SettingsCache.NVZeroCache,
                    (float) SettingsCache.MinCache,
                    (float) SettingsCache.MaxCache,
                    true,
                    string.Format(Constants.ZeroMultiLabel, SettingsCache.NVZeroCache),
                    string.Format(Constants.XLabel,         SettingsCache.MinCache),
                    string.Format(Constants.XLabel,         SettingsCache.MaxCache),
                    1);
                SettingsHelpers.DrawIndicator(rowRect,
                    Constants.DefaultZeroLightMultiplier,
                    Constants.NVDefaultOffsets[0],
                    (float) SettingsCache.MinCache,
                    (float) SettingsCache.MaxCache,
                    IndicatorTex.DefIndicator);
                rowRect.y += rowHeight * 1.5f;
                SettingsCache.NVFullCache = Widgets.HorizontalSlider(rowRect,
                    (float) SettingsCache.NVFullCache,
                    (float) SettingsCache.MinCache,
                    (float) SettingsCache.MaxCache,
                    true,
                    string.Format(Constants.FullMultiLabel, SettingsCache.NVFullCache),
                    string.Format(Constants.XLabel,         SettingsCache.MinCache),
                    string.Format(Constants.XLabel,         SettingsCache.MaxCache),
                    1);
                SettingsHelpers.DrawIndicator(rowRect,
                    Constants.DefaultFullLightMultiplier,
                    Constants.NVDefaultOffsets[1],
                    (float) SettingsCache.MinCache,
                    (float) SettingsCache.MaxCache,
                    IndicatorTex.DefIndicator);
                rowRect.y += rowHeight * 2f;
                Widgets.DrawLineHorizontal(rowRect.x + 24f, rowRect.y, rowRect.width - 48f);
                rowRect.y += Constants.RowGap;

                //Photosensitivity settings

                Widgets.Label(rowRect,
                    "NVMoveWorkSpeedMultipliers"
                                .Translate(VisionType.NVPhotosensitivity.ToString().Translate())
                                .ToLower()
                                .CapitalizeFirst());
                rowRect.y += rowHeight * 1.5f;
                SettingsCache.PSZeroCache = Widgets.HorizontalSlider(rowRect,
                    (float) SettingsCache.PSZeroCache,
                    (float) SettingsCache.MinCache,
                    (float) SettingsCache.MaxCache,
                    true,
                    string.Format(Constants.ZeroMultiLabel, SettingsCache.PSZeroCache),
                    string.Format(Constants.XLabel,         SettingsCache.MinCache),
                    string.Format(Constants.XLabel,         SettingsCache.MaxCache),
                    1);
                SettingsHelpers.DrawIndicator(rowRect,
                    Constants.DefaultZeroLightMultiplier,
                    Constants.PSDefaultOffsets[0],
                    (float) SettingsCache.MinCache,
                    (float) SettingsCache.MaxCache,
                    IndicatorTex.DefIndicator);

                rowRect.y += rowHeight * 1.5f;
                SettingsCache.PSFullCache = Widgets.HorizontalSlider(rowRect,
                    (float) SettingsCache.PSFullCache,
                    (float) SettingsCache.MinCache,
                    (float) SettingsCache.MaxCache,
                    true,
                    string.Format(Constants.FullMultiLabel, SettingsCache.PSFullCache),
                    string.Format(Constants.XLabel,         SettingsCache.MinCache),
                    string.Format(Constants.XLabel,         SettingsCache.MaxCache),
                    1);
                SettingsHelpers.DrawIndicator(rowRect,
                    Constants.DefaultFullLightMultiplier,
                    Constants.PSDefaultOffsets[1],
                    (float) SettingsCache.MinCache,
                    (float) SettingsCache.MaxCache,
                    IndicatorTex.DefIndicator);

                rowRect.y += rowHeight * 2f;
                Widgets.DrawLineHorizontal(rowRect.x + 24f, rowRect.y, rowRect.width - 48f);
                rowRect.y += Constants.RowGap;

                //Multiplier Limits
                Widgets.CheckboxLabeled(rowRect, "NVCustomCapsEnabled".Translate(), ref Storage.CustomCapsEnabled);
                if (Storage.CustomCapsEnabled)
                    {
                        rowRect.y += rowHeight * 1.5f;
                        Text.Font =  GameFont.Tiny;
                        Widgets.Label(rowRect, "NVCapsExp".Translate());
                        Text.Font =  GameFont.Small;
                        rowRect.y += rowHeight + Constants.RowGap;
                        SettingsCache.MinCache = Widgets.HorizontalSlider(rowRect,
                            (float) SettingsCache.MinCache,
                            1f,
                            100f,
                            true,
                            "NVSettingsMinCapLabel".Translate(SettingsCache.MinCache),
                            "1%",
                            "100%",
                            1);
                        SettingsHelpers.DrawIndicator(rowRect,
                            Constants.DefaultMinCap,
                            0f,
                            1f,
                            100f,
                            IndicatorTex.DefIndicator);
                        rowRect.y += rowHeight * 1.5f;
                        SettingsCache.MaxCache = Widgets.HorizontalSlider(rowRect,
                            (float) SettingsCache.MaxCache,
                            100f,
                            200f,
                            true,
                            "NVSettingsMinCapLabel".Translate(SettingsCache.MaxCache),
                            "100%",
                            "200%",
                            1);
                        SettingsHelpers.DrawIndicator(rowRect,
                            Constants.DefaultMaxCap,
                            0f,
                            100f,
                            200f,
                            IndicatorTex.DefIndicator);
                    }

                rowRect.y += rowHeight * 1.5f;
                Widgets.DrawLineHorizontal(rowRect.x + 24f, rowRect.y, rowRect.width - 48f);
                rowRect.y += Constants.RowGap;
                if (Settings.CEDetected)
                    {
                        Widgets.CheckboxLabeled(rowRect,
                            "EnableNVForCE".Translate(),
                            ref Storage.NVEnabledForCE);
                    }

                Text.Anchor = anchor;
            }

        public static void RaceTab(
            Rect inRect)
            {
                int raceCount = Storage.RaceLightMods.Count;
                if (_numberOfCustomRaces == null)
                    {
                        _numberOfCustomRaces =
                                    Storage.RaceLightMods.Count(rlm => rlm.Value.IntSetting == VisionType.NVCustom);
                    }

                inRect = inRect.AtZero();
                SettingsHelpers.DrawLightModifiersHeader(ref inRect, "NVRaces".Translate());

                float num = inRect.y + 3f;
                var viewRect = new Rect(inRect.x,
                    inRect.y,
                    inRect.width * 0.9f,
                    raceCount * (Constants.RowHeight + Constants.RowGap) + (float) _numberOfCustomRaces * 100f);
                var rowRect = new Rect(inRect.x + 6f, num, inRect.width - 12f, Constants.RowHeight);
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

                                GUI.color = Color.red;
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
                        num += Constants.RowHeight + Constants.RowGap;
                        if (count < raceCount)
                            {
                                Widgets.DrawLineHorizontal(rowRect.x + 6f, num - 5.5f, rowRect.width - 12f);
                            }
                    }

                Widgets.EndScrollView();
            }

        public static void ApparelTab(
            Rect inRect)
            {
                Text.Anchor = TextAnchor.LowerCenter;
                int  apparelCount = SettingsCache.GetAllHeadgear.Count;
                var  headerRect   = new Rect(24f, 0f, inRect.width - 64f, 36f);
                Rect leftRect     = headerRect.LeftPart(0.4f);
                Rect midRect      = headerRect.RightPart(0.6f).LeftHalf().RightPart(0.8f);
                Rect rightRect    = headerRect.RightPart(0.6f).RightHalf().LeftPart(0.8f);
                Widgets.Label(leftRect,  "NVApparel".Translate());
                Widgets.Label(midRect,   "NVNullPS".Translate());
                Widgets.Label(rightRect, "NVGiveNV".Translate());

                Widgets.DrawLineHorizontal(headerRect.x + 12f, headerRect.yMax + 4f, headerRect.xMax - 64f);

                Text.Anchor = TextAnchor.MiddleCenter;
                var viewRect   = new Rect(32f, 48f, inRect.width - 64f, apparelCount * 48f);
                var scrollRect = new Rect(12f, 48f, inRect.width - 12f, inRect.height - 48f);

                var   checkboxSize = 20f;
                float leftBoxX     = midRect.center.x + 20f;
                float rightBoxX    = rightRect.center.x + 20f;

                var num = 48f;
                Widgets.BeginScrollView(scrollRect, ref _apparelScrollPosition, viewRect);
                foreach (ThingDef appareldef in SettingsCache.GetAllHeadgear)
                    {
                        var rowRect = new Rect(scrollRect.x + 12f, num, scrollRect.width - 24f, 40);
                        Widgets.DrawAltRect(rowRect);

                        var  locGUIContent = new GUIContent(appareldef.LabelCap, appareldef.uiIcon);
                        Rect apparelRect   = rowRect.LeftPart(0.4f);
                        Widgets.Label(apparelRect, locGUIContent);
                        TooltipHandler.TipRegion(apparelRect,
                            new TipSignal(appareldef.description, apparelRect.GetHashCode()));
                        var leftBoxPos  = new Vector2(leftBoxX,  rowRect.center.y - checkboxSize / 2);
                        var rightBoxPos = new Vector2(rightBoxX, rowRect.center.y - checkboxSize / 2);
                        if (Storage.NVApparel.TryGetValue(appareldef, out ApparelVisionSetting apparelSetting))
                            {
                                Widgets.Checkbox(leftBoxPos,  ref apparelSetting.NullifiesPS, 20f);
                                Widgets.Checkbox(rightBoxPos, ref apparelSetting.GrantsNV,    20f);
                                if (!apparelSetting.Equals(Storage.NVApparel[appareldef]))
                                    {
                                        if (apparelSetting.IsRedundant())
                                            {
                                                Storage.NVApparel.Remove(appareldef);
                                            }
                                        else
                                            {
                                                Storage.NVApparel[appareldef] = apparelSetting;
                                            }
                                    }
                            }
                        else
                            {
                                var nullPs = false;
                                var giveNV = false;
                                Widgets.Checkbox(leftBoxPos,  ref nullPs);
                                Widgets.Checkbox(rightBoxPos, ref giveNV);
                                if (nullPs || giveNV)
                                    {
                                        apparelSetting                = new ApparelVisionSetting(nullPs, giveNV);
                                        Storage.NVApparel[appareldef] = apparelSetting;
                                    }
                            }

                        num += 48f;
                    }

                Widgets.EndScrollView();
                Text.Anchor = TextAnchor.UpperLeft;
            }

        public static void HediffTab(
            Rect inRect)
            {
                int hediffcount = SettingsCache.GetAllHediffs.Count;
                if (_numberOfCustomHediffs == null)
                    {
                        _numberOfCustomHediffs =
                                    Storage.HediffLightMods.Count(hlm => hlm.Value.IntSetting == VisionType.NVCustom);
                    }

                inRect = inRect.AtZero();
                SettingsHelpers.DrawLightModifiersHeader(ref inRect, "NVHediffs".Translate());
                float num = inRect.y + 3f;
                var viewRect = new Rect(inRect.x,
                    inRect.y,
                    inRect.width * 0.9f,
                    hediffcount * (Constants.RowHeight + Constants.RowGap) + (float) _numberOfCustomHediffs * 100f);
                var rowRect = new Rect(inRect.x + 6f, num, inRect.width - 12f, Constants.RowHeight);
                Widgets.BeginScrollView(inRect, ref _hediffScrollPosition, viewRect);
                for (var i = 0; i < hediffcount; i++)
                    {
                        HediffDef hediffdef = SettingsCache.GetAllHediffs[i];
                        rowRect.y = num;
                        if (Storage.HediffLightMods.TryGetValue(hediffdef, out Hediff_LightModifiers hediffmods))
                            {
                                _numberOfCustomHediffs +=
                                            SettingsHelpers.DrawLightModifiersRow(hediffdef,
                                                hediffmods,
                                                rowRect,
                                                ref num,
                                                false);
                            }
                        else
                            {
                                Hediff_LightModifiers temp = Storage.AllEyeHediffs.Contains(hediffdef)
                                            ? new Hediff_LightModifiers {AffectsEye = true}
                                            : new Hediff_LightModifiers();
                                _numberOfCustomHediffs +=
                                            SettingsHelpers.DrawLightModifiersRow(hediffdef,
                                                temp,
                                                rowRect,
                                                ref num,
                                                false);
                                if (temp.IntSetting != VisionType.NVNone)
                                    {
                                        temp.InitialiseNewFromSettings(hediffdef);
                                        Storage.HediffLightMods[hediffdef] = temp;
                                    }
                            }

                        num += Constants.RowHeight + Constants.RowGap;
                        if (i < hediffcount)
                            {
                                Widgets.DrawLineHorizontal(rowRect.x + 6f,
                                    num - (Constants.RowGap / 2 - 0.5f),
                                    rowRect.width - 12f);
                            }
                    }

                Widgets.EndScrollView();
            }

        public static void DebugTab(
            Rect inRect)
            {
                bool playing = Current.ProgramState == ProgramState.Playing;
                inRect = inRect.AtZero();
                var listing = new Listing_Standard();
                listing.Begin(inRect);
                listing.CheckboxLabeled("NVDebugLogComps".Translate(), ref LogPawnComps);
                listing.GapLine();
                listing.LabelDouble("NVDebugAverageTime".Translate(),
                    (NVHarmonyPatcher.TotalGlFactorNanoSec / NVHarmonyPatcher.TotalTicks).ToString("00 ns/tick"));
                listing.Label($"1 tick = {1000000000 / 60:00} ns");
                listing.GapLine();
                float listingY = listing.CurHeight;
                listing.End();
                if (playing)
                    {
                        if (_allPawns == null)
                            {
                                _allPawns = PawnsFinder.AllMaps_Spawned.Where(pwn => pwn.RaceProps.Humanlike).ToList();
                            }

                        float height;
                        if (Math.Abs(_maxY) < 0.001)
                            {
                                height = 25 * _allPawns.Count + 200 * _allPawns
                                                                      .FindAll(pawn => pawn.GetComp<Comp_NightVision>()
                                                                                       != null)
                                                                      .Count;
                            }
                        else
                            {
                                height = _maxY;
                            }

                        var remainRect = new Rect(inRect.x,
                            listingY + 5f,
                            inRect.width,
                            inRect.yMax - listingY + 5f);
                        var viewRect = new Rect(remainRect.x + 6f, remainRect.y, remainRect.width - 12f, height);
                        var rowRect = new Rect(remainRect.x + 10f,
                            remainRect.y + 3f,
                            remainRect.width - 20f,
                            Constants.RowHeight / 2);
                        Widgets.BeginScrollView(remainRect, ref _debugScrollPos, viewRect);
                        Text.Font = GameFont.Tiny;
                        foreach (Pawn pawn in _allPawns)
                            {
                                Widgets.Label(rowRect.LeftPart(0.1f), pawn.Name.ToStringShort);
                                if (pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
                                    {
                                        Rect rightRect = rowRect.RightPart(0.8f);
                                        Widgets.Label(rightRect,
                                            $"Number of eyes: {comp.NumberOfRemainingEyes}/{comp.RaceSightParts.Count}   Natural modifier: {comp.NaturalLightModifiers.IntSetting}");
                                        rightRect.y += Constants.RowHeight / 2;
                                        Widgets.Label(rightRect,
                                            $"0% light modifier: {comp.ZeroLightModifier}   100% light modifier: {comp.FullLightModifier}");
                                        rightRect.y += Constants.RowHeight / 2;
                                        Widgets.Label(rightRect,
                                            $"Psych: {comp.PsychDark()}      Apparel: NightVis: {comp.ApparelGrantsNV}   Anti-brightness: {comp.ApparelNullsPS}");
                                        rightRect.y += Constants.RowHeight / 2;
                                        Widgets.Label(rightRect,
                                            $"Health (pre-cap) [0%,100%] : NightV mod: [{comp.NvhediffMods[0]}, {comp.NvhediffMods[1]}]   PhotoS mod: [{comp.PshediffMods[0]}, {comp.PshediffMods[1]}]   Custom: [{comp.HediffMods[0]},{comp.HediffMods[1]}]");
                                        rightRect.y += Constants.RowHeight / 2;
                                        Widgets.Label(rightRect, "Body Parts & their hediffs");
                                        foreach (KeyValuePair<string, List<HediffDef>> hedifflist in comp.PawnsNVHediffs
                                                    )
                                            {
                                                rightRect.y += Constants.RowHeight / 2;
                                                Widgets.Label(rightRect,
                                                    $"  Body part: {hedifflist.Key} has hediffs: ");
                                                foreach (HediffDef hediff in hedifflist.Value)
                                                    {
                                                        if (Storage.HediffLightMods.TryGetValue(hediff,
                                                            out Hediff_LightModifiers value))
                                                            {
                                                                rightRect.y += Constants.RowHeight / 2;
                                                                Widgets.Label(rightRect,
                                                                    $"    {hediff.LabelCap}: current setting = {value.IntSetting}");
                                                            }
                                                        else
                                                            {
                                                                rightRect.y += Constants.RowHeight / 2;
                                                                Widgets.Label(rightRect,
                                                                    $"    {hediff.LabelCap}: No Setting");
                                                            }
                                                    }
                                            }

                                        rightRect.y += Constants.RowHeight / 2;
                                        Widgets.Label(rightRect, "Eye covering or nightvis Apparel:");
                                        foreach (Apparel apparel in comp.PawnsNVApparel)
                                            {
                                                if (Storage.NVApparel.TryGetValue(apparel.def,
                                                    out ApparelVisionSetting appSet))
                                                    {
                                                        rightRect.y += Constants.RowHeight / 2;
                                                        Widgets.Label(rightRect,
                                                            $"  {apparel.LabelCap}: nightvis: {appSet.GrantsNV}  anti-bright: {appSet.NullifiesPS}  - def file setting: NV:{appSet.CompGrantsNV} A-B:{appSet.CompNullifiesPS}");
                                                    }
                                                else
                                                    {
                                                        rightRect.y += Constants.RowHeight / 2;
                                                        Widgets.Label(rightRect, $"  {apparel.LabelCap}: No Setting");
                                                    }
                                            }

                                        rightRect.y += Constants.RowHeight / 2;
                                        rowRect.y   =  rightRect.y;
                                    }
                                else
                                    {
                                        Widgets.Label(rowRect.RightHalf(), "No Night Vision component found.");
                                    }

                                rowRect.y += Constants.RowHeight / 2;
                                Widgets.DrawLineHorizontal(rowRect.x + 10f, rowRect.y, rowRect.width - 20f);
                                rowRect.y += Constants.RowHeight / 2;
                            }

                        if (Math.Abs(_maxY) < 0.001)
                            {
                                _maxY = rowRect.yMax;
                            }

                        Widgets.EndScrollView();
                    }
            }
    }