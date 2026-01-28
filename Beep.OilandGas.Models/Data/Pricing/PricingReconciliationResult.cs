using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Pricing
{
    public class PricingReconciliationResult : ModelEntityBase
    {
        private string ReconciliationIdValue;

        public string ReconciliationId

        {

            get { return this.ReconciliationIdValue; }

            set { SetProperty(ref ReconciliationIdValue, value); }

        }
        private string RunTicketNumberValue;

        public string RunTicketNumber

        {

            get { return this.RunTicketNumberValue; }

            set { SetProperty(ref RunTicketNumberValue, value); }

        }
        private decimal ExpectedValueValue;

        public decimal ExpectedValue

        {

            get { return this.ExpectedValueValue; }

            set { SetProperty(ref ExpectedValueValue, value); }

        }
        private decimal ActualValueValue;

        public decimal ActualValue

        {

            get { return this.ActualValueValue; }

            set { SetProperty(ref ActualValueValue, value); }

        }
        private decimal VarianceValue;

        public decimal Variance

        {

            get { return this.VarianceValue; }

            set { SetProperty(ref VarianceValue, value); }

        }
        private bool IsReconciledValue;

        public bool IsReconciled

        {

            get { return this.IsReconciledValue; }

            set { SetProperty(ref IsReconciledValue, value); }

        }
        private DateTime ReconciliationDateValue = DateTime.UtcNow;

        public DateTime ReconciliationDate

        {

            get { return this.ReconciliationDateValue; }

            set { SetProperty(ref ReconciliationDateValue, value); }

        }
    }
}
