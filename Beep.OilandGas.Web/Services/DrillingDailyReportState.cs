using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.PermitsAndApplications.Models;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.Common;
using Beep.OilandGas.Client.App;

namespace Beep.OilandGas.Web.Services;

/// <summary>One company/account/route-scoped reporting confirmation and retained-original recovery.</summary>
public sealed partial class DrillingDailyReportState : IDisposable
{
    private readonly RetainedOriginalFeedback originalFeedback = new();
    public bool PendingTabCopyConfirmed => pending is not null && originalFeedback.TabCopyConfirmed;
    public bool PendingSubmissionMayHaveBeenSent => pending is not null && originalFeedback.SubmissionMayHaveBeenSent;
    public string PendingTitle => originalFeedback.Title("drilling report");
    public string? PendingExplanation => pending is null ? null : ConflictAcknowledged ? ConflictCleanupExplanation : originalFeedback.Explanation;
    private readonly CompanyClientContext company;
    private readonly IPersonaContextService personas;
    private readonly IDrillingServiceClient client;
    private readonly IDevelopmentDraftIntentStore journal;
    private readonly Func<Exception, string, string, string> explain;
    private bool disposed, ready, reading, writing;
    private long generation, request;
    private string? reviewId;
    private DrillingDailyReportPage? page;
    private DrillingDailyReportBasis? basis;
    private DrillingDailyReportObservation? correctionTarget;
    private bool completionMode;
    private DrillingDailyReportInput? prepared, pending;
    private DrillingDailyReportReceipt? receipt;
    public DrillingDailyReportPage? Page => Copy(page);
    public DrillingDailyReportBasis? Basis => Copy(basis);
    public DrillingDailyReportObservation? CorrectionTarget => Copy(correctionTarget);
    public bool CompletionMode => completionMode;
    public DrillingDailyReportInput? Prepared => Copy(prepared);
    public DrillingDailyReportInput? Pending => pending?.Basis.Execution.Basis.ReviewInstanceId == reviewId ? Copy(pending) : null;
    public string? RetainedReviewId => pending?.Basis.Execution.Basis.ReviewInstanceId;
    public DrillingDailyReportReceipt? Receipt => Copy(receipt);
    public bool Busy => reading || writing;
    public string? Error { get; private set; }
    public string? Confirmation { get; private set; }
    public bool CanView => company.Selection is {} selected && new[] { "Drilling.View", "Accounting.View", "DevelopmentPlanning.View", "Workflow.View", "Drilling.ViewDailyReports" }
        .All(permission => selected.Permissions.Contains(permission, StringComparer.Ordinal));
    private bool HasWrite => CanView && new[] { "Drilling.Edit", "Drilling.ManageWellProgram" }.All(permission => company.Selection!.Permissions.Contains(permission, StringComparer.Ordinal));
    public bool CanInspect => !Busy && ready && HasWrite && !HasCompletionReviewFlow && pending is null && receipt is null && page is not null;
    private bool ProfileMatches(DrillingDailyReportBasis value) => value.CanRecord && personas.CurrentProfile?.UserId == company.Selection?.UserId &&
        personas.CurrentProfile.PersonaCode == value.PersonaCode && personas.CurrentPersona?.Code == value.PersonaCode &&
        value.RecorderRoles.All(role => company.Selection!.Roles.Contains(role, StringComparer.Ordinal));
    public bool CanPrepare => CanInspect && basis is not null && ProfileMatches(basis);
    public bool CanRecover => !Busy && ready && CanView && pending is not null && pending.Basis.Execution.Basis.ReviewInstanceId == reviewId &&
        pending.Basis.Scope().Company == company.Selection!.Scope && pending.Basis.Scope().ActorUserId == company.Selection.UserId;
    public event Action? Changed;

