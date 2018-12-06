using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using Verse;

namespace NightVision
{
    public static class FieldClearer
    {
        public static List<FieldInfo> SettingsDependentFields = new List<FieldInfo>();
        

        public static void ResetSettingsDependentFields()
        {
            foreach (FieldInfo field in SettingsDependentFields)
            {

                
                if (field.IsStatic)
                {
                    
                    if (field.FieldType == typeof(TriBool))
                    {
                        field.SetValue(null, TriBool.Undefined);
                    }
                    else if (field.FieldType.IsClass)
                    {
                        field.SetValue(null, null);
                    }
                    else if (field.FieldType == typeof(int))
                    {
                        field.SetValue(null, -9999);
                    }
                    else if (field.FieldType == typeof(float))
                    {
                        field.SetValue(null, -9999f);
                    }
                    
                }
                else
                {
                    Log.Warning("Tried to clear non-static field. Either make field static or remove attribute.");
                }
                
            }
        }
    }
}
