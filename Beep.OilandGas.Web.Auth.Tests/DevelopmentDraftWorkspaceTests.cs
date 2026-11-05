using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PermitsAndApplications.Models;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.DrillingAndConstruction.Models;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.Web.Pages.PPDM39.Development;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class DevelopmentDraftWorkspaceTests
{
    internal sealed class Journal : IDevelopmentDraftIntentStore
    {
        public DevelopmentEquipmentInstallation? EquipmentInstallationOriginal;
        public Task<DevelopmentEquipmentInstallation?> ReadEquipmentInstallationAsync(CompanyScope company,string actor)=>Task.FromResult(EquipmentInstallationOriginal?.Review.Proposal.Context.Company==company && EquipmentInstallationOriginal.Review.Proposal.Context.ActorUserId==actor?CompanyInventorySourceReview.Copy(EquipmentInstallationOriginal):null);
        public Task RetainEquipmentInstallationAsync(DevelopmentEquipmentInstallation original){original.Require();if(FailRetain)throw new InvalidOperationException("Retention unavailable.");if(EquipmentInstallationOriginal is not null && EquipmentInstallationOriginal.Revision()!=original.Revision())throw new InvalidOperationException("Recover the original installation.");EquipmentInstallationOriginal=CompanyInventorySourceReview.Copy(original);if(FailRetainReadback)throw new InvalidOperationException("Retention readback unavailable.");return Task.CompletedTask;}
        public Task ClearEquipmentInstallationAsync(DevelopmentEquipmentInstallation original){original.Require();if(FailClear)throw new InvalidOperationException("Clear unavailable.");if(EquipmentInstallationOriginal is not null && EquipmentInstallationOriginal.Revision()!=original.Revision())throw new InvalidOperationException("The original installation changed.");EquipmentInstallationOriginal=null;return Task.CompletedTask;}

        public DevelopmentFacilityCreation? FacilityCreationOriginal;
        public Task<DevelopmentFacilityCreation?> ReadFacilityCreationAsync(CompanyScope company,string actor)=>Task.FromResult(FacilityCreationOriginal?.Review.Draft.Context.Company==company && FacilityCreationOriginal.Review.Draft.Context.ActorUserId==actor?CompanyInventorySourceReview.Copy(FacilityCreationOriginal):null);
        public Task RetainFacilityCreationAsync(DevelopmentFacilityCreation original){original.Require();if(FailRetain)throw new InvalidOperationException("Retention unavailable.");if(FacilityCreationOriginal is not null && FacilityCreationOriginal.Revision()!=original.Revision())throw new InvalidOperationException("Recover the original facility creation.");FacilityCreationOriginal=CompanyInventorySourceReview.Copy(original);if(FailRetainReadback)throw new InvalidOperationException("Retention readback unavailable.");return Task.CompletedTask;}
        public Task ClearFacilityCreationAsync(DevelopmentFacilityCreation original){original.Require();if(FailClear)throw new InvalidOperationException("Clear unavailable.");if(FacilityCreationOriginal is not null && FacilityCreationOriginal.Revision()!=original.Revision())throw new InvalidOperationException("The original facility creation changed.");FacilityCreationOriginal=null;return Task.CompletedTask;}
        public DevelopmentFacilityEdit? FacilityEditOriginal;
        public Task<DevelopmentFacilityEdit?> ReadFacilityEditAsync(CompanyScope company,string actor)=>Task.FromResult(FacilityEditOriginal?.Review.Context.Company==company && FacilityEditOriginal.Review.Context.ActorUserId==actor?CompanyInventorySourceReview.Copy(FacilityEditOriginal):null);
        public Task RetainFacilityEditAsync(DevelopmentFacilityEdit original){original.Require();if(FailRetain)throw new InvalidOperationException("Retention unavailable.");if(FacilityEditOriginal is not null && FacilityEditOriginal.Revision()!=original.Revision())throw new InvalidOperationException("Recover the original facility edit.");FacilityEditOriginal=CompanyInventorySourceReview.Copy(original);if(FailRetainReadback)throw new InvalidOperationException("Retention readback unavailable.");return Task.CompletedTask;}
        public Task ClearFacilityEditAsync(DevelopmentFacilityEdit original){original.Require();if(FailClear)throw new InvalidOperationException("Clear unavailable.");if(FacilityEditOriginal is not null && FacilityEditOriginal.Revision()!=original.Revision())throw new InvalidOperationException("The original facility edit changed.");FacilityEditOriginal=null;return Task.CompletedTask;}
        public DrillingReportingReassignment? ReportingReassignmentOriginal;
        public Task<DrillingReportingReassignment?> ReadReportingReassignmentAsync(CompanyScope company,string actor)=>Task.FromResult(ReportingReassignmentOriginal?.Scope().Company==company && ReportingReassignmentOriginal.Scope().ActorUserId==actor?CompanyInventorySourceReview.Copy(ReportingReassignmentOriginal):null);
        public async Task RetainReportingReassignmentAsync(DrillingReportingReassignment original){if(FailRetain)throw new InvalidOperationException("Retain failed");if(BeforeReviewRetain is not null)await BeforeReviewRetain();if(ReportingReassignmentOriginal is not null && ReportingReassignmentOriginal.Revision()!=original.Revision())throw new InvalidOperationException("Recover original transfer");ReportingReassignmentOriginal=CompanyInventorySourceReview.Copy(original);if(FailRetainReadback)throw new InvalidOperationException("Retention readback is unconfirmed");}
        public Task ClearReportingReassignmentAsync(DrillingReportingReassignment original){if(FailClear)throw new InvalidOperationException("Clear failed");if(ReportingReassignmentOriginal is not null && ReportingReassignmentOriginal.Revision()!=original.Revision())throw new InvalidOperationException("Original transfer changed");ReportingReassignmentOriginal=null;return Task.CompletedTask;}
        public DrillingReportingClosure? ReportingClosureOriginal;
        public Task<DrillingReportingClosure?> ReadReportingClosureAsync(CompanyScope company,string actor)=>Task.FromResult(ReportingClosureOriginal?.Scope().Company==company && ReportingClosureOriginal.Scope().ActorUserId==actor?CompanyInventorySourceReview.Copy(ReportingClosureOriginal):null);
        public async Task RetainReportingClosureAsync(DrillingReportingClosure original){if(FailRetain)throw new InvalidOperationException("Retain failed");if(BeforeReviewRetain is not null)await BeforeReviewRetain();if(ReportingClosureOriginal is not null && ReportingClosureOriginal.Revision()!=original.Revision())throw new InvalidOperationException("Recover original reporting closure");ReportingClosureOriginal=CompanyInventorySourceReview.Copy(original);if(FailRetainReadback)throw new InvalidOperationException("Retention readback is unconfirmed");}
        public Task ClearReportingClosureAsync(DrillingReportingClosure original){if(FailClear)throw new InvalidOperationException("Clear failed");if(ReportingClosureOriginal is not null && ReportingClosureOriginal.Revision()!=original.Revision())throw new InvalidOperationException("Original reporting closure changed");ReportingClosureOriginal=null;return Task.CompletedTask;}
        public DrillingCompletionReviewDecision? CompletionDecisionOriginal;
        public Task<DrillingCompletionReviewDecision?> ReadCompletionDecisionAsync(CompanyScope company,string actor)=>Task.FromResult(CompletionDecisionOriginal?.Scope().Company==company && CompletionDecisionOriginal.Scope().ActorUserId==actor?CompanyInventorySourceReview.Copy(CompletionDecisionOriginal):null);
        public async Task RetainCompletionDecisionAsync(DrillingCompletionReviewDecision original){if(FailRetain)throw new InvalidOperationException("Retain failed");if(BeforeReviewRetain is not null)await BeforeReviewRetain();if(CompletionDecisionOriginal is not null && CompletionDecisionOriginal.Revision()!=original.Revision())throw new InvalidOperationException("Recover original completion decision");CompletionDecisionOriginal=CompanyInventorySourceReview.Copy(original);if(FailRetainReadback)throw new InvalidOperationException("Retention readback is unconfirmed");}
        public Task ClearCompletionDecisionAsync(DrillingCompletionReviewDecision original){if(FailClear)throw new InvalidOperationException("Clear failed");if(CompletionDecisionOriginal is not null && CompletionDecisionOriginal.Revision()!=original.Revision())throw new InvalidOperationException("Original completion decision changed");CompletionDecisionOriginal=null;return Task.CompletedTask;}
        public DrillingCompletionReviewStart? CompletionReviewOriginal;
        public Task<DrillingCompletionReviewStart?> ReadCompletionReviewAsync(CompanyScope company,string actor)=>Task.FromResult(CompletionReviewOriginal?.Scope().Company==company && CompletionReviewOriginal.Scope().ActorUserId==actor?CompanyInventorySourceReview.Copy(CompletionReviewOriginal):null);
        public async Task RetainCompletionReviewAsync(DrillingCompletionReviewStart original){if(FailRetain)throw new InvalidOperationException("Retain failed");if(BeforeReviewRetain is not null)await BeforeReviewRetain();if(CompletionReviewOriginal is not null && CompletionReviewOriginal.Revision()!=original.Revision())throw new InvalidOperationException("Recover original completion review");CompletionReviewOriginal=CompanyInventorySourceReview.Copy(original);if(FailRetainReadback)throw new InvalidOperationException("Retention readback is unconfirmed");}
        public Task ClearCompletionReviewAsync(DrillingCompletionReviewStart original){if(FailClear)throw new InvalidOperationException("Clear failed");if(CompletionReviewOriginal is not null && CompletionReviewOriginal.Revision()!=original.Revision())throw new InvalidOperationException("Original completion review changed");CompletionReviewOriginal=null;return Task.CompletedTask;}
        public DrillingExecutionAssignment? ExecutionAssignmentOriginal;
        public Task<DrillingExecutionAssignment?> ReadExecutionAssignmentAsync(CompanyScope company,string actor) => Task.FromResult(ExecutionAssignmentOriginal?.Scope().Company==company && ExecutionAssignmentOriginal.Scope().ActorUserId==actor?ExecutionAssignmentOriginal:null);
        public Task RetainExecutionAssignmentAsync(DrillingExecutionAssignment original) { ExecutionAssignmentOriginal=CompanyInventorySourceReview.Copy(original);return Task.CompletedTask; }
        public Task ClearExecutionAssignmentAsync(DrillingExecutionAssignment original) { if(ExecutionAssignmentOriginal?.Revision()!=original.Revision())throw new InvalidOperationException("Original assignment changed");ExecutionAssignmentOriginal=null;return Task.CompletedTask; }

        internal bool FailRetainReadback;
        internal DrillingDailyReportInput? DailyReportOriginal;
        public Task<DrillingDailyReportInput?> ReadDailyReportAsync(CompanyScope company,string actor)=>Task.FromResult(DailyReportOriginal?.Basis.Scope().Company==company && DailyReportOriginal.Basis.Scope().ActorUserId==actor?DailyReportOriginal:null);
        public async Task RetainDailyReportAsync(DrillingDailyReportInput original){if(FailRetain)throw new InvalidOperationException("Retain failed");if(BeforeReviewRetain is not null)await BeforeReviewRetain();DailyReportOriginal=original;if(FailRetainReadback)throw new InvalidOperationException("Retention readback is unconfirmed");}
        public Task ClearDailyReportAsync(DrillingDailyReportInput original){if(FailClear)throw new InvalidOperationException("Clear failed");DailyReportOriginal=null;return Task.CompletedTask;}
        internal DrillingExecutionStart? ExecutionOriginal;
        public Task<DrillingExecutionStart?> ReadExecutionAsync(CompanyScope company,string actor)=>Task.FromResult(ExecutionOriginal?.Basis.Scope().Company==company && ExecutionOriginal.Basis.Scope().ActorUserId==actor?ExecutionOriginal:null);
        public async Task RetainExecutionAsync(DrillingExecutionStart original){if(FailRetain)throw new InvalidOperationException("Retain failed");if(BeforeReviewRetain is not null)await BeforeReviewRetain();ExecutionOriginal=original;if(FailRetainReadback)throw new InvalidOperationException("Retention readback is unconfirmed");}
        public Task ClearExecutionAsync(DrillingExecutionStart original){if(FailClear)throw new InvalidOperationException("Clear failed");ExecutionOriginal=null;return Task.CompletedTask;}
        internal DrillingProgramReviewDecision? ProgramDecisionOriginal;
        public Task<DrillingProgramReviewDecision?> ReadProgramDecisionAsync(CompanyScope company,string actor)=>Task.FromResult(ProgramDecisionOriginal?.Scope().Company==company && ProgramDecisionOriginal.Scope().ActorUserId==actor?ProgramDecisionOriginal:null);
        public async Task RetainProgramDecisionAsync(DrillingProgramReviewDecision original){if(FailRetain)throw new InvalidOperationException("Retain failed");if(BeforeReviewRetain is not null)await BeforeReviewRetain();ProgramDecisionOriginal=original;}
        public Task ClearProgramDecisionAsync(DrillingProgramReviewDecision original){if(FailClear)throw new InvalidOperationException("Clear failed");ProgramDecisionOriginal=null;return Task.CompletedTask;}
        internal DrillingProgramReviewStart? ProgramReviewOriginal;
        public Task<DrillingProgramReviewStart?> ReadProgramReviewAsync(CompanyScope company,string actor)=>Task.FromResult(ProgramReviewOriginal?.Scope().Company==company && ProgramReviewOriginal.Scope().ActorUserId==actor?ProgramReviewOriginal:null);
        public async Task RetainProgramReviewAsync(DrillingProgramReviewStart original){if(FailRetain)throw new InvalidOperationException("Retain failed");if(BeforeReviewRetain is not null)await BeforeReviewRetain();ProgramReviewOriginal=original;}
        public Task ClearProgramReviewAsync(DrillingProgramReviewStart original){if(FailClear)throw new InvalidOperationException("Clear failed");ProgramReviewOriginal=null;return Task.CompletedTask;}
        internal CreateDrillingProgramDraft? ProgramOriginal;
        public Task<CreateDrillingProgramDraft?> ReadProgramAsync(CompanyScope company,string actor)=>Task.FromResult(ProgramOriginal?.Basis.Scope().Company==company && ProgramOriginal.Basis.Scope().ActorUserId==actor?ProgramOriginal:null);
        public async Task RetainProgramAsync(CreateDrillingProgramDraft original){if(FailRetain)throw new InvalidOperationException("Retain failed");if(BeforeReviewRetain is not null)await BeforeReviewRetain();ProgramOriginal=original;}
        public Task ClearProgramAsync(CreateDrillingProgramDraft original){if(FailClear)throw new InvalidOperationException("Clear failed");ProgramOriginal=null;return Task.CompletedTask;}

        internal DevelopmentInvestmentStageDecision? StageOriginal;
        public Task<DevelopmentInvestmentStageDecision?> ReadInvestmentStageAsync(CompanyScope company,string actor)=>Task.FromResult(StageOriginal?.Scope().Company==company && StageOriginal.Scope().ActorUserId==actor?StageOriginal:null);
        public async Task RetainInvestmentStageAsync(DevelopmentInvestmentStageDecision original){if(FailRetain)throw new InvalidOperationException("Retain failed");if(BeforeReviewRetain is not null)await BeforeReviewRetain();StageOriginal=original;}
        public Task ClearInvestmentStageAsync(DevelopmentInvestmentStageDecision original){if(FailClear)throw new InvalidOperationException("Clear failed");StageOriginal=null;return Task.CompletedTask;}
        internal DevelopmentInvestmentHandoff? HandoffOriginal;
        public Task<DevelopmentInvestmentHandoff?> ReadInvestmentHandoffAsync(CompanyScope company,string actor)=>Task.FromResult(HandoffOriginal?.Scope().Company==company && HandoffOriginal.Scope().ActorUserId==actor?HandoffOriginal:null);
        public async Task RetainInvestmentHandoffAsync(DevelopmentInvestmentHandoff original){if(FailRetain)throw new InvalidOperationException("Retain failed");if(BeforeReviewRetain is not null)await BeforeReviewRetain();HandoffOriginal=original;}
        public Task ClearInvestmentHandoffAsync(DevelopmentInvestmentHandoff original){if(FailClear)throw new InvalidOperationException("Clear failed");HandoffOriginal=null;return Task.CompletedTask;}
        internal DevelopmentAfeCommand? AfeOriginal;
        public Task<DevelopmentAfeCommand?> ReadAfeAsync(CompanyScope company,string actor)=>Task.FromResult(AfeOriginal?.Review.Source.Review.Scope.Company==company && AfeOriginal.Review.Source.Review.Scope.ActorUserId==actor?AfeOriginal:null);
        public async Task RetainAfeAsync(DevelopmentAfeCommand original){if(FailRetain)throw new InvalidOperationException("Retain failed");if(BeforeReviewRetain is not null)await BeforeReviewRetain();AfeOriginal=original;}
        public Task ClearAfeAsync(DevelopmentAfeCommand original){if(FailClear)throw new InvalidOperationException("Clear failed");AfeOriginal=null;return Task.CompletedTask;}
        internal CreateDevelopmentPlan? Original;
        internal DevelopmentReviewStart? ReviewOriginal;
        internal DevelopmentReviewDecision? DecisionOriginal;
        internal DevelopmentPlanRelease? ReleaseOriginal;
        public Task<DevelopmentPlanRelease?> ReadReleaseAsync(CompanyScope company,string actor)=>Task.FromResult(ReleaseOriginal?.Review.Scope.Company==company && ReleaseOriginal.Review.Scope.ActorUserId==actor?ReleaseOriginal:null);
        public async Task RetainReleaseAsync(DevelopmentPlanRelease original){if(FailRetain)throw new InvalidOperationException("Retain failed");if(BeforeReviewRetain is not null)await BeforeReviewRetain();ReleaseOriginal=original;}
        public Task ClearReleaseAsync(DevelopmentPlanRelease original){if(FailClear)throw new InvalidOperationException("Clear failed");ReleaseOriginal=null;return Task.CompletedTask;}

        public Task<DevelopmentReviewDecision?> ReadDecisionAsync(CompanyScope company,string actor)=>Task.FromResult(DecisionOriginal?.Review.Scope.Company==company && DecisionOriginal.Review.Scope.ActorUserId==actor?DecisionOriginal:null);
        public async Task RetainDecisionAsync(DevelopmentReviewDecision original){if(FailRetain)throw new InvalidOperationException("Retain failed");if(BeforeReviewRetain is not null)await BeforeReviewRetain();DecisionOriginal=original;}
        public Task ClearDecisionAsync(DevelopmentReviewDecision original){if(FailClear)throw new InvalidOperationException("Clear failed");DecisionOriginal=null;return Task.CompletedTask;}

        internal Func<Task>? BeforeReviewRetain;
        public Task<DevelopmentReviewStart?> ReadReviewAsync(CompanyScope company,string actor)=>Task.FromResult(ReviewOriginal?.Source.Context.Company==company && ReviewOriginal.Source.Context.ActorUserId==actor?ReviewOriginal:null);
        public async Task RetainReviewAsync(DevelopmentReviewStart original){if(FailRetain)throw new InvalidOperationException("Retain failed");if(BeforeReviewRetain is not null)await BeforeReviewRetain();ReviewOriginal=original;}
        public Task ClearReviewAsync(DevelopmentReviewStart original){if(FailClear)throw new InvalidOperationException("Clear failed");ReviewOriginal=null;return Task.CompletedTask;}
        internal bool FailRetain,FailClear;
        internal int Retains,Clears;
        public Task<CreateDevelopmentPlan?> ReadAsync(CompanyScope company,string actor)=>Task.FromResult(Original?.SelectionContext.Company==company && Original.SelectionContext.ActorUserId==actor?Original:null);
        public Task RetainAsync(CreateDevelopmentPlan original){Retains++;if(FailRetain)throw new InvalidOperationException("Retain failed");Original=original;return Task.CompletedTask;}
        public Task ClearAsync(CreateDevelopmentPlan original){Clears++;if(FailClear)throw new InvalidOperationException("Clear failed");Original=null;return Task.CompletedTask;}
    }
    internal sealed class Fixture : IDisposable
    {
        internal readonly CompanyClientContext Company=new();
        internal Mock<IFieldSelectionContext> Fields=new();
        internal Mock<IDevelopmentServiceClient> Client=new(MockBehavior.Strict);
        internal Journal Store=new();
        internal FieldSelectionReview Context=new(new("tenant","company"),new("core","core-binding"),"planner","preference","field/واحد");
        internal AssetDatabaseScope Planning=new("planning","planning-binding");
        internal DevelopmentDraftWorkspaceState State;
        internal List<FIELD_DEVELOPMENT_PLAN> Rows=[];
        internal int Writes,Recoveries;
        internal bool FailSave,NullList,ForeignList,MissingOptions,NullDetail,ForeignDetail;
        internal Action? OnSave,OnList,OnDetail;
        internal Fixture(bool create=true)
        {
            Company.TrySelect(Company.Clear(),"planner",new(Context.Company,["ConfiguredPlanner"],create?[PermissionConstants.DevelopmentPlanning.View,PermissionConstants.DevelopmentPlanning.Create]:[PermissionConstants.DevelopmentPlanning.View]));
            Fields.Setup(f=>f.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(()=>new(){FieldId=Context.FieldId!,FieldName="Recorded field",SelectionAvailable=true,SelectionContext=Context});
            Client.Setup(c=>c.GetDraftPlansAsync(It.IsAny<CancellationToken>())).ReturnsAsync(()=>{
                OnList?.Invoke();if(NullList)return null!;return new(ForeignList?Context with{Company=new("tenant","other")}:Context,Planning,Rows,DateTimeOffset.UtcNow);});
            Client.Setup(c=>c.GetDraftOptionsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(()=>MissingOptions?null!:new(Context,Planning,["KWD","EUR"]));
            Client.Setup(c=>c.GetDraftPlanAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync((string id,CancellationToken token)=>{
                var current=ForeignDetail?Context with{Company=new("tenant","other")}:Context;var row=Rows.SingleOrDefault(row=>row.FDP_ID==id);
                var result=new DevelopmentPlanSourceReview(current,Planning,id,row,DevelopmentPlanSourceReview.RevisionFor(current,Planning,id,row),DateTimeOffset.UtcNow);OnDetail?.Invoke();return NullDetail?null!:result;});
            Client.Setup(c=>c.SaveDraftPlanAsync(It.IsAny<CreateDevelopmentPlan>(),It.IsAny<CancellationToken>())).ReturnsAsync((CreateDevelopmentPlan request,CancellationToken token)=>{
                Assert.Equal(request,Store.Original);Writes++;OnSave?.Invoke();if(FailSave)throw new HttpRequestException("Response lost");var row=Recorded(request);Rows.Add(row);return row;});
            Client.Setup(c=>c.RecoverDraftPlanAsync(It.IsAny<CreateDevelopmentPlan>(),It.IsAny<CancellationToken>())).ReturnsAsync((CreateDevelopmentPlan original,CancellationToken token)=>{
                Recoveries++;return new(Context,Planning,original.IntentId,DevelopmentPlanDraftReview.RequestRevision(original),Rows.SingleOrDefault(row=>row.FDP_ID==DevelopmentPlanDraftReview.PlanIdFor(original)),DateTimeOffset.UtcNow);});
            State=new(Company,Fields.Object,Client.Object,Store,(_,_,text)=>text);
        }
        internal async Task Review(){await State.LoadAsync();State.ReviewInput("New <development> concept","Review description",new(2030,4,1),new(2031,4,1),12.125m,"KWD");Assert.NotNull(State.Review);}
        public void Dispose(){State.Dispose();Company.Dispose();}
    }
    internal static FIELD_DEVELOPMENT_PLAN Recorded(CreateDevelopmentPlan input)=>new(){FDP_ID=DevelopmentPlanDraftReview.PlanIdFor(input),TENANT_ID=input.SelectionContext.Company.TenantId,
        BUSINESS_ASSOCIATE_ID=input.SelectionContext.Company.BusinessAssociateId,FIELD_ID=input.FieldId,DRAFT_INTENT_ID=input.IntentId.ToString("D"),DRAFT_REQUEST_REVISION=DevelopmentPlanDraftReview.RequestRevision(input),
        FDP_NAME=input.PlanName,DEVELOPMENT_CONCEPT_DESC=input.Description ?? "",FDP_STATUS="DRAFT",FDP_VERSION=1,TOTAL_CAPEX_MM=input.EstimatedCapexMillions,CAPEX_CURRENCY=input.CapexCurrency ?? "",
        FIRST_PRODUCTION_DATE=input.PlannedFirstProductionDate?.ToDateTime(TimeOnly.MinValue),FIELD_LIFE_END=input.PlannedFieldLifeEndDate?.ToDateTime(TimeOnly.MinValue),ACTIVE_IND="Y",ROW_CREATED_BY=input.SelectionContext.ActorUserId,ROW_CREATED_DATE=DateTime.UtcNow};

    [Fact] public async Task SuccessfulSaveRetainsOriginalBeforeTransportAndConfirmsExactSourceRow()
    {
        using var f=new Fixture();await f.Review();var original=f.State.Review!;await f.State.SaveAsync();Assert.Equal(1,f.Writes);Assert.Equal(1,f.Store.Retains);Assert.Equal(1,f.Store.Clears);
        Assert.Null(f.State.Pending);Assert.NotNull(f.State.Confirmation);DevelopmentPlanDraftReview.RequireRecorded(original,f.State.Confirmed!);Assert.Single(f.State.Rows);
    }
    [Theory][InlineData("retain")][InlineData("response")][InlineData("clear")]
    public async Task UncertainSavePreservesOriginalAndNeverAutomaticallyRetries(string fault)
    {
        using var f=new Fixture();await f.Review();var original=f.State.Review!;f.Store.FailRetain=fault=="retain";f.FailSave=fault=="response";f.Store.FailClear=fault=="clear";
        await f.State.SaveAsync();Assert.NotNull(f.State.Error);Assert.Null(f.State.Confirmation);Assert.Equal(original,f.State.Pending);Assert.False(f.State.CanEdit);
        await f.State.SaveAsync();Assert.Equal(fault=="retain"?0:1,f.Writes);Assert.Equal(1,f.Store.Retains);
        f.Store.FailClear=false;await f.State.RecoverAsync();Assert.Equal(1,f.Recoveries);Assert.Null(f.State.Pending);Assert.NotNull(f.State.Confirmation);
        Assert.Equal(fault=="retain"?0:1,f.Writes);
    }
    [Fact] public async Task LostResponseWithCommittedDraftRecoversAcrossNewPageWithoutSecondWrite()
    {
        using var f=new Fixture();await f.Review();var original=f.State.Review!;f.FailSave=true;await f.State.SaveAsync();f.Rows.Add(Recorded(original));
        f.State.Dispose();f.State=new(f.Company,f.Fields.Object,f.Client.Object,f.Store,(_,_,text)=>text);
        await f.State.LoadAsync();Assert.Equal(original,f.State.Pending);await f.State.RecoverAsync();Assert.NotNull(f.State.Confirmed);Assert.Equal(1,f.Writes);Assert.Equal(1,f.Recoveries);
    }
    [Theory][InlineData("field")][InlineData("core")][InlineData("planning")][InlineData("company")][InlineData("account")]
    public async Task ChangedContextHidesOldEvidenceAndNeverReplaysOriginalUnderNewContext(string fault)
    {
        using var f=new Fixture();await f.Review();f.FailSave=true;await f.State.SaveAsync();var original=f.Store.Original!;
        switch(fault){case "field":f.Context=f.Context with{FieldId="other"};f.Fields.Raise(field=>field.CurrentFieldChanged+=null!,"other");break;
            case "core":f.Context=f.Context with{Database=new("core","new-binding")};break;case "planning":f.Planning=new("planning","new-binding");break;
            default:f.Company.Clear();f.Context=f.Context with{Company=fault=="company"?new("tenant","other"):f.Context.Company,ActorUserId=fault=="account"?"other":"planner"};
                f.Company.TrySelect(f.Company.Version,f.Context.ActorUserId,new(f.Context.Company,["ConfiguredPlanner"],[PermissionConstants.DevelopmentPlanning.View,PermissionConstants.DevelopmentPlanning.Create]));break;}
        await f.State.LoadAsync();Assert.False(f.State.CanRecover);await f.State.RecoverAsync();Assert.Equal(0,f.Recoveries);Assert.Equal(original,f.Store.Original);Assert.Equal(1,f.Writes);
        if(fault is "account" or "company")Assert.Null(f.State.Pending);else Assert.Equal(original,f.State.Pending);
    }
    [Theory][InlineData("null")][InlineData("foreign")][InlineData("options")][InlineData("changed")]
    public async Task FailedOrForeignReadsNeverBecomeAnEmptySuccessfulList(string fault)
    {
        using var f=new Fixture();f.NullList=fault=="null";f.ForeignList=fault=="foreign";f.MissingOptions=fault=="options";
        if(fault=="changed")f.OnList=()=>f.Context=f.Context with{Revision="changed"};
        await f.State.LoadAsync();Assert.False(f.State.Loaded);Assert.NotNull(f.State.Error);Assert.Empty(f.State.Rows);Assert.False(f.State.CanEdit);
    }
    [Fact] public async Task ReadOnlyPlannerCanInspectDraftsWithoutRequestingWriteOptions()
    {
        using var f=new Fixture(false);await f.State.LoadAsync();Assert.True(f.State.Loaded);Assert.False(f.State.CanEdit);
        f.Client.Verify(c=>c.GetDraftOptionsAsync(It.IsAny<CancellationToken>()),Times.Never);f.State.ReviewInput("Attempt",null,null,null,null,null);await f.State.SaveAsync();Assert.Equal(0,f.Writes);
    }
    [Fact] public async Task ChangedPreferenceBeforeSaveCannotRetainOrTransportStaleReview()
    {
        using var f=new Fixture();await f.Review();f.Context=f.Context with{Revision="changed"};await f.State.SaveAsync();
        Assert.Equal(0,f.Store.Retains);Assert.Equal(0,f.Writes);Assert.NotNull(f.State.Error);Assert.Null(f.State.Confirmation);
    }
    [Fact] public async Task LateSavedResponseAfterCompanyChangeCannotClearOriginalOrPublishOldEvidence()
    {
        using var f=new Fixture();await f.Review();var original=f.State.Review!;f.OnSave=()=>f.Company.Clear();await f.State.SaveAsync();
        Assert.Equal(1,f.Writes);Assert.Equal(original,f.Store.Original);Assert.Equal(0,f.Store.Clears);Assert.Empty(f.State.Rows);
        Assert.Null(f.State.Confirmed);Assert.Null(f.State.Confirmation);Assert.True(f.State.NeedsCompany);Assert.False(f.State.Busy);
    }
    [Theory][InlineData("detail")][InlineData("absent")][InlineData("failed")][InlineData("foreign")][InlineData("field-change")][InlineData("company-change")]
    public async Task SourceDetailRequiresExactCurrentFieldAndNeverLoadsWriteOptionsOrPrivateIntent(string mode)
    {
        using var f=new Fixture();await f.Review();var original=f.State.Review!;f.Rows.Add(Recorded(original));var id=DevelopmentPlanDraftReview.PlanIdFor(original);
        f.NullDetail=mode=="failed";f.ForeignDetail=mode=="foreign";
        f.OnDetail=()=>{if(mode=="field-change")f.Context=f.Context with{Revision="changed"};if(mode=="company-change")f.Company.Clear();};
        f.Client.Invocations.Clear();await f.State.LoadAsync(mode=="absent"?"missing":id);
        f.Client.Verify(c=>c.GetDraftPlansAsync(It.IsAny<CancellationToken>()),Times.Never);f.Client.Verify(c=>c.GetDraftOptionsAsync(It.IsAny<CancellationToken>()),Times.Never);
        Assert.False(f.State.CanEdit);Assert.Null(f.State.Pending);Assert.Equal(0,f.Store.Retains);Assert.Equal(0,f.Writes);
        if(mode is "detail" or "absent")
        {Assert.True(f.State.Loaded);Assert.Equal(mode=="absent",f.State.NotFound);if(mode=="detail"){Assert.Equal(original.Description,f.State.SourceReview!.Plan!.DEVELOPMENT_CONCEPT_DESC);f.State.SourceReview.Plan!.FDP_NAME="local";Assert.Equal(original.PlanName,f.State.SourceReview.Plan!.FDP_NAME);}}
        else{Assert.False(f.State.Loaded);Assert.Null(f.State.SourceReview);}
    }
    [Theory][InlineData("detail")][InlineData("absent")][InlineData("failed")][InlineData("missing-id")]
    public void ActualReviewPageShowsExactSavedDraftAndNoInventedApprovalOrFinancialValues(string mode)
    {
        using var f=new Fixture();f.Review().GetAwaiter().GetResult();var original=f.State.Review!;f.Rows.Add(Recorded(original));var id=DevelopmentPlanDraftReview.PlanIdFor(original);
        f.NullDetail=mode=="failed";using var ui=new Bunit.TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Fields.Object);
        ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Store);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        var location="/development/fdp/review"+(mode=="missing-id"?"":"?planId="+Uri.EscapeDataString(mode=="absent"?"missing":id));
        ui.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo(location);
        var page=ui.RenderComponent<DevelopmentPlanReview>();
        page.WaitForAssertion(()=>Assert.DoesNotContain("Loading recorded development draft",page.Markup));Assert.Contains("Review development draft",page.Markup);
        Assert.DoesNotContain("Submit for Approval",page.Markup);Assert.DoesNotContain("USD",page.Markup);Assert.Equal(0,f.Writes);
        if(mode=="detail")
        {Assert.Contains("New &lt;development&gt; concept",page.Markup);Assert.Contains("Review description",page.Markup);Assert.Contains("12.125 million KWD",page.Markup);Assert.Contains("Recorded by: planner",page.Markup);Assert.Contains("2030-04-01",page.Markup);
            f.Company.Clear();page.WaitForAssertion(()=>Assert.Contains("Choose a company",page.Markup));Assert.DoesNotContain("12.125 million KWD",page.Markup);}
        else if(mode=="absent")Assert.Contains("No recorded draft with this identity",page.Markup);
        else if(mode=="missing-id")Assert.Contains("Choose a recorded draft",page.Markup);
        else{Assert.Contains("Development drafts could not be confirmed",page.Markup);Assert.DoesNotContain("No recorded draft with this identity",page.Markup);}
    }
    private static DevelopmentReviewCatalog Catalog(Fixture f,string id,bool empty=false)
    {
        var row=f.Rows.Single(value=>value.FDP_ID==id);var source=new DevelopmentPlanSourceReview(f.Context,f.Planning,id,row,DevelopmentPlanSourceReview.RevisionFor(f.Context,f.Planning,id,row),DateTimeOffset.UtcNow);
        DevelopmentReviewWorkflowChoice[] choices=empty?[]:[new("review-workflow","Configured technical review","2.0",new('A',64),[new("technical","Technical review",1,"ConfiguredReviewer",["RESERVOIR_ENGINEER"],24)])];
        return new(source,new("lifecycle","lifecycle-binding"),choices,DevelopmentReviewCatalog.RevisionFor(source,new("lifecycle","lifecycle-binding"),choices),DateTimeOffset.UtcNow);
    }
    private static void EnableWorkflowView(Fixture f)
    {
        f.Company.Clear();f.Company.TrySelect(f.Company.Version,"planner",new(f.Context.Company,["ConfiguredPlanner"],[PermissionConstants.DevelopmentPlanning.View,PermissionConstants.DevelopmentPlanning.Create,"Workflow.View"]));
    }
    [Theory][InlineData("current")][InlineData("empty")][InlineData("failed")][InlineData("changed-source")][InlineData("changed-field")]
    public async Task WorkflowLookupRemainsBoundToDisplayedSourceAndItsFailureDoesNotEraseTheDraft(string mode)
    {
        using var f=new Fixture();await f.Review();var original=f.State.Review!;f.Rows.Add(Recorded(original));var id=DevelopmentPlanDraftReview.PlanIdFor(original);EnableWorkflowView(f);await f.State.LoadAsync(id);
        f.Client.Setup(client=>client.GetDraftReviewWorkflowsAsync(id,It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(()=>{
            if(mode=="failed")throw new HttpRequestException();if(mode=="changed-source")f.Rows[0].FDP_NAME="Changed source";
            var result=Catalog(f,id,mode=="empty");if(mode=="changed-field")f.Context=f.Context with{Revision="changed"};return result;});
        await f.State.LoadReviewWorkflowsAsync();Assert.True(f.State.Loaded);Assert.NotNull(f.State.SourceReview);Assert.Equal(original.PlanName,f.State.SourceReview.Plan!.FDP_NAME);
        if(mode is "current" or "empty"){Assert.NotNull(f.State.Workflows);Assert.Null(f.State.WorkflowError);Assert.Equal(mode=="empty"?0:1,f.State.Workflows.Choices.Count);}
        else{Assert.Null(f.State.Workflows);Assert.NotNull(f.State.WorkflowError);}Assert.Equal(0,f.Writes);Assert.False(f.State.Busy);
    }
    [Fact] public async Task NewSourceRouteDuringPendingWorkflowLookupClearsOldSourceAndCannotPublishItsLateResult()
    {
        using var f=new Fixture();await f.Review();var original=f.State.Review!;f.Rows.Add(Recorded(original));var id=DevelopmentPlanDraftReview.PlanIdFor(original);EnableWorkflowView(f);await f.State.LoadAsync(id);
        var pending=new TaskCompletionSource<DevelopmentReviewCatalog>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Client.Setup(client=>client.GetDraftReviewWorkflowsAsync(id,It.IsAny<string>(),It.IsAny<CancellationToken>())).Returns(pending.Task);
        var lookup=f.State.LoadReviewWorkflowsAsync();await f.State.LoadAsync("another-source");Assert.Null(f.State.SourceReview);pending.SetResult(Catalog(f,id));await lookup;
        Assert.False(f.State.Loaded);Assert.Null(f.State.Workflows);Assert.Null(f.State.SourceReview);Assert.False(f.State.Busy);
    }
    [Fact] public void ActualReviewPageLoadsConfiguredRoleStepsOnDemand()
    {
        using var f=new Fixture();f.Review().GetAwaiter().GetResult();var original=f.State.Review!;f.Rows.Add(Recorded(original));var id=DevelopmentPlanDraftReview.PlanIdFor(original);EnableWorkflowView(f);
        f.Client.Setup(client=>client.GetDraftReviewWorkflowsAsync(id,It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(()=>Catalog(f,id));
        using var ui=new Bunit.TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Fields.Object);
        ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Store);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo("/development/fdp/review?planId="+Uri.EscapeDataString(id));var page=ui.RenderComponent<DevelopmentPlanReview>();
        page.WaitForAssertion(()=>Assert.Contains("Inspect review workflows",page.Markup));Assert.DoesNotContain("Configured technical review",page.Markup);
        page.FindAll("button").Single(button=>button.TextContent.Contains("Inspect review workflows")).Click();
        page.WaitForAssertion(()=>Assert.Contains("Configured technical review",page.Markup));Assert.Contains("Assigned role: ConfiguredReviewer",page.Markup);Assert.Contains("Company reviewer personas: RESERVOIR_ENGINEER",page.Markup);Assert.Equal(0,f.Writes);
    }
    [Theory][InlineData("save")][InlineData("empty")][InlineData("readonly")][InlineData("failure")]
    public void ActualPlannerPageRendersScopedDataAndFormAndDrivesReviewSave(string mode)
    {
        using var f=new Fixture(mode!="readonly");if(mode=="failure")f.NullList=true;
        using var ui=new Bunit.TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Fields.Object);
        ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Store);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        var page=ui.RenderComponent<FDPWizard>();page.WaitForAssertion(()=>Assert.DoesNotContain("Loading development drafts",page.Markup));
        Assert.Contains("Field development drafts",page.Markup);Assert.DoesNotContain("Track Regulator Response",page.Markup);Assert.DoesNotContain("Revise FDP",page.Markup);
        if(mode=="failure"){page.WaitForAssertion(()=>Assert.Contains("Development drafts could not be confirmed",page.Markup));Assert.DoesNotContain("No recorded development drafts",page.Markup);return;}
        page.WaitForAssertion(()=>Assert.Contains("Company: tenant / company",page.Markup));
        if(mode=="readonly"){Assert.DoesNotContain("Prepare a development draft",page.Markup);Assert.Contains("Create permission is required",page.Markup);return;}
        Assert.Contains("No recorded development drafts",page.Markup);Assert.Contains("CAPEX (millions)",page.Markup);
        if(mode=="save")
        {
            page.Find("input[type=text]").Change("New <development> concept");
            page.FindAll("button").Single(button=>button.TextContent.Contains("Review draft inputs")).Click();
            page.WaitForAssertion(()=>Assert.Contains("Review draft before saving",page.Markup));Assert.Contains("New &lt;development&gt; concept",page.Markup);
            page.FindAll("button").Single(button=>button.TextContent.Contains("Save reviewed draft")).Click();
            page.WaitForAssertion(()=>Assert.Contains("Development draft saved and confirmed",page.Markup));Assert.Equal(1,f.Writes);
        }
        f.Company.Clear();page.WaitForAssertion(()=>Assert.Contains("Choose a company",page.Markup));Assert.DoesNotContain("New &lt;development&gt; concept",page.Markup);
    }
}
