using UnityEngine;
using Verse;

namespace NightVision {
    public static class ApparelTab {
        private static Vector2 _apparelScrollPosition = Vector2.zero;

        public static void Clear()
        {
            NightVision.ApparelTab._apparelScrollPosition = Vector2.zero;
        }

        public static void DrawTab(Rect inRect)
        {

            var nvApparel = Mod.Store.NVApparel;
            var cachedHeadgear = Mod.Cache.GetAllHeadgear;
            
            Text.Anchor = TextAnchor.LowerCenter;
            int apparelCount = cachedHeadgear.Count;
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
            float leftBoxX     = midRect.center.x   + checkboxSize;
            float rightBoxX    = rightRect.center.x + checkboxSize;
            var   leftBox      = new Rect(leftBoxX,  0f, checkboxSize, checkboxSize);
            var   rightBox     = new Rect(rightBoxX, 0f, checkboxSize, checkboxSize);

            var num = 48f;
            Widgets.BeginScrollView(scrollRect, ref _apparelScrollPosition, viewRect);

            foreach (ThingDef appareldef in cachedHeadgear)
            {
                var rowRect = new Rect(scrollRect.x + 12f, num, scrollRect.width - 24f, 40);
                Widgets.DrawAltRect(rowRect);

                var  locGUIContent = new GUIContent(appareldef.LabelCap, appareldef.uiIcon);
                Rect apparelRect   = rowRect.LeftPart(0.4f);
                Widgets.Label(apparelRect, locGUIContent);

                TooltipHandler.TipRegion(
                    apparelRect,
                    new TipSignal(appareldef.DescriptionDetailed /*, apparelRect.GetHashCode()*/)
                );

                var leftBoxPos  = new Vector2(leftBoxX,  rowRect.center.y - checkboxSize / 2);
                var rightBoxPos = new Vector2(rightBoxX, rowRect.center.y - checkboxSize / 2);
                leftBox.y  = leftBoxPos.y;
                rightBox.y = leftBoxPos.y;
                TooltipHandler.TipRegion(leftBox,  new TipSignal("PSApparelExplained".Translate()));
                TooltipHandler.TipRegion(rightBox, new TipSignal("NVApparelExplained".Translate()));

                if (nvApparel.TryGetValue(appareldef, out ApparelVisionSetting apparelSetting))
                {
                    Widgets.Checkbox(leftBoxPos,  ref apparelSetting.NullifiesPS, checkboxSize);
                    Widgets.Checkbox(rightBoxPos, ref apparelSetting.GrantsNV,    checkboxSize);

                    if (!apparelSetting.Equals(nvApparel[appareldef]))
                    {
                        if (apparelSetting.IsRedundant())
                        {
                            nvApparel.Remove(appareldef);
                        }
                        else
                        {
                            nvApparel[appareldef] = apparelSetting;
                        }
                    }
                }
                else
                {
                    var nullPs = false;
                    var giveNV = false;
                    Widgets.Checkbox(leftBoxPos,  ref nullPs, checkboxSize);
                    Widgets.Checkbox(rightBoxPos, ref giveNV, checkboxSize);

                    if (nullPs || giveNV)
                    {
                        if (appareldef.GetCompProperties<CompProperties_NightVisionApparel>() is
                                    CompProperties_NightVisionApparel compprops)
                        {
                            apparelSetting = compprops.AppVisionSetting;
                        }
                        else
                        {
                            apparelSetting =
                                        ApparelVisionSetting.CreateNewApparelVisionSetting(appareldef);
                        }

                        apparelSetting.NullifiesPS    = nullPs;
                        apparelSetting.GrantsNV       = giveNV;
                        nvApparel[appareldef] = apparelSetting;
                    }
                }

                num += 48f;
            }

            Widgets.EndScrollView();
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}