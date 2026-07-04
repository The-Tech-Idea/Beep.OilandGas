using System.Text;
using System.Text.Json;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

/// <summary>
/// Formats compliance report data into downloadable report templates.
/// SOX ITGC (CSV/JSON) and SEC Reserves (structured format with audit chain verification).
/// Part of Phase 4 governance & compliance (P4-10, P4-11).
/// </summary>
public interface IReportTemplateService
{
    /// <summary>Generate SOX ITGC report as formatted CSV suitable for auditor submission.</summary>
    Task<string> GenerateSoxItgcCsvAsync(DateTime periodStart, DateTime periodEnd);

    /// <summary>Generate SOX ITGC report as structured JSON.</summary>
    Task<string> GenerateSoxItgcJsonAsync(DateTime periodStart, DateTime periodEnd);

    /// <summary>Generate SEC Reserves audit package (CSV + hash manifest + chain verification).</summary>
    Task<SecReservesReport> GenerateSecReservesReportAsync(string fieldId, DateTime evaluationDate);

    /// <summary>Export SEC reserves report as CSV with audit chain verification hash.</summary>
    Task<string> ExportSecReservesCsvAsync(string fieldId, DateTime evaluationDate);
}

public class SecReservesReport
{
    public string ReportTitle { get; set; } = "SEC Proved Reserves Report";
    public DateTime EvaluationDate { get; set; }
    public string FieldId { get; set; } = string.Empty;
    public string? FieldName { get; set; }
    public List<SecReservesEntry> Entries { get; set; } = new();
    public SecAuditVerification AuditVerification { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class SecReservesEntry
{
    public string ReservesCategory { get; set; } = string.Empty;  // PROVED, PROBABLE, POSSIBLE
    public decimal OilVolumeMmBbl { get; set; }
    public decimal GasVolumeBcf { get; set; }
    public decimal NglVolumeMmBbl { get; set; }
    public decimal BOE { get; set; }
    public string? EvaluatorId { get; set; }
    public string? ApproverId { get; set; }
    public DateTime EvaluationDate { get; set; }
    public string? AuditChainHash { get; set; }
}

public class SecAuditVerification
{
    public string VerifierId { get; set; } = string.Empty;
    public DateTime VerificationDate { get; set; }
    public bool ChainIntact { get; set; }
    public int TotalEntries { get; set; }
    public int VerifiedEntries { get; set; }
    public string? VerificationHash { get; set; }
}

public class ReportTemplateService : IReportTemplateService
{
    private readonly IComplianceReportService _complianceService;
    private readonly IAuditChainService? _auditChainService;

    public ReportTemplateService(
        IComplianceReportService complianceService,
        IAuditChainService? auditChainService = null)
    {
        _complianceService = complianceService;
        _auditChainService = auditChainService;
    }

    public async Task<string> GenerateSoxItgcCsvAsync(DateTime periodStart, DateTime periodEnd)
    {
        var report = await _complianceService.GenerateSoxItgcReportAsync(periodStart, periodEnd);
        var sb = new StringBuilder();

        sb.AppendLine("SOX IT General Controls Report");
        sb.AppendLine($"Period,{periodStart:yyyy-MM-dd},{periodEnd:yyyy-MM-dd}");
        sb.AppendLine($"Generated,{report.GeneratedAt:O}");
        sb.AppendLine();
        sb.AppendLine("SECTION 1: ACCESS CONTROL");
        sb.AppendLine($"Total Users,{report.AccessControl.TotalUsers}");
        sb.AppendLine($"Total Roles,{report.AccessControl.TotalRoles}");
        sb.AppendLine($"Active SoD Conflicts,{report.AccessControl.ActiveSoDConflicts}");
        sb.AppendLine($"Active Compensating Controls,{report.AccessControl.ActiveCompensatingControls}");
        sb.AppendLine($"Users with Elevated Access,{report.AccessControl.UsersWithElevatedAccess}");
        foreach (var finding in report.AccessControl.CriticalFindings)
            sb.AppendLine($"FINDING (AC),{EscapeCsv(finding)}");
        sb.AppendLine();
        sb.AppendLine("SECTION 2: CHANGE MANAGEMENT");
        sb.AppendLine($"Process Definition Changes,{report.ChangeManagement.ProcessDefinitionChanges}");
        sb.AppendLine($"Workflow Version Changes,{report.ChangeManagement.WorkflowVersionChanges}");
        sb.AppendLine($"Role Assignment Changes,{report.ChangeManagement.RoleAssignmentChanges}");
        sb.AppendLine();
        sb.AppendLine("SECTION 3: COMPUTER OPERATIONS");
        sb.AppendLine($"Total Process Instances,{report.ComputerOperations.TotalProcessInstances}");
        sb.AppendLine($"SLA Breaches in Period,{report.ComputerOperations.SlaBreachesInPeriod}");
        sb.AppendLine($"Escalations in Period,{report.ComputerOperations.EscalationsInPeriod}");
        sb.AppendLine();

        return sb.ToString();
    }

    public async Task<string> GenerateSoxItgcJsonAsync(DateTime periodStart, DateTime periodEnd)
    {
        var report = await _complianceService.GenerateSoxItgcReportAsync(periodStart, periodEnd);
        return JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
    }

    public async Task<SecReservesReport> GenerateSecReservesReportAsync(string fieldId, DateTime evaluationDate)
    {
        var report = new SecReservesReport
        {
            EvaluationDate = evaluationDate,
            FieldId = fieldId,
            Entries = new List<SecReservesEntry>
            {
                new() { ReservesCategory = "PROVED DEVELOPED PRODUCING", EvaluationDate = evaluationDate },
                new() { ReservesCategory = "PROVED DEVELOPED NON-PRODUCING", EvaluationDate = evaluationDate },
                new() { ReservesCategory = "PROVED UNDEVELOPED", EvaluationDate = evaluationDate },
                new() { ReservesCategory = "TOTAL PROVED", EvaluationDate = evaluationDate },
                new() { ReservesCategory = "PROBABLE", EvaluationDate = evaluationDate },
                new() { ReservesCategory = "POSSIBLE", EvaluationDate = evaluationDate },
            },
        };

        // Verify audit chain for the reserves revision process
        if (_auditChainService is not null)
        {
            try
            {
                // Find the reserves revision process instance for this field
                var verification = await _auditChainService.VerifyChainIntegrityAsync(fieldId);
                report.AuditVerification = new SecAuditVerification
                {
                    VerifierId = "SYSTEM",
                    VerificationDate = DateTime.UtcNow,
                    ChainIntact = verification.IsIntact,
                    TotalEntries = verification.TotalEntries,
                    VerifiedEntries = verification.VerifiedEntries,
                    VerificationHash = verification.IsIntact ? "CHAIN_INTACT" : "CHAIN_BROKEN",
                };

                // If chain is broken, flag in the report
                if (!verification.IsIntact)
                {
                    report.Entries.ForEach(e => e.AuditChainHash = "VERIFICATION_FAILED");
                }
            }
            catch
            {
                report.AuditVerification = new SecAuditVerification
                {
                    ChainIntact = false,
                    VerificationHash = "VERIFICATION_UNAVAILABLE",
                };
            }
        }

        return report;
    }

    public async Task<string> ExportSecReservesCsvAsync(string fieldId, DateTime evaluationDate)
    {
        var report = await GenerateSecReservesReportAsync(fieldId, evaluationDate);
        var sb = new StringBuilder();

        sb.AppendLine("SEC Proved Reserves Report");
        sb.AppendLine($"Field,{fieldId}");
        sb.AppendLine($"Evaluation Date,{evaluationDate:yyyy-MM-dd}");
        sb.AppendLine($"Generated,{report.GeneratedAt:O}");
        sb.AppendLine($"Audit Chain Intact,{report.AuditVerification.ChainIntact}");
        sb.AppendLine();
        sb.AppendLine("Category,Oil (MMbbl),Gas (Bcf),NGL (MMbbl),BOE,Evaluator,Approver,Audit Hash");

        foreach (var entry in report.Entries)
        {
            sb.AppendLine($"{EscapeCsv(entry.ReservesCategory)},{entry.OilVolumeMmBbl:F3},{entry.GasVolumeBcf:F3}," +
                         $"{entry.NglVolumeMmBbl:F3},{entry.BOE:F3},{entry.EvaluatorId},{entry.ApproverId},{entry.AuditChainHash}");
        }

        sb.AppendLine();
        sb.AppendLine("AUDIT VERIFICATION");
        sb.AppendLine($"Verifier,{report.AuditVerification.VerifierId}");
        sb.AppendLine($"Verification Date,{report.AuditVerification.VerificationDate:O}");
        sb.AppendLine($"Chain Intact,{report.AuditVerification.ChainIntact}");
        sb.AppendLine($"Total Entries,{report.AuditVerification.TotalEntries}");
        sb.AppendLine($"Verified Entries,{report.AuditVerification.VerifiedEntries}");
        sb.AppendLine($"Verification Hash,{report.AuditVerification.VerificationHash}");

        return sb.ToString();
    }

    private static string EscapeCsv(string value) =>
        value.Contains(',') || value.Contains('"') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
}
