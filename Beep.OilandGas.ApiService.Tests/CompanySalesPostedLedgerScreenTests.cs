#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using System.Security.Claims;
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
public sealed class CompanySalesPostedLedgerScreenTests(CompanySalesJournalPreparationTests.ApprovedSourceFixture source):IClassFixture<CompanySalesJournalPreparationTests.ApprovedSourceFixture>
{
    private sealed class Authentication:AuthenticationStateProvider
    {public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,"readonly-accountant"),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"test"))));}
    private sealed class Components:IComponentActivator
    {public Beep.OilandGas.Web.Components.Workflow.SalesPostedLedgerPanel? Panel;public IComponent CreateInstance(Type type){var component=(IComponent)Activator.CreateInstance(type)!;if(component is Beep.OilandGas.Web.Components.Workflow.SalesPostedLedgerPanel panel)Panel=panel;return component;}}
    [Fact] public async Task RenderedReadOnlyLedgerRequiresExplicitBookCurrencyAndDiscardsLateCompanyResponse()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesJournalPostingTests.Fixture(source.Source);await f.Prepare(source);_=await f.Post();using var company=CompanyGLAccountHttpTests.Selected(actor:"readonly-accountant",grants:["Accounting.View","Accounting.ReviewPostedSalesLedger"]);var client=new Mock<IAccountingServiceClient>();
        Task<CompanySalesPostedLedgerReview> Native()=>f.Reviewed.Draft.Service.GetCompanySalesPostedLedgerAsync(source.Source.Company,"readonly-accountant",source.Source.BindingRevision,source.Source.BookId,source.Source.CurrencyCode,source.Source.ReceivableId,f.AR,f.Guard,f.Field);
        client.Setup(value=>value.GetCompanySalesPostedLedgerAsync(source.Source.ReceivableId,source.Source.BookId,source.Source.CurrencyCode)).Returns(Native);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton(company);services.AddScoped<CompanySalesReceivableWorkspaceState>();services.AddSingleton<AuthenticationStateProvider,Authentication>();services.AddSingleton<IAccountingServiceClient>(client.Object);services.AddSingleton<IComponentActivator,Components>();await using var provider=services.BuildServiceProvider();var state=provider.GetRequiredService<CompanySalesReceivableWorkspaceState>();state.AttachOwner(new object());await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.SalesPostedLedgerPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"ReceivableId",source.Source.ReceivableId}}));var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel!;
            void Invoke(string name,params object?[] values)=>panel.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,values);
            async Task Load(){await (Task)panel.GetType().GetMethod("Load",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null)!;Invoke("StateHasChanged");}
            await Load();Assert.Null(state.PostedLedger);client.Verify(value=>value.GetCompanySalesPostedLedgerAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>()),Times.Never);
            Invoke("Selection","book",source.Source.BookId);Invoke("Selection","currency",source.Source.CurrencyCode);Invoke("StateHasChanged");await Load();var ledger=state.PostedLedger!;Assert.Single(ledger.Journals);Assert.Contains("Posted journal",root.ToHtmlString());Assert.Contains("Posting producer authorized-poster",root.ToHtmlString());Assert.Contains("Account 1110",root.ToHtmlString());Assert.DoesNotContain("Post approved journal",root.ToHtmlString());Assert.Equal(7,f.Reviewed.Draft.Native.Writes);
            var completion=new TaskCompletionSource<CompanySalesPostedLedgerReview>();client.Setup(value=>value.GetCompanySalesPostedLedgerAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>())).Returns(completion.Task);var loading=state.LoadPostedLedgerAsync(source.Source.ReceivableId,source.Source.BookId,source.Source.CurrencyCode,client.Object);company.Clear();completion.SetResult(ledger);await loading;Assert.Null(state.PostedLedger);Invoke("StateHasChanged");Assert.DoesNotContain("Posted journal",root.ToHtmlString());Assert.Equal(7,f.Reviewed.Draft.Native.Writes);
        });
    }
}
#endif
