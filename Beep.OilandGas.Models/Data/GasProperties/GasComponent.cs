namespace Beep.OilandGas.Models.Data.GasProperties
{
    /// <summary>
    /// Represents a gas component in a mixture
    /// DTO for calculations - Entity class: GAS_COMPONENT
    /// </summary>
    public class GasComponent : ModelEntityBase
    {
        /// <summary>
        /// Component name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mole fraction in mixture
        /// </summary>
        public decimal MoleFraction { get; set; }

        /// <summary>
        /// Molecular weight in lb/lbmol
        /// </summary>
        public decimal MolecularWeight { get; set; }

        /// <summary>
        /// Critical pressure in psia
        /// </summary>
        public decimal CriticalPressure { get; set; }

        /// <summary>
        /// Critical temperature in Rankine
        /// </summary>
        public decimal CriticalTemperature { get; set; }
    }
}



