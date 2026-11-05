using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
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
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
[Collection("CompanySalesJournalSource")]
public sealed class CompanySalesJournalPostingScreenTests(CompanySalesJournalPreparationTests.ApprovedSourceFixture source):IClassFixture<CompanySalesJournalPreparationTests.ApprovedSourceFixture>
{
    private static readonly string[] Grants=["Accounting.View","Accounting.ReviewSalesJournalSource","Accounting.ReviewSalesJournalPosting","Accounting.PostSalesJournal","Accounting.PrepareSalesJournal","Accounting.ReviewSalesJournal","Accounting.RecordSalesJournalDecision"];
    private sealed class Authentication:AuthenticationStateProvider
    {public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,"authorized-poster"),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"test"))));}
    private sealed class Components:IComponentActivator
    {public Beep.OilandGas.Web.Components.Workflow.SalesJournalPostingWorkbench? Workbench;public IComponent CreateInstance(Type type){var component=(IComponent)Activator.CreateInstance(type)!;if(component is Beep.OilandGas.Web.Components.Workflow.SalesJournalPostingWorkbench value)Workbench=value;return component;}}
    [Fact] public async Task RenderedPostingFormRetainsExactUnknownRequestAcrossRecreationAndRecoversWithoutWriteGrant()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesJournalPostingTests.Fixture(source.Source);await f.Prepare(source);using var company=CompanyGLAccountHttpTests.Selected(actor:f.Command.Source.ActorUserId,grants:Grants);var client=new Mock<IAccountingServiceClient>();bool failedFollowup=true;var bodies=new List<string>();
        client.Setup(value=>value.GetCompanySalesJournalPostingSourceAsync(source.Source.BookId,source.Source.CurrencyCode,f.Command.Source.Reviewed.Header.JOURNAL_ENTRY_ID)).ReturnsAsync(CompanyInventorySourceReview.Copy(f.Command.Source));
        client.Setup(value=>value.PostCompanySalesJournalAsync(It.IsAny<CompanySalesJournalPostingCommand>())).Returns((CompanySalesJournalPostingCommand command)=>{bodies.Add(JsonSerializer.Serialize(command));f.Command=CompanyInventorySourceReview.Copy(command);return f.Post();});
        client.Setup(value=>value.GetCompanySalesJournalPostingRecoveryAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>())).Returns(()=>failedFollowup?Task.FromException<CompanySalesJournalPostingRecovery>(new HttpRequestException("Lost native follow-up")):f.Recover());
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton(company);services.AddScoped<CompanySalesReceivableWorkspaceState>();services.AddSingleton<AuthenticationStateProvider,Authentication>();services.AddSingleton<IAccountingServiceClient>(client.Object);services.AddSingleton<IComponentActivator,Components>();await using var provider=services.BuildServiceProvider();var state=provider.GetRequiredService<CompanySalesReceivableWorkspaceState>();var owner=new object();state.AttachOwner(owner);await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var workflowSource=CompanySalesJournalWorkflowSource.Create(source.Source.Company,f.Command.Source.ActorUserId,source.Source.BindingRevision,f.Command.Source.Reviewed);
            var assigned=new CompanySalesJournalProcessSourceReview(workflowSource.Company,workflowSource.ActorUserId,"recorded-posting-process",workflowSource.JournalId,workflowSource.SourceRevision,new string('C',64),workflowSource);
            var parameters=ParameterView.FromDictionary(new Dictionary<string,object?>{{"ReceivableId",source.Source.ReceivableId},{"AssignedSource",assigned}});var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.SalesJournalPostingWorkbench>(parameters);var components=(Components)provider.GetRequiredService<IComponentActivator>();
            Assert.Contains(assigned.JournalId,root.ToHtmlString());Assert.Contains(source.Source.BookId,root.ToHtmlString());
            void Invoke(string name,params object?[] values)=>components.Workbench!.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(components.Workbench,values);
            void Field(string name,object? value)=>components.Workbench!.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(components.Workbench,value);
            async Task Action(string name){await (Task)components.Workbench!.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(components.Workbench,null)!;Invoke("StateHasChanged");}
            void Identity(){Invoke("Identity","book",source.Source.BookId);Invoke("Identity","currency",source.Source.CurrencyCode);Invoke("Identity","id",f.Command.Source.Reviewed.Header.JOURNAL_ENTRY_ID);Invoke("StateHasChanged");}
            Identity();await Action("Load");Assert.True(state.CanPostJournal);Assert.Contains("Independent reviewer",root.ToHtmlString());await Action("Post");Assert.False(state.HasPendingJournalPosting);Assert.Equal(0,f.Reviewed.Draft.Native.Writes);
            Field("_reason","Explicit original native posting reason");Invoke("StateHasChanged");await Action("Post");Assert.True(state.HasPendingJournalPosting);Assert.Equal(7,f.Reviewed.Draft.Native.Writes);Assert.False(state.CanPrepareJournal);Assert.False(state.CanRecordJournalReview);Assert.Contains("exact original posting request is retained",root.ToHtmlString());var pending=JsonSerializer.Serialize(state.PendingJournalPosting);
            await Assert.ThrowsAsync<InvalidOperationException>(()=>state.SubmitPendingAsync(client.Object));await Assert.ThrowsAsync<InvalidOperationException>(()=>state.SubmitJournalPreparationAsync(client.Object));await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordJournalReviewAsync("APPROVE","CONFIRMED","financial","CONFIRMED","source","reason",client.Object));
            components.Workbench!.Dispose();root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.SalesJournalPostingWorkbench>(parameters);Assert.Equal(pending,JsonSerializer.Serialize(state.PendingJournalPosting));failedFollowup=false;await Action("Retry");Assert.False(state.HasPendingJournalPosting);Assert.Equal("POSTED",state.JournalPostingRecovery!.Outcome!.Journal.Header.STATUS);Assert.Equal(bodies[0],bodies[1]);Assert.Equal(7,f.Reviewed.Draft.Native.Writes);
            company.TrySelect(company.Version,f.Command.Source.ActorUserId,new(source.Source.Company,["ConfiguredCompanyRole"],Grants.Where(value=>value!="Accounting.PostSalesJournal").ToArray()));Identity();await Action("Recover");Assert.NotNull(state.JournalPostingRecovery!.Outcome);Assert.False(state.CanRetryJournalPosting);Assert.Equal(7,f.Reviewed.Draft.Native.Writes);Assert.Contains("native GL entries and receivable correlation are confirmed",root.ToHtmlString());
            state.DetachOwner(owner);Assert.Null(state.JournalPostingRecovery);
        });
    }
}
#endif
