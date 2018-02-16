using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Harmony;

namespace NightVision
    //Consider adding custom pawn tracker
{
    #region Mod
    class NightVisionMod : Mod
    {
        #region Private Fields
        public static NightVisionMod Instance;
        public static NightVisionSettings settings;

        private SimpleCurve glowCurveforPhotosensitivity = new SimpleCurve();
        //Should create a new glow curve for each race???
        private SimpleCurve humanGlowCurve = new SimpleCurve();
        
        private List<HediffDef> listofNightVisionHediffDefs;
        private List<HediffDef> listofPhotosensitiveHediffDefs;
        #endregion

        #region Getters and Setters
        public SimpleCurve HumanGlowCurve { get => humanGlowCurve; set => humanGlowCurve = value; }
        public SimpleCurve GlowCurveforPhotosensitivity { get => glowCurveforPhotosensitivity; set => glowCurveforPhotosensitivity = value; }

        public List<HediffDef> ListofNightVisionHediffDefs {
            get
            {
                if (listofNightVisionHediffDefs == null)
                {
                    listofNightVisionHediffDefs = new List<HediffDef>;
                }
                return listofNightVisionHediffDefs;

            }
            set => listofNightVisionHediffDefs = value; }

        public List<HediffDef> ListofPhotosensitiveHediffDefs
        {
            get
            {
                if (listofPhotosensitiveHediffDefs == null)
                {
                    listofPhotosensitiveHediffDefs = new List<HediffDef>;
                }
                return listofPhotosensitiveHediffDefs;

            }
            set => listofPhotosensitiveHediffDefs = value;
        }
        #endregion
        #region Mod Main thing
        public NightVisionMod(ModContentPack content) : base(content)
        {
            Instance = this;

            settings = GetSettings<NightVisionSettings>();
            if (settings.PhotosensitiveBonusAtZeroLight < 1f |  settings.PhotosensitiveBonusAtZeroLight > 2f)
            {
                settings.PhotosensitiveBonusAtZeroLight = 1.2f;
            }
            if (settings.PhotosensitiveMalusAtFullLight < -1f | settings.PhotosensitiveMalusAtFullLight > 0f)
            {
                settings.PhotosensitiveMalusAtFullLight = 0.2f;
            }   

            GlowCurveforPhotosensitivity.SetPoints(
                new CurvePoint[] 
                {
                    new CurvePoint(1f, settings.PhotosensitiveMalusAtFullLight),
                    new CurvePoint(0.7f, 1f),
                    new CurvePoint(0.3f,1f),
                    new CurvePoint(0f, settings.PhotosensitiveBonusAtZeroLight)
                });

            //Vanilla field:
            //private SimpleCurve factorFromGlowCurve;
            //vanillaGlowCurve = StatDefOf.MoveSpeed.parts.l
            HumanGlowCurve.SetPoints(
                new CurvePoint[]
                {
                    new CurvePoint(0.3f,1f),
                    new CurvePoint(0f, 0.8f)
                });
            //Log.Message("Grab the vanillaGlowCurve from: " + StatDefOf.MoveSpeed.parts.ToString());

        }

        public override string SettingsCategory() => "Night Vision - Settings Test";
        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.DoSettingsWindowContents(inRect);
        }
        
    }
    #endregion
    #endregion
    #region Mod Settings
    class NightVisionSettings : ModSettings
    {
        private string settingsPSFullLightBuffer = "-0.2";
        private string settingsPSZeroLightBuffer = "0.2";
        public static NightVisionSettings Instance;

        /// <summary>
        /// The percentage penalty for photosensitivity in full light
        /// </summary>
        private float photosensitiveMalusAtFullLight;

        /// <summary>
        /// The percentage bonus for photosensitivity in zero light
        /// </summary>
        private float photosensitiveBonusAtZeroLight;
        

        public float PhotosensitiveMalusAtFullLight { get => photosensitiveMalusAtFullLight; set => photosensitiveMalusAtFullLight = value; }
        public float PhotosensitiveBonusAtZeroLight { get => photosensitiveBonusAtZeroLight; set => photosensitiveBonusAtZeroLight = value; }

        //public const float VanillaZeroLightFactor = 0.8f;




        public NightVisionSettings()
        {
            NightVisionSettings.Instance = this;

        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            Rect myinRect = inRect.ContractedBy(20f);
            Listing_Standard list = new Listing_Standard(GameFont.Small);
            list.verticalSpacing = 17f;

            list.Begin(myinRect);
            list.Label("Penalty for Photosensitive pawns in full light:");
            list.TextFieldNumeric(ref photosensitiveMalusAtFullLight, ref settingsPSFullLightBuffer, -1f, 0f);
            list.Gap();

            list.Label("Bonus for Photosensitive pawns in zero light: ");
            list.TextFieldNumeric(ref photosensitiveBonusAtZeroLight, ref settingsPSZeroLightBuffer, 1f, 2f);
            list.End();

                Instance.Mod.WriteSettings();
                NightVisionMod.Instance.GlowCurveforPhotosensitivity.ToString();
            
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref photosensitiveMalusAtFullLight, "PhotosensitiveMalusAtFullLight", -0.2f);
            Scribe_Values.Look(ref photosensitiveBonusAtZeroLight, "PhotosensitiveBonusAtZeroLight", 1.2f);
        }
    }
    #endregion
}
