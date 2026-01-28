
namespace Beep.OilandGas.Models.Data.NodalAnalysis
{
    public class ReservoirProperties : ModelEntityBase
    {
        /// <summary>
        /// Reservoir pressure (psia or kPa)
        /// </summary>
        private double ReservoirPressureValue;

        public double ReservoirPressure

        {

            get { return this.ReservoirPressureValue; }

            set { SetProperty(ref ReservoirPressureValue, value); }

        }

        /// <summary>
        /// Bubble point pressure (psia or kPa)
        /// </summary>
        private double BubblePointPressureValue;

        public double BubblePointPressure

        {

            get { return this.BubblePointPressureValue; }

            set { SetProperty(ref BubblePointPressureValue, value); }

        }

        /// <summary>
        /// Productivity index (STB/day/psi or m³/day/kPa)
        /// </summary>
        private double ProductivityIndexValue;

        public double ProductivityIndex

        {

            get { return this.ProductivityIndexValue; }

            set { SetProperty(ref ProductivityIndexValue, value); }

        }

        /// <summary>
        /// Water cut (fraction, 0 to 1)
        /// </summary>
        private double WaterCutValue;

        public double WaterCut

        {

            get { return this.WaterCutValue; }

            set { SetProperty(ref WaterCutValue, value); }

        }

        /// <summary>
        /// Gas-oil ratio (scf/STB or m³/m³)
        /// </summary>
        private double GasOilRatioValue;

        public double GasOilRatio

        {

            get { return this.GasOilRatioValue; }

            set { SetProperty(ref GasOilRatioValue, value); }

        }

        /// <summary>
        /// Oil gravity (API degrees)
        /// </summary>
        private double OilGravityValue;

        public double OilGravity

        {

            get { return this.OilGravityValue; }

            set { SetProperty(ref OilGravityValue, value); }

        }

        /// <summary>
        /// Formation volume factor (rbbl/STB or m³/m³)
        /// </summary>
        private double FormationVolumeFactorValue;

        public double FormationVolumeFactor

        {

            get { return this.FormationVolumeFactorValue; }

            set { SetProperty(ref FormationVolumeFactorValue, value); }

        }

        /// <summary>
        /// Oil viscosity (cp)
        /// </summary>
        private double OilViscosityValue;

        public double OilViscosity

        {

            get { return this.OilViscosityValue; }

            set { SetProperty(ref OilViscosityValue, value); }

        }
    }
}
