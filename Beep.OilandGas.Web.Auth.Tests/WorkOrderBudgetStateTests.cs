using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.WorkOrder;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Moq;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderBudgetStateTests
{
    internal static CompanyWorkOrderBudgetReview Review()
    {
        var start=Beep.OilandGas.ApiService.Tests.WorkOrderExecutionStartFixture.Original(true).Command;var requirements=start.Review.Prerequisites.Requirements;
        var context=requirements.Context with{ActorUserId="finance-maker",Revision="budget-authority"};
        var book=new CompanyLedgerBookFacts(Guid.NewGuid().ToString("N"),"recorded-book","Recorded company book","KWD",3,"TO_EVEN","calendar","UTC","book-maker",DateTime.UtcNow,new string('A',64));
        var result=new CompanyWorkOrderBudgetReview(context,new("production","native-production"),new(requirements.Order.InstanceId,requirements.Order.FieldId,book.BookId),requirements.Order,book,[],DateTime.UtcNow);result.Require();return result;
    }
    [Theory][InlineData("confirmed")][InlineData("unknown")][InlineData("before-transport")][InlineData("late-company")]
    public async Task BudgetRetainsOriginalBeforeTransportAndRecoversWithoutAnotherAfe(string mode)
    {
        var review=Review();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),review.Context.ActorUserId,new(review.Context.Company,["FinanceRole"],["Accounting.View","Accounting.PrepareWorkOrderBudget"])));
        using var state=new WorkOrderBudgetState(company);state.Attach(review.Query);var client=new Mock<IWorkOrderServiceClient>();
        client.Setup(value=>value.ReviewBudgetAsync(review.Query,It.IsAny<CancellationToken>())).ReturnsAsync(review);
        var retained=false;var writes=0;CompanyWorkOrderBudgetRecovery? result=null;
        client.Setup(value=>value.CreateBudgetAsync(It.IsAny<CompanyWorkOrderBudgetCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderBudgetCommand command,CancellationToken _)=>{
            Assert.True(retained);Assert.Equal(command.IntentId,state.VisibleReference);writes++;result=new(command,command.Prepare(DateTime.UtcNow));
            if(mode=="unknown")throw new HttpRequestException("Unknown budget commit");if(mode=="late-company")company.Clear();return Task.FromResult(result);});
        await state.ReviewAsync(client.Object);Task Retain(Guid id){retained=true;Assert.Equal(id,state.VisibleReference);Assert.NotNull(state.VisibleCommand);return mode=="before-transport"?Task.FromException(new InvalidOperationException("Reference persistence failed")):Task.CompletedTask;}
        var error=await Record.ExceptionAsync(()=>state.CreateAsync(client.Object,"ExplicitAFE","Recorded work budget",12.875m,DateTime.UtcNow,"Reviewed budget basis",Retain));
        Assert.Equal(mode=="before-transport"?0:1,writes);if(mode=="confirmed"){Assert.Null(error);Assert.NotNull(state.Receipt);return;}
        if(mode=="late-company")
        {
            Assert.Null(state.VisibleCommand);Assert.Null(state.VisibleReference);Assert.Null(state.Receipt);
            Assert.True(company.TrySelect(company.Clear(),review.Context.ActorUserId,new(review.Context.Company,["FinanceRole"],["Accounting.View","Accounting.PrepareWorkOrderBudget"])));state.Attach(review.Query);
        }
        else Assert.NotNull(error);
        var id=state.VisibleReference!.Value;var original=state.VisibleCommand!;Assert.False(state.CanReview);Assert.False(state.CanRecord);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.ReviewAsync(client.Object));
        Assert.True(company.TrySelect(company.Clear(),review.Context.ActorUserId,new(review.Context.Company,["FinanceRole"],["Accounting.View"])));state.Attach(review.Query);
        Assert.Null(state.VisibleCommand);Assert.Null(state.VisibleReference);await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>state.RecoverAsync(client.Object,id));
        Assert.True(company.TrySelect(company.Clear(),review.Context.ActorUserId,new(review.Context.Company,["FinanceRole"],["Accounting.View","Accounting.PrepareWorkOrderBudget"])));state.Attach(review.Query);
        Assert.Equal(JsonSerializer.Serialize(original),JsonSerializer.Serialize(state.VisibleCommand));
        result??=new(original,original.Prepare(DateTime.UtcNow));client.Setup(value=>value.RecoverBudgetAsync(new(review.Query,id),It.IsAny<CancellationToken>())).ReturnsAsync(result);
        await state.RecoverAsync(client.Object,id);Assert.Equal(JsonSerializer.Serialize(original),JsonSerializer.Serialize(state.Receipt!.Command));Assert.Equal(mode=="before-transport"?0:1,writes);
    }
}
#endif
