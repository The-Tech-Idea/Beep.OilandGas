using Beep.OilandGas.Models.Data.WorkOrder;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Moq;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderBudgetDecisionStateTests
{
    internal static CompanyWorkOrderBudgetDecisionReview Review()
    {
        var basis=WorkOrderBudgetStateTests.Review();var command=new CompanyWorkOrderBudgetCommand(basis,Guid.NewGuid(),"ExplicitAFE","Original work budget",12.875m,DateTime.UtcNow,"Maker's original basis");var row=command.Prepare(DateTime.UtcNow);
        var current=basis with{Context=basis.Context with{ActorUserId="independent-reviewer",Revision="independent-authority"},Budgets=[CompanyWorkOrderBudgetCommand.Facts(row)],ComparedAtUtc=DateTime.UtcNow};
        var result=new CompanyWorkOrderBudgetDecisionReview(current,new(basis.Query,row.AFE_ID));result.Require();return result;
    }
    [Theory][InlineData("confirmed")][InlineData("unknown")][InlineData("before-transport")][InlineData("late-company")]
    public async Task IndependentDecisionRetainsOriginalBeforeTransportAndRecoversWithoutAnotherApproval(string mode)
    {
        var review=Review();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),review.Current.Context.ActorUserId,new(review.Current.Context.Company,["FinanceRole"],["Accounting.View","Accounting.ReviewWorkOrderBudget"])));
        using var state=new WorkOrderBudgetDecisionState(company);state.Attach(review.Query);var client=new Mock<IWorkOrderServiceClient>();
        client.Setup(value=>value.ReviewBudgetDecisionAsync(review.Query,It.IsAny<CancellationToken>())).ReturnsAsync(review);
        var retained=false;var writes=0;CompanyWorkOrderBudgetDecisionRecovery? result=null;
        client.Setup(value=>value.RecordBudgetDecisionAsync(It.IsAny<CompanyWorkOrderBudgetDecisionCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderBudgetDecisionCommand command,CancellationToken _)=>{
            Assert.True(retained);Assert.Equal(command.IntentId,state.VisibleReference);writes++;result=new(command,command.Prepare(DateTime.UtcNow));
            if(mode=="unknown")throw new HttpRequestException("Unknown budget commit");if(mode=="late-company")company.Clear();return Task.FromResult(result);});
        await state.ReviewAsync(client.Object);Task Retain(Guid id){retained=true;Assert.Equal(id,state.VisibleReference);Assert.NotNull(state.VisibleCommand);return mode=="before-transport"?Task.FromException(new InvalidOperationException("Reference persistence failed")):Task.CompletedTask;}
        var error=await Record.ExceptionAsync(()=>state.RecordAsync(client.Object,WorkOrderBudgetDecision.Approve,"Independent reviewed budget basis",Retain));
        Assert.Equal(mode=="before-transport"?0:1,writes);if(mode=="confirmed"){Assert.Null(error);Assert.NotNull(state.Receipt);return;}
        if(mode=="late-company")
        {
            Assert.Null(state.VisibleCommand);Assert.Null(state.VisibleReference);Assert.Null(state.Receipt);
            Assert.True(company.TrySelect(company.Clear(),review.Current.Context.ActorUserId,new(review.Current.Context.Company,["FinanceRole"],["Accounting.View","Accounting.ReviewWorkOrderBudget"])));state.Attach(review.Query);
        }
        else Assert.NotNull(error);
        var id=state.VisibleReference!.Value;var original=state.VisibleCommand!;Assert.False(state.CanReview);Assert.False(state.CanRecord);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.ReviewAsync(client.Object));
        Assert.True(company.TrySelect(company.Clear(),review.Current.Context.ActorUserId,new(review.Current.Context.Company,["FinanceRole"],["Accounting.View"])));state.Attach(review.Query);
        Assert.Null(state.VisibleCommand);Assert.Null(state.VisibleReference);await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>state.RecoverAsync(client.Object,id));
        Assert.True(company.TrySelect(company.Clear(),review.Current.Context.ActorUserId,new(review.Current.Context.Company,["FinanceRole"],["Accounting.View","Accounting.ReviewWorkOrderBudget"])));state.Attach(review.Query);
        Assert.Equal(JsonSerializer.Serialize(original),JsonSerializer.Serialize(state.VisibleCommand));
        result??=new(original,original.Prepare(DateTime.UtcNow));client.Setup(value=>value.RecoverBudgetDecisionAsync(new(review.Query,id),It.IsAny<CancellationToken>())).ReturnsAsync(result);
        await state.RecoverAsync(client.Object,id);Assert.Equal(JsonSerializer.Serialize(original),JsonSerializer.Serialize(state.Receipt!.Command));Assert.Equal(mode=="before-transport"?0:1,writes);
    }
}
#endif
