namespace TheTechIdea.Data.OilGas;

public class PermitApplicationSummary
{
    public string PermitApplicationId { get; set; } = string.Empty;
    public string ApplicationType { get; set; } = string.Empty;
    public string RegulatoryAuthority { get; set; } = string.Empty;
    public string ApplicantName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? CreatedDate { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime? DecisionDate { get; set; }
    public string? Decision { get; set; }
    public string SubmittedByUserId { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string FieldId { get; set; } = string.Empty;
}

public class PermitApplicationDetail
{
    public PermitApplicationSummary Application { get; set; } = default!;
    public List<PermitStatusHistoryEntry> StatusHistory { get; set; } = new();
    public PermitApplicationCheckResult? ComplianceResult { get; set; }
}

public class PermitStatusHistoryEntry
{
    public string HistoryId { get; set; } = string.Empty;
    public string ApplicationId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StatusDate { get; set; }
    public string? Remarks { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
}

public class PermitApplicationCheckResult
{
    public string ApplicationId { get; set; } = string.Empty;
    public string FieldId { get; set; } = string.Empty;
    public DateTimeOffset CheckedAtUtc { get; set; }
    public bool IsCompliant { get; set; }
    public decimal ComplianceScore { get; set; }
    public List<string> Violations { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public static class PermitApplicationReadValidator
{
    public static void Require(PermitApplicationDetail? detail, string applicationId, string? fieldId = null)
    {
        var application = detail?.Application;
        if (!LeaseSummaryValidator.Identifier(applicationId) || application is null || application.PermitApplicationId != applicationId ||
            !LeaseSummaryValidator.Identifier(application.FieldId) || fieldId is not null && application.FieldId != fieldId ||
            detail!.StatusHistory is null || detail.StatusHistory.Count > 4096 ||
            detail.StatusHistory.Any(row => row is null || !LeaseSummaryValidator.Identifier(row.HistoryId) || row.ApplicationId != applicationId) ||
            detail.StatusHistory.Select(row => row.HistoryId).Distinct(StringComparer.Ordinal).Count() != detail.StatusHistory.Count)
            throw new InvalidOperationException("The permit application or recorded history belongs to another source or is incomplete.");
        if (detail.ComplianceResult is { } check) RequireCheck(check, applicationId, application.FieldId);
    }
    public static void RequireCheck(PermitApplicationCheckResult? check, string applicationId, string? fieldId = null)
    {
        if (!LeaseSummaryValidator.Identifier(applicationId) || check is null || check.ApplicationId != applicationId || !LeaseSummaryValidator.Identifier(check.FieldId) ||
            fieldId is not null && check.FieldId != fieldId || check.CheckedAtUtc == default || check.CheckedAtUtc.Offset != TimeSpan.Zero ||
            check.ComplianceScore is < 0 or > 100 || check.Violations is null || check.Warnings is null || check.Violations.Count > 256 || check.Warnings.Count > 256 ||
            check.Violations.Concat(check.Warnings).Any(value => value is null || value.Length > 4096) || check.IsCompliant && check.Violations.Count > 0)
            throw new InvalidOperationException("The permit checker evidence is unavailable, inconsistent or belongs to another application.");
    }
}
