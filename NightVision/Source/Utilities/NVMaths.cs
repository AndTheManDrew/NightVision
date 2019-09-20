namespace NightVision
{
    public static class NVMaths
    {
        public static float ClampMod(this LightModifiersBase baseMods, float mod, bool isZeroLight)
        {
            float cap = baseMods[isZeroLight? 0 : 1];
            if (mod > cap == mod > 0)
            {
                if (cap > 0 == mod > 0)
                {
                    return cap;
                }
                return 0;
            }
            return mod;
        }
    }
}