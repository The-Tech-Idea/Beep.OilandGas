using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Trading
{
    public class ExchangeReconciliationResult : ModelEntityBase
    {
        private string ExchangeContractIdValue;

        public string ExchangeContractId

        {

            get { return this.ExchangeContractIdValue; }

            set { SetProperty(ref ExchangeContractIdValue, value); }

        }
        private DateTime ReconciliationDateValue;

        public DateTime ReconciliationDate

        {

            get { return this.ReconciliationDateValue; }

            set { SetProperty(ref ReconciliationDateValue, value); }

        }
        private decimal BookReceiptVolumeValue;

        public decimal BookReceiptVolume

        {

            get { return this.BookReceiptVolumeValue; }

            set { SetProperty(ref BookReceiptVolumeValue, value); }

        }
        private decimal BookDeliveryVolumeValue;

        public decimal BookDeliveryVolume

        {

            get { return this.BookDeliveryVolumeValue; }

            set { SetProperty(ref BookDeliveryVolumeValue, value); }

        }
        private decimal PhysicalReceiptVolumeValue;

        public decimal PhysicalReceiptVolume

        {

            get { return this.PhysicalReceiptVolumeValue; }

            set { SetProperty(ref PhysicalReceiptVolumeValue, value); }

        }
        private decimal PhysicalDeliveryVolumeValue;

        public decimal PhysicalDeliveryVolume

        {

            get { return this.PhysicalDeliveryVolumeValue; }

            set { SetProperty(ref PhysicalDeliveryVolumeValue, value); }

        }
        private decimal ReceiptVarianceValue;

        public decimal ReceiptVariance

        {

            get { return this.ReceiptVarianceValue; }

            set { SetProperty(ref ReceiptVarianceValue, value); }

        }
        private decimal DeliveryVarianceValue;

        public decimal DeliveryVariance

        {

            get { return this.DeliveryVarianceValue; }

            set { SetProperty(ref DeliveryVarianceValue, value); }

        }
        private bool RequiresAdjustmentValue;

        public bool RequiresAdjustment

        {

            get { return this.RequiresAdjustmentValue; }

            set { SetProperty(ref RequiresAdjustmentValue, value); }

        }
    }
}
