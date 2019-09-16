using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Harmony;
using Verse;

namespace NightVision
{
    public static class FieldClearer
    {
        public static List<FieldInfo> SettingsDependentFields = new List<FieldInfo>();
        
        public static List<Traverse> SettingsDependentFieldTraverses = new List<Traverse>();
        
        public static void FindSettingsDependentFields()
        {
            var traverses = new List<Traverse>();

            var markedTypes = GenTypes.AllTypesWithAttribute<NVHasSettingsDependentFieldAttribute>();
            foreach (var type in markedTypes)
            {
                var fields = AccessTools.GetDeclaredFields(type)
                    .FindAll(fi => fi.HasAttribute<NVHasSettingsDependentFieldAttribute>());

                foreach (var info in fields)
                {
                    var traverse = new Traverse(type);
                    traverse.Field(info.Name);
                    
                    traverses.Add(traverse);

                }
            }
            SettingsDependentFieldTraverses = traverses;
        }
        
        
        public static void ResetSettingsDependentFields()
        {
            if (SettingsDependentFieldTraverses.Count == 0)
            {
                return;
            }
            foreach (var fieldTraverse in SettingsDependentFieldTraverses)
            {
                if (!fieldTraverse.FieldExists())
                {
                    Log.Warning($"SettingsDependentFieldTraverses included a field that did not exist.");
                    continue;
                }

                switch (fieldTraverse.GetValue())
                {
                    case float flt:
                        fieldTraverse.SetValue(-9999f);
                        break;
                    case int i:
                        fieldTraverse.SetValue(-9999);
                        break;
                    case TriBool tri:
                        fieldTraverse.SetValue(TriBool.Undefined);
                        break;
                    default:
                        fieldTraverse.SetValue(default);
                        break;
                }
                
            }
            /*foreach (FieldInfo field in SettingsDependentFields)
            {

                
                if (field.IsStatic)
                {
                    if (field.FieldType == typeof(TriBool))
                    {
                        field.SetValue(null, TriBool.Undefined);
                    }/* - covered by default option
                    else if (field.FieldType.IsClass)
                    {
                        field.SetValue(null, null);
                    }#1#
                    else if (field.FieldType == typeof(int))
                    {
                        field.SetValue(null, -9999);
                    }
                    else if (field.FieldType == typeof(float))
                    {
                        field.SetValue(null, -9999f);
                    }
                    else
                    {
                        field.SetValue(null, default);
                    }
                    
                }
                else
                {
                    Log.Warning("Tried to clear non-static field. Either make field static or remove attribute.");
                }
                
            }*/
        }
    }
}
