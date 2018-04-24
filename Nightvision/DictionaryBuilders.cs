using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace NightVision
{
    
    static class NightVisionDictionaryBuilders
    {
        #region Hediff Dict & Lists Builder
        const string eyeTag = "SightSource";

        internal static void MakeHediffsDict()
        {
            //Find all hediffs that effect sight or have NV comp
            NightVisionSettings.AllSightAffectingHediffs =
                DefDatabase<HediffDef>.AllDefsListForReading.FindAll(hediffdef => 
                                                            hediffdef.stages != null
                                                                && hediffdef.stages.Exists(stage => 
                                                                    stage.capMods != null
                                                                        && stage.capMods.Exists(pcm => pcm.capacity == PawnCapacityDefOf.Sight)))
                .Union(
                    DefDatabase<HediffDef>.AllDefsListForReading.FindAll(hediffdef => hediffdef.HasComp(typeof(HediffComp_NightVision))))
                        .ToList();
            
            //Find all hediffs that are applied to eyes
            NightVisionSettings.AllEyeHediffs =
                DefDatabase<RecipeDef>.AllDefsListForReading.FindAll(recdef =>
                                            recdef.addsHediff != null
                                                && recdef.appliedOnFixedBodyParts != null
                                                    && recdef.appliedOnFixedBodyParts.Exists(bpd => bpd.tags != null
                                                        && bpd.tags.Contains(eyeTag)))
                                                            .Select(recdef => recdef.addsHediff)
                .Union(
                DefDatabase<HediffGiverSetDef>.AllDefsListForReading
                                            .FindAll(hgsd => hgsd.hediffGivers != null)
                                                .SelectMany(hgsd => hgsd.hediffGivers.Where(hg => hg.partsToAffect != null
                                                && hg.partsToAffect.Exists(bpd => bpd.tags.Contains(eyeTag))).Select(hg => hg.hediff)))

                .ToList();
            if (NightVisionSettings.HediffGlowMods == null)
            {
                NightVisionSettings.HediffGlowMods = new Dictionary<HediffDef, GlowMods>();
            }

            List<HediffDef> list = NightVisionSettings.AllSightAffectingHediffs.Except(NightVisionSettings.AllEyeHediffs).ToList();


            NightVisionSettings.AllSightAffectingHediffs = NightVisionSettings.AllSightAffectingHediffs.Union(NightVisionSettings.AllEyeHediffs).ToList();
            
            //Check to see if any non eye hediffs have the right comp, if so add to dict if not present or attach the data from the comp to the entry
            for (int i = 0; i < list.Count; i++)
            {
                if (!NightVisionSettings.HediffGlowMods.TryGetValue(list[i], out GlowMods value) || value == null)
                {
                    
                    if (list[i].CompPropsFor(typeof(HediffComp_NightVision)) is HediffCompProperties_NightVision compprops)
                    {
                        NightVisionSettings.HediffGlowMods[list[i]] = new GlowMods(compprops);
                    }
                }
                else if (list[i].CompPropsFor(typeof(HediffComp_NightVision)) is HediffCompProperties_NightVision compprops)
                {
                    value.AttachComp(compprops);
                }
            }
            list = NightVisionSettings.AllEyeHediffs;

            //Do the same thing as above but for eye hediffs; 
            for (int i = 0; i < list.Count; i++)
            {
                if (!NightVisionSettings.HediffGlowMods.TryGetValue(list[i], out GlowMods value) || value == null)
                {
                    if (list[i].CompPropsFor(typeof(HediffComp_NightVision)) is HediffCompProperties_NightVision compprops)
                    {
                        NightVisionSettings.HediffGlowMods[list[i]] = new EyeGlowMods(compprops);
                    }
                    else if (list[i].addedPartProps?.isBionic ?? false)
                    {
                        NightVisionSettings.HediffGlowMods[list[i]] 
                            = new EyeGlowMods(GlowMods.Options.NVNightVision)
                                { fileSetting = GlowMods.Options.NVNightVision, LoadedFromFile = true };
                    }
                }
                else if (list[i].CompPropsFor(typeof(HediffComp_NightVision)) is HediffCompProperties_NightVision compprops)
                {
                    if (!(value is EyeGlowMods))
                    {
                        NightVisionSettings.HediffGlowMods[list[i]] = new EyeGlowMods(value, 2);
                    }
                    value.AttachComp(compprops);
                }
            }
            
            
        }
        #endregion

        #region Race Dictionary Builder
        internal static void RaceDictBuilder()
        {
            List<ThingDef> RaceDefList = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(rdef =>
                rdef.race is RaceProperties Race
                && Race.Humanlike);
            if (NightVisionSettings.RaceGlowMods == null)
            {
                NightVisionSettings.RaceGlowMods = new Dictionary<ThingDef, GlowMods>();
            }
            foreach ( ThingDef rdef in RaceDefList )
            {
                if (!NightVisionSettings.RaceGlowMods.TryGetValue(rdef, out GlowMods raceGlMods) || raceGlMods == null)
                {
                    //There is null check in the constructor
                    NightVisionSettings.RaceGlowMods[rdef] = new GlowMods(rdef.GetCompProperties<CompProperties_NightVision>());
                }
                else
                {
                    //There is null check in attachcomp
                    raceGlMods.AttachComp(rdef.GetCompProperties<CompProperties_NightVision>());
                }
                if (rdef.GetCompProperties<CompProperties_NightVision>() == null)
                {
                    rdef.comps.Add(new CompProperties_NightVision());
                }
                
            }
        }

        #endregion

        #region Apparel Dict & List Builder
        internal static void ApparelDictBuilder()
        {
            ThingCategoryDef headgearCategoryDef = ThingCategoryDef.Named("Headgear");
            BodyPartGroupDef fullHead = BodyPartGroupDefOf.FullHead;
            BodyPartGroupDef eyes = BodyPartGroupDefOf.Eyes;

            NightVisionSettings.AllEyeCoveringHeadgearDefs = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(adef =>
                adef.IsApparel 
                && (adef.thingCategories.Contains(headgearCategoryDef)
                || adef.apparel.bodyPartGroups.Any(bpg => bpg == eyes || bpg == fullHead)
                || adef.HasComp(typeof(Comp_NightVisionApparel))));
            if (NightVisionSettings.NVApparel == null)
            {
                NightVisionSettings.NVApparel = new Dictionary<ThingDef, ApparelSetting>();
            }
            //Add defs that have NV comp to the dict
            foreach (ThingDef apparel in NightVisionSettings.AllEyeCoveringHeadgearDefs)
            {
                if (apparel.comps.Find(comp => comp is CompProperties_NightVisionApparel) is CompProperties_NightVisionApparel compprops)
                {
                    if (!NightVisionSettings.NVApparel.TryGetValue(apparel, out ApparelSetting setting))
                    {
                        NightVisionSettings.NVApparel[apparel] = new ApparelSetting(compprops);
                    }
                    else
                    {
                        setting.AttachComp(compprops);
                    }
                }
            }
        }
        #endregion
    }
}
