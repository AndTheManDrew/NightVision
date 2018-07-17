using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace NightVision
{
    class LordToil_MakeLairOrHideInIt : LordToil
    {
        public override void UpdateAllDuties()
            {
                List<IntVec3> cellsToDig = new List<IntVec3>();
                cellsToDig.AddRange(GenAdj.AdjacentCellsAndInside
                                          .Where(vector => (Data.lairPosition + vector).IsValid
                                                           && (Data.lairPosition + vector).GetFirstMineable(lord.Map)
                                                           != null)
                                          .Select(vector => Data.lairPosition + vector));
                
                foreach (Pawn pawn in lord.ownedPawns)
                    {
                        pawn.mindState.duty = cellsToDig.Count == 0
                                    ? new PawnDuty(DutyDefOf.SleepForever,    Data.lairPosition)
                                    : new PawnDuty(Stalker_Defs.Stalker_Duty, cellsToDig.Pop());
                    }

            }

        private LordToilData_MakeLairOrHideInIt Data => (LordToilData_MakeLairOrHideInIt) data;
        public LordToil_MakeLairOrHideInIt()
            {
                this.data = new LordToilData_MakeLairOrHideInIt();
            }

        public LordToil_MakeLairOrHideInIt(IntVec3 lairPosition)
            {
                this.data = new LordToilData_MakeLairOrHideInIt(){lairPosition = lairPosition};
            }

        public override void Init()
            {
                base.Init();
                if (!Data.lairPosition.IsValid)
                    {
                        if (((LordJob_HuntAndHide) lord.LordJob)?.lairPos.IsValid == true)
                            {
                                Data.lairPosition = ((LordJob_HuntAndHide) lord.LordJob).lairPos;
                            }
                        else
                            {
                                Data.lairPosition = FindNewLairPosition();
                                //TODO Null Check thingy
                                ((LordJob_HuntAndHide) lord.LordJob).lairPos = Data.lairPosition;
                            }
                    }
                else if (((LordJob_HuntAndHide) lord.LordJob)?.lairPos.IsValid == true)
                    {
                        Data.lairPosition = ((LordJob_HuntAndHide) lord.LordJob).lairPos;
                    }

            }
        public IntVec3 FindNewLairPosition()
            {
                //TODO make this better
                Pawn leadPawn;
                if (lord.AnyActivePawn)
                    {
                            leadPawn = this.lord.ownedPawns[0];
                    }
                else
                    {
                         leadPawn =
                                    lord.Map.mapPawns.AllPawnsSpawned.Find(
                                        pawn => pawn.kindDef == PawnKindDef.Named("Mech_Stalker"));
                    }
                Map     map      = leadPawn.Map;
                IntVec3 newLairPos;
                bool foundEmptyCell = CellFinder.TryFindRandomReachableCellNear(leadPawn.Position,
                                                                                map,
                                                                                100,
                                                                                TraverseParms.For(leadPawn,
                                                                                                  Danger.Deadly,
                                                                                                  TraverseMode
                                                                                                              .NoPassClosedDoors,
                                                                                                  false),
                                                                                vec3 =>
                                                                                            map.glowGrid
                                                                                               .GameGlowAt(vec3) < 0.3f
                                                                                            && vec3.Standable(leadPawn
                                                                                                                          .Map)
                                                                                            && vec3
                                                                                                        .Roofed(leadPawn
                                                                                                                            .Map),
                                                                                region => true, out newLairPos);
                if (foundEmptyCell)
                    {
                        return newLairPos;
                    }

                bool foundMineableCell =
                            CellFinder.TryFindRandomCellNear(leadPawn.Position, map, 1000,
                                                             vec3 => vec3.InBounds(map)
                                                                     && vec3.Roofed(map)
                                                                     && vec3.GetFirstMineable(map) != null
                                                                     && !vec3.InNoBuildEdgeArea(map)
                                                                     && IsDeep(vec3, leadPawn), out newLairPos);
                if (foundMineableCell)
                    {
                        return newLairPos;
                    }

                Log.Message("LordJob_HuntAndHide found no new lair position");
                return leadPawn.Position;
            }

        private bool IsDeep(IntVec3 vec3, Pawn pawn)
            {
                IntVec3[] adjacentCellVec3s     = GenAdj.AdjacentCells;
                Map       map                   = pawn.Map;
                bool      neighbourCanBeReached = false;
                for (int index = 0; index < adjacentCellVec3s.Length; ++index)
                    {
                        IntVec3 neighbour = vec3 + adjacentCellVec3s[index];
                        if (!neighbour.IsValid || !neighbour.InBounds(map) || neighbour.GetFirstMineable(map) == null
                            || !neighbour.Roofed(map))
                            {
                                return false;
                            }

                        if (pawn.CanReach(neighbour, PathEndMode.ClosestTouch, Danger.Deadly,
                                          false, TraverseMode.PassAllDestroyableThings))
                            {
                                neighbourCanBeReached = true;
                            }
                    }

                Log.Message("IsDeep: " + "neighb can be reach: " + neighbourCanBeReached + ", for intvec3: "
                            + vec3);
                return neighbourCanBeReached;
            }

        public override void Notify_ReachedDutyLocation(
            Pawn pawn)
            {
                base.Notify_ReachedDutyLocation(pawn);
                this.UpdateAllDuties();
            }
    }

        class LordToilData_MakeLairOrHideInIt : LordToilData
            {
                public IntVec3 lairPosition = IntVec3.Invalid;

                public override void ExposeData()
                    {
                        Scribe_Values.Look(ref lairPosition, "lairPosition", forceSave:true);
                    }
            }

        class LordToil_HuntColonists : LordToil
            {
                public override void UpdateAllDuties() { }
            }
}
