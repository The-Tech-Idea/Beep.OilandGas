#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Moq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor.Services;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanySalesReceivableDecisionTests
{
    internal sealed class Fixture
    {
        public readonly CompanySalesReceivablePreparationTests.Fixture Data=new();
        public CompanySalesReceivableOutcome Original=null!;
        public CompanySalesReceivableDecisionCommand Command=null!;
        public string Actor="controller";
        public bool Denied,FailJournal,FailUpdate,CorruptUpdate,Uncertain;
        public Action? OnUpdate;
        public Action<object>? Audit;
        public int Updates,Reads;
        public async Task Prepare(string decision="APPROVE")
        {
            await Data.Prepare();Original=await Data.Execute();var native=Data.Invoice.Data.Data.Native;
            var columns=Mock.Get((ICommonColumnHandler)typeof(ARService).GetField("_commonColumnHandler",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(Data.Owner)!);
            var actual=new CommonColumnHandler();columns.Setup(value=>value.PrepareForUpdate(It.IsAny<Beep.OilandGas.PPDM.Models.IPPDMEntity>(),It.IsAny<string>())).Callback((Beep.OilandGas.PPDM.Models.IPPDMEntity row,string actor)=>{actual.PrepareForUpdate(row,actor);Audit?.Invoke(row);});
            var types=native.Rows.Select(row=>row.GetType()).Concat([typeof(SALES_RECEIVABLE_REVIEW_INTENT)]).Distinct().ToDictionary(type=>type.Name,type=>type);
            native.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure requested,bool refresh)=>new ClassCreator(native.Editor.Object).ConvertToEntityStructure(types[requested.EntityName]));
            native.Source.Setup(value=>value.InsertEntity("SALES_RECEIVABLE_REVIEW_INTENT",It.IsAny<object>())).Returns((string _,object value)=>{native.Writes++;if(FailJournal)return new ErrorsInfo{Flag=Errors.Failed};native.Rows.Add(CompanyInventorySourceReview.Copy((SALES_RECEIVABLE_REVIEW_INTENT)value));return new ErrorsInfo{Flag=Errors.Ok};});
            native.Source.Setup(value=>value.UpdateEntity("AR_INVOICE",It.IsAny<object>())).Returns((string _,object value)=>{
                native.Writes++;Updates++;if(FailUpdate)return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((AR_INVOICE)value);if(CorruptUpdate)row.TOTAL_AMOUNT+=1;
                native.Rows.RemoveAll(value=>value is AR_INVOICE existing && existing.AR_INVOICE_ID==row.AR_INVOICE_ID);native.Rows.Add(row);if(Uncertain)native.FailCommit=true;OnUpdate?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
            Command=new(Guid.NewGuid().ToString("N"),await Current(),decision,"CONFIRMED","Explicit reviewed financial evidence","CONFIRMED","Explicit reviewed source evidence","Explicit independent receivable disposition");
            native.Writes=0;native.Transactions.Clear();
        }
        public Task Guard(){if(Denied)throw new UnauthorizedAccessException();return Data.Guard();}
        public async Task<CompanySalesReceivableReviewSource> Current()
        {
            Reads++;await Guard();var original=await Data.Owner.GetCompanySalesReceivableForReviewAsync(Data.Command.Source.Company,Data.Command.ReceivableId,Guard,_=>Guard());
            var invoice=(await Data.Invoice.Current()).AsReader(Actor);
            var rows=await Data.Owner.GetCompanySalesReceivablesAsync(invoice.Current.Company,invoice.Current.FieldId,invoice.InvoiceId,Guard);
            return CompanySalesReceivableReviewSource.Create(original,invoice,rows);
        }
        public Task<CompanySalesReceivableOutcome> Execute()=>Data.Owner.RecordCompanySalesReceivableDecisionAsync(Command,Guard,Current);
    }
    [Theory][InlineData("APPROVE")][InlineData("REJECT")]
    public async Task IndependentNativeDispositionPreservesExactOriginalDraftReceiptMoneyAndReplay(string decision)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare(decision);var receipt=JsonSerializer.Serialize(f.Original.Receipt);var preparation=f.Original.Receivable.PREPARATION_WORKFLOW_JSON;
        var result=await f.Execute();result.Require(f.Data.Command);Assert.Equal(decision=="APPROVE"?"APPROVED":"REJECTED",result.Receivable.STATUS);Assert.Equal("controller",result.Receivable.ROW_CHANGED_BY);Assert.Equal("reviewer",result.Receivable.ROW_CREATED_BY);
        Assert.Equal(preparation,result.Receivable.PREPARATION_WORKFLOW_JSON);Assert.Equal(receipt,JsonSerializer.Serialize(result.Receipt));Assert.Equal(f.Original.Receivable.TOTAL_AMOUNT,result.Receivable.TOTAL_AMOUNT);Assert.Equal(0m,result.Receivable.PAID_AMOUNT);
        Assert.Equal(2,f.Data.Invoice.Data.Data.Native.Writes);Assert.Equal(new[]{"begin","commit","begin","commit"},f.Data.Invoice.Data.Data.Native.Transactions);
        Assert.Equal(JsonSerializer.Serialize(f.Original.Receivable),JsonSerializer.Serialize(CompanySalesReceivableDecisionProof.OriginalDraft(f.Command.Company,result.Receivable)));
        var readCount=f.Reads;f.Data.Invoice.Advanced=true;var retry=await f.Execute();Assert.Equal(JsonSerializer.Serialize(result),JsonSerializer.Serialize(retry));Assert.Equal(2,f.Data.Invoice.Data.Data.Native.Writes);Assert.Equal(readCount,f.Reads);
        var preparationReplay=await f.Data.Owner.PrepareCompanySalesReceivableAsync(f.Data.Command,f.Guard,()=>throw new InvalidOperationException("Known original preparation must not execute again."));Assert.Equal(result.Receivable.STATUS,preparationReplay.Receivable.STATUS);Assert.Equal(2,f.Data.Invoice.Data.Data.Native.Writes);
        Assert.DoesNotContain(f.Data.Invoice.Data.Data.Native.Rows,row=>row.GetType().Name is "AR_PAYMENT" or "GL_ENTRY" or "JOURNAL_ENTRY");
    }
    [Theory][InlineData("self")][InlineData("finance")][InlineData("source")][InlineData("reason")][InlineData("decision")][InlineData("transport")]
    public async Task ReviewerCannotSelfApproveOrOmitExplicitDispositionsAndEvidence(string fault)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();var command=f.Command;
        if(fault=="self"){f.Actor="reviewer";command=command with{Review=await f.Current()};Assert.False(command.Review.IsIndependent);}
        if(fault=="finance")command=command with{FinancialDisposition="ISSUE"};if(fault=="source")command=command with{SourceEvidence=""};if(fault=="reason")command=command with{Reason=""};if(fault=="decision")command=command with{Decision="POST"};if(fault=="transport")command=command with{Review=command.Review with{Revision=new string('C',64)}};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Data.Owner.RecordCompanySalesReceivableDecisionAsync(command,f.Guard,f.Current));Assert.Equal(0,f.Data.Invoice.Data.Data.Native.Writes);
    }
    [Theory][InlineData("journal")][InlineData("update")][InlineData("corruption")][InlineData("authority")][InlineData("source")]
    public async Task FailedNativeDecisionRetainsOriginalReviewerCommandAndRollsBackReceivable(string fault)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();f.FailJournal=fault=="journal";f.FailUpdate=fault=="update";f.CorruptUpdate=fault=="corruption";
        f.OnUpdate=()=>{if(fault=="authority")f.Denied=true;if(fault=="source")f.Data.Invoice.Advanced=true;};
        if(fault=="authority")await Assert.ThrowsAsync<UnauthorizedAccessException>(f.Execute);else await Assert.ThrowsAsync<InvalidOperationException>(f.Execute);
        var stored=Assert.Single(f.Data.Invoice.Data.Data.Native.Rows.OfType<AR_INVOICE>());Assert.Equal("DRAFT",stored.STATUS);Assert.Null(stored.REVIEW_WORKFLOW_JSON);Assert.Equal(fault=="journal"?0:1,f.Data.Invoice.Data.Data.Native.Rows.OfType<SALES_RECEIVABLE_REVIEW_INTENT>().Count());
        f.FailJournal=f.FailUpdate=f.CorruptUpdate=f.Denied=false;f.Data.Invoice.Advanced=false;f.OnUpdate=null;var result=await f.Execute();Assert.Equal("APPROVED",result.Receivable.STATUS);
    }
    [Fact]public async Task UnknownCommitReconcilesOriginalDecisionWithoutAnotherMutation()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();f.Uncertain=true;await Assert.ThrowsAsync<InvalidOperationException>(f.Execute);Assert.Equal(2,f.Data.Invoice.Data.Data.Native.Writes);
        var known=await f.Execute();Assert.Equal("APPROVED",known.Receivable.STATUS);Assert.Equal(2,f.Data.Invoice.Data.Data.Native.Writes);
    }
    [Fact]public async Task MissingNativeReviewerJournalCannotRecreateCommittedDisposition()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();await f.Execute();f.Data.Invoice.Data.Data.Native.Rows.RemoveAll(row=>row is SALES_RECEIVABLE_REVIEW_INTENT);
        await Assert.ThrowsAsync<InvalidOperationException>(f.Execute);await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Data.Owner.GetCompanySalesReceivableOutcomeAsync(f.Command.Company,"reviewer",f.Command.Review.InvoiceId,f.Guard));Assert.Equal(2,f.Data.Invoice.Data.Data.Native.Writes);
    }
    [Theory][InlineData("paid")][InlineData("currency")][InlineData("maker")][InlineData("reviewer")][InlineData("date")][InlineData("receipt")]
    public async Task NativeDispositionProofRejectsFinancialAuditAndOriginalReceiptTampering(string fault)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();var result=await f.Execute();
        if(fault=="paid")result.Receivable.PAID_AMOUNT=1;if(fault=="currency")result.Receivable.CURRENCY_CODE="USD";if(fault=="maker")result.Receivable.ROW_CREATED_BY="other";if(fault=="reviewer")result.Receivable.ROW_CHANGED_BY="other";
        if(fault=="date"){var at=result.Receivable.ROW_CHANGED_DATE!.Value;result.Receivable.ROW_CHANGED_DATE=null;result.Receivable.ROW_CHANGED_DATE=DateTime.SpecifyKind(at,DateTimeKind.Unspecified);Assert.Equal(DateTimeKind.Unspecified,result.Receivable.ROW_CHANGED_DATE.Value.Kind);}
        if(fault=="receipt")result.Receipt.RECEIVABLE_JSON=JsonSerializer.Serialize(result.Receivable);
        Assert.Throws<InvalidOperationException>(()=>result.Require(f.Data.Command));
    }
    [Fact]public async Task PublicRecordedHeadersExposeNativeReviewerWithoutPrivateCommandsOrPosting()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();var outcome=await f.Execute();var parent=await f.Data.Invoice.Data.Data.Owner.GetCompanyInvoiceDraftForReviewAsync(f.Command.Company,f.Command.Review.InvoiceId,f.Guard,_=>f.Guard());
        var status=CompanySalesReceivableStatusReview.Create(f.Command.Company,"reader",new string('B',64),parent,[outcome]);var row=Assert.Single(status.Receivables);Assert.Equal("APPROVED",row.Status);Assert.Equal("controller",row.ReviewerId);Assert.NotNull(row.ReviewedUtc);Assert.Equal(f.Original.Receipt.RECORDED_REVISION,row.ReceiptRevision);
        var json=JsonSerializer.Serialize(status);Assert.DoesNotContain("REVIEW_WORKFLOW_JSON",json);Assert.DoesNotContain("COMMAND_JSON",json);Assert.DoesNotContain(f.Command.FinancialEvidence,json);Assert.Equal(f.Original.Receivable.TOTAL_AMOUNT,row.ProposedTotal);
    }
    [Fact]public void ReviewerJournalAndProofColumnHaveCanonicalBeepMetadataAndLegacyNullJson()
    {
        ModuleSchemaBoundary.Validate([typeof(AR_INVOICE),typeof(SALES_RECEIVABLE_REVIEW_INTENT)]);
        var schema=new ClassCreator(Mock.Of<TheTechIdea.Beep.Editor.IDMEEditor>()).ConvertToEntityStructure(typeof(AR_INVOICE));var field=Assert.Single(schema.Fields,value=>value.FieldName=="REVIEW_WORKFLOW_JSON");Assert.True(field.AllowDBNull);Assert.Equal(CompanySalesReceivableDecisionProof.MaximumLength,field.Size1);
        Assert.DoesNotContain("REVIEW_WORKFLOW_JSON",JsonSerializer.Serialize(new AR_INVOICE()));Assert.Equal(typeof(AR_INVOICE).Assembly,typeof(SALES_RECEIVABLE_REVIEW_INTENT).Assembly);
    }
    [Theory][InlineData("money")][InlineData("creator")][InlineData("reviewer")][InlineData("timezone")]
    public async Task CommonAuditCannotChangeFrozenMoneyMakerOrReviewerIdentity(string change)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();f.Audit=value=>{var row=(AR_INVOICE)value;if(change=="money")row.TOTAL_AMOUNT+=1;if(change=="creator")row.ROW_CREATED_BY="other";if(change=="reviewer")row.ROW_CHANGED_BY="other";if(change=="timezone")row.ROW_CHANGED_DATE=DateTime.SpecifyKind(DateTime.UtcNow,DateTimeKind.Unspecified);};
        await Assert.ThrowsAsync<InvalidOperationException>(f.Execute);Assert.Equal("DRAFT",Assert.Single(f.Data.Invoice.Data.Data.Native.Rows.OfType<AR_INVOICE>()).STATUS);Assert.Single(f.Data.Invoice.Data.Data.Native.Rows.OfType<SALES_RECEIVABLE_REVIEW_INTENT>());
    }
    [Theory][InlineData("proof")][InlineData("journal")][InlineData("key")]
    public async Task InsufficientReviewerMigrationMetadataRefusesBeforeAnyIntentWrite(string change)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();var native=f.Data.Invoice.Data.Data.Native;
        native.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(value=>value.EntityName=="AR_INVOICE" || value.EntityName=="SALES_RECEIVABLE_REVIEW_INTENT"),true)).Returns((EntityStructure request,bool refresh)=>{
            var schema=new ClassCreator(native.Editor.Object).ConvertToEntityStructure(request.EntityName=="AR_INVOICE"?typeof(AR_INVOICE):typeof(SALES_RECEIVABLE_REVIEW_INTENT));
            if(change=="proof" && request.EntityName=="AR_INVOICE"){var field=schema.Fields.Single(field=>field.FieldName=="REVIEW_WORKFLOW_JSON");field.Size=field.Size1=0;}
            if(change=="journal" && request.EntityName=="SALES_RECEIVABLE_REVIEW_INTENT")schema.Fields.RemoveAll(field=>field.FieldName=="SOURCE_REVISION");
            if(change=="key" && request.EntityName=="SALES_RECEIVABLE_REVIEW_INTENT")schema.Fields.Single(field=>field.IsKey).IsKey=false;return schema;});
        await Assert.ThrowsAsync<InvalidOperationException>(f.Execute);Assert.Equal(0,native.Writes);Assert.Empty(native.Rows.OfType<SALES_RECEIVABLE_REVIEW_INTENT>());
    }
    [Fact]public async Task FieldAccessPrecedesPrivateReceivableProofRestoration()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();Assert.Single(f.Data.Invoice.Data.Data.Native.Rows.OfType<AR_INVOICE>()).PREPARATION_WORKFLOW_JSON="malformed private proof";
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Data.Owner.GetCompanySalesReceivableForReviewAsync(f.Command.Company,f.Command.ReceivableId,f.Guard,_=>throw new UnauthorizedAccessException()));Assert.Equal(0,f.Data.Invoice.Data.Data.Native.Writes);
    }
    [Fact]public async Task LegacyReadRejectsEvenAnIsolatedReviewOwnershipMarker()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();var native=f.Data.Invoice.Data.Data.Native;
        native.Rows.Add(new AR_INVOICE{AR_INVOICE_ID="legacy-with-review-marker",STATUS="DRAFT",REVIEW_WORKFLOW_JSON="protected native review"});
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Data.Legacy.GetInvoiceAsync("legacy-with-review-marker",native.Connection));Assert.Equal(0,native.Writes);
    }
    [Theory][InlineData("receiving")][InlineData("policy")]
    public async Task AdvancedCurrentPhysicalOrPolicySourceCannotCreateReviewerReservation(string source)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();
        if(source=="receiving")f.Data.Invoice.Data.Data.Current.Delivery.Source.Source.Source.Rows[0].ROW_CHANGED_BY="advanced physical source";else f.Data.Invoice.Data.Data.Current.Policies.Records[0].ROW_CHANGED_BY="advanced policy source";
        await Assert.ThrowsAsync<InvalidOperationException>(f.Execute);Assert.Equal(0,f.Data.Invoice.Data.Data.Native.Writes);Assert.Empty(f.Data.Invoice.Data.Data.Native.Rows.OfType<SALES_RECEIVABLE_REVIEW_INTENT>());
    }
    [Fact]public async Task CompetingReviewerReservationAndPrivateRecoveryCannotReplaceOriginalBody()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();f.FailUpdate=true;await Assert.ThrowsAsync<InvalidOperationException>(f.Execute);
        var retained=await f.Data.Owner.GetCompanySalesReceivableReviewIntentAsync(f.Command.Company,f.Actor,f.Command.ReceivableId,f.Guard,_=>f.Guard());Assert.Equal(JsonSerializer.Serialize(f.Command),JsonSerializer.Serialize(retained!.Restore(f.Command.Company,f.Actor)));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Data.Owner.GetCompanySalesReceivableReviewIntentAsync(f.Command.Company,"other",f.Command.ReceivableId,f.Guard,_=>f.Guard()));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Data.Owner.RecordCompanySalesReceivableDecisionAsync(f.Command with{Reason="Replacement reason"},f.Guard,f.Current));Assert.Equal(2,f.Data.Invoice.Data.Data.Native.Writes);
    }
    private sealed class Authentication:AuthenticationStateProvider
    {public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,"reader"),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"test"))));}
    [Fact]public async Task ActualCompanyApiClientAndRoutedScreenShowReviewedNativeStatusWithoutPrivateCommands()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();await f.Execute();
        await using var host=await f.Data.Invoice.Data.Start(permissions:["Accounting.View"],configure:(services,context)=>{context.Actor="reader";services.AddSingleton<IARService>(f.Data.Owner);services.AddScoped<CompanySalesReceivableReads>();});
        host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor","reader");
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"reader",new(f.Command.Company,["ConfiguredCompanyRole"],["Accounting.View"]));var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,company);
        var status=await client.GetCompanySalesReceivableStatusAsync(f.Command.Review.InvoiceId);Assert.Equal("APPROVED",Assert.Single(status.Receivables).Status);Assert.Equal("controller",Assert.Single(status.Receivables).ReviewerId);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();
        services.AddSingleton(company);services.AddSingleton<IAccountingServiceClient>(client);services.AddScoped<CompanySalesReceivableWorkspaceState>();services.AddSingleton<AuthenticationStateProvider,Authentication>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var html=await renderer.Dispatcher.InvokeAsync(async()=>(await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.SalesReceivables>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Invoice",f.Command.Review.InvoiceId}}))).ToHtmlString());
        Assert.Contains(f.Command.ReceivableId,html);Assert.Contains("Independent reviewer controller",html);Assert.Contains("49.38",html);Assert.DoesNotContain(f.Command.FinancialEvidence,html);Assert.DoesNotContain(f.Command.Reason,html);Assert.Contains("Draft amounts do not establish recognized revenue",html);Assert.Equal(2,f.Data.Invoice.Data.Data.Native.Writes);
    }
}
#endif
