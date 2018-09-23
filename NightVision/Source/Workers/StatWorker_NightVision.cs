using System.Linq;
using System.Text;
using Harmony;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace NightVision
{
    [UsedImplicitly]
    public class StatWorker_NightVision : StatWorker_NightVisionBase
    {
        public StatWorker_NightVision()
        {
            Glow = 0f;
            RelevantField = AccessTools.Field(typeof(ApparelVisionSetting), nameof(ApparelVisionSetting.GrantsNV));
            DefaultStatValue = Constants.DefaultZeroLightMultiplier;
        }
    }
}