using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.Models.Constants
{
    /// <summary>
    /// Standard critical properties and physical constants for common oil &amp; gas components.
    /// Sources: GPSA Engineering Data Book (14th ed.), DIPPR 801, NIST REFPROP.
    ///
    /// Used by FlashCalculations, GasProperties, OilProperties, and Properties projects.
    /// Previously each project estimated these from simple correlations (M-7).
    /// </summary>
    public static class ComponentDatabase
    {
        /// <summary>Critical properties for a pure component.</summary>
        public readonly struct ComponentProperties
        {
            public readonly string Name;
            public readonly string Formula;
            public readonly decimal MolecularWeight;       // lb/lbmol
            public readonly decimal CriticalTemperature;    // °R
            public readonly decimal CriticalPressure;       // psia
            public readonly decimal AcentricFactor;         // dimensionless
            public readonly decimal CriticalVolume;         // ft³/lbmol
            public readonly decimal BoilingPoint;           // °R at 1 atm
            public readonly bool IsHydrocarbon;

            internal ComponentProperties(string name, string formula, decimal mw, decimal tc, decimal pc,
                decimal omega, decimal vc, decimal tb, bool isHC)
            {
                Name = name; Formula = formula; MolecularWeight = mw;
                CriticalTemperature = tc; CriticalPressure = pc; AcentricFactor = omega;
                CriticalVolume = vc; BoilingPoint = tb; IsHydrocarbon = isHC;
            }
        }

        /// <summary>All standard components indexed by name (case-insensitive).</summary>
        public static readonly IReadOnlyDictionary<string, ComponentProperties> All;

        /// <summary>Hydrocarbon components only.</summary>
        public static readonly IReadOnlyList<ComponentProperties> Hydrocarbons;

        /// <summary>Non-hydrocarbon components (N2, CO2, H2S, H2O, etc.).</summary>
        public static readonly IReadOnlyList<ComponentProperties> NonHydrocarbons;

        /// <summary>Try to get properties by component name (case-insensitive).</summary>
        public static bool TryGet(string name, out ComponentProperties props)
            => All.TryGetValue(name, out props);

        /// <summary>Get properties or null if not found.</summary>
        public static ComponentProperties? Get(string name)
            => All.TryGetValue(name, out var p) ? p : null;

        static ComponentDatabase()
        {
            var components = new List<ComponentProperties>
            {
                // ── Normal Alkanes ──────────────────────────────────────────
                new("Methane",       "CH4",    16.043m,  343.0m,  667.8m,  0.0115m,  1.59m,  201.0m, true),
                new("Ethane",        "C2H6",   30.070m,  549.8m,  707.8m,  0.0995m,  2.37m,  332.2m, true),
                new("Propane",       "C3H8",   44.097m,  665.7m,  616.3m,  0.1523m,  3.20m,  416.0m, true),
                new("n-Butane",      "n-C4H10",58.123m,  765.3m,  550.7m,  0.2002m,  4.08m,  490.8m, true),
                new("i-Butane",      "i-C4H10",58.123m,  734.7m,  529.1m,  0.1848m,  4.21m,  470.5m, true),
                new("n-Pentane",     "n-C5H12",72.150m,  845.4m,  488.8m,  0.2515m,  4.87m,  556.9m, true),
                new("i-Pentane",     "i-C5H12",72.150m,  828.8m,  490.4m,  0.2274m,  4.90m,  541.8m, true),
                new("n-Hexane",      "n-C6H14",86.177m,  913.4m,  436.9m,  0.3013m,  5.89m,  615.3m, true),
                new("n-Heptane",     "n-C7H16",100.204m, 972.4m,  396.8m,  0.3495m,  6.68m,  668.6m, true),
                new("n-Octane",      "n-C8H18",114.231m, 1024.0m, 360.6m,  0.3984m,  7.71m,  717.8m, true),
                new("n-Nonane",      "n-C9H20",128.258m, 1070.1m, 328.1m,  0.4473m,  8.67m,  763.1m, true),
                new("n-Decane",      "n-C10H22",142.285m,1111.5m, 304.1m,  0.4902m,  9.63m,  805.5m, true),

                // ── Branched / Cyclic Hydrocarbons ─────────────────────────
                new("Cyclohexane",   "C6H12",  84.161m,  996.3m,  590.0m,  0.2118m,  4.92m,  636.1m, true),
                new("Benzene",       "C6H6",   78.114m, 1012.1m,  714.1m,  0.2100m,  4.17m,  636.8m, true),
                new("Toluene",       "C7H8",   92.141m, 1065.3m,  595.5m,  0.2620m,  5.06m,  690.5m, true),
                new("Ethylbenzene",  "C8H10",  106.167m,1108.7m,  523.2m,  0.3030m,  5.94m,  740.0m, true),
                new("o-Xylene",      "C8H10",  106.167m,1134.2m,  542.0m,  0.3100m,  5.93m,  751.4m, true),

                // ── Olefins ─────────────────────────────────────────────────
                new("Ethylene",      "C2H4",   28.054m,  508.3m,  742.1m,  0.0866m,  2.10m,  305.3m, true),
                new("Propylene",     "C3H6",   42.081m,  656.0m,  670.2m,  0.1424m,  2.85m,  406.5m, true),

                // ── Non-Hydrocarbons ───────────────────────────────────────
                new("Nitrogen",      "N2",     28.013m,  227.2m,  492.8m,  0.0372m,  1.44m,  139.3m, false),
                new("CarbonDioxide", "CO2",    44.010m,  547.6m, 1071.0m,  0.2250m,  1.51m,  350.0m, false),
                new("HydrogenSulfide","H2S",   34.082m,  672.1m, 1300.0m,  0.0948m,  1.58m,  382.5m, false),
                new("Water",         "H2O",    18.015m, 1165.0m, 3200.1m,  0.3443m,  0.90m,  671.6m, false),
                new("Oxygen",        "O2",     31.999m,  278.2m,  731.4m,  0.0222m,  1.21m,  162.5m, false),
                new("Hydrogen",      "H2",      2.016m,   59.4m,  188.1m, -0.2180m,  1.04m,   36.5m, false),
                new("Helium",        "He",      4.003m,    9.3m,   33.0m, -0.3870m,  0.93m,    7.7m, false),
                new("Argon",         "Ar",     39.948m,  271.4m,  710.4m, -0.0022m,  1.19m,  157.0m, false),

                // ── Mercaptans / Sulfur Compounds ──────────────────────────
                new("MethylMercaptan","CH3SH", 48.109m,  885.0m,  935.5m,  0.1589m,  2.45m,  476.0m, false),
                new("EthylMercaptan", "C2H5SH",62.136m,  945.0m,  729.5m,  0.1900m,  3.10m,  528.0m, false),

                // ── Air / Pseudo-components ─────────────────────────────────
                new("Air",           "Air",    28.964m,  238.5m,  547.4m,  0.0330m,  1.45m,  143.4m, false),
            };

            All = components.ToDictionary(c => c.Name, System.StringComparer.OrdinalIgnoreCase);
            Hydrocarbons = components.Where(c => c.IsHydrocarbon).ToList();
            NonHydrocarbons = components.Where(c => !c.IsHydrocarbon).ToList();
        }
    }
}
