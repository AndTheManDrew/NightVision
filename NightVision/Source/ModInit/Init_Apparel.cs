﻿// Nightvision NightVision Init_Apparel.cs
// 
// 06 12 2018
// 
// 06 12 2018

using System.Collections.Generic;
using Verse;

namespace NightVision
{
    public static class Init_Apparel
    {
        #region  Members

        public static void FindAllEyeCoveringApparel()
        {
            ThingCategoryDef headgearCategoryDef = ThingCategoryDef.Named(defName: "Headgear");
            BodyPartGroupDef fullHead            = Defs_Rimworld.Head;
            BodyPartGroupDef eyes                = Defs_Rimworld.Eyes;

            Storage.AllEyeCoveringHeadgearDefs = new HashSet<ThingDef>(
                collection: DefDatabase<ThingDef>.AllDefsListForReading.FindAll(
                    match: adef => adef.IsApparel
                                   && ((adef.thingCategories?.Contains(item: headgearCategoryDef) ?? false)
                                       || adef.apparel.bodyPartGroups.Any(predicate: bpg => bpg == eyes || bpg == fullHead)
                                       || adef.HasComp(compType: typeof(Comp_NightVisionApparel)))
                )
            );

            if (Storage.NVApparel == null)
            {
                Storage.NVApparel = new Dictionary<ThingDef, ApparelVisionSetting>();
            }

            //Add defs that have NV comp
            foreach (ThingDef apparel in Storage.AllEyeCoveringHeadgearDefs)
            {
                if (apparel.comps.Find(match: comp => comp is CompProperties_NightVisionApparel) is CompProperties_NightVisionApparel)
                {
                    if (!Storage.NVApparel.TryGetValue(key: apparel, value: out ApparelVisionSetting setting))
                    {
                        Storage.NVApparel[key: apparel] = new ApparelVisionSetting(apparel: apparel);
                    }
                    else
                    {
                        setting.InitExistingSetting(apparel: apparel);
                    }
                }
                else
                {
                    //This attaches a new comp to the def as a placeholder
                    //Note these comps never get removed, nor do they get saved TODO review
                    ApparelVisionSetting.CreateNewApparelVisionSetting(apparel: apparel);
                }
            }
        }

        #endregion
    }
}