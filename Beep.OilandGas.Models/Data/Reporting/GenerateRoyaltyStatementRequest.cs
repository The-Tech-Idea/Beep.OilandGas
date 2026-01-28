using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Reporting
{
    public class GenerateRoyaltyStatementRequest : ModelEntityBase
    {
        private string RoyaltyOwnerBaIdValue;

        public string RoyaltyOwnerBaId

        {

            get { return this.RoyaltyOwnerBaIdValue; }

            set { SetProperty(ref RoyaltyOwnerBaIdValue, value); }

        }
        private DateTime PeriodStartValue;

        public DateTime PeriodStart

        {

            get { return this.PeriodStartValue; }

            set { SetProperty(ref PeriodStartValue, value); }

        }
        private DateTime PeriodEndValue;

        public DateTime PeriodEnd

        {

            get { return this.PeriodEndValue; }

            set { SetProperty(ref PeriodEndValue, value); }

        }
        private string PropertyIdValue;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
    }
}
