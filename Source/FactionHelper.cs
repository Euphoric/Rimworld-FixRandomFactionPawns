using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Euphoric.FixRandomFactionPawns
{
    public static class FactionHelper
    {
        public static bool TryGetRandomFactionForPawn(
            this FactionManager factionManager,
            out Faction faction,
            bool tryMedievalOrBetter,
            bool allowDefeated,
            TechLevel minTechLevel,
            bool allowTemporary,
            Pawn forPawn)
        {
            var forPawnKindDef = forPawn?.kindDef;
            
            //Log.Message($"Selecting random faction for [{forPawnKindDef}].");
            
            var allowedFactions = AllowedFactionPawns.Instance.GetFactionsForPawnKind(forPawnKindDef);
            var factionsToSelectFrom = factionManager.AllFactions.Where(x =>
                {
                    if (x.IsPlayer || x.Hidden || !x.def.humanlikeFaction || !allowDefeated && x.defeated ||
                        !allowTemporary && x.temporary || !allowedFactions.Contains(x.def))
                        return false;
                    return minTechLevel == TechLevel.Undefined || x.def.techLevel >= minTechLevel;
                })
                .Select(f => new { faction = f, weight = tryMedievalOrBetter && f.def.techLevel < TechLevel.Medieval ? 0.1f : 1f })
                .ToList();

            var foundFaction = factionsToSelectFrom.TryRandomElementByWeight(x => x.weight, out var factionWithWeight);
            
            //Log.Message($"Selected faction [{foundFaction}], [{faction.def}]");

            if (!foundFaction)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Unable to find faction for pawn def {forPawnKindDef}. Defaulting to original method.");
                var factionsString = string.Join(",", DefDatabase<FactionDef>.AllDefs.Select(x => x.defName));
                sb.AppendLine($"Factions:{factionsString}");
                Log.Warning(sb.ToString());

                return factionManager.TryGetRandomNonColonyHumanlikeFaction(out faction, tryMedievalOrBetter, allowDefeated, minTechLevel, allowTemporary);
            }
            else
            {
                faction = factionWithWeight.faction;
                return true;
            }
        }
    }
}