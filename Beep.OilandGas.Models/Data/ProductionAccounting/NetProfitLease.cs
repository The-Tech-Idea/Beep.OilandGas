using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.LeaseManagement;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class NetProfitLease : LeaseAgreement
    {
        /// <summary>
        /// Gets or sets the net profit interest percentage (decimal, 0-1).
        /// </summary>
        private decimal NetProfitInterestValue;

        public decimal NetProfitInterest

        {

            get { return this.NetProfitInterestValue; }

            set { SetProperty(ref NetProfitInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets the cost recovery provisions.
        /// </summary>
        private NetProfitCostRecovery CostRecoveryValue = new();

        public NetProfitCostRecovery CostRecovery

        {

            get { return this.CostRecoveryValue; }

            set { SetProperty(ref CostRecoveryValue, value); }

        }
        private LeaseType _leasetype;
        public LeaseType LeaseType
        {
            get { return _leasetype; }
            set { SetProperty(ref _leasetype, value); }
        }
        public NetProfitLease()
        {
            LeaseType = LeaseType.NetProfit;
        }
    }
}
