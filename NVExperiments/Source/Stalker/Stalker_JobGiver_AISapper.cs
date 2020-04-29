// Nightvision NightVision Stalker_JobGiver_AISapper.cs
// 
// 14 07 2018
// 
// 21 07 2018

using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace NVExperiments
    {
        //Copy and paste but forced no mining non-mineables
        [UsedImplicitly]
        internal class Stalker_JobGiver_AISapper : ThinkNode_JobGiver
            {
                protected override Job TryGiveJob(
                    Pawn pawn)
                    {
                        var intVec = (IntVec3) pawn.mindState.duty.focus;
                        if (intVec.IsValid)
                            {
                                if (intVec.DistanceToSquared(pawn.Position) < 100f
                                    && intVec.GetRoom(pawn.Map, RegionType.Set_Passable)
                                    == pawn.GetRoom(RegionType.Set_Passable) && intVec.WithinRegions(pawn.Position,
                                        pawn.Map,
                                        9,
                                        TraverseMode.NoPassClosedDoors,
                                        RegionType.Set_Passable))
                                    {
                                        pawn.GetLord().Notify_ReachedDutyLocation(pawn);
                                        return null;
                                    }
                            }

                        if (!intVec.IsValid)
                            {
                                if (!(from x in pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn)
                                                where !x.ThreatDisabled(pawn) && x.Thing.Faction == Faction.OfPlayer
                                                                              && pawn.CanReach(x.Thing,
                                                                                  PathEndMode.OnCell,
                                                                                  Danger.Deadly,
                                                                                  false,
                                                                                  TraverseMode.PassAllDestroyableThings)
                                                select x)
                                            .TryRandomElement(out IAttackTarget attackTarget))
                                    {
                                        return null;
                                    }

                                intVec = attackTarget.Thing.Position;
                            }

                        Job result;
                        if (!pawn.CanReach(intVec,
                            PathEndMode.OnCell,
                            Danger.Deadly,
                            false,
                            TraverseMode.PassAllDestroyableThings))
                            {
                                result = null;
                            }
                        else
                            {
                                using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position,
                                    intVec,
                                    TraverseParms.For(pawn,
                                        Danger.Deadly,
                                        TraverseMode.PassAllDestroyableThings,
                                        false),
                                    PathEndMode.OnCell))
                                    {
                                        Thing thing =
                                                    pawnPath.FirstBlockingBuilding(out IntVec3 cellBeforeBlocker, pawn);
                                        if (thing != null)
                                            {
                                                Job job = DigUtility.PassBlockerJob(pawn,
                                                    thing,
                                                    cellBeforeBlocker,
                                                    true,
                                                    false);
                                                if (job != null)
                                                    {
                                                        return job;
                                                    }
                                            }
                                    }

                                result = new Job(JobDefOf.Goto, intVec, 500, true);
                            }

                        return result;
                    }
            }
    }