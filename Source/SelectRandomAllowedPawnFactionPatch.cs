using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;

namespace Euphoric.FixRandomFactionPawns
{
    /// <summary>
    /// Rimworld can pick random faction when generating a parent for a pawn, while keeping the kind of the child pawn.
    /// This replaces the random selection method with one that only selects from factions allowed for the pawn def kind.
    /// </summary>
    [HarmonyPatch]
    internal static class SelectRandomAllowedPawnFactionPatch
    {
        [HarmonyPatch(typeof(PawnRelationWorker_Sibling), "GenerateParent")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var originalMethod = AccessTools.Method(typeof(FactionManager), "TryGetRandomNonColonyHumanlikeFaction");
            var newMethod = AccessTools.Method(typeof(FactionHelper), nameof(FactionHelper.TryGetRandomFactionForPawn));
            foreach (var instruction in instructions)
            {
                if (instruction.Calls(originalMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1); // loads existingChild argument
                    yield return new CodeInstruction(OpCodes.Callvirt, newMethod);
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }
}