using System.Collections.Generic;

namespace Beep.OilandGas.Models.FlashCalculations
{
    /// <summary>
    /// Conditions for flash calculation
    /// DTO for calculations - Entity class: FLASH_CONDITIONS
    /// </summary>
    public class FlashConditions
    {
        /// <summary>
        /// Pressure (in specified units)
        /// </summary>
        public decimal Pressure { get; set; }

        /// <summary>
        /// Temperature (in specified units)
        /// </summary>
        public decimal Temperature { get; set; }

        /// <summary>
        /// Feed composition (component mole fractions)
        /// </summary>
        public List<FlashComponent> FeedComposition { get; set; } = new List<FlashComponent>();
    }

    /// <summary>
    /// Component in flash calculation
    /// DTO for calculations - Entity class: FLASH_COMPONENT
    /// </summary>
    public class FlashComponent
    {
        /// <summary>
        /// Component name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mole fraction in feed
        /// </summary>
        public decimal MoleFraction { get; set; }

        /// <summary>
        /// Critical temperature
        /// </summary>
        public decimal CriticalTemperature { get; set; }

        /// <summary>
        /// Critical pressure
        /// </summary>
        public decimal CriticalPressure { get; set; }

        /// <summary>
        /// Acentric factor
        /// </summary>
        public decimal AcentricFactor { get; set; }

        /// <summary>
        /// Molecular weight
        /// </summary>
        public decimal MolecularWeight { get; set; }
    }

    /// <summary>
    /// Alias for FlashComponent (used in some code as Component)
    /// </summary>
    public class Component : FlashComponent
    {
    }
}



