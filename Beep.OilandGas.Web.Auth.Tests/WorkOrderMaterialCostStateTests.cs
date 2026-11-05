using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.ProductionOperations.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Moq;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderMaterialCostStateTests
{
    internal static CompanyWorkOrderMaterialCostReview Review()
    {
        var before=WorkOrderBudgetDecisionStateTests.Review();
        var decision=new CompanyWorkOrderBudgetDecisionCommand(before,Guid.NewGuid(),WorkOrderBudgetDecision.Approve,"Independent recorded decision");var fact=CompanyWorkOrderBudgetDecisionFacts.From(decision.Prepare(DateTime.UtcNow));
        var current=before.Current with{Context=before.Current.Context with{ActorUserId="cost-recorder",Revision="cost-authority"},ComparedAtUtc=DateTime.UtcNow};
        var status=new CompanyWorkOrderBudgetDecisionStatus(current,before.Query,fact,CompanyWorkOrderBudgetDecisionStatus.GetBlockers(current,before.Query,fact),DateTime.UtcNow);
        var utc=DateTime.UtcNow;var material=new CompanyWorkOrderMaterialCostSource(current.Context.Company,current.BudgetDatabase,current.Query.WorkOrderId,current.Query.FieldId,"recorded-use","recorded-item","recorded-allocation","recorded-start","executor",2m,"EA","warehouse","location",utc,utc,new string('A',64));
        var result=new CompanyWorkOrderMaterialCostReview(status,new(before.Query,material.UsageId),material,[],DateTime.UtcNow);result.Require();return result;
    }
    [Theory][InlineData("confirmed")][InlineData("unknown")][InlineData("before-transport")][InlineData("late-company")]
    public async Task ActualCostRetainsOriginalBeforeTransportAndRecoversWithoutAnotherValuation(string mode)
    {
        var review=Review();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),review.Budget.Current.Context.ActorUserId,new(review.Budget.Current.Context.Company,["FinanceRole"],["Accounting.View","Accounting.RecordWorkOrderActualCost"])));
        using var state=new WorkOrderMaterialCostState(company);state.Attach(review.Query);var client=new Mock<IWorkOrderServiceClient>();
        client.Setup(value=>value.ReviewMaterialCostAsync(review.Query,It.IsAny<CancellationToken>())).ReturnsAsync(review);
        var retained=false;var writes=0;CompanyWorkOrderMaterialCostRecovery? result=null;
        client.Setup(value=>value.RecordMaterialCostAsync(It.IsAny<CompanyWorkOrderMaterialCostCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderMaterialCostCommand command,CancellationToken _)=>{
            Assert.True(retained);Assert.Equal(command.IntentId,state.VisibleReference);writes++;result=new(command,command.Prepare(DateTime.UtcNow));
            if(mode=="unknown")throw new HttpRequestException("Unknown budget commit");if(mode=="late-company")company.Clear();return Task.FromResult(result);});
        await state.ReviewAsync(client.Object);Task Retain(Guid id){retained=true;Assert.Equal(id,state.VisibleReference);Assert.NotNull(state.VisibleCommand);return mode=="before-transport"?Task.FromException(new InvalidOperationException("Reference persistence failed")):Task.CompletedTask;}
        var error=await Record.ExceptionAsync(()=>state.RecordAsync(client.Object,1m,review.Material.Unit,review.Budget.Current.Book.FunctionalCurrency,review.ComparedAtUtc,"recorded-category","Explicit valuation source",Retain));
        Assert.Equal(mode=="before-transport"?0:1,writes);if(mode=="confirmed"){Assert.Null(error);Assert.NotNull(state.Receipt);return;}
        if(mode=="late-company")
        {
            Assert.Null(state.VisibleCommand);Assert.Null(state.VisibleReference);Assert.Null(state.Receipt);
            Assert.True(company.TrySelect(company.Clear(),review.Budget.Current.Context.ActorUserId,new(review.Budget.Current.Context.Company,["FinanceRole"],["Accounting.View","Accounting.RecordWorkOrderActualCost"])));state.Attach(review.Query);
        }
        else Assert.NotNull(error);
        var id=state.VisibleReference!.Value;var original=state.VisibleCommand!;Assert.False(state.CanReview);Assert.False(state.CanRecord);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.ReviewAsync(client.Object));
        Assert.True(company.TrySelect(company.Clear(),review.Budget.Current.Context.ActorUserId,new(review.Budget.Current.Context.Company,["FinanceRole"],["Accounting.View"])));state.Attach(review.Query);
        Assert.Null(state.VisibleCommand);Assert.Null(state.VisibleReference);await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>state.RecoverAsync(client.Object,id));
        Assert.True(company.TrySelect(company.Clear(),review.Budget.Current.Context.ActorUserId,new(review.Budget.Current.Context.Company,["FinanceRole"],["Accounting.View","Accounting.RecordWorkOrderActualCost"])));state.Attach(review.Query);
        Assert.Equal(JsonSerializer.Serialize(original),JsonSerializer.Serialize(state.VisibleCommand));
        result??=new(original,original.Prepare(DateTime.UtcNow));client.Setup(value=>value.RecoverMaterialCostAsync(new(review.Query,id),It.IsAny<CancellationToken>())).ReturnsAsync(result);
        await state.RecoverAsync(client.Object,id);Assert.Equal(JsonSerializer.Serialize(original),JsonSerializer.Serialize(state.Receipt!.Command));Assert.Equal(mode=="before-transport"?0:1,writes);
    }
}
#endif
