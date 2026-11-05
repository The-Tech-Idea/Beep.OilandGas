using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Reporting
{
    /// <summary>
    /// A JIB statement for a lease up to a period end. It takes no period start: the billing service states the lease's
    /// charges and credits from its earliest allocation to the period end, and the statement it records says which period
    /// it covers. The request carried a <c>PeriodStart</c> that nothing read.
    /// </summary>
    public class GenerateJIBStatementRequest : ModelEntityBase
    {
        private string LeaseIdValue;

        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
        private DateTime PeriodEndValue;

        public DateTime PeriodEnd

        {

            get { return this.PeriodEndValue; }

            set { SetProperty(ref PeriodEndValue, value); }

        }
    }
}
