using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;
using System.Text;

namespace NightVision
{
    class NightVisionSettings : ModSettings
    {
        #region Constants

        internal const float DefaultZeroLightMultiplier = 0.8f;
        internal const float DefaultFullLightMultiplier = 1f;

        #endregion

        #region Class Fields
        public static NightVisionSettings Instance;
        public NightVisionSettings()
        {
            Instance = this;
        }
        #endregion

        #region Def lists

        internal static List<HediffDef> AllEyeHediffs = new List<HediffDef>();
        internal static List<HediffDef> AllSightAffectingHediffs = new List<HediffDef>();
        internal static List<ThingDef> AllEyeCoveringHeadgearDefs = new List<ThingDef>();

        internal static bool ApparelSorted = false;
        internal static List<ThingDef> GetAllHeadgear
        {
            get
            {
                if (!ApparelSorted)
                {
                    foreach (ThingDef appareldef in NVApparel.Keys)
                    {
                        int appindex = AllEyeCoveringHeadgearDefs.IndexOf(appareldef);
                        if (appindex >= 0)
                        {
                            AllEyeCoveringHeadgearDefs.RemoveAt(appindex);
                            AllEyeCoveringHeadgearDefs.Insert(0, appareldef);
                        }
                    }
                    ApparelSorted = true;
                }
                return AllEyeCoveringHeadgearDefs;
            }
        }

        internal static bool HediffsSorted = false;
        internal static List<HediffDef> GetAllHediffs
        {
            get
            {
                if (!HediffsSorted)
                {
                    foreach (HediffDef hediffdef in HediffGlowMods.Keys)
                    {
                        int appindex = AllSightAffectingHediffs.IndexOf(hediffdef);
                        if (appindex >= 0)
                        {
                            AllSightAffectingHediffs.RemoveAt(appindex);
                            AllSightAffectingHediffs.Insert(0, hediffdef);
                        }
                    }
                    HediffsSorted = true;
                }
                return AllSightAffectingHediffs;
            }
        }
        #endregion

        #region SavedSettings

        public static Dictionary<ThingDef, GlowMods> RaceGlowMods = new Dictionary<ThingDef, GlowMods>();
        public static Dictionary<ThingDef, ApparelSetting> NVApparel = new Dictionary<ThingDef, ApparelSetting>();
        public static Dictionary<HediffDef, GlowMods> HediffGlowMods = new Dictionary<HediffDef, GlowMods>();

        public static FloatRange MultiplierCaps = new FloatRange(0.8f, 1.2f);

        #endregion

        #region Cached GlowMods for Races
        private static Dictionary<ThingDef, EyeGlowMods> raceEyeGlowModsCached = new Dictionary<ThingDef, EyeGlowMods>();
        
        internal static EyeGlowMods GetRaceNightVisionMod(ThingDef race, int numEyes)
        {
            if (!raceEyeGlowModsCached.ContainsKey(race))
            {
                Log.Message("No Key in Race Cache");
                if (RaceGlowMods.TryGetValue(race, out GlowMods raceGlowMods))
                {
                    Log.Message("Found in race dict: " + race.defName + " with zero " + raceGlowMods.ZeroLight + " & full " + raceGlowMods.FullLight);
                    raceEyeGlowModsCached[race] = new EyeGlowMods(raceGlowMods, numEyes);
                }
                else
                {
                    Log.Message("Didn't find");
                    raceEyeGlowModsCached[race] = new EyeGlowMods()
                    {
                        Setting = GlowMods.Options.NVNone,
                        NumOfEyesNormalisedFor = numEyes
                    };
                }
            }
            return raceEyeGlowModsCached[race];
        }

        #endregion
        
        #region Settings GUI

        #region GUI And Settings specific Fields
        private enum Tab
        {
            General,
            Races,
            Apparel,
            Bionics,
            Debug
        }
        private static Tab tab;
        private static List<TabRecord> tabsList = new List<TabRecord>();
        private static Vector2 scrollPosition = Vector2.zero;
        private static int? NumberOfCustomHediffs = null;
        private static int? NumberOfCustomRaces = null;

        #endregion
        
        #region Settings Window Strings, Cache and Consts
        private float? minCache;
        private float? maxCache;
        private float? nvZeroCache;
        private float? nvFullCache;
        private float? psZeroCache;
        private float? psFullCache;

