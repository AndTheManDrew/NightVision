using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using Verse;

namespace NVExperiments.ThrownLights
{
    public class CompEquipable_SecondaryThrown : CompEquippable
    {
        
    }

    [HarmonyPatch(typeof(CompEquippable), nameof(CompEquippable.GetVerbsCommands))]
    public class CompEquipable_GetVerbCommands_Patch
    {
        public static IEnumerable<Command> Postfix(IEnumerable<Command> commands, CompEquippable __instance)
        {
            if (__instance is CompEquipable_SecondaryThrown compSec && __instance.ParentHolder?.ParentHolder is Pawn pawn && pawn.IsColonistPlayerControlled)
            {
                //Log.Message($"Found CompEquipable_SecondaryThrown");
                ThingWithComps owner = __instance.parent;

                //Want to return a verb that allows for single throws
                yield return new VerbTarget_ThrownLight()
                {
                    defaultDesc = owner.LabelCap + ": " + owner.def.description.CapitalizeFirst(),
                    icon = owner.def.uiIcon,
                    iconAngle = owner.def.uiIconAngle,
                    iconOffset = owner.def.uiIconOffset,
                    verb = __instance.PrimaryVerb,
                    comp = owner.GetComp<Comp_ChangeableProjectile_Thrown>()
                };
            }
                foreach (Command command in commands)
                {
                    yield return command;
                }
            
        }
    }
}
