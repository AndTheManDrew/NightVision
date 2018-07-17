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

    class LordJob_HuntAndHide : LordJob
    {
        public IntVec3 lairPos = IntVec3.Invalid;
        private Faction faction;

        public override bool CanBlockHostileVisitors => false;

        public LordJob_HuntAndHide() { }

        public LordJob_HuntAndHide(Faction faction)
        {
            this.faction = faction;
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil dayToil = new LordToil_MakeLairOrHideInIt();
            LordToil nightToil = new LordToil_HuntEnemies(lairPos);
            int currentHour = GenLocalDate.HourInteger(lord.Map);
            if (currentHour > 20 || currentHour < 3)
                {
                    stateGraph.AddToil(nightToil);
                    stateGraph.AddToil(dayToil);
                }
            else
                {
                    stateGraph.AddToil(dayToil);
                    stateGraph.AddToil(nightToil);
                }

            Transition daytonight = new Transition(dayToil, nightToil);
            daytonight.AddTrigger(new Trigger_Custom(
                (sunset => Find.TickManager.TicksGame % 60 == 0 && GenLocalDate.HourInteger(lord.Map) > 20
                           || lord.Map.GameConditionManager.ConditionIsActive(GameConditionDef.Named("Eclipse")))));

            stateGraph.AddTransition(daytonight, true);

            Transition nighttoday = new Transition(nightToil, dayToil);
            nighttoday.AddTrigger(new Trigger_Custom(sunrise => Find.TickManager.TicksGame % 60 == 0
                                                                && GenLocalDate.HourInteger(lord.Map) < 20
                                                                && GenLocalDate.HourInteger(lord.Map) > 4
                                                                && !lord.Map.GameConditionManager.ConditionIsActive(
                                                                    GameConditionDef.Named("Eclipse"))));

            stateGraph.AddTransition(nighttoday, true);

            return stateGraph;



        }

        
    }
}
