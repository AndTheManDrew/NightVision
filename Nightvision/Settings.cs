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
       
        public static readonly IntRange DefaultPhotosensitiveGlFactors = new IntRange ( 120, 80 );
        public static readonly IntRange DefaultLightFactors = new IntRange ( 80, 100 );
        
        private Vector2 scrollPosition = Vector2.zero;
        private bool settingChangedFlag = false;

        public IntRange BionicLightFactors = new IntRange(100, 100);
        public IntRange PhotosensitiveLightFactors = new IntRange(120, 80);
        public IntRange HumanLightFactors = new IntRange(80, 100);
        public static NightVisionSettings Instance;
        
        public Dictionary<ThingDef, IntRange> DictOfRaceNightVision = new Dictionary<ThingDef, IntRange>();
        
        public List<HediffDef> ListofNightVisionHediffDefs = new List<HediffDef>();

        public List<HediffDef> ListofPhotosensitiveHediffDefs = new List<HediffDef>();

        public NightVisionSettings()
        {
            Instance = this;
        }
        
        public void DoSettingsWindowContents(Rect inRect)
        {
            string settingsPSFullLightBuffer = PhotosensitiveLightFactors.max.ToString();
            string settingsPSZeroLightBuffer = PhotosensitiveLightFactors.min.ToString();
            

            Rect firstinRect = inRect.ContractedBy(20f).TopPart(0.3f);
            Listing_Standard list = new Listing_Standard(GameFont.Small)
            {
                verticalSpacing = 17f
            };

            list.Begin(firstinRect);
            IntRange flag1 = PhotosensitiveLightFactors;
            list.Label("Work and Move Speed Factor for pawns with two photosensitive eyes:");
            list.Label("Full light:");
            list.TextFieldNumeric(ref PhotosensitiveLightFactors.max, ref settingsPSFullLightBuffer, 0, 200);

            list.Label("Zero light:");
            list.TextFieldNumeric(ref PhotosensitiveLightFactors.min, ref settingsPSZeroLightBuffer, 0, 200);
            list.Gap();
            if (flag1 != PhotosensitiveLightFactors)
            {
                Write();
                settingChangedFlag = true;
                Log.Message("Changed Photosensitivity Settings");
            }
            list.End();
            Text.Font = GameFont.Small;
            Rect raceRect = inRect.ContractedBy(16f).BottomPart(0.65f);
            GUI.BeginGroup(raceRect, new GUIStyle(GUI.skin.box));
            int raceCount = DictOfRaceNightVision.Count;
            Rect viewRect = new Rect(0f, 0f, raceRect.width / 10 * 9, raceCount * 32f);
            float num = 3f;
            Widgets.BeginScrollView(raceRect.AtZero(), ref this.scrollPosition, viewRect);
            foreach (ThingDef key in  DictOfRaceNightVision.Keys.ToList())
            {
                int zeroLightFactor = DictOfRaceNightVision[key].min;
                int fullLightFactor = DictOfRaceNightVision[key].max;
                string zeroLightFactorBuffer = zeroLightFactor.ToString();
                string fullLightFactorBuffer = fullLightFactor.ToString();

                Rect rowRect = new Rect(5, num, raceRect.width - 6, 30);
                Rect leftRect = rowRect.LeftPart(0.3f);
                Rect rightRect = rowRect.RightPart(0.6f);
                Widgets.Label(leftRect, key.LabelCap);
                Widgets.TextFieldNumeric(rightRect.LeftPart(0.4f), ref zeroLightFactor, ref zeroLightFactorBuffer);
                Widgets.TextFieldNumeric(rightRect.RightPart(0.4f), ref fullLightFactor, ref fullLightFactorBuffer);
                if (zeroLightFactor != DictOfRaceNightVision[key].min || fullLightFactor != DictOfRaceNightVision[key].max)
                {
                    DictOfRaceNightVision[key] = new IntRange(zeroLightFactor, fullLightFactor);
                    zeroLightFactorBuffer = zeroLightFactor.ToString();
                    fullLightFactorBuffer = fullLightFactor.ToString();
                    Write();
                    settingChangedFlag = true;
                }
                num += 32f;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();

            // TODO List all headgear and allow user to choose what grants nightvision :: saves using a load of patches

        }
        //List<int> keysWorkingList = null;
        //List<IntRange> valuesWorkingList = null;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref PhotosensitiveLightFactors.min, "PhotosensitiveLightFactorsZeroLight", 120);
            Scribe_Values.Look(ref PhotosensitiveLightFactors.max, "PhotosensitiveLightFactorsFullLight", 80);

            Scribe_Values.Look(ref HumanLightFactors.min, "HumanLightFactorsZeroLight", 80);
            Scribe_Values.Look(ref HumanLightFactors.max, "HumanLightFactorsFullLight", 100);

            Scribe_Collections.Look(ref DictOfRaceNightVision, "Race", LookMode.Def, LookMode.Undefined /*, ref keysWorkingList, ref valuesWorkingList*/);
           // Scribe_Collections.Look<string, IntRange>()
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
    }
}
