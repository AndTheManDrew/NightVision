// Nightvision NightVision DebugFlareRaidPawnGenXml.cs
// 
// 18 10 2018
// 
// 18 10 2018

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using RimWorld;
using Verse;

namespace NightVision.Testing
{
    [HasDebugOutput]
    public class DebugFlareRaidPawnGenXml
    {
        public static RaidStrategyDef GetSmart()
        {
            foreach (RaidStrategyDef raidStrategyDef in DefDatabase<RaidStrategyDef>.AllDefs)
            {
                if (raidStrategyDef.defName == "ImmediateAttackSmart")
                {
                    return raidStrategyDef;
                }
            }

            return RaidStrategyDefOf.ImmediateAttack;
        }



        public static pawnGenTrial CurrentTrial;
        [DebugOutput("Nightvision")]
        public static void FlareRaidPawnGroupsMadeToXml()
        {
            
            Dialog_DebugOptionListLister.ShowSimpleDebugMenu(
                elements: new List<int>
                          {
                              11,
                              12,
                              13,
                              14,
                              21,
                              22,
                              23,
                              24,
                              31,
                              32,
                              33,
                              34,
                              41,
                              42,
                              43,
                              44
                          },
                label: i => $"Points x{i / 10} | MaxPawn x{i % 10}",
                chosen: delegate(int multi)
                {
                    var trialData = new pawnGenTrial();
                    SolarRaidGroupMaker.PointMultiplier       = multi / 10;
                    SolarRaidGroupMaker.MaxPawnCostMultiplier = multi % 10;
                    
                    trialData.pointMultiplier       = SolarRaidGroupMaker.PointMultiplier;
                    trialData.maxPawnCostMultiplier = SolarRaidGroupMaker.MaxPawnCostMultiplier;
                    trialData.trialID               = $"{multi}PawnGenTrial{Rand.Int}";

                    var factions = Find.FactionManager.AllFactions.Where(
                        fac => !fac.def.pawnGroupMakers.NullOrEmpty() && fac.def.humanlikeFaction && fac.def.techLevel >= TechLevel.Industrial
                    ).ToList();
                    
                    int numTrials = factions.Count;
                    trialData.trial = new pawnGenTrialTrial[numTrials];

                    for (var ind = 0; ind < factions.Count; ind++)
                    {
                        Faction           fac   = factions[ind];
                        pawnGenTrialTrial trial = new pawnGenTrialTrial();
                        trial.factionName               = fac.def.LabelCap;
                        trial.minPointsToGenCombatGroup = fac.def.MinPointsToGeneratePawnGroup(groupKind: PawnGroupKindDefOf.Combat);


                        List<float> floats = DebugActionsUtility.PointsOptions(extended: false).ToList();
                        trial.groupGenerated = new pawnGenTrialTrialGroupGenerated[floats.Count];

                        for (var index = 0; index < floats.Count; index++)
                        {
                            float num = floats[index];
                            float obj = num;
                            trial.groupGenerated[index] = groupGenerated(fac: fac, obj);
                        }

                        trialData.trial[ind] = trial;
                    }

                    XmlSerializer mySerializer = new   
                                XmlSerializer(typeof(pawnGenTrial));

                    using (var writer = new StreamWriter("pawnGenData" + Rand.Int + ".xml"))
                    {
                        mySerializer.Serialize(writer, trialData);
                    }
                }
            );
            



        }

        public static pawnGenTrialTrialGroupGenerated groupGenerated(Faction fac, float points)
        {
            if (points < fac.def.MinPointsToGeneratePawnGroup(groupKind: PawnGroupKindDefOf.Combat))
            {
                return null;
            }

            pawnGenTrialTrialGroupGenerated groupGen = new pawnGenTrialTrialGroupGenerated();

            groupGen.originalPoints = points;
            points = IncidentWorker_Raid.AdjustedRaidPoints(
                points: points,
                raidArrivalMode: PawnsArrivalModeDefOf.CenterDrop,
                raidStrategy: GetSmart(),
                faction: fac,
                groupKind: PawnGroupKindDefOf.Combat
            );

            groupGen.modifiedPoints = points;

            var pawnGroupMakerParms = new PawnGroupMakerParms();
            pawnGroupMakerParms.groupKind    = PawnGroupKindDefOf.Combat;
            pawnGroupMakerParms.tile         = Find.CurrentMap.Tile;
            pawnGroupMakerParms.points       = points;
            pawnGroupMakerParms.faction      = fac;
            pawnGroupMakerParms.raidStrategy = GetSmart();
            pawnGroupMakerParms.groupKind    = PawnGroupKindDefOf.Combat;
            Log.Message($"raid strat. = {pawnGroupMakerParms.raidStrategy}");
            

            float maxPawnCost = SolarRaidGroupMaker.MaxPawnCostMultiplier * PawnGroupMakerUtility.MaxPawnCost(
                faction: fac,
                totalPoints: points,
                raidStrategy: GetSmart(),
                groupKind: PawnGroupKindDefOf.Combat
            );

            groupGen.maxPawnCost = maxPawnCost;


            SolarRaidGroupMaker.TryGetRandomPawnGroupMaker(parms: pawnGroupMakerParms, pawnGroupMaker: out PawnGroupMaker groupMaker);
            var pawns = SolarRaid_PawnGenerator.GeneratePawns(parms: pawnGroupMakerParms, groupMaker: groupMaker, errorOnZeroResults: false)
                        .OrderBy(keySelector: pa => pa.kindDef.combatPower).ToList();

            int pawnCount = pawns.Count;
            groupGen.pawn = new pawnGenTrialTrialGroupGeneratedPawn[pawnCount];
            var pointsSpent = 0f;

            for (var index = 0; index < pawnCount; index++)
            {
                Pawn                                pawn     = pawns[index];
                pawnGenTrialTrialGroupGeneratedPawn pawnData = new pawnGenTrialTrialGroupGeneratedPawn();
                pawnData.label       = pawn.KindLabel;
                pawnData.combatPower = (int) pawn.kindDef.combatPower;
                pawnData.primaryEq   = pawn.equipment.Primary != null ? pawn.equipment.Primary.LabelCapNoCount : "no weapon";


                List<Apparel> wornApparel = pawn.apparel.WornApparel;
                var           torsoGear   = "";
                var           eyeWear     = "";

                foreach (Apparel apparel in wornApparel)
                {
                    foreach (BodyPartGroupDef bpgd in apparel.def.apparel.bodyPartGroups)
                    {
                        if (bpgd == BodyPartGroupDefOf.Torso)
                        {
                            torsoGear += $"{apparel.def.LabelCap}, ";
                        }
                        else if (bpgd == BodyPartGroupDefOf.Eyes)
                        {
                            eyeWear += $"{apparel.def.LabelCap}, ";
                        }

                        if (apparel is ShieldBelt)
                        {
                            pawnData.hasShield = true;
                        }
                    }
                }

                pawnData.apparelChest = torsoGear.NullOrEmpty() ? "shirtless" : torsoGear.TrimEnd(' ', ',');

                pawnData.apparelHead = eyeWear.NullOrEmpty() ? "not bespectacled" : eyeWear.TrimEnd(' ', ',');

                

                pawnData.hediffs =  pawn.health.hediffSet.hediffs.Where(hd=> !(hd is Hediff_MissingPart)).Select(hdf => hdf.LabelCap).ToCommaList();
                pointsSpent     += pawn.kindDef.combatPower;

                groupGen.pawn[index] = pawnData;
            }

            groupGen.pointsSpent = pointsSpent;
            return groupGen;
        }
    }
}
