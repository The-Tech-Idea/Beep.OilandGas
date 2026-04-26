using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PortfolioReport : ModelEntityBase
    {
        private string PortfolioNameValue = string.Empty;

        public string PortfolioName

        {

            get { return this.PortfolioNameValue; }

            set { SetProperty(ref PortfolioNameValue, value); }

        }
        private string UrlValue = string.Empty;

        public string Url

        {

            get { return this.UrlValue; }

            set { SetProperty(ref UrlValue, value); }

        }
    }
}
