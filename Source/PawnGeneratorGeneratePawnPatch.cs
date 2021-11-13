using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Euphoric.FixRandomFactionPawns
{
    internal class AllowedFactionPawns
    {
        private readonly Dictionary<PawnKindDef, ISet<FactionDef>> _pawnToFactions;
        private static readonly Lazy<AllowedFactionPawns> InstanceLazy = new Lazy<AllowedFactionPawns>(ClassFactory);

        private AllowedFactionPawns(Dictionary<PawnKindDef,ISet<FactionDef>> pawnToFactions)
        {
            _pawnToFactions = pawnToFactions;
        }

        public ISet<FactionDef> GetFactionsForPawnKind(PawnKindDef pawnKindDef)
        {
            return _pawnToFactions.TryGetValue(pawnKindDef, out ISet<FactionDef> factions) ? factions : new HashSet<FactionDef>();
        }

        public static AllowedFactionPawns Instance => InstanceLazy.Value;

        private static AllowedFactionPawns ClassFactory()
        {
            var factionPawns =
                // use pawnKinds from PawnGroupMaker of faction
                DefDatabase<FactionDef>.AllDefs
                    .SelectMany(fd =>
                        (fd.pawnGroupMakers ?? new List<PawnGroupMaker>()).SelectMany(pgm =>
                            (pgm.options ?? new List<PawnGenOption>()).Select(opt => new { factionDef = fd, pawnKindDef = opt.kind }))
                    )
                    // add default faction for pawnKind
                    .Concat(
                        DefDatabase<PawnKindDef>.AllDefs
                            .Where(x=>x.defaultFactionType != null)
                            .Select(x => new { factionDef = x.defaultFactionType, pawnKindDef = x }))
                    // add some hard-coded values for special pawn kinds
                    .Concat(new []
                    {
                        new {factionDef = FactionDefOf.Empire, pawnKindDef = PawnKindDefOf.Colonist},
                        new {factionDef = FactionDefOf.OutlanderCivil, pawnKindDef = PawnKindDefOf.Colonist},
                        new {factionDef = FactionDefOf.OutlanderRough, pawnKindDef = PawnKindDefOf.Colonist},
                        new {factionDef = FactionDefOf.TribeCivil, pawnKindDef = PawnKindDefOf.Colonist},
                        new {factionDef = FactionDefOf.TribeRough, pawnKindDef = PawnKindDefOf.Colonist},
                        new {factionDef = DefDatabase<FactionDef>.GetNamed("TribeSavage"), pawnKindDef = PawnKindDefOf.Colonist},
                        new {factionDef = FactionDefOf.Pirate, pawnKindDef = PawnKindDefOf.Colonist},
                        new {factionDef = FactionDefOf.Ancients, pawnKindDef = PawnKindDefOf.AncientSoldier},
                        new {factionDef = FactionDefOf.AncientsHostile, pawnKindDef = PawnKindDefOf.AncientSoldier},
                        new {factionDef = FactionDefOf.Empire, pawnKindDef = PawnKindDefOf.Slave},
                        new {factionDef = FactionDefOf.OutlanderCivil, pawnKindDef = PawnKindDefOf.Slave},
                        new {factionDef = FactionDefOf.OutlanderRough, pawnKindDef = PawnKindDefOf.Slave},
                        new {factionDef = FactionDefOf.TribeCivil, pawnKindDef = PawnKindDefOf.Slave},
                        new {factionDef = FactionDefOf.TribeRough, pawnKindDef = PawnKindDefOf.Slave},
                        new {factionDef = DefDatabase<FactionDef>.GetNamed("TribeSavage"), pawnKindDef = PawnKindDefOf.Slave},
                    })
                    .ToList();

            var pawnToFactions = factionPawns
                .GroupBy(x => x.pawnKindDef)
                .Select(grp => new { pawnKind = grp.Key, factions = grp.Select(x => x.factionDef).Distinct() })
                .ToDictionary(x=>x.pawnKind, x=>(ISet<FactionDef>)x.factions.ToHashSet());
            
            // var msgStr =
            //     string.Join(Environment.NewLine,
            //         factionPawns
            //             .Select(x => x.factionDef.defName + ":" + x.pawnKindDef.defName));
            //
            // Log.Message(msgStr);
            //
            // StringBuilder sb = new StringBuilder();
            // foreach (var pawnToFaction in pawnToFactions)
            // {
            //     sb.AppendLine(pawnToFaction.Key.defName+":");
            //     foreach (var faction in pawnToFaction.Value)
            //     {
            //         sb.AppendLine("  " + faction.defName);
            //     }
            //
            //     sb.AppendLine();
            // }
            // Log.Message(sb.ToString());
            
            return new AllowedFactionPawns(pawnToFactions);
        }
    }

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