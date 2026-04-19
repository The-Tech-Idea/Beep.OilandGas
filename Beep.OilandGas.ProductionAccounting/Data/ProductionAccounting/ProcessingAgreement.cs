using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class ProcessingAgreement : ModelEntityBase
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
        /// Gets or sets the processor company.
        /// </summary>
        private string ProcessorValue = string.Empty;

        public string Processor

        {

            get { return this.ProcessorValue; }

            set { SetProperty(ref ProcessorValue, value); }

        }

        /// <summary>
        /// Gets or sets the processing facility.
        /// </summary>
        private string ProcessingFacilityValue = string.Empty;

        public string ProcessingFacility

        {

            get { return this.ProcessingFacilityValue; }

            set { SetProperty(ref ProcessingFacilityValue, value); }

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
        /// Gets or sets the processing fee per barrel.
        /// </summary>
        private decimal ProcessingFeeValue;

        public decimal ProcessingFee

        {

            get { return this.ProcessingFeeValue; }

            set { SetProperty(ref ProcessingFeeValue, value); }

        }

        /// <summary>
        /// Gets or sets the processing percentage (if percentage-based).
        /// </summary>
        private decimal? ProcessingPercentageValue;

        public decimal? ProcessingPercentage

        {

            get { return this.ProcessingPercentageValue; }

            set { SetProperty(ref ProcessingPercentageValue, value); }

        }
    }
}
