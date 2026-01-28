using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class ProspectReport : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private string UrlValue = string.Empty;

        public string Url

        {

            get { return this.UrlValue; }

            set { SetProperty(ref UrlValue, value); }

        }
    }
}
