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
            LongEventHandler.QueueLongEvent(InitSettings, "Initialising Night Vision Settings", false, null);
        }

        public override string SettingsCategory() => "Night Vision";
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoSettingsWindowContents(inRect);
        }

        public override void WriteSettings()
        {
            Settings.DoPreWriteTasks();
            base.WriteSettings();
        }

        private static void InitSettings()
        {
            Settings = Instance.GetSettings<NightVisionSettings>();
            NightVisionDictionaryBuilders.MakeHediffsDict();
            NightVisionDictionaryBuilders.RaceDictBuilder();
            NightVisionDictionaryBuilders.ApparelDictBuilder();
        }
    }
}
