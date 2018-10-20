using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NightVision
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class NVHasSettingsDependentFieldAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class NVSettingsDependentFieldAttribute : Attribute
    {

    }
}
