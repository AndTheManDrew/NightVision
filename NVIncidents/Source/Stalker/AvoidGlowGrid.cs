using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace NVIncidents.Stalker
{
    public class AvoidGlow
        {
            public byte[] AvoidGlowGrid;

            public static void InitAvoidGlowGrid(
                Pawn pawn)
                {
                    GlowGrid localGlowGrid = pawn?.Map?.glowGrid;
                    if (localGlowGrid == null)
                        {
                            return;
                        }

                    
                }
        }
}
