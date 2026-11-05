using Beep.OilandGas.Models.Data.HSE;

namespace Beep.OilandGas.Models.Core.Interfaces;

public interface IHAZOPService
{
    Task<List<HAZOPSummary>> GetStudiesAsync(string fieldId);
    Task<string>        CreateStudyAsync(CreateHAZOPStudyRequest request, string userId);
    Task<List<HAZOPNode>> GetNodesAsync(string studyId);
    Task<string>        AddNodeAsync(string studyId, AddNodeRequest request, string userId);
    Task<string>        AddDeviationAsync(string studyId, int nodeSeq, AddDeviationRequest request, string userId);
    Task               UpdateDeviationStatusAsync(string studyId, int nodeSeq, int condSeq, string status, string userId);
    Task<HAZOPSummary> GetSummaryAsync(string studyId);
}

public interface IHSEKPIService
{
    Task<HSEKPISet>           GetKPIsAsync(string fieldId, DateRangeFilter range);
    Task<List<TierRateTrend>> GetTierRateTrendAsync(string fieldId, int months);
    Task<double>              GetExposureHoursAsync(string fieldId, DateRangeFilter range);
    Task<double>              GetTRIRAsync(string fieldId, DateRangeFilter range);

    /// <summary>
    /// The one count of HSE incidents: active incidents in the field when one is named, in every field otherwise, dated
    /// within <paramref name="start"/>..<paramref name="end"/> when given; how many are open and how many in each tier.
    /// </summary>
    Task<IncidentCounts>      CountIncidentsAsync(string? fieldId, DateTime? start, DateTime? end);
}
