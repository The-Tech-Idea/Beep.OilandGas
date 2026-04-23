using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models;

namespace Beep.OilandGas.Drawing.Visualizations.WellSchematic.Rendering
{
    internal static class LegacyEquipmentSvgResolver
    {
        private sealed record SvgAliasRule(string SymbolFile, params string[] MatchTerms);

        private static readonly IReadOnlyDictionary<string, string> ExactAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["blast joint"] = "blast joint.svg",
            ["blast_joint"] = "blast joint.svg",
            ["bridge plug"] = "bridge plug.svg",
            ["bridge_plug"] = "bridge plug.svg",
            ["cement"] = "cement.svg",
            ["chemical injection mandrel"] = "chemical injection mandrel.svg",
            ["circulating sleeve"] = "circulating sleeve.svg",
            ["circulating string"] = "circulating string.svg",
            ["drain nipple pcp"] = "profile nipple.svg",
            ["drain_nipple_pcp"] = "profile nipple.svg",
            ["drill bit"] = "drill bit.svg",
            ["drill collar"] = "drill collar.svg",
            ["dual split tubing hanger"] = "tubing hanger.svg",
            ["dual tubing hanger"] = "dual tubing hanger.svg",
            ["dual_tubing_hanger"] = "dual tubing hanger.svg",
            ["entry guide"] = "guide.svg",
            ["external packer"] = "external packer.svg",
            ["fish"] = "tubing red.svg",
            ["fish and gravel pack"] = "gravel pack.svg",
            ["fish_and_gravel_pack"] = "gravel pack.svg",
            ["gas anchor"] = "gas anchor.svg",
            ["gas lift dummy"] = "gas lift dummy.svg",
            ["gas lift mandrel"] = "glm.svg",
            ["gas_lift_mandrel"] = "glm.svg",
            ["gravel pack"] = "gravel pack.svg",
            ["gun"] = "hole_tub.svg",
            ["hole"] = "hole.svg",
            ["hole in tubing string"] = "hole_tub.svg",
            ["hydraulic packer"] = "hydraulic packer.svg",
            ["icd"] = "icd.svg",
            ["jet pump"] = "jet pump.svg",
            ["landing nipple"] = "landing_nipple.svg",
            ["landing_nipple"] = "landing_nipple.svg",
            ["liner hanger"] = "liner hanger.svg",
            ["lwd"] = "lwd.svg",
            ["mandrel"] = "mandrel.svg",
            ["mud motor"] = "mud motor.svg",
            ["on off tool"] = "on-off tool.svg",
            ["on-off tool"] = "on-off tool.svg",
            ["packer"] = "packer.svg",
            ["packer dual"] = "packer dual.svg",
            ["packer hd"] = "packer HD.svg",
            ["pbr seal"] = "PBR_Seal.svg",
            ["pbr_seal"] = "PBR_Seal.svg",
            ["perf sub"] = "perf sub.svg",
            ["perforated joint"] = "perforated joint.svg",
            ["plug"] = "plug.svg",
            ["profile nipple"] = "profile nipple.svg",
            ["pup joint"] = "pup joint.svg",
            ["pup_joint"] = "pup joint.svg",
            ["reamer"] = "reamer.svg",
            ["rt to top of tubing hanger"] = "tubing green.svg",
            ["sand screen"] = "sand screen.svg",
            ["screen"] = "sand screen.svg",
            ["scssv"] = "scssvsub.svg",
            ["side pocket mandrel"] = "side pocket mandrel.svg",
            ["sliding sleeve"] = "sliding sleeve.svg",
            ["snap latch"] = "seal assembly 2.svg",
            ["snap_latch"] = "seal assembly 2.svg",
            ["stator"] = "stator.svg",
            ["stator pcp"] = "stator.svg",
            ["stator_pcp"] = "stator.svg",
            ["sub surface safety valve"] = "sub surface safety valve.svg",
            ["subsurface safety valve"] = "sub surface safety valve.svg",
            ["tool"] = "tool.svg",
            ["tubing anchor"] = "tubing anchor.svg",
            ["tubing half cut"] = "tubing HC.svg",
            ["tubing hanger"] = "tubing hanger.svg",
            ["tubing_hanger"] = "tubing hanger.svg",
            ["valve - downhole"] = "valve.svg"
        };