        private static string minCapLabel = "NVSettingsMinCapLabel".Translate();
        private static string maxCapLabel = "NVSettingsMaxCapLabel".Translate();
        private static string zeroLabel = "NVSettingsZeroLabel".Translate();
        private static string fullLabel = "NVSettingsFullLabel".Translate();
        private static string multiExplanation = "NVMoveWorkSpeedMultipliers".Translate();
        private static string modExplanation = "NVMoveWorkSpeedModifiers".Translate();


        private const float rowHeight = 40f;
        private const float rowGap = 10f;
        #endregion

        #region SettingsWindow
        public void DoSettingsWindowContents(Rect inRect)
        {
            
            tabsList.Clear();
            tabsList.Add(new TabRecord("General", delegate{tab = Tab.General;}, tab == Tab.General));
            tabsList.Add(new TabRecord("Races", delegate { tab = Tab.Races; }, tab == Tab.Races));
            tabsList.Add(new TabRecord("Apparel", delegate { tab = Tab.Apparel; }, tab == Tab.Apparel));
            tabsList.Add(new TabRecord("Bionics", delegate { tab = Tab.Bionics; }, tab == Tab.Bionics));
            //TODO Remove for release
            if (Prefs.DevMode)
            {
                tabsList.Add(new TabRecord("Debug", delegate { tab = Tab.Debug; }, tab == Tab.Debug));
            }

            inRect.yMin += 32f;
            Widgets.DrawMenuSection(inRect);
            TabDrawer.DrawTabs(inRect, tabsList);

            inRect = inRect.ContractedBy(17f);
            GUI.BeginGroup(inRect);
            TextAnchor Anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            switch (tab)
            {
                default:
                case Tab.General:
                    DrawGeneralTab(inRect);
                    break;
                case Tab.Races:
                    DrawRaceTab(inRect);
                    break;
                case Tab.Apparel:
                    DrawApparelTab(inRect);
                    break;
                case Tab.Bionics:
                    DrawHediffTab(inRect);
                    break;
                case Tab.Debug:
                    DrawDebugTab(inRect);
                    break;
            }

            Text.Anchor = Anchor;
            GUI.EndGroup();
           
        }
        #endregion

        #region General Tab
        private void DrawGeneralTab(Rect inRect)
        {
            //TODO DrawGeneralTab: Add reset defaults and thought settings
            GameFont font = Text.Font;
            TextAnchor anchor = Text.Anchor;

            //Caps
            if (minCache == null)
            {
                minCache = MultiplierCaps.min * 100;
            }
            if (maxCache == null)
            {
                maxCache = MultiplierCaps.max * 100;
            }
            Rect rowRect = new Rect(inRect.width * 0.05f, inRect.height * 0.05f, inRect.width * 0.9f, rowHeight);
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Small;
            Widgets.Label(rowRect, "NVCapsExp".Translate());

            rowRect.y += rowHeight + rowGap;
            minCache = Widgets.HorizontalSlider(rowRect, (float)minCache, 1f, 100f, true, String.Format(minCapLabel, minCache), "1%", "100%", 1);
            rowRect.y += rowHeight;
            maxCache = Widgets.HorizontalSlider(rowRect, (float)maxCache, 100f, 200f, true, String.Format(maxCapLabel, maxCache), "100%", "200%", 1);

            rowRect.y += rowHeight;
            Widgets.DrawLineHorizontal(rowRect.x, rowRect.y, rowRect.width);
            rowRect.y += rowGap;


            //Night Vision Settings
            if (nvZeroCache == null)
            {
                nvZeroCache = ModToMultiPercent(GlowMods.nvZeroLightMod, true);
            }
            if (nvFullCache == null)
            {
                nvFullCache = ModToMultiPercent(GlowMods.nvFullLightMod, false);
            }

            Widgets.Label(rowRect, String.Format(multiExplanation, GlowMods.Options.NVNightVision.ToString().Translate().ToLower()));
            rowRect.y += rowHeight + rowGap;
            nvZeroCache = Widgets.HorizontalSlider(rowRect, (float)nvZeroCache, (float)minCache, (float)maxCache, true, String.Format(zeroLabel, nvZeroCache), AsPercent(minCache), AsPercent(maxCache), 1);
            rowRect.y += rowHeight;
            nvFullCache = Widgets.HorizontalSlider(rowRect, (float)nvFullCache, (float)minCache, (float)maxCache, true, String.Format(fullLabel, nvFullCache), AsPercent(minCache), AsPercent(maxCache), 1);

            rowRect.y += rowHeight;
            Widgets.DrawLineHorizontal(rowRect.x, rowRect.y, rowRect.width);
            rowRect.y += rowGap;

            //Photosensitivity settings
            if (psZeroCache == null)
            {
                psZeroCache = ModToMultiPercent(GlowMods.psZeroLightMod, true);
            }
            if (psFullCache == null)
            {
                psFullCache = ModToMultiPercent(GlowMods.psFullLightMod, false);
            }

            Widgets.Label(rowRect, String.Format(multiExplanation, GlowMods.Options.NVPhotosensitivity.ToString().Translate().ToLower()));
            rowRect.y += rowHeight + rowGap;
            psZeroCache = Widgets.HorizontalSlider(rowRect, (float)psZeroCache, (float)minCache, (float)maxCache, true, String.Format(zeroLabel, psZeroCache), AsPercent(minCache), AsPercent(maxCache), 1);
            rowRect.y += rowHeight;
            psFullCache = Widgets.HorizontalSlider(rowRect, (float)psFullCache, (float)minCache, (float)maxCache, true, String.Format(fullLabel, psFullCache), AsPercent(minCache), AsPercent(maxCache), 1);
        }
        #endregion

