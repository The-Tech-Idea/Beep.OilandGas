using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Web.Services;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public sealed class LeaseCreationUiTests
{
    private static readonly LeaseCreationContext Context = new(new("tenant", "company"), "reviewed-database", "actor", "A", "lease", "company-data", "operational", "UTC");
    private static void Select(CompanyClientContext company, string id = "company")
    { var version = company.Clear(); Assert.True(company.TrySelect(version, "actor", new(new("tenant", id), ["Administrator"], []))); }
    private static Mock<ILeaseServiceClient> Api()
    {
        var api = new Mock<ILeaseServiceClient>(MockBehavior.Strict);
        api.Setup(value => value.GetCreationContextAsync("A")).ReturnsAsync(Context);
        return api;
    }

    [Fact]
    public async Task An_uncertain_write_freezes_the_intent_until_explicit_read_only_reconciliation()
    {
        using var company = new CompanyClientContext(); Select(company); var api = Api(); var writes = new List<CreateLeaseAcquisition>();
        api.Setup(value => value.CreateLeaseAcquisitionAsync(It.IsAny<CreateLeaseAcquisition>())).Returns((CreateLeaseAcquisition request) => {
            writes.Add(request.Snapshot()); return writes.Count == 1 ? Task.FromException<string>(new IOException("lost")) : Task.FromResult(request.CommandId.ToString("N"));
        });
        api.Setup(value => value.ReconcileLeaseCreationAsync(It.IsAny<CreateLeaseAcquisition>())).Returns((CreateLeaseAcquisition request) => Task.FromResult(new LeaseCreationReceipt(request.CommandId, "NotRecorded", null, request.Context!)));
        using var state = new LeaseCreationState(company, api.Object); await state.ReviewAsync("A");
        var id = state.Draft.CommandId; state.Draft.LeaseName = "Original"; state.Draft.CalculatedInterest = .25m;
        await state.SubmitAsync("A"); Assert.True(state.HasPending); Assert.False(state.CanEdit); Assert.False(state.CanSubmit); Assert.True(state.CanReconcile); Assert.NotNull(state.Error);
        state.Draft.CommandId = Guid.NewGuid(); state.Draft.LeaseName = "Changed"; state.Draft.CalculatedInterest = .9m;
        Assert.Equal(id, state.DisplayDraft.CommandId); Assert.Equal("Original", state.DisplayDraft.LeaseName); Assert.Equal(.25m, state.DisplayDraft.CalculatedInterest);
        await state.SubmitAsync("A"); await state.ReviewAsync("A");
        Assert.Single(writes); api.Verify(value => value.GetCreationContextAsync("A"), Times.Once);
        api.Verify(value => value.ReconcileLeaseCreationAsync(It.IsAny<CreateLeaseAcquisition>()), Times.Never);
        await state.ReconcileAsync("A"); Assert.True(state.RetryAvailable); Assert.True(state.CanSubmit); Assert.False(state.CanEdit); Assert.Single(writes);
        await state.SubmitAsync("A"); Assert.False(state.HasPending); Assert.Equal(id.ToString("N"), state.ConfirmedLeaseId);
        Assert.Equal(2, writes.Count); Assert.All(writes, row => { Assert.Equal(id, row.CommandId); Assert.Equal("Original", row.LeaseName); Assert.Equal(.25m, row.CalculatedInterest); });
    }

    [Fact]
    public async Task Recorded_reconciliation_confirms_the_existing_id_without_another_write()
    {
        using var company = new CompanyClientContext(); Select(company); var api = Api();
        api.Setup(value => value.CreateLeaseAcquisitionAsync(It.IsAny<CreateLeaseAcquisition>())).ThrowsAsync(new IOException("lost"));
        api.Setup(value => value.ReconcileLeaseCreationAsync(It.IsAny<CreateLeaseAcquisition>())).Returns((CreateLeaseAcquisition request) => Task.FromResult(new LeaseCreationReceipt(request.CommandId, "Recorded", request.CommandId.ToString("N"), request.Context!)));
        using var state = new LeaseCreationState(company, api.Object); await state.ReviewAsync("A"); var id = state.Draft.CommandId;
        await state.SubmitAsync("A"); await state.ReconcileAsync("A");
        Assert.Equal(id.ToString("N"), state.ConfirmedLeaseId); Assert.False(state.HasPending); Assert.False(state.CanSubmit);
        api.Verify(value => value.CreateLeaseAcquisitionAsync(It.IsAny<CreateLeaseAcquisition>()), Times.Once);
    }

    [Theory]
    [InlineData("command")] [InlineData("context")] [InlineData("state")] [InlineData("recorded-id")] [InlineData("absent-id")]
    public async Task Inconsistent_reconciliation_never_unlocks_a_retry(string fault)
    {
        using var company = new CompanyClientContext(); Select(company); var api = Api();
        api.Setup(value => value.CreateLeaseAcquisitionAsync(It.IsAny<CreateLeaseAcquisition>())).ThrowsAsync(new IOException("lost"));
        api.Setup(value => value.ReconcileLeaseCreationAsync(It.IsAny<CreateLeaseAcquisition>())).Returns((CreateLeaseAcquisition request) => {
            var receipt = new LeaseCreationReceipt(request.CommandId, "NotRecorded", null, request.Context!);
            receipt = fault switch { "command" => receipt with { CommandId = Guid.NewGuid() }, "context" => receipt with { Context = Context with { FieldId = "B" } },
                "state" => receipt with { State = "" }, "recorded-id" => receipt with { State = "Recorded" }, _ => receipt with { LeaseId = "other" } };
            return Task.FromResult(receipt);
        });
        using var state = new LeaseCreationState(company, api.Object); await state.ReviewAsync("A"); await state.SubmitAsync("A"); await state.ReconcileAsync("A");
        Assert.True(state.HasPending); Assert.False(state.CanSubmit); Assert.False(state.RetryAvailable); Assert.Null(state.ConfirmedLeaseId); Assert.NotNull(state.Error);
    }

    [Fact]
    public async Task A_delayed_receipt_cannot_confirm_under_a_different_company_generation()
    {
        using var company = new CompanyClientContext(); Select(company); var api = Api(); var completion = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        api.Setup(value => value.CreateLeaseAcquisitionAsync(It.IsAny<CreateLeaseAcquisition>())).Returns(completion.Task);
        using var state = new LeaseCreationState(company, api.Object); await state.ReviewAsync("A"); var id = state.Draft.CommandId;
        var submit = state.SubmitAsync("A"); Assert.True(state.Busy); Select(company, "other"); completion.SetResult(id.ToString("N")); await submit;
        Assert.True(state.HasPending); Assert.False(state.CanView); Assert.False(state.CanReconcile); Assert.Null(state.ConfirmedLeaseId);
        Select(company); Assert.True(state.CanView); Assert.True(state.CanReconcile); Assert.Equal(id, state.Draft.CommandId);
    }

    [Fact]
    public async Task A_changed_FIELD_cannot_send_or_reconcile_the_original_intent()
    {
        using var company = new CompanyClientContext(); Select(company); var api = Api();
        api.Setup(value => value.CreateLeaseAcquisitionAsync(It.IsAny<CreateLeaseAcquisition>())).ThrowsAsync(new IOException("lost"));
        using var state = new LeaseCreationState(company, api.Object); await state.ReviewAsync("A"); await state.SubmitAsync("B");
        api.Verify(value => value.CreateLeaseAcquisitionAsync(It.IsAny<CreateLeaseAcquisition>()), Times.Never);
        await state.SubmitAsync("A"); await state.ReconcileAsync("B");
        api.Verify(value => value.ReconcileLeaseCreationAsync(It.IsAny<CreateLeaseAcquisition>()), Times.Never); Assert.True(state.HasPending);
    }

    [Fact]
    public async Task Context_review_failure_does_not_leave_an_old_editable_draft()
    {
        using var company = new CompanyClientContext(); Select(company); var api = Api();
        using var state = new LeaseCreationState(company, api.Object); await state.ReviewAsync("A"); state.Draft.LeaseName = "Previous";
        api.Setup(value => value.GetCreationContextAsync("A")).ThrowsAsync(new UnauthorizedAccessException());
        await state.ReviewAsync("A"); Assert.False(state.CanEdit); Assert.False(state.CanView); Assert.False(state.CanSubmit); Assert.Equal("", state.Draft.LeaseName); Assert.NotNull(state.Error);
    }
}
