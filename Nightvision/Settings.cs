using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NightVision.Comps;
using NightVision.LightModifiers;
using RimWorld;
using UnityEngine;
using Verse;

namespace NightVision
{
    internal class NightVisionSettings : ModSettings
    {
        #region Constants and Readonly

        internal const float DefaultZeroLightMultiplier = 0.8f;
        internal const float DefaultFullLightMultiplier = 1f;
        private const float DefaultMinCap = 0.5f;
        private const float DefaultMaxCap = 2f;

        #endregion

        #region Class Fields
        [UsedImplicitly] public static NightVisionSettings Instance;
        [UsedImplicitly]
        public NightVisionSettings()
        {
            Instance = this;
        }
        #endregion

        #region Def hash sets
        //AllEyeHediffs is a subset of AllSightAffectingHediffs
        internal static HashSet<HediffDef> AllEyeHediffs = new HashSet<HediffDef>();
        internal static HashSet<HediffDef> AllSightAffectingHediffs = new HashSet<HediffDef>();
        internal static HashSet<ThingDef> AllEyeCoveringHeadgearDefs = new HashSet<ThingDef>();

        private static List<ThingDef> _headgearCache;
        internal static List<ThingDef> GetAllHeadgear
        {
            get
            {
                if (_headgearCache == null)
                {
                    _headgearCache = new List<ThingDef>(AllEyeCoveringHeadgearDefs);
                    foreach (ThingDef appareldef in NVApparel.Keys)
                    {
                        int appindex = _headgearCache.IndexOf(appareldef);
                        if (appindex > 0)
                        {
                            _headgearCache.RemoveAt(appindex);
                            _headgearCache.Insert(0, appareldef);
                        }
                    }
                }
                return _headgearCache;
            }
        }

        private static List<HediffDef> _allHediffsCache;
        internal static List<HediffDef> GetAllHediffs
        {
            get
            {
                if (_allHediffsCache == null)
                {
                    _allHediffsCache = new List<HediffDef>(AllSightAffectingHediffs);
                    foreach (HediffDef hediffdef in HediffLightMods.Keys)
                    {
                        int appindex = _allHediffsCache.IndexOf(hediffdef);
                        if (appindex > 0)
                        {
                            _allHediffsCache.RemoveAt(appindex);
                            _allHediffsCache.Insert(0, hediffdef);
                        }
                    }
                }
                return _allHediffsCache;
            }
        }


        #endregion

        #region SavedSettings

        public static Dictionary<ThingDef, Race_LightModifiers> RaceLightMods = new Dictionary<ThingDef, Race_LightModifiers>();
        public static Dictionary<ThingDef, ApparelSetting> NVApparel = new Dictionary<ThingDef, ApparelSetting>();
        public static Dictionary<HediffDef, Hediff_LightModifiers> HediffLightMods = new Dictionary<HediffDef, Hediff_LightModifiers>();

        private static FloatRange multiplierCaps = new FloatRange(DefaultMinCap, DefaultMaxCap);
        public static FloatRange MultiplierCaps = new FloatRange(DefaultMinCap, DefaultMaxCap);


        private static bool _customCapsEnabled;
        public static bool CEDetected = false;

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
        private static Tab _tab;
        private static List<TabRecord> _tabsList = new List<TabRecord>();
        private static Vector2 _hediffScrollPosition = Vector2.zero;
        private static Vector2 _apparelScrollPosition = Vector2.zero;
        private static Vector2 _raceScrollPosition = Vector2.zero;
        private static int? _numberOfCustomHediffs;
        private static int? _numberOfCustomRaces;

        #endregion

        #region Settings Window Cache

        private float? _minCache;
        private float? _maxCache;
        private float? _nvZeroCache;
        private float? _nvFullCache;
        private float? _psZeroCache;
        private float? _psFullCache;
        private bool _cacheInited;
        private Dictionary<Def, string> _tipStringHolder;

        private Dictionary<Def, string> TipStringHolder
        {
            get => _tipStringHolder ?? (_tipStringHolder = new Dictionary<Def, string>());
                set => _tipStringHolder = value;
            }


        #endregion

        #region Settings Window Strings and Constants
        //TODO use translate's string format function
        private static readonly string ZeroLabel = "NVZeroLabel".Translate()+ " = {0:+#;-#;0}%";
        private static readonly string FullLabel = "NVFullLabel".Translate()+" = {0:+#;-#;0}%";
        private static readonly string ZeroMultiLabel = "NVZeroLabel".Translate()+" = x{0:##}%";
        private static readonly string FullMultiLabel = "NVFullLabel".Translate()+" = x{0:##}%";

        private const string XLabel = "x{0:#0}%";
        private const string Alabel = "{0:+#;-#;0}%";

        private const float IndicatorSize = 12f;
        private const float RowHeight = 40f;
        private const float RowGap = 10f;


        #endregion

