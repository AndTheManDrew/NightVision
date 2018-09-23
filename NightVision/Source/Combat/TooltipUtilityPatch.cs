using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Harmony;
using Verse;

namespace NightVision
{
    [HarmonyPatch(typeof(TooltipUtility), nameof(TooltipUtility.ShotCalculationTipString))]
    public static class TooltipUtilityPatch
    {
        /*
         Original instructions:
           yield org.instructions till:
           current  ==   IL_0104: call         instance string Verse.ShotReport::GetTextReadout()
           yield current
           yield next == IL_0109: callvirt     instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
           clone next
           yield insert < ldloc.2 //verb
           yield insert < ldfld Verse.Verb::caster
           yield insert < ldarg.0 //target
           yield insert < call OURMETHOD(caster, target)
           yield cloned next
           yield the rest of org instructions
         */
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ShotCalculationTipString_Transpiler(
                        IEnumerable<CodeInstruction> instructions, ILGenerator il
                    )
        {
            var instructionsList = instructions.ToList();

            var casterFldInfo = AccessTools.Field(typeof(Verb), nameof(Verb.caster));

            var signifyingMethodInfo = AccessTools.Method(typeof(ShotReport), nameof(ShotReport.GetTextReadout));

            var ourMethodCall = new CodeInstruction(
                                                    OpCodes.Call,
                                                    AccessTools.Method(
                                                                       typeof(TooltipUtilityPatch),
                                                                       nameof(TooltipUtilityPatch
                                                                                          .NightVisionTooltipElement)
                                                                      )
                                                   );
            
            for (int i = 0; i < instructionsList.Count; i++)
            {
                var current = instructionsList[i];
                if (current.opcode == OpCodes.Call && current.operand == signifyingMethodInfo)
                {
                   
                    yield return current;

                    i++;

                    if (i >= instructionsList.Count)
                    {
                        yield break;
                    }
                    //StringBuilder.Append: consumes string from current, returns ref to stringbuilder
                    var clonedInstr = instructionsList[i].Clone();

                    yield return instructionsList[i];
                    // StringBuilder.Append:  clone this so we can use it to append our string
                    //Note: top of stack is StringBuilder 
                    // load the local variable: verb
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    // load the field of verb: verb.caster
                    yield return new CodeInstruction(OpCodes.Ldfld, casterFldInfo);
                    // load the local variable: verb
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    // load the first argument of static TooltipUtility.ShotCalculationTooltip: Thing target
                    yield return new CodeInstruction(OpCodes.Ldarg_0, null);
                    // call our method(Thing caster, Verb verb, Thing target): consumes caster, verb, and target, returns string
                    yield return ourMethodCall;
                    // top of stack is returned string, then stringbuilder ref: we call cloned StringBuilder.append
                    yield return clonedInstr;
                }
                else
                {
                    yield return instructionsList[i];
                }
            }
        }

        public static string NightVisionTooltipElement(Thing caster, Verb verb, Thing target)
        {
            string result = "";

            if (verb.verbProps.forcedMissRadius > 0.5f)
            {
                return result;
            }
            if (caster is Pawn pawn && pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
            {

                result += "\n   " + "NVAimFactorFromLight".Translate(CombatHelpers.ShootGlowFactor(pawn, target, comp, out float glow), glow);
            }

            return result;
        }
    }
}