        #region Race Tab

        private void DrawRaceTab(Rect inRect)
        {
            int raceCount = RaceGlowMods.Count;
            if (NumberOfCustomRaces == null)
            {
                NumberOfCustomRaces = RaceGlowMods.Count(rgm => rgm.Value.IsCustom());
            }
            inRect = inRect.AtZero();
            DrawGlowModsHeader(ref inRect, "NVRaces".Translate());

            float num = inRect.y + 3f;
            Rect viewRect = new Rect(inRect.x, inRect.y, inRect.width * 0.9f, raceCount * (rowHeight + rowGap) + (float)NumberOfCustomRaces * 100f);
            Rect rowRect = new Rect(inRect.x + 6f, num, inRect.width - 12f, rowHeight);
            Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect);
            int count = 0;
            foreach (var kvp in RaceGlowMods)
            {
                rowRect.y = num;
                NumberOfCustomRaces += DrawGlowModsRow(kvp.Key.LabelCap, kvp.Value, rowRect, ref num, true);
                count++;
                num += rowHeight + rowGap;
                if (count < raceCount - 1)
                {
                    Widgets.DrawLineHorizontal(rowRect.x + 6f, num - 5.5f, rowRect.width - 12f);
                }
            }
            Widgets.EndScrollView();
        }
        #endregion

        #region Apparel Tab

