// Nightvision NightVision LordToils.cs
// 
// 12 07 2018
// 
// 21 07 2018

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace NVTesting
    {
        internal class LordToil_MakeLairOrHideInIt : LordToil
            {
                public LordToil_MakeLairOrHideInIt() => data = new LordToilData_MakeLairOrHideInIt();

                public LordToil_MakeLairOrHideInIt(
                    IntVec3 lairPosition) =>
                            data = new LordToilData_MakeLairOrHideInIt {LairPosition = lairPosition};

                private LordToilData_MakeLairOrHideInIt Data => (LordToilData_MakeLairOrHideInIt) data;

                public override void UpdateAllDuties()
                    {
                        var cellsToDig = new List<IntVec3>();
                        cellsToDig.AddRange(GenAdj.AdjacentCellsAndInside
                                                  .Where(vector => (Data.LairPosition + vector).IsValid
                                                                   && (Data.LairPosition + vector).GetFirstMineable(
                                                                       lord.Map) != null)
                                                  .Select(vector => Data.LairPosition + vector));

                        foreach (Pawn pawn in lord.ownedPawns)
                            {
                                pawn.mindState.duty = cellsToDig.Count == 0
                                            ? new PawnDuty(DutyDefOf.SleepForever,    Data.LairPosition)
                                            : new PawnDuty(Stalker_Defs.Stalker_Duty, cellsToDig.Pop());
                            }
                    }

                public override void Init()
                    {
                        base.Init();
                        if (!Data.LairPosition.IsValid)
                            {
                                if (((LordJob_HuntAndHide) lord.LordJob)?.LairPos.IsValid == true)
                                    {
                                        Data.LairPosition = ((LordJob_HuntAndHide) lord.LordJob).LairPos;
                                    }
                                else
                                    {
                                        Data.LairPosition = FindNewLairPosition();
                                        //TODO Null Check thingy
                                        ((LordJob_HuntAndHide) lord.LordJob).LairPos = Data.LairPosition;
                                    }
                            }
                        else if (((LordJob_HuntAndHide) lord.LordJob)?.LairPos.IsValid == true)
                            {
                                Data.LairPosition = ((LordJob_HuntAndHide) lord.LordJob).LairPos;
                            }
                    }

                public IntVec3 FindNewLairPosition()
                    {
                        //TODO make this better
                        Pawn leadPawn;
                        if (lord.AnyActivePawn)
                            {
                                leadPawn = lord.ownedPawns[0];
                            }
                        else
                            {
                                leadPawn = lord.Map.mapPawns.AllPawnsSpawned.Find(
                                    pawn => pawn.kindDef == PawnKindDef.Named("Mech_Stalker"));
                            }

                        Map map = leadPawn.Map;
                        bool foundEmptyCell = CellFinder.TryFindRandomReachableCellNear(leadPawn.Position,
                            map,
                            100,
                            TraverseParms.For(leadPawn, Danger.Deadly, TraverseMode.NoPassClosedDoors, false),
                            vec3 => map.glowGrid.GameGlowAt(vec3) < 0.3f && vec3.Standable(leadPawn.Map)
                                                                         && vec3.Roofed(leadPawn.Map),
                            region => true,
                            out IntVec3 newLairPos);
                        if (foundEmptyCell)
                            {
                                return newLairPos;
                            }

                        bool foundMineableCell = CellFinder.TryFindRandomCellNear(leadPawn.Position,
                            map,
                            1000,
                            vec3 => vec3.InBounds(map) && vec3.Roofed(map) && vec3.GetFirstMineable(map) != null
                                    && !vec3.InNoBuildEdgeArea(map) && IsDeep(vec3, leadPawn),
                            out newLairPos);
                        if (foundMineableCell)
                            {
                                return newLairPos;
                            }

                        Log.Message("LordJob_HuntAndHide found no new lair position");
                        return leadPawn.Position;
                    }

                public override void Notify_ReachedDutyLocation(
                    Pawn pawn)
                    {
                        base.Notify_ReachedDutyLocation(pawn);
                        UpdateAllDuties();
                    }

                private bool IsDeep(
                    IntVec3 vec3,
                    Pawn    pawn)
                    {
                        IntVec3[] adjacentCellVec3s     = GenAdj.AdjacentCells;
                        Map       map                   = pawn.Map;
                        var       neighbourCanBeReached = false;
                        foreach (IntVec3 adjacentCell in adjacentCellVec3s)
                            {
                                IntVec3 neighbour = vec3 + adjacentCell;
                                if (!neighbour.IsValid || !neighbour.InBounds(map)
                                                       || neighbour.GetFirstMineable(map) == null
                                                       || !neighbour.Roofed(map))
                                    {
                                        return false;
                                    }

                                if (pawn.CanReach(neighbour,
                                    PathEndMode.ClosestTouch,
                                    Danger.Deadly,
                                    false,
                                    TraverseMode.PassAllDestroyableThings))
                                    {
                                        neighbourCanBeReached = true;
                                    }
                            }

                        Log.Message("IsDeep: " + "neighb can be reach: " + neighbourCanBeReached + ", for intvec3: "
                                    + vec3);
                        return neighbourCanBeReached;
                    }
            }

        //TODO implement this with glow grid considerations
    }