using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class GovernmentLease : LeaseAgreement
    {
        /// <summary>
        /// Gets or sets the government agency (BLM, State, etc.).
        /// </summary>
        private string GovernmentAgencyValue = string.Empty;

        public string GovernmentAgency

        {

            get { return this.GovernmentAgencyValue; }

            set { SetProperty(ref GovernmentAgencyValue, value); }

        }

        /// <summary>
        /// Gets or sets the lease number assigned by the agency.
        /// </summary>
        private string GovernmentLeaseNumberValue = string.Empty;

        public string GovernmentLeaseNumber

        {

            get { return this.GovernmentLeaseNumberValue; }

            set { SetProperty(ref GovernmentLeaseNumberValue, value); }

        }

        /// <summary>
        /// Gets or sets whether this is a federal lease.
        /// </summary>
        private bool IsFederalValue;

        public bool IsFederal

        {

            get { return this.IsFederalValue; }

            set { SetProperty(ref IsFederalValue, value); }

        }

        /// <summary>
        /// Gets or sets whether this is an Indian lease.
        /// </summary>
        private bool IsIndianValue;

        public bool IsIndian

        {

            get { return this.IsIndianValue; }

            set { SetProperty(ref IsIndianValue, value); }

        }

        public GovernmentLease()
        {
            LeaseType = LeaseType.Government;
        }
    }
}
