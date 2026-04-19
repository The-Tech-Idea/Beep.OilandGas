using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PortfolioSummary : ModelEntityBase
    {
        private string PortfolioNameValue = string.Empty;

        public string PortfolioName

        {

            get { return this.PortfolioNameValue; }

            set { SetProperty(ref PortfolioNameValue, value); }

        }
        private int TotalProspectsValue;

        public int TotalProspects

        {

            get { return this.TotalProspectsValue; }

            set { SetProperty(ref TotalProspectsValue, value); }

        }
        private decimal TotalEstimatedResourcesValue;

        public decimal TotalEstimatedResources

        {

            get { return this.TotalEstimatedResourcesValue; }

            set { SetProperty(ref TotalEstimatedResourcesValue, value); }

        }
    }
}
