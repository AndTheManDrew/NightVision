// Nightvision NightVision Initialiser.cs
// 
// 03 08 2018
// 
// 16 10 2018

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using RimWorld;
using Verse;

namespace NightVision
{
    public static class Initialiser
    {
        public static void Startup()
        {
            FindSettingsDependentFields();

            BuildDictionaries();
            Init_TapetumAnimals.TapetumInjector();
            Init_Research.AddNightVisionToResearch();


        }

        public static void BuildDictionaries()
        {
            Init_Hediffs.FindAllRelevantHediffs();
            Init_Races.FindAllRelevantRaces();
            Init_Apparel.FindAllEyeCoveringApparel();
        }

        
        public static void FindSettingsDependentFields()
        {
            FieldClearer.SettingsDependentFields = GenTypes.AllTypesWithAttribute<NVHasSettingsDependentFieldAttribute>().SelectMany(
                t => AccessTools.GetDeclaredFields(t).FindAll(fi => fi.HasAttribute<NVSettingsDependentFieldAttribute>())
            ).ToList();
        }
    }
}