        #region SettingsWindow
        public void DoSettingsWindowContents(Rect inRect)
        {
            
            _tabsList.Clear();
            _tabsList.Add(new TabRecord("General", delegate{_tab = Tab.General;}, _tab == Tab.General));
            _tabsList.Add(new TabRecord("Races", delegate { _tab = Tab.Races; }, _tab == Tab.Races));
            _tabsList.Add(new TabRecord("Apparel", delegate { _tab = Tab.Apparel; }, _tab == Tab.Apparel));
            _tabsList.Add(new TabRecord("Bionics", delegate { _tab = Tab.Bionics; }, _tab == Tab.Bionics));
            
            if (Prefs.DevMode)
            {
                _tabsList.Add(new TabRecord("Debug", delegate { _tab = Tab.Debug; }, _tab == Tab.Debug));
            }

            if (!_cacheInited)
            {
                InitSettingsCache();
                _cacheInited = true;
            }

            inRect.yMin += 32f;
            Widgets.DrawMenuSection(inRect);
            TabDrawer.DrawTabs(inRect, _tabsList, 1);

            inRect = inRect.ContractedBy(17f);
            GUI.BeginGroup(inRect);
            GameFont font = Text.Font;
            TextAnchor anchor = Text.Anchor;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            switch (_tab)
            {
                default:
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
            Text.Font = font;
            Text.Anchor = anchor;
            GUI.EndGroup();
           
        }
        #endregion

        #region General Tab
        private void DrawGeneralTab(Rect inRect)
        {
            //TODO DrawGeneralTab: Add reset defaults and thought settings
            //TODO DrawGeneralTab: Move cap and multiplier settings to their own tab
            TextAnchor anchor = Text.Anchor;
            float rowHeight = RowHeight * 0.6f;
            Rect rowRect = new Rect(inRect.width * 0.05f, inRect.height * 0.05f, inRect.width * 0.9f, rowHeight);
            Text.Anchor = TextAnchor.MiddleLeft;

            Widgets.Label(rowRect, "NVVanillaMultiExp".Translate());
            rowRect.y += rowHeight + RowGap;
            Widgets.DrawLineHorizontal(rowRect.x + 24f, rowRect.y, rowRect.width - 48f);
            rowRect.y += RowGap;

            //Night Vision Settings

            Widgets.Label(rowRect, "NVMoveWorkSpeedMultipliers".Translate(LightModifiersBase.Options.NVNightVision.ToString().Translate()).ToLower().CapitalizeFirst());
            rowRect.y += rowHeight + RowGap;
            _nvZeroCache = Widgets.HorizontalSlider(rowRect, (float)_nvZeroCache, (float)_minCache, (float)_maxCache, true, String.Format(ZeroMultiLabel, _nvZeroCache), String.Format(XLabel, _minCache), String.Format(XLabel, _maxCache), 1);
            DrawIndicator(rowRect, DefaultZeroLightMultiplier, LightModifiersBase.NVDefaultOffsets[0], (float)_minCache, (float)_maxCache, IndicatorTex.DefIndicator);
            rowRect.y += rowHeight * 1.5f;
            _nvFullCache = Widgets.HorizontalSlider(rowRect, (float)_nvFullCache, (float)_minCache, (float)_maxCache, true, String.Format(FullMultiLabel, _nvFullCache), String.Format(XLabel, _minCache), String.Format(XLabel, _maxCache), 1);
            DrawIndicator(rowRect, DefaultFullLightMultiplier, LightModifiersBase.NVDefaultOffsets[1], (float)_minCache, (float)_maxCache, IndicatorTex.DefIndicator);
            rowRect.y += rowHeight *2f;
            Widgets.DrawLineHorizontal(rowRect.x + 24f, rowRect.y, rowRect.width - 48f);
            rowRect.y += RowGap;

            //Photosensitivity settings

            Widgets.Label(rowRect, "NVMoveWorkSpeedMultipliers".Translate(LightModifiersBase.Options.NVPhotosensitivity.ToString().Translate()).ToLower().CapitalizeFirst());
            rowRect.y += rowHeight * 1.5f;
            _psZeroCache = Widgets.HorizontalSlider(rowRect, (float)_psZeroCache, (float)_minCache, (float)_maxCache, true, String.Format(ZeroMultiLabel, _psZeroCache), String.Format(XLabel, _minCache), String.Format(XLabel, _maxCache), 1);
            DrawIndicator(rowRect, DefaultZeroLightMultiplier, LightModifiersBase.PSDefaultOffsets[0], (float)_minCache, (float)_maxCache, IndicatorTex.DefIndicator);

            rowRect.y += rowHeight * 1.5f;
            _psFullCache = Widgets.HorizontalSlider(rowRect, (float)_psFullCache, (float)_minCache, (float)_maxCache, true, String.Format(FullMultiLabel, _psFullCache), String.Format(XLabel, _minCache), String.Format(XLabel, _maxCache), 1);
            DrawIndicator(rowRect, DefaultFullLightMultiplier, LightModifiersBase.PSDefaultOffsets[1], (float)_minCache, (float)_maxCache, IndicatorTex.DefIndicator);

            rowRect.y += rowHeight *2f;
            Widgets.DrawLineHorizontal(rowRect.x + 24f, rowRect.y, rowRect.width - 48f);
            rowRect.y += RowGap;

            //Multiplier Limits
            Widgets.CheckboxLabeled(rowRect, "NVCustomCapsEnabled".Translate(), ref _customCapsEnabled);
            if (_customCapsEnabled)
            {
                rowRect.y += rowHeight * 1.5f;
                Text.Font = GameFont.Tiny;
                Widgets.Label(rowRect, "NVCapsExp".Translate());
                Text.Font = GameFont.Small;
                rowRect.y += rowHeight + RowGap;
                _minCache = Widgets.HorizontalSlider(rowRect, (float)_minCache, 1f, 100f, true, "NVSettingsMinCapLabel".Translate(_minCache), "1%", "100%", 1);
                DrawIndicator(rowRect, DefaultMinCap, 0f, 1f, 100f, IndicatorTex.DefIndicator);
                rowRect.y += rowHeight*1.5f;
                _maxCache = Widgets.HorizontalSlider(rowRect, (float)_maxCache, 100f, 200f, true, "NVSettingsMinCapLabel".Translate(_maxCache), "100%", "200%", 1);
                DrawIndicator(rowRect, DefaultMaxCap, 0f, 100f, 200f, IndicatorTex.DefIndicator);
            }
            rowRect.y += rowHeight *1.5f;
            Widgets.DrawLineHorizontal(rowRect.x + 24f, rowRect.y, rowRect.width - 48f);
            rowRect.y += RowGap;
            if (CEDetected)
            {
                Widgets.CheckboxLabeled(rowRect, "EnableNVForCE".Translate(), ref HarmonyPatches.NVEnabledForCE);
            }
            Text.Anchor = anchor;
        }
        #endregion

        #region Race Tab

        private void DrawRaceTab(Rect inRect)
        {
            int raceCount = RaceLightMods.Count;
            if (_numberOfCustomRaces == null)
            {
                _numberOfCustomRaces = RaceLightMods.Count(rlm => rlm.Value.IntSetting == LightModifiersBase.Options.NVCustom);
            }
            inRect = inRect.AtZero();
            DrawLightModifiersHeader(ref inRect, "NVRaces".Translate());

            float num = inRect.y + 3f;
            Rect viewRect = new Rect(inRect.x, inRect.y, inRect.width * 0.9f, raceCount * (RowHeight + RowGap) + (float)_numberOfCustomRaces * 100f);
            Rect rowRect = new Rect(inRect.x + 6f, num, inRect.width - 12f, RowHeight);
            Widgets.BeginScrollView(inRect, ref _raceScrollPosition, viewRect);
            int count = 0;
            foreach (var kvp in RaceLightMods)
                {
                    Color givenColor = GUI.color;
                if (!kvp.Value.ShouldShowInSettings)
                    {
                        if (!Prefs.DevMode)
                            {
                                continue;
                            }
                        GUI.color = Color.red;

                    }
                rowRect.y = num;
                _numberOfCustomRaces += DrawLightModifiersRow(kvp.Key, kvp.Value, rowRect, ref num, true);
                if (!kvp.Value.ShouldShowInSettings)
                    {
                        GUI.color = givenColor;
                    }
                count++;
                num += RowHeight + RowGap;
                if (count < raceCount)
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
            Widgets.BeginScrollView(scrollRect, ref _apparelScrollPosition, viewRect);
            foreach (ThingDef appareldef in GetAllHeadgear)
            {
                Rect rowRect = new Rect(scrollRect.x + 12f, num, scrollRect.width - 24f, 40);
                Widgets.DrawAltRect(rowRect);
                
                var locGUIContent = new GUIContent(appareldef.LabelCap, appareldef.uiIcon);
                Rect apparelRect = rowRect.LeftPart(0.4f);
                Widgets.Label(apparelRect, locGUIContent);
                TooltipHandler.TipRegion(apparelRect, new TipSignal( appareldef.description, apparelRect.GetHashCode()));
                Vector2 leftBoxPos = new Vector2(leftBoxX, rowRect.center.y - (checkboxSize / 2));
                Vector2 rightBoxPos = new Vector2(rightBoxX, rowRect.center.y - (checkboxSize / 2));
                if (NVApparel.TryGetValue(appareldef, out var apparelSetting))
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
                    bool nullPs = false;
                    bool giveNV = false;
                    Widgets.Checkbox(leftBoxPos, ref nullPs);
                    Widgets.Checkbox(rightBoxPos, ref giveNV);
                    if (nullPs || giveNV)
                    {
                        apparelSetting = new ApparelSetting( nullPs,  giveNV );
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
            if (_numberOfCustomHediffs == null)
            {
                _numberOfCustomHediffs = HediffLightMods.Count(hlm => hlm.Value.IntSetting == LightModifiersBase.Options.NVCustom);
            }
            inRect = inRect.AtZero();
            DrawLightModifiersHeader(ref inRect, "NVHediffs".Translate());
            float num = inRect.y + 3f;
            Rect viewRect = new Rect(inRect.x, inRect.y, inRect.width * 0.9f, hediffcount * (RowHeight + RowGap) + (float)_numberOfCustomHediffs * 100f);
            Rect rowRect = new Rect(inRect.x + 6f, num, inRect.width - 12f, RowHeight);
            Widgets.BeginScrollView(inRect, ref _hediffScrollPosition, viewRect);
            for (int i = 0; i < hediffcount; i++)
            {
                HediffDef hediffdef = GetAllHediffs[i];
                rowRect.y = num;
                if (HediffLightMods.TryGetValue(hediffdef, out Hediff_LightModifiers hediffmods))
                {
                    _numberOfCustomHediffs += DrawLightModifiersRow(hediffdef, hediffmods, rowRect, ref num, false);

                }
                else 
                {
                    Hediff_LightModifiers temp = AllEyeHediffs.Contains(hediffdef) ? new Hediff_LightModifiers {AffectsEye = true} : new Hediff_LightModifiers();
                    _numberOfCustomHediffs += DrawLightModifiersRow(hediffdef, temp, rowRect, ref num, false);
                    if (temp.IntSetting != LightModifiersBase.Options.NVNone)
                    {
                        temp.InitialiseNewFromSettings(hediffdef);
                        HediffLightMods[hediffdef] = temp;
                    }
                }
                num += RowHeight + RowGap;
                if (i < hediffcount)
                {
                    Widgets.DrawLineHorizontal(rowRect.x + 6f, num - (RowGap/ 2 - 0.5f), rowRect.width - 12f);
                }
            }
            Widgets.EndScrollView();
        }
        #endregion

        #region Row Draw functions for hediff and race tabs 

        private void DrawLightModifiersHeader(ref Rect inRect, string label)
        {
            Rect headerRect = new Rect(inRect.x + 6f, inRect.y, inRect.width - 12f, inRect.height * 0.1f);

            float labelwidth = headerRect.width * 0.3f;
            Rect labelRect = new Rect(headerRect.x, headerRect.y, labelwidth, headerRect.height);
            float xSettings = headerRect.x + labelwidth + 1f + (headerRect.width) * 0.05f;
            Rect settingsRect = new Rect(xSettings, headerRect.y, headerRect.xMax - xSettings, headerRect.height);

            GameFont font = Text.Font;
            Text.Font = GameFont.Medium;
            Widgets.Label(labelRect, label);
            Widgets.Label(settingsRect.TopPart(0.6f), "NVGlowModOptions".Translate());
            float num = settingsRect.width / 3;
            settingsRect = settingsRect.BottomPart(0.4f);
            Text.Font = GameFont.Tiny;
            settingsRect.width = num;
            Widgets.Label(settingsRect, new GUIContent("default".Translate().ToLower().CapitalizeFirst(), IndicatorTex.DefIndicator));
            settingsRect.x += num;
            Widgets.Label(settingsRect, new GUIContent("NVNightVision".Translate().ToLower().CapitalizeFirst(), IndicatorTex.NvIndicator));
            settingsRect.x += num;
            Widgets.Label(settingsRect, new GUIContent("NVPhotosensitivity".Translate().ToLower().CapitalizeFirst(), IndicatorTex.PsIndicator));
            Text.Font = font;
            Widgets.DrawLineHorizontal(headerRect.x + 10f, headerRect.yMax + RowGap/4 - 0.5f, headerRect.width - 20f);
            inRect.yMin += headerRect.height + RowGap/2;
        }

        private string GetTipString(Def def, LightModifiersBase lightModifiers)
        {
            if (TipStringHolder == null)
            {
                TipStringHolder = new Dictionary<Def, string>();
            }
            if (TipStringHolder.TryGetValue(def, out string tip))
            {
                return tip;
            }

            string result = def.description ?? def.LabelCap;

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
                    else if (Math.Abs(lightModifiers.DefaultOffsets[0]) > 0.001f
                             || Math.Abs(lightModifiers.DefaultOffsets[1]) > 0.001f)
                        {
                            result += "\n" + "NVLoadedFromFile".Translate("default".Translate(), lightModifiers.DefaultOffsets.ToStringSafeEnumerable());
                        }
                }
            else if (Math.Abs(lightModifiers.DefaultOffsets[0]) > 0.001f || Math.Abs(lightModifiers.DefaultOffsets[1]) > 0.001f  )
            {
                result += "\n" + "NVLoadedFromFile".Translate("default".Translate(), lightModifiers.DefaultOffsets.ToStringSafeEnumerable());
            }
            TipStringHolder[def] = result;
            return result;
        }

        /// <summary>
        /// 0 if LightModifiers is unchanged, 1 if LightModifiers changed to custom, -1 if LightModifiers changed from custom
        /// </summary>
        /// <param name="num">the y-coord of the rect: is increased if rect needs more space</param>
        private int DrawLightModifiersRow(Def def, LightModifiersBase lightMods, Rect rowRect, ref float num, bool isRace)
        {
            int result = 0;
            float labelwidth = rowRect.width * 0.3f;
            Rect labelRect = new Rect(rowRect.x, rowRect.y, labelwidth, rowRect.height);

            float buttonwidth = rowRect.width * 0.14f;
            float buttongap = rowRect.width * 0.025f;
            float x = rowRect.x + labelwidth + buttongap;

            Widgets.DrawAltRect(labelRect.ContractedBy(2f));
            Widgets.Label(labelRect, def.LabelCap);
            TooltipHandler.TipRegion(labelRect, new TipSignal(GetTipString(def, lightMods), labelRect.GetHashCode()));
            Widgets.DrawLineVertical(x, rowRect.y, rowRect.height);
            //LightModifiers.Options =  enum: default = 0; nightvis = 1; photosens = 2; custom = 3
            for (int i = 0; i < 4; i++)
            {
                LightModifiersBase.Options iOption = (LightModifiersBase.Options)Enum.ToObject(typeof(LightModifiersBase.Options), i);
                x += buttongap;
                Rect buttonRect = new Rect(x, rowRect.y + 6f, buttonwidth, rowRect.height - 12f);
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
                    bool changesetting = Widgets.ButtonText(buttonRect, iOption.ToString().Translate());
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
                num += RowHeight + RowGap;
                Rect topRect = new Rect(labelRect.xMax + 2 * buttongap, num , rowRect.width -labelRect.width -60f, RowHeight/ 2);
                Rect bottomRect = new Rect(labelRect.xMax + 2 * buttongap, topRect.yMax + RowGap*2, rowRect.width - labelRect.width - 60f, RowHeight / 2);

                Rect explanationRect = new Rect(labelRect.xMax - (labelRect.width * 0.95f), num, labelRect.width * 0.9f, RowHeight);
                //Color savedcolor = GUI.color;
                //GUI.color = Color.red;
                //Widgets.DrawBox(topRect);
                //GUI.color = Color.blue;
                //Widgets.DrawBox(bottomRect);
                //GUI.color = Color.yellow;
                //Widgets.DrawBox(explanationRect);
                //GUI.color = savedcolor;

                Widgets.DrawLineVertical(labelRect.xMax + buttongap, rowRect.y + rowRect.height, RowHeight * 2 - RowGap * 0.5f );

                


                float zeroModAsPercent = (float)Math.Round((lightMods.Offsets[0] + (isRace? DefaultZeroLightMultiplier : 0)) * 100);
                float fullModAsPercent = (float)Math.Round((lightMods.Offsets[1] + (isRace? DefaultFullLightMultiplier : 0)) * 100);
                
                if (isRace)
                {
                    GameFont font = Text.Font;
                    Text.Font = GameFont.Tiny;
                    Widgets.Label(explanationRect, "NVMoveWorkSpeedMultipliers".Translate("") + "\n" + "NVRaceQualifier".Translate());
                    Text.Font = font;

                    zeroModAsPercent =         
                        Widgets.HorizontalSlider(topRect, zeroModAsPercent, (float)_minCache, (float)_maxCache, true, String.Format(ZeroMultiLabel, zeroModAsPercent)
                                                    , String.Format(XLabel, _minCache), String.Format(XLabel, _maxCache), 1);
                    
                    fullModAsPercent = 
                        Widgets.HorizontalSlider(bottomRect, fullModAsPercent, (float)_minCache, (float)_maxCache, true, String.Format(FullMultiLabel, fullModAsPercent)
                                                    , String.Format(XLabel, _minCache), String.Format(XLabel, _maxCache), 1);

                    DrawIndicators(topRect, bottomRect, lightMods, (float)_minCache, (float)_maxCache, (float)_minCache, (float)_maxCache);

                }
                else
                {
                    GameFont font = Text.Font;
                    Text.Font = GameFont.Tiny;
                    //bool AffectsEye = ((Hediff_LightModifiers) lightMods).AffectsEye;
                    Widgets.Label(explanationRect, "NVMoveWorkSpeedModifiers".Translate(def.LabelCap)/* +( AffectsEye? "/n" + "NVHediffQualifier".Translate() : "" )*/);
                    Text.Font = font;
                    zeroModAsPercent = 
                        Widgets.HorizontalSlider(
                            topRect, zeroModAsPercent, (float)_minCache - 80, (float)_maxCache -80, true, String.Format(ZeroLabel, zeroModAsPercent), String.Format(Alabel, _minCache - 80), String.Format(Alabel, _maxCache - 80), 1);
                    

                    fullModAsPercent = 
                        Widgets.HorizontalSlider(
                            bottomRect, fullModAsPercent, (float)_minCache - 100, (float)_maxCache -100, true, String.Format(FullLabel, fullModAsPercent), String.Format(Alabel, _minCache - 100), String.Format(Alabel, _maxCache - 100), 1);

                    DrawIndicators(topRect, bottomRect, lightMods, (float)_minCache, (float)_maxCache, (float)_minCache, (float)_maxCache);
                }
                
                if (!Mathf.Approximately(zeroModAsPercent / 100, lightMods.Offsets[0]))
                {
                    lightMods[0] = (zeroModAsPercent / 100) - (isRace ? DefaultZeroLightMultiplier : 0);
                }
                if (!Mathf.Approximately(fullModAsPercent / 100, lightMods.Offsets[1]))
                {
                    lightMods[1] = (fullModAsPercent / 100) - (isRace ? DefaultFullLightMultiplier : 0);
                }
                num += RowHeight * 0.9f /*+ rowGap*/;
            }
            return result;
        }

        #endregion

        #region Debug Tab
        internal static bool LogPawnComps;
        private List<Pawn> _allPawns;
        private Vector2 _debugScrollPos = Vector2.zero;
        float _maxY;
        private void DrawDebugTab(Rect inRect)
        {
            bool playing = Current.ProgramState == ProgramState.Playing;
            inRect = inRect.AtZero();
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.CheckboxLabeled("NVDebugLogComps".Translate(), ref LogPawnComps);
            listing.GapLine();
            listing.LabelDouble("NVDebugAverageTime".Translate(), (HarmonyPatches.TotalGlFactorNanoSec / HarmonyPatches.TotalTicks).ToString("00 ns/tick"));
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
                    height = 25 * _allPawns.Count + 200 * _allPawns.FindAll(pawn => pawn.GetComp<Comp_NightVision>() != null).Count;
                }
                else
                {
                    height = _maxY;
                }
                
                Rect remainRect = new Rect(inRect.x, listingY + 5f, inRect.width, inRect.yMax - listingY + 5f);
                Rect viewRect = new Rect(remainRect.x + 6f, remainRect.y, remainRect.width -12f, height);
                Rect rowRect = new Rect(remainRect.x + 10f, remainRect.y + 3f, remainRect.width - 20f, RowHeight/ 2);
                Widgets.BeginScrollView(remainRect, ref _debugScrollPos, viewRect);
                Text.Font = GameFont.Tiny;
                foreach (Pawn pawn in _allPawns)
                {
                    Widgets.Label(rowRect.LeftPart(0.1f), pawn.Name.ToStringShort);
                    if (pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
                    {
                        Rect rightRect = rowRect.RightPart(0.8f);
                        Widgets.Label( rightRect,
                            $"Number of eyes: {comp.NumberOfRemainingEyes}/{comp.RaceSightParts.Count}   Natural modifier: {comp.NaturalLightModifiers.IntSetting}");
                        rightRect.y += RowHeight / 2;
                        Widgets.Label( rightRect, 
                            $"0% light modifier: {comp.ZeroLightModifier}   100% light modifier: {comp.FullLightModifier}");
                        rightRect.y += RowHeight / 2;
                        Widgets.Label(rightRect,
                            $"Psych: {comp.PsychDark()}      Apparel: NightVis: {comp.ApparelGrantsNV}   Anti-brightness: {comp.ApparelNullsPS}");
                        rightRect.y += RowHeight / 2;
                        Widgets.Label(rightRect,
                            $"Health (pre-cap) [0%,100%] : NightV mod: [{comp.NvhediffMods[0]}, {comp.NvhediffMods[1]}]   PhotoS mod: [{comp.PshediffMods[0]}, {comp.PshediffMods[1]}]   Custom: [{comp.HediffMods[0]},{comp.HediffMods[1]}]");
                        rightRect.y += RowHeight / 2;
                        Widgets.Label(rightRect,
                            "Body Parts & their hediffs");
                        foreach (var hedifflist in comp.PawnsNVHediffs)
                        {
                            rightRect.y += RowHeight / 2;
                            Widgets.Label(rightRect, $"  Body part: {hedifflist.Key} has hediffs: ");
                            foreach (var hediff in hedifflist.Value)
                            {
                                if(HediffLightMods.TryGetValue(hediff, out Hediff_LightModifiers value))
                                {
                                    rightRect.y += RowHeight / 2;
                                    Widgets.Label(rightRect, $"    {hediff.LabelCap}: current setting = {value.IntSetting}");
                                }
                                else
                                {
                                    rightRect.y += RowHeight / 2;
                                    Widgets.Label(rightRect, $"    {hediff.LabelCap}: No Setting");
                                }
                            }
                        }
                        rightRect.y += RowHeight / 2;
                        Widgets.Label(rightRect,
                            "Eye covering or nightvis Apparel:");
                        foreach (var apparel in comp.PawnsNVApparel)
                        {
                            if (NVApparel.TryGetValue(apparel.def, out ApparelSetting appSet))
                            {
                                rightRect.y += RowHeight / 2;
                                Widgets.Label(rightRect, $"  {apparel.LabelCap}: nightvis: {appSet.GrantsNV}  anti-bright: {appSet.NullifiesPS}  - def file setting: NV:{appSet.CompGrantsNV} A-B:{appSet.CompNullifiesPS}");
                            }
                            else
                            {
                                rightRect.y += RowHeight / 2;
                                Widgets.Label(rightRect, $"  {apparel.LabelCap}: No Setting");
                            }
                            
                        }
                        rightRect.y += RowHeight / 2;
                        rowRect.y = rightRect.y;
                    }
                    else
                    {
                        Widgets.Label(rowRect.RightHalf(), "No Night Vision component found.");
                    }

                    rowRect.y += RowHeight / 2;
                    Widgets.DrawLineHorizontal(rowRect.x + 10f, rowRect.y, rowRect.width - 20f);
                    rowRect.y += RowHeight / 2;
                }
                if (Math.Abs(_maxY) < 0.001)
                {
                    _maxY = rowRect.yMax;
                }
                Widgets.EndScrollView();
            }
        }
        #endregion
        #endregion

        #region Helper Functions

        private static float ModToMultiPercent(float mod, bool isZeroLight)
        {
            return (float)Math.Round(100f * (mod + (isZeroLight? DefaultZeroLightMultiplier : DefaultFullLightMultiplier)));
        }

        private static float MultiPercentToMod(float multipercent, bool isZeroLight)
        {
            return ((float)Math.Round(multipercent / 100f, 2)) - (isZeroLight ? DefaultZeroLightMultiplier : DefaultFullLightMultiplier);
        }
        
        ///// <param name="baseVal">default multiplier as decimal</param>
        ///// <param name="modVal">default modifier as decimal</param>
        //private void DrawIndicator(Rect rowRect, float baseVal, float modVal, float min, float max)
        //{
        //    //min + range * fraction = value = base * 100 + mod * 100
        //    // let b = base * 100, m = mod * 100
        //    // => fraction = (b + m - min) / range
        //    // => pos = rectX + rectWidth * ( b + m - min ) / range
        //    //Also modify the rectX by +6f and rectWidth by -12f to accomodate the offset induced by the slider thumb (which has width 12f)

        //    float posOfDefault = rowRect.x +6f + (rowRect.width - 12f)* ((baseVal + modVal) * 100 - min) / (max - min);
        //    Color color = GUI.color;
        //    GUI.color = Overlay;
        //    Widgets.DrawLineVertical( posOfDefault, rowRect.y + rowRect.height*0.4f, rowRect.height*0.5f);
        //    GUI.color = color;
        //}

        private void DrawIndicator(Rect rowRect, float baseVal, float modVal, float min, float max, Texture2D indicator)
        {
            //min + range * fraction = value = base * 100 + mod * 100
            // let b = base * 100, m = mod * 100
            // => fraction = (b + m - min) / range
            // => pos = rectX + rectWidth * ( b + m - min ) / range
            //Also modify the rectX by +6f and rectWidth by -12f to accomodate the offset induced by the slider thumb (which has width 12f)

            float posOfDefault = rowRect.x + 6f + (rowRect.width - 12f) * ((baseVal + modVal) * 100 - min) / (max - min);
            //Color color = GUI.color;
            //GUI.color = Overlay;
            //Widgets.DrawLineVertical(posOfDefault, rowRect.y + rowRect.height * 0.4f, rowRect.height * 0.5f);
            //GUI.color = color;
            rowRect.position = new Vector2(posOfDefault - 0.5f * IndicatorSize, rowRect.y + rowRect.height * 0.95f );
            rowRect.width = IndicatorSize;
            rowRect.height = IndicatorSize;
            Widgets.DrawTextureFitted(rowRect, indicator, 1);
        }
        private void DrawIndicators(Rect zeroRect, Rect fullRect, LightModifiersBase lightModifiers, float minZero, float maxZero, float minFull, float maxFull)
            {
                //int eyeCount = lightModifiers is Race_LightModifiers rlm ? rlm.EyeCount : 1; 
            //Draw indicators on zero light rect
            DrawIndicator(zeroRect,  DefaultZeroLightMultiplier,  LightModifiersBase.PSLightModifiers[0],  minZero,  maxZero,  IndicatorTex.PsIndicator);
            DrawIndicator(zeroRect, DefaultZeroLightMultiplier,  LightModifiersBase.NVLightModifiers[0],  minZero,  maxZero,  IndicatorTex.NvIndicator);
            DrawIndicator(zeroRect, DefaultZeroLightMultiplier,  lightModifiers.DefaultOffsets[0],  minZero,  maxZero,  IndicatorTex.DefIndicator);
            //Draw indicators on full light rect
            DrawIndicator(fullRect, DefaultFullLightMultiplier, LightModifiersBase.PSLightModifiers[1], minFull, maxFull, IndicatorTex.PsIndicator);
            DrawIndicator(fullRect, DefaultFullLightMultiplier, LightModifiersBase.NVLightModifiers[1], minFull, maxFull, IndicatorTex.NvIndicator);
            DrawIndicator(fullRect, DefaultFullLightMultiplier, lightModifiers.DefaultOffsets[1], minFull, maxFull, IndicatorTex.DefIndicator);
        }
        #endregion

        #region Saving And Loading

        #region ExposeData - Saves all data for the mod
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _customCapsEnabled, "CustomLimitsEnabled");
            if (_customCapsEnabled)
            {
                Scribe_Values.Look(ref MultiplierCaps.min, "LowerLimit", 0.8f);
                Scribe_Values.Look(ref MultiplierCaps.max, "UpperLimit", 1.2f);
            }
            Scribe_Values.Look(ref HarmonyPatches.NVEnabledForCE, "EnabledForCombatExtended", true);
            Scribe_Deep.Look(ref LightModifiersBase.PSLightModifiers, "photosensitivitymodifiers");
            Scribe_Deep.Look(ref LightModifiersBase.NVLightModifiers, "nightvisionmodifiers");
            if (Scribe.mode == LoadSaveMode.LoadingVars)
                {
                    if (LightModifiersBase.PSLightModifiers == null)
                        LightModifiersBase.PSLightModifiers = new LightModifiersBase
                        {
                                    Offsets     = LightModifiersBase.PSDefaultOffsets.ToArray(),
                                    Initialised = true
                        };

                    if (LightModifiersBase.NVLightModifiers == null)
                        LightModifiersBase.NVLightModifiers = new LightModifiersBase
                        {
                                    Offsets     = LightModifiersBase.NVDefaultOffsets.ToArray(),
                                    Initialised = true
                        };
                }
            LightModifiersDictionaryScribe(ref RaceLightMods, "Race");
            LightModifiersDictionaryScribe(ref HediffLightMods, "Hediffs");
            ApparelDictionaryScribe(ref NVApparel);
        }
        #endregion

