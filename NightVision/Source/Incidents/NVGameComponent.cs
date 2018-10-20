// Nightvision NightVision NVGameComponent.cs
// 
// 19 10 2018
// 
// 19 10 2018

using Verse;

namespace NightVision
{
    public class NVGameComponent : GameComponent
    {
        public static NVGameComponent Instance;

        public NVGameComponent(Game game)
        {
            Instance = this;
        }
        
        public SolarRaid_StoryWorker SolarRaidStoryWorker;

        #region Overrides of GameComponent

        public override void GameComponentTick()
        {
            if (!FlareRaidIsDisabled && SolarRaidStoryWorker == null)
            {
                SolarRaidStoryWorker = new SolarRaid_StoryWorker();
            }
            else if (FlareRaidIsDisabled && SolarRaidStoryWorker != null)
            {
                SolarRaidStoryWorker = null;
            }
            SolarRaidStoryWorker?.TickCheckForFlareRaid();
        }


        public override void ExposeData()
        {
            base.ExposeData();
            if (!FlareRaidIsDisabled && Scribe.mode == LoadSaveMode.Saving)
            {
                SolarRaidStoryWorker?.ExposeData();
            }
            else if (!FlareRaidIsDisabled && Scribe.mode == LoadSaveMode.LoadingVars)
            {
                SolarRaidStoryWorker = new SolarRaid_StoryWorker();
                SolarRaidStoryWorker.ExposeData();
            }
        }
        
        public override void FinalizeInit()
        {
            if (!FlareRaidIsDisabled)
            {
                SolarRaidStoryWorker = new SolarRaid_StoryWorker();
            }
            base.FinalizeInit();
        }



        #endregion

        [TweakValue(category: "0 NV Incidents")]
        public static int Evilness = 5;

        [TweakValue(category:"0 NV Incidents")]
        public static bool FlareRaidIsDisabled;
    }
}