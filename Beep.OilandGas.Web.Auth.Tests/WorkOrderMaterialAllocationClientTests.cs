using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ProductionOperations.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderMaterialAllocationClientTests
{
    private sealed class Transport(CompanyWorkOrderMaterialAllocationCommand expected,string mode,Action change):HttpMessageHandler
    {
        public int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Calls++;Assert.EndsWith(mode=="recovery"?"/materials/allocations/recovery":"/materials/allocations",request.RequestUri!.AbsolutePath);var actual=await request.Content!.ReadFromJsonAsync<CompanyWorkOrderMaterialAllocationCommand>(token);Assert.Equal(JsonSerializer.Serialize(expected),JsonSerializer.Serialize(actual));
            var row=expected.Prepare(expected.Review.ComparedAtUtc);if(mode=="forged")row.QUANTITY=999m;if(mode=="late-company")change();return new(HttpStatusCode.OK){Content=JsonContent.Create(row)};
        }
    }
    [Theory][InlineData("record")][InlineData("recovery")][InlineData("permission")][InlineData("forged")][InlineData("late-company")]
    public async Task FullClientUsesExactOriginalReservationBodyAndFencesReturnedEvidence(string mode)
    {
        var source=WorkOrderMaterialSourceTests.Source();var utc=DateTime.UtcNow;var review=CompanyWorkOrderMaterialAllocationReview.Create(source,[],utc);var command=new CompanyWorkOrderMaterialAllocationCommand(review,Guid.NewGuid(),utc,utc.AddHours(2),"Original material requirement");
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"issuer",new(source.Context.Company,["ConfiguredRole"],mode=="permission"?["Inventory.View"]:["Inventory.View","Inventory.ReserveWorkOrderMaterial"])));
        var transport=new Transport(command,mode,()=>company.Clear());using var http=new HttpClient(transport){BaseAddress=new("https://fixture.invalid/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var error=await Record.ExceptionAsync(async()=>{var row=mode=="recovery"?await client.RecoverWorkOrderMaterialAllocationAsync(command):await client.RecordWorkOrderMaterialAllocationAsync(command);command.RequireRecorded(row);});
        if(mode is "record" or "recovery")Assert.Null(error);else Assert.True(error is UnauthorizedAccessException or InvalidOperationException or OperationCanceledException,error?.ToString());Assert.Equal(mode=="permission"?0:1,transport.Calls);
    }
    private sealed class ReferenceTransport(CompanyWorkOrderMaterialAllocationCommand expected,string mode,Action change):HttpMessageHandler
    {
        public int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Calls++;Assert.EndsWith("/materials/allocations/recovery/reference",request.RequestUri!.AbsolutePath);var reference=(await request.Content!.ReadFromJsonAsync<CompanyWorkOrderMaterialAllocationReferenceQuery>(token))!;Assert.Equal(expected.IntentId,reference.IntentId);Assert.Equal(expected.Review.Source.Query.Task,reference.Task);
            var row=expected.Prepare(expected.Review.ComparedAtUtc);if(mode=="forged")row.QUANTITY=999;if(mode=="late-company")change();return new(HttpStatusCode.OK){Content=JsonContent.Create(new CompanyWorkOrderMaterialAllocationRecovery(expected,row))};
        }
    }
    [Theory][InlineData("original")][InlineData("permission")][InlineData("forged")][InlineData("late-company")]
    public async Task FullReferenceClientVerifiesExactRecoveredPrivateBodyAndCurrentCompany(string mode)
    {
        var source=WorkOrderMaterialSourceTests.Source();var utc=DateTime.UtcNow;var review=CompanyWorkOrderMaterialAllocationReview.Create(source,[],utc);var command=new CompanyWorkOrderMaterialAllocationCommand(review,Guid.NewGuid(),utc,utc.AddHours(2),"Original requirement");using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"issuer",new(source.Context.Company,["ConfiguredRole"],mode=="permission"?["Inventory.View"]:["Inventory.View","Inventory.ReserveWorkOrderMaterial"])));
        var transport=new ReferenceTransport(command,mode,()=>company.Clear());using var http=new HttpClient(transport){BaseAddress=new("https://fixture.invalid/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);var query=new CompanyWorkOrderMaterialAllocationReferenceQuery(source.Query.Task,command.IntentId);
        var error=await Record.ExceptionAsync(async()=>{var result=await client.RecoverWorkOrderMaterialAllocationReferenceAsync(query);result.Require(source.Context.Company,"issuer",query);});if(mode=="original")Assert.Null(error);else Assert.True(error is UnauthorizedAccessException or InvalidOperationException or OperationCanceledException,error?.ToString());Assert.Equal(mode=="permission"?0:1,transport.Calls);
    }
    private sealed class ReleaseTransport(CompanyWorkOrderMaterialReleaseCommand expected,string mode,Action change):HttpMessageHandler
    {
        public int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Calls++;Assert.EndsWith(mode=="recovery"?"/release/recovery":"/release",request.RequestUri!.AbsolutePath);var body=await request.Content!.ReadFromJsonAsync<CompanyWorkOrderMaterialReleaseCommand>(token);Assert.Equal(JsonSerializer.Serialize(expected),JsonSerializer.Serialize(body));var row=expected.Prepare(expected.Original.Recorded.ROW_CREATED_DATE!.Value);if(mode=="forged")row.RELEASED_QUANTITY=999;if(mode=="late-company")change();return new(HttpStatusCode.OK){Content=JsonContent.Create(row)};
        }
    }
    [Theory][InlineData("original")][InlineData("recovery")][InlineData("permission")][InlineData("forged")][InlineData("late-company")]
    public async Task FullReleaseClientSendsExactBodyAndFencesReturnedEvidence(string mode)
    {
        var source=WorkOrderMaterialSourceTests.Source();var utc=DateTime.UtcNow;var review=CompanyWorkOrderMaterialAllocationReview.Create(source,[],utc);var allocation=new CompanyWorkOrderMaterialAllocationCommand(review,Guid.NewGuid(),utc,utc.AddHours(1),"Original requirement");var release=new CompanyWorkOrderMaterialReleaseCommand(new(allocation,allocation.Prepare(utc)),Guid.NewGuid(),"Cancelled original");using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"issuer",new(source.Context.Company,["ConfiguredRole"],mode=="permission"?["Inventory.View"]:["Inventory.View","Inventory.ReleaseWorkOrderMaterial"])));var transport=new ReleaseTransport(release,mode,()=>company.Clear());using var http=new HttpClient(transport){BaseAddress=new("https://fixture.invalid/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var error=await Record.ExceptionAsync(async()=>{var row=mode=="recovery"?await client.RecoverWorkOrderMaterialReleaseAsync(release):await client.ReleaseWorkOrderMaterialAsync(release);release.RequireRecorded(row);});if(mode is "original" or "recovery")Assert.Null(error);else Assert.True(error is UnauthorizedAccessException or InvalidOperationException or OperationCanceledException,error?.ToString());Assert.Equal(mode=="permission"?0:1,transport.Calls);
    }
    private sealed class ReleaseReferenceTransport(CompanyWorkOrderMaterialReleaseCommand expected,string mode,Action change):HttpMessageHandler
    {
        public int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Calls++;Assert.EndsWith("/release/recovery/reference",request.RequestUri!.AbsolutePath);var query=(await request.Content!.ReadFromJsonAsync<CompanyWorkOrderMaterialReleaseReferenceQuery>(token))!;Assert.Equal(expected.IntentId,query.IntentId);Assert.Equal(expected.Original.Command.IntentId,query.Allocation.IntentId);var row=expected.Prepare(expected.Original.Recorded.ROW_CREATED_DATE!.Value);if(mode=="forged")row.RELEASED_QUANTITY=999;if(mode=="late-company")change();return new(HttpStatusCode.OK){Content=JsonContent.Create(new CompanyWorkOrderMaterialReleaseRecovery(expected,row))};
        }
    }
    [Theory][InlineData("original")][InlineData("permission")][InlineData("forged")][InlineData("late-company")]
    public async Task FullReleaseReferenceClientRecoversOnlyExactOriginalEvidence(string mode)
    {
        var source=WorkOrderMaterialSourceTests.Source();var utc=DateTime.UtcNow;var review=CompanyWorkOrderMaterialAllocationReview.Create(source,[],utc);var allocation=new CompanyWorkOrderMaterialAllocationCommand(review,Guid.NewGuid(),utc,utc.AddHours(1),"Original requirement");var release=new CompanyWorkOrderMaterialReleaseCommand(new(allocation,allocation.Prepare(utc)),Guid.NewGuid(),"Cancelled original");using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"issuer",new(source.Context.Company,["ConfiguredRole"],mode=="permission"?["Inventory.View"]:["Inventory.View","Inventory.ReleaseWorkOrderMaterial"])));var transport=new ReleaseReferenceTransport(release,mode,()=>company.Clear());using var http=new HttpClient(transport){BaseAddress=new("https://fixture.invalid/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);var query=new CompanyWorkOrderMaterialReleaseReferenceQuery(new(source.Query.Task,allocation.IntentId),release.IntentId);
        var error=await Record.ExceptionAsync(async()=>{var result=await client.RecoverWorkOrderMaterialReleaseReferenceAsync(query);result.Require(source.Context.Company,"issuer",query);});if(mode=="original")Assert.Null(error);else Assert.True(error is UnauthorizedAccessException or InvalidOperationException or OperationCanceledException,error?.ToString());Assert.Equal(mode=="permission"?0:1,transport.Calls);
    }
}
#endif