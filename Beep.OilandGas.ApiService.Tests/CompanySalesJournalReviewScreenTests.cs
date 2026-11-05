#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
[Collection("CompanySalesJournalSource")]
public sealed class CompanySalesJournalReviewScreenTests(CompanySalesJournalPreparationTests.ApprovedSourceFixture source):IClassFixture<CompanySalesJournalPreparationTests.ApprovedSourceFixture>
{
    [Fact] public async Task AssignedJournalUsesRecordedContextAndOwnPresentationWithoutAReceivableParent()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var fixture=new CompanySalesJournalReviewTests.Fixture(source.Source);await fixture.Prepare();using var company=CompanyGLAccountHttpTests.Selected(actor:fixture.Actor,grants:Grants);
        var native=CompanySalesJournalWorkflowSource.Create(source.Source.Company,fixture.Actor,source.Source.BindingRevision,fixture.Original);
        var assigned=new CompanySalesJournalProcessSourceReview(native.Company,native.ActorUserId,"recorded-process",native.JournalId,native.SourceRevision,new string('C',64),native);assigned.Require(native.Company,native.ActorUserId,assigned.InstanceId);
        var client=new Mock<IAccountingServiceClient>(MockBehavior.Strict);client.Setup(value=>value.GetCompanySalesJournalReviewAsync(source.Source.BookId,source.Source.CurrencyCode,native.JournalId)).Returns(fixture.Current);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton(company);services.AddScoped<CompanySalesReceivableWorkspaceState>();services.AddSingleton<AuthenticationStateProvider,Authentication>();services.AddSingleton<IAccountingServiceClient>(client.Object);services.AddSingleton<IComponentActivator,Components>();await using var provider=services.BuildServiceProvider();var state=provider.GetRequiredService<CompanySalesReceivableWorkspaceState>();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.AssignedSalesJournalWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"AssignedSource",assigned}}));var component=((Components)provider.GetRequiredService<IComponentActivator>()).Workbench!;
            Assert.Contains(native.JournalId,root.ToHtmlString());Assert.Contains(source.Source.BookId,root.ToHtmlString());
            await (Task)component.GetType().GetMethod("Load",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(component,null)!;
            Assert.NotNull(state.JournalReview);Assert.True(state.CanRecordJournalReview);Assert.Equal(0,fixture.Draft.Native.Writes);
            company.Clear();Assert.Null(state.JournalReview);Assert.False(state.CanRecordJournalReview);
        });
        client.Verify(value=>value.GetCompanySalesJournalReviewAsync(source.Source.BookId,source.Source.CurrencyCode,native.JournalId),Times.Once);
    }
    private static readonly string[] Grants=["Accounting.View","Accounting.ReviewSalesJournalSource","Accounting.ReviewSalesJournal","Accounting.RecordSalesJournalDecision","Accounting.PrepareSalesJournal","Accounting.RecordSalesReceivableDecision","Accounting.ReviewSalesReceivable"];
    private sealed class Authentication:AuthenticationStateProvider
    {public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,"independent-reviewer"),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"test"))));}
    private sealed class Components:IComponentActivator
    {public Beep.OilandGas.Web.Components.Workflow.SalesJournalReviewWorkbench? Workbench;public IComponent CreateInstance(Type type){var component=(IComponent)Activator.CreateInstance(type)!;if(component is Beep.OilandGas.Web.Components.Workflow.SalesJournalReviewWorkbench value)Workbench=value;return component;}}
    [Fact] public async Task RenderedIndependentFormRetainsExactDecisionAfterLostFollowupAndComponentRecreation()
    {
        var fixture=new CompanySalesJournalReviewTests.Fixture(source.Source);await fixture.Prepare();using var company=CompanyGLAccountHttpTests.Selected(actor:fixture.Actor,grants:Grants);var client=new Mock<IAccountingServiceClient>();var bodies=new List<string>();bool failRecovery=true;
        client.Setup(value=>value.GetCompanySalesJournalReviewAsync(source.Source.BookId,source.Source.CurrencyCode,fixture.Draft.Command.JournalId)).Returns(fixture.Current);
        client.Setup(value=>value.RecordCompanySalesJournalDecisionAsync(It.IsAny<CompanySalesJournalReviewCommand>())).Returns((CompanySalesJournalReviewCommand command)=>{bodies.Add(JsonSerializer.Serialize(command));fixture.Command=CompanyInventorySourceReview.Copy(command);return fixture.Review();});
        client.Setup(value=>value.GetCompanySalesJournalReviewRecoveryAsync(source.Source.BookId,source.Source.CurrencyCode,fixture.Draft.Command.JournalId)).Returns(()=>failRecovery?Task.FromException<CompanySalesJournalReviewRecovery>(new HttpRequestException("Lost follow-up")):fixture.Draft.Service.GetCompanySalesJournalReviewRecoveryAsync(source.Source.Company,fixture.Actor,source.Source.BookId,source.Source.CurrencyCode,fixture.Draft.Command.JournalId,fixture.Guard,_=>fixture.Guard()));
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton(company);services.AddScoped<CompanySalesReceivableWorkspaceState>();services.AddSingleton<AuthenticationStateProvider,Authentication>();services.AddSingleton<IAccountingServiceClient>(client.Object);services.AddSingleton<IComponentActivator,Components>();await using var provider=services.BuildServiceProvider();var state=provider.GetRequiredService<CompanySalesReceivableWorkspaceState>();state.AttachOwner(new object());await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var parameters=ParameterView.FromDictionary(new Dictionary<string,object?>{{"ReceivableId",source.Source.ReceivableId}});var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.SalesJournalReviewWorkbench>(parameters);var components=(Components)provider.GetRequiredService<IComponentActivator>();
            void Invoke(string method,params object?[] values)=>components.Workbench!.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(components.Workbench,values);
            void Field(string name,object? value)=>components.Workbench!.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(components.Workbench,value);
            async Task Action(string method,params object?[] values){await (Task)components.Workbench!.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(components.Workbench,values)!;Invoke("StateHasChanged");}
            Invoke("Identity","book",source.Source.BookId);Invoke("Identity","currency",source.Source.CurrencyCode);Invoke("Identity","id",fixture.Draft.Command.JournalId);await Action("Load");Assert.True(state.CanRecordJournalReview);Assert.Contains("Journal financial review evidence",root.ToHtmlString());
            await Action("Record","APPROVE");Assert.False(state.HasPendingJournalReview);Assert.Equal(0,fixture.Draft.Native.Writes);
            Field("_financial","CONFIRMED");Field("_source","CONFIRMED");Field("_financialEvidence","Complete balanced financial evidence");Field("_sourceEvidence","Original source and mapping/period evidence");Field("_reason","Explicit independent approval");Invoke("StateHasChanged");await Action("Record","APPROVE");Assert.True(state.HasPendingJournalReview);Assert.False(state.CanPrepareJournal);Assert.False(state.CanRecordReview);Assert.Equal(2,fixture.Draft.Native.Writes);Assert.Contains("exact original journal review request is retained",root.ToHtmlString());var retained=JsonSerializer.Serialize(state.PendingJournalReview);
            components.Workbench!.Dispose();root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.SalesJournalReviewWorkbench>(parameters);Assert.Equal(retained,JsonSerializer.Serialize(state.PendingJournalReview));failRecovery=false;await Action("Retry");Assert.False(state.HasPendingJournalReview);Assert.Equal("APPROVED",state.JournalReviewRecovery!.Outcome!.Header.STATUS);Assert.Equal(bodies[0],bodies[1]);Assert.Equal(2,fixture.Draft.Native.Writes);Assert.Contains("does not establish ledger posting",root.ToHtmlString());
        });
    }
    [Fact] public async Task ContextInvalidationHidesPendingDecisionAndLateReadCannotExposeAnotherCompany()
    {
        var fixture=new CompanySalesJournalReviewTests.Fixture(source.Source);await fixture.Prepare();using var company=CompanyGLAccountHttpTests.Selected(actor:fixture.Actor,grants:Grants);var client=new Mock<IAccountingServiceClient>();using var state=new CompanySalesReceivableWorkspaceState(company);var owner=new object();state.AttachOwner(owner);var user=(await new Authentication().GetAuthenticationStateAsync()).User;state.Admit(user);
        client.Setup(value=>value.GetCompanySalesJournalReviewAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>())).ReturnsAsync(fixture.Command.Review);await state.LoadJournalReviewAsync(source.Source.BookId,source.Source.CurrencyCode,fixture.Draft.Command.JournalId,source.Source.ReceivableId,client.Object);
        client.Setup(value=>value.RecordCompanySalesJournalDecisionAsync(It.IsAny<CompanySalesJournalReviewCommand>())).ThrowsAsync(new HttpRequestException());await Assert.ThrowsAsync<HttpRequestException>(()=>state.RecordJournalReviewAsync("APPROVE","CONFIRMED","financial","CONFIRMED","source","reason",client.Object));var command=JsonSerializer.Serialize(state.PendingJournalReview);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.SubmitPendingAsync(client.Object));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.SubmitJournalPreparationAsync(client.Object));
        client.Verify(value=>value.PrepareCompanySalesReceivableAsync(It.IsAny<CompanySalesReceivableCommand>()),Times.Never);
        client.Verify(value=>value.PrepareCompanySalesJournalAsync(It.IsAny<CompanySalesJournalPreparation>()),Times.Never);
        state.DetachOwner(owner);Assert.True(state.HasPendingJournalReview);Assert.Null(state.PendingJournalReview);state.AttachOwner(owner);state.Admit(user);Assert.Equal(command,JsonSerializer.Serialize(state.PendingJournalReview));
        var loading=new TaskCompletionSource<CompanySalesJournalReviewRecovery>();client.Setup(value=>value.GetCompanySalesJournalReviewRecoveryAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>())).Returns(loading.Task);var task=state.RecoverJournalReviewAsync(source.Source.BookId,source.Source.CurrencyCode,fixture.Draft.Command.JournalId,source.Source.ReceivableId,client.Object);state.Invalidate();loading.SetResult(new(source.Source.Company,fixture.Actor,fixture.Draft.Command.JournalId,null,null));await task;Assert.Null(state.JournalReviewRecovery);Assert.True(state.HasPendingJournalReview);
    }
}
#endif
