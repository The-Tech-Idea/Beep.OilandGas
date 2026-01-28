using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class ValidationError : ModelEntityBase
    {
        private string FieldNameValue;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        private string ErrorMessageValue;

        public string ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        private string RuleNameValue;

        public string RuleName

        {

            get { return this.RuleNameValue; }

            set { SetProperty(ref RuleNameValue, value); }

        }
        private ValidationSeverity SeverityValue;

        public ValidationSeverity Severity

        {

            get { return this.SeverityValue; }

            set { SetProperty(ref SeverityValue, value); }

        }
    }
}
