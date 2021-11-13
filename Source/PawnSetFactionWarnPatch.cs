using HarmonyLib;
using Verse;

namespace Euphoric.FixRandomFactionPawns
{
    /// <summary>
    /// Warn if trying to set faction to a pawn, where that pawn doesn't belong to that faction.
    /// </summary>
    [HarmonyPatch(typeof(Pawn), "SetFaction")]
    internal static class PawnSetFactionWarnPatch
    {
        [HarmonyPrefix]
        // ReSharper disable once InconsistentNaming
        public static void Prefix(Pawn __instance, RimWorld.Faction newFaction)
        {
            if (newFaction.IsPlayer)
                return;
            
            var pawnKindDef = __instance.kindDef;
            var faction = newFaction.def;

            var allowedFactions = AllowedFactionPawns.Instance.GetFactionsForPawnKind(pawnKindDef);

            if (!allowedFactions.Contains(faction))
            {
                Log.Warning($"Generated pawn [{pawnKindDef}] for faction [{faction}] which shouldn't have that kind of pawn.");
            }
        }
    }
}