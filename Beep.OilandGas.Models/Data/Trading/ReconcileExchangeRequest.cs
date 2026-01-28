using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Trading
{
    public class ReconcileExchangeRequest : ModelEntityBase
    {
        private DateTime ReconciliationDateValue;

        [Required]
        public DateTime ReconciliationDate

        {

            get { return this.ReconciliationDateValue; }

            set { SetProperty(ref ReconciliationDateValue, value); }

        }
        private DateTime? PeriodStartValue;

        public DateTime? PeriodStart

        {

            get { return this.PeriodStartValue; }

            set { SetProperty(ref PeriodStartValue, value); }

        }
        private DateTime? PeriodEndValue;

        public DateTime? PeriodEnd

        {

            get { return this.PeriodEndValue; }

            set { SetProperty(ref PeriodEndValue, value); }

        }
        private string? CounterpartyStatementIdValue;

        public string? CounterpartyStatementId

        {

            get { return this.CounterpartyStatementIdValue; }

            set { SetProperty(ref CounterpartyStatementIdValue, value); }

        }
    }
}
