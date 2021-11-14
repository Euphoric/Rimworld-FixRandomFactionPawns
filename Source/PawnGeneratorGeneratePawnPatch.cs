using HarmonyLib;
using Verse;

namespace Euphoric.FixRandomFactionPawns
{
    /// <summary>
    /// Warn if trying to generate pawn for faction that doesn't have PawnKind set up.
    /// </summary>
    [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", typeof(PawnGenerationRequest))]
    internal static class PawnGeneratorGeneratePawnWarnPatch
    {
        [HarmonyPrefix]
        public static void Prefix(PawnGenerationRequest request)
        {
            if (request.Faction == null)
                return; // most likely an animal

            if (request.Faction.IsPlayer)
                return;
            
            if (!request.Faction.def.humanlikeFaction)
                return; // insects, mechanoids, etc..
            
            var pawnKindDef = request.KindDef;
            var faction = request.Faction.def;

            var allowedFactions = AllowedFactionPawns.Instance.GetFactionsForPawnKind(pawnKindDef);

            if (!allowedFactions.Contains(faction))
            {
                Log.Warning($"Generated pawn [{pawnKindDef}] for faction [{faction}] which shouldn't have that kind of pawn.");
            }
        }
    }
}