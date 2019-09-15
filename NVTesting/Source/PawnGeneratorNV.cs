// Nightvision NVTesting PawnGeneratorNV.cs
// 
// 25 10 2018
// 
// 06 12 2018

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

using Mod = NightVision.Mod;

namespace NVTesting
{
    [HarmonyPatch(declaringType: typeof(Dialog_DebugActionsMenu), methodName: "DoListingItems_AllModePlayActions")]
    public static class PawnGeneratorNV
    {
        #region  Fields

        public static string[] GlowHediffs = {"", "10Pct", "20Pct", "30Pct", "40Pct"};

        public static float[]      GlowLevels           = {0, 0.1f, 0.2f, 0.3f, 0.4f};
        public static int[]        SkillLevels          = {7, 14, 20};
        public static List<string> UndesiredTraitsMelee = new List<string> {"Wimp", "Tough", "Brawler", "Nimble"};
        public static List<string> UndesiredTraitsShoot = new List<string> {"Wimp", "Tough", "ShootingAccuracy"};

        private static readonly AorB A_Melee = new AorB
                                               {
                                                   fac       = FactionDef.Named(defName: "A"),
                                                   pkd       = PawnKindDef.Named(defName: "A_NV"),
                                                   label     = "A",
                                                   undesired = UndesiredTraitsMelee
                                               };

        private static readonly AorB A_Shoot = new AorB
                                               {
                                                   fac       = FactionDef.Named(defName: "A"),
                                                   pkd       = PawnKindDef.Named(defName: "AS_NV"),
                                                   label     = "A",
                                                   undesired = UndesiredTraitsShoot
                                               };

        private static readonly AorB B_Melee = new AorB
                                               {
                                                   fac       = FactionDef.Named(defName: "B"),
                                                   pkd       = PawnKindDef.Named(defName: "B_NV"),
                                                   label     = "B",
                                                   undesired = UndesiredTraitsMelee
                                               };

        private static readonly AorB[] ABMelee = {A_Melee, B_Melee};

        private static readonly AorB B_Shoot = new AorB
                                               {
                                                   fac       = FactionDef.Named(defName: "B"),
                                                   pkd       = PawnKindDef.Named(defName: "BS_NV"),
                                                   label     = "B",
                                                   undesired = UndesiredTraitsShoot
                                               };

        private static readonly AorB[] ABShoot = {A_Shoot, B_Shoot};

        #endregion

        #region  Members

