﻿// Nightvision NightVision CombatTab.cs
// 
// 24 10 2018
// 
// 24 10 2018

using System;
using RimWorld;
using UnityEngine;
using Verse;
using static NightVision.Storage_Combat;

namespace NightVision
{
    public class CombatTab
    {
        public float VerticalSpacing = 2f;
        public float IntSliderHeight = 28f;

        public string surpBuffer;
        

        public string[] BestAndWorstRangedCd
        {
            get
            {
                if (bestAndWorstRangedCd[0].NullOrEmpty() && bestAndWorstRangedCd[1].NullOrEmpty())
                {
                    var caps = Mod.Store.MultiplierCaps;
                    bestAndWorstRangedCd[0] = (1 / caps.max).ToStringPercent();
                    bestAndWorstRangedCd[1] = (1 / caps.min).ToStringPercent();
                }

                return bestAndWorstRangedCd;
            }
        }
        public  string[] bestAndWorstRangedCd = new string[2];
        
        public  float texRextXMod = 6f;
        
        public  float texRextYMod = 24f;
        
        public  float LineHeight = Text.LineHeight;

        public  float RangedSubListingHeight = 100f;

        public  float MeleeSubListingHeight = 100f;

        public  void DrawTab(Rect inRect)
        {
            Rect tabRect = inRect.AtZero();
            var anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            var listing = new Listing_Standard(GameFont.Small);
            listing.verticalSpacing = VerticalSpacing;
            listing.Begin(tabRect);
            var subListing = listing.SubListing(LineHeight + 12f);
            subListing.Gap(6f);
            subListing.CheckboxLabeled(CombatFeaturesEnabled.Label, ref CombatFeaturesEnabled.TempValue);
            subListing.Gap(6f);
            listing.EndSection(subListing);

            if (CombatFeaturesEnabled.TempValue)
            {

                listing.Gap();
                subListing = listing.SubListing(LineHeight * 2 + IntSliderHeight + 12f);
                subListing.Gap(6f);
                subListing.Label(HitCurviness.Label);
                subListing.Gap(6f);
                var rowRect = subListing.GetRect(IntSliderHeight);
                string hitCurStr = HitCurviness.TempValue.ToString();
                float hitCurNum = HitCurviness.TempValue;
                HitCurviness.TempValue = (int)Widgets.HorizontalSlider(rowRect, hitCurNum, 1f, 5f, true, hitCurStr, "Flat", "Extreme Changes", 1f);
                TooltipHandler.TipRegion(rowRect, HitCurviness.Tooltip);
                DrawIndicator(rowRect: rowRect, 1f / 4);
                subListing.Gap(6f);
                listing.EndSection(subListing);


                listing.Gap();
                subListing = listing.SubListing(RangedSubListingHeight);
                RangedSubListingHeight = 0;
                subListing.Gap();
                RangedSubListingHeight += 12f;
                subListing.CheckboxLabeled(RangedHitEffectsEnabled.Label, ref RangedHitEffectsEnabled.TempValue, RangedHitEffectsEnabled.Tooltip);
                RangedSubListingHeight += Text.LineHeight + VerticalSpacing;
                subListing.ShortGapLine();
                RangedSubListingHeight += 12f;
                subListing.CheckboxLabeled(RangedCooldownEffectsEnabled.Label, ref RangedCooldownEffectsEnabled.TempValue, RangedCooldownEffectsEnabled.Tooltip);
                RangedSubListingHeight += Text.LineHeight + VerticalSpacing;
                if (RangedCooldownEffectsEnabled.TempValue)
                {
                    subListing.ShortGapLine();
                    RangedSubListingHeight += 12f;
                    subListing.CheckboxLabeled(RangedCooldownLinkedToCaps.Label + $"[best: {BestAndWorstRangedCd[0]}, worst: {BestAndWorstRangedCd[1]}]", ref RangedCooldownLinkedToCaps.TempValue, RangedCooldownLinkedToCaps.Tooltip);
                    RangedSubListingHeight += Text.LineHeight + VerticalSpacing;
                    if (!RangedCooldownLinkedToCaps.TempValue)
                    {
                        subListing.ShortGapLine();
                        RangedSubListingHeight += 12f;
                        subListing.Label(RangedCooldownMinAndMax.Label, tooltip: RangedCooldownMinAndMax.Tooltip);
                        RangedSubListingHeight += Text.LineHeight + VerticalSpacing;
                        subListing.IntRange(ref RangedCooldownMinAndMax.TempValue, 1, 200);
                        RangedSubListingHeight += IntSliderHeight;

                        CheckIntRange(ref RangedCooldownMinAndMax.TempValue, 100);

                    }
                }
                subListing.Gap();
                RangedSubListingHeight += 12f;
                listing.EndSection(subListing);
                listing.Gap();

                subListing = listing.SubListing(MeleeSubListingHeight);
                MeleeSubListingHeight = 0f;
                subListing.Gap();
                MeleeSubListingHeight += 12f;
                subListing.CheckboxLabeled(MeleeHitEffectsEnabled.Label, ref MeleeHitEffectsEnabled.TempValue, MeleeHitEffectsEnabled.Tooltip);
                MeleeSubListingHeight += Text.LineHeight + VerticalSpacing;

                if (MeleeHitEffectsEnabled.TempValue)
                {
                    subListing.ShortGapLine();
                    MeleeSubListingHeight += 12f;
                    subListing.Label(SurpriseAttackMultiplier.Label);
                    MeleeSubListingHeight += Text.LineHeight + VerticalSpacing;
                    rowRect               =  subListing.GetRect(IntSliderHeight);
                    MeleeSubListingHeight += IntSliderHeight;
                    string surpStr = SurpriseAttackMultiplier.TempValue.IsTrivial() ? "[Disabled]" : "x" + SurpriseAttackMultiplier.TempValue;
                    float  surpNum = SurpriseAttackMultiplier.TempValue;
                    SurpriseAttackMultiplier.TempValue = Widgets.HorizontalSlider(rowRect, surpNum, 0f, 2f, false, surpStr, "Disabled", "x2", 0.1f);
                    TooltipHandler.TipRegion(rowRect, SurpriseAttackMultiplier.Tooltip);
                    DrawIndicator(rowRect: rowRect, 0.5f / 2f);
                
                    subListing.ShortGapLine();
                    MeleeSubListingHeight += 12f;
                    subListing.Label(DodgeCurviness.Label);
                    MeleeSubListingHeight += Text.LineHeight + VerticalSpacing;
                    rowRect               =  subListing.GetRect(IntSliderHeight);
                    MeleeSubListingHeight += IntSliderHeight;
                    string dodgeCurStr = DodgeCurviness.TempValue.ToString();
                    float  dodgeCurNum = DodgeCurviness.TempValue;
                    DodgeCurviness.TempValue = (int)Widgets.HorizontalSlider(rowRect, dodgeCurNum, 1f, 5f, false, dodgeCurStr, "Flat", "Extreme Changes", 1f);
                    TooltipHandler.TipRegion(rowRect, DodgeCurviness.Tooltip);
                    DrawIndicator(rowRect: rowRect, 2f / 4);
                }
                subListing.Gap();
                MeleeSubListingHeight += 12f;
                listing.EndSection(subListing);
                


            }
            listing.End();
            Text.Anchor = anchor;
        }

        private  void DrawIndicator(Rect rowRect, float fractionalPosition)
        {
            rowRect.x += texRextXMod + (rowRect.width - texRextXMod * 2)* fractionalPosition -6f;
            rowRect.y += texRextYMod;
            rowRect.width  =  12f;
            rowRect.height =  12f;
            Widgets.DrawTextureFitted(rowRect, IndicatorTex.DefIndicator, 1);
        }

        public  void Clear()
        {
            bestAndWorstRangedCd = new string[2];
        }

        public  bool CheckIntRange(ref IntRange range, int mustInclude)
        {
            if (range.TrueMax != range.max)
            {
                int temp = range.max;
                range.max = range.TrueMax;
                range.min = temp;
            }

            if (range.min > mustInclude)
            {
                range.min = mustInclude;
            }

            if (range.max < mustInclude)
            {
                range.max = mustInclude;
            }

            if (range.max - range.min == 0)
            {
                return false;
            }

            return true;
        }
    }
}