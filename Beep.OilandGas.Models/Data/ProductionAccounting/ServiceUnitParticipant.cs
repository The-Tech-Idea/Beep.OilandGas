using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class ServiceUnitParticipant : ModelEntityBase
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
    }
}
