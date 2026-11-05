using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.Web.Services;
using Moq;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public sealed class InvestigatorAssignmentStateTests
{
    public sealed class MemoryStore:IInvestigatorAssignmentIntentStore
    {
        private readonly Dictionary<string,InvestigatorAssignmentIntent> entries=[];
        private static string Key(FieldSelectionReview context,string incident)=>$"{context.Company.TenantId}|{context.Company.BusinessAssociateId}|{context.ActorUserId}|{context.Database.Fingerprint}|{context.FieldId}|{incident}";
        public Task<InvestigatorAssignmentIntent?> ReadAsync(FieldSelectionReview context,string incident)=>Task.FromResult(entries.GetValueOrDefault(Key(context,incident)));
        public Task RetainAsync(InvestigatorAssignmentIntent original){entries[Key(original.Review.Context,original.Review.IncidentId)]=original;return Task.CompletedTask;}
        public Task ClearAsync(InvestigatorAssignmentIntent original){entries.Remove(Key(original.Review.Context,original.Review.IncidentId));return Task.CompletedTask;}
    }
    private static readonly RecordedIncidentParticipant Witness = new(17, 4, "person", "WITNESS");
    private static readonly FieldSelectionReview Frame = new(new("tenant", "company"), new("physical", "database"), "actor", "selection-revision", "A");
    private static InvestigatorAssignmentReview Review(string incident = "local", string revision = "A", RecordedIncidentParticipant? current = null) =>
        new(Frame, incident, new string(revision[0],64), current is null ? [Witness] : [Witness, current], current);
    private sealed class Fixture : IDisposable
    {
        public CompanyClientContext Company { get; } = new();
        public Mock<IFieldSelectionContext> Fields { get; } = new(MockBehavior.Strict);
        public Mock<IHSEServiceClient> Api { get; } = new(MockBehavior.Strict);
        public InvestigatorAssignmentState State { get; }
        public FieldResponse Selected { get; set; } = new() { SelectionAvailable = true, FieldId = "A", SelectionContext = Frame };
        public Func<string, Task<InvestigatorAssignmentReview>> Read { get; set; } = id => Task.FromResult(Review(id));
        public Func<InvestigatorAssignmentReview, RecordedIncidentParticipant, Task> Write { get; set; } = (_,_) => Task.CompletedTask;
        public int Reads { get; private set; } public int Writes { get; private set; }
        public Fixture(bool manage = true)
        {
            SelectCompany("company", manage);
            Fields.Setup(value => value.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).Returns(() => Task.FromResult(Selected));
            Api.Setup(value => value.ReviewInvestigatorAssignmentAsync(It.IsAny<string>(), "A", It.IsAny<CancellationToken>())).Returns((string id, string field, CancellationToken token) => { Reads++; return Read(id); });
            Api.Setup(value => value.AssignInvestigatorAsync(It.IsAny<InvestigatorAssignmentReview>(), It.IsAny<RecordedIncidentParticipant>(), It.IsAny<CancellationToken>())).Returns((InvestigatorAssignmentReview review, RecordedIncidentParticipant participant, CancellationToken token) => { Writes++; return Write(review,participant); });
            State = new(Company, Fields.Object, Api.Object, (_,_,message) => message,new MemoryStore());
        }
        public void SelectCompany(string id, bool manage = true)
        { var version = Company.Clear(); Assert.True(Company.TrySelect(version, "actor", new(new("tenant",id), ["SafetyOfficer"], manage ? [PermissionConstants.HSE.View, PermissionConstants.HSE.ManageIncidents] : [PermissionConstants.HSE.View]))); }
        public async Task Prepare() { await State.LoadAsync("local"); Assert.True(State.Loaded); Assert.True(State.Select(Assert.Single(State.Participants))); }
        public void Dispose() { State.Dispose(); Company.Dispose(); }
    }
    [Fact]
    public async Task Read_only_review_supplies_frozen_complete_role_choices_without_selecting_or_writing()
    {
        using var f = new Fixture(); await f.State.LoadAsync("local");
        Assert.True(f.State.Loaded); Assert.Equal(Witness, Assert.Single(f.State.Participants)); Assert.NotSame(Witness, f.State.Participants[0]);
        Assert.Null(f.State.Selected); Assert.False(f.State.CanAssign); Assert.Equal(1,f.Reads); Assert.Equal(0,f.Writes);
    }
    [Fact]
    public async Task Submission_is_one_frozen_original_review_and_acknowledgment_requires_explicit_outcome_review()
    {
        using var f = new Fixture(); await f.Prepare();
        f.Write = (review,participant) => { Assert.Equal(new string('A',64),review.Revision); Assert.Equal(Witness,participant); Assert.Equal("local",review.IncidentId); return Task.CompletedTask; };
        await f.State.AssignAsync(); await f.State.AssignAsync();
        Assert.Equal(1,f.Writes); Assert.Equal(1,f.Reads); Assert.False(f.State.Loaded); Assert.Empty(f.State.Participants); Assert.True(f.State.OutcomeReviewRequired);
        Assert.Contains("Review the recorded outcome",f.State.Message!); Assert.False(f.State.CanAssign);
    }
    [Fact]
    public async Task Lost_response_retains_the_outcome_and_unchanged_evidence_never_unlocks_retry()
    {
        using var f = new Fixture(); await f.Prepare(); f.Write = (_,_) => throw new HttpRequestException("lost acknowledgment");
        await f.State.AssignAsync(); Assert.True(f.State.OutcomeReviewRequired); Assert.NotNull(f.State.Error);
        await f.State.LoadAsync("local"); Assert.True(f.State.OutcomeReviewRequired); Assert.False(f.State.Select(f.State.Participants[0]));
        await f.State.AssignAsync(); Assert.Equal(1,f.Writes); Assert.Contains("remains unconfirmed",f.State.Message!);
        var investigator = new RecordedIncidentParticipant(17,5,"person",HseReferenceCodes.Investigator.Role);
        f.Read = id => Task.FromResult(Review(id,"B",investigator)); await f.State.LoadAsync("local");
        Assert.False(f.State.OutcomeReviewRequired); Assert.Equal(investigator,f.State.CurrentInvestigator);
        Assert.Contains("currently recorded investigator matches",f.State.Message!); Assert.Null(f.State.Selected); Assert.False(f.State.CanAssign);
    }
    [Fact]
    public async Task Changed_roster_review_does_not_attribute_the_earlier_submission_or_keep_its_selection()
    {
        using var f = new Fixture(); await f.Prepare(); await f.State.AssignAsync();
        var investigator = new RecordedIncidentParticipant(21,7,"other-person",HseReferenceCodes.Investigator.Role);
        f.Read = id => Task.FromResult(Review(id,"B",investigator)); await f.State.LoadAsync("local");
        Assert.True(f.State.OutcomeReviewRequired); Assert.Null(f.State.Selected); Assert.Contains("remains unconfirmed",f.State.Message!); Assert.Equal(1,f.Writes);
    }
    [Theory] [InlineData("company")] [InlineData("field")]
    public async Task Switching_away_and_back_discards_a_late_review(string boundary)
    {
        using var f = new Fixture(); var response = new TaskCompletionSource<InvestigatorAssignmentReview>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Read = _ => response.Task; var task = f.State.LoadAsync("local"); Assert.True(f.State.Loading);
        if (boundary == "company") { f.SelectCompany("other"); f.SelectCompany("company"); }
        else { f.Fields.Raise(value => value.CurrentFieldChanged += null,"B"); f.Fields.Raise(value => value.CurrentFieldChanged += null,"A"); }
        response.SetResult(Review()); await task;
        Assert.False(f.State.Loaded); Assert.Empty(f.State.Participants); Assert.Null(f.State.Selected); Assert.Null(f.State.Message); Assert.Equal(0,f.Writes);
    }
    [Theory] [InlineData("company")] [InlineData("field")] [InlineData("incident")]
    public async Task In_flight_write_keeps_the_reservation_across_navigation_and_cannot_confirm_a_new_context(string boundary)
    {
        using var f = new Fixture(); await f.Prepare(); var response = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); f.Write = (_,_) => response.Task;
        var save = f.State.AssignAsync(); Assert.True(f.State.Saving);
        if (boundary == "company") { f.SelectCompany("other"); f.SelectCompany("company"); }
        else if (boundary == "field") { f.Fields.Raise(value => value.CurrentFieldChanged += null,"B"); f.Fields.Raise(value => value.CurrentFieldChanged += null,"A"); }
        else await f.State.LoadAsync("other");
        Assert.True(f.State.Saving); Assert.Empty(f.State.Participants); Assert.False(f.State.CanAssign); await f.State.AssignAsync(); Assert.Equal(1,f.Writes);
        response.SetResult(); await save; Assert.False(f.State.Saving); Assert.Null(f.State.Message); Assert.False(f.State.Loaded);
        await f.State.LoadAsync("local"); Assert.True(f.State.OutcomeReviewRequired); Assert.False(f.State.CanAssign);
    }
    [Fact]
    public async Task A_stale_choice_reference_cannot_adopt_a_new_review_even_when_all_recorded_values_match()
    {
        using var f = new Fixture(); await f.Prepare(); var old = f.State.Selected;
        await f.State.LoadAsync("local"); Assert.False(f.State.Select(old)); Assert.False(f.State.CanAssign);
        Assert.False(f.State.Select(f.State.Participants[0] with { })); Assert.Equal(0,f.Writes);
    }
    [Fact]
    public async Task Silent_FIELD_review_revision_change_before_submission_prevents_any_write()
    {
        using var f = new Fixture(); await f.Prepare(); f.Selected.SelectionContext = Frame with { Revision = "changed" };
        await f.State.AssignAsync(); Assert.Equal(0,f.Writes); Assert.False(f.State.OutcomeReviewRequired); Assert.False(f.State.Loaded); Assert.NotNull(f.State.Error);
    }
    [Fact]
    public async Task Presentation_permission_hints_cannot_enable_a_write_without_manage_permission()
    {
        using var f = new Fixture(false); await f.Prepare(); Assert.False(f.State.CanAssign); await f.State.AssignAsync(); Assert.Equal(0,f.Writes);
    }
    [Theory] [InlineData("field")] [InlineData("incident")] [InlineData("role")] [InlineData("duplicate-key")]
    public async Task Malformed_or_cross_context_review_clears_evidence_instead_of_retaining_choices(string fault)
    {
        using var f = new Fixture(); var review = Review(); review = fault switch {
            "field" => review with { Context = Frame with { FieldId = "B" } }, "incident" => review with { IncidentId = "other" },
            "role" => review with { CurrentInvestigator = Witness }, _ => review with { Participants = [Witness, Witness with { BusinessAssociateId = "other" }] }
        };
        f.Read = _ => Task.FromResult(review); await f.State.LoadAsync("local");
        Assert.False(f.State.Loaded); Assert.Empty(f.State.Participants); Assert.NotNull(f.State.Error); Assert.Equal(0,f.Writes);
    }
    [Fact]
    public async Task No_selected_FIELD_produces_explicit_selection_state_without_reading_incident_participants()
    {
        using var f = new Fixture(); f.Selected = new() { SelectionAvailable = false, FieldId = "", SelectionContext = Frame with { FieldId = null } };
        await f.State.LoadAsync("local"); Assert.True(f.State.NeedsField); Assert.False(f.State.Loaded); Assert.Equal(0,f.Reads); Assert.Equal(0,f.Writes);
    }
    [Fact]
    public async Task Disposal_discards_a_late_read_and_unsubscribes_selection_events()
    {
        using var f = new Fixture(); var response = new TaskCompletionSource<InvestigatorAssignmentReview>(TaskCreationOptions.RunContinuationsAsynchronously); f.Read = _ => response.Task;
        var task = f.State.LoadAsync("local"); f.State.Dispose(); response.SetResult(Review()); await task;
        Assert.False(f.State.Loaded); Assert.Empty(f.State.Participants); f.SelectCompany("other"); Assert.False(f.State.CanAssign);
    }
    [Fact]
    public async Task Missing_company_never_reads_FIELD_or_incident_evidence()
    {
        using var f = new Fixture(); f.Company.Clear(); await f.State.LoadAsync("local");
        Assert.True(f.State.NeedsCompany); Assert.False(f.State.Loaded); Assert.Empty(f.State.Participants); Assert.Equal(0,f.Reads); Assert.Equal(0,f.Writes);
        f.Fields.Verify(value => value.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task A_failed_refresh_cannot_retain_previous_choices_or_selection()
    {
        using var f = new Fixture(); await f.Prepare(); f.Read = _ => throw new HttpRequestException("read failed"); await f.State.LoadAsync("local");
        Assert.False(f.State.Loaded); Assert.Empty(f.State.Participants); Assert.Null(f.State.Selected); Assert.Null(f.State.CurrentInvestigator); Assert.NotNull(f.State.Error); Assert.False(f.State.CanAssign); Assert.Equal(0,f.Writes);
    }
    [Fact]
    public async Task Failed_outcome_review_preserves_the_original_uncertain_reservation()
    {
        using var f = new Fixture(); await f.Prepare(); f.Write = (_,_) => throw new HttpRequestException("lost"); await f.State.AssignAsync();
        f.Read = _ => throw new HttpRequestException("outcome read failed"); await f.State.LoadAsync("local");
        Assert.True(f.State.OutcomeReviewRequired); Assert.False(f.State.Loaded); Assert.Empty(f.State.Participants); Assert.NotNull(f.State.Error);
        await f.State.AssignAsync(); Assert.Equal(1,f.Writes);
    }

}
