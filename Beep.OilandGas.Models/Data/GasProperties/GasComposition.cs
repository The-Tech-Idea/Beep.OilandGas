using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.GasProperties
{
    public class GasComposition : ModelEntityBase
    {
        /// <summary>
        /// Mole fraction of methane (C1)
        /// </summary>
        private decimal MethaneFractionValue;

        public decimal MethaneFraction

        {

            get { return this.MethaneFractionValue; }

            set { SetProperty(ref MethaneFractionValue, value); }

        }

        /// <summary>
        /// Mole fraction of ethane (C2)
        /// </summary>
        private decimal EthaneFractionValue;

        public decimal EthaneFraction

        {

            get { return this.EthaneFractionValue; }

            set { SetProperty(ref EthaneFractionValue, value); }

        }

        /// <summary>
        /// Mole fraction of propane (C3)
        /// </summary>
        private decimal PropaneFractionValue;

        public decimal PropaneFraction

        {

            get { return this.PropaneFractionValue; }

            set { SetProperty(ref PropaneFractionValue, value); }

        }

        /// <summary>
        /// Mole fraction of i-butane (iC4)
        /// </summary>
        private decimal IButaneFractionValue;

        public decimal IButaneFraction

        {

            get { return this.IButaneFractionValue; }

            set { SetProperty(ref IButaneFractionValue, value); }

        }

        /// <summary>
        /// Mole fraction of n-butane (nC4)
        /// </summary>
        private decimal NButaneFractionValue;

        public decimal NButaneFraction

        {

            get { return this.NButaneFractionValue; }

            set { SetProperty(ref NButaneFractionValue, value); }

        }

        /// <summary>
        /// Mole fraction of i-pentane (iC5)
        /// </summary>
        private decimal IPentaneFractionValue;

        public decimal IPentaneFraction

        {

            get { return this.IPentaneFractionValue; }

            set { SetProperty(ref IPentaneFractionValue, value); }

        }

        /// <summary>
        /// Mole fraction of n-pentane (nC5)
        /// </summary>
        private decimal NPentaneFractionValue;

        public decimal NPentaneFraction

        {

            get { return this.NPentaneFractionValue; }

            set { SetProperty(ref NPentaneFractionValue, value); }

        }

        /// <summary>
        /// Mole fraction of hexane plus (C6+)
        /// </summary>
        private decimal HexanePlusFractionValue;

        public decimal HexanePlusFraction

        {

            get { return this.HexanePlusFractionValue; }

            set { SetProperty(ref HexanePlusFractionValue, value); }

        }

        /// <summary>
        /// Mole fraction of nitrogen (N2)
        /// </summary>
        private decimal NitrogenFractionValue;

        public decimal NitrogenFraction

        {

            get { return this.NitrogenFractionValue; }

            set { SetProperty(ref NitrogenFractionValue, value); }

        }

        /// <summary>
        /// Mole fraction of carbon dioxide (CO2)
        /// </summary>
        private decimal CarbonDioxideFractionValue;

        public decimal CarbonDioxideFraction

        {

            get { return this.CarbonDioxideFractionValue; }

            set { SetProperty(ref CarbonDioxideFractionValue, value); }

        }

        /// <summary>
        /// Mole fraction of hydrogen sulfide (H2S)
        /// </summary>
        private decimal HydrogenSulfideFractionValue;

        public decimal HydrogenSulfideFraction

        {

            get { return this.HydrogenSulfideFractionValue; }

            set { SetProperty(ref HydrogenSulfideFractionValue, value); }

        }

        public decimal SpecificGravity { get; set; }
        public decimal MolecularWeight { get; set; }
        public string CompositionId { get; set; }
        public string CompositionName { get; set; }
        public DateTime CompositionDate { get; set; }
        public decimal TotalMoleFraction { get; set; }

        /// <summary>
        /// Validates that all fractions sum to 1.0 (within tolerance)
        /// </summary>
        public bool IsValid(decimal tolerance = 0.01m)
        {
            decimal total = MethaneFraction + EthaneFraction + PropaneFraction +
                           IButaneFraction + NButaneFraction + IPentaneFraction +
                           NPentaneFraction + HexanePlusFraction + NitrogenFraction +
                           CarbonDioxideFraction + HydrogenSulfideFraction;

            return Math.Abs(total - 1.0m) <= tolerance;
        }
    }
}
