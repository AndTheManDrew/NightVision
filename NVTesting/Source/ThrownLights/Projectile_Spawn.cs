using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace NVTesting.ThrownLights
{
    public class ProjectileSpawn_ModExt : DefModExtension
    {
        public ThingDef thingToSpawn;
    }

    public class Projectile_Spawn : Projectile
    {
        private MovingGlowFlooder glowFlooder;

        protected override void Impact(Thing hitThing)
        {
            Map map = Map;
            base.Impact(hitThing);

            if (def.GetModExtension<ProjectileSpawn_ModExt>()?.thingToSpawn is ThingDef spawnDef)
            {
                //GenSpawn.Spawn(spawnDef, Position, map);
            }
        }

        public override void Draw()
        {
            base.Draw();
            glowFlooder.Draw(DrawPos);
        }

        public override void Tick()
        {
            base.Tick();
            glowFlooder.UpdatePosition(Position);
        }
        [TweakValue("_NV", 1, 50)]
        public static float glowRadius = 5;

        [TweakValue("_NV", 1, 255)]
        public static int alphaMovingGlow = 255;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            glowFlooder = new MovingGlowFlooder(map, Position, glowRadius, new ColorInt(255, 255, 255, alphaMovingGlow));
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            glowFlooder.OnDestroy();
            base.Destroy(mode);

        }
    }
    
}
