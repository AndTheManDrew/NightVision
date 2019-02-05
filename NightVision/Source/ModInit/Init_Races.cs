// Nightvision NightVision Init_Races.cs
// 
// 06 12 2018
// 
// 06 12 2018

using System.Collections.Generic;
using Verse;

namespace NightVision
{
    public static class Init_Races
    {
        #region  Members

        public static void FindAllValidRaces()
        {
            //Check for compprops so that humanlike req can be overridden in xml
            List<ThingDef> raceDefList = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(
                match: rdef => rdef.race is RaceProperties race && (race.Humanlike || rdef.GetCompProperties<CompProperties_NightVision>() != null)
            );

            if (Storage.RaceLightMods == null)
            {
                Storage.RaceLightMods = new Dictionary<ThingDef, Race_LightModifiers>();
            }

            foreach (ThingDef rdef in raceDefList)
            {
                if (!Storage.RaceLightMods.TryGetValue(key: rdef, value: out Race_LightModifiers rLm) || rLm == null)
                {
                    Storage.RaceLightMods[key: rdef] = new Race_LightModifiers(raceDef: rdef);
                }

                // Note: When dictionary is loaded and calls exposedata on the saved Race_LightModifiers the def & corresponding compProps are attached
            }
        }

        #endregion
    }
}