        private static readonly IReadOnlyList<SvgAliasRule> ContainsAliases =
        [
            new SvgAliasRule("sub surface safety valve.svg", "subsurface safety valve", "sub surface safety valve"),
            new SvgAliasRule("scssvsub.svg", "scssv"),
            new SvgAliasRule("ssv.svg", "sssv", "ssv"),
            new SvgAliasRule("electric submersible pump.svg", "electric submersible pump", "esp"),
            new SvgAliasRule("progressive gravity pump.svg", "progressive cavity pump", "progressive gravity pump", "pcp"),
            new SvgAliasRule("pump.svg", "pump"),
            new SvgAliasRule("downhole_sensor.svg", "sensor", "gauge", "monitor"),
            new SvgAliasRule("gas anchor.svg", "gas separator"),
            new SvgAliasRule("gas lift dummy.svg", "gas lift dummy"),
            new SvgAliasRule("glm.svg", "gas lift mandrel"),
            new SvgAliasRule("valve.svg", "gas lift valve", "control valve", "downhole valve", "valve"),
            new SvgAliasRule("sliding sleeve.svg", "sliding sleeve", "sleeve"),
            new SvgAliasRule("chemical injection mandrel.svg", "chemical injection mandrel"),
            new SvgAliasRule("side pocket mandrel.svg", "side pocket mandrel"),
            new SvgAliasRule("mandrel.svg", "mandrel"),
            new SvgAliasRule("hydraulic packer.svg", "hydraulic packer"),
            new SvgAliasRule("external packer.svg", "external packer"),
            new SvgAliasRule("packer.svg", "packer"),
            new SvgAliasRule("sand screen.svg", "sand screen", "screen", "liner screen"),
            new SvgAliasRule("gravel pack.svg", "gravel pack"),
            new SvgAliasRule("bridge plug.svg", "bridge plug"),
            new SvgAliasRule("landing_nipple.svg", "landing nipple"),
            new SvgAliasRule("profile nipple.svg", "profile nipple", "drain nipple"),
            new SvgAliasRule("tubing hanger.svg", "tubing hanger"),
            new SvgAliasRule("dual tubing hanger.svg", "dual tubing hanger"),
            new SvgAliasRule("guide.svg", "guide"),
            new SvgAliasRule("hole_tub.svg", "gun", "hole in tubing"),
            new SvgAliasRule("tool.svg", "tool")
        ];

        public static IEnumerable<string> ResolveAliases(WellData_Equip equipment)
        {
            if (equipment == null)
                yield break;

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var descriptors = GetDescriptors(equipment).ToList();

            foreach (var descriptor in descriptors)
            {
                if (ExactAliases.TryGetValue(descriptor, out var symbolFile) && seen.Add(symbolFile))
                    yield return symbolFile;
            }

            foreach (var rule in ContainsAliases)
            {
                if (descriptors.Any(descriptor => rule.MatchTerms.Any(term => descriptor.Contains(term, StringComparison.OrdinalIgnoreCase))) && seen.Add(rule.SymbolFile))
                    yield return rule.SymbolFile;
            }
        }

        private static IEnumerable<string> GetDescriptors(WellData_Equip equipment)
        {
            foreach (var value in new[]
            {
                equipment.EquipmentType,
                equipment.EquipmentName,
                equipment.EquipmentDescription,
                equipment.ToolTipText
            })
            {
                var normalized = NormalizeDescriptor(value);
                if (!string.IsNullOrWhiteSpace(normalized))
                    yield return normalized;
            }
        }

        private static string NormalizeDescriptor(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return string.Join(' ', value.Trim()
                .ToLowerInvariant()
                .Replace("_", " ", StringComparison.Ordinal)
                .Replace("-", " ", StringComparison.Ordinal)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }
    }
}