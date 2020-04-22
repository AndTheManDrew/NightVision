﻿using System.Linq;
using System.Text;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace NightVision
{
    [UsedImplicitly]
    public class NVStatWorker_NightVision : NVStatWorker
    {
        public NVStatWorker_NightVision()
        {
            Glow = 0f;
            RelevantField = AccessTools.Field(typeof(ApparelVisionSetting), nameof(ApparelVisionSetting.GrantsNV));
            DefaultStatValue = Constants.DEFAULT_ZERO_LIGHT_MULTIPLIER;
            Acronym = Str.NV;
        }
    }
}