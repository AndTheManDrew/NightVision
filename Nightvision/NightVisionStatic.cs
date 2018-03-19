using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace NightVision
{
    #region Finding Hediffs that have isbionic prop and are added to sightsource parts
    [StaticConstructorOnStartup]
    static class NightVisionHediffsListMaker
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
        
        static NightVisionHediffsListMaker()
        {
            Log.Message("NV_ListMaker got called");

            
            //Have to go via recipes as there is nothing in the bionic eye hediff
            //that actually says it applies to a sightsource; aside from the label/defname
            //but that would probably cause problems with mods and their janky
            //cool names for bionic stuff and/or eyes
            List<HediffDef> AppropriateHediffs = DefDatabase<RecipeDef>.AllDefs.Where(rpd => 
                rpd.appliedOnFixedBodyParts != null 
                && rpd.addsHediff != null
                && rpd.appliedOnFixedBodyParts.Exists(bpd => bpd.tags.Contains(eyeTag)))
                .Select(rec =>  rec.addsHediff ).ToList();
            if (AppropriateHediffs != null)
            {
                foreach (HediffDef hediffdef in AppropriateHediffs)
                {
                    if((hediffdef.addedPartProps?.isBionic ?? false)
                    || (hediffdef.CompProps<HediffCompProperties_NightVision>()?.grantsNightVision ?? false))
                    {
                        Log.Message($"Adding {hediffdef} to list of NV Hediff Defs");
                        NightVisionSettings.Instance.ListofNightVisionHediffDefs.Add(hediffdef);
                    }

                    else if (hediffdef.CompProps<HediffCompProperties_NightVision>()?.grantsPhotosensitivity ?? false)
                    {
                        Log.Message($"Adding {hediffdef} to list of PS Hediff Defs");
                        NightVisionSettings.Instance.ListofPhotosensitiveHediffDefs.Add(hediffdef);
                    }
                }
            }
        }
    }
    #endregion

    #region Race Dictionary Builder
    [StaticConstructorOnStartup]
    static class NightVisionRaceDictBuilder
    {
        static NightVisionRaceDictBuilder()
        {

            Log.Message("NV Dict");

            List<ThingDef> RaceDefList = DefDatabase<ThingDef>.AllDefs.Where(rdef =>
                rdef.race is RaceProperties Race
                && Race.Humanlike).ToList();
            foreach ( ThingDef rdef in RaceDefList )
            {
                IntRange raceLightFactors = new IntRange();
                if (!NightVisionMod.Settings.DictOfRaceNightVision.ContainsKey(rdef))
                {
                    if (rdef.GetCompProperties<CompProperties_NightVision>() is CompProperties_NightVision compprops)
                    {
                        if (compprops.zeroLightFactor != null || compprops.fullLightFactor != null)
                        {
                            Log.Message("RaceDict: Found CompProperties_NightVision zeroLF or fullLF for: " + rdef.defName);
                            raceLightFactors.min = compprops.zeroLightFactor ?? NightVisionSettings.DefaultLightFactors.min;
                            raceLightFactors.max = compprops.fullLightFactor ?? NightVisionSettings.DefaultLightFactors.max;
                        }
                        else if (compprops.naturalNightVision)
                        {
                            Log.Message("RaceDict: Found CompProperties_NightVision naturalNightVision for: " + rdef.defName);
                            raceLightFactors = new IntRange(1, 1 );
                        }
                    }
                    else
                    {
                        Log.Message("RaceDict: Defaulting light factors for: " + rdef.defName);
                        raceLightFactors = NightVisionSettings.DefaultLightFactors;
                    }
                }
                else
                {
                    raceLightFactors = NightVisionSettings.Instance.DictOfRaceNightVision.TryGetValue(rdef, out raceLightFactors) && raceLightFactors != IntRange.zero? raceLightFactors : NightVisionSettings.DefaultLightFactors;
                }
                if (rdef.GetCompProperties<CompProperties_NightVision>() == null)
                {
                    rdef.comps.Add(new CompProperties_NightVision());
                }
                NightVisionSettings.Instance.DictOfRaceNightVision[rdef] = raceLightFactors;
                Log.Message($"RaceDictBuilder: {rdef} corresponds to {rdef.GetHashCode()}:  [{rdef}][{NightVisionSettings.Instance.DictOfRaceNightVision[rdef]}]");
            }

        }
    }
    #endregion
}
