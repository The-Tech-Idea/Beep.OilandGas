using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

public partial class DrillingServiceClient
{
    public async Task<DrillingDailyReportConflictReview> GetDailyReportConflictAsync(DrillingDailyReportInput original, CancellationToken cancellationToken = default)
    {
        original.Require();
        var result = await _apiClient.PostAsync<DrillingDailyReportInput, DrillingDailyReportConflictReview>(
            "/api/drilling/programs/execution/reports/conflict", original, cancellationToken)
            ?? throw new InvalidOperationException("The different recorded report is unconfirmed. Preserve the original input.");
        result.Require(original); return result;
    }
    public async Task<DrillingDailyReportConflictReview> ConfirmDailyReportConflictAsync(DrillingDailyReportConflictAcknowledgement original, CancellationToken cancellationToken = default)
    {
        original.Require();
        var result = await _apiClient.PostAsync<DrillingDailyReportConflictAcknowledgement, DrillingDailyReportConflictReview>(
            "/api/drilling/programs/execution/reports/conflict/confirm", original, cancellationToken)
            ?? throw new InvalidOperationException("The reviewed report history and original absence are unconfirmed.");
        result.Require(original.Original);
        if (result.Revision != original.ReviewRevision)
            throw new InvalidOperationException("The reviewed report history changed. Inspect again before clearing the original.");
        return result;
    }
}
