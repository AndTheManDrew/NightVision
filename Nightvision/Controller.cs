using Verse;
using UnityEngine;
using JetBrains.Annotations;

namespace NightVision
{
    internal class Controller : Mod
    {
        public static Controller Instance;
        public static NightVisionSettings Settings;

        [UsedImplicitly]
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
        /// <summary>
        /// Called by Rimworld before displaying the list of mod settings & after closing this mods settings window
        /// </summary>
        public override void WriteSettings()
        {
            Log.Message("Nightvision: WriteSettings called");
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
