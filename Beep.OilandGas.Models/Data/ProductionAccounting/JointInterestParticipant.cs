using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class JointInterestParticipant : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the participant company name.
        /// </summary>
        private string CompanyNameValue = string.Empty;

        public string CompanyName

        {

            get { return this.CompanyNameValue; }

            set { SetProperty(ref CompanyNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the working interest (decimal, 0-1).
        /// </summary>
        private decimal WorkingInterestValue;

        public decimal WorkingInterest

        {

            get { return this.WorkingInterestValue; }

            set { SetProperty(ref WorkingInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets the net revenue interest (decimal, 0-1).
        /// </summary>
        private decimal NetRevenueInterestValue;

        public decimal NetRevenueInterest

        {

            get { return this.NetRevenueInterestValue; }

            set { SetProperty(ref NetRevenueInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets whether this participant is the operator.
        /// </summary>
        private bool IsOperatorValue;

        public bool IsOperator

        {

            get { return this.IsOperatorValue; }

            set { SetProperty(ref IsOperatorValue, value); }

        }
    }
}
