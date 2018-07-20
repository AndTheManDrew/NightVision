using NightVision.LightModifiers;
using Verse;

namespace NightVision.Utilities
    {
        internal static class AutoQualifier
            {
                internal static LightModifiersBase.Options? HediffCheck(
                    HediffDef hediffDef)
                    {
                        if (hediffDef.addedPartProps is AddedBodyPartProps abpp && abpp.partEfficiency > 1.0f)
                            {
                                return LightModifiersBase.Options.NVNightVision;
                            }

                        return null;
                    }
            }
    }