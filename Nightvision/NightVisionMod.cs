using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Nightvision
    //Consider adding custom pawn tracker
{
    [StaticConstructorOnStartup]
    public class NightVisionMod : Mod
    {
        public static NightVisionMod Instance;

        public NightVisionMod(ModContentPack content) : base(content) => Instance = this;

        public static Dictionary<string, int> dictOfNVGranters = new Dictionary<string, int>();

        public override string SettingsCategory() => "Night Vision";

        public static void FindAndAddNVGranters()
        {
            List<ThingDef> list = DefDatabase<ThingDef>.AllDefs.Where(td =>
                td.HasComp(Comp_NightVision))
        }
    }
}
