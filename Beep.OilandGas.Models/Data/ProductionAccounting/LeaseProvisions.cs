using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class LeaseProvisions : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the delay rental amount per acre per year.
        /// </summary>
        private decimal? DelayRentalPerAcreValue;

        public decimal? DelayRentalPerAcre

        {

            get { return this.DelayRentalPerAcreValue; }

            set { SetProperty(ref DelayRentalPerAcreValue, value); }

        }

        /// <summary>
        /// Gets or sets the shut-in royalty amount.
        /// </summary>
        private decimal? ShutInRoyaltyValue;

        public decimal? ShutInRoyalty

        {

            get { return this.ShutInRoyaltyValue; }

            set { SetProperty(ref ShutInRoyaltyValue, value); }

        }

        /// <summary>
        /// Gets or sets the minimum royalty amount.
        /// </summary>
        private decimal? MinimumRoyaltyValue;

        public decimal? MinimumRoyalty

        {

            get { return this.MinimumRoyaltyValue; }

            set { SetProperty(ref MinimumRoyaltyValue, value); }

        }

        /// <summary>
        /// Gets or sets whether pooling is allowed.
        /// </summary>
        private bool AllowPoolingValue;

        public bool AllowPooling

        {

            get { return this.AllowPoolingValue; }

            set { SetProperty(ref AllowPoolingValue, value); }

        }

        /// <summary>
        /// Gets or sets whether unitization is allowed.
        /// </summary>
        private bool AllowUnitizationValue;

        public bool AllowUnitization

        {

            get { return this.AllowUnitizationValue; }

            set { SetProperty(ref AllowUnitizationValue, value); }

        }

        /// <summary>
        /// Gets or sets the force majeure provisions.
        /// </summary>
        private string? ForceMajeureProvisionsValue;

        public string? ForceMajeureProvisions

        {

            get { return this.ForceMajeureProvisionsValue; }

            set { SetProperty(ref ForceMajeureProvisionsValue, value); }

        }

        /// <summary>
        /// Gets or sets the assignment provisions.
        /// </summary>
        private string? AssignmentProvisionsValue;

        public string? AssignmentProvisions

        {

            get { return this.AssignmentProvisionsValue; }

            set { SetProperty(ref AssignmentProvisionsValue, value); }

        }

        /// <summary>
        /// Gets or sets whether the lease is held by production (HBP).
        /// </summary>
        private bool IsHeldByProductionValue;

        public bool IsHeldByProduction

        {

            get { return this.IsHeldByProductionValue; }

            set { SetProperty(ref IsHeldByProductionValue, value); }

        }

        /// <summary>
        /// Gets or sets the date the lease became HBP.
        /// </summary>
        private DateTime? HeldByProductionDateValue;

        public DateTime? HeldByProductionDate

        {

            get { return this.HeldByProductionDateValue; }

            set { SetProperty(ref HeldByProductionDateValue, value); }

        }
    }
}
