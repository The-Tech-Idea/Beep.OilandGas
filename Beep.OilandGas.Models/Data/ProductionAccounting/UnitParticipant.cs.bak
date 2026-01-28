using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class UnitParticipant : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the participant identifier.
        /// </summary>
        private string ParticipantIdValue = string.Empty;

        public string ParticipantId

        {

            get { return this.ParticipantIdValue; }

            set { SetProperty(ref ParticipantIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the company name.
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

        /// <summary>
        /// Gets or sets the voting percentage (0-100).
        /// </summary>
        private decimal VotingPercentageValue;

        public decimal VotingPercentage

        {

            get { return this.VotingPercentageValue; }

            set { SetProperty(ref VotingPercentageValue, value); }

        }
    }
}
