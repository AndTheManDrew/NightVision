using System;
using System.Collections.Generic;
using System.Linq;
using NightVision.Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace NightVision {
    public static class DebugTab {
        private static List<Pawn> _allPawns;
        private static Vector2    _debugScrollPos = Vector2.zero;
        private static float      _maxY;

        public static void Clear()
        {
            DebugTab._debugScrollPos = Vector2.zero;


            DebugTab._allPawns = null;
            DebugTab._maxY     = -1;
        }

        public static void DrawTab(Rect inRect)
        {
            bool playing = Current.ProgramState == ProgramState.Playing;
            inRect    = inRect.AtZero();
            Text.Font = GameFont.Tiny;
            float listingY = default;
#if DEBUG
            var listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.CheckboxLabeled("NVDebugLogComps".Translate(), ref LogPawnComps);
            listing.GapLine();

            listing.LabelDouble(
                "NVDebugAverageTime".Translate(),
                (StatPartGlow_FactorFromGlow.TotalGlFactorNanoSec / StatPartGlow_FactorFromGlow.TotalTicks).ToString("00 ns/tick")
            );

            listing.Label($"1 tick = {1000000000 / 60:00} ns");
            listing.GapLine();
            listingY = listing.CurHeight;
            listing.End();
#endif

            var rowRect = new Rect(
                inRect.width * 0.05f,
                inRect.height * 0.05f + listingY,
                inRect.width * 0.9f,
                Constants_Draw.RowHeight
            );

            Text.Anchor = TextAnchor.MiddleLeft;
            //Multiplier Limits
            Widgets.CheckboxLabeled(rowRect, "NVCustomCapsEnabled".Translate(), ref Storage.CustomCapsEnabled);

            if (Storage.CustomCapsEnabled)
            {
                rowRect.y += Constants_Draw.RowHeight;
                Text.Font =  GameFont.Tiny;
                Widgets.Label(rowRect, "NVCapsExp".Translate());
                //Text.Font =  GameFont.Small;
                rowRect.y += Constants_Draw.RowHeight /*+ Constants.RowGap*/;

                SettingsCache.MinCache = Widgets.HorizontalSlider(
                    rowRect,
                    (float) SettingsCache.MinCache,
                    1f,
                    100f,
                    true,
                    "NVSettingsMinCapLabel".Translate(SettingsCache.MinCache.Value),
                    "1%",
                    "100%",
                    1
                );

                SettingsHelpers.DrawIndicator(
                    rowRect.TopPart(0.9f),
                    Constants_Calculations.DefaultMinCap,
                    0f,
                    1f,
                    100f,
                    IndicatorTex.DefIndicator
                );

                rowRect.y += Constants_Draw.RowHeight;

                SettingsCache.MaxCache = Widgets.HorizontalSlider(
                    rowRect,
                    (float) SettingsCache.MaxCache,
                    100f,
                    200f,
                    true,
                    "NVSettingsMaxCapLabel".Translate(
                        SettingsCache
                                    .MaxCache.Value
                    ),
                    "100%",
                    "200%",
                    1
                );

                SettingsHelpers.DrawIndicator(
                    rowRect.TopPart(0.9f),
                    Constants_Calculations.DefaultMaxCap,
                    0f,
                    100f,
                    200f,
                    IndicatorTex.DefIndicator
                );
            }

            rowRect.y += Constants_Draw.RowHeight;
            Widgets.DrawLineHorizontal(rowRect.x + 24f, rowRect.y, rowRect.width - 48f);
            float endY = rowRect.yMax;

            if (playing)
            {
                if (_allPawns == null)
                {
                    _allPawns = PawnsFinder.AllMaps_Spawned.Where(pwn => pwn.RaceProps.Humanlike).ToList();
                }

                float height;

                if (Math.Abs(_maxY) < 0.001)
                {
                    height = 25 * _allPawns.Count
                             + 200
                             * _allPawns
                                         .FindAll(
                                             pawn => pawn.GetComp<Comp_NightVision>()
                                                     != null
                                         )
                                         .Count;
                }
                else
                {
                    height = _maxY;
                }

                var remainRect = new Rect(
                    inRect.x,
                    endY + 5f,
                    inRect.width,
                    inRect.yMax - endY + 5f
                );

                var viewRect = new Rect(remainRect.x + 6f, remainRect.y, remainRect.width - 12f, height);

                rowRect = new Rect(
                    remainRect.x     + 10f,
                    remainRect.y     + 3f,
                    remainRect.width - 20f,
                    Constants_Draw.RowHeight / 2
                );

                Widgets.BeginScrollView(remainRect, ref _debugScrollPos, viewRect);
                Text.Font = GameFont.Tiny;

                foreach (Pawn pawn in _allPawns)
                {
                    Widgets.Label(rowRect.LeftPart(0.1f), pawn.Name.ToStringShort);

                    if (pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
                    {
                        Rect rightRect = rowRect.RightPart(0.8f);

                        Widgets.Label(
                            rightRect,
                            $"Number of eyes: {comp.NumberOfRemainingEyes}/{comp.RaceSightParts.Count}   Natural modifier: {comp.NaturalLightModifiers.IntSetting}"
                        );

                        rightRect.y += Constants_Draw.RowHeight / 2;

                        Widgets.Label(
                            rightRect,
                            $"0% light modifier: {comp.ZeroLightModifier}   100% light modifier: {comp.FullLightModifier}"
                        );

                        rightRect.y += Constants_Draw.RowHeight / 2;

                        Widgets.Label(
                            rightRect,
                            $"Psych: {comp.PsychDark}      Apparel: NightVis: {comp.ApparelGrantsNV}   Anti-brightness: {comp.ApparelNullsPS}"
                        );

                        rightRect.y += Constants_Draw.RowHeight / 2;

                        Widgets.Label(
                            rightRect,
                            $"Health (pre-cap) [0%,100%] : NightV mod: [{comp.NvhediffMods[0]}, {comp.NvhediffMods[1]}]   PhotoS mod: [{comp.PshediffMods[0]}, {comp.PshediffMods[1]}]   Custom: [{comp.HediffMods[0]},{comp.HediffMods[1]}]"
                        );

                        rightRect.y += Constants_Draw.RowHeight / 2;
                        Widgets.Label(rightRect, "Body Parts & their hediffs");

                        foreach (KeyValuePair<string, List<HediffDef>> hedifflist in comp.PawnsNVHediffs
                        )
                        {
                            rightRect.y += Constants_Draw.RowHeight / 2;

                            Widgets.Label(
                                rightRect,
                                $"  Body part: {hedifflist.Key} has hediffs: "
                            );

                            foreach (HediffDef hediff in hedifflist.Value)
                            {
                                if (Storage.HediffLightMods.TryGetValue(
                                    hediff,
                                    out Hediff_LightModifiers value
                                ))
                                {
                                    rightRect.y += Constants_Draw.RowHeight / 2;

                                    Widgets.Label(
                                        rightRect,
                                        $"    {hediff.LabelCap}: current setting = {value.IntSetting}"
                                    );
                                }
                                else
                                {
                                    rightRect.y += Constants_Draw.RowHeight / 2;

                                    Widgets.Label(
                                        rightRect,
                                        $"    {hediff.LabelCap}: No Setting"
                                    );
                                }
                            }
                        }

                        rightRect.y += Constants_Draw.RowHeight / 2;
                        Widgets.Label(rightRect, "Eye covering or nightvis Apparel:");

                        foreach (Apparel apparel in comp.PawnsNVApparel)
                        {
                            if (Storage.NVApparel.TryGetValue(
                                apparel.def,
                                out ApparelVisionSetting appSet
                            ))
                            {
                                rightRect.y += Constants_Draw.RowHeight / 2;

                                Widgets.Label(
                                    rightRect,
                                    $"  {apparel.LabelCap}: nightvis: {appSet.GrantsNV}  anti-bright: {appSet.NullifiesPS}  - def file setting: NV:{appSet.CompGrantsNV} A-B:{appSet.CompNullifiesPS}"
                                );
                            }
                            else
                            {
                                rightRect.y += Constants_Draw.RowHeight / 2;
                                Widgets.Label(rightRect, $"  {apparel.LabelCap}: No Setting");
                            }
                        }

                        rightRect.y += Constants_Draw.RowHeight / 2;
                        rowRect.y   =  rightRect.y;
                    }
                    else
                    {
                        Widgets.Label(rowRect.RightHalf(), "No Night Vision component found.");
                    }

                    rowRect.y += Constants_Draw.RowHeight / 2;
                    Widgets.DrawLineHorizontal(rowRect.x + 10f, rowRect.y, rowRect.width - 20f);
                    rowRect.y += Constants_Draw.RowHeight / 2;
                }

                if (Math.Abs(_maxY) < 0.001)
                {
                    _maxY = rowRect.yMax;
                }

                Widgets.EndScrollView();
            }
        }

        public static bool LogPawnComps;
    }
}