        private void DrawApparelTab(Rect inRect)
        {
            Text.Anchor = TextAnchor.LowerCenter;
            int apparelCount = GetAllHeadgear.Count;
            Rect headerRect = new Rect(24f, 0f, inRect.width - 64f, 36f);
            Rect leftRect = headerRect.LeftPart(0.4f);
            Rect midRect = headerRect.RightPart(0.6f).LeftHalf().RightPart(0.8f);
            Rect rightRect = headerRect.RightPart(0.6f).RightHalf().LeftPart(0.8f);
            Widgets.Label(leftRect, "NVApparel".Translate());
            Widgets.Label(midRect, "NVNullPS".Translate());
            Widgets.Label(rightRect, "NVGiveNV".Translate());

            Widgets.DrawLineHorizontal(headerRect.x + 12f, headerRect.yMax + 4f, headerRect.xMax - 64f);

            Text.Anchor = TextAnchor.MiddleCenter;
            Rect viewRect = new Rect(32f, 48f, inRect.width - 64f, apparelCount * 48f);
            Rect scrollRect = new Rect(12f, 48f, inRect.width - 12f, inRect.height - 48f);

            float checkboxSize = 20f;
            float leftBoxX = midRect.center.x + 20f;
            float rightBoxX = rightRect.center.x + 20f;

            float num = 48f;
            Widgets.BeginScrollView(scrollRect, ref scrollPosition, viewRect);
            foreach (ThingDef appareldef in GetAllHeadgear)
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
        #endregion

        #region HediffTab
        private void DrawHediffTab(Rect inRect)
        {
            int hediffcount = GetAllHediffs.Count;
            if (NumberOfCustomHediffs == null)
            {
                NumberOfCustomHediffs = HediffGlowMods.Count(hgm => hgm.Value.IsCustom());
            }
            inRect = inRect.AtZero();
            DrawGlowModsHeader(ref inRect, "NVHediffs".Translate());
            float num = inRect.y + 3f;
            Rect viewRect = new Rect(inRect.x, inRect.y, inRect.width * 0.9f, hediffcount * (rowHeight + rowGap) + (float)NumberOfCustomHediffs * 100f);
            Rect rowRect = new Rect(inRect.x + 6f, num, inRect.width - 12f, rowHeight);
            Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect);
            for (int i = 0; i < hediffcount; i++)
            {
                HediffDef hediffdef = GetAllHediffs[i];
                rowRect.y = num;
                if (HediffGlowMods.TryGetValue(hediffdef, out GlowMods hediffmods))
                {
                    NumberOfCustomHediffs += DrawGlowModsRow(hediffdef.LabelCap, hediffmods, rowRect, ref num, false);

                }
                else 
                {
                    GlowMods temp;
                    if (AllEyeHediffs.Contains(hediffdef))
                    {
                        temp = new EyeGlowMods();
                    }
                    else
                    {
                        temp = new GlowMods();
                    }
                    NumberOfCustomHediffs += DrawGlowModsRow(hediffdef.LabelCap, temp, rowRect, ref num, false);
                    if (!temp.IsNone())
                    {
                        HediffGlowMods[hediffdef] = temp;
                    }
                }
                num += rowHeight + rowGap;
                if (i < hediffcount - 1)
                {
                    Widgets.DrawLineHorizontal(rowRect.x + 6f, num - (rowGap/ 2 - 0.5f), rowRect.width - 12f);
                }
            }
            Widgets.EndScrollView();
        }
        #endregion

        #region Row Draw functions for hediff and race tabs 

        private void DrawGlowModsHeader(ref Rect inRect, string label)
        {
            Rect headerRect = new Rect(inRect.x + 6f, inRect.y, inRect.width - 12f, inRect.height * 0.07f);

            float labelwidth = headerRect.width * 0.3f;
            Rect labelRect = new Rect(headerRect.x, headerRect.y, labelwidth, headerRect.height);
            float xSettings = headerRect.x + labelwidth + 1f + (headerRect.width) * 0.05f;
            Rect settingsRect = new Rect(xSettings, headerRect.y, headerRect.xMax - xSettings, headerRect.height);

            GameFont font = Text.Font;
            Text.Font = GameFont.Medium;
            Widgets.Label(labelRect, label);
            Widgets.Label(settingsRect, "NVGlowModOptions".Translate());
            Text.Font = font;

            Widgets.DrawLineHorizontal(headerRect.x + 10f, headerRect.yMax + rowGap/4 - 0.5f, headerRect.width - 20f);
            inRect.yMin += headerRect.height + rowGap/2;
        }

        #region Static Stuff for GlowMods Row

        private static int raceMax = 200;
        private static int raceMin = 1;
        private static int max = 100;

        private static string raceMaxLabel = "200%";
        private static string raceMinLabel = "1%";
        private static string maxLabel = "100%";
        
        #endregion


