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
public sealed class WorkOrderPersonnelAcceptanceStateTests
{
    [Theory][InlineData("record")][InlineData("unknown")][InlineData("retention")][InlineData("late-company")][InlineData("late-field")][InlineData("consumer")]
    public async Task ReviewerWorkspaceRetainsExactOriginalBeforeTransportAndUncertaintyBlocksReplacement(string mode)
    {
        var f=new WorkOrderPersonnelAcceptanceFixture();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"another-reviewer",new(f.Evidence.Assignment.Roster.Context.Company,["ConfiguredRole"],["HSE.ApprovePermit"])));using var state=new WorkOrderExecutionPermitDecisionState(company);state.Attach(f.Command.Review.Assignment.Query.Permit,"source");var client=new Mock<ICompanyMaintenanceHandoffClient>();CompanyWorkOrderPersonnelAcceptanceCommand? retained=null;var writes=0;
        client.Setup(c=>c.RecordWorkOrderPersonnelAcceptanceAsync(It.IsAny<CompanyWorkOrderPersonnelAcceptanceCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderPersonnelAcceptanceCommand command,CancellationToken _)=>{writes++;Assert.Equal(JsonSerializer.Serialize(retained),JsonSerializer.Serialize(command));if(mode=="late-company")company.Clear();if(mode=="late-field")state.Attach(null,null);return mode=="unknown"?Task.FromException<Row>(new HttpRequestException("Unconfirmed acceptance")):Task.FromResult(command.Prepare(f.Pending,DateTime.UtcNow));});
        var error=await Record.ExceptionAsync(()=>state.AcceptPersonnelAsync(client.Object,f.Command.Review,f.Pending.INSPECTION_ID,"Original independent acceptance",command=>{retained=command;if(mode=="consumer")command.Review.Assignment.CurrentPersonnel.PermitReview.CurrentSource.Order.WoSubType="CHANGED";if(mode=="retention")throw new InvalidOperationException("Unable to retain original reference");return Task.CompletedTask;}));
        Assert.NotNull(retained);Assert.Equal(mode=="retention"?0:1,writes);if(mode is "late-company" or "late-field"){Assert.Null(state.PersonnelAcceptanceReceipt);Assert.Null(state.VisiblePersonnelAcceptanceCommand);return;}if(mode=="consumer")Assert.NotNull(error);
        Assert.False(state.CanAcceptPersonnel);Assert.NotNull(state.VisiblePersonnelAcceptanceCommand);Assert.Equal("CORRECTIVE",state.VisiblePersonnelAcceptanceCommand!.Review.Assignment.CurrentPersonnel.PermitReview.CurrentSource.Order.WoSubType);await Assert.ThrowsAsync<InvalidOperationException>(()=>state.AcceptPersonnelAsync(client.Object,f.Command.Review,f.Pending.INSPECTION_ID,"Another attempt",_=>Task.CompletedTask));
        client.Setup(c=>c.RecoverWorkOrderPersonnelAcceptanceAsync(It.IsAny<CompanyWorkOrderPersonnelAcceptanceCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderPersonnelAcceptanceCommand command,CancellationToken _)=>Task.FromResult(command.Prepare(f.Pending,DateTime.UtcNow)));await state.RecoverPersonnelAcceptanceAsync(client.Object);Assert.NotNull(state.PersonnelAcceptanceReceipt);Assert.Equal(mode=="retention"?0:1,writes);
    }
    [Theory][InlineData("original")][InlineData("late-company")][InlineData("task")][InlineData("damaged")]
    public async Task FreshReviewerWorkspaceRetainsReferenceBeforeReadAndRestoresExactOriginalWithoutWrite(string mode)
    {
        var f=new WorkOrderPersonnelAcceptanceFixture();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"another-reviewer",new(f.Evidence.Assignment.Roster.Context.Company,["ConfiguredRole"],["HSE.ApprovePermit"])));using var state=new WorkOrderExecutionPermitDecisionState(company);state.Attach(f.Command.Review.Assignment.Query.Permit,"source");var reference=new CompanyWorkOrderPersonnelAcceptanceReference(f.Command.Review.Assignment.Query.Permit,f.Command.InspectionId,f.Command.IntentId);var client=new Mock<ICompanyMaintenanceHandoffClient>();var retained=false;
        client.Setup(c=>c.RecoverWorkOrderPersonnelAcceptanceReferenceAsync(reference,It.IsAny<CancellationToken>())).Returns(()=>{Assert.True(retained);Assert.Equal(reference,state.VisiblePersonnelAcceptanceReference);var row=f.Command.Prepare(f.Pending,DateTime.UtcNow);if(mode=="damaged")row.RESULT_NOTES="damaged";if(mode=="late-company")company.Clear();if(mode=="task")state.Attach(null,null);return Task.FromResult(new CompanyWorkOrderPersonnelAcceptanceRecovery(f.Command,row));});
        var error=await Record.ExceptionAsync(()=>state.RecoverPersonnelAcceptanceReferenceAsync(client.Object,reference,original=>{Assert.Equal(reference,original);retained=true;return Task.CompletedTask;}));if(mode=="original"){Assert.Null(error);Assert.NotNull(state.PersonnelAcceptanceReceipt);Assert.Equal(JsonSerializer.Serialize(f.Command),JsonSerializer.Serialize(state.VisiblePersonnelAcceptanceCommand));}else Assert.Null(state.PersonnelAcceptanceReceipt);
        Assert.False(state.CanAcceptPersonnel);client.Verify(c=>c.RecordWorkOrderPersonnelAcceptanceAsync(It.IsAny<CompanyWorkOrderPersonnelAcceptanceCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
}
#endif
