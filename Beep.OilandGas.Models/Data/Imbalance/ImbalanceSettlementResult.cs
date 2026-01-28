using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Imbalance
{
    public class ImbalanceSettlementResult : ModelEntityBase
    {
        private string SettlementIdValue;

        public string SettlementId

        {

            get { return this.SettlementIdValue; }

            set { SetProperty(ref SettlementIdValue, value); }

        }
        private string ImbalanceIdValue;

        public string ImbalanceId

        {

            get { return this.ImbalanceIdValue; }

            set { SetProperty(ref ImbalanceIdValue, value); }

        }
        private DateTime SettlementDateValue;

        public DateTime SettlementDate

        {

            get { return this.SettlementDateValue; }

            set { SetProperty(ref SettlementDateValue, value); }

        }
        private decimal SettlementAmountValue;

        public decimal SettlementAmount

        {

            get { return this.SettlementAmountValue; }

            set { SetProperty(ref SettlementAmountValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string SettledByValue;

        public string SettledBy

        {

            get { return this.SettledByValue; }

            set { SetProperty(ref SettledByValue, value); }

        }
    }
}
