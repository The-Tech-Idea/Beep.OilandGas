#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

internal static class DrillingExecutionAssignmentApiJourney
{
    internal static async Task Exercise(DrillingProgramApiTests.Fixture f,DrillingExecutionStart execution,bool reconcileOnly=false)
    {
        var db=f.Funding.Routing.Afe.Db;var lifecycle=f.Funding.Routing.Catalog.Workflow;var scope=execution.Basis.Scope();
        lifecycle.Service=new(lifecycle.Native.Editor.Object,new CommonColumnHandler(),Mock.Of<IPPDM39DefaultsRepository>(),Mock.Of<IPPDMMetadataRepository>(),"lifecycle",personaReader:lifecycle.Native.Personas.Object);
        db.RoleClaims.Add(new(){RoleId="execution-role",ClaimType="permission",ClaimValue="Workflow.Start"});
        db.Users.Add(new(){Id="execution-recipient",IsActive=true});db.Roles.Add(new(){Id="reporting-role",Name="ConfiguredReportingRole",NormalizedName="CONFIGUREDREPORTINGROLE"});
        db.UserRoles.Add(new(){UserId="execution-recipient",RoleId="reporting-role"});
        db.Add(new AppCompanyMembership{TenantId="tenant",BusinessAssociateId="company",UserId="execution-recipient",GrantedByUserId="administrator",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});
        db.Add(new AppUserRoleExtension{TenantId="tenant",BusinessAssociateId="company",UserId="execution-recipient",RoleId="reporting-role",ApprovalStatus="Approved",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});
        db.Add(new AppPersona{Code="DRILLING_REPORTER",Name="Configured reporting site"});db.Add(new AppPersona{Code="DRILLING_UNMAPPED",Name="Unmapped historical reader"});
        db.Add(new AppUserPersona{UserId="execution-recipient",TenantId="tenant",BusinessAssociateId="company",PersonaCode="DRILLING_REPORTER"});
        foreach(var permission in new[]{"Drilling.View","Drilling.ViewDailyReports","Drilling.Edit","Drilling.ManageWellProgram","Accounting.View","DevelopmentPlanning.View","Workflow.View"})
            db.RoleClaims.Add(new(){RoleId="reporting-role",ClaimType="permission",ClaimValue=permission});
        db.RoleClaims.Add(new(){RoleId="reporting-role",ClaimType=RepositoryPersonaService.WorkspaceClaimType,ClaimValue="DRILLING_REPORTER"});
        db.Add(new AppUserAssetAccess{Id="execution-recipient-field",UserId="execution-recipient",DatabaseScope=scope.CoreDatabase.Fingerprint,OrganizationId="company",AssetType="FIELD",AssetId=scope.FieldId,AccessLevel="WRITE",IsActive=true});
        await db.SaveChangesAsync();
        var definition=CompanyInventorySourceReview.Copy(f.Funding.Routing.Definition);definition.PROCESS_DEFINITION_ID="configured-execution-reporting";definition.PROCESS_TYPE="DRILLING";definition.ENTITY_TYPE="DRILLING_OPERATION";definition.PROCESS_NAME="Configured ongoing reporting";
        definition.PROCESS_CONFIG_JSON=JsonSerializer.Serialize(new{Steps=new[]{new ProcessStepDefinition{StepId="record-progress",StepName="Record observed execution progress",StepType="DATA_ENTRY",RequiresApproval=false,SequenceNumber=1,RequiredRoles=["ConfiguredReportingRole"]}}});lifecycle.Native.Rows.Add(definition);
        await using var host=await AccountingSourceHttpTests.Start(configure:(services,_)=>{
            services.AddScoped<CompanyDrillingProgramService>(p=>f.Build(p.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>()));
            services.AddScoped<IDrillingProgramAssignedReviewReader>(p=>p.GetRequiredService<CompanyDrillingProgramService>());services.AddSingleton(db);
            services.AddScoped<RepositoryTenantContext>(_=>f.Funding.Routing.Afe.Receiver.Tenant);services.AddScoped<IRepositoryAccessService>(_=>f.Funding.Routing.Afe.Receiver.Accounts);
            services.AddScoped<RepositoryPersonaService>(_=>f.Funding.Routing.Afe.Receiver.Workspaces);services.AddScoped<IAccessControlService>(_=>f.Funding.Routing.Afe.Receiver.Resources.Object);
            services.AddScoped<IProcessService>(_=>f.Funding.Routing.Afe.Receiver.Processes);services.AddScoped<ICrossPersonaTaskRouter>(_=>lifecycle.Native.Router);services.AddSingleton(Mock.Of<IFailureReporter>());
        });
        using var company=new Beep.OilandGas.Client.App.CompanyClientContext();
        async Task Select(string actor,string role)
        {
            await f.Select(actor,role);host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Remove("X-Test-Assigned-Role");
            host.Client.DefaultRequestHeaders.Add("X-Test-Actor",actor);host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role",role);
            var selected=f.Funding.Routing.Afe.Receiver.Tenant.Company!;company.TrySelect(company.Clear(),actor,new(selected.Scope,selected.Roles.ToArray(),selected.Permissions.ToArray()));
        }
        await Select("site-recorder","ConfiguredExecutionRole");var client=new Beep.OilandGas.Web.Services.DrillingServiceClient(new(host.Client,Mock.Of<IFailureReporter>(),company));
        var review=await client.GetExecutionAssignmentAsync(execution.Basis.ReviewInstanceId);Assert.True(review.CanAssign);var option=Assert.Single(review.Options);
        Assert.Equal("ConfiguredReportingRole",option.Step.AssignedRole);Assert.Equal("execution-recipient",Assert.Single(option.Authority.Recipients).UserId);
        var original=new DrillingExecutionAssignment(review.Source,option,"ConfiguredExecutionRole",review.SenderPersonaCode!,review.SenderAuthorityRevision!,Guid.NewGuid(),"Original company site reporting responsibility assigned");
        var before=lifecycle.Native.Writes;var originalExecution=JsonSerializer.Serialize(f.Store.Store.Native.Rows.OfType<DRILLING_OPERATION>());
        lifecycle.Native.AfterCommit=()=>{if(lifecycle.Native.Rows.OfType<PROCESS_INSTANCE>().Any(row=>row.ENTITY_TYPE=="DRILLING_OPERATION"))lifecycle.Native.FailCommit=true;};
        await Assert.ThrowsAnyAsync<Exception>(()=>client.AssignExecutionAsync(original));lifecycle.Native.AfterCommit=null;lifecycle.Native.FailCommit=false;
        Assert.Single(lifecycle.Native.Rows.OfType<PROCESS_INSTANCE>().Where(row=>row.ENTITY_TYPE=="DRILLING_OPERATION"));Assert.Equal(before+4,lifecycle.Native.Writes);
        var removed=await db.RoleClaims.Where(row=>row.RoleId=="execution-role" && (row.ClaimValue=="Drilling.Edit" || row.ClaimValue=="Drilling.ManageWellProgram" || row.ClaimValue=="Workflow.Start")).ToArrayAsync();
        db.RemoveRange(removed);var senderProfile=await db.Set<AppUserPersona>().SingleAsync(row=>row.UserId=="site-recorder");senderProfile.PersonaCode="DRILLING_UNMAPPED";
        var senderGrant=await db.Set<AppUserAssetAccess>().SingleAsync(row=>row.UserId=="site-recorder");senderGrant.AccessLevel="READ";await db.SaveChangesAsync();await Select("site-recorder","ConfiguredExecutionRole");
        Assert.True((await client.RecoverExecutionAssignmentAsync(original)).Recorded);Assert.Equal(before+4,lifecycle.Native.Writes);
        Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.PostAsJsonAsync("/api/drilling/programs/execution/assignment",original)).StatusCode);
        if(reconcileOnly)
        {
            f.Well.ACTIVE_IND="N";
            var unrecorded=original with{IntentId=Guid.NewGuid(),Reason="Retained different original reporting assignment"};
            var beforeInspection=JsonSerializer.Serialize(lifecycle.Native.Rows);
            var proof=await client.GetExecutionAssignmentConflictAsync(unrecorded);proof.Require(unrecorded);
            Assert.Equal(original.InstanceId(),proof.Competing.InstanceId());Assert.Equal("execution-recipient",Assert.Single(proof.Competing.Workflow.Authority.Recipients).UserId);
            var confirmed=await client.ConfirmExecutionAssignmentConflictAsync(new(unrecorded,proof.Revision));Assert.Equal(proof.Revision,confirmed.Revision);
            Assert.Equal(HttpStatusCode.Conflict,(await host.Client.PostAsJsonAsync("/api/drilling/programs/execution/assignment/conflict",original)).StatusCode);
            Assert.Equal(HttpStatusCode.Conflict,(await host.Client.PostAsJsonAsync("/api/drilling/programs/execution/assignment/conflict/confirm",new DrillingExecutionAssignmentConflictAcknowledgement(unrecorded,new('F',64)))).StatusCode);
            Assert.Equal(beforeInspection,JsonSerializer.Serialize(lifecycle.Native.Rows));Assert.Equal(before+4,lifecycle.Native.Writes);
            senderGrant.IsActive=false;await db.SaveChangesAsync();
            Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.PostAsJsonAsync("/api/drilling/programs/execution/assignment/conflict",unrecorded)).StatusCode);
            senderGrant.IsActive=true;await db.SaveChangesAsync();Action? revoke=()=>{senderGrant.IsActive=false;db.SaveChanges();};
            lifecycle.Native.Source.Setup(source=>source.GetEntityAsync(It.IsAny<string>(),It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync((string table,List<TheTechIdea.Beep.Report.AppFilter> filters)=>
            {
                if(table=="PROCESS_INSTANCE" && filters.Any(filter=>filter.FieldName=="PROCESS_INSTANCE_ID" && filter.FilterValue==unrecorded.InstanceId())){var change=revoke;revoke=null;change?.Invoke();}
                return lifecycle.Native.Rows.Where(row=>row.GetType().Name==table && filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).Select(row=>JsonSerializer.Deserialize(JsonSerializer.Serialize(row),row.GetType())!).ToArray();
            });
            Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.PostAsJsonAsync("/api/drilling/programs/execution/assignment/conflict",unrecorded)).StatusCode);Assert.Null(revoke);
            senderGrant.IsActive=true;await db.SaveChangesAsync();
            var orphan=new PROCESS_HISTORY{PROCESS_HISTORY_ID="original-orphan",PROCESS_INSTANCE_ID=unrecorded.InstanceId()};lifecycle.Native.Rows.Add(orphan);
            Assert.Equal(HttpStatusCode.Conflict,(await host.Client.PostAsJsonAsync("/api/drilling/programs/execution/assignment/conflict",unrecorded)).StatusCode);lifecycle.Native.Rows.Remove(orphan);
            await Select("execution-recipient","ConfiguredReportingRole");
            Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.PostAsJsonAsync("/api/drilling/programs/execution/assignment/conflict",unrecorded)).StatusCode);
            await f.Select("foreign-reviewer","ConfiguredReviewRole",new("tenant","foreign"));
            host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Remove("X-Test-Assigned-Role");host.Client.DefaultRequestHeaders.Add("X-Test-Actor","foreign-reviewer");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","ConfiguredReviewRole");
            var foreign=await host.Client.PostAsJsonAsync("/api/drilling/programs/execution/assignment/conflict",unrecorded);Assert.Equal(HttpStatusCode.Forbidden,foreign.StatusCode);Assert.DoesNotContain(original.InstanceId(),await foreign.Content.ReadAsStringAsync());
            host.Client.DefaultRequestHeaders.Remove("X-Test-Auth");
            Assert.Equal(HttpStatusCode.Unauthorized,(await host.Client.PostAsJsonAsync("/api/drilling/programs/execution/assignment/conflict",unrecorded)).StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized,(await host.Client.PostAsJsonAsync("/api/drilling/programs/execution/assignment/conflict/confirm",new DrillingExecutionAssignmentConflictAcknowledgement(unrecorded,proof.Revision))).StatusCode);
            Assert.Equal(before+4,lifecycle.Native.Writes);Assert.Equal(originalExecution,JsonSerializer.Serialize(f.Store.Store.Native.Rows.OfType<DRILLING_OPERATION>()));return;
        }
        await Select("execution-recipient","ConfiguredReportingRole");var assigned=await client.GetAssignedExecutionAsync(original.InstanceId());
        Assert.Equal("execution-recipient",assigned.Source.Basis.Scope().ActorUserId);Assert.Equal("site-recorder",assigned.Responsibility.Root.Scope().ActorUserId);
        var inbox=await host.Client.GetFromJsonAsync<UnifiedInbox>("/api/workflow/tasks/inbox?personaCode=DRILLING_REPORTER");var task=Assert.Single(inbox!.NormalTasks);
        Assert.Equal(assigned.TaskId,task.TaskId);Assert.Equal(assigned.Revision,task.ExecutionAssignmentRevision);Assert.Equal("DATA_ENTRY",task.TaskType);Assert.Equal("UNSCHEDULED",task.SlaStatus);
        Assert.Equal(1,inbox.Counts.TotalPending);Assert.Equal(1,inbox.Counts.DataEntry);Assert.Equal(0,inbox.Counts.Reviews);
        Assert.Equal(WorkflowTaskRoute.For("FIELD",original.InstanceId(),"DRILLING_OPERATION"),task.Route);
        var receiverGrant=await db.Set<AppUserAssetAccess>().SingleAsync(row=>row.UserId=="execution-recipient");receiverGrant.AccessLevel="READ";
        var edit=await db.RoleClaims.SingleAsync(row=>row.RoleId=="reporting-role" && row.ClaimValue=="Drilling.Edit");db.Remove(edit);await db.SaveChangesAsync();await Select("execution-recipient","ConfiguredReportingRole");
        Assert.Equal(original.InstanceId(),(await client.GetAssignedExecutionAsync(original.InstanceId())).InstanceId);
        Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.PostAsJsonAsync("/api/drilling/programs/execution/assignment/recover",original)).StatusCode);
        var assignedRole=await db.Set<AppUserRoleExtension>().SingleAsync(row=>row.UserId=="execution-recipient");assignedRole.ApprovalStatus="Revoked";await db.SaveChangesAsync();
        await f.Select("execution-recipient","ConfiguredReportingRole");Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.GetAsync("/api/drilling/programs/execution/assigned?processId="+Uri.EscapeDataString(original.InstanceId()))).StatusCode);
        assignedRole.ApprovalStatus="Approved";senderProfile.PersonaCode="DRILLING_SITE";senderGrant.AccessLevel="WRITE";
        foreach(var claim in removed){claim.Id=0;db.RoleClaims.Add(claim);}await db.SaveChangesAsync();await Select("site-recorder","ConfiguredExecutionRole");
        Assert.Equal(original.InstanceId(),(await client.GetExecutionAssignmentAsync(execution.Basis.ReviewInstanceId)).ExistingInstanceId);
        Assert.Equal(before+4,lifecycle.Native.Writes);Assert.Equal(originalExecution,JsonSerializer.Serialize(f.Store.Store.Native.Rows.OfType<DRILLING_OPERATION>()));
    }
}
#endif
