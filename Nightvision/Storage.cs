using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace NightVision
{
    
    static class NightVisionDatabase
    {
        #region Finding Hediffs that have isbionic prop and are added to sightsource parts
        const string eyeTag = "SightSource";

        public static void HediffsListMaker()
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
                        NightVisionSettings.NightVisionHediffDefs.Add(hediffdef);
                    }

                    else if (hediffdef.CompProps<HediffCompProperties_NightVision>()?.grantsPhotosensitivity ?? false)
                    {
                        Log.Message($"Adding {hediffdef} to list of PS Hediff Defs");
                        NightVisionSettings.PhotosensitiveHediffDefs.Add(hediffdef);
                    }
                }
            }
            
        }
        #endregion


        #region Race Dictionary Builder
        public static void RaceDictBuilder()
        {
            List<ThingDef> RaceDefList = DefDatabase<ThingDef>.AllDefs.Where(rdef =>
                rdef.race is RaceProperties Race
                && Race.Humanlike).ToList();
            foreach ( ThingDef rdef in RaceDefList )
            {
                IntRange raceLightFactors = new IntRange();
                if (!NightVisionSettings.RaceNightVisionFactors.ContainsKey(rdef))
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
                    raceLightFactors = NightVisionSettings.RaceNightVisionFactors.TryGetValue(rdef, out raceLightFactors) && raceLightFactors != IntRange.zero? raceLightFactors : NightVisionSettings.DefaultLightFactors;
                }
                if (rdef.GetCompProperties<CompProperties_NightVision>() == null)
                {
                    rdef.comps.Add(new CompProperties_NightVision());
                }
                NightVisionSettings.RaceNightVisionFactors[rdef] = raceLightFactors;
                Log.Message($"RaceDictBuilder: {rdef} corresponds to {rdef}:  [{rdef}][{NightVisionSettings.RaceNightVisionFactors[rdef]}]");
            }
        }

        #endregion

        #region Apparel Dictionary Builder
        /// <summary>
        /// Builds a dictionary with [key = ThingDef][values = 
        /// </summary>
        // TODO Make this a dictionary?????
        public static void ApparelDictBuilder()
        {
            ThingCategoryDef headgearCategoryDef = ThingCategoryDef.Named("Headgear");

            List<ThingDef> ApparelDefs = DefDatabase<ThingDef>.AllDefs.Where(adef =>
                adef.IsApparel && adef.thingCategories.Contains(headgearCategoryDef)).ToList();
            //Add defs that have NV comp to the list
            foreach (ThingDef apparel in ApparelDefs)
            {
                if (apparel.comps.Exists(comp => comp is CompProperties_NightVisionApparel)
                    && !NightVisionSettings.NVApparel.ContainsKey(apparel))
                {
                    CompProperties_NightVisionApparel compprops =
                        (CompProperties_NightVisionApparel)apparel.comps.First(comp => comp is CompProperties_NightVisionApparel);
                    NightVisionSettings.NVApparel[apparel] = new ApparelSetting(compprops);
                }
            }
        }
        #endregion
    }
    
    public struct ApparelSetting : IExposable
    {
        //public ThingDef ParentApparel;
        public bool NullifiesPS;
        public bool GrantsNV;
        public ApparelSetting(bool nullifiesPhotosensitivity, bool grantsNightVision)
        {
            //ParentApparel = apparel;
            NullifiesPS = nullifiesPhotosensitivity;
            GrantsNV = grantsNightVision;
        }
        public ApparelSetting(CompProperties_NightVisionApparel compprops)
        {
           // ParentApparel = apparel;
            NullifiesPS = compprops.nullifiesPhotosensitivity;
            GrantsNV = compprops.grantsNightVision;
        }
        public bool Equals(ApparelSetting other)
        {
            return this.GrantsNV == other.GrantsNV && this.NullifiesPS == other.NullifiesPS;
        }
        public bool IsRedundant()
        {
            return !(this.GrantsNV || this.NullifiesPS);
        }
        public void ExposeData()
        {
            //Scribe_Defs.Look(ref ParentApparel, ParentApparel.defName);
            Scribe_Values.Look(ref NullifiesPS, "nullifiesphotosens", false);
            Scribe_Values.Look(ref GrantsNV, "grantsnightvis", false);
        }

    }
}
