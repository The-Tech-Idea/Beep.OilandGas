#if RECORDED_APPROVAL_HTTP_FOCUSED
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.Repository.Tests;
public sealed class PermitApplicationWorkflowStateTests
{
    private const string Id="permit/شركة?one";private const string Reason="Reviewed the recorded application and source history.";
    private sealed class Auth:AuthenticationStateProvider
    {
        public ClaimsPrincipal Principal=new(new ClaimsIdentity([new Claim("party_id","actor"),new Claim(ClaimTypes.Role,"Safety")],"test"));
        public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(Principal));
    }
    private sealed class Memory:IPermitApplicationIntentStore
    {
        public PermitApplicationCommand? Stored;public bool SaveFail,ClearFail;public int Clears;public Action? AfterSave;
        public Task<PermitApplicationCommand?> ReadAsync(FieldSelectionReview field,string id)=>Task.FromResult(Stored is {} original && original.Review.Context.Field.Company==field.Company && original.Review.Context.Field.ActorUserId==field.ActorUserId && original.Review.Context.Field.FieldId==field.FieldId && original.Review.ReadApplication().PERMIT_APPLICATION_ID==id?Stored:null);
        public Task<PermitApplicationCommand?> ReadCreationAsync(FieldSelectionReview field)=>Task.FromResult(Stored is {Action:"CREATE_DRAFT"} original && original.Review.Context.Field.Company==field.Company && original.Review.Context.Field.ActorUserId==field.ActorUserId && original.Review.Context.Field.FieldId==field.FieldId?Stored:null);
        public Task RetainAsync(PermitApplicationCommand original){if(SaveFail)throw new IOException("Storage unavailable");if(Stored is not null && Stored!=original)throw new InvalidOperationException("Different original");Stored=original;AfterSave?.Invoke();return Task.CompletedTask;}
        public Task ClearAsync(PermitApplicationCommand original){if(ClearFail)throw new IOException("Cleanup acknowledgement lost");Assert.Equal(original,Stored);Stored=null;Clears++;return Task.CompletedTask;}
    }
    private sealed class Fixture:IDisposable
    {
        public CompanyClientContext Company=new();public Mock<IFieldSelectionContext> Fields=new();public Auth Authentication=new();public Mock<IPermitServiceClient> Client=new();public Memory Store=new();
        public FieldSelectionReview Field=new(new("tenant","company"),new("core","core-binding"),"actor","selection","A");
        public string? WellId;public DRILLING_PERMIT_APPLICATION? Drilling;public ENVIRONMENTAL_PERMIT_APPLICATION? Environmental;public INJECTION_PERMIT_APPLICATION? Injection;
        public APPLICATION_ATTACHMENT[] Attachments=[];
        public PermitApplicationType ApplicationType=PermitApplicationType.Drilling;
        public PermitApplicationStatus Status=PermitApplicationStatus.Draft;public string Submitter="";public bool LoseAck,FailBeforeEffect;
        public List<PermitApplicationCommand> Sent=[];public Dictionary<Guid,PermitApplicationCommandOutcome> Effects=[];public Func<PermitApplicationCommand,Task<PermitApplicationCommandOutcome>>? Delayed;
        public Fixture()
        {
            Select(true);
            Fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(()=>new FieldResponse{FieldId=Field.FieldId!,SelectionAvailable=true,SelectionContext=Field});
            Client.Setup(x=>x.ReviewCreationAsync(It.IsAny<PermitApplicationCreationProposal>(),It.IsAny<FieldSelectionReview>(),It.IsAny<CancellationToken>())).ReturnsAsync((PermitApplicationCreationProposal proposal,FieldSelectionReview field,CancellationToken _)=>PermitApplicationCreationReview.Capture(new(field,new("permits","permit-binding")),proposal,"field-source",proposal.WellId is null?null:"well-source",proposal.FacilityId is null?null:"facility-source"));
            Client.Setup(x=>x.ReviewSourceAsync(Id,It.IsAny<FieldSelectionReview>(),It.IsAny<CancellationToken>())).ReturnsAsync(()=>Source());
            Client.Setup(x=>x.RecordCommandAsync(It.IsAny<PermitApplicationCommand>(),It.IsAny<CancellationToken>())).Returns(async(PermitApplicationCommand original,CancellationToken _) =>
            {
                Assert.Equal(original,Store.Stored);Sent.Add(original);
                if(Delayed is not null)return await Delayed(original);
                if(FailBeforeEffect)throw new HttpRequestException("Before effect");
                if(!Effects.TryGetValue(original.IntentId,out var result)){result=PermitApplicationCommandOutcome.Prepare(original,DateTimeOffset.UtcNow);Effects.Add(original.IntentId,result);}
                if(LoseAck){LoseAck=false;throw new HttpRequestException("Commit acknowledgement lost");}return result;
            });
            Client.Setup(x=>x.RecoverCommandAsync(It.IsAny<PermitApplicationCommand>(),It.IsAny<CancellationToken>())).ReturnsAsync((PermitApplicationCommand original,CancellationToken _)=>new PermitApplicationCommandRecovery(original,Effects.GetValueOrDefault(original.IntentId)));
        }
        public void Select(bool write,string company="company")=>Assert.True(Company.TrySelect(Company.Clear(),"actor",new(new("tenant",company),["Safety"],write?[PermissionConstants.HSE.ViewPermits,PermissionConstants.HSE.IssuePermit,PermissionConstants.HSE.ApprovePermit]:[PermissionConstants.HSE.ViewPermits])));
        public PermitApplicationSourceReview Source()=>PermitApplicationSourceReview.Capture(new(Field,new("permits","permit-binding")),new PERMIT_APPLICATION{PERMIT_APPLICATION_ID=Id,OWNER_TENANT_ID=Field.Company.TenantId,OWNER_COMPANY_ID=Field.Company.BusinessAssociateId,FIELD_ID=Field.FieldId!,APPLICANT_ID="Recorded applicant",APPLICATION_TYPE=ApplicationType,WELL_ID=WellId,COUNTRY=Country.Kuwait,STATUS=Status,SUBMITTED_BY_USER_ID=Submitter},[],PermitApplicationSupportingSnapshot.Capture(new PERMIT_APPLICATION{PERMIT_APPLICATION_ID=Id},Drilling,Environmental,Injection,Attachments));
        public PermitApplicationCommandState State()=>new(Company,Fields.Object,Authentication,Client.Object,Store);
        public void Dispose()=>Company.Dispose();
    }
    [Theory][InlineData("submit")][InlineData("approve")][InlineData("reject")][InlineData("self")][InlineData("terminal")][InlineData("readonly")]
    public async Task PermitWorkbenchUsesSharedStageIndependenceAndRoleEligibility(string mode)
    {
        using var f=new Fixture();if(mode is "approve" or "reject" or "self"){f.Status=PermitApplicationStatus.Submitted;f.Submitter=mode=="self"?"actor":"issuer";}if(mode=="terminal")f.Status=PermitApplicationStatus.Approved;if(mode=="readonly")f.Select(false);
        using var state=f.State();await state.AttachAsync(Id);await state.ReviewAsync();
        var action=mode=="approve"?"APPROVE":mode=="reject"?"REJECT":"SUBMIT";
        if(mode is "self" or "terminal" or "readonly"){Assert.False(state.CanPrepare(action));await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordAsync(action,Reason,true));Assert.Empty(f.Sent);return;}
        Assert.True(state.CanPrepare(action));await state.RecordAsync(action,Reason,true);Assert.Single(f.Effects);Assert.Single(f.Sent);Assert.Null(f.Store.Stored);Assert.False(state.HasPending);Assert.Equal(action,state.Outcome!.Original.Action);
    }
    [Theory][InlineData("reason")][InlineData("confirmation")][InlineData("storage")][InlineData("context-before-send")]
    public async Task PermitWorkbenchRetainsBeforeTransportAndRefusesInvalidOrUnretainedCommands(string mode)
    {
        using var f=new Fixture();using var state=f.State();await state.AttachAsync(Id);await state.ReviewAsync();
        if(mode=="storage")f.Store.SaveFail=true;if(mode=="context-before-send")f.Store.AfterSave=()=>f.Select(true,"other");
        await Assert.ThrowsAnyAsync<Exception>(()=>state.RecordAsync("SUBMIT",mode=="reason"?"short":Reason,mode!="confirmation"));
        Assert.Empty(f.Sent);Assert.Empty(f.Effects);
        if(mode=="storage"){Assert.True(state.HasPending);Assert.Contains("No submission was sent",state.OriginalExplanation);}
        if(mode=="context-before-send"){Assert.NotNull(f.Store.Stored);Assert.Null(state.VisibleOriginal);Assert.Null(state.Review);}
    }
    [Theory][InlineData("reload-readonly")][InlineData("cleanup")][InlineData("absence-resend")][InlineData("late-response")][InlineData("binding")]
    public async Task PermitWorkbenchPreservesUncertaintyAndRecoversTheSameOriginal(string mode)
    {
        using var f=new Fixture();using var state=f.State();await state.AttachAsync(Id);await state.ReviewAsync();
        if(mode=="late-response")
        {
            var started=new TaskCompletionSource<PermitApplicationCommand>(TaskCreationOptions.RunContinuationsAsynchronously);var release=new TaskCompletionSource<PermitApplicationCommandOutcome>(TaskCreationOptions.RunContinuationsAsynchronously);
            f.Delayed=original=>{started.SetResult(original);return release.Task;};var send=state.RecordAsync("SUBMIT",Reason,true);var original=await started.Task;f.Select(true,"other");release.SetResult(PermitApplicationCommandOutcome.Prepare(original,DateTimeOffset.UtcNow));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>send);Assert.NotNull(f.Store.Stored);Assert.Equal(0,f.Store.Clears);Assert.Null(state.Outcome);Assert.Null(state.VisibleOriginal);return;
        }
        if(mode=="cleanup")f.Store.ClearFail=true;else if(mode=="absence-resend")f.FailBeforeEffect=true;else f.LoseAck=true;
        await Assert.ThrowsAnyAsync<Exception>(()=>state.RecordAsync("SUBMIT",Reason,true));var pending=f.Store.Stored!;Assert.NotNull(pending);Assert.False(state.CanReview);
        if(mode=="binding")
        {
            f.Field=f.Field with{Database=new("different-core","different-binding")};f.Fields.Raise(x=>x.CurrentFieldChanged+=null,"A");await state.AttachAsync(Id);
            Assert.True(state.HasPending);Assert.Null(state.VisibleOriginal);Assert.False(state.CanRecover);Assert.False(state.CanReview);Assert.Equal(pending,f.Store.Stored);return;
        }
        if(mode=="reload-readonly")
        {
            f.Select(false);using var restored=f.State();await restored.AttachAsync(Id);Assert.True(restored.CanRecover);Assert.False(restored.CanPrepare("SUBMIT"));var sent=f.Sent.Count;await restored.RecoverAsync();Assert.Equal(sent,f.Sent.Count);Assert.Equal(pending,restored.Outcome!.Original);Assert.Null(f.Store.Stored);return;
        }
        if(mode=="cleanup"){Assert.NotNull(state.Outcome);Assert.True(state.HasPending);f.Store.ClearFail=false;await state.RecoverAsync();Assert.False(state.HasPending);Assert.Single(f.Sent);return;}
        await state.RecoverAsync();Assert.True(state.AbsenceObserved);Assert.True(state.HasPending);Assert.Equal(pending,f.Store.Stored);Assert.False(state.CanReview);
        // The old request can commit after an absent observation; only the retained id may be resent.
        f.Effects.Add(pending.IntentId,PermitApplicationCommandOutcome.Prepare(pending,DateTimeOffset.UtcNow));f.FailBeforeEffect=false;
        await state.ResendAsync();Assert.Equal(2,f.Sent.Count);Assert.All(f.Sent,x=>Assert.Equal(pending,x));Assert.Single(f.Effects);Assert.False(state.HasPending);
    }
    private static PermitApplicationDraftChanges Changes(PermitApplicationSourceReview review)
    {
        var source=review.ReadApplication();
        return new(source.APPLICATION_TYPE,source.REGULATORY_AUTHORITY,source.COUNTRY,source.STATE_PROVINCE,"Revised applicant","Reviewed draft description");
    }
    [Theory][InlineData("draft")][InlineData("correction")][InlineData("missing")][InlineData("noop")][InlineData("invalid")][InlineData("submitted")][InlineData("injected-submit")][InlineData("rewritten-result")]
    public void ReviewedPermitDraftProposalPreservesLifecycleAndRejectsInvalidOrRewrittenEvidence(string mode)
    {
        using var f=new Fixture();
        if(mode=="correction")f.Status=PermitApplicationStatus.AdditionalInformationRequired;
        if(mode=="submitted"){f.Status=PermitApplicationStatus.Submitted;f.Submitter="issuer";}
        var review=f.Source();var source=review.ReadApplication();var changes=Changes(review);
        if(mode=="noop")changes=changes with{Applicant=source.APPLICANT_ID,Description=source.REMARKS};
        if(mode=="invalid")changes=changes with{Country=Country.Other};
        var command=new PermitApplicationCommand(Guid.NewGuid(),review,mode=="injected-submit"?"SUBMIT":"EDIT_DRAFT",Reason){DraftChanges=mode=="missing"?null:changes};
        if(mode is "missing" or "noop" or "invalid" or "submitted" or "injected-submit")
        {Assert.Throws<InvalidOperationException>(command.Require);return;}
        command.Require();var outcome=PermitApplicationCommandOutcome.Prepare(command,DateTimeOffset.UtcNow);outcome.Require();var saved=outcome.ReadApplication();
        Assert.Equal(source.STATUS,saved.STATUS);Assert.Equal(source.SUBMITTED_BY_USER_ID,saved.SUBMITTED_BY_USER_ID);Assert.Equal(source.SUBMITTED_DATE,saved.SUBMITTED_DATE);
        Assert.Equal(source.DECISION,saved.DECISION);Assert.Equal(source.DECISION_DATE,saved.DECISION_DATE);
        var expected=review.ReadApplication();changes.Apply(expected);expected.ROW_CHANGED_BY=saved.ROW_CHANGED_BY;expected.ROW_CHANGED_DATE=saved.ROW_CHANGED_DATE;
        Assert.Equal(PermitApplicationCommandOutcome.Payload(expected),PermitApplicationCommandOutcome.Payload(saved));
        Assert.Equal(changes,JsonSerializer.Deserialize<PermitApplicationCommand>(JsonSerializer.Serialize(command))!.DraftChanges);
        if(mode=="rewritten-result")
        {saved.FIELD_ID="foreign";var rewritten=outcome with{ApplicationJson=JsonSerializer.Serialize(saved)};Assert.Throws<InvalidOperationException>(rewritten.Require);}
    }
    [Theory][InlineData("confirmed")][InlineData("lost-response")][InlineData("readonly")]
    public async Task ReviewedPermitDraftRetainsExactProposalAndRecoversWithoutAnotherWrite(string mode)
    {
        using var f=new Fixture();using var state=f.State();if(mode=="readonly")f.Select(false);
        await state.AttachAsync(Id);await state.ReviewAsync();var changes=Changes(state.Review!);
        if(mode=="readonly")
        {Assert.False(state.CanPrepare("EDIT_DRAFT"));await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordAsync("EDIT_DRAFT",Reason,true,changes));Assert.Empty(f.Sent);return;}
        Assert.True(state.CanPrepare("EDIT_DRAFT"));f.LoseAck=mode=="lost-response";
        if(mode=="lost-response")
        {
            await Assert.ThrowsAsync<HttpRequestException>(()=>state.RecordAsync("EDIT_DRAFT",Reason,true,changes));
            var original=f.Store.Stored!;Assert.Equal(changes,original.DraftChanges);f.Select(false);
            using var restored=f.State();await restored.AttachAsync(Id);Assert.True(restored.CanRecover);await restored.RecoverAsync();
            Assert.Equal(original,restored.Outcome!.Original);Assert.Equal(changes.Applicant,restored.Outcome.ReadApplication().APPLICANT_ID);Assert.Null(f.Store.Stored);
        }
        else {await state.RecordAsync("EDIT_DRAFT",Reason,true,changes);Assert.Equal(changes,state.Outcome!.Original.DraftChanges);}
        Assert.Single(f.Sent);Assert.Single(f.Effects);
    }
    [Theory][InlineData("valid")][InlineData("half-facility")][InlineData("empty-well")][InlineData("invalid-header")][InlineData("changed-proposal")][InlineData("changed-source")][InlineData("missing-resource")][InlineData("foreign-selection")]
    public void PermitCreationReviewRequiresCompleteIdentitiesAndFrozenProposalContext(string mode)
    {
        using var f=new Fixture();var proposal=new PermitApplicationCreationProposal(Changes(f.Source()),"well","shared","gas");
        if(mode=="half-facility")proposal=proposal with{FacilityType=null};if(mode=="empty-well")proposal=proposal with{WellId=""};
        if(mode=="invalid-header")proposal=proposal with{Header=proposal.Header with{Country=Country.Other}};
        if(mode is "half-facility" or "empty-well" or "invalid-header"){Assert.Throws<InvalidOperationException>(proposal.Require);return;}
        if(mode=="missing-resource"){Assert.Throws<InvalidOperationException>(()=>PermitApplicationCreationReview.Capture(f.Source().Context,proposal,"field-source",null,"facility-source"));return;}
        var review=PermitApplicationCreationReview.Capture(f.Source().Context,proposal,"field-source","well-source","facility-source");
        if(mode=="changed-source"){Assert.Throws<InvalidOperationException>((review with{LinkedSourcesJson=review.LinkedSourcesJson.Replace("well-source","other-source")}).Require);return;}
        if(mode=="changed-proposal"){Assert.Throws<InvalidOperationException>(()=>review.RequireFor(proposal with{FacilityType="oil"},f.Field));return;}
        if(mode=="foreign-selection"){Assert.Throws<InvalidOperationException>(()=>review.RequireFor(proposal,f.Field with{Company=new("tenant","foreign")}));return;}
        review.RequireFor(proposal,f.Field);Assert.Equal(review,JsonSerializer.Deserialize<PermitApplicationCreationReview>(JsonSerializer.Serialize(review)));
    }
    [Theory][InlineData("valid")][InlineData("status")][InlineData("owner")][InlineData("asset")][InlineData("audit")][InlineData("history")][InlineData("intent")][InlineData("missing-review")][InlineData("misplaced-review")][InlineData("changed-result")][InlineData("created-audit")]
    public void PermitCreationOriginalAndOutcomeRefuseUnreviewedIdentityLifecycleResourcesAndAudit(string mode)
    {
        using var f=new Fixture();var proposal=new PermitApplicationCreationProposal(Changes(f.Source()),"well","shared","gas");
        var creation=PermitApplicationCreationReview.Capture(f.Source().Context,proposal,"field-source","well-source","facility-source");
        var original=creation.PrepareOriginal(Guid.NewGuid(),Reason);var row=original.Review.ReadApplication();
        if(mode=="status")row.STATUS=PermitApplicationStatus.Submitted;
        if(mode=="owner")row.OWNER_COMPANY_ID="foreign";
        if(mode=="asset")row.FACILITY_TYPE="oil";
        if(mode=="audit")row.ROW_CREATED_BY="forged";
        if(mode is "status" or "owner" or "asset" or "audit" or "history")
        {
            if(mode=="owner"){Assert.Throws<InvalidOperationException>(()=>PermitApplicationSourceReview.Capture(original.Review.Context,row,[]));return;}
            var history=mode=="history"?new[]{new PERMIT_STATUS_HISTORY{PERMIT_STATUS_HISTORY_ID="injected",PERMIT_APPLICATION_ID=row.PERMIT_APPLICATION_ID,ACTIVE_IND="Y"}}:Array.Empty<PERMIT_STATUS_HISTORY>();
            original=original with{Review=PermitApplicationSourceReview.Capture(original.Review.Context,row,history)};
        }
        if(mode=="intent")original=original with{IntentId=Guid.NewGuid()};if(mode=="missing-review")original=original with{CreationReview=null};
        if(mode=="misplaced-review")original=original with{Action="SUBMIT"};
        if(mode is "status" or "asset" or "audit" or "history" or "intent" or "missing-review" or "misplaced-review")
        {Assert.Throws<InvalidOperationException>(original.Require);return;}
        original.Require();var outcome=PermitApplicationCommandOutcome.Prepare(original,DateTimeOffset.UtcNow);var saved=outcome.ReadApplication();
        Assert.Equal("PA-"+original.IntentId.ToString("N"),saved.PERMIT_APPLICATION_ID);Assert.Equal(PermitApplicationStatus.Draft,saved.STATUS);
        Assert.Equal("gas",saved.FACILITY_TYPE);Assert.Equal("actor",saved.ROW_CREATED_BY);Assert.Equal(outcome.RecordedAtUtc.UtcDateTime,saved.CREATED_DATE);
        Assert.False(saved.SUBMISSION_COMPLETE);Assert.Equal("N",saved.SUBMISSION_COMPLETE_IND);Assert.Empty(saved.SUBMITTED_BY_USER_ID);Assert.Null(saved.DECISION_DATE);
        if(mode=="changed-result")saved.FACILITY_TYPE="oil";if(mode=="created-audit")saved.ROW_CREATED_DATE=outcome.RecordedAtUtc.UtcDateTime.AddDays(-1);
        if(mode is "changed-result" or "created-audit")Assert.Throws<InvalidOperationException>((outcome with{ApplicationJson=JsonSerializer.Serialize(saved)}).Require);
        else{Assert.Equal(original,JsonSerializer.Deserialize<PermitApplicationCommand>(JsonSerializer.Serialize(original)));outcome.Require();}
    }
    [Fact]
    public void PermitCreationCompositeFacilityColumnIsOwnedByExistingBeepMigrationManifest()
    {
        Assert.Contains(typeof(PERMIT_APPLICATION),Beep.OilandGas.PermitsAndApplications.Modules.PermitsModule.OwnedEntities);
        var column=typeof(PERMIT_APPLICATION).GetProperty("FACILITY_TYPE")!;Assert.Equal(typeof(string),column.PropertyType);
        Assert.Equal(128,column.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.MaxLengthAttribute),true).Cast<System.ComponentModel.DataAnnotations.MaxLengthAttribute>().Single().Length);
    }
    private static PermitApplicationCreationProposal CreationValues()=>new(new(default,default,Country.Kuwait,default,"Reviewed applicant","Draft scope"),"well","shared","gas");
    [Theory][InlineData("confirmed")][InlineData("reload-readonly")][InlineData("absence-resend")][InlineData("storage")][InlineData("readonly")][InlineData("changed-context")]
    public async Task PermitCreationStateRetainsOneOriginalAndRecoversWithoutReplacingItsProposal(string mode)
    {
        using var f=new Fixture();if(mode=="readonly")f.Select(false);using var state=f.State();await state.AttachCreationAsync();
        if(mode=="readonly"){Assert.False(state.CanReviewCreation);await Assert.ThrowsAsync<InvalidOperationException>(()=>state.ReviewCreationAsync(CreationValues()));Assert.Empty(f.Sent);return;}
        await state.ReviewCreationAsync(CreationValues());Assert.True(state.CanPrepare("CREATE_DRAFT"));Assert.False(state.CanPrepare("SUBMIT"));
        if(mode=="confirmed")
        {await state.RecordAsync("CREATE_DRAFT",Reason,true);Assert.Equal("CREATE_DRAFT",state.Outcome!.Original.Action);Assert.Single(f.Sent);Assert.Single(f.Effects);return;}
        if(mode=="changed-context")
        {f.Select(true,"foreign");Assert.False(state.CanPrepare("CREATE_DRAFT"));await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordAsync("CREATE_DRAFT",Reason,true));Assert.Empty(f.Sent);return;}
        if(mode=="storage")f.Store.SaveFail=true;else if(mode=="absence-resend")f.FailBeforeEffect=true;else f.LoseAck=true;
        await Assert.ThrowsAnyAsync<Exception>(()=>state.RecordAsync("CREATE_DRAFT",Reason,true));Assert.True(state.HasPending);Assert.False(state.CanReviewCreation);
        if(mode=="storage"){Assert.Empty(f.Sent);Assert.Empty(f.Effects);return;}
        var pending=f.Store.Stored!;Assert.Equal(CreationValues(),pending.CreationReview!.Proposal);
        if(mode=="reload-readonly")
        {
            f.Select(false);using var restored=f.State();await restored.AttachCreationAsync();Assert.True(restored.CanRecover);Assert.False(restored.CanReviewCreation);
            await restored.RecoverAsync();Assert.Equal(pending,restored.Outcome!.Original);Assert.Single(f.Sent);Assert.Null(f.Store.Stored);return;
        }
        await state.RecoverAsync();Assert.True(state.AbsenceObserved);Assert.Equal(pending,f.Store.Stored);Assert.Empty(f.Effects);
        f.FailBeforeEffect=false;await state.ResendAsync();Assert.Equal(2,f.Sent.Count);Assert.All(f.Sent,x=>Assert.Equal(pending,x));Assert.Single(f.Effects);Assert.Null(f.Store.Stored);
    }
    [Theory][InlineData("reload")][InlineData("replacement")][InlineData("detail")]
    public async Task ProtectedCreationOriginalIsDiscoverableAfterReloadAndRefusesAnotherDraft(string mode)
    {
        using var f=new Fixture();var tab=new Tab();var protector=new EphemeralDataProtectionProvider();
        var first=new PermitApplicationIntentStore(new ProtectedSessionStorage(tab,protector));
        var creation=PermitApplicationCreationReview.Capture(f.Source().Context,CreationValues(),"field-source","well-source","facility-source");var original=creation.PrepareOriginal(Guid.NewGuid(),Reason);
        await first.RetainAsync(original);var reloaded=new PermitApplicationIntentStore(new ProtectedSessionStorage(tab,protector));Assert.Equal(original,await reloaded.ReadCreationAsync(f.Field));
        if(mode=="replacement")await Assert.ThrowsAsync<InvalidOperationException>(()=>reloaded.RetainAsync(creation.PrepareOriginal(Guid.NewGuid(),Reason)));
        if(mode=="detail")Assert.Equal(original,await reloaded.ReadAsync(f.Field,original.Review.ReadApplication().PERMIT_APPLICATION_ID));
        Assert.Single(tab.Values);Assert.DoesNotContain(Reason,tab.Values.Single().Value);await reloaded.ClearAsync(original);Assert.Null(await reloaded.ReadCreationAsync(f.Field));
    }
    private sealed class CreationPageActivator:IComponentActivator
    {
        public Beep.OilandGas.Web.Pages.PPDM39.Permits.NewPermitApplication? Captured;
        public IComponent CreateInstance(Type type){var instance=(IComponent)Activator.CreateInstance(type)!;if(instance is Beep.OilandGas.Web.Pages.PPDM39.Permits.NewPermitApplication page)Captured=page;return instance;}
    }
    [Theory][InlineData("changed-proposal")][InlineData("confirmed")]
    public async Task ActualCreationPageReviewsCompleteKeysAndRefusesChangedInputBeforeTransport(string mode)
    {
        using var f=new Fixture();var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime>(new Tab());
        services.AddSingleton(f.Company);services.AddSingleton(f.Fields.Object);services.AddSingleton(f.Client.Object);services.AddSingleton<IPermitApplicationIntentStore>(f.Store);services.AddSingleton<AuthenticationStateProvider>(f.Authentication);
        var activator=new CreationPageActivator();services.AddSingleton<IComponentActivator>(activator);services.AddSingleton(new OilGasCallFailures(Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Permits.NewPermitApplication>();var page=activator.Captured!;var type=page.GetType();var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
            async Task Invoke(string name)=>await (Task)type.GetMethod(name,flags)!.Invoke(page,null)!;
            void Set(string name,object value)=>type.GetField(name,flags)!.SetValue(page,value);
            await Invoke("ReadOriginal");Set("_type",Enum.GetNames<PermitApplicationType>().First());Set("_country","Kuwait");Set("_region",Enum.GetNames<StateProvince>().First());Set("_authority",Enum.GetNames<RegulatoryAuthority>().First());
            Set("_applicant","Reviewed applicant");Set("_facility","shared");Set("_facilityType","gas");await Invoke("ReviewProposal");
            Set("_reason",Reason);Set("_confirmed",true);if(mode=="changed-proposal")Set("_facilityType","oil");
            await Invoke("Create");typeof(ComponentBase).GetMethod("StateHasChanged",flags)!.Invoke(page,null);var html=root.ToHtmlString();
            if(mode=="changed-proposal"){Assert.Empty(f.Sent);Assert.Null(f.Store.Stored);Assert.Contains("proposal changed",html);}
            else {Assert.Single(f.Sent);Assert.Equal("gas",f.Sent.Single().CreationReview!.Proposal.FacilityType);Assert.Contains("Open created application",html);Assert.Contains("Recorded draft creation",html);}
        });
    }
    [Theory][InlineData("submitted")][InlineData("under-review")][InlineData("self")][InlineData("terminal")][InlineData("forged-outcome")]
    public void PermitCorrectionReviewPreservesSubmissionAndSupportsExplicitEditResubmission(string mode)
    {
        using var f=new Fixture();var row=f.Source().ReadApplication();row.STATUS=mode=="terminal"?PermitApplicationStatus.Approved:mode=="under-review"?PermitApplicationStatus.UnderReview:PermitApplicationStatus.Submitted;
        row.SUBMITTED_BY_USER_ID=mode=="self"?"actor":"issuer";row.SUBMITTED_DATE=DateTime.UtcNow.AddDays(-1);row.SUBMISSION_COMPLETE=true;row.SUBMISSION_COMPLETE_IND="Y";
        var source=PermitApplicationSourceReview.Capture(f.Source().Context,row,[]);var original=new PermitApplicationCommand(Guid.NewGuid(),source,"RETURN_FOR_CORRECTION",Reason);
        if(mode is "self" or "terminal"){Assert.Throws<InvalidOperationException>(original.Require);return;}
        var returned=PermitApplicationCommandOutcome.Prepare(original,DateTimeOffset.UtcNow);var saved=returned.ReadApplication();Assert.Equal(PermitApplicationStatus.AdditionalInformationRequired,saved.STATUS);
        Assert.Equal(row.SUBMITTED_BY_USER_ID,saved.SUBMITTED_BY_USER_ID);Assert.Equal(row.SUBMITTED_DATE,saved.SUBMITTED_DATE);Assert.False(saved.SUBMISSION_COMPLETE);Assert.Equal("N",saved.SUBMISSION_COMPLETE_IND);
        Assert.Equal("RETURN_FOR_CORRECTION",saved.DECISION);Assert.Equal("ADDITIONAL_INFO_REQUIRED",returned.ReadHistory().STATUS);
        if(mode=="forged-outcome"){saved.SUBMITTED_BY_USER_ID="rewritten";Assert.Throws<InvalidOperationException>((returned with{ApplicationJson=JsonSerializer.Serialize(saved)}).Require);return;}
        var editReview=PermitApplicationSourceReview.Capture(source.Context,saved,[returned.ReadHistory()]);
        var edit=new PermitApplicationCommand(Guid.NewGuid(),editReview,"EDIT_DRAFT",Reason){DraftChanges=Changes(editReview)};
        var edited=PermitApplicationCommandOutcome.Prepare(edit,DateTimeOffset.UtcNow);Assert.Equal(PermitApplicationStatus.AdditionalInformationRequired,edited.ReadApplication().STATUS);Assert.Equal("RETURN_FOR_CORRECTION",edited.ReadApplication().DECISION);
        var resubmit=new PermitApplicationCommand(Guid.NewGuid(),PermitApplicationSourceReview.Capture(source.Context,edited.ReadApplication(),[returned.ReadHistory(),edited.ReadHistory()]),"SUBMIT",Reason);
        var submitted=PermitApplicationCommandOutcome.Prepare(resubmit,DateTimeOffset.UtcNow);Assert.Equal(PermitApplicationStatus.Submitted,submitted.ReadApplication().STATUS);Assert.Null(submitted.ReadApplication().DECISION);Assert.Equal("actor",submitted.ReadApplication().SUBMITTED_BY_USER_ID);
        Assert.DoesNotContain("RETURN_FOR_CORRECTION",PermitApplicationCommand.AllowedActions(PermitApplicationSourceReview.Capture(source.Context,submitted.ReadApplication(),[returned.ReadHistory(),edited.ReadHistory(),submitted.ReadHistory()])));
    }
    [Theory][InlineData("independent")][InlineData("self")][InlineData("readonly")][InlineData("lost-response")]
    public async Task ActualPermitCorrectionControlRequiresIndependentReviewerAndRetainsUncertainty(string mode)
    {
        using var f=new Fixture();f.Status=PermitApplicationStatus.Submitted;f.Submitter=mode=="self"?"actor":"issuer";if(mode=="readonly")f.Select(false);
        using var state=f.State();await state.AttachAsync(Id);await state.ReviewAsync();
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime>(new Tab());services.AddSingleton(new OilGasCallFailures(Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.PermitApplicationWorkflow>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"State",state},{"ApplicationId",Id}}));
            Assert.Equal(mode is "independent" or "lost-response",root.ToHtmlString().Contains("Request correction",StringComparison.Ordinal));
        });
        if(mode=="lost-response")
        {
            f.LoseAck=true;await Assert.ThrowsAsync<HttpRequestException>(()=>state.RecordAsync("RETURN_FOR_CORRECTION",Reason,true));var pending=f.Store.Stored!;
            f.Select(false);using var restored=f.State();await restored.AttachAsync(Id);await restored.RecoverAsync();Assert.Equal(pending,restored.Outcome!.Original);Assert.Equal(PermitApplicationStatus.AdditionalInformationRequired,restored.Outcome.ReadApplication().STATUS);Assert.Single(f.Sent);
        }
    }
    private sealed class DrillingWorkflowActivator:IComponentActivator
    {
        public IComponent? Captured;
        public IComponent CreateInstance(Type type)
        {
            var instance=(IComponent)Activator.CreateInstance(type)!;
            if(instance is Beep.OilandGas.Web.Components.Workflow.PermitApplicationWorkflow)Captured=instance;
            return instance;
        }
    }
    [Theory][InlineData("author")][InlineData("readonly")][InlineData("independent")][InlineData("lost-response")]
    public async Task ActualDrillingAmendmentFormUsesRoleStageAndRetainedOriginal(string mode)
    {
        using var f=new Fixture();f.WellId="recorded-well";
        f.Drilling=new DRILLING_PERMIT_APPLICATION { PERMIT_APPLICATION_ID=Id,ACTIVE_IND="Y",PROPOSED_DEPTH=50.5m,PROPOSED_DEPTH_UNIT="ft",TARGET_FORMATION="Recorded formation" };
        if(mode=="readonly")f.Select(false);
        if(mode=="independent"){f.Status=PermitApplicationStatus.Submitted;f.Submitter="issuer";}
        using var state=f.State();await state.AttachAsync(Id);await state.ReviewAsync();
        var activator=new DrillingWorkflowActivator();var services=new ServiceCollection();services.AddSingleton<IComponentActivator>(activator);services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime>(new Tab());services.AddSingleton(new OilGasCallFailures(Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.PermitApplicationWorkflow>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"State",state},{"ApplicationId",Id}}));
            var html=root.ToHtmlString();Assert.Contains("Reviewed drilling details",html);Assert.Contains("Recorded formation",html);Assert.Contains("ft",html);
            Assert.Equal(mode is "author" or "lost-response",html.Contains("Save reviewed drilling details",StringComparison.Ordinal));
            if(mode is "author" or "lost-response"){Assert.Contains("Recorded depth unit",html);Assert.Contains("Unknown / not recorded",html);}
            if(mode=="author")
            {
                var component=activator.Captured!;var type=component.GetType();var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
                var proposal=PermitDrillingDraftChanges.From(f.Drilling!) with { ProposedDepth=100.25m,DepthUnit="m" };
                type.GetField("_drilling",flags)!.SetValue(component,proposal);type.GetField("_reason",flags)!.SetValue(component,Reason);type.GetField("_confirmed",flags)!.SetValue(component,true);
                await (Task)type.GetMethod("Submit",flags)!.Invoke(component,null)!;Assert.Empty(f.Sent);
                await (Task)type.GetMethod("SaveDrilling",flags)!.Invoke(component,null)!;Assert.Single(f.Sent);
                Assert.Equal(proposal,state.Outcome!.Original.DrillingChanges);
                var result=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.PermitApplicationWorkflow>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"State",state},{"ApplicationId",Id}}));
                Assert.Contains("Recorded drilling result for this original",result.ToHtmlString());
            }
        });
        var changes=PermitDrillingDraftChanges.From(f.Drilling) with { ProposedDepth=100.25m,DepthUnit="m" };
        if(mode is "readonly" or "independent")
        {await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordAsync("AMEND_DRILLING",Reason,true,drillingChanges:changes));Assert.Empty(f.Sent);return;}
        if(mode=="lost-response")
        {
            f.LoseAck=true;await Assert.ThrowsAsync<HttpRequestException>(()=>state.RecordAsync("AMEND_DRILLING",Reason,true,drillingChanges:changes));
            var original=f.Store.Stored!;Assert.Equal(changes,original.DrillingChanges);Assert.Equal(f.Source(),original.Review);
            await renderer.Dispatcher.InvokeAsync(async()=>
            {
                var pending=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.PermitApplicationWorkflow>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"State",state},{"ApplicationId",Id}}));
                Assert.Contains("Retained drilling proposal",pending.ToHtmlString());Assert.Contains("100.25",pending.ToHtmlString());
            });
            Assert.Equal(original,JsonSerializer.Deserialize<PermitApplicationCommand>(JsonSerializer.Serialize(original)));
            f.Select(false);using var restored=f.State();await restored.AttachAsync(Id);await restored.RecoverAsync();
            Assert.Equal(original,restored.Outcome!.Original);Assert.Equal(changes,PermitDrillingDraftChanges.From(restored.Outcome.Supporting.Drilling!));
            Assert.Single(f.Sent);Assert.Null(f.Store.Stored);return;
        }
        Assert.Equal(changes,PermitDrillingDraftChanges.From(state.Outcome!.Supporting.Drilling!));Assert.Single(f.Sent);Assert.Single(f.Effects);
    }
    [Theory]
    [InlineData("environmental","author")][InlineData("environmental","readonly")][InlineData("environmental","independent")][InlineData("environmental","lost-response")]
    [InlineData("injection","author")][InlineData("injection","readonly")][InlineData("injection","independent")][InlineData("injection","lost-response")]
    public async Task ActualSupportingAmendmentFormUsesRoleStageAndRetainedOriginal(string kind,string mode)
    {
        using var f=new Fixture();f.WellId="recorded-well";f.ApplicationType=kind=="environmental"?PermitApplicationType.Environmental:PermitApplicationType.Injection;
        var initial=PermitApplicationCommandOutcome.Prepare(PermitProviderQualificationTests.SupportingOriginal(f.Source(),kind),DateTimeOffset.UtcNow);
        f.Environmental=initial.Supporting.Environmental;f.Injection=initial.Supporting.Injection;
        if(mode=="readonly")f.Select(false);
        if(mode=="independent"){f.Status=PermitApplicationStatus.Submitted;f.Submitter="issuer";}
        using var state=f.State();await state.AttachAsync(Id);await state.ReviewAsync();
        var proposed=PermitProviderQualificationTests.SupportingOriginal(f.Source(),kind,true);
        var activator=new DrillingWorkflowActivator();var services=new ServiceCollection();services.AddSingleton<IComponentActivator>(activator);
        services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime>(new Tab());services.AddSingleton(new OilGasCallFailures(Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.PermitApplicationWorkflow>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"State",state},{"ApplicationId",Id}}));
            var html=root.ToHtmlString();Assert.Contains("Reviewed "+kind+" details",html);Assert.Contains(kind=="environmental"?"m3":"kPa",html);
            Assert.Equal(mode is "author" or "lost-response",html.Contains("Save reviewed "+kind+" details",StringComparison.Ordinal));
            if(mode=="author")
            {
                var component=activator.Captured!;var type=component.GetType();var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
                type.GetField("_"+kind,flags)!.SetValue(component,kind=="environmental"?(object)proposed.EnvironmentalChanges!:proposed.InjectionChanges!);
                type.GetField("_reason",flags)!.SetValue(component,Reason);type.GetField("_confirmed",flags)!.SetValue(component,true);
                await (Task)type.GetMethod("Submit",flags)!.Invoke(component,null)!;Assert.Empty(f.Sent);
                await (Task)type.GetMethod("Save"+(kind=="environmental"?"Environmental":"Injection"),flags)!.Invoke(component,null)!;Assert.Single(f.Sent);
                Assert.Equal(proposed.EnvironmentalChanges,state.Outcome!.Original.EnvironmentalChanges);Assert.Equal(proposed.InjectionChanges,state.Outcome.Original.InjectionChanges);
                var result=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.PermitApplicationWorkflow>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"State",state},{"ApplicationId",Id}}));
                Assert.Contains("Recorded "+kind+" result for this original",result.ToHtmlString());
            }
        });
        if(mode is "readonly" or "independent")
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordAsync(proposed.Action,Reason,true,environmentalChanges:proposed.EnvironmentalChanges,injectionChanges:proposed.InjectionChanges));Assert.Empty(f.Sent);return;
        }
        if(mode=="lost-response")
        {
            f.LoseAck=true;await Assert.ThrowsAsync<HttpRequestException>(()=>state.RecordAsync(proposed.Action,Reason,true,environmentalChanges:proposed.EnvironmentalChanges,injectionChanges:proposed.InjectionChanges));
            var original=f.Store.Stored!;Assert.Equal(proposed.EnvironmentalChanges,original.EnvironmentalChanges);Assert.Equal(proposed.InjectionChanges,original.InjectionChanges);
            Assert.Equal(original,JsonSerializer.Deserialize<PermitApplicationCommand>(JsonSerializer.Serialize(original)));
            await renderer.Dispatcher.InvokeAsync(async()=>
            {
                var pending=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.PermitApplicationWorkflow>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"State",state},{"ApplicationId",Id}}));
                Assert.Contains("Retained "+kind+" proposal",pending.ToHtmlString());Assert.Contains("Amended notes",pending.ToHtmlString());
            });
            f.Select(false);using var restored=f.State();await restored.AttachAsync(Id);await restored.RecoverAsync();Assert.Equal(original,restored.Outcome!.Original);
            Assert.Single(f.Sent);Assert.Null(f.Store.Stored);return;
        }
        Assert.Single(f.Sent);Assert.Single(f.Effects);
    }
    [Theory][InlineData("author")][InlineData("independent")][InlineData("readonly")]
    public async Task ActualPermitAttachmentMetadataIsInertAndCannotConfirmContent(string mode)
    {
        using var f=new Fixture();f.Attachments=[new APPLICATION_ATTACHMENT { APPLICATION_ATTACHMENT_ID="metadata-1",PERMIT_APPLICATION_ID=Id,FILE_NAME="https://untrusted.test/<file>.pdf",FILE_TYPE="application/pdf",FILE_SIZE=123,UPLOADED_BY="recorded-uploader",ACTIVE_IND="Y" }];
        if(mode=="independent"){f.Status=PermitApplicationStatus.Submitted;f.Submitter="issuer";}if(mode=="readonly")f.Select(false);
        using var state=f.State();await state.AttachAsync(Id);await state.ReviewAsync();
        Assert.False(state.CanPrepare("SUBMIT"));Assert.False(state.CanPrepare("APPROVE"));
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime>(new Tab());services.AddSingleton(new OilGasCallFailures(Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.PermitApplicationWorkflow>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"State",state},{"ApplicationId",Id}}));
            var html=root.ToHtmlString();Assert.Contains("Recorded attachment metadata",html);Assert.Contains("File content has not been confirmed",html);Assert.Contains("unit not established",html);
            Assert.Contains("&lt;file&gt;",html);Assert.DoesNotContain("href="+(char)34+"https://untrusted.test",html);Assert.DoesNotContain("Submit application",html);Assert.DoesNotContain("Record independent approval",html);
            Assert.Equal(mode=="independent",html.Contains("Request correction",StringComparison.Ordinal));
        });
        if(mode=="independent")
        {
            await state.RecordAsync("RETURN_FOR_CORRECTION",Reason,true);Assert.Single(f.Sent);Assert.Single(state.Outcome!.Supporting.Attachments);Assert.Equal(PermitApplicationStatus.AdditionalInformationRequired,state.Outcome.ReadApplication().STATUS);
        }
        else Assert.Empty(f.Sent);
    }
    private static PermitApplicationSummary InboxRow(string id,string status,string submitter="")=>new(){PermitApplicationId=id,FieldId="A",Status=status,SubmittedByUserId=submitter,ApplicantName="Applicant",ApplicationType="Drilling",Country="Kuwait",Region="Other",RegulatoryAuthority="Other"};
    private static PermitApplicationInboxReview Inbox(Fixture f,IReadOnlyList<PermitApplicationSummary> rows,bool write=true)=>PermitApplicationInboxReview.Capture(new(f.Source().Context,write,write,ProcessDefinitionRevision.FromJson("{}")),DateTimeOffset.UtcNow,rows);
    [Theory][InlineData("roles")][InlineData("readonly")][InlineData("foreign")][InlineData("duplicates")][InlineData("late-field")][InlineData("missing-role")]
    public async Task PermitInboxStateUsesReturnedRoleQueuesAndWithholdsForeignOrLateRecords(string mode)
    {
        using var f=new Fixture();if(mode=="readonly")f.Select(false);if(mode=="missing-role")f.Authentication.Principal=new(new ClaimsIdentity([new Claim("party_id","actor")],"test"));
        var rows=new[]{InboxRow("draft","Draft"),InboxRow("correction","AdditionalInformationRequired"),InboxRow("independent","Submitted","issuer"),InboxRow("self","UnderReview","actor"),InboxRow("unattributed","Submitted"),InboxRow("terminal","Approved")};
        var review=Inbox(f,rows);if(mode=="foreign")review=review with{Authority=review.Authority with{Context=review.Authority.Context with{Field=f.Field with{FieldId="B"}}}};
        if(mode=="duplicates")review=review with{ApplicationsJson=JsonSerializer.Serialize(new[]{rows[0],rows[0]})};
        var release=new TaskCompletionSource<PermitApplicationInboxReview>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Client.Setup(x=>x.ReadInboxAsync(It.IsAny<FieldSelectionReview>(),It.IsAny<CancellationToken>())).Returns(()=>mode=="late-field"?release.Task:Task.FromResult(review));
        using var state=new PermitApplicationInboxState(f.Company,f.Fields.Object,f.Authentication,f.Client.Object);var loading=state.LoadAsync();
        if(mode=="late-field"){f.Fields.Raise(x=>x.CurrentFieldChanged+=null!,"B");release.SetResult(review);}
        if(mode is "foreign" or "duplicates" or "late-field" or "missing-role"){await Assert.ThrowsAnyAsync<Exception>(()=>loading);Assert.Empty(state.Applications);Assert.Null(state.Review);return;}
        await loading;Assert.Equal(6,state.Count("All returned"));Assert.Equal(mode=="readonly"?0:1,state.Count("Independent review"));Assert.Equal(mode=="readonly"?0:1,state.Count("Corrections"));
        Assert.Equal("Other returned records",state.Category(rows[3]));Assert.Equal("Other returned records",state.Category(rows[4]));
    }
    [Fact]
    public async Task PermitInboxPagingSearchAndContextResetKeepExactReturnedRecordGroups()
    {
        using var f=new Fixture();var review=Inbox(f,Enumerable.Range(0,12).Select(i=>InboxRow("PAD"+i.ToString("D2"),"Draft")).ToArray());
        f.Client.Setup(x=>x.ReadInboxAsync(It.IsAny<FieldSelectionReview>(),It.IsAny<CancellationToken>())).ReturnsAsync(review);
        using var state=new PermitApplicationInboxState(f.Company,f.Fields.Object,f.Authentication,f.Client.Object);await state.LoadAsync();state.Select("Drafts");Assert.Equal(10,state.PageItems.Count);state.Next();Assert.Equal(2,state.PageItems.Count);
        state.SetSearch("PAD00");Assert.Equal(0,state.Page);Assert.Single(state.PageItems);state.Select("Corrections");Assert.Empty(state.PageItems);
        f.Fields.Raise(x=>x.CurrentFieldChanged+=null!,"B");Assert.Empty(state.Applications);Assert.Equal("All returned",state.SelectedQueue);Assert.Equal("",state.Search);Assert.Equal(0,state.Page);
    }
    [Theory][InlineData(true)][InlineData(false)]
    public async Task ActualPermitDashboardRendersScopedRoleQueuesAndExactApplicationLinks(bool write)
    {
        using var f=new Fixture();if(!write)f.Select(false);var id="review/شركة?1";var review=Inbox(f,[InboxRow("draft","Draft"),InboxRow("correction","AdditionalInformationRequired"),InboxRow(id,"Submitted","issuer")],write);
        f.Client.Setup(x=>x.ReadInboxAsync(It.IsAny<FieldSelectionReview>(),It.IsAny<CancellationToken>())).ReturnsAsync(review);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime>(new Tab());services.AddSingleton(f.Company);services.AddSingleton(f.Fields.Object);services.AddSingleton(f.Client.Object);services.AddSingleton<AuthenticationStateProvider>(f.Authentication);services.AddSingleton(new OilGasCallFailures(Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Permits.PermitDashboard>();var html=root.ToHtmlString();Assert.Contains("Permit work queues",html);Assert.Contains("data-label=\"Application\"",html);Assert.Contains("aria-pressed=\"true\"",html);Assert.Contains("Additional information required",html);Assert.Contains("aria-label=\"Previous permit records\"",html);Assert.Contains("3 returned applications",html);Assert.Contains("applicationId="+System.Uri.EscapeDataString(id),html);
            Assert.Equal(write,html.Contains("New application",StringComparison.Ordinal));Assert.Equal(write,html.Contains("Independent review (1)",StringComparison.Ordinal));Assert.Equal(write,html.Contains("Corrections (1)",StringComparison.Ordinal));
        });
    }
    private sealed class Tab:IJSRuntime
    {
        public Dictionary<string,string> Values=[];
        public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>InvokeAsync<T>(id,default,args);
        public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)
        {
            var key=(string)args![0]!;if(id=="sessionStorage.setItem")Values[key]=(string)args[1]!;
            else if(id=="sessionStorage.removeItem")Values.Remove(key);
            else if(id=="sessionStorage.getItem")return ValueTask.FromResult(Values.TryGetValue(key,out var value)?JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value))!:default!);
            else throw new InvalidOperationException(id);return ValueTask.FromResult(default(T)!);
        }
    }
    [Theory][InlineData("reload")][InlineData("replacement")][InlineData("binding")][InlineData("company")][InlineData("actor")][InlineData("field")][InlineData("corrupt")]
    public async Task ProtectedPermitOriginalSurvivesReloadAndRefusesReplacementOrForeignScope(string mode)
    {
        using var f=new Fixture();var tab=new Tab();var protection=new EphemeralDataProtectionProvider();var first=new PermitApplicationIntentStore(new ProtectedSessionStorage(tab,protection));var second=new PermitApplicationIntentStore(new ProtectedSessionStorage(tab,protection));
        var original=new PermitApplicationCommand(Guid.NewGuid(),f.Source(),"SUBMIT",Reason);await first.RetainAsync(original);Assert.DoesNotContain(Reason,tab.Values.Single().Value);
        var field=f.Field;if(mode=="binding")field=field with{Database=new("other","other-binding")};if(mode=="company")field=field with{Company=new("tenant","other")};if(mode=="actor")field=field with{ActorUserId="other"};if(mode=="field")field=field with{FieldId="B"};
        if(mode=="corrupt"){tab.Values[tab.Values.Single().Key]="invalid protected input";await Assert.ThrowsAnyAsync<Exception>(()=>second.ReadAsync(field,Id));return;}
        if(mode is "company" or "actor" or "field"){Assert.Null(await second.ReadAsync(field,Id));Assert.Single(tab.Values);return;}
        Assert.Equal(original,await second.ReadAsync(field,Id));
        if(mode is "replacement" or "binding"){await Assert.ThrowsAsync<InvalidOperationException>(()=>second.RetainAsync(original with{IntentId=Guid.NewGuid()}));Assert.Equal(original,await first.ReadAsync(f.Field,Id));}
        else {await second.ClearAsync(original);Assert.Null(await first.ReadAsync(field,Id));}
    }
    private sealed class Navigation:NavigationManager
    {
        public Navigation()=>Initialize("https://oilgas.test/","https://oilgas.test/ppdm39/permits/detail?applicationId="+System.Uri.EscapeDataString(Id));
        protected override void NavigateToCore(string uri,bool forceLoad)=>Initialize(BaseUri,ToAbsoluteUri(uri).ToString());
    }
    private sealed class PageActivator:IComponentActivator
    {
        public IComponent CreateInstance(Type type)
        {
            var component=(IComponent)Activator.CreateInstance(type)!;
            if(component is Beep.OilandGas.Web.Pages.PPDM39.Permits.PermitDetail page)page.ApplicationId=Id;
            return component;
        }
    }
    [Theory][InlineData(true)][InlineData(false)]
    public async Task PermitDetailIntegratesWorkbenchOnlyForCurrentReadableSource(bool allowed)
    {
        using var f=new Fixture();
        if(!allowed)Assert.True(f.Company.TrySelect(f.Company.Clear(),"actor",new(new("tenant","company"),["Safety"],[])));
        f.Client.Setup(x=>x.GetByIdAsync(Id,It.IsAny<CancellationToken>())).ReturnsAsync(new PermitApplicationDetail{Application=new(){PermitApplicationId=Id,FieldId="A",ApplicantName="Recorded applicant",ApplicationType="Drilling",RegulatoryAuthority="Recorded authority",Status="Draft"},StatusHistory=[]});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime>(new Tab());services.AddSingleton(f.Company);services.AddSingleton(f.Fields.Object);services.AddSingleton(f.Client.Object);services.AddSingleton<IPermitApplicationIntentStore>(f.Store);services.AddSingleton<AuthenticationStateProvider>(f.Authentication);
        services.AddSingleton(new OilGasCallFailures(Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>()));services.AddSingleton<NavigationManager,Navigation>();services.AddSingleton<IComponentActivator,PageActivator>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Permits.PermitDetail>();var html=root.ToHtmlString();
            if(allowed){Assert.Contains("Recorded applicant",html);Assert.Contains("Reviewed permit workflow",html);Assert.Contains("Read retained permit original",html);}
            else {Assert.DoesNotContain("Reviewed permit workflow",html);Assert.DoesNotContain("Recorded applicant",html);}
        });
        f.Client.Verify(x=>x.GetByIdAsync(Id,It.IsAny<CancellationToken>()),allowed?Times.Once():Times.Never());
    }
    private sealed class WorkbenchActivator:IComponentActivator
    {
        public Beep.OilandGas.Web.Components.Workflow.PermitApplicationWorkflow? Captured;
        public IComponent CreateInstance(Type type)
        {
            var component=(IComponent)Activator.CreateInstance(type)!;
            if(component is Beep.OilandGas.Web.Components.Workflow.PermitApplicationWorkflow workbench)Captured=workbench;
            return component;
        }
    }
    [Fact]
    public async Task PermitDraftFormRefusesSubmissionWhileProposedEditsAreUnsaved()
    {
        using var f=new Fixture();using var state=f.State();await state.AttachAsync(Id);await state.ReviewAsync();
        var activator=new WorkbenchActivator();var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime>(new Tab());
        services.AddSingleton<IComponentActivator>(activator);services.AddSingleton(new OilGasCallFailures(Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.PermitApplicationWorkflow>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"State",state},{"ApplicationId",Id}}));
            var component=activator.Captured!;var type=component.GetType();var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
            // Drive the component's bound inputs on its real renderer dispatcher; this is not a browser event assertion.
            type.GetField("_draftApplicant",flags)!.SetValue(component,"Unsaved applicant");type.GetField("_reason",flags)!.SetValue(component,Reason);type.GetField("_confirmed",flags)!.SetValue(component,true);
            await (Task)type.GetMethod("Submit",flags)!.Invoke(component,null)!;
            typeof(ComponentBase).GetMethod("StateHasChanged",flags)!.Invoke(component,null);
            Assert.Contains("Save the reviewed draft changes and review the saved source before submitting.",root.ToHtmlString());
            Assert.Empty(f.Sent);Assert.Empty(f.Effects);Assert.Null(f.Store.Stored);
        });
    }
    [Theory][InlineData("submit")][InlineData("independent")][InlineData("self")][InlineData("readonly")]
    public async Task PermitWorkbenchRendersExactSourceAndOnlyEligiblePersonaControls(string mode)
    {
        using var f=new Fixture();if(mode is "independent" or "self"){f.Status=PermitApplicationStatus.Submitted;f.Submitter=mode=="self"?"actor":"issuer";}if(mode=="readonly")f.Select(false);
        using var state=f.State();await state.AttachAsync(Id);await state.ReviewAsync();
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime>(new Tab());services.AddSingleton(new OilGasCallFailures(Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.PermitApplicationWorkflow>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"State",state},{"ApplicationId",Id}}));var html=root.ToHtmlString();
            Assert.Contains("Kuwait",html);Assert.Contains("Recorded applicant",html);Assert.Contains("does not establish regulator clearance",html);
            if(mode=="submit"){Assert.Contains("Submit application",html);Assert.Contains("Proposed draft header",html);Assert.Contains("Save reviewed changes",html);Assert.DoesNotContain("Record independent approval",html);}
            else if(mode=="independent"){Assert.DoesNotContain("Save reviewed changes",html);Assert.Contains("Record independent approval",html);Assert.Contains("Record independent rejection",html);Assert.Contains("issuer",html);}
            else {Assert.DoesNotContain("Save reviewed changes",html);Assert.DoesNotContain("Submit application",html);Assert.DoesNotContain("Record independent approval",html);}
        });
    }
}
#endif
