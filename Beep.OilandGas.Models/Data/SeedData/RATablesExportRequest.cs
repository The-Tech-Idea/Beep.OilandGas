using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class RATablesExportRequest : ModelEntityBase
    {
        private string? OutputPathValue;

        public string? OutputPath

        {

            get { return this.OutputPathValue; }

            set { SetProperty(ref OutputPathValue, value); }

        }
    }
}
