using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace NightVision
    {
        [DefOf]
        public static class RecipeDef_ExtractTapetumLucidum
            {
                [UsedImplicitly] public static RecipeDef ExtractTapetumLucidum;
            }

        [UsedImplicitly]
        internal class Recipe_ExtractTapetum : Recipe_Surgery
            {
                [NotNull] private static readonly ThingDef ExtractedTapetum = ThingDef.Named("NV_TapetumRaw");

                public override void ApplyOnPawn(
                    Pawn           pawn,
                    BodyPartRecord part,
                    Pawn           billDoer,
                    List<Thing>    ingredients,
                    Bill           bill)
                    {
                        if (billDoer != null)
                            {
                                if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                                    {
                                        return;
                                    }

                                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
                                GenSpawn.Spawn(ExtractedTapetum, billDoer.Position, billDoer.Map);
                            }

                        DamageDef surgicalCut      = DamageDefOf.SurgicalCut;
                        var       amount           = 99999f;
                        var       armorPenetration = 999f;
                        pawn.TakeDamage(new DamageInfo(surgicalCut,
                            amount,
                            armorPenetration,
                            -1f,
                            null,
                            part,
                            null,
                            DamageInfo.SourceCategory.ThingOrUnknown,
                            null));

                        if (pawn.Faction != null && billDoer?.Faction != null)
                            {
                                Faction faction        = pawn.Faction;
                                Faction faction2       = billDoer.Faction;
                                int     goodwillChange = -15;
                                string reason =
                                            "GoodwillChangedReason_RemovedBodyPart".Translate(part.LabelShort);
                                Pawn lookTarget = pawn;
                                faction.TryAffectGoodwillWith(faction2, goodwillChange, true, true, reason, lookTarget);
                            }
                    }

                public override string GetLabelWhenUsedOn(
                    Pawn           pawn,
                    BodyPartRecord part) =>
                            "HarvestOrgan".Translate();

                public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(
                    Pawn      pawn,
                    RecipeDef recipe)
                    {
                        IEnumerable<BodyPartRecord> parts =
                                    pawn.health.hediffSet.GetNotMissingParts(tag: BodyPartTagDefOf.SightSource);
                        foreach (BodyPartRecord part in parts.DefaultIfEmpty())
                            {
                                if (!pawn.health.hediffSet.HasDirectlyAddedPartFor(part)
                                    && MedicalRecipesUtility.IsClean(pawn, part))
                                    {
                                        yield return part;
                                    }
                            }
                    }
            }
    }