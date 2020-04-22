using HarmonyLib;
using JetBrains.Annotations;

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