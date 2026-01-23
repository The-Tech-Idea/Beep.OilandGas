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
        private string NameValue = string.Empty;

        public string Name

        {

            get { return this.NameValue; }

            set { SetProperty(ref NameValue, value); }

        }

        /// <summary>
        /// Mole fraction in mixture
        /// </summary>
        private decimal MoleFractionValue;

        public decimal MoleFraction

        {

            get { return this.MoleFractionValue; }

            set { SetProperty(ref MoleFractionValue, value); }

        }

        /// <summary>
        /// Molecular weight in lb/lbmol
        /// </summary>
        private decimal MolecularWeightValue;

        public decimal MolecularWeight

        {

            get { return this.MolecularWeightValue; }

            set { SetProperty(ref MolecularWeightValue, value); }

        }

        /// <summary>
        /// Critical pressure in psia
        /// </summary>
        private decimal CriticalPressureValue;

        public decimal CriticalPressure

        {

            get { return this.CriticalPressureValue; }

            set { SetProperty(ref CriticalPressureValue, value); }

        }

        /// <summary>
        /// Critical temperature in Rankine
        /// </summary>
        private decimal CriticalTemperatureValue;

        public decimal CriticalTemperature

        {

            get { return this.CriticalTemperatureValue; }

            set { SetProperty(ref CriticalTemperatureValue, value); }

        }
    }
}





