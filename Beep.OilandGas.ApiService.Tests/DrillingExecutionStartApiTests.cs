#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed partial class DrillingExecutionStartApiTests
{
    [Fact]public Task DifferentExecutionPersonaUsesActualApprovedSourceRetainedStartAndViewOnlyRecoveryWithoutWellOrFinancialMutation()=>Exercise(false);
    [Fact]public Task FocusedConflictUsesCurrentOriginalFieldReadAuthorityAndPreservesClosingSourceChecks()=>Exercise(true);
    [Fact]public Task FocusedAssignmentConflictPreservesRecordedRoutingAndRequiresOriginalAccountReadAuthority()=>Exercise(false,true);
    [Fact]public Task FocusedReportConflictPreservesObservedHistoryAndRequiresOriginalReadAuthority()=>Exercise(false,false,true);
    [Fact]public Task FocusedReportPagesBindCurrentReadAuthorityAndRejectChangedHistoryWithoutMutations()=>Exercise(false,false,false,true);
    [Fact]public Task FocusedReportingSenderAndRecipientUsePairedCurrentExecutionReadAuthority()=>Exercise(false,false,false,false,true);
    [Fact]public Task FocusedReportCorrectionPreservesOriginalsAndUsesCurrentReadOnlyRecoveryAuthority()=>Exercise(false,reportCorrectionOnly:true);
    [Fact]public Task FocusedCompletionReportRemainsUnacceptedAndRecoversWithCurrentReadOnlyAuthority()=>Exercise(false,completionReportOnly:true);
    [Fact]public Task FocusedCompletionEvidenceReachesConfiguredRecipientWithoutCompletingResponsibility()=>Exercise(false,completionRecipientOnly:true);
    [Fact]public Task FocusedCompletionRoutingUsesCurrentClaimAndIndependentConfiguredRecipientsWithoutStartingReview()=>Exercise(false,completionRoutingOnly:true);
    [Fact]public Task FocusedCompletionSubmissionRecoversExactOriginalAndOpensQualifiedIndependentRecipientInbox()=>Exercise(false,completionSubmissionOnly:true);
    [Fact]public Task FocusedCompletionDecisionsRecoverOriginalAndRouteOrderedReviewerInboxWithoutClosingExecution()=>Exercise(false,completionDecisionOnly:true);
    [Fact]public Task FocusedPhysicalCompletionAcceptanceRecoversOriginalAndPreservesReportingResponsibility()=>Exercise(false,completionAcceptanceOnly:true);
    [Fact]public Task FocusedReportingClosureUsesManagerAuthorityAndReadOnlyRecoveryThenRemovesOnlyOwnedPendingWork()=>Exercise(false,completionAcceptanceOnly:true,reportingClosureOnly:true);
    [Fact]public Task FocusedReportingTransferUsesCurrentManagementAndRecipientAuthorityWithRetainedReadOnlyRecovery()=>Exercise(false,reportingTransferOnly:true);
    private async Task Exercise(bool conflictOnly,bool assignmentConflictOnly=false,bool reportConflictOnly=false,bool reportPagesOnly=false,bool assignmentReadersOnly=false,bool reportCorrectionOnly=false,bool completionReportOnly=false,bool completionRecipientOnly=false,bool completionRoutingOnly=false,bool completionSubmissionOnly=false,bool completionDecisionOnly=false,bool completionAcceptanceOnly=false,bool reportingClosureOnly=false,bool reportingTransferOnly=false)
    {
        using var f=new DrillingProgramApiTests.Fixture();await f.Prepare();var db=f.Funding.Routing.Afe.Db;foreach(var permission in new[]{"Drilling.View","Drilling.Approve"})db.RoleClaims.Add(new(){RoleId="investment-role",ClaimType="permission",ClaimValue=permission});foreach(var permission in new[]{"Drilling.View","Accounting.View"})db.RoleClaims.Add(new(){RoleId="review-role",ClaimType="permission",ClaimValue=permission});await db.SaveChangesAsync();await f.Select();
        var definition=CompanyInventorySourceReview.Copy(f.Funding.Routing.Definition);definition.PROCESS_DEFINITION_ID="execution-program-review";definition.PROCESS_TYPE="DRILLING";definition.ENTITY_TYPE="DRILLING_PROGRAM";definition.PROCESS_NAME="Execution basis independent program review";definition.PROCESS_CONFIG_JSON=JsonSerializer.Serialize(new{Steps=new[]{new ProcessStepDefinition{StepId="program-technical",StepName="Independent approved program",SequenceNumber=1,StepType="APPROVAL",RequiresApproval=true,RequiredRoles=["InvestmentReviewer"]}}});f.Funding.Routing.Catalog.Workflow.Native.Rows.Add(definition);
        var basis=await f.Basis();var draft=f.Store.Command with{Basis=basis,IntentId=Guid.NewGuid()};var saved=await f.Build().RecordAsync(f.Funding.Routing.Afe.Receiver.User,draft);var routing=await f.Build().ReviewRoutingAsync(f.Funding.Routing.Afe.Receiver.User,f.Funding.Routing.Afe.Process,draft.Basis.Funding.Source.AfeId,f.Well.UWI,saved.Recorded!.PROGRAM_ID,saved.Recorded.PROGRAM_DRAFT_REVISION);var original=new DrillingProgramReviewStart(routing.Source,routing.Options.Single(),Guid.NewGuid());await f.Build().SubmitReviewAsync(f.Funding.Routing.Afe.Receiver.User,original);
        db.Users.Add(new(){Id="site-recorder",IsActive=true});db.Roles.Add(new(){Id="execution-role",Name="ConfiguredExecutionRole",NormalizedName="CONFIGUREDEXECUTIONROLE"});db.UserRoles.Add(new(){UserId="site-recorder",RoleId="execution-role"});db.Add(new AppCompanyMembership{TenantId="tenant",BusinessAssociateId="company",UserId="site-recorder",GrantedByUserId="administrator",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});db.Add(new AppUserRoleExtension{TenantId="tenant",BusinessAssociateId="company",UserId="site-recorder",RoleId="execution-role",ApprovalStatus="Approved",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});db.Add(new AppPersona{Code="DRILLING_SITE",Name="Company drilling site recorder"});db.Add(new AppUserPersona{UserId="site-recorder",TenantId="tenant",BusinessAssociateId="company",PersonaCode="DRILLING_SITE"});foreach(var permission in new[]{"Drilling.View","Drilling.Create","Drilling.Edit","Drilling.ViewDailyReports","Drilling.ManageWellProgram","Accounting.View","DevelopmentPlanning.View","Workflow.View"})db.RoleClaims.Add(new(){RoleId="execution-role",ClaimType="permission",ClaimValue=permission});db.RoleClaims.Add(new(){RoleId="execution-role",ClaimType=RepositoryPersonaService.WorkspaceClaimType,ClaimValue="DRILLING_SITE"});db.Add(new AppUserAssetAccess{Id="site-original-field",UserId="site-recorder",DatabaseScope=basis.Scope().CoreDatabase.Fingerprint,OrganizationId="company",AssetType="FIELD",AssetId=basis.Scope().FieldId,AccessLevel="WRITE",IsActive=true});await db.SaveChangesAsync();await f.Select("site-recorder","ConfiguredExecutionRole");f.Funding.Routing.Afe.Receiver.Resources.Setup(r=>r.CheckAssetAccessAsync("site-recorder","field","FIELD","WRITE",null)).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=true});
        var native=f.Store.Store.Native;var classCreator=new ClassCreator(native.Editor.Object);native.Source.Setup(s=>s.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure requested,bool refresh)=>classCreator.ConvertToEntityStructure(requested.EntityName switch{"DRILLING_OPERATION"=>typeof(DRILLING_OPERATION),"DRILLING_DAILY_REPORT"=>typeof(DRILLING_DAILY_REPORT),_=>typeof(Beep.OilandGas.Models.Data.DrillingAndConstruction.DRILLING_PROGRAM)}));
        native.Source.Setup(s=>s.GetEntityAsync("DRILLING_DAILY_REPORT",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync((string table,List<TheTechIdea.Beep.Report.AppFilter> filters)=>native.Rows.Where(row=>row is DRILLING_DAILY_REPORT && DrillingDailyReportNativeTests.Matches(row,filters)).Select(row=>CompanyInventorySourceReview.Copy((DRILLING_DAILY_REPORT)row)).ToArray());
        Action? onExecutionInsert=null;native.Source.Setup(s=>s.InsertEntity("DRILLING_OPERATION",It.IsAny<object>())).Returns((string table,object value)=>{native.Writes++;native.Rows.Add(CompanyInventorySourceReview.Copy((DRILLING_OPERATION)value));onExecutionInsert?.Invoke();return new TheTechIdea.Beep.ConfigUtil.ErrorsInfo{Flag=TheTechIdea.Beep.ConfigUtil.Errors.Ok};});
        int reportWrites=0;Action? onReportInsert=null;native.Source.Setup(source=>source.InsertEntity("DRILLING_DAILY_REPORT",It.IsAny<object>())).Returns((string table,object value)=>{native.Writes++;reportWrites++;native.Rows.Add(CompanyInventorySourceReview.Copy((DRILLING_DAILY_REPORT)value));onReportInsert?.Invoke();return new TheTechIdea.Beep.ConfigUtil.ErrorsInfo{Flag=TheTechIdea.Beep.ConfigUtil.Errors.Ok};});
        await using var host=await AccountingSourceHttpTests.Start(configure:(services,_)=>{services.AddScoped<CompanyDrillingProgramService>(p=>f.Build(p.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>()));services.AddSingleton(Mock.Of<IFailureReporter>());});host.Client.DefaultRequestHeaders.Add("X-Test-Actor","site-recorder");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","ConfiguredExecutionRole");using var company=new Beep.OilandGas.Client.App.CompanyClientContext();void Selection(string actor){var current=f.Funding.Routing.Afe.Receiver.Tenant.Company!;company.TrySelect(company.Clear(),actor,new(current.Scope,current.Roles.ToArray(),current.Permissions.ToArray()));}Selection("site-recorder");var client=new Beep.OilandGas.Web.Services.DrillingServiceClient(new(host.Client,Mock.Of<IFailureReporter>(),company));var path="/api/drilling/programs/execution/basis?reviewId="+Uri.EscapeDataString(original.InstanceId());Assert.Equal(HttpStatusCode.Conflict,(await host.Client.GetAsync(path)).StatusCode);Assert.Equal(1,native.Writes);
        await f.Select("investor","InvestmentReviewer");var packet=await f.Build().ReadAssignedProgramAsync(f.Funding.Routing.Afe.Receiver.User,original.InstanceId());await f.Build().DecideProgramAsync(f.Funding.Routing.Afe.Receiver.User,new(packet,Guid.NewGuid(),"APPROVE","Program design independently approved before actual execution"));await f.Select("site-recorder","ConfiguredExecutionRole");f.Funding.Routing.Afe.Receiver.Resources.Setup(r=>r.CheckAssetAccessAsync("site-recorder","field","FIELD","WRITE",null)).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=true});Selection("site-recorder");var source=await client.GetExecutionBasisAsync(original.InstanceId());Assert.True(source.CanRecord);Assert.Equal("site-recorder",source.Scope().ActorUserId);Assert.Equal("DRILLING_SITE",source.PersonaCode);Assert.Equal("investor",source.Approvals.Single().Reviewer);Assert.Equal("APPROVED",source.Approvals.Single().State);Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.GetAsync("/api/drilling/programs/review/progress?processId="+Uri.EscapeDataString(original.InstanceId()))).StatusCode);Assert.Null((await client.GetExecutionStatusAsync(original.InstanceId())).Recorded);
        f.Well.ACTIVE_IND="N";Assert.Equal(HttpStatusCode.Conflict,(await host.Client.GetAsync(path)).StatusCode);f.Well.ACTIVE_IND="Y";source=await client.GetExecutionBasisAsync(original.InstanceId());var command=new DrillingExecutionStart(source,Guid.NewGuid(),"ConfiguredExecutionRole",DateTimeOffset.UtcNow,"Observed site execution start, actual UTC and original approved program confirmed");var grant=await db.Set<AppUserAssetAccess>().SingleAsync(row=>row.UserId=="site-recorder");grant.AccessLevel="READ";await db.SaveChangesAsync();await f.Select("site-recorder","ConfiguredExecutionRole");Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.PostAsJsonAsync("/api/drilling/programs/execution/start",command)).StatusCode);Assert.Equal(1,native.Writes);grant.AccessLevel="WRITE";await db.SaveChangesAsync();await f.Select("site-recorder","ConfiguredExecutionRole");f.Funding.Routing.Afe.Receiver.Resources.Setup(r=>r.CheckAssetAccessAsync("site-recorder","field","FIELD","WRITE",null)).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=true});
        var lifecycleBinding=await db.Set<ModuleDatabaseBinding>().SingleAsync(row=>row.ModuleId=="LIFECYCLE");var originalStamp=lifecycleBinding.ConcurrencyStamp;onExecutionInsert=()=>{lifecycleBinding.ConcurrencyStamp="changed-during-execution-insert";db.SaveChanges();};await Assert.ThrowsAnyAsync<Exception>(()=>client.StartExecutionAsync(command));onExecutionInsert=null;Assert.Empty(native.Rows.OfType<DRILLING_OPERATION>());Assert.Equal(2,native.Writes);lifecycleBinding.ConcurrencyStamp=originalStamp;await db.SaveChangesAsync();
        var well=JsonSerializer.Serialize(f.Well);var financeWrites=f.Funding.Routing.Afe.Native.Native.Writes;var lifecycle=JsonSerializer.Serialize(f.Funding.Routing.Catalog.Workflow.Native.Rows);native.AfterCommit=()=>{if(native.Rows.OfType<DRILLING_OPERATION>().Any())native.FailCommit=true;};await Assert.ThrowsAnyAsync<Exception>(()=>client.StartExecutionAsync(command));native.AfterCommit=null;native.FailCommit=false;Assert.Single(native.Rows.OfType<DRILLING_OPERATION>());Assert.Equal(3,native.Writes);
        if(completionSubmissionOnly || completionDecisionOnly || completionAcceptanceOnly || reportingTransferOnly)
        {
            if(completionAcceptanceOnly || reportingTransferOnly)await DrillingExecutionAssignmentApiJourney.Exercise(f,command);
            var ongoingId=f.Funding.Routing.Catalog.Workflow.Native.Rows.OfType<PROCESS_INSTANCE>().SingleOrDefault(row=>row.ENTITY_TYPE=="DRILLING_OPERATION")?.PROCESS_INSTANCE_ID;
            if(reportingTransferOnly){await ExerciseReportingTransfer(f,ongoingId!);return;}
            string OngoingGraph()=>JsonSerializer.Serialize(f.Funding.Routing.Catalog.Workflow.Native.Rows.Where(row=>ongoingId is not null && row.GetType().GetProperty("PROCESS_INSTANCE_ID")?.GetValue(row)?.ToString()==ongoingId));var ongoingBefore=OngoingGraph();
            await ConfigureCompletionRouting(f,command.Basis.Scope());
            var owner=f.Funding.Routing.Catalog.Workflow;
            owner.Service=new(owner.Native.Editor.Object,new Beep.OilandGas.PPDM39.DataManagement.Core.Common.CommonColumnHandler(),Mock.Of<Beep.OilandGas.PPDM39.Core.IPPDM39DefaultsRepository>(),Mock.Of<Beep.OilandGas.PPDM39.Core.Metadata.IPPDMMetadataRepository>(),"lifecycle",personaReader:owner.Native.Personas.Object);
            f.Funding.Routing.Afe.Receiver.Resources.Setup(resource=>resource.CheckAssetAccessAsync("site-recorder","field","FIELD","WRITE",null)).ReturnsAsync(()=>{
                Assert.DoesNotContain("begin",owner.Native.Transactions.TakeLast(1));return new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=true};});
            await CorrectObservedReport(f,command,client,host.Client,Selection,true,recipientEvidence:completionAcceptanceOnly,routingEvidence:true,submissionEvidence:true,decisionEvidence:completionDecisionOnly || completionAcceptanceOnly,acceptanceEvidence:completionAcceptanceOnly);
            Assert.Equal(2,reportWrites);Assert.Equal(5,native.Writes);Assert.Equal(financeWrites,f.Funding.Routing.Afe.Native.Native.Writes);
            Assert.Null(f.Well.SPUD_DATE);if(completionAcceptanceOnly){Assert.NotNull(ongoingId);Assert.Equal(ongoingBefore,OngoingGraph());Assert.Equal("PENDING",f.Funding.Routing.Catalog.Workflow.Native.Rows.OfType<CROSS_PERSONA_TASK>().Single(row=>row.PROCESS_INSTANCE_ID==ongoingId).TASK_STATUS);}
            if(reportingClosureOnly)await ExerciseReportingClosure(f,ongoingId!);
            return;
        }
        if(completionRoutingOnly)
        {
            await ConfigureCompletionRouting(f,command.Basis.Scope());
            var configuredLifecycle=JsonSerializer.Serialize(f.Funding.Routing.Catalog.Workflow.Native.Rows);
            await CorrectObservedReport(f,command,client,host.Client,Selection,true,routingEvidence:true);
            Assert.Equal(2,reportWrites);Assert.Equal(5,native.Writes);Assert.Equal(financeWrites,f.Funding.Routing.Afe.Native.Native.Writes);
            Assert.Null(f.Well.SPUD_DATE);Assert.Equal(configuredLifecycle,JsonSerializer.Serialize(f.Funding.Routing.Catalog.Workflow.Native.Rows));return;
        }
        if(completionRecipientOnly)
        {
            await DrillingExecutionAssignmentApiJourney.Exercise(f,command);
            var originalResponsibility=JsonSerializer.Serialize(f.Funding.Routing.Catalog.Workflow.Native.Rows);
            await CorrectObservedReport(f,command,client,host.Client,Selection,true,true);
            Assert.Equal(2,reportWrites);Assert.Equal(5,native.Writes);Assert.Equal(financeWrites,f.Funding.Routing.Afe.Native.Native.Writes);
            Assert.Null(f.Well.SPUD_DATE);Assert.Equal(originalResponsibility,JsonSerializer.Serialize(f.Funding.Routing.Catalog.Workflow.Native.Rows));return;
        }
        if(completionReportOnly)
        {
            await CorrectObservedReport(f,command,client,host.Client,Selection,true);
            Assert.Equal(2,reportWrites);Assert.Equal(5,native.Writes);Assert.Equal(financeWrites,f.Funding.Routing.Afe.Native.Native.Writes);
            Assert.Null(f.Well.SPUD_DATE);Assert.Equal(lifecycle,JsonSerializer.Serialize(f.Funding.Routing.Catalog.Workflow.Native.Rows));return;
        }
        if(reportCorrectionOnly)
        {
            await CorrectObservedReport(f,command,client,host.Client,Selection);
            Assert.Equal(2,reportWrites);Assert.Equal(5,native.Writes);Assert.Equal(financeWrites,f.Funding.Routing.Afe.Native.Native.Writes);
            Assert.Null(f.Well.SPUD_DATE);Assert.Equal(lifecycle,JsonSerializer.Serialize(f.Funding.Routing.Catalog.Workflow.Native.Rows));return;
        }
        if(assignmentReadersOnly)
        {
            await DrillingExecutionAssignmentApiJourney.Exercise(f,command);
            Assert.Equal(3,native.Writes);Assert.Equal(financeWrites,f.Funding.Routing.Afe.Native.Native.Writes);Assert.Null(f.Well.SPUD_DATE);return;
        }
        if(reportPagesOnly)
        {
            await InspectReportPages(f,command,client,host.Client,Selection);
            Assert.Equal(1,reportWrites);Assert.Equal(4,native.Writes);Assert.Equal(financeWrites,f.Funding.Routing.Afe.Native.Native.Writes);
            Assert.Null(f.Well.SPUD_DATE);Assert.Equal(lifecycle,JsonSerializer.Serialize(f.Funding.Routing.Catalog.Workflow.Native.Rows));return;
        }
        if(reportConflictOnly)
        {
            await ReconcileReportHistory(f,command,client,host.Client,Selection);
            Assert.Equal(1,reportWrites);Assert.Equal(4,native.Writes);Assert.Equal(financeWrites,f.Funding.Routing.Afe.Native.Native.Writes);
            Assert.Null(f.Well.SPUD_DATE);Assert.Equal(lifecycle,JsonSerializer.Serialize(f.Funding.Routing.Catalog.Workflow.Native.Rows));return;
        }
        if(assignmentConflictOnly)
        {
            await DrillingExecutionAssignmentApiJourney.Exercise(f,command,true);
            Assert.Equal(3,native.Writes);Assert.Equal(financeWrites,f.Funding.Routing.Afe.Native.Native.Writes);Assert.Null(f.Well.SPUD_DATE);return;
        }
        if(conflictOnly)
        {
            db.RemoveRange(await db.RoleClaims.Where(row=>row.RoleId=="execution-role" && (row.ClaimValue=="Drilling.Create" || row.ClaimValue=="Drilling.Edit" || row.ClaimValue=="Drilling.ManageWellProgram")).ToArrayAsync());
            grant.AccessLevel="READ";db.Add(new AppPersona{Code="CONFLICT_READER",Name="Unmapped conflict reader"});
            (await db.Set<AppUserPersona>().SingleAsync(row=>row.UserId=="site-recorder")).PersonaCode="CONFLICT_READER";
            await db.SaveChangesAsync();await f.Select("site-recorder","ConfiguredExecutionRole");f.Well.ACTIVE_IND="N";Selection("site-recorder");
            await ReconcileCompetingStart(client,host.Client,command);Assert.Equal(3,native.Writes);
            var unrecorded=command with{IntentId=Guid.NewGuid()};
            grant.IsActive=false;await db.SaveChangesAsync();
            Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.PostAsJsonAsync("/api/drilling/programs/execution/conflict",unrecorded)).StatusCode);
            grant.IsActive=true;await db.SaveChangesAsync();
            Action? onExecutionRead=()=>{grant.IsActive=false;db.SaveChanges();};
            native.Source.Setup(s=>s.GetEntityAsync("DRILLING_OPERATION",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync((string table,List<TheTechIdea.Beep.Report.AppFilter> filters)=>
            {
                var change=onExecutionRead;onExecutionRead=null;change?.Invoke();
                return native.Rows.Where(row=>row.GetType().Name==table && filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).Select(row=>JsonSerializer.Deserialize(JsonSerializer.Serialize(row),row.GetType())!).ToArray();
            });
            Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.PostAsJsonAsync("/api/drilling/programs/execution/conflict",unrecorded)).StatusCode);
            Assert.Null(onExecutionRead);Assert.Equal(3,native.Writes);grant.IsActive=true;await db.SaveChangesAsync();
            var step=f.Funding.Routing.Catalog.Workflow.Native.Rows.OfType<PROCESS_STEP_INSTANCE>().Single(row=>row.PROCESS_INSTANCE_ID==original.InstanceId());
            onExecutionRead=()=>step.STATUS="PENDING";
            Assert.Equal(HttpStatusCode.Conflict,(await host.Client.PostAsJsonAsync("/api/drilling/programs/execution/conflict",unrecorded)).StatusCode);
            Assert.Null(onExecutionRead);Assert.Equal(3,native.Writes);Assert.Equal(financeWrites,f.Funding.Routing.Afe.Native.Native.Writes);Assert.Null(f.Well.SPUD_DATE);
            return;
        }
        await DrillingExecutionAssignmentApiJourney.Exercise(f,command);
        lifecycle=JsonSerializer.Serialize(f.Funding.Routing.Catalog.Workflow.Native.Rows);
        Assert.Empty((await client.GetDailyReportPageAsync(original.InstanceId())).Reports);
        var reportBasis=await client.GetDailyReportBasisAsync(original.InstanceId());Assert.True(reportBasis.CanRecord);
        var report=new DrillingDailyReportInput(reportBasis,Guid.NewGuid(),"ConfiguredExecutionRole",command.SpudAtUtc,DateTimeOffset.UtcNow,0,125,source.Source.Program.DEPTH_OUOM,null,null,null,"Observed site log 471 / actual measured hole MD","Observed progress after actual approved-program execution start",null);
        grant.AccessLevel="READ";await db.SaveChangesAsync();await f.Select("site-recorder","ConfiguredExecutionRole");
        Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.PostAsJsonAsync("/api/drilling/programs/execution/reports",report)).StatusCode);Assert.Equal(0,reportWrites);
        grant.AccessLevel="WRITE";await db.SaveChangesAsync();await f.Select("site-recorder","ConfiguredExecutionRole");f.Funding.Routing.Afe.Receiver.Resources.Setup(r=>r.CheckAssetAccessAsync("site-recorder","field","FIELD","WRITE",null)).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=true});
        onReportInsert=()=>{lifecycleBinding.ConcurrencyStamp="changed-during-report-insert";db.SaveChanges();};
        await Assert.ThrowsAnyAsync<Exception>(()=>client.RecordDailyReportAsync(report));onReportInsert=null;Assert.Empty(native.Rows.OfType<DRILLING_DAILY_REPORT>());Assert.Equal(1,reportWrites);lifecycleBinding.ConcurrencyStamp=originalStamp;await db.SaveChangesAsync();
        native.AfterCommit=()=>{if(native.Rows.OfType<DRILLING_DAILY_REPORT>().Any())native.FailCommit=true;};
        await Assert.ThrowsAnyAsync<Exception>(()=>client.RecordDailyReportAsync(report));native.AfterCommit=null;native.FailCommit=false;Assert.Single(native.Rows.OfType<DRILLING_DAILY_REPORT>());Assert.Equal(2,reportWrites);
        db.RemoveRange(await db.RoleClaims.Where(row=>row.RoleId=="execution-role" && (row.ClaimValue=="Drilling.Create" || row.ClaimValue=="Drilling.Edit" || row.ClaimValue=="Drilling.ManageWellProgram")).ToArrayAsync());grant.AccessLevel="READ";db.Add(new AppPersona{Code="UNMAPPED",Name="Workspace without a current execution role mapping"});(await db.Set<AppUserPersona>().SingleAsync(row=>row.UserId=="site-recorder")).PersonaCode="UNMAPPED";await db.SaveChangesAsync();await f.Select("site-recorder","ConfiguredExecutionRole");f.Well.ACTIVE_IND="N";Selection("site-recorder");var recovered=await client.RecoverExecutionStartAsync(command);recovered.Require(command);Assert.Equal(command.OperationId(),recovered.Recorded!.DRILLING_OPERATION_ID);Assert.Equal(command.SpudAtUtc.UtcDateTime,recovered.Recorded.SPUD_DATE);Assert.Equal("STARTED",recovered.Recorded.STATUS);Assert.Equal(3+reportWrites,native.Writes);Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.PostAsJsonAsync("/api/drilling/programs/execution/start",command)).StatusCode);var status=await client.GetExecutionStatusAsync(original.InstanceId());Assert.False(status.Basis.CanRecord);Assert.Equal(command.OperationId(),status.Recorded!.DRILLING_OPERATION_ID);Assert.Equal("site-recorder",status.Recorded.ROW_CREATED_BY);Assert.Equal(3+reportWrites,native.Writes);await ReconcileCompetingStart(client,host.Client,command);Assert.Equal(3+reportWrites,native.Writes);f.Well.ACTIVE_IND="Y";Assert.Equal(well,JsonSerializer.Serialize(f.Well));Assert.Equal(financeWrites,f.Funding.Routing.Afe.Native.Native.Writes);Assert.Equal(lifecycle,JsonSerializer.Serialize(f.Funding.Routing.Catalog.Workflow.Native.Rows));Assert.Equal("DRAFT",saved.Recorded.PROGRAM_STATUS);Assert.Null(f.Well.SPUD_DATE);
        var reportRecovered=await client.RecoverDailyReportAsync(report);reportRecovered.Require(report);Assert.Equal(report.Provenance,reportRecovered.Recorded!.REPORT_PROVENANCE);Assert.Null(reportRecovered.Recorded.NPT_HOURS);Assert.Equal(2,reportWrites);
        Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.PostAsJsonAsync("/api/drilling/programs/execution/reports",report)).StatusCode);
        var reports=await client.GetDailyReportPageAsync(original.InstanceId());Assert.Single(reports.Reports);Assert.Equal(125,reports.Reports[0].DEPTH_END_MD);Assert.Equal("site-recorder",reports.Reports[0].ROW_CREATED_BY);Assert.Equal(2,reportWrites);
        Assert.Equal(HttpStatusCode.NotFound,(await host.Client.PostAsJsonAsync("/api/drilling/program/create",new{Well=f.Well.UWI})).StatusCode);
        db.Add(new AppUserAssetAccess{Id="foreign-execution-field",UserId="foreign-reviewer",DatabaseScope=source.Scope().CoreDatabase.Fingerprint,OrganizationId="foreign",AssetType="FIELD",AssetId=source.Scope().FieldId,AccessLevel="READ",IsActive=true});await db.SaveChangesAsync();await f.Select("foreign-reviewer","ConfiguredReviewRole",new("tenant","foreign"));host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Remove("X-Test-Assigned-Role");host.Client.DefaultRequestHeaders.Add("X-Test-Actor","foreign-reviewer");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","ConfiguredReviewRole");Selection("foreign-reviewer");var denied=await host.Client.GetAsync("/api/drilling/programs/execution?reviewId="+Uri.EscapeDataString(original.InstanceId()));Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);Assert.DoesNotContain(command.OperationId(),await denied.Content.ReadAsStringAsync());var foreignReports=await host.Client.GetAsync("/api/drilling/programs/execution/reports?reviewId="+Uri.EscapeDataString(original.InstanceId()));Assert.Equal(HttpStatusCode.Forbidden,foreignReports.StatusCode);Assert.DoesNotContain(report.Provenance,await foreignReports.Content.ReadAsStringAsync());Assert.Equal(3+reportWrites,native.Writes);Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.PostAsJsonAsync("/api/drilling/programs/execution/conflict",command with{IntentId=Guid.NewGuid()})).StatusCode);host.Client.DefaultRequestHeaders.Remove("X-Test-Auth");Assert.Equal(HttpStatusCode.Unauthorized,(await host.Client.PostAsJsonAsync("/api/drilling/programs/execution/recover",command)).StatusCode);Assert.Equal(HttpStatusCode.Unauthorized,(await host.Client.PostAsJsonAsync("/api/drilling/programs/execution/reports/recover",report)).StatusCode);
    }
    private static async Task ReconcileCompetingStart(Beep.OilandGas.Web.Services.DrillingServiceClient client,HttpClient http,DrillingExecutionStart recorded)
    {
        var original=recorded with{IntentId=Guid.NewGuid(),Rationale="Another retained original that was never recorded"};
        var proof=await client.GetExecutionConflictAsync(original);proof.Require(original);
        Assert.True(proof.OriginalConfirmedAbsent);Assert.Equal(recorded.OperationId(),proof.Competing.DRILLING_OPERATION_ID);
        var confirmed=await client.ConfirmExecutionConflictAsync(new(original,proof));
        Assert.Equal(proof.Revision,confirmed.Revision);Assert.True(confirmed.CheckedAtUtc>=proof.CheckedAtUtc);
        Assert.Equal(HttpStatusCode.Conflict,(await http.PostAsJsonAsync("/api/drilling/programs/execution/conflict",recorded)).StatusCode);
        var altered=CompanyInventorySourceReview.Copy(proof);var other=DrillingExecutionStart.FromRecord(altered.Competing) with{Rationale="A different reviewed original"};
        altered=altered with{Competing=other.ToRecord(altered.Competing.ROW_CREATED_DATE!.Value)};
        altered=altered with{Revision=altered.ContentRevision()};altered.Require(original);
        Assert.Equal(HttpStatusCode.Conflict,(await http.PostAsJsonAsync("/api/drilling/programs/execution/conflict/confirm",new DrillingExecutionConflictAcknowledgement(original,altered))).StatusCode);
    }
}
#endif
