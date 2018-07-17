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

    public class Stalker_ModExtension : DefModExtension
    {
    }

        public class Stealth_ModExtension : DefModExtension
            {
                public float lowlightbodysizefactor;
            }

    [UsedImplicitly]
    [DefOf]
        public class Stalker_Defs
        {
            public static PawnKindDef Mech_Stalker;
                //public static PawnKindDef Stalker_Kind = PawnKindDef.Named("Mech_Stalker");
                public static DutyDef Stalker_Duty;
            }
    
}
