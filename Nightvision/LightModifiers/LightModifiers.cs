using RimWorld;
using Verse;
using System;
using NightVision.Comps;

namespace NightVision
    {

        public class LightModifiers : IExposable, INVSaveCheck
            {
                internal static readonly float[] NVDefaultOffsets = new[] {0.2f, 0f};
                internal static readonly float[] PSDefaultOffsets = new[] {0.4f, -0.2f};

                private Def parentDef;

                public virtual Def ParentDef => parentDef;

                public bool Initialised;
                

                public enum Options : byte
                    {
                        NVNone = 0,

                        NVNightVision = 1,

                        NVPhotosensitivity = 2,

                        NVCustom = 3
                    }

                public static LightModifiers NVLightModifiers =
                            new LightModifiers() {offsets = new []{NVDefaultOffsets[0], NVDefaultOffsets[1]}, Initialised = true};

                public static LightModifiers PSLightModifiers =
                            new LightModifiers() {offsets = new []{PSDefaultOffsets[0], PSDefaultOffsets[1]}, Initialised = true};

                internal float[] offsets = new float[2];
        
                public virtual float this[int index]
                {
                    get { return offsets[index]; }
                    set { offsets[index] = value; }
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
                /// Save and load: in this class only used for NV and PS settings
                /// </summary>
                public virtual void ExposeData()
                    {
                        Scribe_Values.Look(ref offsets[0], "ZeroOffset");
                        Scribe_Values.Look(ref offsets[1], "FullOffset");
                    }   

                public virtual bool ShouldBeSaved()
                    {
                        return true;
                    }

            }
    }