using System.Linq;
using System.Text;
using Harmony;
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
            DefaultStatValue = Constants_Calculations.DefaultZeroLightMultiplier;
            Acronym = Str.NV;
        }
    }
}