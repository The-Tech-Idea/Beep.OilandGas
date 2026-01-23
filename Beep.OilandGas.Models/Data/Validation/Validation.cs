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

    public class ValidationIssue : ModelEntityBase
    {
        private string FieldValue;

        public string Field

        {

            get { return this.FieldValue; }

            set { SetProperty(ref FieldValue, value); }

        }
        private string MessageValue;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
    }

    public class CreateValidationRuleRequest : ModelEntityBase
    {
        private string EntityTypeValue;

        public string EntityType

        {

            get { return this.EntityTypeValue; }

            set { SetProperty(ref EntityTypeValue, value); }

        }
        private string RuleNameValue;

        public string RuleName

        {

            get { return this.RuleNameValue; }

            set { SetProperty(ref RuleNameValue, value); }

        }
        private string RuleDefinitionValue;

        public string RuleDefinition

        {

            get { return this.RuleDefinitionValue; }

            set { SetProperty(ref RuleDefinitionValue, value); }

        }
        private string RuleTypeValue;

        public string RuleType

        {

            get { return this.RuleTypeValue; }

            set { SetProperty(ref RuleTypeValue, value); }

        }
        private bool IsActiveValue = true;

        public bool IsActive

        {

            get { return this.IsActiveValue; }

            set { SetProperty(ref IsActiveValue, value); }

        }
    }

    public class ValidationRuleResponse : ModelEntityBase
    {
        private string RuleIdValue;

        public string RuleId

        {

            get { return this.RuleIdValue; }

            set { SetProperty(ref RuleIdValue, value); }

        }
        private string EntityTypeValue;

        public string EntityType

        {

            get { return this.EntityTypeValue; }

            set { SetProperty(ref EntityTypeValue, value); }

        }
        private string RuleNameValue;

        public string RuleName

        {

            get { return this.RuleNameValue; }

            set { SetProperty(ref RuleNameValue, value); }

        }
        private string RuleDefinitionValue;

        public string RuleDefinition

        {

            get { return this.RuleDefinitionValue; }

            set { SetProperty(ref RuleDefinitionValue, value); }

        }
        private string RuleTypeValue;

        public string RuleType

        {

            get { return this.RuleTypeValue; }

            set { SetProperty(ref RuleTypeValue, value); }

        }
        private bool IsActiveValue;

        public bool IsActive

        {

            get { return this.IsActiveValue; }

            set { SetProperty(ref IsActiveValue, value); }

        }
    }

    public class ValidationSummary : ModelEntityBase
    {
        private string EntityTypeValue;

        public string EntityType

        {

            get { return this.EntityTypeValue; }

            set { SetProperty(ref EntityTypeValue, value); }

        }
        private DateTime? StartDateValue;

        public DateTime? StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime? EndDateValue;

        public DateTime? EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
        private int TotalValidationsValue;

        public int TotalValidations

        {

            get { return this.TotalValidationsValue; }

            set { SetProperty(ref TotalValidationsValue, value); }

        }
        private int ValidCountValue;

        public int ValidCount

        {

            get { return this.ValidCountValue; }

            set { SetProperty(ref ValidCountValue, value); }

        }
        private int InvalidCountValue;

        public int InvalidCount

        {

            get { return this.InvalidCountValue; }

            set { SetProperty(ref InvalidCountValue, value); }

        }
        private int WarningCountValue;

        public int WarningCount

        {

            get { return this.WarningCountValue; }

            set { SetProperty(ref WarningCountValue, value); }

        }
        private decimal ValidationSuccessRateValue;

        public decimal ValidationSuccessRate

        {

            get { return this.ValidationSuccessRateValue; }

            set { SetProperty(ref ValidationSuccessRateValue, value); }

        }
    }
}








