using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using NightVision;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace NVIncidents
{
    [HarmonyPatch(typeof(Dialog_DebugActionsMenu), "DoListingItems_AllModePlayActions")]
    public static class PawnGeneratorNV
    {
        public static List<string> UndesiredTraitsMelee = new List<string> {"Wimp", "Tough", "Brawler","Nimble"};
        public static List<string> UndesiredTraitsShoot = new List<string> {"Wimp", "Tough", "ShootingAccuracy"};
        [global::Harmony.HarmonyPostfix]
        public static void DebugActionPostfix(global::Verse.Dialog_DebugActionsMenu __instance)
        {
            global::Harmony.Traverse menuTraverse = global::Harmony.Traverse.Create(__instance);
            menuTraverse.Method("DoGap").GetValue();
            menuTraverse.Method("DoLabel", "NightVision Testing: ").GetValue();
            menuTraverse.Method("DoLabel","   Pawn Spawners: ").GetValue();
            menuTraverse.Method(
                "DebugAction",
                "T: AB Shoot",
                (global::System.Action) delegate
                {
                    global::System.Collections.Generic.List<global::Verse.DebugMenuOption> list3 = new global::System.Collections.Generic.List<global::Verse.DebugMenuOption>();

                    for (int i = 0; i < 4; i++)
                    {
                        int skill = global::System.Math.Min(i * 7, 20);

                        list3.Add(
                            new global::Verse.DebugMenuOption(
                                "ShootSkill: " + skill,
                                global::Verse.DebugMenuOptionMode.Action,
                                delegate
                                {
                                    global::System.Collections.Generic.List<global::Verse.DebugMenuOption> listAB = new global::System.Collections.Generic.List<global::Verse.DebugMenuOption>();

                                    foreach (var ab in global::NVIncidents.PawnGeneratorNV.ABShoot)
                                    {
                                        listAB.Add(
                                            new global::Verse.DebugMenuOption(
                                                ab.label,
                                                global::Verse.DebugMenuOptionMode.Action,
                                                delegate
                                                {
                                                    global::System.Collections.Generic.List<global::Verse.DebugMenuOption> list4 = new global::System.Collections.Generic.List<global::Verse.DebugMenuOption>();

                                                    list4.Add(
                                                        new global::Verse.DebugMenuOption(
                                                            "None",
                                                            global::Verse.DebugMenuOptionMode.Tool,
                                                            delegate
                                                            {
                                                                global::NVIncidents.PawnGeneratorNV.SpawnPawn(skill, ab, null, false, global::NightVision.Defs_Rimworld.ShootSkill);
                                                            }
                                                        )
                                                    );

                                                    foreach (var hediffDef in global::NightVision.Storage.HediffLightMods)
                                                    {
                                                        var nvhediff   = hediffDef.Key;
                                                        var hediffMods = hediffDef.Value;

                                                        list4.Add(
                                                            new global::Verse.DebugMenuOption(
                                                                nvhediff.defName,
                                                                global::Verse.DebugMenuOptionMode.Tool,
                                                                delegate
                                                                {
                                                                    global::NVIncidents.PawnGeneratorNV.SpawnPawn(skill, ab, nvhediff, hediffMods.AffectsEye, global::NightVision.Defs_Rimworld.ShootSkill);
                                                                }
                                                            )
                                                        );
                                                    }

                                                    global::Verse.Find.WindowStack.Add(new global::Verse.Dialog_DebugOptionListLister(list4));
                                                }
                                            )
                                        );
                                    }

                                    global::Verse.Find.WindowStack.Add(new global::Verse.Dialog_DebugOptionListLister(listAB));
                                }
                            )
                        );
                    }

                    global::Verse.Find.WindowStack.Add(new global::Verse.Dialog_DebugOptionListLister(list3));
                }
            ).GetValue();
            menuTraverse.Method(
                "DebugAction",
                "T: AB Melee",
                (global::System.Action) delegate
                {
                    global::System.Collections.Generic.List<global::Verse.DebugMenuOption> list3 = new global::System.Collections.Generic.List<global::Verse.DebugMenuOption>();

                    for (int i = 0; i < 4; i++)
                    {
                        int skill = global::System.Math.Min(i * 7, 20);

                        list3.Add(
                            new global::Verse.DebugMenuOption(
                                "Melee: " + skill,
                                global::Verse.DebugMenuOptionMode.Action,
                                delegate
                                {
                                    global::System.Collections.Generic.List<global::Verse.DebugMenuOption> listAB = new global::System.Collections.Generic.List<global::Verse.DebugMenuOption>();

                                    foreach (var ab in global::NVIncidents.PawnGeneratorNV.ABMelee)
                                    {
                                        listAB.Add(
                                            new global::Verse.DebugMenuOption(
                                                ab.label,
                                                global::Verse.DebugMenuOptionMode.Action,
                                                delegate
                                                {
                                                    global::System.Collections.Generic.List<global::Verse.DebugMenuOption> list4 = new global::System.Collections.Generic.List<global::Verse.DebugMenuOption>();

                                                    list4.Add(
                                                        new global::Verse.DebugMenuOption(
                                                            "None",
                                                            global::Verse.DebugMenuOptionMode.Tool,
                                                            delegate
                                                            {
                                                                global::NVIncidents.PawnGeneratorNV.SpawnPawn(skill, ab, null, false, global::NightVision.Defs_Rimworld.MeleeSkill);
                                                            }
                                                        )
                                                    );

                                                    foreach (var hediffDef in global::NightVision.Storage.HediffLightMods)
                                                    {
                                                        var nvhediff   = hediffDef.Key;
                                                        var hediffMods = hediffDef.Value;

                                                        list4.Add(
                                                            new global::Verse.DebugMenuOption(
                                                                nvhediff.defName,
                                                                global::Verse.DebugMenuOptionMode.Tool,
                                                                delegate
                                                                {
                                                                    global::NVIncidents.PawnGeneratorNV.SpawnPawn(skill, ab, nvhediff, hediffMods.AffectsEye, global::NightVision.Defs_Rimworld.MeleeSkill);
                                                                }
                                                            )
                                                        );
                                                    }

                                                    global::Verse.Find.WindowStack.Add(new global::Verse.Dialog_DebugOptionListLister(list4));
                                                }
                                            )
                                        );
                                    }

                                    global::Verse.Find.WindowStack.Add(new global::Verse.Dialog_DebugOptionListLister(listAB));
                                }
                            )
                        );
                    }

                    global::Verse.Find.WindowStack.Add(new global::Verse.Dialog_DebugOptionListLister(list3));
                }
            ).GetValue();
            menuTraverse.Method("DoLabel", "    Battle Royales ").GetValue();
            menuTraverse.Method(
                "DebugAction",
                "NightVision: Battle Royale Melee",
                (global::System.Action) delegate
                {
                    global::NVIncidents.PawnGeneratorNV.PerformBattleRoyale(global::NightVision.Defs_Rimworld.MeleeSkill);
                }
            ).GetValue();
            menuTraverse.Method(
                "DebugAction",
                "NightVision: Battle Royale ShootSkill",
                (global::System.Action) delegate
                {
                    global::NVIncidents.PawnGeneratorNV.PerformBattleRoyale(global::NightVision.Defs_Rimworld.ShootSkill);
                }
            ).GetValue();
        }

        private class AorB
        {
            public string label;
            public FactionDef fac;
            public PawnKindDef pkd;
            public List<String> undesired;
        }
         private static AorB A_Melee = new AorB {fac = FactionDef.Named("A"), pkd = PawnKindDef.Named("A_NV"), label = "A", undesired = UndesiredTraitsMelee};
        private static AorB B_Melee = new AorB {fac = FactionDef.Named("B"), pkd = PawnKindDef.Named("B_NV"), label = "B", undesired = UndesiredTraitsMelee};
        private static AorB[] ABMelee = {A_Melee, B_Melee};

        private static AorB   A_Shoot= new AorB {fac  = FactionDef.Named("A"), pkd = PawnKindDef.Named("AS_NV"), label = "A", undesired = UndesiredTraitsShoot};
        private static AorB   B_Shoot       = new AorB {fac = FactionDef.Named("B"), pkd = PawnKindDef.Named("BS_NV"), label = "B", undesired = UndesiredTraitsShoot};
        private static AorB[] ABShoot = {A_Shoot, B_Shoot};



        private static void SpawnPawn(int skill, AorB ab, HediffDef hediffs, bool AffectsEye, SkillDef skilldef)
        {
            Faction faction = FactionUtility.DefaultFactionFrom(ab.fac);
            var pawngen = new PawnGenerationRequest(ab.pkd, faction, PawnGenerationContext.NonPlayer, mustBeCapableOfViolence: true, forceGenerateNewPawn: true, worldPawnFactionDoesntMatter: true);
            Pawn newPawn = PawnGenerator.GeneratePawn(pawngen);
            newPawn.skills.GetSkill(skilldef).Level = skill;
            
            newPawn.story.traits.allTraits.RemoveAll(trt => ab.undesired.Contains(trt.def.defName));

            if (hediffs != null)
            {
                if (AffectsEye)
                {
                    foreach (var bpr in newPawn.RaceProps.body.GetPartsWithTag(Defs_Rimworld.EyeTag))
                    {
                        newPawn.health.AddHediff(hediffs, bpr, null, null);
                    }
                }
                else
                {
                    newPawn.health.AddHediff(hediffs);
                }
            }
            GenSpawn.Spawn(newPawn, UI.MouseCell(), Find.CurrentMap, WipeMode.Vanish);

            if (faction != null && faction != Faction.OfPlayer)
            {
                Lord lord = null;
                
                    LordJob_DefendPoint lordJob = new LordJob_DefendPoint(newPawn.Position);
                    lord = LordMaker.MakeNewLord(faction, lordJob, Find.CurrentMap, null);
                

                lord.AddPawn(newPawn);
            }
        }

        private static void NightvisionArenaFight(SkillDef skill, AorB[] sides, GlowTeam lhsTeam, int lhsCount, GlowTeam rhsTeam, int rhsCount, Action<ArenaUtility.ArenaResult> callback)
        {
            MapParent mapParent = (MapParent)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Debug_Arena);
            mapParent.Tile = TileFinder.RandomSettlementTileFor(Faction.OfPlayer, true, (int tile) => Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(tile, ThingDef.Named("Human")));
            mapParent.SetFaction(Faction.OfPlayer);
            Find.WorldObjects.Add(mapParent);
            Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(mapParent.Tile, new IntVec3(50, 1, 50), null);
            IntVec3 spot;
            IntVec3 spot2;
            MultipleCaravansCellFinder.FindStartingCellsFor2Groups(orGenerateMap, out spot, out spot2);
            List<Pawn> lhs2 = NVPawnSetSpawner(orGenerateMap, skill, lhsTeam, lhsCount, spot, sides[0]);
            List<Pawn> rhs2 = NVPawnSetSpawner(orGenerateMap, skill, rhsTeam, rhsCount, spot2, sides[1]);
            DebugArena component = mapParent.GetComponent<DebugArena>();
            component.lhs = lhs2;
            component.rhs = rhs2;
            component.callback = callback;

        }

        public static float[] GlowLevels = {0, 0.1f, 0.2f, 0.3f, 0.4f};
        public static string[] GlowHediffs = {"", "10Pct","20Pct", "30Pct", "40Pct"};
        public static int[] SkillLevels = {7, 14, 20};

        public class GlowTeam
        {
            public HediffDef GlowHediff;
            private string name;

            public string Name
            {
                get
                {
                    if (name.NullOrEmpty())
                    {
                        name = $"{GlowHediff?.defName ?? "No glow"} & Skill: {Skill}";
                    }

                    return name;
                }
            }
            public int Skill;

            public GlowTeam(string glowL, int skill, float rating)
            {
                if (glowL.NullOrEmpty())
                {
                    GlowHediff = null;
                }
                else
                {
                    GlowHediff = HediffDef.Named(glowL);
                }
                Skill = skill;
                InitialRating = (int)rating;
            }

            public int InitialRating;


        }

        public static void PerformBattleRoyale(SkillDef skill)
		{
			if (ArenaUtility.ValidateArenaCapability())
			{
				
				Dictionary<GlowTeam, float> ratings = new Dictionary<GlowTeam, float>();

			    for (int i = 0; i < GlowHediffs.Length; i++)
			    {
			        foreach (int skillLevel in SkillLevels)
			        {
			            var initialR = skillLevel * 10 * (1 + GlowLevels[i]);
			            var newGT = new GlowTeam(GlowHediffs[i], skillLevel, initialR);
                        
			            ratings[newGT] = EloUtility.CalculateRating(initialR, 1500f, 60f);
			        }
			    }

			    List<GlowTeam> Teams = ratings.Keys.ToList();
			    AorB[] sides;
			    if (skill == Defs_Rimworld.MeleeSkill)
			    {
			        sides = ABMelee;
			    }
			    else
			    {
			        sides = ABShoot;
			    }

				int currentFights = 0;
				int completeFights = 0;
				Current.Game.GetComponent<GameComponent_DebugTools>().AddPerFrameCallback(delegate
				{
					bool result2;
					if (currentFights >= 15)
					{
						result2 = false;
					}
					else
					{
					    GlowTeam lhsTeam = Teams.RandomElement();
					    GlowTeam rhsTeam = Teams.RandomElement();
						float num = EloUtility.CalculateExpectation(ratings[lhsTeam], ratings[rhsTeam]);
						float num2 = 1f - num;
						float num3 = num;
						float num4 = Mathf.Min(num2, num3);
						num2 /= num4;
						num3 /= num4;
						float num5 = Mathf.Max(num2, num3);
						if (num5 > 40f)
						{
							result2 = false;
						}
						else
						{
							float num6 = 40f / num5;
							float num7 = (float)Math.Exp((double)Rand.Range(0f, (float)Math.Log((double)num6)));
							num2 *= num7;
							num3 *= num7;
						    int lhsNum = GenMath.RoundRandom(num2);
						    int rhsNum = GenMath.RoundRandom(num3);
							currentFights++;
							NightvisionArenaFight(skill, sides, lhsTeam, lhsNum, rhsTeam, rhsNum, delegate(ArenaUtility.ArenaResult result)
							{
								currentFights--;
								completeFights++;
								if (result.winner != ArenaUtility.ArenaResult.Winner.Other)
								{
									float value = ratings[lhsTeam];
									float value2 = ratings[rhsTeam];
									float kfactor = 8f * Mathf.Pow(0.5f, Time.realtimeSinceStartup / 900f);
									EloUtility.Update(ref value, ref value2, 0.5f, (float)((result.winner != ArenaUtility.ArenaResult.Winner.Lhs) ? 0 : 1), kfactor);
									ratings[lhsTeam] = value;
									ratings[rhsTeam] = value2;
									Log.Message(string.Format("Scores after {0} trials:\n\n{1}", completeFights, (from v in ratings
									orderby v.Value
									select string.Format("  {0}: {1}->{2} (rating {2})", new object[]
									{
										v.Key.Name,
										v.Key.InitialRating,
										EloUtility.CalculateLinearScore(v.Value, 1500f, 60f).ToString("F0"),
										v.Value.ToString("F0")
									})).ToLineList("")), false);
								}
							});
							result2 = false;
						}
					}
					return result2;
				});
			}
		}

        private static List<Pawn> NVPawnSetSpawner(Map map, SkillDef skill, GlowTeam team, int count, IntVec3 spot, AorB side)
        {
            List<Pawn> list = new List<Pawn>();
            Faction faction = FactionUtility.DefaultFactionFrom(side.fac);

            for (int i = 0; i < count; i++)
            {
                var     pawngen = new PawnGenerationRequest(side.pkd, faction, PawnGenerationContext.NonPlayer, mustBeCapableOfViolence: true, forceGenerateNewPawn: true, worldPawnFactionDoesntMatter: true);
                Pawn    newPawn = PawnGenerator.GeneratePawn(pawngen);
                newPawn.skills.GetSkill(skill).Level = team.Skill;
            
                newPawn.story.traits.allTraits.RemoveAll(trt => side.undesired.Contains(trt.def.defName));

                if (team.GlowHediff != null)
                {
                    newPawn.health.AddHediff(team.GlowHediff, newPawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ConsciousnessSource).First());
                }

                IntVec3 loc  = CellFinder.RandomClosewalkCellNear(spot, map, 12, null);
                GenSpawn.Spawn(newPawn, loc, map, Rot4.Random, WipeMode.Vanish, false);
                list.Add(newPawn);
            }
            LordMaker.MakeNewLord(faction, new LordJob_DefendPoint(map.Center), map, list);
            return list;
        }
    }
}