        [HarmonyPostfix]
        public static void DebugActionPostfix(Dialog_DebugActionsMenu __instance)
        {
            Traverse menuTraverse = Traverse.Create(root: __instance);
            menuTraverse.Method(name: "DoGap").GetValue();
            menuTraverse.Method("DoLabel", "NightVision Testing: ").GetValue();
            menuTraverse.Method("DoLabel", "   Pawn Spawners: ").GetValue();

            menuTraverse.Method(
                "DebugAction",
                "T: AB Shoot",
                (Action) delegate
                {
                    var list3 = new List<DebugMenuOption>();

                    for (var i = 0; i < 4; i++)
                    {
                        int skill = Math.Min(val1: i * 7, val2: 20);

                        list3.Add(
                            item: new DebugMenuOption(
                                label: "ShootSkill: " + skill,
                                mode: DebugMenuOptionMode.Action,
                                method: delegate
                                {
                                    var listAB = new List<DebugMenuOption>();

                                    foreach (AorB ab in ABShoot)
                                    {
                                        listAB.Add(
                                            item: new DebugMenuOption(
                                                label: ab.label,
                                                mode: DebugMenuOptionMode.Action,
                                                method: delegate
                                                {
                                                    var list4 = new List<DebugMenuOption>();

                                                    list4.Add(
                                                        item: new DebugMenuOption(
                                                            label: "None",
                                                            mode: DebugMenuOptionMode.Tool,
                                                            method: delegate
                                                            {
                                                                SpawnPawn(
                                                                    skill: skill,
                                                                    ab: ab,
                                                                    hediffs: null,
                                                                    AffectsEye: false,
                                                                    skilldef: Defs_Rimworld.ShootSkill
                                                                );
                                                            }
                                                        )
                                                    );

                                                    foreach (KeyValuePair<HediffDef, Hediff_LightModifiers> hediffDef in Mod.Store.HediffLightMods)
                                                    {
                                                        HediffDef             nvhediff   = hediffDef.Key;
                                                        Hediff_LightModifiers hediffMods = hediffDef.Value;

                                                        list4.Add(
                                                            item: new DebugMenuOption(
                                                                label: nvhediff.defName,
                                                                mode: DebugMenuOptionMode.Tool,
                                                                method: delegate
                                                                {
                                                                    SpawnPawn(
                                                                        skill: skill,
                                                                        ab: ab,
                                                                        hediffs: nvhediff,
                                                                        AffectsEye: hediffMods.AffectsEye,
                                                                        skilldef: Defs_Rimworld.ShootSkill
                                                                    );
                                                                }
                                                            )
                                                        );
                                                    }

                                                    Find.WindowStack.Add(window: new Dialog_DebugOptionListLister(options: list4));
                                                }
                                            )
                                        );
                                    }

                                    Find.WindowStack.Add(window: new Dialog_DebugOptionListLister(options: listAB));
                                }
                            )
                        );
                    }

                    Find.WindowStack.Add(window: new Dialog_DebugOptionListLister(options: list3));
                }
            ).GetValue();

            menuTraverse.Method(
                "DebugAction",
                "T: AB Melee",
                (Action) delegate
                {
                    var list3 = new List<DebugMenuOption>();

                    for (var i = 0; i < 4; i++)
                    {
                        int skill = Math.Min(val1: i * 7, val2: 20);

                        list3.Add(
                            item: new DebugMenuOption(
                                label: "Melee: " + skill,
                                mode: DebugMenuOptionMode.Action,
                                method: delegate
                                {
                                    var listAB = new List<DebugMenuOption>();

                                    foreach (AorB ab in ABMelee)
                                    {
                                        listAB.Add(
                                            item: new DebugMenuOption(
                                                label: ab.label,
                                                mode: DebugMenuOptionMode.Action,
                                                method: delegate
                                                {
                                                    var list4 = new List<DebugMenuOption>();

                                                    list4.Add(
                                                        item: new DebugMenuOption(
                                                            label: "None",
                                                            mode: DebugMenuOptionMode.Tool,
                                                            method: delegate
                                                            {
                                                                SpawnPawn(
                                                                    skill: skill,
                                                                    ab: ab,
                                                                    hediffs: null,
                                                                    AffectsEye: false,
                                                                    skilldef: Defs_Rimworld.MeleeSkill
                                                                );
                                                            }
                                                        )
                                                    );

                                                    foreach (KeyValuePair<HediffDef, Hediff_LightModifiers> hediffDef in Mod.Store.HediffLightMods)
                                                    {
                                                        HediffDef             nvhediff   = hediffDef.Key;
                                                        Hediff_LightModifiers hediffMods = hediffDef.Value;

                                                        list4.Add(
                                                            item: new DebugMenuOption(
                                                                label: nvhediff.defName,
                                                                mode: DebugMenuOptionMode.Tool,
                                                                method: delegate
                                                                {
                                                                    SpawnPawn(
                                                                        skill: skill,
                                                                        ab: ab,
                                                                        hediffs: nvhediff,
                                                                        AffectsEye: hediffMods.AffectsEye,
                                                                        skilldef: Defs_Rimworld.MeleeSkill
                                                                    );
                                                                }
                                                            )
                                                        );
                                                    }

                                                    Find.WindowStack.Add(window: new Dialog_DebugOptionListLister(options: list4));
                                                }
                                            )
                                        );
                                    }

                                    Find.WindowStack.Add(window: new Dialog_DebugOptionListLister(options: listAB));
                                }
                            )
                        );
                    }

                    Find.WindowStack.Add(window: new Dialog_DebugOptionListLister(options: list3));
                }
            ).GetValue();

            menuTraverse.Method("DoLabel", "    Battle Royales ").GetValue();

            menuTraverse.Method(
                "DebugAction",
                "NightVision: Battle Royale Melee",
                (Action) delegate
                {
                    PerformBattleRoyale(skill: Defs_Rimworld.MeleeSkill);
                }
            ).GetValue();

            menuTraverse.Method(
                "DebugAction",
                "NightVision: Battle Royale ShootSkill",
                (Action) delegate
                {
                    PerformBattleRoyale(skill: Defs_Rimworld.ShootSkill);
                }
            ).GetValue();
        }

