using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class GetWellStatusFacetsRequest : ModelEntityBase
    {
        private string StatusIdValue = string.Empty;

        public string StatusId

        {

            get { return this.StatusIdValue; }

            set { SetProperty(ref StatusIdValue, value); }

        }
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
    }
}
