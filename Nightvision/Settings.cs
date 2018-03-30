using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace NightVision
{
    class NightVisionSettings : ModSettings
    {
        #region GUI And Settings Fields
        private enum Tab
        {
            General,
            Races,
            Apparel,
            Bionics
        }
        private Tab tab;
        private static List<TabRecord> tabsList = new List<TabRecord>();
        private Vector2 scrollPosition = Vector2.zero;
        private List<ThingDef> RacesWithChangedSettings = new List<ThingDef>();

        #endregion

        #region Constants & Static Struct

        internal const float DefaultZeroLightMultiplier = 0.8f;
        internal const float DefaultFullLightMultiplier = 1f;

        internal static readonly FloatRange DefaultNightVisionMultipliers = FloatRange.One;
        internal static readonly FloatRange DefaultModifiers = FloatRange.Zero;
        internal static readonly FloatRange DefaultPhotosensitiveMultipliers = new FloatRange(1.2f, 0.8f);

        #endregion

        
        public static NightVisionSettings Instance;

        #region Def lists

        internal static List<HediffDef> AllRelevantEyeBrainOrBodyHediffs = new List<HediffDef>();
        internal static List<ThingDef> AllEyeCoveringHeadgearDefs = new List<ThingDef>();

        #endregion

        #region SavedSettings

        public static Dictionary<ThingDef, FloatRange> RaceNVMultipliers = new Dictionary<ThingDef, FloatRange>();
        public static Dictionary<ThingDef, ApparelSetting> NVApparel = new Dictionary<ThingDef, ApparelSetting>();
        public static Dictionary<HediffDef, FloatRange> NVHediffs = new Dictionary<HediffDef, FloatRange>();

        private static FloatRange nightVisionMultipliers;
        private static FloatRange photosensitiveMultipliers;

        public static FloatRange MultiplierCaps = new FloatRange(0.8f, 1.2f);

        #endregion

        private static Dictionary<ThingDef, FloatRange> raceNightVisionModsCached = new Dictionary<ThingDef, FloatRange>();

        public static FloatRange NightVisionMultipliers
        {
            get
            {
                if (nightVisionMultipliers == null)
                {
                    Log.Message("nightVisionMultipliers was null");
                    nightVisionMultipliers = DefaultNightVisionMultipliers;
                }
                return nightVisionMultipliers;
            }
        }
        public static FloatRange PhotosensitiveMultipliers
        {
            get
            {
                if (photosensitiveMultipliers == null)
                {
                    Log.Message("nightVisionMultipliers was null");
                    photosensitiveMultipliers = DefaultPhotosensitiveMultipliers;
                }
                return photosensitiveMultipliers;
            }
        }

        /// <returns>TOTAL decimal modifiers (0.8f + min, 1f + max)</returns>
        internal static FloatRange GetRaceNightVisionMod(ThingDef race)
        {
            if (!raceNightVisionModsCached.ContainsKey(race))
            {
                if (RaceNVMultipliers.TryGetValue(race, out FloatRange raceNVMultiplier))
                {
                    raceNightVisionModsCached[race] = new FloatRange(raceNVMultiplier.min - DefaultZeroLightMultiplier, raceNVMultiplier.max - DefaultFullLightMultiplier);
                }
                else
                {
                    raceNightVisionModsCached[race] = DefaultModifiers;
                }
            }
            return raceNightVisionModsCached[race];
        }







        public NightVisionSettings()
        {
            Instance = this;
        }
        
        public void DoSettingsWindowContents(Rect inRect)
        {
            
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;

            tabsList.Clear();
            tabsList.Add(new TabRecord("General", delegate{tab = Tab.General;}, tab == Tab.General));
            tabsList.Add(new TabRecord("Races", delegate { tab = Tab.Races; }, tab == Tab.Races));
            tabsList.Add(new TabRecord("Apparel", delegate { tab = Tab.Apparel; }, tab == Tab.Apparel));
            tabsList.Add(new TabRecord("Bionics", delegate { tab = Tab.Bionics; }, tab == Tab.Bionics));

            inRect.yMin += 32f;
            Widgets.DrawMenuSection(inRect);
            TabDrawer.DrawTabs(inRect, tabsList);
            //inRect = inRect.ContractedBy(17f);
            inRect = inRect.ContractedBy(17f);
            GUI.BeginGroup(inRect);
            switch (tab)
            {
                case Tab.General:
                    DrawGeneralTab(inRect);
                    break;
                case Tab.Races:
                    DrawRaceTab(inRect);
                    break;
                case Tab.Apparel:
                    foreach (ThingDef appareldef in NVApparel.Keys)
                    {
                        int appindex = AllEyeCoveringHeadgearDefs.IndexOf(appareldef);
                        if (appindex >= 0)
                        {
                            AllEyeCoveringHeadgearDefs.RemoveAt(appindex);
                            AllEyeCoveringHeadgearDefs.Insert(0, appareldef);
                        }
                    }

                    DrawApparelTab(inRect);
                    break;
                case Tab.Bionics:
                    DrawHediffTab(inRect);
                    break;
                default:
                    break;
            }
            GUI.EndGroup();
           
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref MultiplierCaps.min, "LowerLimit", 0.8f);
            Scribe_Values.Look(ref MultiplierCaps.max, "UpperLimit", 1.2f);

            Scribe_Values.Look(ref photosensitiveMultipliers.min, "PhotosensZeroLightMultiplier", 1.2f);
            Scribe_Values.Look(ref photosensitiveMultipliers.max, "PhotosensFullLightMultiplier", 0.8f);

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                RaceNVMultipliers.RemoveAll(kvp => kvp.Key == null);
                NVApparel.RemoveAll(appset => appset.Key == null || appset.Value.IsRedundant());
            }

            Scribe_Collections.Look(ref RaceNVMultipliers, "Race", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref NVApparel, "Apparel", LookMode.Def, LookMode.Deep);
            Scribe_Collections.Look(ref NVHediffs, "Hediffs", LookMode.Def, LookMode.Value);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                int numOfRemoved = RaceNVMultipliers.RemoveAll(kvp => kvp.Key == null);
                if (numOfRemoved > 0)
                {
                    Log.Message("Night Vision: Removed " + numOfRemoved + " null defs from race dictionary." +
                        " This is to be expected if you have removed a race mod.");
                }
                numOfRemoved = NVApparel.RemoveAll(appset => appset.Key == null || appset.Value.IsRedundant());
                if (numOfRemoved > 0)
                {
                    Log.Message("Night Vision: Removed " + numOfRemoved + " null defs and redundant settings from apparel list." +
                        " This is to be expected if you have removed a mod or changed Night Vision mod settings.");
                }
                numOfRemoved = NVHediffs.RemoveAll(entry => entry.Key == null || (Mathf.Approximately(entry.Value.min, 0f) && Mathf.Approximately(entry.Value.max, 0f)));
                if (numOfRemoved > 0)
                {
                    Log.Message("Night Vision: Removed " + numOfRemoved + " null hediffDefs and redundant settings from hediffdef list" +
                        " This is to be expected if you have removed a mod or changed Night Vision mod settings.");
                }
            }

        }

        /// <summary>
        /// So that the comps will update with the new settings, sets all the comps dirty
        /// </summary>
        internal void SetDirtyAllComps()
        {
            foreach(Pawn pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods.Where(pwn => pwn.RaceProps.Humanlike))
            {
                if (pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
                {
                    comp.SetDirty();
                }
            }
        }

        private void DrawGeneralTab(Rect inRect)
        {
            //TODO DrawGeneralTab: Add reset defaults and thought settings
            //TODO DrawGeneralTab: Add general settings for NV and Photosensitivity?
            float minAsPercent = MultiplierCaps.min * 100;
            float maxAsPercent = MultiplierCaps.max * 100;
            string minBuffer = minAsPercent.ToString() + "%";
            string maxBuffer = maxAsPercent.ToString() + "%";
            Rect rowRect = inRect.AtZero().TopPart(0.2f);
            minAsPercent = Widgets.HorizontalSlider(rowRect.TopHalf(), minAsPercent, 10f, 80f, false, minBuffer, "10%", "80%", 1);
            maxAsPercent = Widgets.HorizontalSlider(rowRect.BottomHalf(), maxAsPercent, 100f, 200f, false, maxBuffer, "100%", "200%", 1);
            if (Math.Abs(minAsPercent / 100f - MultiplierCaps.min) > 0.001f || Math.Abs(maxAsPercent / 100f - MultiplierCaps.max) > 0.001f)
            {
                Log.Message("Night Vision: DrawGeneralTab: float comparison false: " + minAsPercent / 100f + " != " + MultiplierCaps.min + " or " + maxAsPercent / 100f + " != " + MultiplierCaps.max);
                MultiplierCaps = new FloatRange(minAsPercent / 100f, maxAsPercent / 100f);
            }
        }

        private void DrawRaceTab(Rect inRect)
        {
            // TODO DrawRaceTab: Add headers
            //  TODO DrawRaceTab: Try and fix not letting the user input due to min max settings; Sliders?
            int raceCount = RaceNVMultipliers.Count;
            Rect viewRect = new Rect(0f, 0f, inRect.width, raceCount * 32f);
            float num = 3f;
            Widgets.BeginScrollView(inRect.AtZero(), ref this.scrollPosition, viewRect);
            foreach (ThingDef key in RaceNVMultipliers.Keys.ToList())
            {
                float zeroLightMultiplier = RaceNVMultipliers[key].min;
                float fullLightMultiplier = RaceNVMultipliers[key].max;
                string zeroLightMultiplierBuffer = (zeroLightMultiplier * 100).ToString();
                string fullLightMultiplierBuffer = (fullLightMultiplier * 100).ToString();

                Rect rowRect = new Rect(12, num, inRect.width - 12, 30);
                Rect leftRect = rowRect.LeftPart(0.3f);
                Rect rightRect = rowRect.RightPart(0.6f);
                Widgets.Label(leftRect, key.LabelCap);
                Widgets.TextFieldPercent(rightRect.LeftPart(0.4f), ref zeroLightMultiplier, ref zeroLightMultiplierBuffer, MultiplierCaps.min, MultiplierCaps.max );
                Widgets.TextFieldPercent(rightRect.RightPart(0.4f), ref fullLightMultiplier, ref fullLightMultiplierBuffer, MultiplierCaps.min, MultiplierCaps.max);
                if (Math.Abs(zeroLightMultiplier - RaceNVMultipliers[key].min) > 0.001f || Math.Abs(fullLightMultiplier - RaceNVMultipliers[key].max) > 0.001f)
                {
                    Log.Message("Night Vision: DrawRaceTab: float comparison false: " + zeroLightMultiplier + " != " + RaceNVMultipliers[key].min + " or " + fullLightMultiplier + " != " + RaceNVMultipliers[key].max);
                    RaceNVMultipliers[key] = new FloatRange(zeroLightMultiplier, fullLightMultiplier);
                    RacesWithChangedSettings.Add(key);
                }
                num += 32f;
            }
            Widgets.EndScrollView();
        }
        private void DrawApparelTab(Rect inRect)
        {
            Text.Anchor = TextAnchor.LowerCenter;
            int apparelCount = AllEyeCoveringHeadgearDefs.Count;
            Rect headerRect = new Rect(24f, 0f, inRect.width - 64f, 36f);
            Rect leftRect = headerRect.LeftPart(0.4f);
            Rect midRect = headerRect.RightPart(0.6f).LeftHalf().RightPart(0.8f);
            Rect rightRect = headerRect.RightPart(0.6f).RightHalf().LeftPart(0.8f);
            Widgets.Label(leftRect, "Apparel");
            Widgets.Label(midRect, "Nullifies Photosensitivity");
            Widgets.Label(rightRect, "Gives NightVision");

            Widgets.DrawLineHorizontal(headerRect.x + 12f, headerRect.yMax + 4f, headerRect.xMax - 64f);

            Text.Anchor = TextAnchor.MiddleCenter;
            Rect viewRect = new Rect(32f, 48f, inRect.width - 64f, apparelCount * 48f);
            Rect scrollRect = new Rect(12f, 48f, inRect.width - 12f, inRect.height - 48f);

            float checkboxSize = 20f;
            float leftBoxX = midRect.center.x + 20f;
            float rightBoxX = rightRect.center.x + 20f;

            float num = 48f;
            Widgets.BeginScrollView(scrollRect, ref this.scrollPosition, viewRect);
            foreach (ThingDef appareldef in AllEyeCoveringHeadgearDefs)
            {

                ApparelSetting apparelSetting;
                Rect rowRect = new Rect(scrollRect.x + 12f, num, scrollRect.width - 24f, 40);
                Widgets.DrawAltRect(rowRect);
                
                GUIContent gUIContent = new GUIContent(appareldef.LabelCap, appareldef.uiIcon, appareldef.description);
                
                Widgets.Label(rowRect.LeftPart(0.4f), gUIContent);
                Vector2 leftBoxPos = new Vector2(leftBoxX, rowRect.center.y - (checkboxSize / 2));
                Vector2 rightBoxPos = new Vector2(rightBoxX, rowRect.center.y - (checkboxSize / 2));
                if (NVApparel.TryGetValue(appareldef, out apparelSetting))
                {
                    Widgets.Checkbox(leftBoxPos, ref apparelSetting.NullifiesPS, 20f);
                    Widgets.Checkbox(rightBoxPos, ref apparelSetting.GrantsNV, 20f);
                    if (!apparelSetting.Equals(NVApparel[appareldef]))
                    {
                        if (apparelSetting.IsRedundant())
                        {
                            NVApparel.Remove(appareldef);
                        }
                        else
                        {
                            NVApparel[appareldef] = apparelSetting;
                        }
                    }
                }
                else
                {
                    bool nullPS = false;
                    bool giveNV = false;
                    Widgets.Checkbox(leftBoxPos, ref nullPS);
                    Widgets.Checkbox(rightBoxPos, ref giveNV);
                    if (nullPS || giveNV)
                    {
                        apparelSetting = new ApparelSetting( nullPS,  giveNV );
                        NVApparel[appareldef] = apparelSetting;
                    }
                }
                num += 48f;
            }
            Widgets.EndScrollView();
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DrawHediffTab(Rect inRect)
        {
            // TODO DrawHediffTab: Add headers
            // TODO DrawHediffTab: Add Use General Values
            int hediffcount = AllRelevantEyeBrainOrBodyHediffs.Count;

            Rect viewRect = new Rect(0f, 0f, inRect.width / 10 * 9, hediffcount * 32f);
            float num = 3f;
            Widgets.BeginScrollView(inRect.AtZero(), ref this.scrollPosition, viewRect);
            foreach (HediffDef hediffdef in AllRelevantEyeBrainOrBodyHediffs)
            {
                

                Rect rowRect = new Rect(5, num, inRect.width - 6, 30);
                Rect leftRect = rowRect.LeftPart(0.3f);
                Rect rightRect = rowRect.RightPart(0.6f);
                Widgets.Label(leftRect, hediffdef.LabelCap);
                if (NVHediffs.TryGetValue(hediffdef, out FloatRange hediffmods))
                {
                    string zerolightmodbuffer = (hediffmods.min * 100).ToString();
                    string fulllightmodbuffer = (hediffmods.max * 100).ToString();
                    Widgets.TextFieldPercent(rightRect.LeftPart(0.4f), ref hediffmods.min, ref zerolightmodbuffer, -0.45f, +0.45f);
                    Widgets.TextFieldPercent(rightRect.RightPart(0.4f), ref hediffmods.max, ref fulllightmodbuffer, -0.45f, +0.45f);
                    if (Math.Abs(hediffmods.min - NVHediffs[hediffdef].min) > 0.001f || Math.Abs(hediffmods.max - NVHediffs[hediffdef].max) > 0.001f)
                    {
                        Log.Message("Night Vision: DrawHediffTab: float comparison false: " + hediffmods.min + " != " + NVHediffs[hediffdef].min + " or " + hediffmods.max + " != " + NVHediffs[hediffdef].max);

                        if (Math.Abs(hediffmods.min) < 0.001f || Math.Abs(hediffmods.max) < 0.001f)
                        {
                            Log.Message("Night Vision: DrawHediffTab: hediffmod set to Zero: " + hediffmods.min + " and " + hediffmods.max + "...Removing");
                            NVHediffs.Remove(hediffdef);
                        }
                        else
                        {
                            NVHediffs[hediffdef] = hediffmods;
                        }
                    }
                }
                else
                {
                    float zeroMod = DefaultModifiers.min;
                    float fullMod = DefaultModifiers.max;
                    string zeroModBuffer = (zeroMod*100).ToString();
                    string fullModBuffer = (fullMod*100).ToString();
                    Widgets.TextFieldPercent(rightRect.LeftPart(0.4f), ref zeroMod, ref zeroModBuffer, -0.45f, +0.45f);
                    Widgets.TextFieldPercent(rightRect.RightPart(0.4f), ref fullMod, ref fullModBuffer, -0.45f, +0.45f);
                    if (Math.Abs(hediffmods.min) > 0.001f || Math.Abs(hediffmods.max) > 0.001f)
                    {
                        Log.Message("Night Vision: DrawHediffTab: new hediffmod != Zero: " + hediffmods.min + " and " + hediffmods.max + "...adding");
                        NVHediffs[hediffdef] = new FloatRange(zeroMod, fullMod);
                    }
                }
                num += 32f;
            }
            Widgets.EndScrollView();
        }
        /// <summary>
        /// Checks to see if any settings changed flags have been activated
        /// If they have then reset them, in the case of the eyemods, or remove them
        /// in the case of race settings.
        /// </summary>
        internal void ResetDependants()
        {
            //if (BionicSettingsChanged)
            //{
            //    intBionicEyeMods.min = -1;
            //    intBionicEyeMods.max = -1;
            //    BionicSettingsChanged = false;
            //}
            //if (PhotoSensSettingsChanged)
            //{
            //    intPhotoSensEyeMods.min = -1;
            //    intPhotoSensEyeMods.max = -1;
            //    PhotoSensSettingsChanged = false;
            //}
            if (RacesWithChangedSettings.Count > 0)
            {
                foreach (ThingDef race in RacesWithChangedSettings)
                {
                    raceNightVisionModsCached.Remove(race);
                }
                RacesWithChangedSettings.Clear();
            }
        }
    }
}
