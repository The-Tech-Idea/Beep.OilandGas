using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PortfolioReportRequest : ModelEntityBase
    {
        private string PortfolioNameValue = string.Empty;

        public string PortfolioName

        {

            get { return this.PortfolioNameValue; }

            set { SetProperty(ref PortfolioNameValue, value); }

        }
        private List<string> ProspectIdsValue = new();

        public List<string> ProspectIds

        {

            get { return this.ProspectIdsValue; }

            set { SetProperty(ref ProspectIdsValue, value); }

        }
    }
}
