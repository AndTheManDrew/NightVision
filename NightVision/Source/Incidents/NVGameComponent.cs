// Nightvision NightVision NVGameComponent.cs
// 
// 19 10 2018
// 
// 19 10 2018

using JetBrains.Annotations;
using Verse;

namespace NightVision
{
    [UsedImplicitly]
    public class NVGameComponent : GameComponent
    {
        [UsedImplicitly] public static NVGameComponent Instance;

        public NVGameComponent(Game game)
        {
            Instance = this;
        }

        public SolarRaid_StoryWorker SolarRaidStoryWorker;

        public override void GameComponentTick()
        {
            if (FlareRaidIsEnabled && SolarRaidStoryWorker == null)
            {
                SolarRaidStoryWorker = new SolarRaid_StoryWorker();
            }
            else if (!FlareRaidIsEnabled && SolarRaidStoryWorker != null)
            {
                SolarRaidStoryWorker = null;
            }
            SolarRaidStoryWorker?.TickCheckForFlareRaid();
        }


        public override void ExposeData()
        {
            base.ExposeData();
            if (FlareRaidIsEnabled && Scribe.mode == LoadSaveMode.Saving)
            {
                SolarRaidStoryWorker?.ExposeData();
            }
            else if (FlareRaidIsEnabled && Scribe.mode == LoadSaveMode.LoadingVars)
            {
                SolarRaidStoryWorker = new SolarRaid_StoryWorker();
                SolarRaidStoryWorker.ExposeData();
            }
        }

        public override void FinalizeInit()
        {
            if (FlareRaidIsEnabled)
            {
                SolarRaidStoryWorker = new SolarRaid_StoryWorker();
            }
            base.FinalizeInit();
        }


        public static int Evilness => Find.Storyteller.difficulty.difficulty * 2;

        public static bool FlareRaidIsEnabled = true;
    }
}