        #region Dictionary Scribe
        private void ApparelDictionaryScribe( ref Dictionary<ThingDef, ApparelSetting> dictionary)
        {
            if (dictionary == null)
                {
                    return;
                }
            List<ApparelSaveLoadClass> tempList = new List<ApparelSaveLoadClass>();  
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                foreach (var kvp in dictionary)
                {
                    if (kvp.Value != null && kvp.Value.ShouldBeSaved())
                    {
                        tempList.Add(new ApparelSaveLoadClass(kvp.Key, kvp.Value));
                    }
                }
            }
            Scribe_Collections.Look(ref tempList, "Apparel", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                dictionary.Clear();
                int removed = 0;
                for (int i = tempList.Count - 1; i >= 0; i--)
                {
                    if (tempList[i] != null && tempList[i].HasValue())
                    {
                        dictionary[tempList[i].Key] = tempList[i].Value;
                    }
                    else
                    {
                        tempList.RemoveAt(i);
                        removed++;
                    }
                }
                if (removed > 0)
                {
                    Log.Message("NVNullEntryLog".Translate(removed, nameof(dictionary)));
                    Scribe.mode = LoadSaveMode.Saving;
                    Scribe_Collections.Look(ref tempList, "Apparel", LookMode.Deep);
                    Scribe.mode = LoadSaveMode.LoadingVars;
                }
            }
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                tempList?.Clear();
            }
        }

        private void LightModifiersDictionaryScribe<K, V>(ref Dictionary<K, V> dictionary, string label)
                    where K : Def where V : LightModifiersBase
            {
                List<V> tempList;
                if (Scribe.mode == LoadSaveMode.Saving)
                    {
                        if (dictionary == null || dictionary.Count == 0)
                            {
                                return;
                            }
                        tempList = dictionary.Values.ToList();
                        tempList.RemoveAll(lm => (lm == null) || (!lm.ShouldBeSaved()));
                        Scribe_Collections.Look(ref tempList, label, LookMode.Deep);
                    }
                else if (Scribe.mode == LoadSaveMode.LoadingVars)
                    {
                        tempList = new List<V>();
                        Scribe_Collections.Look(ref tempList, label, LookMode.Deep);
                        dictionary.Clear();
                        int removed = 0;
                        for (int i = tempList.Count - 1; i >= 0; i--)
                            {
                                if (tempList[i] != null && tempList[i].ParentDef != null)
                                    {
                                        dictionary[(K) (tempList[i].ParentDef)] = tempList[i];
                                    }
                                else
                                    {
                                        tempList.RemoveAt(i);
                                        removed++;
                                    }
                            }

                        if (removed > 0)
                            {
                                Log.Message("NVNullEntryLog".Translate(removed, nameof(dictionary)));
                                Scribe.mode = LoadSaveMode.Saving;
                                Scribe_Collections.Look(ref tempList, label, LookMode.Deep);
                                Scribe.mode = LoadSaveMode.LoadingVars;
                            }
                    }
            }

        public class ApparelSaveLoadClass : IExposable
            {
                [UsedImplicitly]
                public ApparelSaveLoadClass() { }

                public ApparelSaveLoadClass(ThingDef key, ApparelSetting value)
                    {
                        Key   = key;
                        Value = value;
                    }

                public bool HasValue()
                    {
                        return Key != null && Value != null;
                    }

                public ThingDef       Key;
                public ApparelSetting Value;

                public void ExposeData()
                    {
                        Scribe_Defs.Look(ref Key, "apparelDef");
                        Scribe_Deep.Look(ref Value, "apparelSetting");
                    }
            }

        #endregion

        #endregion

        #region Init Cache and Write from cache
        /// <summary>
        /// Sets new settings
        /// Clears all cached stuff
        /// Runs when opening the settings menu and when closing it
        /// </summary>
        internal void DoPreWriteTasks()
        {
            // this check is required because this method is run on opening the menu
            if (_cacheInited)
            {
                MultiplierCaps.min = _minCache != null? (float)Math.Round((float)_minCache / 100, 2) : MultiplierCaps.min;
                MultiplierCaps.max = _maxCache != null? (float)Math.Round((float)_maxCache / 100, 2): MultiplierCaps.max;

                LightModifiersBase.NVLightModifiers.Offsets = new[]
                {
                            _nvZeroCache != null
                                        ? MultiPercentToMod((float) _nvZeroCache, true)
                                        : LightModifiersBase.NVLightModifiers[0],
                            _nvFullCache != null
                                        ? MultiPercentToMod((float) _nvFullCache, false)
                                        : LightModifiersBase.NVLightModifiers[1]
                };
                LightModifiersBase.PSLightModifiers.Offsets = new[]
                {
                            _psZeroCache != null
                                        ? MultiPercentToMod((float) _psZeroCache, true)
                                        : LightModifiersBase.PSLightModifiers[0],
                            _psFullCache != null
                                        ? MultiPercentToMod((float) _psFullCache, false)
                                        : LightModifiersBase.PSLightModifiers[1]
                };

                Utilities.Classifier.ZeroLightTurningPoints = null;
                Utilities.Classifier.FullLightTurningPoint = null;
            
                _minCache = null;
                _maxCache = null;
                _nvZeroCache = null;
                _nvFullCache = null;
                _psZeroCache = null;
                _psFullCache = null;
            }

            _cacheInited = false;

            //if (_raceEyeLightModifiersCached.Count != 0)
            //{
            //    _raceEyeLightModifiersCached.Clear();
            //}
            TipStringHolder.Clear();
            _allHediffsCache = null;
            _headgearCache = null;
            _numberOfCustomRaces = null;
            _numberOfCustomHediffs = null;
            if (Current.ProgramState == ProgramState.Playing)
            {
                SetDirtyAllComps();
            }
            
            _allPawns = null;
            _maxY = 0f;
        }

        /// <summary>
        /// So that the comps will update with the new settings, sets all the comps dirty
        /// </summary>
        private void SetDirtyAllComps()
        {
            foreach (Pawn pawn in PawnsFinder.AllMaps_Spawned/*.Where(pwn => pwn.RaceProps.Humanlike)*/)
            {
                if (pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
                {
                    comp.SetDirty();
                }
            }
        }

        private void InitSettingsCache()
        {
            //if (minCache == null)
            //{
                _minCache = (float)Math.Round(MultiplierCaps.min * 100);
            //}
            //if (maxCache == null)
            //{
                _maxCache = (float)Math.Round(MultiplierCaps.max * 100);
            //}
            //if (nvZeroCache == null)
            //{
                _nvZeroCache = ModToMultiPercent(LightModifiersBase.NVLightModifiers[0], true);
            //}
            //if (nvFullCache == null)
            //{
                _nvFullCache = ModToMultiPercent(LightModifiersBase.NVLightModifiers[1], false);
            //}
            //if (psZeroCache == null)
            //{
                _psZeroCache = ModToMultiPercent(LightModifiersBase.PSLightModifiers[0], true);
            //}
            //if (psFullCache == null)
            //{
                _psFullCache = ModToMultiPercent(LightModifiersBase.PSLightModifiers[1], false);
            //}
        }
    }
        
     #endregion


}
