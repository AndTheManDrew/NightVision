// Nightvision NightVision Race_LightModifiers.cs
// 
// 14 06 2018
// 
// 21 07 2018

using System;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace NightVision
    {
        public class Race_LightModifiers : LightModifiersBase
            {
                internal VisionType IntSetting = VisionType.NVNone;

                internal bool    ShouldShowInSettings = true;
                private  float[] _defaultOffsets;

                private int _eyeCount;

                [CanBeNull] private CompProperties_NightVision _nvCompProps;

                private ThingDef _parentDef;


                public Race_LightModifiers() { }

                public Race_LightModifiers(
                    ThingDef raceDef)
                    {
                        _parentDef = raceDef;
                        CountEyes();
                        AttachCompProps();
                    }

                /// <summary>
                ///     Returns normalised modifiers
                /// </summary>
                /// <param name="index">0: 0% light, 1: 100% light</param>
                /// <returns>modifers divided by number of eyes</returns>
                public override float this[
                    int index]
                    {
                        get
                            {
                                switch (IntSetting)
                                    {
                                        default:                            return 0f;
                                        case VisionType.NVNightVision:      return NVLightModifiers[index] / _eyeCount;
                                        case VisionType.NVPhotosensitivity: return PSLightModifiers[index] / _eyeCount;
                                        case VisionType.NVCustom:           return Offsets[index] / _eyeCount;
                                    }
                            }
                        set =>
                                    Offsets[index] = (float) Math.Round(
                                        Mathf.Clamp(value, -0.99f + 0.2f * (1 - index), +1f + 0.2f * (1 - index)),
                                        2,
                                        MidpointRounding.AwayFromZero);
                    }

                internal override VisionType Setting
                    {
                        get => IntSetting;
                        set => IntSetting = value;
                    }

                public override Def  ParentDef => _parentDef;
                public          bool CanCheat  => _nvCompProps?.CanCheat ?? false;

                public override float[] DefaultOffsets
                    {
                        get
                            {
                                if (_defaultOffsets == null)
                                    {
                                        switch (GetSetting(_nvCompProps))
                                            {
                                                default: return new float[2];
                                                case VisionType.NVNightVision:
                                                    return NVLightModifiers.Offsets.ToArray();
                                                case VisionType.NVPhotosensitivity:
                                                    return PSLightModifiers.Offsets.ToArray();
                                                case VisionType.NVCustom:
                                                    return new[]
                                                    {
                                                        _nvCompProps.ZeroLightMultiplier
                                                        - Constants.DefaultZeroLightMultiplier,
                                                        _nvCompProps.FullLightMultiplier
                                                        - Constants.DefaultFullLightMultiplier
                                                    };
                                            }
                                    }

                                return _defaultOffsets;
                            }
                    }

                public override void ExposeData()
                    {
                        Scribe_Defs.Look(ref _parentDef, "RaceDef");
                        Scribe_Values.Look(ref IntSetting, "Setting", forceSave: true);
                        if (IntSetting == VisionType.NVCustom)
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
                
                // Already normalised in the indexer
                //public override float GetEffectAtGlow(
                //    float glow,
                //    int   numEyesNormalisedFor = 1) =>
                //            base.GetEffectAtGlow(glow, _eyeCount);

                public override bool ShouldBeSaved()
                    {
                        if (_nvCompProps == null)
                            {
                                return IntSetting != VisionType.NVNone;
                            }

                        switch (IntSetting)
                            {
                                default:                            return !_nvCompProps.IsDefault();
                                case VisionType.NVNightVision:      return !_nvCompProps.NaturalNightVision;
                                case VisionType.NVPhotosensitivity: return !_nvCompProps.NaturalPhotosensitivity;
                                case VisionType.NVCustom:
                                    return !(Math.Abs(_nvCompProps.FullLightMultiplier
                                                      - Constants.DefaultFullLightMultiplier - Offsets[1]) < 0.001f)
                                           || !(Math.Abs(_nvCompProps.ZeroLightMultiplier
                                                         - Constants.DefaultZeroLightMultiplier - Offsets[0]) < 0.001f);
                            }
                    }

                public static VisionType GetSetting(
                    Race_LightModifiers modifiers) =>
                            GetSetting(modifiers._nvCompProps);

                private void AttachCompProps()
                    {
                        if (_parentDef.GetCompProperties<CompProperties_NightVision>() is CompProperties_NightVision
                                    compProps)
                            {
                                _nvCompProps         = compProps;
                                ShouldShowInSettings = _nvCompProps.ShouldShowInSettings;
                                if (Initialised)
                                    {
                                        return;
                                    }

                                IntSetting = GetSetting(_nvCompProps);
                                Offsets = new[]
                                {
                                    _nvCompProps.ZeroLightMultiplier - Constants.DefaultZeroLightMultiplier,
                                    _nvCompProps.FullLightMultiplier - Constants.DefaultFullLightMultiplier
                                };
                                Initialised = true;
                            }
                        else
                            {
                                _nvCompProps = null;
                                Initialised  = true;
                                _parentDef.comps.Add(new CompProperties_NightVision());
                            }
                    }

                private void CountEyes()
                    {
                        _eyeCount = _parentDef.race.body.AllParts
                                              .FindAll(bpr => bpr.def.tags.Contains(BodyPartTagDefOf.SightSource))
                                              .Count;
                    }

                private static VisionType GetSetting(
                    CompProperties_NightVision compprops)
                    {
                        if (compprops == null)
                            {
                                return VisionType.NVNone;
                            }

                        if (compprops.NaturalNightVision)
                            {
                                return VisionType.NVNightVision;
                            }

                        if (compprops.NaturalPhotosensitivity)
                            {
                                return VisionType.NVPhotosensitivity;
                            }

                        if (compprops.IsDefault())
                            {
                                return VisionType.NVNone;
                            }

                        return VisionType.NVCustom;
                    }
            }
    }