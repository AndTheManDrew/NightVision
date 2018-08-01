﻿// Nightvision NightVision Scribes.cs
// 
// 21 07 2018
// 
// 21 07 2018

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Verse;

namespace NightVision
{
    public static class Scribes
    {
        public static void ApparelDict(
                        ref Dictionary<ThingDef, ApparelVisionSetting> dictionary
                    )
        {
            if (dictionary == null)
            {
                return;
            }

            var tempList = new List<ApparelSaveLoadClass>();

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                foreach (KeyValuePair<ThingDef, ApparelVisionSetting> kvp in dictionary)
                {
                    if (kvp.Value != null && kvp.Value.ShouldBeSaved())
                    {
                        tempList.Add(new ApparelSaveLoadClass(kvp.Key, kvp.Value));
                    }
                }
            }

            Scribe_Collections.Look(ref tempList, "Apparel", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                dictionary.Clear();
                var removed = 0;

                for (int i = tempList.Count - 1; i >= 0; i--)
                {
                    if (tempList[i] != null && tempList[i].HasValue())
                    {
                        dictionary[tempList[i].Key] = tempList[i].Value;
                    }
                    else
                    {
                        tempList.RemoveAt(i);
                        removed++;
                    }
                }

                if (removed > 0)
                {
                    Log.Message("NVNullEntryLog".Translate(removed, nameof(dictionary)));
                    Scribe.mode = LoadSaveMode.Saving;
                    Scribe_Collections.Look(ref tempList, "Apparel", LookMode.Deep);
                    Scribe.mode = LoadSaveMode.LoadingVars;
                }
            }

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                tempList?.Clear();
            }
        }

        public static void LightModifiersDict<TK, TV>(
                        ref Dictionary<TK, TV> dictionary,
                        string                 label
                    ) where TK : Def where TV : LightModifiersBase
        {
            List<TV> tempList;

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                if (dictionary == null || dictionary.Count == 0)
                {
                    return;
                }

                tempList = dictionary.Values.ToList();
                tempList.RemoveAll(lm => lm == null || !lm.ShouldBeSaved());
                Scribe_Collections.Look(ref tempList, label, LookMode.Deep);
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                tempList = new List<TV>();
                Scribe_Collections.Look(ref tempList, label, LookMode.Deep);
                dictionary.Clear();
                var removed = 0;

                for (int i = tempList.Count - 1; i >= 0; i--)
                {
                    if (tempList[i] != null && tempList[i].ParentDef != null)
                    {
                        dictionary[(TK) tempList[i].ParentDef] = tempList[i];
                    }
                    else
                    {
                        tempList.RemoveAt(i);
                        removed++;
                    }
                }

                if (removed > 0)
                {
                    Log.Message("NVNullEntryLog".Translate(removed, nameof(dictionary)));
                    Scribe.mode = LoadSaveMode.Saving;
                    Scribe_Collections.Look(ref tempList, label, LookMode.Deep);
                    Scribe.mode = LoadSaveMode.LoadingVars;
                }
            }
        }

        public class ApparelSaveLoadClass : IExposable
        {
            public ThingDef             Key;
            public ApparelVisionSetting Value;

            [UsedImplicitly]
            public ApparelSaveLoadClass() { }

            public ApparelSaveLoadClass(
                            ThingDef             key,
                            ApparelVisionSetting value
                        )
            {
                Key   = key;
                Value = value;
            }

            public void ExposeData()
            {
                Scribe_Defs.Look(ref Key, "apparelDef");
                Scribe_Deep.Look(ref Value, "apparelSetting");
            }

            public bool HasValue() => Key != null && Value != null;
        }
    }
}