        /// <returns>0 if glowMods is unchanged, 1 if glowMods changed to custom, -1 if glowMods changed from custom</returns>
        private int DrawGlowModsRow(string label, GlowMods glowMods, Rect rowRect, ref float num, bool isRace)
        {
            int result = 0;
            float labelwidth = rowRect.width * 0.3f;
            Rect labelRect = new Rect(rowRect.x, rowRect.y, labelwidth, rowRect.height);

            float buttonwidth = rowRect.width * 0.14f;
            float buttongap = rowRect.width * 0.025f;
            float x = rowRect.x + labelwidth + buttongap;

            Widgets.DrawAltRect(labelRect.ContractedBy(2f));
            Widgets.Label(labelRect, label);
            Widgets.DrawLineVertical(x, rowRect.y, rowRect.height);
            //I could have done this with radio buttons, oh well
            //GlowMods.Options =  enum: default = 0; nightvis = 1; photosens = 2; custom = 3
            for (int i = 0; i < 4; i++)
            {
                GlowMods.Options iOption = (GlowMods.Options)Enum.ToObject(typeof(GlowMods.Options), i);
                x += buttongap;
                Rect buttonRect = new Rect(x, rowRect.y + 6f, buttonwidth, rowRect.height - 12f);
                if (iOption == glowMods.Setting)
                {
                    Color color = GUI.color;
                    GUI.color = Color.yellow;
                    Widgets.DrawBox(buttonRect.ExpandedBy(2f), 1);
                    GUI.color = color;
                    Widgets.DrawAtlas(buttonRect, Widgets.ButtonSubtleAtlas);
                    Widgets.Label(buttonRect, iOption.ToString().Translate());
                }
                else
                {
                    bool changesetting = Widgets.ButtonText(buttonRect, iOption.ToString().Translate());
                    if (changesetting)
                    {
                        if (glowMods.IsCustom())
                        {
                            result = -1;
                        }
                        glowMods.ChangeSetting(iOption);
                        if (glowMods.IsCustom())
                        {
                            result = 1;
                        }
                    }
                }
                x += buttonwidth;
            }
            if (glowMods.IsCustom())
            {
                num += rowHeight + rowGap;
                Rect topRect = new Rect(labelRect.xMax + 2 * buttongap, num + (rowGap * 0.5f), rowRect.width -labelRect.width -60f, rowHeight/ 2);
                Rect bottomRect = new Rect(labelRect.xMax + 2 * buttongap, topRect.yMax + rowGap, rowRect.width - labelRect.width - 60f, rowHeight / 2);

                Rect explanationRect = new Rect(labelRect.xMax - (labelRect.width * 0.9f), num, labelRect.width * 0.9f, rowHeight);
                Widgets.DrawLineVertical(labelRect.xMax + buttongap, rowRect.y + rowRect.height, rowHeight * 2 - rowGap );

                GameFont font = Text.Font;
                Text.Font = GameFont.Tiny;
                Widgets.Label(explanationRect, String.Format(isRace? multiExplanation : modExplanation, label));
                Text.Font = font;

                float zeroModAsPercent = (float)Math.Round((glowMods.ZeroLight + (isRace? DefaultZeroLightMultiplier : 0)) * 100);
                float fullModAsPercent = (float)Math.Round((glowMods.FullLight + (isRace? DefaultFullLightMultiplier : 0)) * 100);
                
                if (isRace)
                {
                    zeroModAsPercent = Widgets.HorizontalSlider(topRect, zeroModAsPercent, raceMin, raceMax, true, String.Format(zeroLabel, zeroModAsPercent), raceMinLabel, raceMaxLabel, 1);
                    fullModAsPercent = Widgets.HorizontalSlider(bottomRect, fullModAsPercent, raceMin, raceMax, true, String.Format(fullLabel, fullModAsPercent), raceMinLabel, raceMaxLabel, 1);
                    Color color = GUI.color;
                    GUI.color = new Color(1f, 1f, 1f, 0.5f);
                    Widgets.DrawLineVertical(topRect.x - 1f + (glowMods.DefaultZeroLightMod + DefaultZeroLightMultiplier)* 100 * topRect.width / (raceMax - raceMin), topRect.y, topRect.height);
                    Widgets.DrawLineVertical(bottomRect.x - 1f + (glowMods.DefaultFullLightMod + DefaultFullLightMultiplier) * 100 * bottomRect.width / (raceMax - raceMin), bottomRect.y, bottomRect.height);
                    GUI.color = color;
                }
                else
                {
                    int numEyes = 1;
                    if (glowMods is EyeGlowMods)
                    {
                        numEyes = ((EyeGlowMods)glowMods).NumOfEyesNormalisedFor;
                    }
                    int min = (int)Math.Round(-100f / numEyes + 1);
                    string minLabel = min + "%";
                    zeroModAsPercent = Widgets.HorizontalSlider(topRect, zeroModAsPercent, min, max, true, String.Format(zeroLabel, zeroModAsPercent), minLabel, maxLabel, 1);
                    fullModAsPercent = Widgets.HorizontalSlider(bottomRect, fullModAsPercent, min, max, true, String.Format(fullLabel, fullModAsPercent), minLabel, maxLabel, 1);
                }
                
                if (!Mathf.Approximately(zeroModAsPercent / 100, glowMods.ZeroLight))
                {
                    glowMods.ZeroLight = (zeroModAsPercent / 100) - (isRace ? DefaultZeroLightMultiplier : 0);
                }
                if (!Mathf.Approximately(fullModAsPercent / 100, glowMods.FullLight))
                {
                    glowMods.FullLight = (fullModAsPercent / 100) - (isRace ? DefaultFullLightMultiplier : 0);
                }
                num += rowHeight * 0.7f /*+ rowGap*/;
            }
            return result;
        }

