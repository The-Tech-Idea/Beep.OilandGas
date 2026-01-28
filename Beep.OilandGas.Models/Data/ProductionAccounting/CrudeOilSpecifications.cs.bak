using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class CrudeOilSpecifications : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the minimum API gravity.
        /// </summary>
        private decimal? MinimumApiGravityValue;

        public decimal? MinimumApiGravity

        {

            get { return this.MinimumApiGravityValue; }

            set { SetProperty(ref MinimumApiGravityValue, value); }

        }

        /// <summary>
        /// Gets or sets the maximum API gravity.
        /// </summary>
        private decimal? MaximumApiGravityValue;

        public decimal? MaximumApiGravity

        {

            get { return this.MaximumApiGravityValue; }

            set { SetProperty(ref MaximumApiGravityValue, value); }

        }

        /// <summary>
        /// Gets or sets the maximum sulfur content (weight %).
        /// </summary>
        private decimal? MaximumSulfurContentValue;

        public decimal? MaximumSulfurContent

        {

            get { return this.MaximumSulfurContentValue; }

            set { SetProperty(ref MaximumSulfurContentValue, value); }

        }

        /// <summary>
        /// Gets or sets the maximum BS&W (volume %).
        /// </summary>
        private decimal? MaximumBSWValue;

        public decimal? MaximumBSW

        {

            get { return this.MaximumBSWValue; }

            set { SetProperty(ref MaximumBSWValue, value); }

        }

        /// <summary>
        /// Gets or sets the maximum water content (volume %).
        /// </summary>
        private decimal? MaximumWaterContentValue;

        public decimal? MaximumWaterContent

        {

            get { return this.MaximumWaterContentValue; }

            set { SetProperty(ref MaximumWaterContentValue, value); }

        }

        /// <summary>
        /// Gets or sets the maximum salt content (PTB).
        /// </summary>
        private decimal? MaximumSaltContentValue;

        public decimal? MaximumSaltContent

        {

            get { return this.MaximumSaltContentValue; }

            set { SetProperty(ref MaximumSaltContentValue, value); }

        }

        /// <summary>
        /// Gets or sets the maximum viscosity (centipoise).
        /// </summary>
        private decimal? MaximumViscosityValue;

        public decimal? MaximumViscosity

        {

            get { return this.MaximumViscosityValue; }

            set { SetProperty(ref MaximumViscosityValue, value); }

        }

        /// <summary>
        /// Validates crude oil properties against specifications.
        /// </summary>
        public bool Validate(CrudeOilProperties properties)
        {
            if (MinimumApiGravity.HasValue && properties.ApiGravity < MinimumApiGravity.Value)
                return false;

            if (MaximumApiGravity.HasValue && properties.ApiGravity > MaximumApiGravity.Value)
                return false;

            if (MaximumSulfurContent.HasValue && properties.SulfurContent > MaximumSulfurContent.Value)
                return false;

            if (MaximumBSW.HasValue && properties.BSW > MaximumBSW.Value)
                return false;

            if (MaximumWaterContent.HasValue && properties.WaterContent > MaximumWaterContent.Value)
                return false;

            if (MaximumSaltContent.HasValue && properties.SaltContent > MaximumSaltContent.Value)
                return false;

            if (MaximumViscosity.HasValue && properties.Viscosity > MaximumViscosity.Value)
                return false;

            return true;
        }
    }
}
