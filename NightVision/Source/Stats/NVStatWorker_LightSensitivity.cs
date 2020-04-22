using System.Linq;
using System.Text;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace NightVision
{
    [UsedImplicitly]
    public class NVStatWorker_LightSensitivity : NVStatWorker
    {
        public NVStatWorker_LightSensitivity()
        {
            Glow = 1f;
            RelevantField = AccessTools.Field(typeof(ApparelVisionSetting), nameof(ApparelVisionSetting.NullifiesPS));
            DefaultStatValue = Constants.DEFAULT_FULL_LIGHT_MULTIPLIER;
            Acronym = Str.PS;
        }
    }
}