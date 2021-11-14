using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Euphoric.FixRandomFactionPawns
{
    /// <summary>
    /// When adding pawn to a world, Rimworld can select different random faction for pawn if current faction is not valid.
    /// This replaces the random selection method with one that only selects from factions allowed for the pawn def kind.
    /// </summary>
    [HarmonyPatch]
    internal static class UpdateRandomFactionSelectionForPawnPatch
    {
        [HarmonyPatch(typeof(Pawn), "Notify_PassedToWorld")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var originalMethod = AccessTools.Method(typeof(FactionManager), "TryGetRandomNonColonyHumanlikeFaction");
            var newMethod = AccessTools.Method(typeof(FactionHelper), nameof(FactionHelper.TryGetRandomFactionForPawn));
            foreach (var instruction in instructions)
            {
                if (instruction.Calls(originalMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0); // loads Pawn instance
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