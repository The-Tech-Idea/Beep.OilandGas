using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;
public partial class DrillingServiceClient
{
    public async Task<DrillingDailyReportPage> GetDailyReportPageAsync(string reviewId, int after = 0,
        int size = DrillingDailyReportPage.DefaultPageSize, string? historyRevision = null, CancellationToken cancellationToken = default)
    {
        DrillingDailyReportPage.RequireRequest(after, size, historyRevision);
        var path = ExecutionPath(reviewId, "/reports") + "&after=" + after.ToString(System.Globalization.CultureInfo.InvariantCulture) + "&size=" + size.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (historyRevision is not null) path += "&historyRevision=" + Uri.EscapeDataString(historyRevision);
        var result = await _apiClient.GetAsync<DrillingDailyReportPage>(path, cancellationToken)
            ?? throw new InvalidOperationException("The checked report page is unavailable.");
        result.Require();
        if (result.History.Execution.Basis.ReviewInstanceId != reviewId || result.AfterNumber != after || result.PageSize != size ||
            historyRevision is not null && result.History.Revision != historyRevision)
            throw new InvalidOperationException("The report page belongs to another source, range or changed history. Reload its first page.");
        return result;
    }
    public async Task<DrillingDailyReportBasis> GetDailyReportBasisAsync(string reviewId, CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.GetAsync<DrillingDailyReportBasis>(ExecutionPath(reviewId, "/reports/basis"), cancellationToken)
            ?? throw new InvalidOperationException("The original report basis is unavailable.");
        result.Require();
        if (result.Execution.Basis.ReviewInstanceId != reviewId) throw new InvalidOperationException("The report basis belongs to another program.");
        return result;
    }
    public Task<DrillingDailyReportReceipt> RecordDailyReportAsync(DrillingDailyReportInput original, CancellationToken cancellationToken = default) => Report(original, false, cancellationToken);
    public Task<DrillingDailyReportReceipt> RecoverDailyReportAsync(DrillingDailyReportInput original, CancellationToken cancellationToken = default) => Report(original, true, cancellationToken);
    private async Task<DrillingDailyReportReceipt> Report(DrillingDailyReportInput original, bool recover, CancellationToken token)
    {
        original.Require();
        var result = await _apiClient.PostAsync<DrillingDailyReportInput, DrillingDailyReportReceipt>("/api/drilling/programs/execution/reports" + (recover ? "/recover" : ""), original, token)
            ?? throw new InvalidOperationException("The original drilling report is unconfirmed. Preserve its retained input.");
        result.Require(original); return result;
    }
}
