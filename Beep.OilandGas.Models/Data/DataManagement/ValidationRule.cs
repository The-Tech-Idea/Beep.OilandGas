using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class ValidationRule : ModelEntityBase
    {
        private string RuleNameValue;

        public string RuleName

        {

            get { return this.RuleNameValue; }

            set { SetProperty(ref RuleNameValue, value); }

        }
        private string FieldNameValue;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        private ValidationRuleType RuleTypeValue;

        public ValidationRuleType RuleType

        {

            get { return this.RuleTypeValue; }

            set { SetProperty(ref RuleTypeValue, value); }

        }
        private string RuleValueValue;

        public string RuleValue

        {

            get { return this.RuleValueValue; }

            set { SetProperty(ref RuleValueValue, value); }

        }
        private string ErrorMessageValue;

        public string ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        private ValidationSeverity SeverityValue;

        public ValidationSeverity Severity

        {

            get { return this.SeverityValue; }

            set { SetProperty(ref SeverityValue, value); }

        }
        private bool IsActiveValue;

        public bool IsActive

        {

            get { return this.IsActiveValue; }

            set { SetProperty(ref IsActiveValue, value); }

        }
      
        public Dictionary<string, object> RuleParameters { get; set; } = new Dictionary<string, object>();
        private string RuleIdValue = string.Empty;

        public string RuleId

        {

            get { return this.RuleIdValue; }

            set { SetProperty(ref RuleIdValue, value); }

        }
       
    }
}
