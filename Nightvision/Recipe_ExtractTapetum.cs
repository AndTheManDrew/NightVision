using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace NightVision
{
        [DefOf]
        public static class RecipeDef_ExtractTapetumLucidum
            {
                public static RecipeDef ExtractTapetumLucidum;
            }
    class Recipe_ExtractTapetum : Recipe_Surgery
        {
        public static ThingDef ExtractedTapetum = ThingDef.Named("NV_TapetumRaw");
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (billDoer != null)
                {
                    if (base.CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                        {
                            return;
                        }
                    TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
                    {
                        billDoer,
                        pawn
                    });
                    GenSpawn.Spawn(ExtractedTapetum, billDoer.Position, billDoer.Map);
                }
            DamageDef surgicalCut      = DamageDefOf.SurgicalCut;
            float     amount           = 99999f;
            float     armorPenetration = 999f;
            pawn.TakeDamage(new DamageInfo(surgicalCut, amount, armorPenetration, -1f, null, part, null, DamageInfo.SourceCategory.ThingOrUnknown, null));

            if (pawn.Faction != null && billDoer?.Faction != null)
                {
                    Faction faction        = pawn.Faction;
                    Faction faction2       = billDoer.Faction;
                    int     goodwillChange = -15;
                    string reason =
                                "GoodwillChangedReason_RemovedBodyPart".Translate(new object[] {part.LabelShort});
                    GlobalTargetInfo? lookTarget = new GlobalTargetInfo?(pawn);
                    faction.TryAffectGoodwillWith(faction2, goodwillChange, true, true, reason, lookTarget);
                }
        }

        public override string GetLabelWhenUsedOn(Pawn pawn, BodyPartRecord part)
            {
                return "HarvestOrgan".Translate();
            }

        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(
            Pawn      pawn,
            RecipeDef recipe)
            {
                IEnumerable<BodyPartRecord> parts =
                            pawn.health.hediffSet.GetNotMissingParts(tag: BodyPartTagDefOf.SightSource);
                foreach (var part in parts.DefaultIfEmpty())
                    {
                        if (!pawn.health.hediffSet.HasDirectlyAddedPartFor(part) && MedicalRecipesUtility.IsClean(pawn, part))
                            {
                                yield return part;
                            }
                    }
            }
        
    }
}
