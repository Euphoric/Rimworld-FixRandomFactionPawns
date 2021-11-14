using HarmonyLib;
using RimWorld;
using Verse;

namespace Euphoric.FixRandomFactionPawns
{
    /// <summary>
    /// Generates warning when trying to create pawn with faction into which it doesn't belong. 
    /// </summary>
    [HarmonyPatch]
    internal static class PawnFactionWarnPatch
    {
        private static bool IsAllowedFaction(Faction faction, PawnKindDef pawnKindDef)
        {
            if (faction == null)
                return true; // most likely an animal

            if (faction.IsPlayer)
                return true;

            if (!faction.def.humanlikeFaction)
                return true; // insects, mechanoids, etc..
            
            var factionDef = faction.def;

            var allowedFactions = AllowedFactionPawns.Instance.GetFactionsForPawnKind(pawnKindDef);

            var isAllowedFaction = allowedFactions.Contains(factionDef);
            return isAllowedFaction;
        }

        /// <summary>
        /// Warn if trying to generate pawn for faction that doesn't have PawnKind set up.
        /// </summary>
        [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", typeof(PawnGenerationRequest))]
        [HarmonyPrefix]
        public static void PawnGeneratorGeneratePawn_Prefix(PawnGenerationRequest request)
        {
            var faction = request.Faction;
            var pawnKindDef = request.KindDef;
            if (!IsAllowedFaction(faction, pawnKindDef))
            {
                Log.Warning($"Generated pawn [{pawnKindDef}] for faction [{faction?.def}] which shouldn't have that kind of pawn.");
            }
        }

        /// <summary>
        /// Warn if trying to set faction to a pawn, where that pawn doesn't belong to that faction.
        /// </summary>
        [HarmonyPatch(typeof(Pawn), "SetFaction")]
        [HarmonyPrefix]
        // ReSharper disable once InconsistentNaming
        public static void PawnSetFaction_Prefix(Pawn __instance, Faction newFaction)
        {
            var pawnKindDef = __instance.kindDef;

            if (!IsAllowedFaction(newFaction, pawnKindDef))
            {
                Log.Warning(
                    $"Setting faction [{newFaction.def}] to pawn [{pawnKindDef}], where faction shouldn't have that kind of pawn.");
            }
        }
    }
}