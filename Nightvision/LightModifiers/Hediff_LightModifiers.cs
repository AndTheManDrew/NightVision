using System;
using JetBrains.Annotations;
using NightVision.Comps;
using UnityEngine;

namespace NightVision
    {
        using Verse;

        public class Hediff_LightModifiers : LightModifiers
            {
                internal bool AffectsEye = false;

                internal Options Setting;

                private HediffDef parentDef;
                public override Def ParentDef => parentDef;

                [CanBeNull] internal HediffCompProperties_NightVision HNVCompProps;

                public override float this[int index]
                    {
                        get
                            {
                                switch (Setting)
                                    {
                                        default:
                                            return 0f;
                                        case Options.NVNightVision:
                                            return NVLightModifiers[index];
                                        case Options.NVPhotosensitivity:
                                            return PSLightModifiers[index];
                                        case Options.NVCustom:
                                            return offsets[index];
                                    }
                            }
                        set
                            {
                                Setting = Options.NVCustom;
                                offsets[index] = Mathf.Clamp(value, -0.99f + 0.2f*(1-index), +1f + 0.2f * (1 - index));
                            }
                    }

                public override float[] DefaultOffsets
                    {
                        get
                            {
                                switch (GetSetting(HNVCompProps))
                                    {
                                        default:
                                            return new float[2];
                                        case Options.NVNightVision:
                                            return NVLightModifiers.offsets;
                                        case Options.NVPhotosensitivity:
                                            return PSLightModifiers.offsets;
                                        case Options.NVCustom:
                                            return new[] {HNVCompProps.ZeroLightMod, HNVCompProps.FullLightMod};
                                    }
                            }
                    }

                public override void ExposeData()
                    {
                        Scribe_Defs.Look(ref parentDef, "HediffDef");
                        Scribe_Values.Look(ref Setting, "Setting", forceSave: true);
                        if (Setting == LightModifiers.Options.NVCustom)
                            {
                                base.ExposeData();
                            }

                        if (Scribe.mode == LoadSaveMode.LoadingVars && ParentDef != null)
                            {
                                Initialised = true;
                                AttachCompProps();
                            }
                    }

                public Hediff_LightModifiers() { }

                public Hediff_LightModifiers(HediffDef hediffDef)
                    {
                        parentDef = hediffDef;
                        AttachCompProps();
                    }

                private void AttachCompProps()
                    {
                        if (parentDef.CompPropsFor(typeof(HediffComp_NightVision)) is HediffCompProperties_NightVision
                                    compProps)
                            {
                                HNVCompProps = compProps;
                                if (!Initialised)
                                    {
                                        Setting = GetSetting(compProps);
                                        offsets = new[] {compProps.ZeroLightMod, compProps.FullLightMod};
                                    }

                            }
                        else
                            {
                                HNVCompProps = null;
                                parentDef.comps.Add(new HediffCompProperties_NightVision());
                                Initialised = true;
                            }
                    }


                public override bool ShouldBeSaved()
                    {
                        if (HNVCompProps == null)
                            {
                                return Setting != Options.NVNone;
                            }

                        switch (Setting)
                            {
                                default:
                                    return !HNVCompProps.IsDefault();
                                case Options.NVNightVision:
                                    return !HNVCompProps.GrantsNightVision;
                                case Options.NVPhotosensitivity:
                                    return !HNVCompProps.GrantsPhotosensitivity;
                                case Options.NVCustom:
                                    return !(Math.Abs(HNVCompProps.FullLightMod - offsets[1]) < 0.001f)
                                           || !(Math.Abs(HNVCompProps.ZeroLightMod - offsets[0]) < 0.001f);
                            }
                    }

                public static Options GetSetting(HediffCompProperties_NightVision compprops)
                    {
                        if (compprops == null)
                            {
                                return Options.NVNone;
                            }
                        if (compprops.GrantsNightVision)
                            {
                                return LightModifiers.Options.NVNightVision;
                            }
                        else if (compprops.GrantsPhotosensitivity)
                            {
                                return LightModifiers.Options.NVPhotosensitivity;
                            }
                        else if (compprops.IsDefault())
                            {
                                return LightModifiers.Options.NVNone;
                            }
                        else
                            {
                                return LightModifiers.Options.NVCustom;
                            }
                    }

    }
}