using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Trading
{
    public partial class ExchangeStatement : ModelEntityBase
    {
        private System.String StatementIdValue;
        public System.String StatementId
        {
            get { return this.StatementIdValue; }
            set { SetProperty(ref StatementIdValue, value); }
        }

        private System.DateTime StatementPeriodStartValue;
        public System.DateTime StatementPeriodStart
        {
            get { return this.StatementPeriodStartValue; }
            set { SetProperty(ref StatementPeriodStartValue, value); }
        }

        private System.DateTime StatementPeriodEndValue;
        public System.DateTime StatementPeriodEnd
        {
            get { return this.StatementPeriodEndValue; }
            set { SetProperty(ref StatementPeriodEndValue, value); }
        }

        private System.String ContractIdValue;
        public System.String ContractId
        {
            get { return this.ContractIdValue; }
            set { SetProperty(ref ContractIdValue, value); }
        }

        private List<EXCHANGE_TRANSACTION> TransactionsValue = new List<EXCHANGE_TRANSACTION>();
        public List<EXCHANGE_TRANSACTION> Transactions
        {
            get { return this.TransactionsValue; }
            set { SetProperty(ref TransactionsValue, value); }
        }

        private ExchangeSummary ReceiptsValue = new ExchangeSummary();
        public ExchangeSummary Receipts
        {
            get { return this.ReceiptsValue; }
            set { SetProperty(ref ReceiptsValue, value); }
        }

        private ExchangeSummary DeliveriesValue = new ExchangeSummary();
        public ExchangeSummary Deliveries
        {
            get { return this.DeliveriesValue; }
            set { SetProperty(ref DeliveriesValue, value); }
        }

        private ExchangeNetPosition NetPositionValue = new ExchangeNetPosition();
        public ExchangeNetPosition NetPosition
        {
            get { return this.NetPositionValue; }
            set { SetProperty(ref NetPositionValue, value); }
        }
    }
}
