using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.PermitsAndApplications.Models;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Web.Services;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

// Actual UI state with an explicit facet fixture. HTTP, DI and provider evidence are separate gates.
public sealed class FieldSelectionTests
{
    [Fact]
    public async Task Absent_selection_is_reviewed_and_the_first_command_contains_the_exact_context()
    {
        using var f = new Fixture(); f.Data.Current.SelectionContext = f.Data.Review with { FieldId = null }; f.Data.Current.SelectionAvailable = false; f.Data.Current.FieldId = "";
        await f.State.LoadAsync(); Assert.True(f.State.Loaded); Assert.Null(f.State.SelectedId);
        await f.State.SelectAsync("B"); Assert.Equal("B", f.State.SelectedId); Assert.False(f.State.Loaded); Assert.NotNull(f.State.Confirmation);
        Assert.Equal(f.Data.Current.SelectionContext, Assert.Single(f.Data.Commands).Context);
    }
    [Theory] [InlineData("null")] [InlineData("duplicate")] [InlineData("identity")] [InlineData("foreign-current")] [InlineData("actor")] [InlineData("company")] [InlineData("revision")] [InlineData("availability")]
    public async Task Malformed_or_foreign_evidence_is_not_a_confirmed_empty_catalog(string fault)
    {
        using var f = new Fixture();
        if (fault == "null") f.Data.Rows = null!;
        if (fault == "duplicate") f.Data.Rows.Add(new() { FieldId = "A" });
        if (fault == "identity") f.Data.Rows[0].FieldId = " A";
        if (fault == "foreign-current") { f.Data.Current.FieldId = "foreign"; f.Data.Current.SelectionContext = f.Data.Review with { FieldId = "foreign" }; }
        if (fault == "actor") f.Data.Current.SelectionContext = f.Data.Review with { ActorUserId = "other" };
        if (fault == "company") f.Data.Current.SelectionContext = f.Data.Review with { Company = new("tenant", "other") };
        if (fault == "revision") f.Data.Current.SelectionContext = f.Data.Review with { Revision = "" };
        if (fault == "availability") f.Data.Current.SelectionAvailable = false;
        await f.State.LoadAsync(); Assert.False(f.State.Loaded); Assert.Empty(f.State.Fields); Assert.NotNull(f.State.Error); await f.State.SelectAsync("B"); Assert.Empty(f.Data.Commands);
    }
    [Fact]
    public async Task A_true_empty_catalog_is_distinct_from_a_read_failure()
    {
        using var f = new Fixture(); f.Data.Rows.Clear(); f.Data.Current.FieldId = ""; f.Data.Current.SelectionAvailable = false; f.Data.Current.SelectionContext = f.Data.Review with { FieldId = null };
        await f.State.LoadAsync(); Assert.True(f.State.Loaded); Assert.Empty(f.State.Fields); Assert.Null(f.State.Error);
        f.Data.Read = _ => throw new UnauthorizedAccessException(); await f.State.LoadAsync(); Assert.False(f.State.Loaded); Assert.NotNull(f.State.Error);
    }
    [Fact]
    public async Task Unavailable_saved_FIELD_can_be_replaced_without_presenting_it_as_authority()
    {
        using var f = new Fixture(); f.Data.Current.FieldId = ""; f.Data.Current.SelectionAvailable = false;
        await f.State.LoadAsync(); Assert.True(f.State.Loaded); Assert.Null(f.State.SelectedId); Assert.NotNull(f.State.Error);
        await f.State.SelectAsync("B"); Assert.Equal("A", Assert.Single(f.Data.Commands).Context!.FieldId); Assert.Equal("B", f.State.SelectedId);
    }
    [Fact]
    public async Task Company_change_cancels_the_read_and_discards_a_late_catalog()
    {
        using var f = new Fixture(); var pending = new TaskCompletionSource<List<FieldListItem>>(TaskCreationOptions.RunContinuationsAsynchronously); CancellationToken token = default;
        f.Data.Read = captured => { token = captured; return pending.Task; }; var read = f.State.LoadAsync();
        f.Company.Clear(); Assert.True(token.IsCancellationRequested); Assert.Empty(f.State.Fields); Assert.True(f.State.NeedsCompany);
        pending.SetResult(f.Data.Rows); await read; Assert.False(f.State.Loaded); Assert.Null(f.State.Error); Assert.Equal(0, f.Data.CurrentReads);
    }
    [Fact]
    public async Task Current_read_cannot_be_published_after_switching_away_and_back()
    {
        using var f = new Fixture(); var pending = new TaskCompletionSource<FieldResponse>(TaskCreationOptions.RunContinuationsAsynchronously); f.Data.Selected = _ => pending.Task;
        var read = f.State.LoadAsync(); f.SelectCompany(); pending.SetResult(f.Data.Current); await read; Assert.False(f.State.Loaded); Assert.Empty(f.State.Fields);
    }
    [Fact]
    public async Task A_pending_write_has_one_attempt_and_a_late_ack_does_not_confirm_in_another_company_generation()
    {
        using var f = new Fixture(); await f.State.LoadAsync(); var pending = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously); f.Data.Save = _ => pending.Task;
        var save = f.State.SelectAsync("B"); await f.State.SelectAsync("A"); f.Data.Publish("B"); Assert.Null(f.State.Confirmation); Assert.True(f.State.Saving);
        f.SelectCompany(); await f.State.LoadAsync(); await f.State.SelectAsync("B"); Assert.Single(f.Data.Commands);
        pending.SetResult(true); await save; Assert.Null(f.State.SelectedId); Assert.Null(f.State.Confirmation); Assert.False(f.State.Loaded); Assert.False(f.State.Saving);
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task Rejected_or_uncertain_commands_require_a_new_review_without_automatic_retry(bool throws)
    {
        using var f = new Fixture(); await f.State.LoadAsync(); f.Data.Save = _ => throws ? throw new HttpRequestException() : Task.FromResult(false);
        await f.State.SelectAsync("B"); Assert.False(f.State.Loaded); Assert.Null(f.State.SelectedId); Assert.Empty(f.State.Fields); Assert.NotNull(f.State.Error);
        await f.State.SelectAsync("B"); Assert.Single(f.Data.Commands);
    }
    [Fact]
    public async Task Transport_rows_are_copied_and_a_mutated_UI_identity_is_not_a_command_target()
    {
        using var f = new Fixture(); await f.State.LoadAsync(); f.Data.Rows[1].FieldId = "foreign"; Assert.Equal("B", f.State.Fields.Single(row => row.FieldName == "Beta").FieldId);
        f.State.Fields.Single(row => row.FieldId == "B").FieldId = "foreign"; await f.State.SelectAsync("foreign"); Assert.Empty(f.Data.Commands);
    }
    [Fact]
    public async Task External_FIELD_events_require_a_fresh_profile_review()
    {
        using var f = new Fixture(); await f.State.LoadAsync(); f.Data.Publish("B"); Assert.False(f.State.Loaded); Assert.Empty(f.State.Fields); Assert.Null(f.State.SelectedId); Assert.NotNull(f.State.Error);
    }
    [Fact]
    public async Task A_different_FIELD_event_during_save_prevents_confirmation_of_the_pending_target()
    {
        using var f = new Fixture(); await f.State.LoadAsync(); var pending = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously); f.Data.Save = _ => pending.Task;
        var save = f.State.SelectAsync("B"); f.Data.Publish("A"); pending.SetResult(true); await save;
        Assert.Null(f.State.Confirmation); Assert.Null(f.State.SelectedId); Assert.False(f.State.Loaded); Assert.NotNull(f.State.Error);
    }
    [Theory] [InlineData("id")] [InlineData("actor")] [InlineData("database")] [InlineData("revision")] [InlineData("failure")] [InlineData("missing")]
    public void Acknowledgements_require_exact_new_review_evidence(string fault)
    {
        using var f = new Fixture(); var intent = new SetActiveFieldRequest { FieldId = "B", Context = f.Data.Review };
        var response = new SetActiveFieldResponse { Success = true, FieldId = "B", Context = f.Data.Review with { FieldId = "B", Revision = "new" } };
        if (fault == "id") response.FieldId = "A"; if (fault == "actor") response.Context = response.Context with { ActorUserId = "other" };
        if (fault == "database") response.Context = response.Context with { Database = new("other", "different") };
        if (fault == "revision") response.Context = response.Context with { Revision = f.Data.Review.Revision }; if (fault == "failure") response.Success = false; if (fault == "missing") response.Context = null;
        Assert.Throws<InvalidOperationException>(() => FieldSelectionReviewValidator.RequireAcknowledgement(response, intent));
    }
    private sealed class Fixture : IDisposable
    {
        public CompanyClientContext Company { get; } = new(); public SelectionFacet Data { get; } = new(); public FieldSelectionState State { get; }
        public Fixture() { SelectCompany(); State = new(Company, Data, (_, _, sentence) => sentence); }
        public void SelectCompany() => Assert.True(Company.TrySelect(Company.Clear(), "actor", new(new("tenant", "company"), ["Reader"], [])));
        public void Dispose() { State.Dispose(); Company.Dispose(); }
    }
    private sealed class SelectionFacet : IFieldSelectionContext
    {
        public FieldSelectionReview Review { get; } = new(new("tenant", "company"), new("connection", "fingerprint"), "actor", "revision", "A");
        public List<FieldListItem> Rows { get; set; } = [new() { FieldId = "A", FieldName = "Alpha" }, new() { FieldId = "B", FieldName = "Beta" }];
        public FieldResponse Current { get; } public Func<CancellationToken, Task<List<FieldListItem>>>? Read;
        public Func<CancellationToken, Task<FieldResponse>>? Selected; public Func<SetActiveFieldRequest, Task<bool>>? Save;
        public List<SetActiveFieldRequest> Commands { get; } = []; public int CurrentReads;
        public event Action<string>? CurrentFieldChanged;
        public SelectionFacet() { Current = new() { FieldId = "A", SelectionAvailable = true, SelectionContext = Review }; }
        public Task<string?> GetCurrentFieldIdAsync() => Task.FromResult<string?>(Current.SelectionAvailable ? Current.FieldId : null);
        public Task<List<FieldListItem>> GetAvailableFieldsAsync(CancellationToken cancellationToken = default) => Read?.Invoke(cancellationToken) ?? Task.FromResult(Rows);
        public Task<FieldResponse> GetCurrentFieldSelectionAsync(CancellationToken cancellationToken = default) { ++CurrentReads; return Selected?.Invoke(cancellationToken) ?? Task.FromResult(Current); }
        public Task<bool> SetCurrentFieldAsync(SetActiveFieldRequest intent, CancellationToken cancellationToken = default) { Commands.Add(intent); return Save?.Invoke(intent) ?? Task.FromResult(true); }
        public void Publish(string id) => CurrentFieldChanged?.Invoke(id);
    }
}
