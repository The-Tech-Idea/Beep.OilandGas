using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.Validation
{
    public class ValidationResultResponse : ModelEntityBase
    {
        public string ValidationId { get; set; }
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public bool IsValid { get; set; }
        public List<ValidationIssue> Errors { get; set; } = new();
        public List<ValidationIssue> Warnings { get; set; } = new();
        public DateTime ValidationDate { get; set; } = DateTime.UtcNow;
    }

    public class ValidationIssue : ModelEntityBase
    {
        public string Field { get; set; }
        public string Message { get; set; }
    }

    public class CreateValidationRuleRequest : ModelEntityBase
    {
        public string EntityType { get; set; }
        public string RuleName { get; set; }
        public string RuleDefinition { get; set; }
        public string RuleType { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class ValidationRuleResponse : ModelEntityBase
    {
        public string RuleId { get; set; }
        public string EntityType { get; set; }
        public string RuleName { get; set; }
        public string RuleDefinition { get; set; }
        public string RuleType { get; set; }
        public bool IsActive { get; set; }
    }

    public class ValidationSummary : ModelEntityBase
    {
        public string EntityType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int TotalValidations { get; set; }
        public int ValidCount { get; set; }
        public int InvalidCount { get; set; }
        public int WarningCount { get; set; }
        public decimal ValidationSuccessRate { get; set; }
    }
}





