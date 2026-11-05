using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.Process
{
    public class ProcessInstanceSummary
    {
        public string InstanceId { get; set; } = string.Empty;
        public string ProcessId { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
        public string ProcessType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string? CurrentStepId { get; set; }
        public string CurrentStepName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? StartedAt { get; set; }
        public string StartedBy { get; set; } = string.Empty;
        public DateTime? CompletedAt { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysUntilDue { get; set; }
    }

    public class ProcessTransitionResult
    {
        public bool Success { get; set; }
        public string InstanceId { get; set; } = string.Empty;
        public string TransitionName { get; set; } = string.Empty;
        public string FromState { get; set; } = string.Empty;
        public string NewStepId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime TransitionedAt { get; set; }
    }

    public class ComplianceObligationSummary
    {
        public string ObligationId { get; set; } = string.Empty;
        public string ObligationType { get; set; } = string.Empty;
        public string JurisdictionTag { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int DaysUntilDue { get; set; }
    }

    public class ComplianceReportRequest
    {
        public string ObligationType { get; set; } = string.Empty;
        public string JurisdictionTag { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string SubmissionReference { get; set; } = string.Empty;
        public string ReportData { get; set; } = string.Empty;
    }

    public class GateReviewSubmitRequest
    {
        public string EntityId { get; set; } = string.Empty;
        public string? EntityType { get; set; }
        public string Comments { get; set; } = string.Empty;
    }

    public class GateReviewDecisionRequest
    {
        public GateDecision Decision { get; set; }
        public string Comments { get; set; } = string.Empty;
        public DateTime? DeferTargetDate { get; set; }
    }

    public enum GateDecision
    {
        Approve,
        Reject,
        Defer
    }

    public class GateChecklistResponse
    {
        public string GateId { get; set; } = string.Empty;
        public List<string> RequiredDocuments { get; set; } = new();
    }


}
