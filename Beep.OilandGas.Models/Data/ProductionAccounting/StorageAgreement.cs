using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class StorageAgreement : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the agreement identifier.
        /// </summary>
        private string AgreementIdValue = string.Empty;

        public string AgreementId

        {

            get { return this.AgreementIdValue; }

            set { SetProperty(ref AgreementIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the storage facility.
        /// </summary>
        private string StorageFacilityValue = string.Empty;

        public string StorageFacility

        {

            get { return this.StorageFacilityValue; }

            set { SetProperty(ref StorageFacilityValue, value); }

        }

        /// <summary>
        /// Gets or sets the effective date.
        /// </summary>
        private DateTime EffectiveDateValue;

        public DateTime EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the storage fee per barrel per month.
        /// </summary>
        private decimal StorageFeeValue;

        public decimal StorageFee

        {

            get { return this.StorageFeeValue; }

            set { SetProperty(ref StorageFeeValue, value); }

        }

        /// <summary>
        /// Gets or sets the reserved capacity (barrels).
        /// </summary>
        private decimal? ReservedCapacityValue;

        public decimal? ReservedCapacity

        {

            get { return this.ReservedCapacityValue; }

            set { SetProperty(ref ReservedCapacityValue, value); }

        }
    }
}
