using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.FlashCalculations
{
    /// <summary>
    /// Conditions for flash calculation
    /// DTO for calculations - Entity class: FLASH_CONDITIONS
    /// </summary>
    public class FlashConditions : ModelEntityBase
    {
        /// <summary>
        /// Pressure (in specified units)
        /// </summary>
        private decimal PressureValue;

        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }

        /// <summary>
        /// Temperature (in specified units)
        /// </summary>
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }

        /// <summary>
        /// Feed composition (component mole fractions)
        /// </summary>
        private List<FlashComponent> FeedCompositionValue = new List<FlashComponent>();

        public List<FlashComponent> FeedComposition

        {

            get { return this.FeedCompositionValue; }

            set { SetProperty(ref FeedCompositionValue, value); }

        }
    }

    /// <summary>
    /// Component in flash calculation
    /// DTO for calculations - Entity class: FLASH_COMPONENT
    /// </summary>
    public class FlashComponent : ModelEntityBase
    {
        /// <summary>
        /// Component name
        /// </summary>
        private string NameValue = string.Empty;

        public string Name

        {

            get { return this.NameValue; }

            set { SetProperty(ref NameValue, value); }

        }

        /// <summary>
        /// Mole fraction in feed
        /// </summary>
        private decimal MoleFractionValue;

        public decimal MoleFraction

        {

            get { return this.MoleFractionValue; }

            set { SetProperty(ref MoleFractionValue, value); }

        }

        /// <summary>
        /// Critical temperature
        /// </summary>
        private decimal CriticalTemperatureValue;

        public decimal CriticalTemperature

        {

            get { return this.CriticalTemperatureValue; }

            set { SetProperty(ref CriticalTemperatureValue, value); }

        }

        /// <summary>
        /// Critical pressure
        /// </summary>
        private decimal CriticalPressureValue;

        public decimal CriticalPressure

        {

            get { return this.CriticalPressureValue; }

            set { SetProperty(ref CriticalPressureValue, value); }

        }

        /// <summary>
        /// Acentric factor
        /// </summary>
        private decimal AcentricFactorValue;

        public decimal AcentricFactor

        {

            get { return this.AcentricFactorValue; }

            set { SetProperty(ref AcentricFactorValue, value); }

        }

        /// <summary>
        /// Molecular weight
        /// </summary>
        private decimal MolecularWeightValue;

        public decimal MolecularWeight

        {

            get { return this.MolecularWeightValue; }

            set { SetProperty(ref MolecularWeightValue, value); }

        }
    }

    /// <summary>
    /// Alias for FlashComponent (used in some code as Component)
    /// </summary>
    public class Component : FlashComponent
    {
    }
}






