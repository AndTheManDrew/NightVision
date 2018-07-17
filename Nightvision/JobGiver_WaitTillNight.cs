using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI;

namespace NightVision
{
    //class JobGiver_WaitTillNight : JobGiver_AIFightEnemies
    //        {
    //    //public IntVec3 TryFindHidingSpot(Pawn pawn)
    //    //    {
    //    //        IntVec3 result;
    //    //        Map map = pawn.Map;
    //    //        bool foundEmptyCell = CellFinder.TryFindRandomReachableCellNear(pawn.Position, map, 100,
    //    //                                                  TraverseParms.For(pawn, Danger.Deadly, TraverseMode.NoPassClosedDoors, false),
    //    //                                                  vec3 => map.glowGrid.GameGlowAt(vec3) < 0.3f && vec3.Standable(pawn.Map) && vec3.Roofed(pawn.Map),
    //    //                                                  region => true, out result);
    //    //        if (foundEmptyCell) return result;

    //    //bool foundMineableCell =
    //    //            CellFinder.TryFindRandomCellNear(pawn.Position, map, 1000,
    //    //                                             vec3 => vec3.InBounds(map)
    //    //                                                     && vec3.Roofed(map)
    //    //                                                     && vec3.GetFirstMineable(map) != null
    //    //                                                     && !vec3.InNoBuildEdgeArea(map)
    //    //                                                     && IsDeep(vec3, pawn), out result);
    //    //                if (foundMineableCell) return result;

    //            //        return pawn.Position;
    //            //    }

    //            private bool IsDeep(IntVec3 vec3, Pawn pawn)
    //                {
    //                    IntVec3[] adjacentCellVec3s = GenAdj.AdjacentCells;
    //                    Map map = pawn.Map;
    //                    bool neighbourCanBeReached = false;
    //                    for (int index = 0; index < adjacentCellVec3s.Length; ++index)
    //                        {
    //                            IntVec3 neighbour = vec3 + adjacentCellVec3s[index];
    //                            if (!neighbour.IsValid || !neighbour.InBounds(map)|| neighbour.GetFirstMineable(map) == null || !neighbour.Roofed(map))
    //                                {
    //                                    return false;
    //                                }

    //                            if (pawn.CanReach(neighbour, PathEndMode.ClosestTouch, Danger.Deadly,
    //                                              false, TraverseMode.PassAllDestroyableThings))
    //                                {
    //                                    neighbourCanBeReached = true;
    //                                }

    //                        }

    //                    Log.Message("IsDeep: " + "neighb can be reach: " + neighbourCanBeReached + ", for intvec3: "
    //                                + vec3);
    //                    return neighbourCanBeReached;
    //                }

        
    //    protected override Job TryGiveJob(Pawn pawn)
    //        {
    //            IntVec3 intVec3 = (IntVec3)pawn.mindState.forcedGotoPosition;
    //            Log.Message("Forcedgoto: " + pawn.mindState.forcedGotoPosition.ToString());
    //            Log.Message("duty: " + pawn.mindState.duty);
    //            if (intVec3.IsValid && (double) intVec3.DistanceToSquared(pawn.Position) < 100.0
    //                                && (intVec3.GetRoom(pawn.Map, RegionType.Set_Passable)
    //                                    == pawn.GetRoom(RegionType.Set_Passable)
    //                                    && intVec3.WithinRegions(pawn.Position, pawn.Map, 9,
    //                                                             (TraverseParms) TraverseMode.NoPassClosedDoors,
    //                                                             RegionType.Set_Passable)))
    //                {
    //                    Log.Message("Stalker arrived at dest");
    //                    return new Job(JobDefOf.Wait_Wander, pawn.Position);
    //                }

    //            Map     map = pawn.Map;
    //        bool foundEmptyCell = CellFinder.TryFindRandomReachableCellNear(pawn.Position, map, 100,
    //                                                                        TraverseParms.For(pawn, Danger.Deadly, TraverseMode.NoPassClosedDoors, false),
    //                                                                        vec3 => map.glowGrid.GameGlowAt(vec3) < 0.3f && vec3.Standable(pawn.Map) && vec3.Roofed(pawn.Map),
    //                                                                        region => true, out intVec3);
    //            if (foundEmptyCell)
    //                {
    //                    pawn.mindState.forcedGotoPosition = intVec3;
    //                    return new Job(JobDefOf.Goto, intVec3, 500, true);
    //                }

    //            bool foundMineableCell =
    //                        CellFinder.TryFindRandomCellNear(pawn.Position, map, 1000,
    //                                                         vec3 => vec3.InBounds(map)
    //                                                                 && vec3.Roofed(map)
    //                                                                 && vec3.GetFirstMineable(map) != null
    //                                                                 && !vec3.InNoBuildEdgeArea(map)
    //                                                                 && IsDeep(vec3, pawn), out intVec3);
    //            if (foundMineableCell)
    //                {
    //                    pawn.mindState.forcedGotoPosition = intVec3;
    //                    using (PawnPath path = pawn.Map.pathFinder.FindPath(pawn.Position, (LocalTargetInfo) intVec3,
    //                                                                        TraverseParms.For(pawn, Danger.Deadly,
    //                                                                                          TraverseMode
    //                                                                                                      .PassAllDestroyableThings,
    //                                                                                          false),
    //                                                                        PathEndMode.OnCell))
    //                        {
    //                            IntVec3 cellBefore;
    //                            Thing   blocker = path.FirstBlockingBuilding(out cellBefore, pawn);
    //                            if (blocker != null)
    //                                {
    //                                    Job job = DigUtility.PassBlockerJob(pawn, blocker, cellBefore, true, false);
    //                                    if (job != null)
    //                                        return job;
    //                                }
    //                        }
    //                }

    //            Log.Message("Stalker found no dest");
    //            return new Job(JobDefOf.Wait_Wander, pawn.Position, 500);
    //    }
    //}
    //[DefOf]
    //class WaitTillNight_JobDefOf
    //{
    //    public static JobDef WaitTillNight;
    //}
}
