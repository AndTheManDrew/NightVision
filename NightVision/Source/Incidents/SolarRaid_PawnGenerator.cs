// Nightvision NightVision SolarRaid_PawnGenerator.cs
// 
// 17 10 2018
// 
// 18 10 2018

using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace NightVision
{
    public static class SolarRaid_PawnGenerator
    {
        public static List<Pawn> GeneratePawns(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, bool errorOnZeroResults = true)
        {
            var list = new List<Pawn>();
            PawnGroupKindWorker.pawnsBeingGeneratedNow.Add(item: list);

            try
            {
                GeneratePawns(parms: parms, groupMaker: groupMaker, outPawns: list, errorOnZeroResults: errorOnZeroResults);
            }
            catch (Exception arg)
            {
                Log.Error(text: "Exception while generating pawn group: " + arg, ignoreStopLoggingLimit: false);

                for (var i = 0; i < list.Count; i++)
                {
                    list[index: i].Destroy(mode: DestroyMode.Vanish);
                }

                list.Clear();
            }
            finally
            {
                PawnGroupKindWorker.pawnsBeingGeneratedNow.Remove(item: list);
            }

            return list;
        }

        
        public static void GeneratePawns(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, List<Pawn> outPawns, bool errorOnZeroResults = true)
        {
            bool canBringFood = parms.raidStrategy?.pawnsCanBringFood ?? true;

            Predicate<Pawn> postGear = parms.raidStrategy == null
                        ? null
                        : new Predicate<Pawn>(p => parms.raidStrategy.Worker.CanUsePawn(p: p, otherPawns: outPawns));

            var madeOnePawnIncap = false;
            PawnGenUtility.cachedConvertedPawnKindDefs = new Dictionary<string, PawnKindDef>();

            foreach (PawnGenOption pawnGenOption in SolarRaidGroupMaker.ChoosePawnGenByConstraint(
                pointsTotal: parms.points,
                options: groupMaker.options,
                groupParms: parms
            ))
            {
                PawnKindDef     kind              = pawnGenOption.kind;
                Faction         faction           = parms.faction;
                int             tile              = parms.tile;
                bool            allowFood         = canBringFood;
                bool            inhabitants       = parms.inhabitants;
                Predicate<Pawn> validatorPostGear = postGear;

                var request = new PawnGenerationRequest(
                    kind: PawnGenUtility.ConvertDefAndStoreOld(original: kind),
                    faction: faction,
                    context: PawnGenerationContext.NonPlayer,
                    tile: tile,
                    forceGenerateNewPawn: false,
                    newborn: false,
                    allowDead: false,
                    allowDowned: false,
                    canGeneratePawnRelations: true,
                    mustBeCapableOfViolence: true,
                    colonistRelationChanceFactor: 1f,
                    forceAddFreeWarmLayerIfNeeded: true,
                    allowGay: true,
                    allowFood: allowFood,
                    inhabitant: inhabitants,
                    certainlyBeenInCryptosleep: false,
                    forceRedressWorldPawnIfFormerColonist: false,
                    worldPawnFactionDoesntMatter: false,
                    validatorPreGear: pa => !pa.skills.GetSkill(skillDef: RwDefs.MeleeSkill).TotallyDisabled,
                    validatorPostGear: validatorPostGear,
                    minChanceToRedressWorldPawn: null,
                    fixedBiologicalAge: null,
                    fixedChronologicalAge: null,
                    fixedGender: null,
                    fixedMelanin: null,
                    fixedLastName: null
                );

                Pawn pawn = PawnGenerator.GeneratePawn(request: request);

                if (parms.forceOneIncap && !madeOnePawnIncap)
                {
                    pawn.health.forceIncap           = true;
                    pawn.mindState.canFleeIndividual = false;
                    madeOnePawnIncap                 = true;
                }

                PawnFinaliser(pawn: pawn);
                outPawns.Add(item: pawn);
            }
        }

        public static void PawnFinaliser(Pawn pawn)
        {
            int meleeSkill = pawn.skills.GetSkill(skillDef: RwDefs.MeleeSkill).Level;

            if (meleeSkill < 10)
            {
                pawn.skills.GetSkill(skillDef: RwDefs.MeleeSkill).Level += Rand.RangeInclusive(min: 10 - meleeSkill, max: 10 - meleeSkill + 5);
            }

            var choiceArray = new[] {10 - NVGameComponent.Evilness, 5 + NVGameComponent.Evilness, 5 + NVGameComponent.Evilness};
            var indexes     = new[] {0, 1, 2};

            int choice = indexes.RandomElementByWeight(ind => choiceArray[ind]);

            switch (choice)
            {
                case 1:

                    if (PawnGenUtility.AnyPSHediffsExist.IsNotFalse())
                    {
                        foreach (BodyPartRecord bodyPartRecord in pawn.RaceProps.body.GetPartsWithTag(RwDefs.EyeTag))
                        {
                            var hediff = PawnGenUtility.GetRandomPhotosensitiveHediffDef();

                            if (hediff != null)
                            {
                                pawn.health.AddHediff(hediff, bodyPartRecord);
                            }
                        }

                        if (ApparelGenUtility.GetNullPSApparelDefByTag(pawn.kindDef.apparelTags) is ThingDef appDef)
                        {
                            if (ApparelUtility.HasPartsToWear(pawn, appDef))
                            {
                                Thing apparel = ThingMaker.MakeThing(
                                    appDef,
                                    appDef.MadeFromStuff ? GenStuff.RandomStuffByCommonalityFor(appDef) : null
                                );

                                pawn.apparel.Wear((Apparel) apparel, false);
                            }
                        }

                        break;
                    }
                    else
                    {
                        goto case 2;
                    }
                case 2:

                    if (ApparelGenUtility.GetGiveNVApparelDefByTag(pawn.kindDef.apparelTags) is ThingDef nvAppDef)
                    {
                        if (ApparelUtility.HasPartsToWear(pawn, nvAppDef))
                        {
                            Thing apparel = ThingMaker.MakeThing(
                                nvAppDef,
                                nvAppDef.MadeFromStuff ? GenStuff.RandomStuffByCommonalityFor(nvAppDef) : null
                            );

                            pawn.apparel.Wear((Apparel) apparel, false);
                        }
                    }

                    break;
            }
        
            if (Rand.Chance(NVGameComponent.Evilness / 6f))
            {
                ThingDef shield = RwDefs.ShieldDef;

                if (ApparelUtility.HasPartsToWear(pawn, shield))
                {
                    Thing shieldBelt = ThingMaker.MakeThing(shield, shield.MadeFromStuff ? GenStuff.RandomStuffFor(shield) : null);

                    pawn.apparel.Wear((Apparel) shieldBelt, false);
                }
            }
        }
        
    }
}