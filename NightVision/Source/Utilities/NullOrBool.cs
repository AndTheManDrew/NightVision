// Nightvision NightVision NullOrBool.cs
// 
// 19 10 2018
// 
// 19 10 2018

namespace NightVision
{
    public enum TriBool : sbyte
    {
        Null = -1,
        False = 0,
        True = 1,
        Undefined = -1
    }

    public static class TriBoolExtensions
    {
        public static bool IsNull(this TriBool val)
        {
            return val == TriBool.Null;
        }

        public static bool IsUndefined(this TriBool val)
        {
            return val == TriBool.Undefined;
        }

        public static bool IsTrue(this TriBool val)
        {
            return val == TriBool.True;
        }

        public static bool IsNotTrue(this TriBool val)
        {
            return val != TriBool.True;
        }

        public static bool IsFalse(this TriBool val)
        {
            return val == TriBool.False;
        }

        public static bool IsNotFalse(this TriBool val)
        {
            return val != TriBool.False;
        }

        public static void MakeTrue(this ref TriBool val)
        {
            val = TriBool.True;
        }

        public static void MakeFalse(this ref TriBool val)
        {
            val = TriBool.False;
        }
    }
}