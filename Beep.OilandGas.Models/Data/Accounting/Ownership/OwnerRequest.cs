using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting.Ownership
{
    public class OwnerRequest : ModelEntityBase
    {
        private string OwnerNameValue = string.Empty;

        public string OwnerName

        {

            get { return this.OwnerNameValue; }

            set { SetProperty(ref OwnerNameValue, value); }

        }
        private string? TaxIdValue;

        public string? TaxId

        {

            get { return this.TaxIdValue; }

            set { SetProperty(ref TaxIdValue, value); }

        }
    }
}
