namespace Beep.OilandGas.ProductionAccounting.Constants
{
    /// <summary>
    /// Allocation method constants.
    /// </summary>
    public static class AllocationMethods
    {
        /// <summary>Pro Rata allocation by ownership percentage.</summary>
        public const string ProRata = "ProRata";

        /// <summary>Equation-based allocation (custom formula).</summary>
        public const string Equation = "Equation";

        /// <summary>Volumetric allocation by well/tract volume.</summary>
        public const string Volumetric = "Volumetric";

        /// <summary>Yield-based allocation (reserve-weighted).</summary>
        public const string Yield = "Yield";

        /// <summary>Get all valid allocation methods.</summary>
        public static string[] AllMethods => new[] { ProRata, Equation, Volumetric, Yield };
    }
}
