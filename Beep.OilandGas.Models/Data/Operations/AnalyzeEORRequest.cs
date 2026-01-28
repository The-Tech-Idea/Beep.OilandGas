using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Operations
{
    public class AnalyzeEORRequest : ModelEntityBase
    {
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string EorMethodValue = string.Empty;

        public string EorMethod

        {

            get { return this.EorMethodValue; }

            set { SetProperty(ref EorMethodValue, value); }

        }
    }
}
