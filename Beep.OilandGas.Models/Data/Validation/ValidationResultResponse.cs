using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Validation
{
    public class ValidationResultResponse : ModelEntityBase
    {
        private string ValidationIdValue;

        public string ValidationId

        {

            get { return this.ValidationIdValue; }

            set { SetProperty(ref ValidationIdValue, value); }

        }
        private string RuleIdValue;

        public string RuleId

        {

            get { return this.RuleIdValue; }

            set { SetProperty(ref RuleIdValue, value); }

        } // Added for consolidation with Process.cs
        private string EntityTypeValue;

        public string EntityType

        {

            get { return this.EntityTypeValue; }

            set { SetProperty(ref EntityTypeValue, value); }

        }
        private string EntityIdValue;

        public string EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private bool IsValidValue;

        public bool IsValid

        {

            get { return this.IsValidValue; }

            set { SetProperty(ref IsValidValue, value); }

        }
        private string ErrorMessageValue;

        public string ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        } // Added for consolidation
        private List<ValidationIssue> ErrorsValue = new();

        public List<ValidationIssue> Errors

        {

            get { return this.ErrorsValue; }

            set { SetProperty(ref ErrorsValue, value); }

        }
        private List<ValidationIssue> WarningsValue = new();

        public List<ValidationIssue> Warnings

        {

            get { return this.WarningsValue; }

            set { SetProperty(ref WarningsValue, value); }

        }
        private DateTime ValidationDateValue = DateTime.UtcNow;

        public DateTime ValidationDate

        {

            get { return this.ValidationDateValue; }

            set { SetProperty(ref ValidationDateValue, value); }

        }
    }
}
