#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderMaterialAllocationStateTests
{
    [Theory][InlineData("confirmed")][InlineData("unknown")][InlineData("before-transport")][InlineData("late-company")]
    public async Task OriginalCommandIsRetainedBeforeTransportAndUncertaintyBlocksReplacement(string mode)
    {
        var source=WorkOrderMaterialSourceTests.Source();var utc=DateTime.UtcNow;var review=CompanyWorkOrderMaterialAllocationReview.Create(source,[],utc);using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"issuer",new(source.Context.Company,["ConfiguredRole"],["Inventory.View","Inventory.ReserveWorkOrderMaterial"])));using var state=new WorkOrderMaterialAllocationState(company);state.Attach(source.Query.Task);
        var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(c=>c.ReviewWorkOrderMaterialAllocationAsync(source.Query,It.IsAny<CancellationToken>())).ReturnsAsync(review);await state.ReviewAsync(client.Object,source.Query);
        CompanyWorkOrderMaterialAllocationCommand? retained=null;var writes=0;
        client.Setup(c=>c.RecordWorkOrderMaterialAllocationAsync(It.IsAny<CompanyWorkOrderMaterialAllocationCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderMaterialAllocationCommand command,CancellationToken _)=>
        {
            Assert.Equal(retained!.IntentId,command.IntentId);Assert.Equal(command.IntentId,state.VisibleCommand!.IntentId);writes++;if(mode=="late-company")company.Clear();return mode=="unknown"?Task.FromException<WORK_ORDER_MATERIAL_ALLOCATION>(new HttpRequestException("Unknown commit")):Task.FromResult(command.Prepare(utc));
        });
        var error=await Record.ExceptionAsync(()=>state.ReserveAsync(client.Object,utc,utc.AddHours(1),"Original requirement",command=>{retained=command;if(mode=="before-transport")throw new InvalidOperationException("Retention unavailable");return Task.CompletedTask;}));
        Assert.NotNull(retained);Assert.Equal(mode=="before-transport"?0:1,writes);Assert.False(state.CanReserve);Assert.Equal(mode is "unknown" or "before-transport",error is not null);if(mode=="late-company"){Assert.Null(state.Receipt);Assert.Null(state.VisibleCommand);return;}
        Assert.NotNull(state.VisibleCommand);await Assert.ThrowsAsync<InvalidOperationException>(()=>state.ReviewAsync(client.Object,source.Query));
        client.Setup(c=>c.RecoverWorkOrderMaterialAllocationAsync(It.IsAny<CompanyWorkOrderMaterialAllocationCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderMaterialAllocationCommand command,CancellationToken _)=>Task.FromResult(command.Prepare(utc)));await state.RecoverAsync(client.Object);Assert.NotNull(state.Receipt);Assert.Equal(mode=="before-transport"?0:1,writes);
    }
    [Theory][InlineData("original")][InlineData("unknown")][InlineData("body")][InlineData("late-company")]
    public async Task FreshCircuitRecoversOriginalReferenceWithoutReservationCommand(string mode)
    {
        var source=WorkOrderMaterialSourceTests.Source();var utc=DateTime.UtcNow;var review=CompanyWorkOrderMaterialAllocationReview.Create(source,[],utc);var command=new CompanyWorkOrderMaterialAllocationCommand(review,Guid.NewGuid(),utc,utc.AddHours(2),"Original requirement");using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"issuer",new(source.Context.Company,["ConfiguredRole"],["Inventory.View","Inventory.ReserveWorkOrderMaterial"])));using var state=new WorkOrderMaterialAllocationState(company);state.Attach(source.Query.Task);
        var client=new Mock<ICompanyMaintenanceHandoffClient>();var reads=0;client.Setup(c=>c.RecoverWorkOrderMaterialAllocationReferenceAsync(It.IsAny<CompanyWorkOrderMaterialAllocationReferenceQuery>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderMaterialAllocationReferenceQuery query,CancellationToken _)=>
        {reads++;Assert.Equal(command.IntentId,query.IntentId);if(mode=="late-company")company.Clear();var row=command.Prepare(utc);if(mode=="body")row.QUANTITY=999;return mode=="unknown"?Task.FromException<CompanyWorkOrderMaterialAllocationRecovery>(new HttpRequestException("Unconfirmed recovery")):Task.FromResult(new CompanyWorkOrderMaterialAllocationRecovery(command,row));});
        var error=await Record.ExceptionAsync(()=>state.RecoverReferenceAsync(client.Object,command.IntentId));Assert.Equal(1,reads);Assert.False(state.CanReserve);if(mode=="original"){Assert.Null(error);Assert.NotNull(state.Receipt);Assert.Equal(command.IntentId,state.VisibleCommand!.IntentId);}else Assert.Null(state.Receipt);
        client.Verify(c=>c.RecordWorkOrderMaterialAllocationAsync(It.IsAny<CompanyWorkOrderMaterialAllocationCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    [Theory][InlineData("original")][InlineData("unknown")][InlineData("permission")][InlineData("late-company")]
    public async Task ReleaseRetainsOriginalIntentBeforeTransportAndReconcilesReadOnly(string mode)
    {
        var source=WorkOrderMaterialSourceTests.Source();var utc=DateTime.UtcNow;var review=CompanyWorkOrderMaterialAllocationReview.Create(source,[],utc);var allocation=new CompanyWorkOrderMaterialAllocationCommand(review,Guid.NewGuid(),utc,utc.AddHours(2),"Original requirement");using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"issuer",new(source.Context.Company,["ConfiguredRole"],mode=="permission"?["Inventory.View","Inventory.ReserveWorkOrderMaterial"]:["Inventory.View","Inventory.ReserveWorkOrderMaterial","Inventory.ReleaseWorkOrderMaterial"])));using var state=new WorkOrderMaterialAllocationState(company);state.Attach(source.Query.Task);
        var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(c=>c.RecoverWorkOrderMaterialAllocationReferenceAsync(It.IsAny<CompanyWorkOrderMaterialAllocationReferenceQuery>(),It.IsAny<CancellationToken>())).ReturnsAsync(new CompanyWorkOrderMaterialAllocationRecovery(allocation,allocation.Prepare(utc)));await state.RecoverReferenceAsync(client.Object,allocation.IntentId);
        Guid retained=Guid.Empty;CompanyWorkOrderMaterialReleaseCommand? original=null;var writes=0;client.Setup(c=>c.ReleaseWorkOrderMaterialAsync(It.IsAny<CompanyWorkOrderMaterialReleaseCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderMaterialReleaseCommand command,CancellationToken _)=>{Assert.Equal(retained,command.IntentId);Assert.Equal(retained,state.VisibleReleaseReference);original=command;writes++;if(mode=="late-company")company.Clear();return mode=="unknown"?Task.FromException<WORK_ORDER_MATERIAL_RELEASE>(new HttpRequestException("Unknown release commit")):Task.FromResult(command.Prepare(utc));});
        var error=await Record.ExceptionAsync(()=>state.ReleaseAsync(client.Object,"Cancelled original requirement",id=>{retained=id;return Task.CompletedTask;}));Assert.Equal(mode=="permission"?0:1,writes);
        if(mode=="permission"){Assert.IsType<UnauthorizedAccessException>(error);return;}Assert.NotNull(original);Assert.False(state.CanRelease);if(mode=="late-company"){Assert.Null(state.ReleaseReceipt);return;}
        client.Setup(c=>c.RecoverWorkOrderMaterialReleaseReferenceAsync(It.IsAny<CompanyWorkOrderMaterialReleaseReferenceQuery>(),It.IsAny<CancellationToken>())).ReturnsAsync(new CompanyWorkOrderMaterialReleaseRecovery(original!,original!.Prepare(utc)));await state.RecoverReleaseReferenceAsync(client.Object,retained);Assert.NotNull(state.ReleaseReceipt);Assert.Equal(1,writes);state.BeginNewRequirement();Assert.Null(state.VisibleReference);Assert.Null(state.VisibleReleaseReference);Assert.Null(state.Receipt);Assert.Null(state.ReleaseReceipt);Assert.Equal(1,writes);
    }
}
#endif