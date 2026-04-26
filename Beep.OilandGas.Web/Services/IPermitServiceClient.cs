using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Web.Services;

public interface IPermitServiceClient
{
    Task<List<PermitApplicationSummary>> GetAllAsync(string? status = null, string? authority = null, CancellationToken cancellationToken = default);
    Task<PermitApplicationDetail?> GetByIdAsync(string applicationId, CancellationToken cancellationToken = default);
    Task<string> CreateAsync(CreatePermitApplicationRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(string applicationId, UpdatePermitApplicationRequest request, CancellationToken cancellationToken = default);
    Task SubmitAsync(string applicationId, CancellationToken cancellationToken = default);
    Task<PermitDecisionResult> ProcessDecisionAsync(string applicationId, PermitDecisionRequest request, CancellationToken cancellationToken = default);
    Task<PermitComplianceResult?> CheckComplianceAsync(string applicationId, CancellationToken cancellationToken = default);
}

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
    public string FieldId { get; set; } = string.Empty;
}

public class PermitApplicationDetail
{
    public PermitApplicationSummary Application { get; set; } = default!;
    public List<PermitStatusHistoryEntry> StatusHistory { get; set; } = new();
    public PermitComplianceResult? ComplianceResult { get; set; }
}

public class PermitStatusHistoryEntry
{
    public string Status { get; set; } = string.Empty;
    public DateTime StatusDate { get; set; }
    public string? Remarks { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
}

public class PermitComplianceResult
{
    public bool IsCompliant { get; set; }
    public decimal ComplianceScore { get; set; }
    public List<string> Violations { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class CreatePermitApplicationRequest
{
    public string ApplicationType { get; set; } = string.Empty;
    public string RegulatoryAuthority { get; set; } = string.Empty;
    public string ApplicantName { get; set; } = string.Empty;
    public string? WellId { get; set; }
    public string? FacilityId { get; set; }
    public string? Description { get; set; }
}

public class UpdatePermitApplicationRequest
{
    public string? ApplicationType { get; set; }
    public string? RegulatoryAuthority { get; set; }
    public string? ApplicantName { get; set; }
    public string? Description { get; set; }
}

public class PermitDecisionRequest
{
    public string Decision { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}

public class PermitDecisionResult
{
    public string ApplicationId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty;
}
