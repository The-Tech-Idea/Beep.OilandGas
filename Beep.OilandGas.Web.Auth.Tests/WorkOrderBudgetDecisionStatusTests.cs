#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Moq;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderBudgetDecisionStatusTests
{
    [Theory][InlineData("current")][InlineData("late-company")][InlineData("late-field")][InlineData("wrong-account")]
    public async Task PublicActualCostStateFencesFinancialAccountAndRetainsCurrentApprovalSeparately(string mode)
    {
        var budget=Status("changed");var review=new CompanyWorkOrderActualCostReview(budget,[],DateTime.UtcNow);
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),budget.Current.Context.ActorUserId,new(budget.Current.Context.Company,["FinanceViewer"],["Accounting.View"])));
        using var state=new WorkOrderBudgetDecisionStatusState(company);state.Attach(budget.Query);var client=new Mock<IWorkOrderServiceClient>();
        client.Setup(value=>value.ReadActualCostsAsync(budget.Query,It.IsAny<CancellationToken>())).Returns(()=>
        {
            if(mode=="late-company")company.Clear();if(mode=="late-field")state.Clear();
            return Task.FromResult(mode=="wrong-account"?review with{Budget=budget with{Current=budget.Current with{Context=budget.Current.Context with{ActorUserId="other"}}}}:review);
        });
        var error=await Record.ExceptionAsync(()=>state.LoadCostsAsync(client.Object));
        if(mode=="current"){Assert.Null(error);Assert.NotNull(state.ActualCosts);Assert.Equal(0m,state.ActualCosts.RecordedTotal());Assert.False(state.Current!.IsApprovedForCurrentScope);}
        else{Assert.Null(state.ActualCosts);if(mode=="wrong-account")Assert.IsType<UnauthorizedAccessException>(error);else Assert.Null(error);}
    }
    internal static CompanyWorkOrderBudgetDecisionStatus Status(string mode="approved")
    {
        var review=WorkOrderBudgetDecisionStateTests.Review();var command=new CompanyWorkOrderBudgetDecisionCommand(review,Guid.NewGuid(),mode=="rejected"?WorkOrderBudgetDecision.Reject:WorkOrderBudgetDecision.Approve,"Private reviewer rationale");
        var fact=mode=="unrecorded"?null:CompanyWorkOrderBudgetDecisionFacts.From(command.Prepare(DateTime.UtcNow));
        var current=review.Current with{Context=review.Current.Context with{ActorUserId="financial-viewer",Revision="current-view-authority"},ComparedAtUtc=DateTime.UtcNow};
        if(mode=="changed"){var order=CompanyInventorySourceReview.Copy(current.Order);order.Instructions="Changed work scope";current=current with{Order=order};}
        var result=new CompanyWorkOrderBudgetDecisionStatus(current,review.Query,fact,CompanyWorkOrderBudgetDecisionStatus.GetBlockers(current,review.Query,fact),DateTime.UtcNow);result.Require();return result;
    }
    [Theory][InlineData("approved")][InlineData("rejected")][InlineData("unrecorded")][InlineData("changed")][InlineData("late-company")][InlineData("late-field")]
    public async Task ReadOnlyStatusFencesContextAndSeparatesRecordedOutcomeFromCurrentValidity(string mode)
    {
        var status=Status(mode);using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),status.Current.Context.ActorUserId,new(status.Current.Context.Company,["FinanceViewer"],["Accounting.View"])));
        using var state=new WorkOrderBudgetDecisionStatusState(company);state.Attach(status.Query);var client=new Mock<IWorkOrderServiceClient>();
        client.Setup(value=>value.ReadBudgetDecisionStatusAsync(status.Query,It.IsAny<CancellationToken>())).Returns(()=>{if(mode=="late-company")company.Clear();if(mode=="late-field")state.Clear();return Task.FromResult(status);});
        await state.LoadAsync(client.Object);
        if(mode.StartsWith("late-",StringComparison.Ordinal))Assert.Null(state.Current);
        else{Assert.NotNull(state.Current);Assert.Equal(mode=="approved",state.Current.IsApprovedForCurrentScope);Assert.Equal(mode!="unrecorded",state.Current.Recorded is not null);Assert.DoesNotContain("Private reviewer rationale",JsonSerializer.Serialize(state.Current));}
        client.Verify(value=>value.RecordBudgetDecisionAsync(It.IsAny<CompanyWorkOrderBudgetDecisionCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    private sealed class Response(CompanyWorkOrderBudgetDecisionStatus status,Action changed):HttpMessageHandler
    {
        internal int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token){Calls++;Assert.EndsWith("/decision/status",request.RequestUri!.AbsolutePath);changed();return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=JsonContent.Create(status)});}
    }
    private sealed class CostResponse(CompanyWorkOrderActualCostReview review,Action changed):HttpMessageHandler
    {
        internal int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token){Calls++;Assert.EndsWith("/actual-costs",request.RequestUri!.AbsolutePath);changed();return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=JsonContent.Create(review)});}
    }
    [Theory][InlineData("current")][InlineData("permission")][InlineData("late-company")][InlineData("wrong-account")]
    public async Task TypedActualCostsReadRequiresViewAndFencesReturnedAccount(string mode)
    {
        var status=Status();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),status.Current.Context.ActorUserId,new(status.Current.Context.Company,["FinanceViewer"],mode=="permission"?[]:["Accounting.View"])));
        var review=new CompanyWorkOrderActualCostReview(mode=="wrong-account"?status with{Current=status.Current with{Context=status.Current.Context with{ActorUserId="other"}}}:status,[],DateTime.UtcNow);
        var handler=new CostResponse(review,()=>{if(mode=="late-company")company.Clear();});using var http=new HttpClient(handler){BaseAddress=new("https://fixture.invalid/")};
        var client=new WorkOrderServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company));var error=await Record.ExceptionAsync(()=>client.ReadActualCostsAsync(status.Query));
        if(mode=="current")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission"?0:1,handler.Calls);
    }
    [Theory][InlineData("current")][InlineData("permission")][InlineData("late-company")]
    public async Task TypedReadOnlyClientNeedsVisibilityAndCannotPublishAnotherCompanyResponse(string mode)
    {
        var status=Status();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),status.Current.Context.ActorUserId,new(status.Current.Context.Company,["FinanceViewer"],mode=="permission"?[]:["Accounting.View"])));
        var handler=new Response(status,()=>{if(mode=="late-company")company.Clear();});using var http=new HttpClient(handler){BaseAddress=new("https://fixture.invalid/")};
        var client=new WorkOrderServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company));var error=await Record.ExceptionAsync(()=>client.ReadBudgetDecisionStatusAsync(status.Query));
        if(mode=="current")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission"?0:1,handler.Calls);
    }
}
#endif
