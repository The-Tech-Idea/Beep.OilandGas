#if WORKFLOW_INBOX_FOCUSED
extern alias actualData;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
using Row=actualData::Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderMaterialAcceptanceClientTests
{
    private sealed class Transport(WorkOrderMaterialAcceptanceFixture f,string mode,Action change):HttpMessageHandler
    {
        public int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Calls++;Assert.EndsWith("/material-acceptance"+(mode=="recovery"?"/recovery":""),request.RequestUri!.AbsolutePath);var command=(await request.Content!.ReadFromJsonAsync<CompanyWorkOrderMaterialAcceptanceCommand>(token))!;Assert.Equal(JsonSerializer.Serialize(f.Command),JsonSerializer.Serialize(command));command.Require();var row=command.Prepare(f.Pending,DateTime.UtcNow);if(mode=="forged")row.RESULT_NOTES="damaged";if(mode=="late")change();if(mode=="unknown")return new(HttpStatusCode.ServiceUnavailable){Content=new StringContent("Unconfirmed outcome")};return new(HttpStatusCode.OK){Content=JsonContent.Create(row)};
        }
    }
    [Theory][InlineData("record")][InlineData("recovery")][InlineData("forged")][InlineData("late")][InlineData("permission")][InlineData("actor")][InlineData("role")][InlineData("unknown")][InlineData("versioned")]
    public async Task FullMaterialAcceptanceClientKeepsOriginalCompleteBodyAndRejectsChangedOwnerRoleOrOutcome(string mode)
    {
        var f=new WorkOrderMaterialAcceptanceFixture(requirementProof:mode=="versioned");using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),mode=="actor"?"other":"independent",new(f.Permit.Context.Company,mode=="role"?["OtherRole"]:["ConfiguredRole"],mode=="permission"?["HSE.ApprovePermit"]:["HSE.ApprovePermit","Inventory.View"])));var transport=new Transport(f,mode,()=>company.Clear());using var http=new HttpClient(transport){BaseAddress=new("https://material.test/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var error=await Record.ExceptionAsync(async()=>{Row row=mode=="recovery"?await client.RecoverWorkOrderMaterialAcceptanceAsync(f.Command):await client.RecordWorkOrderMaterialAcceptanceAsync(f.Command);f.Command.RequireRecorded(row);});if(mode is "record" or "recovery" or "versioned")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode is "permission" or "actor" or "role"?0:1,transport.Calls);f.Command.Require();
    }
    private sealed class ReferenceTransport(WorkOrderMaterialAcceptanceFixture f,string mode,Action change):HttpMessageHandler
    {
        public int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Calls++;Assert.EndsWith("/material-acceptance/recovery/reference",request.RequestUri!.AbsolutePath);var reference=(await request.Content!.ReadFromJsonAsync<CompanyWorkOrderMaterialAcceptanceReference>(token))!;Assert.Equal(f.Command.IntentId,reference.IntentId);var row=f.Command.Prepare(f.Pending,DateTime.UtcNow);if(mode=="forged")row.RESULT_NOTES="changed";if(mode=="late")change();return new(HttpStatusCode.OK){Content=JsonContent.Create(new CompanyWorkOrderMaterialAcceptanceRecovery(f.Command,row))};
        }
    }
    [Theory][InlineData("original")][InlineData("forged")][InlineData("late")][InlineData("permission")][InlineData("actor")]
    public async Task ReferenceClientRestoresOnlyExactOwnedCommandAndRejectsLateOrDamagedEvidence(string mode)
    {
        var f=new WorkOrderMaterialAcceptanceFixture();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),mode=="actor"?"other":"independent",new(f.Permit.Context.Company,["ConfiguredRole"],mode=="permission"?["HSE.ApprovePermit"]:["HSE.ApprovePermit","Inventory.View"])));var transport=new ReferenceTransport(f,mode,()=>company.Clear());using var http=new HttpClient(transport){BaseAddress=new("https://material.test/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);var reference=new CompanyWorkOrderMaterialAcceptanceReference(f.Command.Review.Query.Permit,f.Command.InspectionId,f.Command.IntentId);
        var error=await Record.ExceptionAsync(async()=>{var result=await client.RecoverWorkOrderMaterialAcceptanceReferenceAsync(reference);result.Require(f.Permit.Context.Company,"independent",reference);Assert.Equal(JsonSerializer.Serialize(f.Command),JsonSerializer.Serialize(result.Command));});if(mode=="original")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission"?0:1,transport.Calls);
    }
}
#endif
