using Verse;
using Verse.AI.Group;

namespace NVIncidents
    {
        internal class LordToilData_MakeLairOrHideInIt : LordToilData
            {
                public IntVec3 LairPosition = IntVec3.Invalid;

                public override void ExposeData()
                    {
                        Scribe_Values.Look(ref LairPosition, "lairPosition", forceSave: true);
                    }
            }
    }