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
    class Controller : Mod
    {

        public static Controller Instance;
        public static NightVisionSettings Settings;

        public Controller(ModContentPack content) : base(content)
        {
            Instance = this;
            LongEventHandler.QueueLongEvent(SetSettings, "Loading Night Vision Settings", false, null);
        }

        public override string SettingsCategory() => "Night Vision";
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoSettingsWindowContents(inRect);
        }


        public override void WriteSettings()
        {
            base.WriteSettings();
            Settings.ResetDependants();
            if (Current.ProgramState == ProgramState.Playing)
            {
                Settings.SetDirtyAllComps();
            }
        }

        private static void SetSettings()
        {
            Settings = Instance.GetSettings<NightVisionSettings>();
            NightVisionDatabaseBuilders.MakeHediffsDict();
            NightVisionDatabaseBuilders.RaceDictBuilder();
            NightVisionDatabaseBuilders.ApparelDictBuilder();
        }
    }
}
