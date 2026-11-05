#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Core.Interfaces;
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

public sealed class CompanySalesReceivableWorkspaceTests
{
    private const string Grant="Accounting.PrepareSalesReceivable",Route="api/accounting/traditional/company-receivables";
    private static ClaimsPrincipal User(string actor="reviewer",bool role=true)=>new(new ClaimsIdentity(new[]{new Claim(ClaimTypes.NameIdentifier,actor)}.Concat(role?[new Claim(ClaimTypes.Role,"ConfiguredCompanyRole")]:[]),"test"));
    private static CompanyClientContext Selected(CompanyScope scope,params string[] grants)
    {var selected=new CompanyClientContext();selected.TrySelect(selected.Clear(),"reviewer",new(scope,["ConfiguredCompanyRole"],new[]{"Accounting.View"}.Concat(grants).ToArray()));return selected;}
    private static async Task<AccountingSourceHttpTests.Host> Start(CompanySalesReceivablePreparationTests.Fixture f,bool auth=true,bool role=true,string[]? grants=null)
    {
        var host=await f.Invoice.Data.Start(authenticated:auth,role:role,permissions:grants??["Accounting.View",Grant],configure:(services,context)=>{
            context.Actor="reviewer";services.AddSingleton<IARService>(f.Owner);services.AddScoped<CompanySalesReceivableReads>();});
        host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor","reviewer");return host;
    }
    private static CompanySalesReceivableCommand Command(CompanySalesReceivableWorkspaceReview workspace)=>new(Guid.NewGuid().ToString("N"),Guid.NewGuid().ToString("N"),workspace.Source,"Explicit receivable number","Explicit preparation reason");
    [Fact]public async Task ActualNativeApiAndClientPrepareRecoverAndExposeOnlyRecordedHeaders()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesReceivablePreparationTests.Fixture();await f.Prepare();await using var host=await Start(f);
        using var company=Selected(f.Command.Source.Company,Grant);var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,company);var id=f.Command.Source.InvoiceId;
        var status=await client.GetCompanySalesReceivableStatusAsync(id);Assert.Empty(status.Receivables);Assert.Equal("APPROVED",status.Invoice.Status);
        var command=Command(await client.GetCompanySalesReceivableSourceAsync(id));var result=await client.PrepareCompanySalesReceivableAsync(command);result.Require(command);
        var recovered=await client.GetCompanySalesReceivableRecoveryAsync(id);recovered.Outcome!.Require(command);Assert.Equal(3,f.Invoice.Data.Data.Native.Writes);
        status=await client.GetCompanySalesReceivableStatusAsync(id);Assert.Equal(command.ReceivableId,Assert.Single(status.Receivables).Id);
        var json=JsonSerializer.Serialize(status);Assert.DoesNotContain("COMMAND_JSON",json);Assert.DoesNotContain("PREPARATION_WORKFLOW_JSON",json);Assert.DoesNotContain(command.Reason,json);
        Assert.Equal(status.Invoice.ProposedTotal,Assert.Single(status.Receivables).ProposedTotal);Assert.Equal(status.Invoice.Currency,Assert.Single(status.Receivables).Currency);
        (await client.PrepareCompanySalesReceivableAsync(command)).Require(command);Assert.Equal(3,f.Invoice.Data.Data.Native.Writes);
    }
    [Theory][InlineData("anonymous",401)][InlineData("role",403)][InlineData("view",403)][InlineData("field",403)][InlineData("binding",403)][InlineData("company",409)]
    public async Task RecordedStatusRequiresCurrentCompanyRoleFieldAndBinding(string fault,int expected)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesReceivablePreparationTests.Fixture();await f.Prepare();f.Invoice.Data.AssetDenied=fault=="field";
        await using var host=await Start(f,auth:fault!="anonymous",role:fault!="role",grants:fault=="view"?[]:["Accounting.View"]);
        if(fault=="binding")host.Context.ChangeBinding=true;if(fault=="company")host.Client.DefaultRequestHeaders.Add("X-Test-Company","other");
        Assert.Equal((HttpStatusCode)expected,(await host.Client.GetAsync(Route+"/status/"+f.Command.Source.InvoiceId)).StatusCode);Assert.Equal(0,f.Invoice.Data.Data.Native.Writes);
    }
    [Theory][InlineData("Accounting.View")][InlineData("Accounting.ReviewSalesInvoice")][InlineData("Accounting.PrepareSalesInvoice")][InlineData("Accounting.PrepareBillingPolicy")]
    public async Task OtherInvoiceAndPolicyGrantsCannotBorrowReceivablePreparation(string grant)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesReceivablePreparationTests.Fixture();await f.Prepare();await using var host=await Start(f,grants:["Accounting.View",grant]);
        Assert.Equal(HttpStatusCode.OK,(await host.Client.GetAsync(Route+"/status/"+f.Command.Source.InvoiceId)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.GetAsync(Route+"/source/"+f.Command.Source.InvoiceId)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.PostAsJsonAsync(Route+"/drafts",f.Command)).StatusCode);Assert.Equal(0,f.Invoice.Data.Data.Native.Writes);
    }
    [Fact]public async Task RecoveryObservesKnownOriginalOutcomeAfterPhysicalAdvanceWithoutPreparationGrant()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesReceivablePreparationTests.Fixture();await f.Prepare();var result=await f.Execute();
        var physical=f.Invoice.Data.Data.Native.Rows.First(row=>row.GetType().Name=="REVENUE_TRANSACTION");physical.GetType().GetProperty("ROW_CHANGED_BY")!.SetValue(physical,"advanced receiving evidence");
        await using var host=await Start(f,grants:["Accounting.View"]);using var selected=Selected(f.Command.Source.Company);var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,selected);
        (await client.GetCompanySalesReceivableRecoveryAsync(f.Command.Source.InvoiceId)).Outcome!.Require(f.Command);Assert.Equal(3,f.Invoice.Data.Data.Native.Writes);
        Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.GetAsync(Route+"/source/"+f.Command.Source.InvoiceId)).StatusCode);
        host.Context.Permissions=["Accounting.View",Grant];Assert.Equal(HttpStatusCode.Conflict,(await host.Client.GetAsync(Route+"/source/"+f.Command.Source.InvoiceId)).StatusCode);
        (await client.PrepareCompanySalesReceivableAsync(f.Command)).Require(f.Command);Assert.Equal(JsonSerializer.Serialize(result),JsonSerializer.Serialize((await client.GetCompanySalesReceivableRecoveryAsync(f.Command.Source.InvoiceId)).Outcome));Assert.Equal(3,f.Invoice.Data.Data.Native.Writes);
    }
    [Theory][InlineData("field-write")][InlineData("changed-roles")][InlineData("changed-grant")]
    public async Task DraftCannotPublishAfterWriteAuthorityChanges(string fault)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesReceivablePreparationTests.Fixture();await f.Prepare();await using var host=await Start(f);
        using var selected=Selected(f.Command.Source.Company,Grant);var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,selected);var command=Command(await client.GetCompanySalesReceivableSourceAsync(f.Command.Source.InvoiceId));
        if(fault=="field-write")f.Invoice.Data.Assets.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),command.Source.FieldId,"FIELD","WRITE",null)).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=false});
        else f.OnReceipt=()=>{if(fault=="changed-roles")host.Context.Roles=[];else host.Context.Permissions=["Accounting.View"];};
        Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.PostAsJsonAsync(Route+"/drafts",command)).StatusCode);Assert.DoesNotContain(f.Invoice.Data.Data.Native.Rows,row=>row.GetType().Name=="AR_INVOICE");
    }
    [Fact]public async Task CircuitReopensExactRetainedRequestAndNeverReplacesFailedDraft()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesReceivablePreparationTests.Fixture();await f.Prepare();await using var host=await Start(f);using var company=Selected(f.Command.Source.Company,Grant);
        var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,company);CompanySalesReceivableCommand original;
        using(var first=new CompanySalesReceivableWorkspaceState(company)){
            first.AttachOwner(new object());first.Admit(User());await first.LoadSourceAsync(f.Command.Source.InvoiceId,client);f.FailDraft=true;
            await Assert.ThrowsAsync<OilGasApiException>(()=>first.PrepareAsync("Explicit number","Explicit reason",client));original=first.Pending!;Assert.True(first.HasPendingIntent);Assert.False(first.CanPrepare);}
        using var next=new CompanySalesReceivableWorkspaceState(company);next.AttachOwner(new object());next.Admit(User());await next.RecoverAsync(f.Command.Source.InvoiceId,client);
        Assert.Equal(JsonSerializer.Serialize(original),JsonSerializer.Serialize(next.Pending));f.FailDraft=false;await next.SubmitPendingAsync(client);next.Recovery!.Outcome!.Require(original);Assert.False(next.HasPendingIntent);
        Assert.Single(f.Invoice.Data.Data.Native.Rows.OfType<SALES_RECEIVABLE_INTENT>());Assert.Single(f.Invoice.Data.Data.Native.Rows.OfType<SALES_RECEIVABLE_OUTCOME>());
    }
    [Fact]public async Task AnotherAuthorizedAccountingReaderSeesHeadersButCannotRecoverPrivateMakerCommand()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesReceivablePreparationTests.Fixture();await f.Prepare();await f.Execute();await using var host=await Start(f,grants:["Accounting.View"]);
        host.Context.Actor="reader";host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor","reader");
        var response=await host.Client.GetAsync(Route+"/status/"+f.Command.Source.InvoiceId);Assert.Equal(HttpStatusCode.OK,response.StatusCode);var json=await response.Content.ReadAsStringAsync();Assert.DoesNotContain(f.Command.Reason,json);Assert.DoesNotContain("COMMAND_JSON",json);
        response=await host.Client.GetAsync(Route+"/recovery/"+f.Command.Source.InvoiceId);Assert.Equal(HttpStatusCode.Conflict,response.StatusCode);Assert.DoesNotContain(f.Command.Reason,await response.Content.ReadAsStringAsync());Assert.Equal(3,f.Invoice.Data.Data.Native.Writes);
    }
    [Theory][InlineData("auth")][InlineData("detached")][InlineData("company")]
    public async Task LateCommitRemainsPrivateUntilExactRecovery(string change)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesReceivablePreparationTests.Fixture();await f.Prepare();await using var host=await Start(f);using var company=Selected(f.Command.Source.Company,Grant);
        var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,company);using var state=new CompanySalesReceivableWorkspaceState(company);var owner=new object();state.AttachOwner(owner);state.Admit(User());await state.LoadSourceAsync(f.Command.Source.InvoiceId,client);
        var committed=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);f.Invoice.Data.Data.Native.AfterCommit=()=>{if(f.Invoice.Data.Data.Native.Rows.OfType<SALES_RECEIVABLE_OUTCOME>().Any())committed.TrySetResult();};
        f.OnDraft=()=>{if(change=="auth")state.Invalidate();else if(change=="company")company.Clear();else state.DetachOwner(owner);};
        if(change=="company"){var refused=await Record.ExceptionAsync(()=>state.PrepareAsync("Explicit number","Explicit reason",client));Assert.True(refused is OperationCanceledException or InvalidOperationException);}
        else await state.PrepareAsync("Explicit number","Explicit reason",client);
        await committed.Task.WaitAsync(TimeSpan.FromSeconds(30));
        Assert.True(state.HasPendingIntent);Assert.Null(state.Pending);Assert.Null(state.Recovery);Assert.Null(state.Workspace);Assert.Equal(3,f.Invoice.Data.Data.Native.Writes);
        company.TrySelect(company.Clear(),"reviewer",new(f.Command.Source.Company,["ConfiguredCompanyRole"],["Accounting.View"]));state.AttachOwner(new object());state.Admit(User());await state.RecoverAsync(f.Command.Source.InvoiceId,client);Assert.False(state.HasPendingIntent);Assert.NotNull(state.Recovery!.Outcome);
    }
    private sealed class Authentication:AuthenticationStateProvider
    {public bool Role=true;public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(User(role:Role)));public void Revoke(){Role=false;NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());}}
    private sealed class Components:IComponentActivator
    {public Beep.OilandGas.Web.Components.Workflow.SalesReceivableWorkbench? Workbench;public IComponent CreateInstance(Type type){var component=(IComponent)Activator.CreateInstance(type)!;if(component is Beep.OilandGas.Web.Components.Workflow.SalesReceivableWorkbench workbench)Workbench=workbench;return component;}}
    [Theory][InlineData(false)][InlineData(true)]
    public async Task ActualRoutedMudScreenShowsRecordedEvidenceExplicitFormAndRedactsRevokedRole(bool prepare)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesReceivablePreparationTests.Fixture();await f.Prepare();await using var host=await Start(f);using var company=Selected(f.Command.Source.Company,prepare?[Grant]:[]);
        var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,company);var auth=new Authentication();var services=new ServiceCollection();services.AddLogging();services.AddMudServices();
        services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton(company);services.AddSingleton<IAccountingServiceClient>(client);services.AddScoped<CompanySalesReceivableWorkspaceState>();services.AddSingleton<AuthenticationStateProvider>(auth);services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.SalesReceivables>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Invoice",f.Command.Source.InvoiceId}}));
            var html=root.ToHtmlString();Assert.Contains("APPROVED",html);Assert.Contains("49.38",html);Assert.Contains("KWD",html);Assert.Contains("No owned receivable draft",html);Assert.Contains("Billing and receiving evidence",html);
            var component=((Components)provider.GetRequiredService<IComponentActivator>()).Workbench!;
            if(prepare){await (Task)component.GetType().GetMethod("Source",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!.Invoke(component,null)!;
                typeof(ComponentBase).GetMethod("StateHasChanged",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!.Invoke(component,null);await root.QuiescenceTask;html=root.ToHtmlString();
                Assert.Contains("Receivable document number",html);Assert.Contains("Preparation reason",html);Assert.DoesNotContain("Explicit receivable number",html);
                await (Task)component.GetType().GetMethod("Prepare",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!.Invoke(component,null)!;Assert.Equal(0,f.Invoice.Data.Data.Native.Writes);
                component.GetType().GetField("_number",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!.SetValue(component,"Explicit screen number");
                component.GetType().GetField("_reason",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!.SetValue(component,"Explicit screen reason");
                typeof(ComponentBase).GetMethod("StateHasChanged",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!.Invoke(component,null);await root.QuiescenceTask;
                await (Task)component.GetType().GetMethod("Prepare",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!.Invoke(component,null)!;
                typeof(ComponentBase).GetMethod("StateHasChanged",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!.Invoke(component,null);await root.QuiescenceTask;html=root.ToHtmlString();
                Assert.Contains("original receivable draft and durable receipt are confirmed",html);Assert.Contains("Explicit screen number",html);Assert.Equal(3,f.Invoice.Data.Data.Native.Writes);}
            else Assert.DoesNotContain("Review current preparation source",html);
            auth.Revoke();await Task.Yield();html=root.ToHtmlString();Assert.DoesNotContain("49.38",html);Assert.DoesNotContain("Receivable document number",html);Assert.DoesNotContain("Explicit screen number",html);
        });
    }
    [Fact]public async Task UnknownCommitKeepsOriginalRequestAndRecoveryConfirmsWithoutAnotherWrite()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesReceivablePreparationTests.Fixture();await f.Prepare();await using var host=await Start(f);using var company=Selected(f.Command.Source.Company,Grant);
        var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,company);using var state=new CompanySalesReceivableWorkspaceState(company);state.AttachOwner(new object());state.Admit(User());await state.LoadSourceAsync(f.Command.Source.InvoiceId,client);
        f.UncertainDraft=true;await Assert.ThrowsAsync<OilGasApiException>(()=>state.PrepareAsync("Original uncertain number","Original uncertain reason",client));
        var original=state.Pending!;Assert.True(state.HasPendingIntent);Assert.Null(state.Recovery);Assert.Equal(3,f.Invoice.Data.Data.Native.Writes);
        await state.RecoverAsync(f.Command.Source.InvoiceId,client);state.Recovery!.Outcome!.Require(original);Assert.False(state.HasPendingIntent);Assert.Equal(3,f.Invoice.Data.Data.Native.Writes);
    }
    [Fact]public async Task MissingConfirmationReceiptCannotDiscardOriginalRequest()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesReceivablePreparationTests.Fixture();await f.Prepare();await using var host=await Start(f);using var company=Selected(f.Command.Source.Company,Grant);
        var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,company);using var state=new CompanySalesReceivableWorkspaceState(company);state.AttachOwner(new object());state.Admit(User());await state.LoadSourceAsync(f.Command.Source.InvoiceId,client);
        var damaged=new Mock<IAccountingServiceClient>();damaged.Setup(value=>value.PrepareCompanySalesReceivableAsync(It.IsAny<CompanySalesReceivableCommand>())).Returns((CompanySalesReceivableCommand command)=>client.PrepareCompanySalesReceivableAsync(command));
        damaged.Setup(value=>value.GetCompanySalesReceivableRecoveryAsync(f.Command.Source.InvoiceId)).Returns(async()=>{var loaded=await client.GetCompanySalesReceivableRecoveryAsync(f.Command.Source.InvoiceId);return loaded with{Outcome=null};});
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.PrepareAsync("Original number","Original reason",damaged.Object));Assert.True(state.HasPendingIntent);Assert.Null(state.Recovery);Assert.Equal(3,f.Invoice.Data.Data.Native.Writes);
        var original=state.Pending!;await state.RecoverAsync(f.Command.Source.InvoiceId,client);state.Recovery!.Outcome!.Require(original);Assert.False(state.HasPendingIntent);
    }
    [Theory][InlineData("company")][InlineData("account")][InlineData("actor")][InlineData("amount")][InlineData("revision")]
    public async Task TypedClientRefusesLateOrAlteredRecordedHeaders(string change)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesReceivablePreparationTests.Fixture();await f.Prepare();var parent=await f.Invoice.Data.Data.Owner.GetCompanyInvoiceDraftForReviewAsync(f.Command.Source.Company,f.Command.Source.InvoiceId,f.Guard,_=>f.Guard());
        var status=CompanySalesReceivableStatusReview.Create(f.Command.Source.Company,"reviewer",new string('B',64),parent,[]);using var company=Selected(f.Command.Source.Company);
        using var http=new HttpClient(new Handler(request=>{
            Assert.EndsWith("/status/"+f.Command.Source.InvoiceId,request.RequestUri!.AbsolutePath);
            if(change=="company")company.TrySelect(company.Clear(),"reviewer",new(new("tenant","other"),["ConfiguredCompanyRole"],["Accounting.View"]));
            if(change=="account")company.TrySelect(company.Clear(),"other",new(f.Command.Source.Company,["ConfiguredCompanyRole"],["Accounting.View"]));
            if(change=="actor")status=status with{ActorUserId="other"};if(change=="amount")status=status with{Invoice=status.Invoice with{ProposedTotal=999m}};if(change=="revision")status=status with{Revision=new string('C',64)};
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=JsonContent.Create(status)});
        })){BaseAddress=new("http://localhost/")};var client=CompanySalesInvoiceHttpTests.Fixture.Client(http,company);
        if(change is "company" or "account")await Assert.ThrowsAsync<OperationCanceledException>(()=>client.GetCompanySalesReceivableStatusAsync(f.Command.Source.InvoiceId));
        else await Assert.ThrowsAsync<InvalidOperationException>(()=>client.GetCompanySalesReceivableStatusAsync(f.Command.Source.InvoiceId));
    }
    private sealed class Handler(Func<HttpRequestMessage,Task<HttpResponseMessage>> send):HttpMessageHandler
    {protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)=>send(request);}
}
#endif
