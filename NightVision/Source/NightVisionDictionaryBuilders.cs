// Nightvision NightVision DictionaryBuilders.cs
// 
// 05 05 2018
// 
// 21 07 2018

using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace NightVision
{
    internal static class NightVisionDictionaryBuilders
    {
        #region Race Dictionary Builder

        internal static void RaceDictBuilder()
        {
            List<ThingDef> raceDefList =
                        DefDatabase<ThingDef>.AllDefsListForReading.FindAll(
                                                                            rdef => rdef.race is RaceProperties race
                                                                                    && (race.Humanlike
                                                                                        || rdef
                                                                                                    .GetCompProperties<
                                                                                                                    CompProperties_NightVision
                                                                                                                >()
                                                                                        != null)
                                                                           );

            if (Storage.RaceLightMods == null)
            {
                Storage.RaceLightMods = new Dictionary<ThingDef, Race_LightModifiers>();
            }

            foreach (ThingDef rdef in raceDefList)
            {
                if (!Storage.RaceLightMods.TryGetValue(rdef, out Race_LightModifiers rLm)
                    || rLm == null)
                {
                    Storage.RaceLightMods[rdef] = new Race_LightModifiers(rdef);
                }

                // Note: When dictionary is loaded and calls exposedata on the saved Race_LightModifiers the def & corresponding compProps are attached
            }
        }

        #endregion

        #region Apparel Dict & List Builder

        internal static void ApparelDictBuilder()
        {
            ThingCategoryDef headgearCategoryDef = ThingCategoryDef.Named("Headgear");
            BodyPartGroupDef fullHead            = BodyPartGroupDefOf.FullHead;
            BodyPartGroupDef eyes                = BodyPartGroupDefOf.Eyes;

            Storage.AllEyeCoveringHeadgearDefs = new HashSet<ThingDef>(
                                                                       DefDatabase<ThingDef>
                                                                                   .AllDefsListForReading.FindAll(
                                                                                                                  adef
                                                                                                                              => adef
                                                                                                                                             .IsApparel
                                                                                                                                 && (
                                                                                                                                     (adef
                                                                                                                                      .thingCategories
                                                                                                                                      ?.Contains(
                                                                                                                                                 headgearCategoryDef
                                                                                                                                                )
                                                                                                                                      ?? false
                                                                                                                                     )
                                                                                                                                     || adef
                                                                                                                                        .apparel
                                                                                                                                        .bodyPartGroups
                                                                                                                                        .Any(
                                                                                                                                             bpg
                                                                                                                                                         => bpg
                                                                                                                                                            == eyes
                                                                                                                                                            || bpg
                                                                                                                                                            == fullHead
                                                                                                                                            )
                                                                                                                                     || adef
                                                                                                                                                 .HasComp(
                                                                                                                                                          typeof
                                                                                                                                                                      (Comp_NightVisionApparel
                                                                                                                                                                      )
                                                                                                                                                         ))
                                                                                                                 )
                                                                      );

            if (Storage.NVApparel == null)
            {
                Storage.NVApparel = new Dictionary<ThingDef, ApparelVisionSetting>();
            }

            //Add defs that have NV comp
            foreach (ThingDef apparel in Storage.AllEyeCoveringHeadgearDefs)
            {
                if (apparel.comps.Find(comp => comp is CompProperties_NightVisionApparel) is
                            CompProperties_NightVisionApparel)
                {
                    if (!Storage.NVApparel.TryGetValue(apparel, out ApparelVisionSetting setting))
                    {
                        Storage.NVApparel[apparel] = new ApparelVisionSetting(apparel);
                    }
                    else
                    {
                        setting.InitExistingSetting(apparel);
                    }
                }
                else
                {
                    //This attaches a new comp to the def as a placeholder
                    //Note these comps never get removed, nor do they get saved TODO review
                    ApparelVisionSetting.CreateNewApparelVisionSetting(apparel);
                }
            }
        }

        #endregion

        #region Add Tapetum to large predator

        internal static void TapetumInjector()
        {
            //TODO simplify/inline/merge_loops
            var                bestAnimals     = new List<ThingDef>();
            ResearchProjectDef tapetumResearch = ResearchProjectDef.Named("TapetumImplant");
            var                descAppendage   = new StringBuilder();

            foreach (BiomeDef biome in DefDatabase<BiomeDef>.AllDefs)
            {
                if (!biome.AllWildAnimals.Any())
                {
                    continue;
                }

                List<PawnKindDef> possibleAnimals = biome
                                                    .AllWildAnimals
                                                    .Where(
                                                           pkd => pkd.RaceProps.predator
                                                                  && pkd.RaceProps.baseBodySize > 1
                                                          )
                                                    .ToList();

                if (possibleAnimals.Count == 0)
                {
                    continue;
                }

                ThingDef bestAnimal = possibleAnimals.Aggregate(
                                                                (
                                                                                best,
                                                                                next
                                                                            ) =>
                                                                            best.RaceProps.baseBodySize
                                                                            > next
                                                                              .RaceProps.baseBodySize
                                                                                        ? best
                                                                                        : next
                                                               )
                                                     .race;

                bestAnimals.AddDistinct(bestAnimal);
#if DEBUG
                                Log.Message(
                                    $"NightVision.NightVisionDictionaryBuilders.TapetumInjector: {biome} best animal = {bestAnimal}");
#endif
            }

            foreach (ThingDef animal in bestAnimals)
            {
                if (animal.recipes.NullOrEmpty())
                {
                    animal.recipes = new List<RecipeDef>();
                }

                animal.recipes.Add(RecipeDef_ExtractTapetumLucidum.ExtractTapetumLucidum);
#if DEBUG
                                Log.Message(
                                    $"NightVision.NightVisionDictionaryBuilders.TapetumInjector: Added tapetum recipe to: {animal}");
#endif

                descAppendage.Append("\n - " + animal.LabelCap);
            }

            tapetumResearch.description += descAppendage.ToString();
#if DEBUG
                        Log.Message(
                            $"NightVision.NightVisionDictionaryBuilders.TapetumInjector: {tapetumResearch.description}");
#endif
        }

        #endregion

        #region Hediff Dict & Lists Builder

        private static readonly BodyPartTagDef EyeTag = BodyPartTagDefOf.SightSource;

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

            /*
                This is vanilla 1.0 implementation of the same thing from ThingDef.SpecialDisplayStats TODO see if this would be better
               foreach (RecipeDef def in from x in DefDatabase<RecipeDef>.AllDefs
               where x.IsIngredient(this.$this)
               select x)
            */


            //Find all hediffs that effect sight
            Storage.AllSightAffectingHediffs = new HashSet<HediffDef>(
                                                                      DefDatabase<HediffDef>
                                                                                  .AllDefsListForReading.FindAll(
                                                                                                                 hediffdef
                                                                                                                             =>
                                                                                                                             hediffdef
                                                                                                                                         .stages
                                                                                                                             != null
                                                                                                                             && hediffdef
                                                                                                                                .stages
                                                                                                                                .Exists(
                                                                                                                                        stage
                                                                                                                                                    =>
                                                                                                                                                    stage
                                                                                                                                                                .capMods
                                                                                                                                                    != null
                                                                                                                                                    && stage
                                                                                                                                                       .capMods
                                                                                                                                                       .Exists(
                                                                                                                                                               pcm
                                                                                                                                                                           =>
                                                                                                                                                                           pcm
                                                                                                                                                                                       .capacity
                                                                                                                                                                           == PawnCapacityDefOf
                                                                                                                                                                                       .Sight
                                                                                                                                                              )
                                                                                                                                       )
                                                                                                                )
                                                                     );

            //Add all the hediffs with our comp
            Storage.AllSightAffectingHediffs.UnionWith(
                                                       DefDatabase<HediffDef>.AllDefsListForReading.FindAll(
                                                                                                            hediffdef =>
                                                                                                                        hediffdef
                                                                                                                                    .HasComp(
                                                                                                                                             typeof
                                                                                                                                                         (HediffComp_NightVision
                                                                                                                                                         )
                                                                                                                                            )
                                                                                                           )
                                                      );

            //Having searched for all references to eyes within the def files these are the only options I have found that reference eyes specifically
            //Find all hediffs that have recipes that apply to eyes -- as fun as it is to be able to dev mode apply a  bionic eye to a pawns knee, it would be nice to have
            // some way of saying where a hediff should be applied within the hediffdef itself
            Storage.AllEyeHediffs = new HashSet<HediffDef>(
                                                           DefDatabase<RecipeDef>
                                                                       .AllDefsListForReading
                                                                       .FindAll(
                                                                                recdef => recdef.addsHediff != null
                                                                                          && recdef
                                                                                                      .appliedOnFixedBodyParts
                                                                                          != null
                                                                                          && recdef
                                                                                             .appliedOnFixedBodyParts
                                                                                             .Exists(
                                                                                                     bpd => bpd.tags
                                                                                                            != null
                                                                                                            && bpd
                                                                                                               .tags
                                                                                                               .Contains(
                                                                                                                         NightVisionDictionaryBuilders
                                                                                                                                     .EyeTag
                                                                                                                        )
                                                                                                    )
                                                                                          && recdef.AllRecipeUsers.Any(
                                                                                                                       ru
                                                                                                                                   =>
                                                                                                                                   ru
                                                                                                                                               .race
                                                                                                                                               ?.Humanlike
                                                                                                                                   == true
                                                                                                                      )
                                                                               )
                                                                       .Select(recdef => recdef.addsHediff)
                                                          );

            //Add all the eye hediffs that are assigned from HediffGivers: i.e. cataracts from HediffGiver_Birthday
            Storage.AllEyeHediffs.UnionWith(
                                            DefDatabase<HediffGiverSetDef>
                                                        .AllDefsListForReading
                                                        .FindAll(hgsd => hgsd.hediffGivers != null)
                                                        .SelectMany(
                                                                    hgsd => hgsd
                                                                            .hediffGivers
                                                                            .Where(
                                                                                   hg => hg.partsToAffect != null
                                                                                         && hg.partsToAffect.Exists(
                                                                                                                    bpd
                                                                                                                                =>
                                                                                                                                bpd
                                                                                                                                            .tags
                                                                                                                                            .Contains(
                                                                                                                                                      NightVisionDictionaryBuilders
                                                                                                                                                                  .EyeTag
                                                                                                                                                     )
                                                                                                                   )
                                                                                  )
                                                                            .Select(hg => hg.hediff)
                                                                   )
                                           );

            //Clean up a bit (we rely on comps as an interface between rimworld stat reporting and our calculations)
            Storage.AllEyeHediffs.RemoveWhere(hdD => !typeof(HediffWithComps).IsAssignableFrom(hdD.hediffClass));

            Storage.AllSightAffectingHediffs.RemoveWhere(
                                                         hdD => !typeof(HediffWithComps).IsAssignableFrom(
                                                                                                          hdD
                                                                                                                      .hediffClass
                                                                                                         )
                                                        );

            //Using IEnumerable.except because hashset.exceptwith changes in place and returns void
            List<HediffDef> list = Storage.AllSightAffectingHediffs.Except(Storage.AllEyeHediffs).ToList();

            Storage.AllSightAffectingHediffs.UnionWith(Storage.AllEyeHediffs);

            if (Storage.HediffLightMods == null)
            {
                Storage.HediffLightMods = new Dictionary<HediffDef, Hediff_LightModifiers>();
            }

            //Check to see if any non eye hediffs have the right comp
            foreach (HediffDef hediffDef in list)
            {
                if (!Storage.HediffLightMods.TryGetValue(hediffDef, out Hediff_LightModifiers value)
                    || value == null)
                {
                    if (hediffDef.HasComp(typeof(HediffComp_NightVision)))
                    {
                        Storage.HediffLightMods[hediffDef] =
                                    new Hediff_LightModifiers(hediffDef);
                    }
                }

                if (value != null && AutoQualifier.HediffCheck(hediffDef) != null)
                {
                    value.AutoAssigned = true;
                }
            }

            list = Storage.AllEyeHediffs.ToList();

            //Do the same thing as above but for eye hediffs; 
            foreach (HediffDef hediffDef in list)
            {
                if (!Storage.HediffLightMods.TryGetValue(hediffDef, out Hediff_LightModifiers value)
                    || value == null)
                {
                    if (hediffDef.CompPropsFor(typeof(HediffComp_NightVision)) is
                                HediffCompProperties_NightVision)
                    {
                        Storage.HediffLightMods[hediffDef] =
                                    new Hediff_LightModifiers(hediffDef) {AffectsEye = true};
                    }
                    //bionic eyes and such are automatically assigned night vision, this can be individually overridden in the mod settings
                    else if (AutoQualifier.HediffCheck(hediffDef) is VisionType autoOptions)
                    {
                        Storage.HediffLightMods[hediffDef] =
                                    new Hediff_LightModifiers(hediffDef)
                                    {
                                        AffectsEye   = true,
                                        AutoAssigned = true,
                                        Setting      = autoOptions
                                    };
                    }
                }
                else if (hediffDef.CompPropsFor(typeof(HediffComp_NightVision)) is
                            HediffCompProperties_NightVision)
                {
                    // Ensure bool is correct for an eye hediff
                    value.AffectsEye = true;
                }

                if (value != null && AutoQualifier.HediffCheck(hediffDef) != null)
                {
                    value.AutoAssigned = true;
                }
            }
        }

        #endregion
    }
}