namespace Beep.OilandGas.Models.Data.NodalAnalysis
{
    /// <summary>
    /// Represents lateral branch properties for multilateral well calculations.
    /// </summary>
    public class LateralBranch : ModelEntityBase
    {
        /// <summary>
        /// Branch name or identifier.
        /// </summary>
        private string? NameValue;

        public string? Name

        {

            get { return this.NameValue; }

            set { SetProperty(ref NameValue, value); }

        }

        /// <summary>
        /// Reservoir pressure at the branch in psia.
        /// </summary>
        private double ReservoirPressureValue;

        public double ReservoirPressure

        {

            get { return this.ReservoirPressureValue; }

            set { SetProperty(ref ReservoirPressureValue, value); }

        }

        /// <summary>
        /// Productivity index for the branch in bbl/day/psi.
        /// </summary>
        private double ProductivityIndexValue;

        public double ProductivityIndex

        {

            get { return this.ProductivityIndexValue; }

            set { SetProperty(ref ProductivityIndexValue, value); }

        }

        /// <summary>
        /// Permeability of the formation in md.
        /// </summary>
        private double PermeabilityValue;

        public double Permeability

        {

            get { return this.PermeabilityValue; }

            set { SetProperty(ref PermeabilityValue, value); }

        }

        /// <summary>
        /// Formation thickness in feet.
        /// </summary>
        private double FormationThicknessValue;

        public double FormationThickness

        {

            get { return this.FormationThicknessValue; }

            set { SetProperty(ref FormationThicknessValue, value); }

        }

        /// <summary>
        /// Drainage radius in feet.
        /// </summary>
        private double DrainageRadiusValue;

        public double DrainageRadius

        {

            get { return this.DrainageRadiusValue; }

            set { SetProperty(ref DrainageRadiusValue, value); }

        }

        /// <summary>
        /// Wellbore radius in feet.
        /// </summary>
        private double WellboreRadiusValue;

        public double WellboreRadius

        {

            get { return this.WellboreRadiusValue; }

            set { SetProperty(ref WellboreRadiusValue, value); }

        }

        /// <summary>
        /// Skin factor (dimensionless).
        /// </summary>
        private double SkinFactorValue;

        public double SkinFactor

        {

            get { return this.SkinFactorValue; }

            set { SetProperty(ref SkinFactorValue, value); }

        }
    }
}

