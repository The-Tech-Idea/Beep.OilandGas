using System.Security.Claims;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

public sealed partial class CompanyDrillingProgramService
{
    private async Task RequireReportViewAsync(ClaimsPrincipal user, DevelopmentAssignedReviewScope scope)
    {
        var current = await company.RefreshCompanyAsync(); company.RequireActor(scope.ActorUserId);
        if (user.Identity?.IsAuthenticated != true || user.ActingUserId() != scope.ActorUserId ||
            current.Scope != scope.Company || !current.Roles.Any(user.IsInRole) ||
            !current.Permissions.Contains("Drilling.ViewDailyReports", StringComparer.Ordinal))
            throw new UnauthorizedAccessException("Current company daily-report view access is required.");
    }

    private async Task<DrillingDailyReportHistory> ReadDailyReportsAsync(ClaimsPrincipal user, string reviewId)
    {
        var access = await CaptureExecutionReadAsync(user, reviewId); var scope = access.Basis.Scope();
        await RequireReportViewAsync(user, scope);
        async Task Guard() { await access.Check(); await RequireReportViewAsync(user, scope); }
        var execution = await native.ReadExecutionStatusAsync(access.Basis, Guard);
        var result = await native.ReadDailyReportsAsync(execution, Guard);
        result.Require(); await access.Confirm(); await Guard(); return result;
    }

    public async Task<DrillingDailyReportPage> ReadDailyReportPageAsync(ClaimsPrincipal user, string reviewId,
        int after, int size, string? expectedHistory)
    {
        DrillingDailyReportPage.RequireRequest(after, size, expectedHistory);
        var access = await CaptureExecutionReadAsync(user, reviewId); var scope = access.Basis.Scope();
        await RequireReportViewAsync(user, scope);
        async Task Guard() { await access.Check(); await RequireReportViewAsync(user, scope); }
        var execution = await native.ReadExecutionStatusAsync(access.Basis, Guard);
        var page = await native.ReadDailyReportPageAsync(execution, after, size, expectedHistory, Guard, access.Confirm);
        page.Require(); await Guard(); return page;
    }

    public async Task<DrillingDailyReportBasis> ReadDailyReportBasisAsync(ClaimsPrincipal user, string reviewId)
    {
        var history = await ReadDailyReportsAsync(user, reviewId); var execution = history.Execution;
        var recorder = await CaptureExecutionRecorderAsync(user, execution.Basis.Scope(), reporting: true);
        if (recorder.Roles.Length > 0)
        {
            var current = await native.ReadProgramWellAsync(execution.Basis.Scope().CoreDatabase, execution.Basis.Scope().FieldId,
                execution.Basis.Source.Basis.Well.Uwi, true, recorder.Check);
            if (current != execution.Basis.Source.Basis.Well) throw new InvalidOperationException("The original active execution well changed.");
        }
        var head = history.Head;
        if (history.Count == int.MaxValue) throw new InvalidOperationException("The report sequence is full.");
        var result = new DrillingDailyReportBasis(execution, history.Count + 1, head?.ReportId, head?.NativeRevision, head?.LatestPeriodEndUtc,
            recorder.Roles, recorder.Persona, recorder.Revision, "", DateTimeOffset.UtcNow);
        result = result with { Revision = result.ContentRevision() }; result.Require(); await recorder.Check(); return result;
    }

    public async Task<DrillingDailyReportReceipt> RecordDailyReportAsync(ClaimsPrincipal user, DrillingDailyReportInput input, bool recover = false)
    {
        input.Require(); var original = CompanyInventorySourceReview.Copy(input); var basis = original.Basis; var scope = basis.Scope();
        if (user.ActingUserId() != scope.ActorUserId || await account.RequireAsync() != scope.ActorUserId)
            throw new UnauthorizedAccessException("Only the original recorder may record or recover this report intent.");
        await RequireReportViewAsync(user, scope);
        var source = basis.Execution.Basis.Source.Basis; var fundingSource = source.Funding.Source;
        // Bind write/read admission to the retained source without a separate
        // complete funding walk. The current approved-program reader below and
        // the native owner's two callbacks still requalify the complete source.
        var access = await Capture(user, fundingSource.Current.Source.Review.InstanceId, fundingSource.AfeId, source.Well.Uwi, !recover, recover, source, writePermission: "Drilling.Edit");
        var executionAccess = await CaptureExecutionReadAsync(user, basis.Execution.Basis.ReviewInstanceId);
        var observed = executionAccess.Basis.Source.Basis;
        if (observed.Scope() != scope || observed.ExecutionDatabase != source.ExecutionDatabase)
            throw new UnauthorizedAccessException("The report belongs to another original company/account/FIELD/well/binding.");
        int sourceReads = 0;
        var recorder = recover ? null : await CaptureExecutionRecorderAsync(user, scope, true, original.RecorderRole, true);
        var bindings = await CaptureExecutionBindingsAsync(basis.Execution.Basis);
        async Task Guard()
        {
            await access.Check(); await executionAccess.Check(); await bindings(); await RequireReportViewAsync(user, scope);
            if (recorder is not null)
            {
                await recorder.Check();
                if (recorder.Revision != basis.RecorderAuthorityRevision || recorder.Persona != basis.PersonaCode || !recorder.Roles.SequenceEqual(basis.RecorderRoles))
                    throw new UnauthorizedAccessException("The original configured report recorder role/profile/authority changed.");
            }
        }
        async Task<DrillingExecutionStatus> Read()
        {
            await Guard(); ++sourceReads;
            if (!recover)
            {
                var active = await native.ReadProgramWellAsync(scope.CoreDatabase, scope.FieldId, source.Well.Uwi, true, Guard);
                if (active != source.Well) throw new InvalidOperationException("The original active report well changed.");
            }
            var current = await native.ReadExecutionStatusAsync(executionAccess.Basis, Guard);
            basis.RequireExecution(current);
            if (sourceReads > 1) await executionAccess.Confirm();
            await access.Confirm(); await Guard(); return current;
        }
        var result = await native.RecordDailyReportAsync(original, Guard, Read, recover); result.Require(original);
        if (!recover && result.Recorded is null) throw new InvalidOperationException("The original report is unconfirmed. Recover its retained input.");
        await access.Confirm(); await Guard(); return result;
    }
}
