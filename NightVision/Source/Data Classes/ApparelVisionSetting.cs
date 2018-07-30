// Nightvision NightVision SettingClasses.cs
// 
// 06 04 2018
// 
// 21 07 2018

using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace NightVision
    {
        /// <summary>
        ///     For storing night vision settings for items of apparel
        /// </summary>
        public class ApparelVisionSetting : IExposable, ISaveCheck
            {
                public void ExposeData()
                    {
                        Scribe_Values.Look(ref NullifiesPS, "nullifiesphotosens", CompNullifiesPS);
                        Scribe_Values.Look(ref GrantsNV,    "grantsnightvis",     CompGrantsNV);
                    }

                #region Fields

                //Current Settings
                internal bool NullifiesPS;
                internal bool GrantsNV;

                //Settings in xml defs
                internal bool CompNullifiesPS;
                internal bool CompGrantsNV;

                //Corresponding ThingDef

                #endregion

                #region Constructors And comp attacher

                /// <summary>
                ///     Parameterless Constructor: necessary for RW scribe system
                /// </summary>
                [UsedImplicitly]
                public ApparelVisionSetting() { }

                public ThingDef ParentDef;
                public CompProperties_NightVisionApparel CompProps;

                /// <summary>
                ///     New Setting
                /// </summary>
                internal ApparelVisionSetting(
                    ThingDef apparel)
                    {
                        ParentDef = apparel;
                        AttachComp();
                        NullifiesPS     = CompNullifiesPS;
                        GrantsNV        = CompGrantsNV;
                    }

                private void AttachComp()
                    {
                        if (ParentDef == null)
                            {
                                Log.Message("NightVision.ApparelVisionSetting.AttachComp: Null Parentdef");
                                return;
                            }
                        if (ParentDef.GetCompProperties<CompProperties_NightVisionApparel>() is
                                    CompProperties_NightVisionApparel props)
                            {
                                CompNullifiesPS            = props.NullifiesPhotosensitivity;
                                CompGrantsNV               = props.GrantsNightVision;
                                props.AppVisionSetting = this;
                            }
                        else
                            {
                                if (ParentDef.comps.NullOrEmpty())
                                    {
                                        ParentDef.comps = new List<CompProperties>();
                                    }
                                CompProps = new CompProperties_NightVisionApparel(){AppVisionSetting = this};
                                ParentDef.comps.Add(CompProps);
                                CompNullifiesPS = false;
                                CompGrantsNV = false;
                            }
                    }
                /// <summary>
                ///     Dictionary builder attaches the comp settings to preexisting entries
                /// </summary>
                internal void InitExistingSetting(ThingDef Apparel)
                    {
                        ParentDef = Apparel;
                        AttachComp();
                    }

                public static ApparelVisionSetting CreateNewApparelVisionSetting(
                     ThingDef apparel)
                    {
                        var newAppSetting = new ApparelVisionSetting(){ParentDef = apparel};
                        newAppSetting.AttachComp();
                        if (newAppSetting.ParentDef != apparel)
                            {
                                Log.Message("NightVision.ApparelVisionSetting.CreateNewApparelVisionSetting: Failed to attach Comp, parentdef != given appareldef");
                                
                            }
                        return newAppSetting;

                    }
                #endregion

                #region Equality, Redundancy, and INVSaveCheck checks

                internal bool Equals(
                    ApparelVisionSetting other) =>
                            GrantsNV == other.GrantsNV && NullifiesPS == other.NullifiesPS;

                /// <summary>
                ///     Check to see if this setting should be removed from the dictionary, i.e. current and def values are all false
                /// </summary>
                internal bool IsRedundant() => !(GrantsNV || NullifiesPS) && !(CompGrantsNV || CompNullifiesPS);

                /// <summary>
                ///     Check to see if this setting should be saved, i.e. current and def values are all false,
                ///     or current values are equal to def values
                /// </summary>
                /// <returns></returns>
                public bool ShouldBeSaved() =>
                            !(IsRedundant() || GrantsNV == CompGrantsNV && NullifiesPS == CompNullifiesPS);

                #endregion
            }
    }