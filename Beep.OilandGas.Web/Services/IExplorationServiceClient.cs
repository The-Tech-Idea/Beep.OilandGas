using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using PROSPECT = Beep.OilandGas.PPDM39.Models.PROSPECT;
using SEIS_ACQTN_SURVEY = Beep.OilandGas.PPDM39.Models.SEIS_ACQTN_SURVEY;
using SEIS_LINE = Beep.OilandGas.PPDM39.Models.SEIS_LINE;

namespace Beep.OilandGas.Web.Services;

public interface IExplorationServiceClient
{
    Task<ExplorationDashboardSummaryDto?> GetDashboardSummaryAsync(CancellationToken cancellationToken = default);
    Task<List<PROSPECT>> GetProspectsAsync(CancellationToken cancellationToken = default);
    Task<PROSPECT?> GetProspectAsync(string prospectId, CancellationToken cancellationToken = default);
    Task<List<ExplorationAfeLineDto>> GetProspectAfeLinesAsync(string prospectId, CancellationToken cancellationToken = default);
    Task<ProspectEvaluation> EvaluateProspectAsync(string prospectId, CancellationToken cancellationToken = default);
    Task<PROSPECT?> CreateProspectAsync(ProspectRequest request, string? userId = null, CancellationToken cancellationToken = default);
    Task<PROSPECT?> UpdateProspectAsync(string prospectId, ProspectRequest request, string? userId = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteProspectAsync(string prospectId, string? userId = null, CancellationToken cancellationToken = default);
    Task<List<SEIS_ACQTN_SURVEY>> GetSeismicSurveysAsync(CancellationToken cancellationToken = default);
    Task<SEIS_ACQTN_SURVEY?> CreateSeismicSurveyAsync(SeismicSurveyRequest request, string? userId = null, CancellationToken cancellationToken = default);
    Task<List<SEIS_LINE>> GetSeismicLinesAsync(string surveyId, CancellationToken cancellationToken = default);
    Task<bool> RecordProspectDecisionAsync(string prospectId, string decision, string? comments = null, CancellationToken cancellationToken = default);
}

public sealed class ExplorationDashboardSummaryDto
{
    public int LeadCount { get; set; }
    public int ProspectCount { get; set; }
    public int WellCount { get; set; }
    public int SurveyCount { get; set; }
    public double SuccessRatePct { get; set; }
    public List<ExplorationPendingDecisionDto> PendingDecisions { get; set; } = new();
    public List<ExplorationWellProgramDto> UpcomingPrograms { get; set; } = new();
}

public sealed record ExplorationPendingDecisionDto(string Name, string Description, string Status);

public sealed record ExplorationWellProgramDto(string Name, string TargetDate, string Status);

public sealed class ExplorationAfeLineDto
{
    public string AfeId { get; set; } = string.Empty;
    public string AfeNumber { get; set; } = string.Empty;
    public string AfeName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal CostUsd { get; set; }
    public string Status { get; set; } = string.Empty;
}