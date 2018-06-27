using System;
using JetBrains.Annotations;
using NightVision.Comps;
using RimWorld;
using UnityEngine;
using Verse;

namespace NightVision.LightModifiers
    {

        public class Race_LightModifiers : LightModifiersBase
            {
                internal override Options Setting
                    {
                        get => IntSetting;
                        set => IntSetting = value;
                    }

                internal Options IntSetting = Options.NVNone;

                private ThingDef _parentDef;
                public override Def      ParentDef => _parentDef;

                [CanBeNull] private CompProperties_NightVision _nvCompProps;

                private int _eyeCount;

                /// <summary>
                /// Returns normalised modifiers
                /// </summary>
                /// <param name="index">0: 0% light, 1: 100% light</param>
                /// <returns>modifers divided by number of eyes</returns>
                public override float this[int index]
                    {
                        get
                            {
                                switch (IntSetting)
                                    {
                                        default:
                                            return 0f;
                                        case Options.NVNightVision:
                                            return NVLightModifiers[index] / _eyeCount;
                                        case Options.NVPhotosensitivity:
                                            return PSLightModifiers[index] / _eyeCount;
                                        case Options.NVCustom:
                                            return Offsets[index] / _eyeCount;
                                    }
                            }
                        set => Offsets[index] =
                                    (float)Math.Round(
                                                      Mathf.Clamp(value, -0.99f + 0.2f * (1 - index), +1f + 0.2f * (1 - index)), 2, MidpointRounding.AwayFromZero);
                    }

                public override float[] DefaultOffsets
                    {
                        get
                            {
                                switch (GetSetting(_nvCompProps))
                                    {
                                        default:
                                            return new float[2];
                                        case Options.NVNightVision:
                                            return NVLightModifiers.Offsets;
                                        case Options.NVPhotosensitivity:
                                            return PSLightModifiers.Offsets;
                                        case Options.NVCustom:
                                            return new[]
                                            {
                                                        _nvCompProps.ZeroLightMultiplier
                                                        - NightVisionSettings.DefaultZeroLightMultiplier,
                                                        _nvCompProps.FullLightMultiplier
                                                        - NightVisionSettings.DefaultFullLightMultiplier
                                            };
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
                        Scribe_Values.Look(ref IntSetting, "Setting", forceSave: true);
                        if (IntSetting == Options.NVCustom)
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
                                _nvCompProps = compProps;
                                if (Initialised)
                                    {
                                        return;
                                    }

                                IntSetting = GetSetting(_nvCompProps);
                                Offsets = new[]
                                {
                                            _nvCompProps.ZeroLightMultiplier
                                            - NightVisionSettings.DefaultZeroLightMultiplier,
                                            _nvCompProps.FullLightMultiplier
                                            - NightVisionSettings.DefaultFullLightMultiplier
                                };
                                Initialised = true;
                            }
                        else
                            {
                                _nvCompProps = null;
                                Initialised = true;
                                _parentDef.comps.Add(new CompProperties_NightVision());
                            }
                    }

                private void CountEyes()
                    {
                        _eyeCount = _parentDef.race.body.AllParts.FindAll(bpr => bpr.def.tags.Contains(BodyPartTagDefOf.SightSource))
                                             .Count;
                    }

                public override bool ShouldBeSaved()
                    {
                        if (_nvCompProps == null)
                            {
                                return IntSetting != Options.NVNone;
                            }

                        switch (IntSetting)
                            {
                                default:
                                    return !_nvCompProps.IsDefault();
                                case Options.NVNightVision:
                                    return !_nvCompProps.NaturalNightVision;
                                case Options.NVPhotosensitivity:
                                    return !_nvCompProps.NaturalPhotosensitivity;
                                case Options.NVCustom:
                                    return !(Math.Abs(_nvCompProps.FullLightMultiplier
                                                      - NightVisionSettings.DefaultFullLightMultiplier
                                                      - Offsets[1]) < 0.001f)
                                           || !(Math.Abs(_nvCompProps.ZeroLightMultiplier
                                                         - NightVisionSettings.DefaultZeroLightMultiplier
                                                         - Offsets[0]) < 0.001f);
                            }
                    }

                private static Options GetSetting(CompProperties_NightVision compprops)
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