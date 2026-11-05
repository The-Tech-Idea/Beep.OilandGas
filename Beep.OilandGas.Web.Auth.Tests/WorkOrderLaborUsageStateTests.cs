using Beep.OilandGas.Models.Data.WorkOrder;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Moq;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderLaborUsageStateTests
{
    [Theory][InlineData("confirmed")][InlineData("unknown")][InlineData("before-transport")][InlineData("late-company")]
    public async Task ActualLaborRetainsOriginalBeforeTransportAndRecoversWithoutAnotherTimeEntry(string mode)
    {
        var original=Beep.OilandGas.ApiService.Tests.WorkOrderExecutionStartFixture.Original(true);var start=original.Command;var started=start.Prepare(DateTime.UtcNow);
        var comparison=CompanyWorkOrderSubmissionInspectionReview.Create(new(start,started),start.Review.Prerequisites.Requirements,DateTime.UtcNow);
        var review=new CompanyWorkOrderLaborUsageReview(comparison,start.Review.Stage,start.Review.Prerequisites.Personnel,[],null,DateTime.UtcNow);review.Require();
        var context=comparison.Current.Context;using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,[start.Review.Stage.AssignedRole],["Maintenance.ExecuteWork"])));
        var reference=new CompanyWorkOrderExecutionStartReference(comparison.Start.Permit,start.IntentId);using var state=new WorkOrderLaborUsageState(company);state.Attach(reference);
        var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(value=>value.ReviewWorkOrderLaborUsageAsync(reference,It.IsAny<CancellationToken>())).ReturnsAsync(review);
        var retained=false;var writes=0;CompanyWorkOrderLaborUsageRecovery? recorded=null;
        client.Setup(value=>value.RecordWorkOrderLaborUsageAsync(It.IsAny<CompanyWorkOrderLaborUsageCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderLaborUsageCommand sent,CancellationToken _)=>{
            Assert.True(retained);Assert.Equal(sent.IntentId,state.VisibleReference);writes++;recorded=new(sent,sent.Prepare(DateTime.UtcNow));
            if(mode=="unknown")throw new HttpRequestException("Unknown native commit");if(mode=="late-company")company.Clear();return Task.FromResult(recorded);});
        await state.ReviewAsync(client.Object);
        var quantity=(decimal)(review.ComparedAtUtc.Ticks-started.STARTED_AT_UTC.Ticks)/TimeSpan.TicksPerSecond;
        Task Retain(Guid id){retained=true;Assert.Equal(id,state.VisibleReference);Assert.NotNull(state.VisibleCommand);return mode=="before-transport"?Task.FromException(new InvalidOperationException("Reference persistence failed")):Task.CompletedTask;}
        var error=await Record.ExceptionAsync(()=>state.RecordAsync(client.Object,started.STARTED_AT_UTC,review.ComparedAtUtc,quantity,WorkOrderLaborTimeUnit.Second,"Actual performed work","Own labor report",Retain));
        Assert.Equal(mode=="before-transport"?0:1,writes);
        if(mode=="confirmed"){Assert.Null(error);Assert.NotNull(state.Receipt);return;}
        if(mode=="late-company")
        {
            Assert.Null(state.VisibleReference);Assert.Null(state.VisibleCommand);Assert.Null(state.Receipt);
            Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,[start.Review.Stage.AssignedRole],["Maintenance.ExecuteWork"])));state.Attach(reference);
        }
        else Assert.NotNull(error);
        Assert.False(state.CanReview);Assert.False(state.CanRecord);var id=state.VisibleReference!.Value;var command=state.VisibleCommand!;
        Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,["AnotherRole"],["Maintenance.ExecuteWork"])));state.Attach(reference);
        Assert.Null(state.VisibleCommand);Assert.Null(state.VisibleReference);
        await Assert.ThrowsAnyAsync<Exception>(()=>state.RecoverAsync(client.Object,id));
        Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,[start.Review.Stage.AssignedRole],["Maintenance.ExecuteWork"])));state.Attach(reference);
        Assert.Equal(id,state.VisibleReference);Assert.Equal(JsonSerializer.Serialize(command),JsonSerializer.Serialize(state.VisibleCommand));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.ReviewAsync(client.Object));
        recorded??=new(command,command.Prepare(DateTime.UtcNow));client.Setup(value=>value.RecoverWorkOrderLaborUsageAsync(new(reference,id),It.IsAny<CancellationToken>())).ReturnsAsync(recorded);
        await state.RecoverAsync(client.Object,id);Assert.Equal(JsonSerializer.Serialize(command),JsonSerializer.Serialize(state.Receipt!.Command));Assert.Equal(mode=="before-transport"?0:1,writes);
    }
}
#endif
