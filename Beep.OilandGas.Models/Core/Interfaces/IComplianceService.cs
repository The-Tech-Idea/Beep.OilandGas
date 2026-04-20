using Beep.OilandGas.Models.Data.Compliance;

namespace Beep.OilandGas.Models.Core.Interfaces;

public interface IComplianceService
{
    Task<string> CreateObligationAsync(CreateObligationRequest request, string userId);

    Task MarkSubmittedAsync(string obligationId, DateTime submitDate, string userId);

    Task WaiveObligationAsync(string obligationId, string reason, string userId);

    Task RecordPaymentAsync(string obligationId, RecordPaymentRequest request, string userId);

    Task<ObligationDetailModel?> GetByIdAsync(string obligationId);

    Task<List<ObligationSummary>> GetUpcomingObligationsAsync(string fieldId, int daysAhead = 30);

    Task<List<ObligationSummary>> GetOverdueObligationsAsync(string fieldId);

    Task<List<ObligationSummary>> GetAllObligationsAsync(string fieldId, int year);

    Task<ComplianceScoreCard> GetComplianceScoreAsync(string fieldId, int year);
}
