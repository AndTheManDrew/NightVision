using System;
using JetBrains.Annotations;
using NightVision.Comps;
using UnityEngine;

namespace NightVision
    {
        using Verse;

        public class Race_LightModifiers : LightModifiers
            {
                internal Options Setting = Options.NVNone;

                internal ThingDef _parentDef;
                public override Def ParentDef => _parentDef;

                [CanBeNull]
                internal CompProperties_NightVision NVCompProps;

                internal int EyeCount;
        

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
                offsets[index] = Mathf.Clamp(value, -0.99f + 0.2f * (1 - index), +1f + 0.2f * (1 - index));
            }
        }

        public override float[] DefaultOffsets
        {
            get
            {
                switch (GetSetting(NVCompProps))
                {
                    default:
                        return new float[2];
                    case Options.NVNightVision:
                        return NVLightModifiers.offsets;
                    case Options.NVPhotosensitivity:
                        return PSLightModifiers.offsets;
                    case Options.NVCustom:
                        return new[] { NVCompProps.ZeroLightMultiplier - NightVisionSettings.DefaultZeroLightMultiplier, NVCompProps.FullLightMultiplier - NightVisionSettings.DefaultFullLightMultiplier };
                }
            }
        }


        public Race_LightModifiers() { }

                public Race_LightModifiers(ThingDef raceDef)
                    {
                        _parentDef = raceDef;
                        CountEyes();
                        AttachCompProps();
                        
                    }

                public override void ExposeData()
                    {
                        Scribe_Defs.Look(ref _parentDef, "RaceDef");
                        Scribe_Values.Look(ref Setting, "Setting", forceSave: true);
                        if (Setting == Options.NVCustom)
                            {
                                base.ExposeData();
                            }

                        if (Scribe.mode == LoadSaveMode.LoadingVars && ParentDef != null)
                            {
                                Initialised = true;
                                CountEyes();
                                AttachCompProps();
                            }
                    }

                private void AttachCompProps()
                    {
                        if (_parentDef.GetCompProperties<CompProperties_NightVision>() is CompProperties_NightVision
                                    compProps)
                            {
                                NVCompProps = compProps;
                                if (Initialised)
                                    {
                                        return;
                                    }

                                Setting = GetSetting(NVCompProps);
                                offsets = new[]
                                {
                                            NVCompProps.ZeroLightMultiplier
                                            - NightVisionSettings.DefaultZeroLightMultiplier,
                                            NVCompProps.FullLightMultiplier
                                            - NightVisionSettings.DefaultFullLightMultiplier
                                };
                                Initialised = true;
                            }
                        else
                            {
                                NVCompProps = null;
                                Initialised = true;
                                _parentDef.comps.Add(new CompProperties_NightVision());
                            }
                    }

                private void CountEyes()
                    {
                        EyeCount = _parentDef.race.body.AllParts.FindAll(bpr => bpr.def.tags.Contains(NVStrings.EyeTag))
                                          .Count;
                    }

                public override bool ShouldBeSaved()
                    {
                        if (NVCompProps == null)
                            {
                                return Setting != Options.NVNone;
                            }

                        switch (Setting)
                            {
                                default:
                                    return !NVCompProps.IsDefault();
                                case Options.NVNightVision:
                                    return !NVCompProps.NaturalNightVision;
                                case Options.NVPhotosensitivity:
                                    return !NVCompProps.NaturalPhotosensitivity;
                                case Options.NVCustom:
                                    return !(Math.Abs(NVCompProps.FullLightMultiplier
                                                      - NightVisionSettings.DefaultFullLightMultiplier
                                                      - offsets[1]) < 0.001f)
                                           || !(Math.Abs(NVCompProps.ZeroLightMultiplier
                                                         - NightVisionSettings.DefaultZeroLightMultiplier
                                                         - offsets[0]) < 0.001f);
                            }
                    }

                public static Options GetSetting(CompProperties_NightVision compprops)
                    {
                        if (compprops == null)
                            {
                                return Options.NVNone;
                            }
                        if (compprops.NaturalNightVision)
                            {
                                return Options.NVNightVision;
                            }

                        if (compprops.NaturalPhotosensitivity)
                            {
                                return Options.NVPhotosensitivity;
                            }

                        if (compprops.IsDefault())
                            {
                                return Options.NVNone;
                            }

                        return Options.NVCustom;
                    }


    }
    }