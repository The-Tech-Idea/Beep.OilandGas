#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class WorkOrderMaterialUsageStateTests
{
    [Fact]
    public async Task RevokedInventoryAuthorityHidesAnUncertainOriginalBodyUntilTheSameActorRegainsAuthority()
    {
        var command=Beep.OilandGas.ApiService.Tests.WorkOrderExecutionStartFixture.Usage();var context=command.Review.Material.Source.Context;
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,["ConfiguredRole"],["Maintenance.ExecuteWork","Inventory.View"])));
        var start=new CompanyWorkOrderExecutionStartReference(command.Review.Start.Permit,command.Review.Start.IntentId);
        using var state=new WorkOrderMaterialUsageState(company);state.Attach(start);var client=new Mock<ICompanyMaintenanceHandoffClient>();
        client.Setup(value=>value.ReviewWorkOrderMaterialUsageAsync(start,It.IsAny<CancellationToken>())).ReturnsAsync(command.Review);
        client.Setup(value=>value.RecordWorkOrderMaterialUsageAsync(It.IsAny<CompanyWorkOrderMaterialUsageCommand>(),It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Unknown original issue outcome"));
        await state.ReviewAsync(client.Object);
        await Assert.ThrowsAsync<HttpRequestException>(()=>state.RecordAsync(client.Object,1m,command.Unit,DateTime.UtcNow,"Actual use",_=>Task.CompletedTask));
        var id=state.VisibleReference!.Value;var body=JsonSerializer.Serialize(state.VisibleCommand);
        Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,["ConfiguredRole"],["Maintenance.ExecuteWork"])));state.Attach(start);
        Assert.Null(state.VisibleCommand);Assert.Null(state.VisibleReference);Assert.False(state.CanReview);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>state.RecoverAsync(client.Object,id));
        Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,["ConfiguredRole"],["Maintenance.ExecuteWork","Inventory.View"])));state.Attach(start);
        Assert.Equal(id,state.VisibleReference);Assert.Equal(body,JsonSerializer.Serialize(state.VisibleCommand));
        client.Verify(value=>value.RecordWorkOrderMaterialUsageAsync(It.IsAny<CompanyWorkOrderMaterialUsageCommand>(),It.IsAny<CancellationToken>()),Times.Once);
    }
    [Theory][InlineData("confirmed")][InlineData("unknown")][InlineData("before-transport")][InlineData("late-company")]
    public async Task PhysicalUsageRetainsOriginalBodyBeforeTransportAndRecoversWithoutAnotherIssue(string mode)
    {
        var command=Beep.OilandGas.ApiService.Tests.WorkOrderExecutionStartFixture.Usage();var context=command.Review.Material.Source.Context;
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,["ConfiguredRole"],["Maintenance.ExecuteWork","Inventory.View"])));
        var start=new CompanyWorkOrderExecutionStartReference(command.Review.Start.Permit,command.Review.Start.IntentId);
        using var state=new WorkOrderMaterialUsageState(company);state.Attach(start);var client=new Mock<ICompanyMaintenanceHandoffClient>();
        var retained=false;var writes=0;CompanyWorkOrderMaterialUsageRecovery? recorded=null;
        client.Setup(value=>value.ReviewWorkOrderMaterialUsageAsync(start,It.IsAny<CancellationToken>())).ReturnsAsync(command.Review);
        client.Setup(value=>value.RecordWorkOrderMaterialUsageAsync(It.IsAny<CompanyWorkOrderMaterialUsageCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderMaterialUsageCommand sent,CancellationToken _)=>
        {
            Assert.True(retained);Assert.Equal(sent.IntentId,state.VisibleReference);writes++;
            var utc=DateTime.UtcNow;recorded=new(sent,sent.Prepare(utc),sent.PrepareMovement(utc));
            if(mode=="unknown")throw new InvalidOperationException("Unknown stock commit");if(mode=="late-company")company.Clear();return Task.FromResult(recorded);
        });
        client.Setup(value=>value.RecoverWorkOrderMaterialUsageAsync(It.IsAny<CompanyWorkOrderMaterialUsageReference>(),It.IsAny<CancellationToken>()))
            .Returns((CompanyWorkOrderMaterialUsageReference reference,CancellationToken _)=>
            {if(recorded is null)throw new InvalidOperationException("Not recorded");recorded.Require(context.Company,context.ActorUserId,reference);return Task.FromResult(recorded);});
        await state.ReviewAsync(client.Object);Assert.True(state.CanRecord);
        var error=await Record.ExceptionAsync(()=>state.RecordAsync(client.Object,1m,command.Unit,DateTime.UtcNow,"Physical use",id=>
        {
            retained=true;Assert.Equal(id,state.VisibleReference);Assert.NotNull(state.VisibleCommand);
            if(mode=="before-transport")throw new InvalidOperationException("Route retention failed");return Task.CompletedTask;
        }));
        if(mode is "unknown" or "before-transport")Assert.NotNull(error);else Assert.Null(error);
        Assert.Equal(mode=="before-transport"?0:1,writes);
        if(mode=="late-company")
        {
            Assert.Null(state.Receipt);Assert.Null(state.VisibleCommand);
            Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,["ConfiguredRole"],["Maintenance.ExecuteWork","Inventory.View"])));state.Attach(start);
        }
        var id=state.VisibleReference!.Value;var body=JsonSerializer.Serialize(state.VisibleCommand);
        state.VisibleCommand!.Review.Material.Source.Inventory.Item.QUANTITY_ON_HAND=999;
        Assert.Equal(body,JsonSerializer.Serialize(state.VisibleCommand));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordAsync(client.Object,1m,command.Unit,DateTime.UtcNow,"Replacement",_=>Task.CompletedTask));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecoverAsync(client.Object,Guid.NewGuid()));
        if(mode=="before-transport")await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecoverAsync(client.Object,id));
        else{await state.RecoverAsync(client.Object,id);Assert.NotNull(state.Receipt);Assert.Equal(body,JsonSerializer.Serialize(state.VisibleCommand));}
        Assert.Equal(mode=="before-transport"?0:1,writes);
        if(mode is "unknown" or "before-transport")
        {
            if(mode=="before-transport")Assert.False(state.CanReview);
        }
    }

    private sealed class Response(object payload,Action changed):HttpMessageHandler
    {
        internal int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)
        {Calls++;changed();return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=JsonContent.Create(payload,payload.GetType())});}
    }
    [Theory][InlineData("review","current")][InlineData("record","current")][InlineData("recover","current")]
    [InlineData("review","permission")][InlineData("record","permission")][InlineData("recover","permission")]
    [InlineData("review","late-company")][InlineData("record","late-company")][InlineData("recover","late-company")]
    public async Task ActualTypedPhysicalUsageClientChecksAuthorityAndFencesLateResponses(string action,string mode)
    {
        var command=Beep.OilandGas.ApiService.Tests.WorkOrderExecutionStartFixture.Usage();var context=command.Review.Material.Source.Context;
        var start=new CompanyWorkOrderExecutionStartReference(command.Review.Start.Permit,command.Review.Start.IntentId);var utc=DateTime.UtcNow;
        var recovery=new CompanyWorkOrderMaterialUsageRecovery(command,command.Prepare(utc),command.PrepareMovement(utc));
        object payload=action=="review"?command.Review:recovery;
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,["ConfiguredRole"],mode=="permission"?["Maintenance.ExecuteWork"]:["Maintenance.ExecuteWork","Inventory.View"])));
        var response=new Response(payload,()=>{if(mode=="late-company")company.Clear();});using var http=new HttpClient(response){BaseAddress=new("https://fixture.invalid/")};
        var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var error=await Record.ExceptionAsync(async()=>
        {
            if(action=="review")(await client.ReviewWorkOrderMaterialUsageAsync(start)).Require();
            else if(action=="record")(await client.RecordWorkOrderMaterialUsageAsync(command)).Require(context.Company,context.ActorUserId,new(start,command.IntentId));
            else(await client.RecoverWorkOrderMaterialUsageAsync(new(start,command.IntentId))).Require(context.Company,context.ActorUserId,new(start,command.IntentId));
        });
        if(mode=="current")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission"?0:1,response.Calls);
    }
}
#endif
