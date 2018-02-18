using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace NightVision
{

    [StaticConstructorOnStartup]
    static class NightVisionGrantersListMaker
    {
        const string eyeTag = "SightSource";

        //public static NightVisionGrantersDatabase()
        //{
        //    Log.Message("The NV_GrantersDatabase constructor?");
        //    listOfNightVisionThings = new List<HediffDef>();
        //    AddThingsToListOfNightVisionThings();
        //    Log.Message("List of NV Things: ");
        //    foreach (HediffDef hediffdef in listOfNightVisionThings)
        //    {
        //        Log.Message(hediffdef.defName);
        //    }
        //}

        static NightVisionGrantersListMaker()
        {
            Log.Message("NV_ListMaker got called");

            #region Finding Hediffs that have isbionic prop and are added to sightsource parts
            //Have to go via recipes as there is nothing in the bionic eye hediff
            //that actually says it applies to a sightsource; aside from the label/defname
            //but that would probably cause problems with mods and their janky
            //cool names for bionic stuff and/or eyes
            List<HediffDef> AppropriateHediffs = DefDatabase<RecipeDef>.AllDefs.Where(rpd => 
                rpd.appliedOnFixedBodyParts != null 
                && rpd.addsHediff != null
                && rpd.appliedOnFixedBodyParts.Exists(bpd => bpd.tags.Contains(eyeTag)))
                .Select<RecipeDef,HediffDef>(rec => { Log.Message("In the exp.tree: " + rec.label); return rec.addsHediff; }).ToList();
            if (AppropriateHediffs != null)
            {
                foreach (HediffDef hediffdef in AppropriateHediffs)
                {
                    if((hediffdef.addedPartProps?.isBionic ?? false)
                    || (hediffdef.CompProps<HediffCompProperties_NightVision>()?.grantsNightVision ?? false))
                    {
                        Log.Message($"Adding {hediffdef} to list of NV Hediff Defs");
                        NightVisionMod.Instance.ListofNightVisionHediffDefs.Add(hediffdef);
                    }

                    else if (hediffdef.CompProps<HediffCompProperties_NightVision>()?.grantsPhotosensitivity ?? false)
                    {
                        Log.Message($"Adding {hediffdef} to list of PS Hediff Defs");
                        NightVisionMod.Instance.ListofPhotosensitiveHediffDefs.Add(hediffdef);
                    }
                }
            }
            #endregion

            #region Finding Things with NightVisionApparel_Comp
          //  List<ThingDef> NightVisionThings = DefDatabase<ThingDef>.AllDefs.Where(td=>)
            


            #endregion



        }
    }
}
