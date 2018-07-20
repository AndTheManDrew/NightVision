using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Verse;

namespace NightVision.LightModifiers
    {
        [SuppressMessage("ReSharper", "ValueParameterNotUsed")]
        public class LightModifiersBase : IExposable, INVSaveCheck
            {
                public enum Options : byte
                    {
                        NVNone = 0,

                        NVNightVision = 1,

                        NVPhotosensitivity = 2,

                        NVCustom = 3
                    }

                internal static readonly float[] NVDefaultOffsets = {0.2f, 0f};
                internal static readonly float[] PSDefaultOffsets = {0.4f, -0.2f};

                public static LightModifiersBase NVLightModifiers = new LightModifiersBase
                {
                    Offsets     = new[] {NVDefaultOffsets[0], NVDefaultOffsets[1]},
                    Initialised = true
                };

                public static LightModifiersBase PSLightModifiers = new LightModifiersBase
                {
                    Offsets     = new[] {PSDefaultOffsets[0], PSDefaultOffsets[1]},
                    Initialised = true
                };

                public bool Initialised;

                internal float[] Offsets = new float[2];


                public virtual Def ParentDef => null;

                internal virtual Options Setting
                    {
                        get => Options.NVNone;
                        set { }
                    }

                public virtual float this[
                    int index]
                    {
                        get => Offsets[index];
                        set => Offsets[index] = value;
                    }

                public virtual float[] DefaultOffsets
                    {
                        get
                            {
                                if (this == NVLightModifiers)
                                    {
                                        return NVDefaultOffsets;
                                    }

                                if (this == PSLightModifiers)
                                    {
                                        return PSDefaultOffsets;
                                    }

                                return new float[2];
                            }
                    }


                /// <summary>
                ///     Save and load: in this class only used for NV and PS settings
                /// </summary>
                public virtual void ExposeData()
                    {
                        Scribe_Values.Look(ref Offsets[0], "ZeroOffset");
                        Scribe_Values.Look(ref Offsets[1], "FullOffset");
                        if (Scribe.mode == LoadSaveMode.LoadingVars)
                            {
                                if (Offsets == null)
                                    {
                                        if (this == NVLightModifiers)
                                            {
                                                Offsets = NVDefaultOffsets.ToArray();
                                            }

                                        else if (this == PSLightModifiers)
                                            {
                                                Offsets = PSDefaultOffsets.ToArray();
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
                    int   numEyesNormalisedFor = 1)
                    {
                        if (glow < 0.001)
                            {
                                return (float) Math.Round(this[0] / numEyesNormalisedFor, 2);
                            }

                        if (glow > 0.999)
                            {
                                return (float) Math.Round(this[1] / numEyesNormalisedFor, 2);
                            }

                        if (glow < 0.3)
                            {
                                return (float) Math.Round(this[0] / numEyesNormalisedFor * (0.3f - glow) / 0.3f, 2);
                            }

                        if (glow > 0.7)
                            {
                                return (float) Math.Round(this[1] / numEyesNormalisedFor * (glow - 0.7f) / 0.3f, 2);
                            }

                        return 0;
                    }

                public static float[] GetCapsAtGlow(
                    float glow)
                    {
                        float mincap;
                        float maxcap;
                        float nvcap;
                        float pscap;
                        if (glow < 0.3f)
                            {
                                mincap = (NightVisionSettings.MultiplierCaps.min
                                          - NightVisionSettings.DefaultZeroLightMultiplier) * (0.3f - glow) / 0.3f;
                                maxcap = (NightVisionSettings.MultiplierCaps.max
                                          - NightVisionSettings.DefaultZeroLightMultiplier) * (0.3f - glow) / 0.3f;
                                pscap = PSLightModifiers[0] * (0.3f - glow) / 0.3f;
                                nvcap = NVLightModifiers[0] * (0.3f - glow) / 0.3f;
                            }
                        else
                            {
                                mincap = (NightVisionSettings.MultiplierCaps.min
                                          - NightVisionSettings.DefaultFullLightMultiplier) * (glow - 0.7f) / 0.3f;
                                maxcap = (NightVisionSettings.MultiplierCaps.max
                                          - NightVisionSettings.DefaultFullLightMultiplier) * (glow - 0.7f) / 0.3f;
                                pscap = PSLightModifiers[1] * (glow - 0.7f) / 0.3f;
                                nvcap = NVLightModifiers[1] * (glow - 0.7f) / 0.3f;
                            }

                        return new[] {maxcap, mincap, nvcap, pscap};
                    }

                public bool HasModifier() => Math.Abs(this[0]) > 0.001 && Math.Abs(this[1]) > 0.001;

                public bool IsCustom() => Setting == Options.NVCustom;

                internal void ChangeSetting(
                    Options newsetting)
                    {
                        if (Setting != newsetting)
                            {
                                if (newsetting == Options.NVCustom && !HasModifier())
                                    {
                                        float[] defaultValues = DefaultOffsets;
                                        if (Math.Abs(defaultValues[0]) > 0.001 && Math.Abs(defaultValues[1]) > 0.001)
                                            {
                                                Offsets = DefaultOffsets.ToArray();
                                            }
                                        else if (Setting == Options.NVNightVision)
                                            {
                                                Offsets = NVLightModifiers.Offsets.ToArray();
                                            }
                                        else if (Setting == Options.NVPhotosensitivity)
                                            {
                                                Offsets = PSLightModifiers.Offsets.ToArray();
                                            }
                                    }
                            }

                        Setting = newsetting;
                    }
            }
    }