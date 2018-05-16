using System.Collections.Generic;
using System.Linq;
using NightVision.Comps;
using RimWorld;
using Verse;

namespace NightVision
{
    
    static class NightVisionDictionaryBuilders
    {
        #region Hediff Dict & Lists Builder
        const string EyeTag = "SightSource";

        internal static void MakeHediffsDict()
        {
            //Essentially we construct two collections: 
            //  the first contains all hediffs that affect sight/are applied to eyes/have our HediffComp_NightVision
            //  the second is a subset of the 1st with just those hediffs that apply to the eyes directly
            //  the subset is so that we can mark the eye hediffs, this differentiation lets us ensure that 
            //  two bionic eyes are required to get the full nightvis or photosensitivity where as for non-eye hediffs
            //  only one is required
            //  In both cases, we try and filter quite strictly


            //use HashSets because during gameplay these sets are used for membership checks only ( HashSet.Contains is O(1) )

            //Find all hediffs that effect sight
            NightVisionSettings.AllSightAffectingHediffs = new HashSet<HediffDef>(
                DefDatabase<HediffDef>.AllDefsListForReading.FindAll(hediffdef =>
                                                            hediffdef.stages != null
                                                                && hediffdef.stages.Exists(stage =>
                                                                    stage.capMods != null
                                                                        && stage.capMods.Exists(pcm => pcm.capacity == PawnCapacityDefOf.Sight))));
            //Add all the hediffs with our comp
            NightVisionSettings.AllSightAffectingHediffs.UnionWith(
                    DefDatabase<HediffDef>.AllDefsListForReading.FindAll(hediffdef => hediffdef.HasComp(typeof(HediffComp_NightVision))));

            //Having searched for all references to eyes within the def files these are the only options I have found that reference eyes specifically
            //Find all hediffs that have recipes that apply to eyes -- as fun as it is to be able to dev mode apply a  bionic eye to a pawns knee, it would be nice to have
            // some way of saying where a hediff should be applied within the hediffdef itself
            NightVisionSettings.AllEyeHediffs = new HashSet<HediffDef>(
                DefDatabase<RecipeDef>.AllDefsListForReading.FindAll(recdef =>
                                            recdef.addsHediff != null
                                                && recdef.appliedOnFixedBodyParts != null
                                                    && recdef.appliedOnFixedBodyParts.Exists(bpd => bpd.tags != null
                                                        && bpd.tags.Contains(EyeTag))
                                                            && (recdef.AllRecipeUsers.Any(ru=> ru.race?.Humanlike == true)))
                                                            .Select(recdef => recdef.addsHediff));
            //Add all the eye hediffs that are assigned from HediffGivers: i.e. cataracts from HediffGiver_Birthday
            NightVisionSettings.AllEyeHediffs.UnionWith(
                DefDatabase<HediffGiverSetDef>.AllDefsListForReading
                                            .FindAll(hgsd => hgsd.hediffGivers != null)
                                                .SelectMany(hgsd => hgsd.hediffGivers.Where(hg => hg.partsToAffect != null
                                                && hg.partsToAffect.Exists(bpd => bpd.tags.Contains(EyeTag))).Select(hg => hg.hediff)));

            //Using IEnumerable except because hashset exceptwith changes in place and returns void
            List<HediffDef> list = NightVisionSettings.AllSightAffectingHediffs.Except(NightVisionSettings.AllEyeHediffs).ToList();

            NightVisionSettings.AllSightAffectingHediffs.UnionWith(NightVisionSettings.AllEyeHediffs);

            if (NightVisionSettings.HediffLightMods == null)
            {
                NightVisionSettings.HediffLightMods = new Dictionary<HediffDef, Hediff_LightModifiers>();
            }
            
            //Check to see if any non eye hediffs have the right comp
            foreach (var hediffDef in list)
            {
                if (!NightVisionSettings.HediffLightMods.TryGetValue(hediffDef, out Hediff_LightModifiers value) || value == null)
                {
                    
                }
            }
            list = NightVisionSettings.AllEyeHediffs.ToList();

            //Do the same thing as above but for eye hediffs; 
            foreach (var hediffDef in list)
            {
                if (!NightVisionSettings.HediffLightMods.TryGetValue(hediffDef, out LightModifiers value) || value == null)
                {
                    if (hediffDef.CompPropsFor(typeof(HediffComp_NightVision)) is HediffCompProperties_NightVision compprops)
                    {
                        NightVisionSettings.HediffLightMods[hediffDef] = new EyeLightModifiers(compprops);
                    }
                    //bionic eyes and such are automatically assigned night vision, this can be individually overridden in the mod settings
                    //this does not include bionic implants e.g. EPOE's tactical cornea implant, which do not remove the eye
                    else if (hediffDef.addedPartProps is AddedBodyPartProps abpp && abpp.isBionic && abpp.isSolid)
                    {
                        NightVisionSettings.HediffLightMods[hediffDef] 
                            = new EyeLightModifiers(LightModifiers.Options.NVNightVision)
                                { FileSetting = LightModifiers.Options.NVNightVision, LoadedFromFile = true };
                    }
                }
                else if (hediffDef.CompPropsFor(typeof(HediffComp_NightVision)) is HediffCompProperties_NightVision compprops)
                {
                    // Check to see if class is correct for an eye hediff
                    if (!(value is EyeLightModifiers))
                    {
                        NightVisionSettings.HediffLightMods[hediffDef] = new EyeLightModifiers(value);
                    }
                    value.AttachComp(compprops);
                }
            }
            
            
        }
        #endregion

        #region Race Dictionary Builder
        internal static void RaceDictBuilder()
        {
            List<ThingDef> raceDefList = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(rdef =>
                rdef.race is RaceProperties race
                && race.Humanlike);
            if (NightVisionSettings.RaceLightMods == null)
            {
                NightVisionSettings.RaceLightMods = new Dictionary<ThingDef, Race_LightModifiers>();
            }
            foreach ( ThingDef rdef in raceDefList )
            {
                if (!NightVisionSettings.RaceLightMods.TryGetValue(rdef, out Race_LightModifiers rLM) || rLM == null)
                {
                    NightVisionSettings.RaceLightMods[rdef] = new Race_LightModifiers(rdef);
                }
                // Note: When dictionary is loaded and calls exposedata on the saved Race_LightModifiers the def & corresponding compProps are attached
            }
        }

        #endregion

        #region Apparel Dict & List Builder
        internal static void ApparelDictBuilder()
        {
            //ThingCategoryDef headgearCategoryDef = ThingCategoryDef.Named("Headgear");  --- adds stuff like cowboy hat; which, though cool, isnt what I'm going for
            BodyPartGroupDef fullHead = BodyPartGroupDefOf.FullHead;
            BodyPartGroupDef eyes = BodyPartGroupDefOf.Eyes;

            NightVisionSettings.AllEyeCoveringHeadgearDefs = new HashSet<ThingDef>(DefDatabase<ThingDef>.AllDefsListForReading.FindAll(adef =>
                adef.IsApparel 
                && (/*adef.thingCategories.Contains(headgearCategoryDef)
                ||*/ adef.apparel.bodyPartGroups.Any(bpg => bpg == eyes || bpg == fullHead)
                || adef.HasComp(typeof(Comp_NightVisionApparel)))));
            if (NightVisionSettings.NVApparel == null)
            {
                NightVisionSettings.NVApparel = new Dictionary<ThingDef, ApparelSetting>();
            }
            //Add defs that have NV comp
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
