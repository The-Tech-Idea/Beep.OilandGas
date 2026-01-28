using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class ExchangeParty : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the party name.
        /// </summary>
        private string PartyNameValue = string.Empty;

        public string PartyName

        {

            get { return this.PartyNameValue; }

            set { SetProperty(ref PartyNameValue, value); }

        }

        /// <summary>
        /// Gets or sets whether this party is the initiator.
        /// </summary>
        private bool IsInitiatorValue;

        public bool IsInitiator

        {

            get { return this.IsInitiatorValue; }

            set { SetProperty(ref IsInitiatorValue, value); }

        }

        /// <summary>
        /// Gets or sets the role (Buyer, Seller, Exchanger).
        /// </summary>
        private string RoleValue = string.Empty;

        public string Role

        {

            get { return this.RoleValue; }

            set { SetProperty(ref RoleValue, value); }

        }
    }
}