    public DrillingDailyReportState(CompanyClientContext company, IPersonaContextService personas, IDrillingServiceClient client,
        IDevelopmentDraftIntentStore journal, Func<Exception, string, string, string> explain)
    {
        this.company = company; this.personas = personas; this.client = client; this.journal = journal; this.explain = explain;
        company.Changed += Invalidate; personas.Changed += Invalidate;
    }
    private static T? Copy<T>(T? value) where T : class => value is null ? null : CompanyInventorySourceReview.Copy(value);
    private void Invalidate()
    {
        ClearCompletionRouting();
        ClearCompletionFlow();
        ++generation; reading = false; ready = false; page = null; basis = null; correctionTarget = null; completionMode = false; prepared = null; pending = null; receipt = null; conflict = null; ConflictAcknowledged = false; originalFeedback.Reset(); Error = null; Confirmation = null;
        if (!disposed) Changed?.Invoke();
    }
    private bool Active((long Version, CompanyClientSelection? Selection) selected, long epoch) => !disposed && company.Snapshot == selected && generation == epoch;
    private void RequireExecution(DrillingExecutionStatus execution)
    {
        execution.Require(); var selected = company.Selection ?? throw new InvalidOperationException("Choose the report company.");
        if (execution.Recorded is null || execution.Basis.Scope().Company != selected.Scope || execution.Basis.Scope().ActorUserId != selected.UserId || execution.Basis.ReviewInstanceId != reviewId)
            throw new InvalidOperationException("The report belongs to another original company/account/execution.");
    }
    public async Task LoadAsync(string? requestedReview)
    {
        if (disposed) return;
        var sequence = ++request; reviewId = requestedReview; Invalidate(); var selected = company.Snapshot;
        if (!CanView || selected.Selection is null) return;
        var epoch = generation; bool Current() => sequence == request && Active(selected, epoch);
        reading = true; Changed?.Invoke();
        try
        {
            if (!LeaseSummaryValidator.Identifier(reviewId) || !reviewId.StartsWith(DrillingProgramReviewStart.InstancePrefix, StringComparison.Ordinal))
                throw new InvalidOperationException("Open the exact recorded execution report link.");
            var completionOriginal=await journal.ReadCompletionReviewAsync(selected.Selection.Scope,selected.Selection.UserId);if(!Current())return;
            if(completionOriginal is not null)
            {
                completionOriginal.Require();if(completionOriginal.Scope().Company!=selected.Selection.Scope || completionOriginal.Scope().ActorUserId!=selected.Selection.UserId)
                    throw new InvalidOperationException("The retained completion review belongs to another company/account.");
                completionPending=Copy(completionOriginal);completionFeedback.ReadFromTab();ready=true;return;
            }
            var original = await journal.ReadDailyReportAsync(selected.Selection.Scope, selected.Selection.UserId);
            if (!Current()) return;
            if (original is not null)
            {
                original.Require();
                if (original.Basis.Scope().Company != selected.Selection.Scope || original.Basis.Scope().ActorUserId != selected.Selection.UserId)
                    throw new InvalidOperationException("The retained report belongs to another company/account.");
                pending = Copy(original); originalFeedback.ReadFromTab(); ready = true; return;
            }
            var found = await client.GetDailyReportPageAsync(reviewId); if (!Current()) return;
            found.Require(); RequireExecution(found.History.Execution);
            if (HasWrite)
            {
                await personas.EnsureLoadedAsync();
                if (disposed || selected != company.Snapshot || sequence != request) return;
                epoch = generation;
            }
            if (Current()) { page = Copy(found); ready = true; }
        }
        catch (Exception error)
        {
            var text = explain(error, "loading drilling report history", "The original execution/report history could not be confirmed");
            if (Current()) { page = null; basis = null; Error = text; }
        }
        finally { if (Current()) { reading = false; Changed?.Invoke(); } }
    }
    public async Task InspectAsync()
    {
        if (!CanInspect) return;
        ClearCompletionRouting();
        var selected = company.Snapshot; var epoch = generation; reading = true; Error = null; basis = null; correctionTarget = null; completionMode = false; prepared = null; Changed?.Invoke();
        try
        {
            var found = await client.GetDailyReportBasisAsync(reviewId!); if (!Active(selected, epoch)) return;
            found.Require(); RequireExecution(found.Execution); found.RequireExecution(page!.History.Execution);
            var head = page!.History.Head;
            if ((long)found.NextNumber != (long)page.History.Count + 1 || found.PreviousReportId != head?.ReportId || found.PreviousReportRevision != head?.NativeRevision ||
                found.PreviousPeriodEndUtc != head?.LatestPeriodEndUtc || !ProfileMatches(found))
                throw new InvalidOperationException("Reload report history and confirm your current configured role/workspace before reporting.");
            basis = Copy(found);
        }
        catch (Exception error)
        {
            var text = explain(error, "reviewing drilling report source", "The current report head, active well and configured recorder could not be confirmed");
            if (Active(selected, epoch)) Error = text;
        }
        finally { if (Active(selected, epoch)) { reading = false; Changed?.Invoke(); } }
    }
    public async Task InspectCorrectionAsync(string reportId)
    {
        if (!CanInspect) return;
        var target = page!.Reports.SingleOrDefault(row => row.DDR_ID == reportId && row.SupersededByReportId is null);
        if (target is null) return;
        var selected = company.Snapshot; var epoch = generation; var checkedPage = page.Revision;
        await InspectAsync();
        if (!Active(selected, epoch) || !CanPrepare || page?.Revision != checkedPage) return;
        correctionTarget = Copy(target); completionMode = target.Completion is not null; Changed?.Invoke();
    }
    public async Task InspectCompletionAsync(string reportId)
    {
        if (!CanInspect || page!.History.LatestObservedReport?.ReportId != reportId) return;
        var selected = company.Snapshot; var epoch = generation;
        await InspectCorrectionAsync(reportId);
        if (!Active(selected, epoch) || !CanPrepare || correctionTarget?.DDR_ID != reportId) return;
        completionMode = true; Changed?.Invoke();
    }
    public void Prepare(string? role, string? start, string? end, decimal? startMd, decimal? endMd, string? unit,
        decimal? rotating, decimal? npt, string? nptReason, string? provenance, string? summary, string? nextPlan, string? correctionReason = null,
        DrillingCompletionReport? completion = null, bool withdrawCompletion = false)
    {
        if (!CanPrepare) return;
        Error = null; prepared = null;
        try
        {
            if ((completionMode || correctionTarget?.Completion is not null) && completion is null && !withdrawCompletion)
                throw new InvalidOperationException("Complete the reported completion evidence or explicitly withdraw the existing completion claim.");
            if (withdrawCompletion && correctionTarget?.Completion is null)
                throw new InvalidOperationException("Only an existing reported completion can be explicitly withdrawn.");
            if (!ExplicitUtcInput.TryParse(start, out var startUtc) || !ExplicitUtcInput.TryParse(end, out var endUtc) || endUtc > DateTime.UtcNow || startMd is null || endMd is null)
                throw new InvalidOperationException("Enter completed explicit UTC periods ending in Z/+00:00 and both observed MD values.");
            string? Optional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            var original = new DrillingDailyReportInput(Copy(basis)!, Guid.NewGuid(), role ?? "", new(startUtc), new(endUtc), startMd.Value, endMd.Value,
                unit ?? "", rotating, npt, Optional(nptReason), provenance?.Trim() ?? "", summary?.Trim() ?? "", Optional(nextPlan))
            {
                Correction = correctionTarget is {} target ? new(target.DDR_ID, target.REPORT_REVISION, target.REPORT_NUMBER, correctionReason?.Trim() ?? "") : null,
                Completion = Copy(completion), WithdrawCompletion = withdrawCompletion
            };
            if (correctionTarget?.MatchesObservations(original) == true)
                throw new InvalidOperationException("Change the observed values or source attribution before confirming a correction.");
            original.Require();
            if (completion is not null && (completion.CompletedAtUtc > DateTimeOffset.UtcNow || completion.Tests.Any(test => test.TestedAtUtc > DateTimeOffset.UtcNow)))
                throw new InvalidOperationException("Report actual completed work and performed tests, never future event times.");
            prepared = original;
        }
        catch (Exception error) { Error = explain(error, "preparing observed drilling report", "Confirm the actual period, measured hole depth/unit, source provenance and summary"); }
        Changed?.Invoke();
    }
    public void Cancel() { if (Busy || pending is not null) return; prepared = null; Changed?.Invoke(); }
    public Task SubmitAsync() => Record(false);
    public Task RecoverAsync() => Record(true);
    private async Task Record(bool recover)
    {
        if (disposed || (recover ? !CanRecover : !CanPrepare || prepared is null)) return;
        var original = Copy(recover ? pending : prepared)!; var selected = company.Snapshot; var epoch = generation;
        bool Current() => Active(selected, epoch);
        writing = true; Error = null; Confirmation = null;
        conflict = null; ConflictAcknowledged = false;
        if (!recover) { receipt = null; pending = original; prepared = null; page = null; basis = null; correctionTarget = null; completionMode = false; originalFeedback.BeginSaving(); }
        var failureExplanation = originalFeedback.FailureExplanation;
        Changed?.Invoke();
        try
        {
            if (!recover)
            {
                if (!Current()) return;
                await journal.RetainDailyReportAsync(original); if (!Current()) return;
                originalFeedback.ConfirmSaved(); originalFeedback.AttemptSubmission();
                failureExplanation = originalFeedback.FailureExplanation; Changed?.Invoke();
                if (!Current()) return;
            }
            var result = recover ? await client.RecoverDailyReportAsync(original) : await client.RecordDailyReportAsync(original);
            result.Require(original);
            if (!recover && result.Recorded is null) throw new InvalidOperationException("The original drilling report is not confirmed recorded.");
            if (!Current()) return;
            receipt = Copy(result); basis = null; page = null;
            originalFeedback.ConfirmReceiptAndBeginClear(result.Recorded is not null); failureExplanation = originalFeedback.FailureExplanation; Changed?.Invoke();
            if (!Current()) return;
            await journal.ClearDailyReportAsync(original); if (!Current()) return;
            pending = null; originalFeedback.Reset();
            Confirmation = result.Recorded is null ? "The original report was confirmed absent. Reload history before preparing another." :
                original.Completion is not null ? "The exact completion report was confirmed recorded, pending independent acceptance. Reporting responsibility and production handover remain separate." :
                original.WithdrawCompletion ? "The exact withdrawal of the reported completion was confirmed recorded. Reload its history; no acceptance or production release is implied." :
                "The exact original observed report was confirmed recorded. Reload its history to inspect progress.";
        }
        catch (Exception error)
        {
            var text = explain(error, recover ? "recovering original drilling report" : "recording observed drilling report",
                failureExplanation);
            if (Current()) { originalFeedback.Failed(); Error = text; }
        }
        finally { writing = false; if (!disposed) Changed?.Invoke(); }
    }
    public void Dispose()
    {
        if (disposed) return; disposed = true; company.Changed -= Invalidate; personas.Changed -= Invalidate; ++request; Invalidate();
    }
}
