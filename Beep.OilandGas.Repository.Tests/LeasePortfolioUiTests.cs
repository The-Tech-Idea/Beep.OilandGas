using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Web.Services;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public sealed class LeasePortfolioUiTests
{
    private static void Select(CompanyClientContext company, string id = "company", string role = "Administrator")
    { var version = company.Clear(); Assert.True(company.TrySelect(version, "actor", new(new("tenant", id), [role], []))); }
    private static LeaseSummary Row(string field = "A", string id = "lease") => new()
    { LeaseId = id, LandRightSubtype = "lease", FieldId = field, DateZoneId = "UTC", Status = "COMPANY_REVIEW", CalculatedInterest = .25m };
    private static Task<string?> Field(string value = "A") => Task.FromResult<string?>(value);
    private static Mock<ILeaseServiceClient> Api()
    {
        var api = new Mock<ILeaseServiceClient>(MockBehavior.Strict);
        api.Setup(x => x.GetAvailableLeasesAsync(It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns((Dictionary<string, string> filter, CancellationToken _) => Task.FromResult(new List<LeaseSummary> { Row(filter["FieldId"]) }));
        return api;
    }
    private static LeasePortfolioState State(CompanyClientContext company, Mock<ILeaseServiceClient> api) => new(company, api.Object, (_, _, message) => message);
    [Fact]
    public async Task Clear_selection_removes_retrieval_time_records_and_late_results()
    {
        using var company=new CompanyClientContext();Select(company);var api=Api();using var state=State(company,api);
        await state.LoadAsync(()=>Field());Assert.NotNull(state.RetrievedAtUtc);Assert.Equal(DateTimeKind.Utc,state.RetrievedAtUtc!.Value.Kind);
        var completion=new TaskCompletionSource<List<LeaseSummary>>();api.Setup(value=>value.GetAvailableLeasesAsync(It.IsAny<Dictionary<string,string>>(),It.IsAny<CancellationToken>())).Returns(completion.Task);
        var load=state.LoadAsync(()=>Field());state.ClearSelection();Assert.Null(state.RetrievedAtUtc);Assert.Empty(state.Leases);Assert.False(state.Current);
        completion.SetResult([Row()]);await load;Assert.Empty(state.Leases);Assert.Null(state.RetrievedAtUtc);Assert.True(state.NeedsField);
    }

    [Fact]
    public async Task Missing_company_does_not_resolve_a_FIELD_or_send_business_reads()
    {
        using var company = new CompanyClientContext(); var api = Api(); using var state = State(company, api);
        await state.LoadAsync(() => throw new Exception("Must not resolve"));
        Assert.True(state.NeedsCompany); Assert.False(state.Current); Assert.False(state.Loaded); Assert.Empty(state.Leases);
        api.VerifyNoOtherCalls();
    }
    [Theory] [InlineData(null)] [InlineData("")]
    public async Task Missing_FIELD_has_an_explicit_unavailable_state(string? field)
    {
        using var company = new CompanyClientContext(); Select(company); var api = Api(); using var state = State(company, api);
        await state.LoadAsync(() => Task.FromResult(field));
        Assert.True(state.NeedsField); Assert.False(state.Loaded); Assert.Null(state.Error); api.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task A_failed_read_clears_previous_records_and_does_not_become_a_confirmed_empty_portfolio()
    {
        using var company = new CompanyClientContext(); Select(company); var api = Api(); using var state = State(company, api);
        await state.LoadAsync(() => Field()); Assert.True(state.Current);
        api.Setup(x => x.GetAvailableLeasesAsync(It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>())).ThrowsAsync(new UnauthorizedAccessException());
        await state.LoadAsync(() => Field());
        Assert.False(state.Current); Assert.False(state.Loaded); Assert.Empty(state.Leases); Assert.NotNull(state.Error);
    }
    [Fact]
    public async Task An_explicit_empty_response_is_confirmed_and_raw_status_and_interest_are_preserved()
    {
        using var company = new CompanyClientContext(); Select(company); var api = Api(); using var state = State(company, api);
        await state.LoadAsync(() => Field()); var row = Assert.Single(state.Leases);
        Assert.Equal("COMPANY_REVIEW", row.Status); Assert.Equal(.25m, row.CalculatedInterest); Assert.Null(row.WorkingInterest);
        api.Setup(x => x.GetAvailableLeasesAsync(It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        await state.LoadAsync(() => Field()); Assert.True(state.Current); Assert.Empty(state.Leases); Assert.Null(state.Error);
    }
    [Theory]
    [InlineData("field")] [InlineData("id")] [InlineData("subtype")] [InlineData("zone")] [InlineData("duplicate")]
    [InlineData("null-list")] [InlineData("null-row")] [InlineData("interest")] [InlineData("term")] [InlineData("status")]
    public async Task Invalid_or_foreign_evidence_never_leaves_displayable_records(string fault)
    {
        using var company = new CompanyClientContext(); Select(company); var api = Api(); using var state = State(company, api);
        var row = Row();
        if (fault == "field") row.FieldId = "B";
        if (fault == "id") row.LeaseId = " lease";
        if (fault == "subtype") row.LandRightSubtype = "";
        if (fault == "zone") row.DateZoneId = "not-a-zone";
        if (fault == "interest") row.WorkingInterest = 0;
        if (fault == "term") row.PrimaryTermMonths = 0;
        if (fault == "status") row.Status = null!;
        List<LeaseSummary> rows = fault switch { "duplicate" => [row, row], "null-list" => null!, "null-row" => [null!], _ => [row] };
        api.Setup(x => x.GetAvailableLeasesAsync(It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(rows);
        await state.LoadAsync(() => Field()); Assert.False(state.Current); Assert.Empty(state.Leases); Assert.NotNull(state.Error);
    }
    [Theory] [InlineData("company")] [InlineData("field")] [InlineData("dispose")]
    public async Task Superseded_read_is_cancelled_and_late_records_never_restore_old_evidence(string change)
    {
        using var company = new CompanyClientContext(); Select(company); var api = Api(); using var state = State(company, api);
        var completion = new TaskCompletionSource<List<LeaseSummary>>(TaskCreationOptions.RunContinuationsAsynchronously); CancellationToken cancellation = default;
        api.Setup(x => x.GetAvailableLeasesAsync(It.Is<Dictionary<string, string>>(filter => filter["FieldId"] == "A"), It.IsAny<CancellationToken>()))
            .Returns((Dictionary<string, string> _, CancellationToken token) => { cancellation = token; return completion.Task; });
        var old = state.LoadAsync(() => Field());
        if (change == "company") Select(company, "other");
        if (change == "field") await state.LoadAsync(() => Field("B"));
        if (change == "dispose") state.Dispose();
        Assert.True(cancellation.IsCancellationRequested); completion.SetResult([Row()]); await old;
        if (change == "field") { Assert.True(state.Current); Assert.Equal("B", Assert.Single(state.Leases).FieldId); }
        else { Assert.False(state.Current); Assert.Empty(state.Leases); if (change == "company") Assert.True(state.NeedsField); }
    }
    [Fact]
    public async Task A_late_FIELD_selection_cannot_start_an_old_read_after_a_new_selection()
    {
        using var company = new CompanyClientContext(); Select(company); var api = Api(); using var state = State(company, api);
        var completion = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var old = state.LoadAsync(() => completion.Task); await state.LoadAsync(() => Field("B")); completion.SetResult("A"); await old;
        Assert.Equal("B", state.FieldId);
        api.Verify(x => x.GetAvailableLeasesAsync(It.Is<Dictionary<string, string>>(filter => filter["FieldId"] == "A"), It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task Transport_mutation_does_not_change_displayed_facts_and_foreign_rows_cannot_be_command_targets()
    {
        using var company = new CompanyClientContext(); Select(company); var api = Api(); using var state = State(company, api); var source = Row(); var rows = new List<LeaseSummary> { source };
        api.Setup(x => x.GetAvailableLeasesAsync(It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(rows);
        await state.LoadAsync(() => Field()); source.CalculatedInterest = 99; source.FieldId = "B"; rows.Clear(); var visible = Assert.Single(state.Leases);
        Assert.Equal(.25m, visible.CalculatedInterest); Assert.Equal("A", visible.FieldId); Assert.True(state.Owns(visible));
        await state.EvaluateAsync(Row()); await state.UpdateStatusAsync(Row(), "ACTIVE");
        visible.LeaseId = "another"; Assert.False(state.Owns(visible)); await state.UpdateStatusAsync(visible, "ACTIVE");
        api.Verify(x => x.UpdateLeaseStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        api.Verify(x => x.EvaluateLeaseAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task Current_app_Administrator_is_required_and_confirmed_status_invalidates_old_facts()
    {
        using var company = new CompanyClientContext(); Select(company, role: "Reader"); var api = Api(); using var state = State(company, api);
        await state.LoadAsync(() => Field()); await state.UpdateStatusAsync(state.Leases[0], "ACTIVE");
        api.Verify(x => x.UpdateLeaseStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        Select(company); await state.LoadAsync(() => Field()); var row = state.Leases[0];
        api.Setup(x => x.UpdateLeaseStatusAsync("lease", "ACTIVE", "A")).Returns(Task.CompletedTask);
        await state.UpdateStatusAsync(row, "ACTIVE"); Assert.False(state.Current); Assert.Empty(state.Leases); Assert.NotNull(state.Confirmation); Assert.False(state.Busy);
        await state.UpdateStatusAsync(row, "ACTIVE"); api.Verify(x => x.UpdateLeaseStatusAsync("lease", "ACTIVE", "A"), Times.Once);
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task An_inflight_status_reserves_its_original_target_across_FIELD_changes_and_reload(bool failure)
    {
        using var company = new CompanyClientContext(); Select(company); var api = Api(); using var state = State(company, api);
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        api.Setup(x => x.UpdateLeaseStatusAsync("lease", "ACTIVE", "A")).Returns(completion.Task);
        await state.LoadAsync(() => Field()); var write = state.UpdateStatusAsync(state.Leases[0], "ACTIVE");
        await state.LoadAsync(() => Field("B")); Assert.False(state.HasUnconfirmedStatus);
        await state.LoadAsync(() => Field()); Assert.True(state.HasUnconfirmedStatus); Assert.False(state.CanUpdate(state.Leases[0]));
        await state.UpdateStatusAsync(state.Leases[0], "ACTIVE");
        if (failure) completion.SetException(new IOException("lost")); else completion.SetResult(); await write;
        Assert.Null(state.Confirmation); Assert.True(state.Current); Assert.Equal(failure, state.HasUnconfirmedStatus);
        Assert.Equal(!failure, state.CanUpdate(state.Leases[0]));
        await state.LoadAsync(() => Field()); Assert.Equal(failure, state.HasUnconfirmedStatus);
        api.Verify(x => x.UpdateLeaseStatusAsync("lease", "ACTIVE", "A"), Times.Once);
    }
    [Fact]
    public async Task Failed_status_remains_unconfirmed_after_reload_with_original_records()
    {
        using var company = new CompanyClientContext(); Select(company); var api = Api(); using var state = State(company, api);
        api.Setup(x => x.UpdateLeaseStatusAsync("lease", "ACTIVE", "A")).ThrowsAsync(new IOException("lost"));
        await state.LoadAsync(() => Field()); await state.UpdateStatusAsync(state.Leases[0], "ACTIVE");
        Assert.NotNull(state.CommandError); Assert.True(state.HasUnconfirmedStatus); Assert.Null(state.Confirmation);
        await state.LoadAsync(() => Field()); Assert.True(state.Current); Assert.True(state.HasUnconfirmedStatus); Assert.False(state.CanUpdate(state.Leases[0]));
    }
    [Theory] [InlineData("field")] [InlineData("company")] [InlineData("subtype")] [InlineData("zone")]
    public async Task Evaluation_requires_the_original_context_and_full_record_interpretation(string change)
    {
        using var company = new CompanyClientContext(); Select(company); var api = Api(); using var state = State(company, api);
        var completion = new TaskCompletionSource<LeaseSummary>(TaskCreationOptions.RunContinuationsAsynchronously);
        api.Setup(x => x.EvaluateLeaseAsync("lease", "A", It.IsAny<CancellationToken>())).Returns(completion.Task);
        await state.LoadAsync(() => Field()); var read = state.EvaluateAsync(state.Leases[0]);
        if (change == "field") await state.LoadAsync(() => Field("B"));
        if (change == "company") Select(company, "other");
        var row = Row(); if (change == "subtype") row.LandRightSubtype = "different"; if (change == "zone") row.DateZoneId = "Asia/Kuwait";
        completion.SetResult(row); await read; Assert.Null(state.Evaluation);
        if (change is "subtype" or "zone") Assert.NotNull(state.CommandError);
    }
}
