using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace NightVision
{
    
    static class NightVisionDatabaseBuilders
    {
        #region Finding Hediffs that have isbionic prop and are added to sightsource parts
        const string eyeTag = "SightSource";
        const string brainTag = "ConsciousnessSource";

        internal static void MakeHediffsDict()
        {
            //Have to go via recipes as there is nothing in the bionic eye hediff
            //that actually says it applies to a sightsource; aside from the label/defname
            //but that would probably cause problems with mods and their janky
            //cool names for bionic stuff and/or eyes
            NightVisionSettings.AllRelevantEyeBrainOrBodyHediffs = 
                DefDatabase<RecipeDef>.AllDefs.Where(rpd =>
                    rpd.addsHediff != null
                    && (rpd.targetsBodyPart == false
                    || (rpd.appliedOnFixedBodyParts != null 
                    && rpd.appliedOnFixedBodyParts.Exists(bpd => bpd.tags.Contains(eyeTag) || bpd.tags.Contains(brainTag)))))
                    .Select(rec =>  rec.addsHediff )
                .Union(
                DefDatabase<HediffDef>.AllDefs.Where(hdd => hdd.HasComp(typeof(HediffComp_NightVision))))
                .Union(
                DefDatabase<IncidentDef>.AllDefs.Where(idef => idef.diseaseIncident != null).Select(idef => idef.diseaseIncident))
                .Union(
                DefDatabase<ThingDef>.AllDefs
                    .Where(def => def.IsIngestible && def.ingestible.outcomeDoers != null)
                    .SelectMany(def => def.ingestible.outcomeDoers
                    .Where(od => od is IngestionOutcomeDoer_GiveHediff)
                    .Select(od => ((IngestionOutcomeDoer_GiveHediff)od).hediffDef)))
                .ToList();


            if (NightVisionSettings.NVHediffs == null)
            {
                NightVisionSettings.NVHediffs = new Dictionary<HediffDef, FloatRange>();
            }
            if (!NightVisionSettings.AllRelevantEyeBrainOrBodyHediffs.NullOrEmpty())
            {
                foreach (HediffDef hediffdef in NightVisionSettings.AllRelevantEyeBrainOrBodyHediffs.Where(hediffdef => !NightVisionSettings.NVHediffs.ContainsKey(hediffdef)))
                {
                    // TODO Assign values chosen in settings
                    if((hediffdef.addedPartProps?.isBionic ?? false)
                    || (hediffdef.CompProps<HediffCompProperties_NightVision>()?.grantsNightVision ?? false))
                    {
                        Log.Message($"NightVision: Adding {hediffdef.LabelCap} to NVhediff dictionary with night vision.");
                        NightVisionSettings.NVHediffs[hediffdef] = 
                                new FloatRange(
                                                NightVisionSettings.NightVisionMultipliers.min - NightVisionSettings.DefaultZeroLightMultiplier,
                                                NightVisionSettings.NightVisionMultipliers.max - NightVisionSettings.DefaultFullLightMultiplier);
                    }

                    else if (hediffdef.CompProps<HediffCompProperties_NightVision>()?.grantsPhotosensitivity ?? false)
                    {
                        Log.Message($"NightVision: Adding {hediffdef.LabelCap} to NVhediff dictionary with photosensitivity.");
                        NightVisionSettings.NVHediffs[hediffdef] =
                                new FloatRange(
                                                NightVisionSettings.PhotosensitiveMultipliers.min - NightVisionSettings.DefaultZeroLightMultiplier,
                                                NightVisionSettings.PhotosensitiveMultipliers.max - NightVisionSettings.DefaultFullLightMultiplier);
                    }
                    //TODO Add custom hediff mods??
                }
            }
            
        }
        #endregion


        #region Race Dictionary Builder
        internal static void RaceDictBuilder()
        {
            List<ThingDef> RaceDefList = DefDatabase<ThingDef>.AllDefs.Where(rdef =>
                rdef.race is RaceProperties Race
                && Race.Humanlike).ToList();
            if (NightVisionSettings.RaceNVMultipliers == null)
            {
                NightVisionSettings.RaceNVMultipliers = new Dictionary<ThingDef, FloatRange>();
            }
            foreach ( ThingDef rdef in RaceDefList )
            {
                if (!NightVisionSettings.RaceNVMultipliers.ContainsKey(rdef))
                {
                    FloatRange racemultiplier;
                    if (rdef.GetCompProperties<CompProperties_NightVision>() is CompProperties_NightVision compprops)
                    {
                        racemultiplier = new FloatRange(compprops.zeroLightMultplier, compprops.fullLightMultiplier);
                        if (compprops.naturalNightVision)
                        {
                            Log.Message("RaceDict: Found CompProperties_NightVision naturalNightVision for: " + rdef.defName);
                            racemultiplier.min = 1f;
                        }
                    }
                    else
                    {
                        racemultiplier = new FloatRange(NightVisionSettings.DefaultZeroLightMultiplier, NightVisionSettings.DefaultFullLightMultiplier);
                    }
                    
                    NightVisionSettings.RaceNVMultipliers[rdef] = racemultiplier;
                }
                if (rdef.GetCompProperties<CompProperties_NightVision>() == null)
                {
                    rdef.comps.Add(new CompProperties_NightVision());
                }
                
                Log.Message($"RaceDictBuilder:  [{rdef}][{NightVisionSettings.RaceNVMultipliers[rdef]}]");
            }
        }

        #endregion

        #region Apparel Dictionary Builder
        internal static void ApparelDictBuilder()
        {
            ThingCategoryDef headgearCategoryDef = ThingCategoryDef.Named("Headgear");
            BodyPartGroupDef fullHead = BodyPartGroupDefOf.FullHead;
            BodyPartGroupDef eyes = BodyPartGroupDefOf.Eyes;

            NightVisionSettings.AllEyeCoveringHeadgearDefs = DefDatabase<ThingDef>.AllDefs.Where(adef =>
                adef.IsApparel 
                && (adef.thingCategories.Contains(headgearCategoryDef)
                || adef.apparel.bodyPartGroups.Any(bpg => bpg == eyes || bpg == fullHead))).ToList();
            if (NightVisionSettings.NVApparel == null)
            {
                NightVisionSettings.NVApparel = new Dictionary<ThingDef, ApparelSetting>();
            }
            //Add defs that have NV comp to the list
            foreach (ThingDef apparel in NightVisionSettings.AllEyeCoveringHeadgearDefs)
            {
                if (apparel.comps.Exists(comp => comp is CompProperties_NightVisionApparel)
                    && !NightVisionSettings.NVApparel.ContainsKey(apparel))
                {
                    CompProperties_NightVisionApparel compprops =
                        (CompProperties_NightVisionApparel)apparel.comps.Find(comp => comp is CompProperties_NightVisionApparel);
                    NightVisionSettings.NVApparel[apparel] = new ApparelSetting(compprops);
                }
            }
        }
        #endregion
    }
    
    public class ApparelSetting : IExposable
    {
        internal bool NullifiesPS;
        internal bool GrantsNV;
        public ApparelSetting()
        {
            NullifiesPS = false;
            GrantsNV = false;
        }
        internal ApparelSetting(bool nullPS, bool giveNV)
        {
            NullifiesPS = nullPS;
            GrantsNV = giveNV;
        }
        internal ApparelSetting(CompProperties_NightVisionApparel compprops)
        {
            NullifiesPS = compprops.nullifiesPhotosensitivity;
            GrantsNV = compprops.grantsNightVision;
        }
        internal bool Equals(ApparelSetting other) => this.GrantsNV == other.GrantsNV && this.NullifiesPS == other.NullifiesPS;
        internal bool IsRedundant() => !(this.GrantsNV || this.NullifiesPS);
        public void ExposeData()
        {
            Scribe_Values.Look(ref NullifiesPS, "nullifiesphotosens");
            Scribe_Values.Look(ref GrantsNV, "grantsnightvis");
        }
    }
    
}
