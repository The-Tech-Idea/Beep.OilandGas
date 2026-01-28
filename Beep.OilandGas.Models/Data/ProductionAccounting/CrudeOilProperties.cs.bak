using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class CrudeOilProperties : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the API gravity at 60°F.
        /// </summary>
        private decimal ApiGravityValue;

        public decimal ApiGravity

        {

            get { return this.ApiGravityValue; }

            set { SetProperty(ref ApiGravityValue, value); }

        }

        /// <summary>
        /// Gets or sets the sulfur content as weight percentage.
        /// </summary>
        private decimal SulfurContentValue;

        public decimal SulfurContent

        {

            get { return this.SulfurContentValue; }

            set { SetProperty(ref SulfurContentValue, value); }

        }

        /// <summary>
        /// Gets or sets the viscosity in centipoise at standard temperature.
        /// </summary>
        private decimal ViscosityValue;

        public decimal Viscosity

        {

            get { return this.ViscosityValue; }

            set { SetProperty(ref ViscosityValue, value); }

        }

        /// <summary>
        /// Gets or sets the pour point in degrees Fahrenheit.
        /// </summary>
        private decimal PourPointValue;

        public decimal PourPoint

        {

            get { return this.PourPointValue; }

            set { SetProperty(ref PourPointValue, value); }

        }

        /// <summary>
        /// Gets or sets the flash point in degrees Fahrenheit.
        /// </summary>
        private decimal FlashPointValue;

        public decimal FlashPoint

        {

            get { return this.FlashPointValue; }

            set { SetProperty(ref FlashPointValue, value); }

        }

        /// <summary>
        /// Gets or sets the water content as volume percentage.
        /// </summary>
        private decimal WaterContentValue;

        public decimal WaterContent

        {

            get { return this.WaterContentValue; }

            set { SetProperty(ref WaterContentValue, value); }

        }

        /// <summary>
        /// Gets or sets the BS&W (Basic Sediment and Water) as volume percentage.
        /// </summary>
        private decimal BSWValue;

        public decimal BSW

        {

            get { return this.BSWValue; }

            set { SetProperty(ref BSWValue, value); }

        }

        /// <summary>
        /// Gets or sets the salt content in pounds per thousand barrels (PTB).
        /// </summary>
        private decimal SaltContentValue;

        public decimal SaltContent

        {

            get { return this.SaltContentValue; }

            set { SetProperty(ref SaltContentValue, value); }

        }

        /// <summary>
        /// Gets or sets the Reid Vapor Pressure (RVP) in psi.
        /// </summary>
        private decimal? ReidVaporPressureValue;

        public decimal? ReidVaporPressure

        {

            get { return this.ReidVaporPressureValue; }

            set { SetProperty(ref ReidVaporPressureValue, value); }

        }

        /// <summary>
        /// Gets or sets the density in pounds per gallon.
        /// </summary>
        private decimal? DensityValue;

        public decimal? Density

        {

            get { return this.DensityValue; }

            set { SetProperty(ref DensityValue, value); }

        }

        /// <summary>
        /// Gets or sets the temperature at which properties were measured in degrees Fahrenheit.
        /// </summary>
        private decimal MeasurementTemperatureValue = 60m;

        public decimal MeasurementTemperature

        {

            get { return this.MeasurementTemperatureValue; }

            set { SetProperty(ref MeasurementTemperatureValue, value); }

        }

        /// <summary>
        /// Gets or sets the date of measurement.
        /// </summary>
        private DateTime MeasurementDateValue = DateTime.Now;

        public DateTime MeasurementDate

        {

            get { return this.MeasurementDateValue; }

            set { SetProperty(ref MeasurementDateValue, value); }

        }

        /// <summary>
        /// Gets the crude oil type based on API gravity.
        /// </summary>
        public CrudeOilType GetCrudeOilType()
        {
            if (ApiGravity >= 45m)
                return CrudeOilType.Condensate;
            if (ApiGravity > 31.1m)
                return CrudeOilType.Light;
            if (ApiGravity >= 22.3m)
                return CrudeOilType.Medium;
            if (ApiGravity >= 10m)
                return CrudeOilType.Heavy;
            return CrudeOilType.ExtraHeavy;
        }
    }
}
