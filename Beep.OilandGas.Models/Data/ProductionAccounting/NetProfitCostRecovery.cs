using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class NetProfitCostRecovery : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets whether costs are recoverable.
        /// </summary>
        private bool CostsRecoverableValue;

        public bool CostsRecoverable

        {

            get { return this.CostsRecoverableValue; }

            set { SetProperty(ref CostsRecoverableValue, value); }

        }

        /// <summary>
        /// Gets or sets the recovery percentage (decimal, 0-1).
        /// </summary>
        private decimal RecoveryPercentageValue;

        public decimal RecoveryPercentage

        {

            get { return this.RecoveryPercentageValue; }

            set { SetProperty(ref RecoveryPercentageValue, value); }

        }

        /// <summary>
        /// Gets or sets the types of costs that are recoverable.
        /// </summary>
        private List<string> RecoverableCostTypesValue = new();

        public List<string> RecoverableCostTypes

        {

            get { return this.RecoverableCostTypesValue; }

            set { SetProperty(ref RecoverableCostTypesValue, value); }

        }
    }
}