        public static void PerformBattleRoyale(SkillDef skill)
        {
            if (ArenaUtility.ValidateArenaCapability())
            {
                var ratings = new Dictionary<GlowTeam, float>();

                for (var i = 0; i < GlowHediffs.Length; i++)
                {
                    foreach (int skillLevel in SkillLevels)
                    {
                        float initialR = skillLevel * 10 * (1 + GlowLevels[i]);
                        var   newGT    = new GlowTeam(glowL: GlowHediffs[i], skill: skillLevel, rating: initialR);

                        ratings[key: newGT] = EloUtility.CalculateRating(teamScore: initialR, referenceRating: 1500f, referenceScore: 60f);
                    }
                }

                List<GlowTeam> Teams = ratings.Keys.ToList();
                AorB[]         sides;

                if (skill == Defs_Rimworld.MeleeSkill)
                {
                    sides = ABMelee;
                }
                else
                {
                    sides = ABShoot;
                }

                var currentFights  = 0;
                var completeFights = 0;

                Current.Game.GetComponent<GameComponent_DebugTools>().AddPerFrameCallback(
                    callback: delegate
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
                            float    num     = EloUtility.CalculateExpectation(teamA: ratings[key: lhsTeam], teamB: ratings[key: rhsTeam]);
                            float    num2    = 1f - num;
                            float    num3    = num;
                            float    num4    = Mathf.Min(a: num2, b: num3);
                            num2 /= num4;
                            num3 /= num4;
                            float num5 = Mathf.Max(a: num2, b: num3);

                            if (num5 > 40f)
                            {
                                result2 = false;
                            }
                            else
                            {
                                float num6 = 40f / num5;
                                var   num7 = (float) Math.Exp(d: Rand.Range(min: 0f, max: (float) Math.Log(d: num6)));
                                num2 *= num7;
                                num3 *= num7;
                                int lhsNum = GenMath.RoundRandom(f: num2);
                                int rhsNum = GenMath.RoundRandom(f: num3);
                                currentFights++;

                                NightvisionArenaFight(
                                    skill: skill,
                                    sides: sides,
                                    lhsTeam: lhsTeam,
                                    lhsCount: lhsNum,
                                    rhsTeam: rhsTeam,
                                    rhsCount: rhsNum,
                                    callback: delegate(ArenaUtility.ArenaResult result)
                                    {
                                        currentFights--;
                                        completeFights++;

                                        if (result.winner != ArenaUtility.ArenaResult.Winner.Other)
                                        {
                                            float value   = ratings[key: lhsTeam];
                                            float value2  = ratings[key: rhsTeam];
                                            float kfactor = 8f * Mathf.Pow(f: 0.5f, p: Time.realtimeSinceStartup / 900f);

                                            EloUtility.Update(
                                                teamA: ref value,
                                                teamB: ref value2,
                                                expectedA: 0.5f,
                                                scoreA: result.winner != ArenaUtility.ArenaResult.Winner.Lhs ? 0 : 1,
                                                kfactor: kfactor
                                            );

                                            ratings[key: lhsTeam] = value;
                                            ratings[key: rhsTeam] = value2;

                                            Log.Message(
                                                text: string.Format(
                                                    "Scores after {0} trials:\n\n{1}",
                                                    completeFights,
                                                    (
                                                        from v in ratings orderby v.Value select string.Format(
                                                            "  {0}: {1}->{2} (rating {2})",
                                                            v.Key.Name,
                                                            v.Key.InitialRating,
                                                            EloUtility.CalculateLinearScore(
                                                                teamRating: v.Value,
                                                                referenceRating: 1500f,
                                                                referenceScore: 60f
                                                            ).ToString(format: "F0"),
                                                            v.Value.ToString(format: "F0")
                                                        )).ToLineList(prefix: "")
                                                ),
                                                ignoreStopLoggingLimit: false
                                            );
                                        }
                                    }
                                );

                                result2 = false;
                            }
                        }

                        return result2;
                    }
                );
            }
        }

        private static void NightvisionArenaFight
        (
            SkillDef skill,    AorB[]                           sides, GlowTeam lhsTeam, int lhsCount, GlowTeam rhsTeam,
            int      rhsCount, Action<ArenaUtility.ArenaResult> callback
        )
        {
            var mapParent = (MapParent) WorldObjectMaker.MakeWorldObject(def: WorldObjectDefOf.Debug_Arena);

            mapParent.Tile = TileFinder.RandomSettlementTileFor(
                faction: Faction.OfPlayer,
                mustBeAutoChoosable: true,
                extraValidator: tile
                            => Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(
                                tile: tile,
                                animalRace: ThingDef.Named(defName: "Human")
                            )
            );

            mapParent.SetFaction(newFaction: Faction.OfPlayer);
            Find.WorldObjects.Add(o: mapParent);

            Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(
                tile: mapParent.Tile,
                size: new IntVec3(newX: 50, newY: 1, newZ: 50),
                suggestedMapParentDef: null
            );

            IntVec3 spot;
            IntVec3 spot2;
            MultipleCaravansCellFinder.FindStartingCellsFor2Groups(map: orGenerateMap, first: out spot, second: out spot2);
            List<Pawn> lhs2      = NVPawnSetSpawner(map: orGenerateMap, skill: skill, team: lhsTeam, count: lhsCount, spot: spot,  side: sides[0]);
            List<Pawn> rhs2      = NVPawnSetSpawner(map: orGenerateMap, skill: skill, team: rhsTeam, count: rhsCount, spot: spot2, side: sides[1]);
            var        component = mapParent.GetComponent<DebugArena>();
            component.lhs      = lhs2;
            component.rhs      = rhs2;
            component.callback = callback;
        }

        private static List<Pawn> NVPawnSetSpawner
        (
            Map  map, SkillDef skill, GlowTeam team, int count, IntVec3 spot,
            AorB side
        )
        {
            var     list    = new List<Pawn>();
            Faction faction = FactionUtility.DefaultFactionFrom(ft: side.fac);

            for (var i = 0; i < count; i++)
            {
                var pawngen = new PawnGenerationRequest(
                    kind: side.pkd,
                    faction: faction,
                    context: PawnGenerationContext.NonPlayer,
                    mustBeCapableOfViolence: true,
                    forceGenerateNewPawn: true,
                    worldPawnFactionDoesntMatter: true
                );

                Pawn newPawn = PawnGenerator.GeneratePawn(request: pawngen);
                newPawn.skills.GetSkill(skillDef: skill).Level = team.Skill;

                newPawn.story.traits.allTraits.RemoveAll(match: trt => side.undesired.Contains(item: trt.def.defName));

                if (team.GlowHediff != null)
                {
                    newPawn.health.AddHediff(
                        def: team.GlowHediff,
                        part: newPawn.RaceProps.body.GetPartsWithTag(tag: BodyPartTagDefOf.ConsciousnessSource).First()
                    );
                }

                IntVec3 loc = CellFinder.RandomClosewalkCellNear(root: spot, map: map, radius: 12, extraValidator: null);
                GenSpawn.Spawn(newThing: newPawn, loc: loc, map: map, rot: Rot4.Random, wipeMode: WipeMode.Vanish, respawningAfterLoad: false);
                list.Add(item: newPawn);
            }

            LordMaker.MakeNewLord(faction: faction, lordJob: new LordJob_DefendPoint(point: map.Center), map: map, startingPawns: list);

            return list;
        }


        private static void SpawnPawn(int skill, AorB ab, HediffDef hediffs, bool AffectsEye, SkillDef skilldef)
        {
            Faction faction = FactionUtility.DefaultFactionFrom(ft: ab.fac);

            var pawngen = new PawnGenerationRequest(
                kind: ab.pkd,
                faction: faction,
                context: PawnGenerationContext.NonPlayer,
                mustBeCapableOfViolence: true,
                forceGenerateNewPawn: true,
                worldPawnFactionDoesntMatter: true
            );

            Pawn newPawn = PawnGenerator.GeneratePawn(request: pawngen);
            newPawn.skills.GetSkill(skillDef: skilldef).Level = skill;

            newPawn.story.traits.allTraits.RemoveAll(match: trt => ab.undesired.Contains(item: trt.def.defName));

            if (hediffs != null)
            {
                if (AffectsEye)
                {
                    foreach (BodyPartRecord bpr in newPawn.RaceProps.body.GetPartsWithTag(tag: Defs_Rimworld.EyeTag))
                    {
                        newPawn.health.AddHediff(def: hediffs, part: bpr, dinfo: null, result: null);
                    }
                }
                else
                {
                    newPawn.health.AddHediff(def: hediffs);
                }
            }

            GenSpawn.Spawn(newThing: newPawn, loc: UI.MouseCell(), map: Find.CurrentMap, wipeMode: WipeMode.Vanish);

            if (faction != null && faction != Faction.OfPlayer)
            {
                Lord lord = null;

                var lordJob = new LordJob_DefendPoint(point: newPawn.Position);
                lord = LordMaker.MakeNewLord(faction: faction, lordJob: lordJob, map: Find.CurrentMap, startingPawns: null);


                lord.AddPawn(p: newPawn);
            }
        }

        #endregion

        #region Nested type: AorB

        private class AorB
        {
            #region  Fields

            public FactionDef   fac;
            public string       label;
            public PawnKindDef  pkd;
            public List<string> undesired;

            #endregion
        }

        #endregion

        #region Nested type: GlowTeam

        public class GlowTeam
        {
            #region  Fields

            public HediffDef GlowHediff;

            public  int    InitialRating;
            public  int    Skill;
            private string name;

            #endregion

            #region  Constructors

            public GlowTeam(string glowL, int skill, float rating)
            {
                if (glowL.NullOrEmpty())
                {
                    GlowHediff = null;
                }
                else
                {
                    GlowHediff = HediffDef.Named(defName: glowL);
                }

                Skill         = skill;
                InitialRating = (int) rating;
            }

            #endregion

            #region  Properties And Indexers

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

            #endregion
        }

        #endregion
    }
}