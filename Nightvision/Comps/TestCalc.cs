using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NightVision.Comps
{
        using Verse;

        class TestCalc
        {
            private int NumEyes = 4;

            private float EyeFactor;

            private List<Hediff> hediffs;

            private Dictionary<LightModifiers, int> lightModCounts;

            private Race_LightModifiers raceLightModifiers;

            private Dictionary<HediffDef, Hediff_LightModifiers> hediffLightModifiers;

            private float hediffFullLight()
                {
                    foreach (var hediff in hediffs)
                        {
                            if (hediffLightModifiers.TryGetValue(hediff.def, out Hediff_LightModifiers hlm))
                                {
                                    lightModCounts.TryGetValue(hlm, out int value);
                                    value += hlm.AffectsEye ? 1 / NumEyes : 1;
                                    lightModCounts[hlm] = value;
                                }
                        }
                }


        }
}
