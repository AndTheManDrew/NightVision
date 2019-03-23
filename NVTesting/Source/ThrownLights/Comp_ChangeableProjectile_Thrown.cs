// Nightvision NVTesting Comp_ChangeableProjectile_Thrown.cs
// 
// 19 03 2019
// 
// 19 03 2019

using RimWorld;
using Verse;

namespace NVTesting.ThrownLights
{
    public class Comp_ChangeableProjectile_Thrown : CompChangeableProjectile
    {
        public int ammo;

        public new CompProps_ChangeableProjectile_Thrown Props => (CompProps_ChangeableProjectile_Thrown) props;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref ammo, "ammo");
        }

        public override void Initialize(CompProperties props)
        {
            this.props = props;
            ammo = Props.maxAmmo;
        }

        public override void Notify_ProjectileLaunched()
        {
            ammo--;

            if (ammo == 0)
            {
                parent.Destroy(DestroyMode.Vanish);
            }
        }
        
        public override string CompInspectStringExtra()
        {
            return $"Ammo: {ammo} / {Props.maxAmmo}";
        }
    }
}