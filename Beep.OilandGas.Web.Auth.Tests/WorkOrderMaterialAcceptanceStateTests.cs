#if WORKFLOW_INBOX_FOCUSED
extern alias actualData;
using System.Text.Json;
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;
using Row=actualData::Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderMaterialAcceptanceStateTests
{
    [Theory][InlineData("record")][InlineData("unknown")][InlineData("retention")][InlineData("late-company")][InlineData("late-field")][InlineData("consumer")]
    public async Task ReviewerWorkspaceRetainsExactOriginalBeforeTransportAndUncertaintyBlocksReplacement(string mode)
    {
        var f=new WorkOrderMaterialAcceptanceFixture();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"independent",new(f.Permit.Context.Company,["ConfiguredRole"],["HSE.ApprovePermit","Inventory.View"])));using var state=new WorkOrderExecutionPermitDecisionState(company);state.Attach(f.Command.Review.Query.Permit,"source");var client=new Mock<ICompanyMaintenanceHandoffClient>();CompanyWorkOrderMaterialAcceptanceCommand? retained=null;var writes=0;
        client.Setup(c=>c.RecordWorkOrderMaterialAcceptanceAsync(It.IsAny<CompanyWorkOrderMaterialAcceptanceCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderMaterialAcceptanceCommand command,CancellationToken _)=>{writes++;Assert.Equal(JsonSerializer.Serialize(retained),JsonSerializer.Serialize(command));if(mode=="late-company")company.Clear();if(mode=="late-field")state.Attach(null,null);return mode=="unknown"?Task.FromException<Row>(new HttpRequestException("Unconfirmed acceptance")):Task.FromResult(command.Prepare(f.Pending,DateTime.UtcNow));});
        var error=await Record.ExceptionAsync(()=>state.AcceptMaterialAsync(client.Object,f.Command.Review,f.Pending.INSPECTION_ID,"Original independent acceptance",command=>{retained=command;if(mode=="consumer")command.Review.CurrentMaterial.Source.Inventory.Item.QUANTITY_ON_HAND=999;if(mode=="retention")throw new InvalidOperationException("Unable to retain original reference");return Task.CompletedTask;}));
        Assert.NotNull(retained);Assert.Equal(mode=="retention"?0:1,writes);if(mode is "late-company" or "late-field"){Assert.Null(state.MaterialReceipt);Assert.Null(state.VisibleMaterialCommand);return;}if(mode=="consumer")Assert.NotNull(error);
        Assert.False(state.CanAcceptMaterial);Assert.NotNull(state.VisibleMaterialCommand);Assert.Equal(5m,state.VisibleMaterialCommand!.Review.CurrentMaterial.Source.Inventory.Item.QUANTITY_ON_HAND);await Assert.ThrowsAsync<InvalidOperationException>(()=>state.AcceptMaterialAsync(client.Object,f.Command.Review,f.Pending.INSPECTION_ID,"Another attempt",_=>Task.CompletedTask));
        client.Setup(c=>c.RecoverWorkOrderMaterialAcceptanceAsync(It.IsAny<CompanyWorkOrderMaterialAcceptanceCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderMaterialAcceptanceCommand command,CancellationToken _)=>Task.FromResult(command.Prepare(f.Pending,DateTime.UtcNow)));await state.RecoverMaterialAsync(client.Object);Assert.NotNull(state.MaterialReceipt);Assert.Equal(mode=="retention"?0:1,writes);
    }
    [Theory][InlineData("original")][InlineData("late-company")][InlineData("task")][InlineData("damaged")]
    public async Task FreshReviewerWorkspaceRetainsReferenceBeforeReadAndRestoresExactOriginalWithoutWrite(string mode)
    {
        var f=new WorkOrderMaterialAcceptanceFixture();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"independent",new(f.Permit.Context.Company,["ConfiguredRole"],["HSE.ApprovePermit","Inventory.View"])));using var state=new WorkOrderExecutionPermitDecisionState(company);state.Attach(f.Command.Review.Query.Permit,"source");var reference=new CompanyWorkOrderMaterialAcceptanceReference(f.Command.Review.Query.Permit,f.Command.InspectionId,f.Command.IntentId);var client=new Mock<ICompanyMaintenanceHandoffClient>();var retained=false;
        client.Setup(c=>c.RecoverWorkOrderMaterialAcceptanceReferenceAsync(reference,It.IsAny<CancellationToken>())).Returns(()=>{Assert.True(retained);Assert.Equal(reference,state.VisibleMaterialReference);var row=f.Command.Prepare(f.Pending,DateTime.UtcNow);if(mode=="damaged")row.RESULT_NOTES="damaged";if(mode=="late-company")company.Clear();if(mode=="task")state.Attach(null,null);return Task.FromResult(new CompanyWorkOrderMaterialAcceptanceRecovery(f.Command,row));});
        var error=await Record.ExceptionAsync(()=>state.RecoverMaterialReferenceAsync(client.Object,reference,original=>{Assert.Equal(reference,original);retained=true;return Task.CompletedTask;}));if(mode=="original"){Assert.Null(error);Assert.NotNull(state.MaterialReceipt);Assert.Equal(JsonSerializer.Serialize(f.Command),JsonSerializer.Serialize(state.VisibleMaterialCommand));}else Assert.Null(state.MaterialReceipt);
        Assert.False(state.CanAcceptMaterial);client.Verify(c=>c.RecordWorkOrderMaterialAcceptanceAsync(It.IsAny<CompanyWorkOrderMaterialAcceptanceCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
}
#endif
