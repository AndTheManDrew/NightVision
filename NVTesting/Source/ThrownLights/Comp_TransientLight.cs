// Nightvision NVTesting Comp_TransientLight.cs
// 
// 19 03 2019
// 
// 19 03 2019

using System.Collections.Generic;
using RimWorld;
using Verse;

namespace NVTesting.ThrownLights
{
    public class Comp_TransientLight : CompGlower
    {

        public int ticksRemaining;

        public int radiusChangeTicks;

        public int nextRadiusChangeTick;
        
        
        public CompProperties_TransientLight Props => (CompProperties_TransientLight) props;


        
        public override void Initialize(CompProperties props)
        {
            // hacky way to have dynamic glow radius and colour
            CompProperties_TransientLight propsCopy = new CompProperties_TransientLight(props);
            base.Initialize(propsCopy);
            ticksRemaining = Props.ticksToGlow;

            radiusChangeTicks = ticksRemaining / (int) (propsCopy.glowRadius - propsCopy.finalGlowRadius + 1);
            nextRadiusChangeTick = ticksRemaining - radiusChangeTicks;


        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            parent.Map.mapDrawer.MapMeshDirty(this.parent.Position, MapMeshFlag.Things);
            parent.Map.glowGrid.RegisterGlower(this);
            
        }
        
        public override void PostExposeData()
        {
            Scribe_Values.Look<int>(ref ticksRemaining, "ticksRemaining", 0);
        }

        public override void CompTick()
        {
            base.CompTick();
            ticksRemaining--;

            if (ticksRemaining <= 0)
            {
                parent.Destroy(DestroyMode.Vanish);
            }
            else
            {
                if (Find.TickManager.TicksGame % 250 == 0)
                {
                    this.CompTickRare();
                }
            }
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            Log.Message($"TicksRemaining {ticksRemaining}");

            if (ticksRemaining < nextRadiusChangeTick)
            {
                Props.glowRadius--;

                parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.Things);
                parent.Map.glowGrid.MarkGlowGridDirty(parent.Position);
                nextRadiusChangeTick -= radiusChangeTicks;
            }
            
        }

        public override void PostDeSpawn(Map map)
        {
            map.mapDrawer.MapMeshDirty(this.parent.Position, MapMeshFlag.Things);
            map.glowGrid.DeRegisterGlower(this);
        }


        public override string CompInspectStringExtra()
        {
            
            int hrsRemaining = ticksRemaining/ GenDate.TicksPerHour;

            int qHrsRemaining = (ticksRemaining % GenDate.TicksPerHour) * 4 / GenDate.TicksPerHour;

            return hrsRemaining + qHrsRemaining == 0
                        ? "Less than 15mins left"
                        : $"{(hrsRemaining > 0 ? $"{hrsRemaining}hrs " : "")}{(qHrsRemaining > 0 ? $"{qHrsRemaining * 15}mins " : "")}left.";
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            return base.CompFloatMenuOptions(selPawn);
        }
    }
}