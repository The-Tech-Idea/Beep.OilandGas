using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class DrillingProgramAssignedReviewApiTests
{
    [Fact]public async Task ProtectedTypedRecipientAndInboxRequireOriginalCompanyRoleProfileFieldSourceAndCompleteNativeTask()
    {
        using var f=new DrillingProgramApiTests.Fixture();await f.Prepare();var db=f.Funding.Routing.Afe.Db;foreach(var permission in new[]{"Drilling.View","Drilling.Approve"})db.RoleClaims.Add(new(){RoleId="investment-role",ClaimType="permission",ClaimValue=permission});await db.SaveChangesAsync();await f.Select();
        var definition=CompanyInventorySourceReview.Copy(f.Funding.Routing.Definition);definition.PROCESS_DEFINITION_ID="recorded-program-review";definition.PROCESS_TYPE="DRILLING";definition.ENTITY_TYPE="DRILLING_PROGRAM";definition.PROCESS_NAME="Independent drilling program review";definition.PROCESS_CONFIG_JSON=JsonSerializer.Serialize(new{Steps=new[]{new ProcessStepDefinition{StepId="program-technical",StepName="Independent program technical review",SequenceNumber=1,StepType="APPROVAL",RequiresApproval=true,RequiredRoles=["InvestmentReviewer"]}}});f.Funding.Routing.Catalog.Workflow.Native.Rows.Add(definition);
        var source=await f.Basis();var draft=f.Store.Command with{Basis=source,IntentId=Guid.NewGuid()};var saved=await f.Build().RecordAsync(f.Funding.Routing.Afe.Receiver.User,draft);var routing=await f.Build().ReviewRoutingAsync(f.Funding.Routing.Afe.Receiver.User,f.Funding.Routing.Afe.Process,draft.Basis.Funding.Source.AfeId,f.Well.UWI,saved.Recorded!.PROGRAM_ID,saved.Recorded.PROGRAM_DRAFT_REVISION);var original=new DrillingProgramReviewStart(routing.Source,routing.Options.Single(),Guid.NewGuid());await f.Build().SubmitReviewAsync(f.Funding.Routing.Afe.Receiver.User,original);var native=f.Funding.Routing.Catalog.Workflow.Native;var before=JsonSerializer.Serialize(native.Rows);var writes=native.Writes;var financeWrites=f.Funding.Routing.Afe.Native.Native.Writes;
        await f.Select("investor","InvestmentReviewer");await using var host=await AccountingSourceHttpTests.Start(configure:(services,_)=>
        {
            services.AddScoped<CompanyDrillingProgramService>(p=>f.Build(p.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>()));services.AddScoped<IDrillingProgramAssignedReviewReader>(p=>p.GetRequiredService<CompanyDrillingProgramService>());services.AddSingleton(db);services.AddScoped<RepositoryTenantContext>(_=>f.Funding.Routing.Afe.Receiver.Tenant);services.AddScoped<IRepositoryAccessService>(_=>f.Funding.Routing.Afe.Receiver.Accounts);services.AddScoped<RepositoryPersonaService>(_=>f.Funding.Routing.Afe.Receiver.Workspaces);services.AddScoped<IAccessControlService>(_=>f.Funding.Routing.Afe.Receiver.Resources.Object);services.AddScoped<IProcessService>(_=>f.Funding.Routing.Afe.Receiver.Processes);services.AddScoped<ICrossPersonaTaskRouter>(_=>native.Router);services.AddSingleton(Mock.Of<IFailureReporter>());
        });host.Client.DefaultRequestHeaders.Add("X-Test-Actor","investor");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","InvestmentReviewer");using var company=new Beep.OilandGas.Client.App.CompanyClientContext();void Selection(){var current=f.Funding.Routing.Afe.Receiver.Tenant.Company!;company.TrySelect(company.Clear(),current.UserId,new(current.Scope,current.Roles.ToArray(),current.Permissions.ToArray()));}Selection();var client=new Beep.OilandGas.Web.Services.DrillingServiceClient(new(host.Client,Mock.Of<IFailureReporter>(),company));
        var assigned=await client.GetAssignedProgramReviewAsync(original.InstanceId());Assert.Equal("investor",assigned.Source.Basis.Scope().ActorUserId);Assert.Equal("FINANCE",assigned.PersonaCode);Assert.Equal("accountant",assigned.Original.Scope().ActorUserId);Assert.Equal(saved.Recorded.PROGRAM_ID,assigned.Source.Program.PROGRAM_ID);Assert.Equal("DRAFT",assigned.Source.Program.PROGRAM_STATUS);
        var inbox=await host.Client.GetFromJsonAsync<UnifiedInbox>("/api/workflow/tasks/inbox?personaCode=FINANCE");var task=Assert.Single(inbox!.NormalTasks);Assert.Equal(assigned.TaskId,task.TaskId);Assert.Equal(assigned.Revision,task.ProgramReviewRevision);Assert.Equal("/drilling/program-review/assigned?processId="+Uri.EscapeDataString(original.InstanceId()),task.Route);Assert.Equal(1,inbox.Counts.TotalPending);Assert.Equal(1,inbox.Counts.Approvals);Assert.Equal(assigned.Source.Program.PROGRAM_NAME,task.EntityDescription);
        var path="/api/drilling/programs/review/assigned?processId="+Uri.EscapeDataString(original.InstanceId());
        var approval=await db.RoleClaims.SingleAsync(row=>row.RoleId=="investment-role" && row.ClaimValue=="Drilling.Approve");db.Remove(approval);await db.SaveChangesAsync();await f.Select("investor","InvestmentReviewer");Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.GetAsync(path)).StatusCode);approval.Id=0;db.RoleClaims.Add(approval);await db.SaveChangesAsync();
        var profile=await db.Set<AppUserPersona>().SingleAsync(row=>row.UserId=="investor");profile.PersonaCode="FOREIGN_REVIEWER";await db.SaveChangesAsync();await f.Select("investor","InvestmentReviewer");Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.GetAsync(path)).StatusCode);profile.PersonaCode="FINANCE";await db.SaveChangesAsync();
        var grant=await db.Set<AppUserAssetAccess>().SingleAsync(row=>row.UserId=="investor");grant.IsActive=false;await db.SaveChangesAsync();await f.Select("investor","InvestmentReviewer");Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.GetAsync(path)).StatusCode);grant.IsActive=true;await db.SaveChangesAsync();await f.Select("investor","InvestmentReviewer");
        f.WellAccess=false;Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.GetAsync(path)).StatusCode);f.WellAccess=true;
        var recordedTask=native.Rows.OfType<CROSS_PERSONA_TASK>().Single(row=>row.CROSS_TASK_ID==assigned.TaskId);recordedTask.ENTITY_TYPE="WELL";Assert.Equal(HttpStatusCode.Conflict,(await host.Client.GetAsync("/api/workflow/tasks/inbox?personaCode=FINANCE")).StatusCode);native.Rows.OfType<CROSS_PERSONA_TASK>().Single(row=>row.CROSS_TASK_ID==assigned.TaskId).ENTITY_TYPE="DRILLING_PROGRAM";
        var binding=await db.Set<ModuleDatabaseBinding>().SingleAsync(row=>row.ModuleId=="DRILLING_EXECUTION");var stamp=binding.ConcurrencyStamp;binding.ConcurrencyStamp="changed-execution";await db.SaveChangesAsync();await f.Select("investor","InvestmentReviewer");Assert.Equal(HttpStatusCode.Conflict,(await host.Client.GetAsync(path)).StatusCode);binding.ConcurrencyStamp=stamp;await db.SaveChangesAsync();
        foreach(var permission in new[]{"Drilling.View","Drilling.Approve","Accounting.View"})db.RoleClaims.Add(new(){RoleId="review-role",ClaimType="permission",ClaimValue=permission});db.RoleClaims.Add(new(){RoleId="review-role",ClaimType=RepositoryPersonaService.WorkspaceClaimType,ClaimValue="FOREIGN_REVIEWER"});db.Add(new AppUserAssetAccess{Id="foreign-program-field",UserId="foreign-reviewer",DatabaseScope=original.Scope().CoreDatabase.Fingerprint,OrganizationId="foreign",AssetType="FIELD",AssetId=original.Scope().FieldId,AccessLevel="READ",IsActive=true});await db.SaveChangesAsync();await f.Select("foreign-reviewer","ConfiguredReviewRole",new("tenant","foreign"));host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Remove("X-Test-Assigned-Role");host.Client.DefaultRequestHeaders.Add("X-Test-Actor","foreign-reviewer");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","ConfiguredReviewRole");Selection();var denied=await host.Client.GetAsync(path);Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);Assert.DoesNotContain(original.Source.Program.PROGRAM_NAME,await denied.Content.ReadAsStringAsync());
        host.Client.DefaultRequestHeaders.Remove("X-Test-Auth");Assert.Equal(HttpStatusCode.Unauthorized,(await host.Client.GetAsync(path)).StatusCode);Assert.Equal(before,JsonSerializer.Serialize(native.Rows));Assert.Equal(writes,native.Writes);Assert.Equal(financeWrites,f.Funding.Routing.Afe.Native.Native.Writes);Assert.Equal(1,f.Store.Store.Native.Writes);Assert.Null(f.Well.SPUD_DATE);
    }
}
#endif
