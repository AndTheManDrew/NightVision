using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;
using Verse.AI.Group;

namespace NightVision
{
    class JobGiver_MakeLord : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
            {
            //TODO work out how to change pawns current lord
            if (pawn.GetLord() != null)
            {
                //Log.Message("Tried to make lord for pawn but pawn had lord.");
                pawn.GetLord().Notify_PawnLost(pawn, PawnLostCondition.Undefined);
                //return null;
            }
            Map map = pawn.Map;
                foreach (var lord in map.lordManager.lords)
                    {
                        if (lord.LordJob is LordJob_HuntAndHide)
                            {
                                lord.AddPawn(pawn);
                                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
                                return null;
                            }
                    }

                List<Pawn> stalkersOnMap = new List<Pawn>();

                foreach (var mapPawn in map.mapPawns.AllPawnsSpawned)
                    {
                        if (mapPawn.Faction == Faction.OfMechanoids && mapPawn.kindDef == Stalker_Defs.Mech_Stalker)
                            {
                                stalkersOnMap.Add(mapPawn);
                            }
                    }

                LordMaker.MakeNewLord(Faction.OfMechanoids, (LordJob) new LordJob_HuntAndHide(Faction.OfMechanoids), map, (IEnumerable<Pawn>) stalkersOnMap);

                for (int index = 0; index < stalkersOnMap.Count; ++index)
                    stalkersOnMap[index].jobs.EndCurrentJob(JobCondition.InterruptForced, true);

                return null;
            }
    }
}
