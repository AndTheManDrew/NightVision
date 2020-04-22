// Nightvision NightVision LordJob_HuntAndHide.cs
// 
// 12 07 2018
// 
// 21 07 2018

using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace NVExperiments
    {
        internal class LordJob_HuntAndHide : LordJob
            {
                public                   IntVec3 LairPos = IntVec3.Invalid;
                [UsedImplicitly] private Faction _faction;

                public LordJob_HuntAndHide() { }

                public LordJob_HuntAndHide(
                    Faction faction) =>
                            _faction = faction;

                public override bool CanBlockHostileVisitors => false;

                public override StateGraph CreateGraph()
                    {
                        var      stateGraph  = new StateGraph();
                        LordToil dayToil     = new LordToil_MakeLairOrHideInIt();
                        LordToil nightToil   = new LordToil_HuntEnemies(LairPos);
                        int      currentHour = GenLocalDate.HourInteger(lord.Map);
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

                        var daytonight = new Transition(dayToil, nightToil);
                        daytonight.AddTrigger(new Trigger_Custom(
                            sunset => Find.TickManager.TicksGame % 60 == 0 && GenLocalDate.HourInteger(lord.Map) > 20
                                      || lord.Map.GameConditionManager.ConditionIsActive(
                                          GameConditionDef.Named("Eclipse"))));

                        stateGraph.AddTransition(daytonight, true);

                        var nighttoday = new Transition(nightToil, dayToil);
                        nighttoday.AddTrigger(new Trigger_Custom(
                            sunrise => Find.TickManager.TicksGame % 60 == 0 && GenLocalDate.HourInteger(lord.Map) < 20
                                                                            && GenLocalDate.HourInteger(lord.Map) > 4
                                                                            && !lord.Map.GameConditionManager
                                                                                    .ConditionIsActive(
                                                                                        GameConditionDef.Named(
                                                                                            "Eclipse"))));

                        stateGraph.AddTransition(nighttoday, true);

                        return stateGraph;
                    }
            }
    }