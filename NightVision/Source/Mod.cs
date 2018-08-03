// Nightvision NightVision Mod.cs
// 
// 10 03 2018
// 
// 21 07 2018

using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace NightVision
{
    internal class Mod : Verse.Mod
    {
        public static Mod Instance;

        [UsedImplicitly]
        public static Settings Settings;

        [UsedImplicitly]
        public Mod(
                        ModContentPack content
                    ) : base(content)
        {
            Mod.Instance = this;

            LongEventHandler.QueueLongEvent(
                                            Mod.InitSettings,
                                            "Initialising Night Vision Settings",
                                            false,
                                            null
                                           );
        }

        public override string SettingsCategory() => "Night Vision";

        public override void DoSettingsWindowContents(
                        Rect inRect
                    )
        {
            Settings.DoSettingsWindowContents(inRect);
        }

        /// <summary>
        ///     Called by Rimworld before displaying the list of mod settings & after closing this mods settings window
        /// </summary>
        public override void WriteSettings()
        {
            Log.Message("Nightvision: WriteSettings called");
            SettingsCache.DoPreWriteTasks();
            base.WriteSettings();
        }

        private static void InitSettings()
        {
            Mod.Settings = Mod.Instance.GetSettings<Settings>();
            Initialiser.Startup();

            if (Storage.NullRefWhenLoading)
            {
                Mod.Instance.WriteSettings();
            }
        }
    }
}