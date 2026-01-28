using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class ValidationResult : ModelEntityBase
    {
        private bool IsValidValue;

        public bool IsValid

        {

            get { return this.IsValidValue; }

            set { SetProperty(ref IsValidValue, value); }

        }
        private string FieldNameValue = string.Empty;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
          public Dictionary<string, object> RuleParameters { get; set; } = new Dictionary<string, object>();
        private List<ValidationError> ErrorsValue = new List<ValidationError>();

        public List<ValidationError> Errors

        {

            get { return this.ErrorsValue; }

            set { SetProperty(ref ErrorsValue, value); }

        }
        private List<ValidationWarning> WarningsValue = new List<ValidationWarning>();

        public List<ValidationWarning> Warnings

        {

            get { return this.WarningsValue; }

            set { SetProperty(ref WarningsValue, value); }

        }
        private object EntityValue;

        public object Entity

        {

            get { return this.EntityValue; }

            set { SetProperty(ref EntityValue, value); }

        }
        private string TableNameValue;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private string ErrorMessageValue = string.Empty;

        public string ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }

        private string ValidationIdValue = string.Empty;


        public string ValidationId


        {


            get { return this.ValidationIdValue; }


            set { SetProperty(ref ValidationIdValue, value); }


        }

        private DateTime ValidatedDateValue = DateTime.UtcNow;


        public DateTime ValidatedDate


        {


            get { return this.ValidatedDateValue; }


            set { SetProperty(ref ValidatedDateValue, value); }


        }

           private string RuleIdValue = string.Empty;


           public string RuleId


           {


               get { return this.RuleIdValue; }


               set { SetProperty(ref RuleIdValue, value); }


           }
            private string RuleNameValue = string.Empty;

            public string RuleName

            {

                get { return this.RuleNameValue; }

                set { SetProperty(ref RuleNameValue, value); }

            }
            private string RuleTypeValue = string.Empty;

            public string RuleType

            {

                get { return this.RuleTypeValue; }

                set { SetProperty(ref RuleTypeValue, value); }

            } // REQUIRED, RANGE, FORMAT, BUSINESS_RULE
    }
}
