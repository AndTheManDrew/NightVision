// Nightvision NightVision Constants.cs
// 
// 03 08 2018
// 
// 16 10 2018

using System;
using RimWorld;
using Verse;

namespace NightVision
{
    public static class Defs_Rimworld {
        public static readonly BodyPartTagDef EyeTag = BodyPartTagDefOf.SightSource;
        public static readonly SkillDef ShootSkill = SkillDefOf.Shooting;
        public static readonly BodyPartGroupDef Eyes = BodyPartGroupDefOf.Eyes;
        public static readonly BodyPartGroupDef Head = BodyPartGroupDefOf.FullHead;
        public static readonly StatDef MeleeHitStat = StatDefOf.MeleeHitChance;
        public static readonly StatDef MeleeDodgeStat = StatDefOf.MeleeDodgeChance;
        public static readonly StatCategoryDef BasicStats = StatCategoryDefOf.Basics;
        public static readonly SkillDef MeleeSkill = SkillDefOf.Melee;
        public static readonly GameConditionDef SolarFlare = GameConditionDefOf.SolarFlare;
        public static readonly ThingDef ShieldDef = ThingDef.Named("Apparel_ShieldBelt");
        public static readonly PawnGroupKindDef CombatGroup = PawnGroupKindDefOf.Combat;

        //fallback animals for tapetum lucidum injection
    }
    
}