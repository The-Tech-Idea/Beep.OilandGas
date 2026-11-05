using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Processes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class DrillingProgramReviewStartApiTests
{
    [Fact]public async Task ProtectedTypedSubmissionRecordsOneOriginalAssignmentAndReadOnlyRecoverySurvivesRevokedStartAndRecipients()
    {
        using var f=new DrillingProgramApiTests.Fixture();await f.Prepare();var db=f.Funding.Routing.Afe.Db;foreach(var permission in new[]{"Drilling.View","Drilling.Approve"})db.RoleClaims.Add(new(){RoleId="investment-role",ClaimType="permission",ClaimValue=permission});await db.SaveChangesAsync();await f.Select();
        var definition=CompanyInventorySourceReview.Copy(f.Funding.Routing.Definition);definition.PROCESS_DEFINITION_ID="recorded-drilling-review";definition.PROCESS_TYPE="DRILLING";definition.ENTITY_TYPE="DRILLING_PROGRAM";definition.PROCESS_NAME="Recorded independent drilling review";definition.PROCESS_CONFIG_JSON=JsonSerializer.Serialize(new{Steps=new[]{new ProcessStepDefinition{StepId="program-technical",StepName="Independent drilling program review",SequenceNumber=1,StepType="APPROVAL",RequiresApproval=true,RequiredRoles=["InvestmentReviewer"],SlaHours=24}}});f.Funding.Routing.Catalog.Workflow.Native.Rows.Add(definition);
        var basis=await f.Basis();var draft=f.Store.Command with{Basis=basis,IntentId=Guid.NewGuid()};var saved=await f.Build().RecordAsync(f.Funding.Routing.Afe.Receiver.User,draft);var financeWrites=f.Funding.Routing.Afe.Native.Native.Writes;var workflowWrites=f.Funding.Routing.Catalog.Workflow.Native.Writes;
        await using var host=await AccountingSourceHttpTests.Start(configure:(services,_)=>{services.AddScoped<CompanyDrillingProgramService>(p=>f.Build(p.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>()));services.AddSingleton(Mock.Of<IFailureReporter>());});host.Client.DefaultRequestHeaders.Add("X-Test-Actor","accountant");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","FinanceRole");using var company=new Beep.OilandGas.Client.App.CompanyClientContext();var current=f.Funding.Routing.Afe.Receiver.Tenant.Company!;company.TrySelect(company.Clear(),"accountant",new(current.Scope,current.Roles.ToArray(),current.Permissions.ToArray()));var client=new Beep.OilandGas.Web.Services.DrillingServiceClient(new(host.Client,Mock.Of<IFailureReporter>(),company));
        var row=saved.Recorded!;Task<DrillingProgramReviewReadiness> Review()=>client.ReviewProgramRoutingAsync(f.Funding.Routing.Afe.Process,draft.Basis.Funding.Source.AfeId,f.Well.UWI,row.PROGRAM_ID,row.PROGRAM_DRAFT_REVISION);var routing=await Review();Assert.True(Assert.Single(routing.Options).Ready);var original=new DrillingProgramReviewStart(routing.Source,routing.Options[0],Guid.NewGuid());
        var receipt=await client.SubmitProgramReviewAsync(original);receipt.Require(original);Assert.True(receipt.Recorded);Assert.Single(receipt.TaskIds);Assert.Equal(workflowWrites+4,f.Funding.Routing.Catalog.Workflow.Native.Writes);Assert.Equal("DRAFT",row.PROGRAM_STATUS);Assert.Null(f.Well.SPUD_DATE);Assert.Equal(financeWrites,f.Funding.Routing.Afe.Native.Native.Writes);
        var reloaded=await Review();Assert.Equal(receipt.InstanceId,reloaded.ExistingReviewInstanceId);Assert.Empty(reloaded.Options);
        foreach(var claim in await db.RoleClaims.Where(claim=>claim.RoleId=="finance-role" && (claim.ClaimValue=="Drilling.Create" || claim.ClaimValue=="Workflow.Start") || claim.RoleId=="investment-role" && claim.ClaimValue=="Drilling.Approve").ToArrayAsync())db.Remove(claim);(await db.Set<AppUserAssetAccess>().SingleAsync(grant=>grant.UserId=="accountant")).AccessLevel="READ";await db.SaveChangesAsync();await f.Select();f.Funding.Routing.Catalog.Workflow.Native.Rows.Remove(definition);
        Assert.True((await client.RecoverProgramReviewAsync(original)).Recorded);Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.PostAsJsonAsync("/api/drilling/programs/review/start",original)).StatusCode);Assert.Equal(workflowWrites+4,f.Funding.Routing.Catalog.Workflow.Native.Writes);Assert.Equal(1,f.Store.Store.Native.Writes);Assert.Equal(financeWrites,f.Funding.Routing.Afe.Native.Native.Writes);Assert.Null(f.Well.SPUD_DATE);
        host.Client.DefaultRequestHeaders.Remove("X-Test-Auth");Assert.Equal(HttpStatusCode.Unauthorized,(await host.Client.PostAsJsonAsync("/api/drilling/programs/review/recover",original)).StatusCode);
    }
}
#endif
