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
    /// <summary>
    /// Reads all loaded item definitions and builds maps for items that meet certain requirements
    /// Greedy rather than efficient - to support options being changed at a later date
    /// </summary>
    public partial class Initialiser
    {
        public void Startup()
        {
            //FindSettingsDependentFields();
            FieldClearer.FindSettingsDependentFields();

            FindDefsToAddNightVisionTo();
            AddNightVisionMarkerToVanillaResearch();
            AddTapetumRecipeToAnimals();


        }

        public void FindDefsToAddNightVisionTo()
        {
            FindAllValidHediffs();
            FindAllValidRaces();
            FindAllValidApparel();
        }

        
        public void FindSettingsDependentFields()
        {
            /*

            FieldClearer.SettingsDependentFields = GenTypes.AllTypesWithAttribute<NVHasSettingsDependentFieldAttribute>().SelectMany(
                t => AccessTools.GetDeclaredFields(t).FindAll(fi => fi.HasAttribute<NVSettingsDependentFieldAttribute>())
            ).ToList();
            */

            
            var traverses = new List<Traverse>();

            var markedTypes = GenTypes.AllTypesWithAttribute<NVHasSettingsDependentFieldAttribute>();
            foreach (var type in markedTypes)
            {
                var fields = AccessTools.GetDeclaredFields(type)
                    .FindAll(fi => fi.HasAttribute<NVHasSettingsDependentFieldAttribute>());

                foreach (var info in fields)
                {
                    var traverse = new Traverse(type);
                    traverse.Field(info.Name);
                    
                    traverses.Add(traverse);

                }
            }

            FieldClearer.SettingsDependentFieldTraverses = traverses;
        }
    }
}