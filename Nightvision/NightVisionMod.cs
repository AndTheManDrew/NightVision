using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace NightVision
    //Consider adding custom pawn tracker
{
    //    [StaticConstructorOnStartup]
    class NightVisionMod : Mod
    {
        public static NightVisionMod Instance;

        public NightVisionMod(ModContentPack content) : base(content) => Instance = this;

        public List<HediffDef> listofNightVisionHediffDefs;
        public List<HediffDef> listofPhotoSensitiveHediffDefs;

        //public static Dictionary<string, int> dictOfNVGranters = new Dictionary<string, int>();

        public override string SettingsCategory() => "Night Vision - Settings Test";

        NightVisionSettings Settings;

    //    Vector2 leftScrollPosition;
    //    Vector2 rightScrollPosition;

    //    HediffDef leftSelectedDef;
    //    HediffDef rightSelectedDef;

    //    public override void DoSettingsWindowContents(Rect inRect)
    //    {
    //        base.DoSettingsWindowContents(inRect);
    //        Text.Font = GameFont.Medium;
    //        Rect topRect = inRect.TopPart(0.05f);
    //        Rect labelRect = inRect.TopPart(0.1f).BottomHalf();
    //        Rect bottomRect = inRect.BottomPart(0.9f);

    //        Rect leftRect = bottomRect.LeftHalf().RightPart(0.9f).LeftPart(0.9f);
    //        GUI.BeginGroup(leftRect, new GUIStyle(GUI.skin.box));
    //        List<HediffDef> found = this.listofNightVisionThings;
    //        float num = 3f;
    //        Widgets.BeginScrollView(leftRect.AtZero(), ref this.leftScrollPosition, new Rect(0f, 0f, leftRect.width / 10 * 9, found.Count() * 32f));
    //        if (!found.NullOrEmpty())
    //        {
    //            foreach (HediffDef def in found)
    //            {
    //                Rect rowRect = new Rect(5, num, leftRect.width - 6, 30);
    //                Widgets.DrawHighlightIfMouseover(rowRect);
    //                if (def == this.leftSelectedDef)
    //                    Widgets.DrawHighlightSelected(rowRect);
    //                Widgets.Label(rowRect, def.LabelCap ?? def.defName);
    //                if (Widgets.ButtonInvisible(rowRect))
    //                    this.leftSelectedDef = def;

    //                num += 32f;
    //            }
    //        }
    //        Widgets.EndScrollView();
    //        GUI.EndGroup();

    //        this.Settings.Write();

    //    }
    }
    class NightVisionSettings : ModSettings
    {
        
    }
}
