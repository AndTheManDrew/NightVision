using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NightVision.Comps;
using NightVision.Utilities;
using UnityEngine;
using Verse;

namespace NightVision.LightModifiers
    {

        public class Hediff_LightModifiers : LightModifiersBase
            {
                internal bool AffectsEye = false;
                internal bool AutoAssigned = false;
                internal override Options Setting
                    {
                        get => IntSetting;
                        set => IntSetting = value;
                    }

                internal Options IntSetting;

                private HediffDef _parentDef;
                public override Def ParentDef => _parentDef;

                [CanBeNull] private HediffCompProperties_NightVision _hediffCompProps;

                public override float this[int index]
                    {
                        get
                            {
                                switch (IntSetting)
                                    {
                                        default:
                                            return 0f;
                                        case Options.NVNightVision:
                                            return NVLightModifiers[index];
                                        case Options.NVPhotosensitivity:
                                            return PSLightModifiers[index];
                                        case Options.NVCustom:
                                            return Offsets[index];
                                    }
                            }
                        set => Offsets[index] =
                                    (float)Math.Round(
                                                      Mathf.Clamp(value, -0.99f + 0.2f * (1 - index), +1f + 0.2f * (1 - index)), 2, MidpointRounding.AwayFromZero);
                    }

                private float[] _defaultOffsets;
                public override float[] DefaultOffsets
                    {
                        get
                            {
                                if (_defaultOffsets == null)
                                    {
                                        Options defaultSetting = AutoAssigned
                                                    ? AutoQualifier.HediffCheck(_parentDef) ?? Options.NVNone
                                                    : GetSetting(_hediffCompProps);
                                        switch (defaultSetting)
                                            {
                                                default:
                                                    _defaultOffsets = new float[2];
                                                    break;
                                                case Options.NVNightVision:
                                                    _defaultOffsets =  NVLightModifiers.Offsets.ToArray();
                                                    break;
                                                case Options.NVPhotosensitivity:
                                                    _defaultOffsets = PSLightModifiers.Offsets.ToArray();
                                                    break;
                                                case Options.NVCustom:
                                                    _defaultOffsets = new[] {_hediffCompProps.ZeroLightMod, _hediffCompProps.FullLightMod};
                                                    break;
                                            }
                                    }

                                return _defaultOffsets;

                            }
                    }

                public override void ExposeData()
                    {
                        Scribe_Defs.Look(ref _parentDef, "HediffDef");
                        Scribe_Values.Look(ref IntSetting, "Setting", forceSave: true);
                        if (IntSetting == Options.NVCustom)
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
                        _parentDef = hediffDef;
                        AttachCompProps();
                    }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="glow"> [0,1]</param>
                /// <param name="NumOfEyesNormalisedFor">Required for hediffs</param>
                /// <returns>Normalised value if hediff is an eye hediff</returns>
                public override float GetEffectAtGlow(float glow, int NumOfEyesNormalisedFor)
                    {
                        if (AffectsEye)
                            {
                                return base.GetEffectAtGlow(glow, NumOfEyesNormalisedFor);
                            }

                        return base.GetEffectAtGlow(glow);
                    }


                private void AttachCompProps()
                    {
                        if (_parentDef.CompPropsFor(typeof(HediffComp_NightVision)) is HediffCompProperties_NightVision
                                    compProps)
                            {
                                _hediffCompProps = compProps;
                                compProps.LightModifiers = this;
                                if (!Initialised)
                                    {
                                        IntSetting = GetSetting(compProps);
                                        Offsets = new[] {compProps.ZeroLightMod, compProps.FullLightMod};
                                    }

                            }
                        else
                            {
                                if (_parentDef.comps == null)
                                    {
                                        //TODO Review
                                        _parentDef.comps = new List<HediffCompProperties>();
                                    }
                                _hediffCompProps = new HediffCompProperties_NightVision {LightModifiers = this};
                                _parentDef.comps.Add(_hediffCompProps);
                                Initialised = true;
                            }
                    }

                public void InitialiseNewFromSettings(HediffDef hediffDef)
                    {
                        Initialised = true;
                        _parentDef = hediffDef;
                        AttachCompProps();
                    }


                public override bool ShouldBeSaved()
                    {
                        if (AutoAssigned)
                            {
                                return IntSetting != AutoQualifier.HediffCheck(_parentDef);
                            }

                        switch (IntSetting)
                            {
                                default:
                                    return !_hediffCompProps.IsDefault();
                                case Options.NVNightVision:
                                    return !_hediffCompProps.GrantsNightVision;
                                case Options.NVPhotosensitivity:
                                    return !_hediffCompProps.GrantsPhotosensitivity;
                                case Options.NVCustom:
                                    return !(Math.Abs(_hediffCompProps.FullLightMod - Offsets[1]) < 0.001f)
                                           || !(Math.Abs(_hediffCompProps.ZeroLightMod - Offsets[0]) < 0.001f);
                            }
                    }

                private static Options GetSetting(HediffCompProperties_NightVision compprops)
                    {
                        if (compprops == null)
                            {
                                return Options.NVNone;
                            }
                        if (compprops.GrantsNightVision)
                            {
                                return Options.NVNightVision;
                            }

                        if (compprops.GrantsPhotosensitivity)
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