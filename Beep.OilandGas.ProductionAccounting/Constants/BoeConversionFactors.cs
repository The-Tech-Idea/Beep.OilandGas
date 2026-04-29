namespace Beep.OilandGas.ProductionAccounting.Constants
{
    /// <summary>Rough oil–gas energy equivalence for BOE-style rollups (compatibility helpers and quick views).</summary>
    public static class BoeConversionFactors
    {
        /// <summary>Common rule-of-thumb divisor: Mcf gas per barrel oil equivalent (~6:1).</summary>
        public const decimal StandardMcfPerOilBarrelEquivalent = 6m;
    }
}
