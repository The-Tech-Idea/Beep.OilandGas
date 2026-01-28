using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class BatchValidationRequest : ModelEntityBase
    {
        private string TableNameValue = string.Empty;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        public List<Dictionary<string, object>> Entities { get; set; } = new List<Dictionary<string, object>>();
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
    }
}
