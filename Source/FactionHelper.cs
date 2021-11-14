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
                .Select(f => new
                    { faction = f, weight = tryMedievalOrBetter && f.def.techLevel < TechLevel.Medieval ? 0.1f : 1f });

            // var sb = new StringBuilder();
            // sb.AppendLine($"Factions to select for [{forPawnKindDef}] from: ");
            // foreach (var fw in factionsToSelectFrom)
            // {
            //     sb.AppendLine(fw.faction.def.defName + ":" + fw.weight);
            // }
            // Log.Message(sb.ToString());

            var foundFaction = factionsToSelectFrom.TryRandomElementByWeight(x => x.weight, out var factionWithWeight);

            faction = factionWithWeight.faction;
            //Log.Message($"Selected faction [{foundFaction}], [{faction.def}]");

            return foundFaction;
        }
    }
}