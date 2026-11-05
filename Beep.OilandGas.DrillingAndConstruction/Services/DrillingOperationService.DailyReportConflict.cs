using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.DrillingAndConstruction.Services;

public partial class DrillingOperationService
{
    public async Task<DrillingDailyReportConflictReview> ReadDailyReportConflictAsync(DrillingDailyReportInput input,
        Func<Task> authority, Func<Task<DrillingExecutionStatus>> readExecution)
    {
        input.Require(); ArgumentNullException.ThrowIfNull(authority); ArgumentNullException.ThrowIfNull(readExecution);
        var original = CompanyInventorySourceReview.Copy(input); var basis = original.Basis;
        var source = _editor.GetDataSource(basis.Execution.Basis.Source.Basis.ExecutionDatabase.ConnectionName)
            ?? throw new InvalidOperationException("The original report source is unavailable.");
        var guard = DailyReportGuard(source, basis.Execution, authority);
        async Task Execution()
        {
            await guard(false); basis.RequireExecution(await readExecution()); await guard(false);
        }
        async Task<DrillingDailyReportConflictReview> Read(bool owned)
        {
            RequireExecutionSchema(source); RequireDailyReportSchema(source);
            // This existing owner checks the global key, company intent and complete ordered execution population.
            var scan = await ReadDailyReportRowsAsync(basis.Execution, () => guard(owned), original); var history = scan.History;
            if (scan.Original is not null)
                throw new InvalidOperationException("The original report has native evidence. Use exact original recovery.");
            var competing = scan.Competing
                ?? throw new InvalidOperationException("No different report occupies the original reviewed position. Recover the exact original instead.");
            var result = new DrillingDailyReportConflictReview(original.Revision(), original.ReportId(), true,
                CompanyInventorySourceReview.Copy(competing), history.Revision, history.Count, "", DateTimeOffset.UtcNow);
            result = result with { Revision = result.ContentRevision() }; result.Require(original); return result;
        }
        await Execution(); DrillingDailyReportConflictReview? found = null;
        await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
        {
            await guard(true); found = await Read(true); await guard(true);
            if (found.Revision != (await Read(true)).Revision)
                throw new InvalidOperationException("The original absence or exact report history changed during reconciliation.");
            await guard(true);
        });
        await guard(false);
        if (found!.Revision != (await Read(false)).Revision)
            throw new InvalidOperationException("The original absence or exact report history changed after readback.");
        await Execution();
        if (found.Revision != (await Read(false)).Revision)
            throw new InvalidOperationException("The report evidence changed around the closing execution source check.");
        await guard(false); var confirmed = found with { CheckedAtUtc = DateTimeOffset.UtcNow };
        confirmed.Require(original); return confirmed;
    }
}
