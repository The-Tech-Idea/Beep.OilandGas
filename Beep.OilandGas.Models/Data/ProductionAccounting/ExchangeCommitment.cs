using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class ExchangeCommitment : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the commitment identifier.
        /// </summary>
        private string CommitmentIdValue = string.Empty;

        public string CommitmentId

        {

            get { return this.CommitmentIdValue; }

            set { SetProperty(ref CommitmentIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the contract reference.
        /// </summary>
        private string ContractIdValue = string.Empty;

        public string ContractId

        {

            get { return this.ContractIdValue; }

            set { SetProperty(ref ContractIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the commitment type.
        /// </summary>
        private ExchangeCommitmentType CommitmentTypeValue;

        public ExchangeCommitmentType CommitmentType

        {

            get { return this.CommitmentTypeValue; }

            set { SetProperty(ref CommitmentTypeValue, value); }

        }

        /// <summary>
        /// Gets or sets the committed volume in barrels.
        /// </summary>
        private decimal CommittedVolumeValue;

        public decimal CommittedVolume

        {

            get { return this.CommittedVolumeValue; }

            set { SetProperty(ref CommittedVolumeValue, value); }

        }

        /// <summary>
        /// Gets or sets the delivery period start date.
        /// </summary>
        private DateTime DeliveryPeriodStartValue;

        public DateTime DeliveryPeriodStart

        {

            get { return this.DeliveryPeriodStartValue; }

            set { SetProperty(ref DeliveryPeriodStartValue, value); }

        }

        /// <summary>
        /// Gets or sets the delivery period end date.
        /// </summary>
        private DateTime DeliveryPeriodEndValue;

        public DateTime DeliveryPeriodEnd

        {

            get { return this.DeliveryPeriodEndValue; }

            set { SetProperty(ref DeliveryPeriodEndValue, value); }

        }

        /// <summary>
        /// Gets or sets the actual volume delivered in barrels.
        /// </summary>
        private decimal ActualVolumeDeliveredValue;

        public decimal ActualVolumeDelivered

        {

            get { return this.ActualVolumeDeliveredValue; }

            set { SetProperty(ref ActualVolumeDeliveredValue, value); }

        }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        private ExchangeCommitmentStatus StatusValue = ExchangeCommitmentStatus.Pending;

        public ExchangeCommitmentStatus Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }

        /// <summary>
        /// Gets the remaining commitment.
        /// </summary>
        public decimal RemainingCommitment => CommittedVolume - ActualVolumeDelivered;

        /// <summary>
        /// Gets the fulfillment percentage.
        /// </summary>
        public decimal FulfillmentPercentage => CommittedVolume > 0
            ? (ActualVolumeDelivered / CommittedVolume) * 100m
            : 0m;
    }
}
