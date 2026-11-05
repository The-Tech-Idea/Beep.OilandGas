using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.Lease;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

// One dialog's intent. Pending evidence survives closing the dialog and company changes in this session.
public sealed class LeaseCreationState : IDisposable
{
    private readonly CompanyClientContext company;
    private readonly ILeaseServiceClient api;
    public LeaseCreationState(CompanyClientContext company, ILeaseServiceClient api)
    { this.company = company; this.api = api; company.Changed += CompanyChanged; }
    public event Action? Changed;
    private void CompanyChanged() => Changed?.Invoke();
    public void Dispose() => company.Changed -= CompanyChanged;
    private CreateLeaseAcquisition? pending;
    private LeaseCreationContext? reviewed;
    private Guid commandId;
    public CreateLeaseAcquisition Draft { get; private set; } = new();
    public CreateLeaseAcquisition DisplayDraft => pending?.Snapshot() ?? Draft;
    public bool Busy { get; private set; }
    public bool HasPending => pending is not null;
    public bool RetryAvailable { get; private set; }
    public string? Error { get; private set; }
    public string? ConfirmedLeaseId { get; private set; }
    public string? Message { get; private set; }
    public bool CanEdit => !Busy && !HasPending && Matches(reviewed);
    public bool CanView => Matches(pending?.Context ?? reviewed);
    public bool CanSubmit => !Busy && (HasPending ? RetryAvailable && Matches(pending!.Context) : Matches(reviewed));
    public bool CanReconcile => !Busy && HasPending && Matches(pending!.Context);

    public async Task ReviewAsync(string field)
    {
        if (Busy) return;
        if (HasPending) { Error = pending!.PropertyId == field && Matches(pending.Context) ? Error : "Return to the original company, account and field to check the pending registration."; return; }
        Busy = true; Error = null; Message = null; reviewed = null; ConfirmedLeaseId = null; Draft = new();
        var captured = company.Snapshot;
        try
        {
            var context = await api.GetCreationContextAsync(field);
            Current(captured);
            if (context.FieldId != field || !Matches(context)) throw new InvalidOperationException("The reviewed creation context changed.");
            reviewed = context; commandId = Guid.NewGuid();
            Draft = new() { CommandId = commandId, Context = reviewed, PropertyId = field };
        }
        catch (Exception) { Error = "The creation context could not be reviewed. Select the company and field and try again."; }
        finally { Busy = false; }
    }
    public async Task SubmitAsync(string field)
    {
        if (!CanSubmit || (pending?.PropertyId ?? reviewed?.FieldId) != field) return;
        if (pending is null)
        {
            if (Draft.CommandId != commandId || Draft.Context != reviewed || Draft.PropertyId != field)
            { Error = "The creation intent changed. Review it again."; return; }
            pending = Draft.Snapshot();
        }
        var intent = pending.Snapshot(); var captured = company.Snapshot;
        Busy = true; RetryAvailable = false; Error = null; Message = null;
        try
        {
            var id = await api.CreateLeaseAcquisitionAsync(intent);
            Current(captured);
            if (id != intent.CommandId.ToString("N")) throw new InvalidOperationException("The registration receipt has a different identity.");
            Confirm(id);
        }
        catch (Exception) { Error = "Registration is unconfirmed. Check the same intent before another write."; }
        finally { Busy = false; }
    }
    public async Task ReconcileAsync(string field)
    {
        if (!CanReconcile || pending!.PropertyId != field) return;
        var intent = pending.Snapshot(); var captured = company.Snapshot;
        Busy = true; Error = null; Message = null; RetryAvailable = false;
        try
        {
            var receipt = await api.ReconcileLeaseCreationAsync(intent);
            Current(captured);
            if (receipt.CommandId != intent.CommandId || receipt.Context != intent.Context ||
                receipt.State is not ("Recorded" or "NotRecorded") ||
                (receipt.State == "Recorded" ? receipt.LeaseId != intent.CommandId.ToString("N") : receipt.LeaseId is not null))
                throw new InvalidOperationException("The reconciliation receipt is inconsistent.");
            if (receipt.State == "Recorded") Confirm(receipt.LeaseId!);
            else { RetryAvailable = true; Message = "No registration found. Retry will send the same unchanged intent."; }
        }
        catch (Exception) { Error = "Registration could not be reconciled. Keep this intent and review the original records."; }
        finally { Busy = false; }
    }
    private void Confirm(string id) { ConfirmedLeaseId = id; pending = null; reviewed = null; RetryAvailable = false; Message = "Lease registration confirmed."; }
    private bool Matches(LeaseCreationContext? context) => context is not null && company.Selection is { } selected && context.Company == selected.Scope && context.ActorUserId == selected.UserId;
    private void Current((long Version, CompanyClientSelection? Selection) captured)
    { if (company.Snapshot != captured) throw new OperationCanceledException("The selected company or account changed."); }
}
