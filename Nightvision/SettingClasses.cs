using JetBrains.Annotations;
using NightVision.Comps;
using Verse;

namespace NightVision
    {
        public interface INVSaveCheck
            {
                bool ShouldBeSaved();
            }

        /// <summary>
        ///     For storing night vision settings for items of apparel
        /// </summary>
        public class ApparelSetting : IExposable, INVSaveCheck
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

                #endregion

                #region Constructors And comp attacher

                /// <summary>
                ///     Parameterless Constructor: necessary for RW scribe system
                /// </summary>
                [UsedImplicitly]
                public ApparelSetting() { }

                /// <summary>
                ///     Manual Constructor: for when user instantiates new setting in mod settings
                /// </summary>
                internal ApparelSetting(
                    bool nullPS,
                    bool giveNv)
                    {
                        NullifiesPS = nullPS;
                        GrantsNV    = giveNv;
                    }

                /// <summary>
                ///     Constructor for Dictionary builder
                /// </summary>
                internal ApparelSetting(
                    CompProperties_NightVisionApparel compprops)
                    {
                        CompNullifiesPS = compprops.NullifiesPhotosensitivity;
                        CompGrantsNV    = compprops.GrantsNightVision;
                        NullifiesPS     = CompNullifiesPS;
                        GrantsNV        = CompGrantsNV;
                    }

                /// <summary>
                ///     Dictionary builder attaches the comp settings to preexisting entries
                /// </summary>
                internal void AttachComp(
                    CompProperties_NightVisionApparel compprops)
                    {
                        CompNullifiesPS = compprops.NullifiesPhotosensitivity;
                        CompGrantsNV    = compprops.GrantsNightVision;
                    }

                #endregion

                #region Equality, Redundancy, and INVSaveCheck checks

                internal bool Equals(
                    ApparelSetting other) =>
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