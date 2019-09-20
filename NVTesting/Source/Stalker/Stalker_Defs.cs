using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI;

namespace NVExperiments
    {
        [UsedImplicitly]
        [DefOf]
        public class Stalker_Defs
            {
                [UsedImplicitly] public static PawnKindDef Mech_Stalker;

                //public static PawnKindDef Stalker_Kind = PawnKindDef.Named("Mech_Stalker");
                public static DutyDef Stalker_Duty;
            }
    }