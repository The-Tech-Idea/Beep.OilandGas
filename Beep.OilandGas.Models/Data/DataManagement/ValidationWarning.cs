using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class ValidationWarning : ModelEntityBase
    {
        private string FieldNameValue;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        private string WarningMessageValue;

        public string WarningMessage

        {

            get { return this.WarningMessageValue; }

            set { SetProperty(ref WarningMessageValue, value); }

        }
        private string RuleNameValue;

        public string RuleName

        {

            get { return this.RuleNameValue; }

            set { SetProperty(ref RuleNameValue, value); }

        }
    }
}
