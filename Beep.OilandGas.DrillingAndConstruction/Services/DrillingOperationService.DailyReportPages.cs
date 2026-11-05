using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.DrillingAndConstruction.Services;

public partial class DrillingOperationService
{
    public async Task<DrillingDailyReportPage> ReadDailyReportPageAsync(DrillingExecutionStatus execution, int after, int size,
        string? expectedHistory, Func<Task> authority, Func<Task> confirmSource)
    {
        DrillingDailyReportPage.RequireRequest(after, size, expectedHistory);
        return await ReadDailyReportPageCoreAsync(execution,after,size,expectedHistory,authority,confirmSource);
    }
    public async Task<DrillingDailyReportPage> ReadDailyReportEntryAsync(DrillingExecutionStatus execution,DrillingDailyReportHead entry,
        Func<Task> authority,Func<Task> confirmSource)
    {
        if(entry is null || !LeaseSummaryValidator.Identifier(entry.ReportId) || entry.Number<1 || !ProcessDefinitionRevision.IsValid(entry.NativeRevision) ||
            entry.LatestPeriodEndUtc==default || entry.LatestPeriodEndUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("An exact immutable observed report reference is required.");
        var page=await ReadDailyReportPageCoreAsync(execution,entry.Number-1,1,null,authority,confirmSource);
        var row=page.Reports.SingleOrDefault();
        if(row is null || row.DDR_ID!=entry.ReportId || row.REPORT_NUMBER!=entry.Number || row.REPORT_REVISION!=entry.NativeRevision || row.PERIOD_END_UTC!=entry.LatestPeriodEndUtc.UtcDateTime)
            throw new InvalidOperationException("The immutable report reference is missing or changed in the complete checked current history.");
        return page;
    }
    private async Task<DrillingDailyReportPage> ReadDailyReportPageCoreAsync(DrillingExecutionStatus execution,int after,int size,
        string? expectedHistory,Func<Task> authority,Func<Task> confirmSource)
    {
        execution.Require();
        var source = _editor.GetDataSource(execution.Basis.Source.Basis.ExecutionDatabase.ConnectionName)
            ?? throw new InvalidOperationException("The original report source is unavailable.");
        var guard = DailyReportGuard(source, execution, authority);
        async Task<DailyReportRows> Read(bool owned)
        {
            RequireExecutionSchema(source); RequireDailyReportSchema(source);
            var result = await ReadDailyReportRowsAsync(execution, () => guard(owned), after: after, size: size);
            if (expectedHistory is not null && result.History.Revision != expectedHistory)
                throw new InvalidOperationException("The report history changed. Reload the first page before navigating or preparing a report.");
            return result;
        }
        await guard(false); DailyReportRows? found = null;
        await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
        {
            await guard(true); found = await Read(true); await guard(true);
            if (found.History.Revision != (await Read(true)).History.Revision)
                throw new InvalidOperationException("The report history changed during page readback.");
            await guard(true);
        });
        await guard(false);
        if (found!.History.Revision != (await Read(false)).History.Revision)
            throw new InvalidOperationException("The report history changed after page readback.");
        await guard(false); await confirmSource(); await guard(false);
        if (found.History.Revision != (await Read(false)).History.Revision)
            throw new InvalidOperationException("The report history changed around its closing source check.");
        await guard(false);
        var page = new DrillingDailyReportPage(found.History, after, size, found.Page, "");
        page = page with { Revision = page.ContentRevision() }; page.Require(); return page;
    }
}
