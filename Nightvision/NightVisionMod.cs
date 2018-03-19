using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Harmony;

namespace NightVision
{
    #region Mod
    class NightVisionMod : Mod
    {

        public static NightVisionMod Instance;
        public static NightVisionSettings Settings;




        public NightVisionMod(ModContentPack content) : base(content)
        {
            Log.Message("NV Mod");
            Instance = this;

            Settings = GetSettings<NightVisionSettings>();
            Log.Message("NVmod settings loaded? " + Settings.PhotosensitiveLightFactors);

        }

        public override string SettingsCategory() => "Night Vision";
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoSettingsWindowContents(inRect);
        }
        
    }
    #endregion




}
