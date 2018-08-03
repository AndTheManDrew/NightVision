// Nightvision NightVision LightModifiers.cs
// 
// 16 05 2018
// 
// 21 07 2018

using System;
using System.Linq;
using JetBrains.Annotations;
using Verse;

namespace NightVision
{
    public class LightModifiersBase : IExposable, ISaveCheck
    {
        public static LightModifiersBase NVLightModifiers = new LightModifiersBase
                                                            {
                                                                Offsets = new[]
                                                                          {
                                                                              Constants.NVDefaultOffsets[0],
                                                                              Constants.NVDefaultOffsets[1]
                                                                          },
                                                                Initialised = true
                                                            };

        public static LightModifiersBase PSLightModifiers = new LightModifiersBase
                                                            {
                                                                Offsets = new[]
                                                                          {
                                                                              Constants.PSDefaultOffsets[0],
                                                                              Constants.PSDefaultOffsets[1]
                                                                          },
                                                                Initialised = true
                                                            };

        public bool Initialised;

        internal float[] Offsets = new float[2];

        public LightModifiersBase() { }

        [UsedImplicitly]
        public LightModifiersBase(
                        bool isPhotosensitiveLm,
                        bool isNightVisionLm
                    )
        {
            if (isPhotosensitiveLm)
            {
                LightModifiersBase.PSLightModifiers = this;
            }
            else if (isNightVisionLm)

            {
                LightModifiersBase.NVLightModifiers = this;
            }
        }

        public virtual float this[
                        int index
                    ]
        {
            get => Offsets[index];
            set => Offsets[index] = value;
        }


        public virtual Def ParentDef => null;

        internal virtual VisionType Setting
        {
            get => VisionType.NVNone;
            set { }
        }

        public virtual float[] DefaultOffsets
        {
            get
            {
                if (this == LightModifiersBase.NVLightModifiers)
                {
                    return Constants.NVDefaultOffsets;
                }

                if (this == LightModifiersBase.PSLightModifiers)
                {
                    return Constants.PSDefaultOffsets;
                }

                return new float[2];
            }
        }


        /// <summary>
        ///     Save and load: in this class only used for NV and PS settings
        /// </summary>
        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref Offsets[0], "ZeroOffset", forceSave: true);
            Scribe_Values.Look(ref Offsets[1], "FullOffset", forceSave: true);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (Offsets == null)
                {
                    if (this == LightModifiersBase.NVLightModifiers)
                    {
                        Offsets = Constants.NVDefaultOffsets.ToArray();
                    }

                    else if (this == LightModifiersBase.PSLightModifiers)
                    {
                        Offsets = Constants.PSDefaultOffsets.ToArray();
                    }
                    else
                    {
                        Offsets = new float[2];
                    }
                }

                Initialised = true;
            }
        }

        public virtual bool ShouldBeSaved() => true;

        public virtual float GetEffectAtGlow(
                        float glow,
                        int   numEyesNormalisedFor = 1
                    )
        {
            if (glow < 0.001)
            {
#if DEBUG
                                Log.Message("NightVision.LightModifiersBase.GetEffectAtGlow: " + this[0] + "/"
                                            + numEyesNormalisedFor + "= " + this[0] / numEyesNormalisedFor);

#endif

                return (float) Math.Round(
                                          this[0] / numEyesNormalisedFor,
                                          Constants.NumberOfDigits,
                                          Constants.Rounding
                                         );
            }

            if (glow > 0.999)
            {
                return (float) Math.Round(
                                          this[1] / numEyesNormalisedFor,
                                          Constants.NumberOfDigits,
                                          Constants.Rounding
                                         );
            }

            if (glow < 0.3)
            {
                return (float) Math.Round(
                                          this[0] / numEyesNormalisedFor * (0.3f - glow) / 0.3f,
                                          Constants.NumberOfDigits,
                                          Constants.Rounding
                                         );
            }

            if (glow > 0.7)
            {
                return (float) Math.Round(
                                          this[1] / numEyesNormalisedFor * (glow - 0.7f) / 0.3f,
                                          Constants.NumberOfDigits,
                                          Constants.Rounding
                                         );
            }

            return 0;
        }

        public static float[] GetCapsAtGlow(
                        float glow
                    )
        {
            float mincap;
            float maxcap;
            float nvcap;
            float pscap;

            if (glow < 0.3f)
            {
                mincap = (Storage.MultiplierCaps.min - Constants.DefaultZeroLightMultiplier)
                         * (0.3f                     - glow)
                         / 0.3f;

                maxcap = (Storage.MultiplierCaps.max - Constants.DefaultZeroLightMultiplier)
                         * (0.3f                     - glow)
                         / 0.3f;

                pscap = LightModifiersBase.PSLightModifiers[0] * (0.3f - glow) / 0.3f;
                nvcap = LightModifiersBase.NVLightModifiers[0] * (0.3f - glow) / 0.3f;
            }
            else
            {
                mincap = (Storage.MultiplierCaps.min - Constants.DefaultFullLightMultiplier)
                         * (glow                     - 0.7f)
                         / 0.3f;

                maxcap = (Storage.MultiplierCaps.max - Constants.DefaultFullLightMultiplier)
                         * (glow                     - 0.7f)
                         / 0.3f;

                pscap = LightModifiersBase.PSLightModifiers[1] * (glow - 0.7f) / 0.3f;
                nvcap = LightModifiersBase.NVLightModifiers[1] * (glow - 0.7f) / 0.3f;
            }
            
            return new[]
                   {
                       (float) Math.Round(maxcap, Constants.NumberOfDigits, Constants.Rounding)
                       ,
                        ((float) Math.Round(mincap, Constants.NumberOfDigits, Constants.Rounding))
                       ,
                        (float)Math.Round(nvcap, Constants.NumberOfDigits, Constants.Rounding)
                       , 
                       (float)Math.Round(pscap, Constants.NumberOfDigits, Constants.Rounding)
                       
                   };

        }

        //public bool HasAnyModifier() => Math.Abs(this[0]) > 0.001 && <- HAHAHAHA took me too long to find that Math.Abs(this[1]) > 0.001;
        public bool HasAnyModifier() => Math.Abs(this[0]) > 0.001 || Math.Abs(this[1]) > 0.001;

        public bool HasAnyCustomModifier() => Math.Abs(Offsets[0]) > 0.001 || Math.Abs(Offsets[1]) > 0.001;


        public bool IsCustom() => Setting == VisionType.NVCustom;

        internal void ChangeSetting(
                        VisionType newsetting
                    )
        {
            if (Setting != newsetting)
            {
                if (newsetting == VisionType.NVCustom && !HasAnyCustomModifier())
                {
                    float[] defaultValues = this.DefaultOffsets;

                    if (Math.Abs(defaultValues[0]) > 0.001 && Math.Abs(defaultValues[1]) > 0.001)
                    {
                        Offsets = DefaultOffsets.ToArray();
                    }
                    else if (Setting == VisionType.NVNightVision)
                    {
                        Offsets = LightModifiersBase.NVLightModifiers.Offsets.ToArray();
                    }
                    else if (Setting == VisionType.NVPhotosensitivity)
                    {
                        Offsets = LightModifiersBase.PSLightModifiers.Offsets.ToArray();
                    }
                }
            }

            Setting = newsetting;
        }
    }
}