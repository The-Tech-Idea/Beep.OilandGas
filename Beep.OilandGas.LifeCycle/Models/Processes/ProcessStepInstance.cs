using Beep.OilandGas.Models.DTOs;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.LifeCycle.Models.Processes
{
    /// <summary>
    /// Represents an instance of a process step execution
    /// </summary>
    public class ProcessStepInstance
    {
        public string StepInstanceId { get; set; } = string.Empty;
        public string InstanceId { get; set; } = string.Empty;
        public string StepId { get; set; } = string.Empty;
        public int SequenceNumber { get; set; }
        public StepStatus Status { get; set; } = StepStatus.PENDING;
        public DateTime? StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string CompletedBy { get; set; } = string.Empty;
        public Dictionary<string, object> StepData { get; set; } = new Dictionary<string, object>();
        public List<ApprovalRecord> Approvals { get; set; } = new List<ApprovalRecord>();
        public List<ValidationResult> ValidationResults { get; set; } = new List<ValidationResult>();
        public string Outcome { get; set; } = string.Empty; // SUCCESS, FAILED, CONDITIONAL
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Step status enumeration
    /// </summary>
    public enum StepStatus
    {
        PENDING,
        IN_PROGRESS,
        COMPLETED,
        FAILED,
        SKIPPED,
        BLOCKED
    }

    /// <summary>
    /// Approval record for a process step
    /// </summary>
    public class ApprovalRecord
    {
        public string ApprovalId { get; set; } = string.Empty;
        public string ApprovalType { get; set; } = string.Empty; // STEP_APPROVAL, STATE_TRANSITION_APPROVAL
        public DateTime RequestedDate { get; set; } = DateTime.UtcNow;
        public string RequestedBy { get; set; } = string.Empty;
        public DateTime? ApprovedDate { get; set; }
        public string ApprovedBy { get; set; } = string.Empty;
        public ApprovalStatus Status { get; set; } = ApprovalStatus.PENDING;
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Approval status enumeration
    /// </summary>
    public enum ApprovalStatus
    {
        PENDING,
        APPROVED,
        REJECTED
    }

    /// <summary>
    /// Validation result for a process step
    /// </summary>
    //public class ValidationResult
    //{
    //    public string ValidationId { get; set; } = string.Empty;
    //    public string RuleId { get; set; } = string.Empty;
    //    public bool IsValid { get; set; }
    //    public string ErrorMessage { get; set; } = string.Empty;
    //    public DateTime ValidatedDate { get; set; } = DateTime.UtcNow;
    //    public string ValidatedBy { get; set; } = string.Empty;
    //}
}

