namespace Beep.OilandGas.Models.OilProperties
{
    /// <summary>
    /// Represents oil properties at given conditions
    /// DTO for calculations - Entity class: OIL_PROPERTY_RESULT
    /// </summary>
    public class OilPropertyResult
    {
        /// <summary>
        /// Oil density in lb/ft³
        /// </summary>
        public decimal Density { get; set; }

        /// <summary>
        /// Oil specific gravity
        /// </summary>
        public decimal SpecificGravity { get; set; }

        /// <summary>
        /// Oil API gravity
        /// </summary>
        public decimal ApiGravity { get; set; }

        /// <summary>
        /// Oil viscosity in cp
        /// </summary>
        public decimal Viscosity { get; set; }

        /// <summary>
        /// Oil formation volume factor (Bo) in bbl/STB
        /// </summary>
        public decimal FormationVolumeFactor { get; set; }

        /// <summary>
        /// Solution gas-oil ratio (Rs) in scf/STB
        /// </summary>
        public decimal SolutionGasOilRatio { get; set; }

        /// <summary>
        /// Oil compressibility in 1/psi
        /// </summary>
        public decimal Compressibility { get; set; }
    }
}



