using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Trading
{
    public class CreateExchangeCommitmentRequest : ModelEntityBase
    {
        private string ExchangeContractIdValue;

        public string ExchangeContractId

        {

            get { return this.ExchangeContractIdValue; }

            set { SetProperty(ref ExchangeContractIdValue, value); }

        }
        private string CommitmentTypeValue;

        public string CommitmentType

        {

            get { return this.CommitmentTypeValue; }

            set { SetProperty(ref CommitmentTypeValue, value); }

        }
        private decimal CommittedVolumeValue;

        public decimal CommittedVolume

        {

            get { return this.CommittedVolumeValue; }

            set { SetProperty(ref CommittedVolumeValue, value); }

        }
        private DateTime DeliveryPeriodStartValue;

        public DateTime DeliveryPeriodStart

        {

            get { return this.DeliveryPeriodStartValue; }

            set { SetProperty(ref DeliveryPeriodStartValue, value); }

        }
        private DateTime DeliveryPeriodEndValue;

        public DateTime DeliveryPeriodEnd

        {

            get { return this.DeliveryPeriodEndValue; }

            set { SetProperty(ref DeliveryPeriodEndValue, value); }

        }
    }
}
