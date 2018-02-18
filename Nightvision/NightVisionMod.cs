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

        /// <summary>
        /// Array of modifying factors of photosensitivity:
        /// [0]: for zero light
        /// [1]: for full light
        /// </summary>
        private float[] photosensitiveGlowfactorsPerEye;
        //Should create a new glow curve for each race???
        private SimpleCurve humanGlowCurve = new SimpleCurve();
        
        private List<HediffDef> listofNightVisionHediffDefs;
        private List<HediffDef> listofPhotosensitiveHediffDefs;

        
        #endregion

        #region Getters and Setters
        public SimpleCurve HumanGlowCurve { get => humanGlowCurve; set => humanGlowCurve = value; }
        /// <summary>
        /// Prop of Array of modifying factors of photosensitivity:
        /// [0]: for zero light
        /// [1]: for full light
        /// </summary>
        public float[] PhotosensitiveGlowfactorsPerEye
        {
            get
            {
                if (photosensitiveGlowfactorsPerEye == null || photosensitiveGlowfactorsPerEye.Length == 0)
                {
                    photosensitiveGlowfactorsPerEye = new float[2];
                }
                return photosensitiveGlowfactorsPerEye;
            }
            set
            {
                {
                    if (photosensitiveGlowfactorsPerEye.Length == 0)
                    {
                        photosensitiveGlowfactorsPerEye = new float[2];
                    }
                    photosensitiveGlowfactorsPerEye = value;
                }
            }
        }

        public List<HediffDef> ListofNightVisionHediffDefs {
            get
            {
                if (listofNightVisionHediffDefs == null)
                {
                    listofNightVisionHediffDefs = new List<HediffDef>();
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
                    listofPhotosensitiveHediffDefs = new List<HediffDef>();
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
            if (settings.PhotosensitiveBonusAtZeroLight < 0f |  settings.PhotosensitiveBonusAtZeroLight > 1f)
            {
                settings.PhotosensitiveBonusAtZeroLight = 0.1f;
            }
            if (settings.PhotosensitiveMalusAtFullLight < -1f | settings.PhotosensitiveMalusAtFullLight > 0f)
            {
                settings.PhotosensitiveMalusAtFullLight = -0.1f;
            }

            PhotosensitiveGlowfactorsPerEye[0] = settings.PhotosensitiveBonusAtZeroLight;
            PhotosensitiveGlowfactorsPerEye[1] = settings.PhotosensitiveMalusAtFullLight;
            

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

        public override string SettingsCategory() => "Night Vision";
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
        private string settingsPSFullLightBuffer = "-0.1";
        private string settingsPSZeroLightBuffer = "0.1";
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
            Listing_Standard list = new Listing_Standard(GameFont.Small)
            {
                verticalSpacing = 17f
            };

            list.Begin(myinRect);
            list.Label("Penalty per photosensitive eye in full light:");
            list.TextFieldNumeric(ref photosensitiveMalusAtFullLight, ref settingsPSFullLightBuffer, -1f, 0f);
            list.Gap();

            list.Label("Bonus per photosensitive eye in zero light: ");
            list.TextFieldNumeric(ref photosensitiveBonusAtZeroLight, ref settingsPSZeroLightBuffer, 0f, 1f);
            list.End();
            
            if (PhotosensitiveBonusAtZeroLight != NightVisionMod.Instance.PhotosensitiveGlowfactorsPerEye[0]
                || PhotosensitiveMalusAtFullLight != NightVisionMod.Instance.PhotosensitiveGlowfactorsPerEye[1])
            {
                Instance.Mod.WriteSettings();
                NightVisionMod.Instance.PhotosensitiveGlowfactorsPerEye.ToString();
            }
            
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref photosensitiveMalusAtFullLight, "PhotosensitiveMalusAtFullLight", -0.1f);
            Scribe_Values.Look(ref photosensitiveBonusAtZeroLight, "PhotosensitiveBonusAtZeroLight", 0.1f);
        }
    }
    #endregion
}
