using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace NightVision
{
    public class GlowMods : IExposable
    {
        protected float zeroLightMod;
        protected float fullLightMod;

        public GlowMods()
        {
            zeroLightMod = -1;
            fullLightMod = -1;
        }
        public GlowMods(FloatRange floatRange)
        {
            zeroLightMod = floatRange.min;
            fullLightMod = floatRange.max;
        }
        public GlowMods(CompProperties_NightVision compprops)
        {
            zeroLightMod = NightVisionSettings.DefaultModifiers.min;
            fullLightMod = NightVisionSettings.DefaultModifiers.max;

            if (compprops == null)
            {
                return;
            }
            if (compprops.naturalNightVision)
            {
                zeroLightMod = 0.2f;
            }
            if (compprops.fullLightMultiplier != NightVisionSettings.DefaultFullLightMultiplier
                || compprops.zeroLightMultplier != NightVisionSettings.DefaultZeroLightMultiplier)
            {
                zeroLightMod = compprops.zeroLightMultplier - NightVisionSettings.DefaultZeroLightMultiplier;
                fullLightMod = compprops.fullLightMultiplier - NightVisionSettings.DefaultFullLightMultiplier;
            }

        }
        public GlowMods(float min, float max)
        {
            zeroLightMod = min;
            fullLightMod = max;
        }

        public float ZeroLight
        {
            get
            {
                return zeroLightMod;
            }
            set
            {
                if (value < -0.90f)
                {
                    zeroLightMod = -0.90f;
                }
                else
                {
                    zeroLightMod = value;
                }
            }
        }
        public float FullLight
        {
            get
            {
                return fullLightMod;
            }
            set
            {
                if (value < -0.90f)
                {
                    fullLightMod = -0.90f;
                }
                else
                {
                    fullLightMod = value;
                }
            }
        }  
        public void ExposeData()
        {
            Scribe_Values.Look(ref zeroLightMod, "zerolight", NightVisionSettings.DefaultModifiers.min);
            Scribe_Values.Look(ref fullLightMod, "fulllight", NightVisionSettings.DefaultModifiers.max);
        }

        public bool IsZero()
        {
            return Math.Abs(zeroLightMod) < 0.001 && Math.Abs(fullLightMod) < 0.001;
        }
        public bool IsNotSet()
        {
            return zeroLightMod < -0.95 && fullLightMod < -0.95;
        }
        public void Reset()
        {
            zeroLightMod = -1;
            fullLightMod = -1;
        }

        public override bool Equals(object obj)
        {
            var mods = obj as GlowMods;
            return mods != null &&
                   Math.Abs(zeroLightMod - mods.zeroLightMod) < 0.001f &&
                   Math.Abs(fullLightMod - mods.fullLightMod) < 0.001f;
        }

        public override int GetHashCode()
        {
            var hashCode = 2099861367;
            hashCode = hashCode * -1521134295 + zeroLightMod.GetHashCode();
            hashCode = hashCode * -1521134295 + fullLightMod.GetHashCode();
            return hashCode;
        }
    }
    public class EyeGlowMods : GlowMods
    {
        public EyeGlowMods() : base(){}
        public EyeGlowMods(FloatRange floatRange) : base(floatRange){}
        public EyeGlowMods(FloatRange floatRange, float numberOfEyes)
        {
            zeroLightMod = (float)Math.Round((floatRange.min) / numberOfEyes, 2);
            fullLightMod = (float)Math.Round((floatRange.max) / numberOfEyes, 2);
            Log.Message("EyeGlowMods: ctor: " + zeroLightMod + " calced from " + floatRange.min + " and " + fullLightMod + " calced from " + floatRange.max);
        }
        public EyeGlowMods(float min, float max) : base(min, max){}




    }
}