        #endregion

        #region Debug Tab
        internal static bool LogPawnComps = false;
        private void DrawDebugTab(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect.AtZero());
            listing.CheckboxLabeled("NVDebugLogComps".Translate(), ref LogPawnComps);
            listing.GapLine();
            listing.LabelDouble("NVDebugAverageTime".Translate(), (HarmonyPatches.TotalGlFactorNanoSec / HarmonyPatches.TotalTicks).ToString("00 ns/tick"));
            listing.Label($"1 tick = {1000000000 / 60:00} ns");
            listing.GapLine();
            listing.End();
        }
        #endregion
        #endregion

        #region Helper Functions
        private static string AsPercent<T>(T str)
        {
            return $"{str.ToString()}%";
        }

        private float ModToMultiPercent(float mod, bool isZeroLight)
        {
            return (float)Math.Round(100f * (mod + (isZeroLight? DefaultZeroLightMultiplier : DefaultFullLightMultiplier)));
        }

        private float MultiPercentToMod(float multipercent, bool isZeroLight)
        {
            return ((float)Math.Round(multipercent / 100f, 2)) - (isZeroLight ? DefaultZeroLightMultiplier : DefaultFullLightMultiplier);
        }

        #endregion

        #region Saving And Loading

        #region ExposeData - Saves all data for the mod
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref MultiplierCaps.min, "LowerLimit", 0.8f);
            Scribe_Values.Look(ref MultiplierCaps.max, "UpperLimit", 1.2f);

            GlowMods.SaveOrLoadSettings();

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                RaceGlowMods.RemoveAll(entry => entry.Key == null || entry.Value == null);
                NVApparel.RemoveAll(entry => entry.Key == null || entry.Value == null || entry.Value.IsRedundant());
                HediffGlowMods.RemoveAll(entry => entry.Key == null || entry.Value == null || entry.Value.IsRedundant());

            }
            DictionaryScribe(ref RaceGlowMods, "Race");
            DictionaryScribe(ref NVApparel, "Apparel");
            DictionaryScribe(ref HediffGlowMods, "Hediffs");
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                int numOfRemoved1 = RaceGlowMods?.RemoveAll(kvp => kvp.Key == null || kvp.Value == null) ?? 0;
                if (numOfRemoved1 > 0)
                {
                    Log.Message("Night Vision: Removed " + numOfRemoved1 + " null defs from race dictionary." +
                        " This is to be expected if you have removed a race mod.");
                    Scribe.mode = LoadSaveMode.Saving;
                    DictionaryScribe(ref RaceGlowMods, "Race");
                    Scribe.mode = LoadSaveMode.LoadingVars;

                }
                int numOfRemoved2 = NVApparel?.RemoveAll(appset => appset.Key == null || appset.Value == null) ?? 0;
                if (numOfRemoved2 > 0)
                {
                    Log.Message("Night Vision: Removed " + numOfRemoved2 + " null entries from apparel dictionary." +
                        " This is to be expected if you have removed a mod.");
                    Scribe.mode = LoadSaveMode.Saving;
                    DictionaryScribe(ref NVApparel, "Apparel");
                    Scribe.mode = LoadSaveMode.LoadingVars;
                }
                Log.Message("HediffGlowMods left: " + HediffGlowMods.ToStringSafeEnumerable());
                int numOfRemoved3 = HediffGlowMods?.RemoveAll(entry => entry.Key == null || entry.Value == null) ?? 0;
                if (numOfRemoved3 > 0)
                {
                    Log.Message("Night Vision: Removed " + numOfRemoved3 + " null entries from hediffdef dictionary" +
                        " This is to be expected if you have removed a mod or changed Night Vision mod settings.");
                    Scribe.mode = LoadSaveMode.Saving;
                    DictionaryScribe(ref HediffGlowMods, "Hediffs");
                    Scribe.mode = LoadSaveMode.LoadingVars;
                    Log.Message("HediffGlowMods left: " + HediffGlowMods.ToStringSafeEnumerable());
                }
            }
        }
        #endregion

        #region Cache cleanup and settings update
        /// <summary>
        /// Sets new settings
        /// Clears all cached stuff
        /// </summary>
        internal void DoPreWriteTasks()
        {
            
            MultiplierCaps.min = minCache != null? (float)Math.Round((float)minCache / 100, 2) : MultiplierCaps.min;
            MultiplierCaps.max = maxCache != null? (float)Math.Round((float)maxCache / 100, 2): MultiplierCaps.max;
            
            GlowMods.SetNVZeroLightMod = nvZeroCache != null? MultiPercentToMod((float)nvZeroCache, true) : GlowMods.nvZeroLightMod;
            GlowMods.SetNVFullLightMod = nvFullCache != null? MultiPercentToMod((float)nvFullCache, false) : GlowMods.nvFullLightMod;
            GlowMods.SetPSZeroLightMod = psZeroCache != null? MultiPercentToMod((float)psZeroCache, true) : GlowMods.psZeroLightMod;
            GlowMods.SetPSFullLightMod = psFullCache != null? MultiPercentToMod((float)psFullCache, false) : GlowMods.psFullLightMod;
            
            minCache = null;
            maxCache = null;
            nvZeroCache = null;
            nvFullCache = null;
            psZeroCache = null;
            psFullCache = null;

            raceEyeGlowModsCached.Clear();

            HediffsSorted = false;
            ApparelSorted = false;
            NumberOfCustomHediffs = null;
            if (Current.ProgramState == ProgramState.Playing)
            {
                SetDirtyAllComps();
            }
        }

        /// <summary>
        /// So that the comps will update with the new settings, sets all the comps dirty
        /// </summary>
        private void SetDirtyAllComps()
        {
            foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods.Where(pwn => pwn.RaceProps.Humanlike))
            {
                if (pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
                {
                    comp.SetDirty();
                }
            }
        }
        #endregion

        #region Dictionary Scribe And Data class
        private void DictionaryScribe<K, V>( ref Dictionary<K, V> dictionary, string label) where K : Def where V : INVSaveCheck
        {
            List<SaveLoadThing<K, V>> tempList = new List<SaveLoadThing<K, V>>();  
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                foreach (var kvp in dictionary)
                {
                    if (kvp.Value.ShouldBeSaved())
                    {
                        tempList.Add(new SaveLoadThing<K, V>(kvp.Key, kvp.Value));
                    }
                }
            }
            Scribe_Collections.Look(ref tempList, label, LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                dictionary.Clear();
                for (int i = 0; i < tempList.Count; i++)
                {
                    var entry = tempList[i];
                    if (entry != null && entry.HasValue())
                    {
                        dictionary[(K)entry.Key] = entry.Value;
                    }
                }
            }
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (tempList != null)
                {
                    tempList.Clear();
                    tempList = null;
                }
            }
        }
    }

    public class SaveLoadThing<K, V> : IExposable where K : Def  where V : INVSaveCheck
    {
        public SaveLoadThing(){}
        public SaveLoadThing(K key, V value)
        {
            Key = key;
            Value = value;
        }
        public bool HasValue()
        {
            return Key != null && Value != null;
        }
        public K Key;
        public V Value;
        public void ExposeData()
        {
            if (typeof(K) == typeof(HediffDef))
            {
                HediffDef hediffDef = (HediffDef)((object)Key);
                Scribe_Defs.Look(ref hediffDef, "hediffDef");
                Key = (K)((object)hediffDef);
            }
            else if (typeof(K) == typeof(ThingDef))
            {
                ThingDef thingdef = (ThingDef)((object)Key);
                Scribe_Defs.Look(ref thingdef, "thingDef");
                Key = (K)((object)thingdef);
            }
            Scribe_Deep.Look(ref Value, typeof(V).ToString(), new object[0]);
        }
    }
    #endregion

        #endregion

}
