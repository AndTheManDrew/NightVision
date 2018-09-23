using System.Linq;
using System.Text;
using Harmony;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace NightVision
{
    [UsedImplicitly]
    public class StatWorker_LightSensitivity : StatWorker_NightVisionBase
    {
        public StatWorker_LightSensitivity()
        {
            Glow = 1f;
            RelevantField = AccessTools.Field(typeof(ApparelVisionSetting), nameof(ApparelVisionSetting.NullifiesPS));
            DefaultStatValue = Constants.DefaultFullLightMultiplier;
        }
    }
}