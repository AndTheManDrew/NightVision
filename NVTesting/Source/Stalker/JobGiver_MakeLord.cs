// Nightvision NightVision JobGiver_MakeLord.cs
// 
// 13 07 2018
// 
// 21 07 2018

using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace NVExperiments
    {
        //Just used for testing - should assign lord in incident worker
        [UsedImplicitly]
        internal class JobGiver_MakeLord : ThinkNode_JobGiver
            {
                protected override Job TryGiveJob(
                    Pawn pawn)
                    {
                        //TODO work out how to change pawns current lord
                        if (pawn.GetLord() != null)
                            {
                                //Log.Message("Tried to make lord for pawn but pawn had lord.");
                                pawn.GetLord().Notify_PawnLost(pawn, PawnLostCondition.Undefined);
                                //return null;
                            }

                        Map map = pawn.Map;
                        foreach (Lord lord in map.lordManager.lords)
                            {
                                if (lord.LordJob is LordJob_HuntAndHide)
                                    {
                                        lord.AddPawn(pawn);
                                        pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
                                        return null;
                                    }
                            }

                        var stalkersOnMap = new List<Pawn>();

                        foreach (Pawn mapPawn in map.mapPawns.AllPawnsSpawned)
                            {
                                if (mapPawn.Faction == Faction.OfMechanoids
                                    && mapPawn.kindDef == Stalker_Defs.Mech_Stalker)
                                    {
                                        stalkersOnMap.Add(mapPawn);
                                    }
                            }

                        LordMaker.MakeNewLord(Faction.OfMechanoids,
                            new LordJob_HuntAndHide(Faction.OfMechanoids),
                            map,
                            stalkersOnMap);

                        foreach (Pawn stalker in stalkersOnMap)
                            {
                                stalker.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
                            }

                        return null;
                    }
            }
    }