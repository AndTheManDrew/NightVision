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
        private enum Tab
        {
            General,
            Races,
            Apparel,
            Bionics
        }
        private Tab tab;
        private static List<TabRecord> tabsList = new List<TabRecord>();

        public static readonly IntRange DefaultPhotosensitiveGlFactors = new IntRange ( 120, 80 );
        public static readonly IntRange DefaultLightFactors = new IntRange ( 80, 100 );
        private static readonly FloatRange DefaultEyeMods = new FloatRange(-0.1f, 0f);
        
        private Vector2 scrollPosition = Vector2.zero;
        private FloatRange intBionicEyeMods = new FloatRange(-1, -1);
        private FloatRange intPhotoSensEyeMods = new FloatRange(-1, -1);

        public static NightVisionSettings Instance;
        private List<ThingDef> RacesWithChangedSettings = new List<ThingDef>();
        private bool PhotoSensSettingsChanged = false;
        private bool BionicSettingsChanged = false;
        public IntRange BionicLightFactors = new IntRange(100, 100);
        public IntRange PhotosensitiveLightFactors = new IntRange(120, 80);
        public FloatRange BionicEyeMods
        {
            get
            {
                if (intBionicEyeMods == null)
                {
                    ConvertFactorToMod(BionicLightFactors, out intBionicEyeMods);
                }
                return intBionicEyeMods;
            }
            set => intBionicEyeMods = value;
        }
        public FloatRange PhotosensEyeMods
        {
            get
            {
                if (intPhotoSensEyeMods.min == -1 && intPhotoSensEyeMods.max == -1)
                {
                    ConvertFactorToMod(PhotosensitiveLightFactors, out intPhotoSensEyeMods);
                }
                return intPhotoSensEyeMods;
            }
            set => intPhotoSensEyeMods = value;
        }

        private static Dictionary<ThingDef, FloatRange> intRaceNightVisionMods = new Dictionary<ThingDef, FloatRange>();
        public FloatRange GetRaceNightVisionMod(ThingDef race)
        {
            if (!intRaceNightVisionMods.ContainsKey(race))
            {
                if (RaceNightVisionFactors.ContainsKey(race))
                {
                    FloatRange floatRange;
                    ConvertFactorToMod(RaceNightVisionFactors[race], out floatRange);
                    intRaceNightVisionMods[race] = floatRange;
                }
                else
                {
                    intRaceNightVisionMods[race] = DefaultEyeMods;
                    Log.Message("Night Vision: Could not find entry for " + race.LabelCap + ", setting to default.");
                }
            }
            return intRaceNightVisionMods[race];
        }

        public static Dictionary<ThingDef, IntRange> RaceNightVisionFactors = new Dictionary<ThingDef, IntRange>();
        public static List<HediffDef> NightVisionHediffDefs = new List<HediffDef>();
        public static List<HediffDef> PhotosensitiveHediffDefs = new List<HediffDef>();
        public static Dictionary<ThingDef, ApparelSetting> NVApparel = new Dictionary<ThingDef, ApparelSetting>();
        public static List<ThingDef> AllHeadgearDefs = new List<ThingDef>();




        public NightVisionSettings()
        {
            Instance = this;
        }
        
        public void DoSettingsWindowContents(Rect inRect)
        {

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            tabsList.Clear();
            tabsList.Add(new TabRecord("General", delegate{tab = Tab.General;}, tab == Tab.General));
            tabsList.Add(new TabRecord("Races", delegate { tab = Tab.Races; }, tab == Tab.Races));
            tabsList.Add(new TabRecord("Apparel", delegate { tab = Tab.Apparel; }, tab == Tab.Apparel));
            tabsList.Add(new TabRecord("Bionics", delegate { tab = Tab.Bionics; }, tab == Tab.Bionics));

            inRect.yMin += 32f;
            Widgets.DrawMenuSection(inRect);
            TabDrawer.DrawTabs(inRect, tabsList);
            inRect = inRect.ContractedBy(17f);
            GUI.BeginGroup(inRect);
            Rect firstinRect = inRect.ContractedBy(20f);
            if (tab == Tab.General)
            {
                DrawGeneralTab(firstinRect);
            }
            else if (tab == Tab.Races)
            {
                DrawRaceTab(firstinRect);
            }
            else if (tab == Tab.Apparel)
            {
                if (AllHeadgearDefs.Count() == 0)
                {
                    AllHeadgearDefs = DefDatabase<ThingDef>.AllDefs.Where(adef =>
                    adef.IsApparel && adef.thingCategories.Contains(ThingCategoryDef.Named("Headgear"))).ToList();
                    // Move the defs that already have NV settings to the top of the list -- aesthetic sensibilities
                    foreach (ThingDef appareldef in NVApparel.Keys)
                    {
                        int appindex = AllHeadgearDefs.IndexOf(appareldef);
                        if (appindex >= 0)
                        {
                            AllHeadgearDefs.RemoveAt(appindex);
                            AllHeadgearDefs.Insert(0, appareldef);
                        }
                    }
                    Log.Message("Made the ApparelDefs list.");
                }
                DrawApparelTab(firstinRect);
            }
            GUI.EndGroup();
           
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref PhotosensitiveLightFactors.min, "PhotosensitiveLightFactorsZeroLight", 120);
            Scribe_Values.Look(ref PhotosensitiveLightFactors.max, "PhotosensitiveLightFactorsFullLight", 80);

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                RaceNightVisionFactors.RemoveAll(kvp => kvp.Key == null);
                NVApparel.RemoveAll(appset => appset.Key == null || !(appset.Value.NullifiesPS || appset.Value.GrantsNV));
            }

            Scribe_Collections.Look(ref RaceNightVisionFactors, "Race", LookMode.Def, LookMode.Undefined);
            Scribe_Collections.Look(ref NVApparel, "Apparel", LookMode.Def, LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                int numOfRemoved = RaceNightVisionFactors.RemoveAll(kvp => kvp.Key == null);
                if (numOfRemoved > 0)
                {
                    Log.Message("Night Vision: Removed " + numOfRemoved + " null defs from race dictionary." +
                        " This is to be expected if you have removed a race mod.");
                }
                numOfRemoved = NVApparel.RemoveAll(appset => appset.Key == null || !(appset.Value.NullifiesPS || appset.Value.GrantsNV));
                if (numOfRemoved > 0)
                {
                    Log.Message("Night Vision: Removed " + numOfRemoved + " null defs and redundant settings from apparel list." +
                        " This is to be expected if you have removed a clothing mod or changed the mod settings.");
                }
            }

        }
        
        /// <summary>
        /// So that the comps will update with the new settings, sets all the comps dirty
        /// </summary>
        public void SetDirtyAllComps()
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
            //TODO Add reset defaults and thought settings
            string settingsPSFullLightBuffer = PhotosensitiveLightFactors.max.ToString();
            string settingsPSZeroLightBuffer = PhotosensitiveLightFactors.min.ToString();

            Listing_Standard list = new Listing_Standard(GameFont.Small){ verticalSpacing = 17f };

            list.Begin(inRect);
            IntRange flag1 = PhotosensitiveLightFactors;
            list.Label("Work and Move Speed Factor for pawns with two photosensitive eyes:");
            list.Label("Full light:");
            list.TextFieldNumeric(ref PhotosensitiveLightFactors.max, ref settingsPSFullLightBuffer, 0, 200);

            list.Label("Zero light:");
            list.TextFieldNumeric(ref PhotosensitiveLightFactors.min, ref settingsPSZeroLightBuffer, 0, 200);
            list.Gap();
            if (flag1 != PhotosensitiveLightFactors)
            {
                PhotoSensSettingsChanged = true;
            }
            list.End();
        }

        private void DrawRaceTab(Rect inRect)
        {
            // TODO Add labels and stuff
            int raceCount = RaceNightVisionFactors.Count;
            Rect viewRect = new Rect(0f, 0f, inRect.width / 10 * 9, raceCount * 32f);
            float num = 3f;
            Widgets.BeginScrollView(inRect.AtZero(), ref this.scrollPosition, viewRect);
            foreach (ThingDef key in RaceNightVisionFactors.Keys.ToList())
            {
                int zeroLightFactor = RaceNightVisionFactors[key].min;
                int fullLightFactor = RaceNightVisionFactors[key].max;
                string zeroLightFactorBuffer = zeroLightFactor.ToString();
                string fullLightFactorBuffer = fullLightFactor.ToString();

                Rect rowRect = new Rect(5, num, inRect.width - 6, 30);
                Rect leftRect = rowRect.LeftPart(0.3f);
                Rect rightRect = rowRect.RightPart(0.6f);
                Widgets.Label(leftRect, key.LabelCap);
                Widgets.TextFieldNumeric(rightRect.LeftPart(0.4f), ref zeroLightFactor, ref zeroLightFactorBuffer);
                Widgets.TextFieldNumeric(rightRect.RightPart(0.4f), ref fullLightFactor, ref fullLightFactorBuffer);
                if (zeroLightFactor != RaceNightVisionFactors[key].min || fullLightFactor != RaceNightVisionFactors[key].max)
                {
                    RaceNightVisionFactors[key] = new IntRange(zeroLightFactor, fullLightFactor);
                    zeroLightFactorBuffer = zeroLightFactor.ToString();
                    fullLightFactorBuffer = fullLightFactor.ToString();
                    RacesWithChangedSettings.Add(key);
                }
                num += 32f;
            }
            Widgets.EndScrollView();
        }
        private void DrawApparelTab(Rect inRect)
        {
            // TODO Add labels and stuff
            int apparelCount = AllHeadgearDefs.Count;
            
            Rect viewRect = new Rect(0f, 0f, inRect.width / 10 * 9, apparelCount * 32f);
            float num = 3f;
            Widgets.BeginScrollView(inRect.AtZero(), ref this.scrollPosition, viewRect);
            foreach (ThingDef appareldef in AllHeadgearDefs)
            {
                
                // TODO Rework these labels, move them to the top of the box
                string nullPSLabel = "Nullify Photosensitivity";
                string giveNVLabel = "Give NightVision";
                ApparelSetting apparelSetting;

                Rect rowRect = new Rect(5, num, inRect.width - 6, 30);
                Rect leftRect = rowRect.LeftPart(0.3f);
                Rect rightRect = rowRect.RightPart(0.6f);
                Widgets.Label(leftRect, appareldef.LabelCap);
                if (NVApparel.TryGetValue(appareldef, out apparelSetting))
                {
                    Widgets.CheckboxLabeled(rightRect.LeftPart(0.4f), nullPSLabel, ref apparelSetting.NullifiesPS);
                    Widgets.CheckboxLabeled(rightRect.RightPart(0.4f), giveNVLabel, ref apparelSetting.GrantsNV);
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
                    Widgets.CheckboxLabeled(rightRect.LeftPart(0.4f), nullPSLabel, ref nullPS);
                    Widgets.CheckboxLabeled(rightRect.RightPart(0.4f), giveNVLabel, ref giveNV);
                    if (nullPS || giveNV)
                    {
                        apparelSetting = new ApparelSetting(nullPS, giveNV);
                        NVApparel[appareldef] = apparelSetting;
                    }
                }
                num += 32f;
            }
            Widgets.EndScrollView();
        }

        public void ResetDependants()
        {
            if (BionicSettingsChanged)
            {
                intBionicEyeMods.min = -1;
                intBionicEyeMods.max = -1;
            }
            if (PhotoSensSettingsChanged)
            {
                intPhotoSensEyeMods.min = -1;
                intPhotoSensEyeMods.max = -1;
            }
            if (RacesWithChangedSettings.Count() > 0)
            {
                foreach (ThingDef race in RacesWithChangedSettings)
                {
                    intRaceNightVisionMods.Remove(race);
                }
                RacesWithChangedSettings.Clear();
            }
        }
        private void ConvertFactorToMod(IntRange intRange, out FloatRange floatRange)
        {
            floatRange = new FloatRange
            {
                min = (intRange.min - 80) / 20f,
                max = (intRange.max - 100) / 20f
            };
        }
    }
}
