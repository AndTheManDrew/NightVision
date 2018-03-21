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

        /// <summary>
        /// TODO Change multipliers for races & nv & ps to additives: UX issue could be resolved by converting values here
        ///     - need to change expose data to account for this? No. Have duplicate dictionaries? Yes, store factor one. The modifier one would have a getter that
        ///         redirected to the factor dict, cached it, and was cleared when this method was called in Window.PreClose()
        ///    - performance issues? Setting all comps dirty every time = maybe not best idea?
        ///    - possibly have a if-else cascade - comparing cached lengths of dicts to new lengths - then comparing dicts
        ///    
        /// </summary>
        public override void WriteSettings()
        {
            base.WriteSettings();
            Settings.SetDirtyAllComps();
            Settings.ResetDependants();
            //TODO Convert all % based glow factors to decimal based modifiers
        }

        private static void SetSettings()
        {
            Settings = Instance.GetSettings<NightVisionSettings>();
            NightVisionDatabase.HediffsListMaker();
            NightVisionDatabase.RaceDictBuilder();
            NightVisionDatabase.ApparelDictBuilder();
        }
    }
}
