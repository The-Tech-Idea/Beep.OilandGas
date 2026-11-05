using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.ProductionAccounting.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Microsoft.Extensions.Logging.Abstractions;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class LegacyLedgerHttpAdmissionTests
{
    [Theory][InlineData("accounts")][InlineData("account")][InlineData("create-account")][InlineData("create-journal")][InlineData("post")][InlineData("journal")][InlineData("lines")]
    public void GlobalFacadeCannotQueryInsertFallbackFactsOrReportPosting(string operation)
    {
        var native=new CompanyGLAccountReviewTests.Fixture();var service=new ProductionAccountingService(native.Native.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),Mock.Of<IPPDMMetadataRepository>());
        var accounts=new ProductionAccountingService.GeneralLedgerCompatibility(service);var journals=new ProductionAccountingService.JournalEntryCompatibility(service);
        void Invoke(){switch(operation){case "accounts":accounts.GetAllAccounts();break;case "account":accounts.GetAccount("account");break;case "create-account":accounts.CreateAccount(new CreateGLAccountRequest(),"actor");break;
            case "create-journal":journals.CreateJournalEntry("number",new DateTime(2026,1,1),"MANUAL","Explicit description",[],"actor");break;case "post":journals.PostJournalEntry("journal","actor");break;case "journal":journals.GetJournalEntry("journal");break;case "lines":journals.GetJournalEntryLines("journal");break;}}
        Assert.Throws<InvalidOperationException>(Invoke);Assert.Equal(0,native.Reads);native.NoWrites();
    }
    [Theory][InlineData(true)][InlineData(false)]
    public async Task RetiredInventoryCreationCannotWriteStockBeforeChoosingDefaultLedgerAccounts(bool authenticate)
    {
        var activations=0;await using var host=await AccountingSourceHttpTests.Start(authenticate:authenticate,configureAccounting:(services,_)=>services.AddSingleton<ProductionAccountingService>(_=>{
            activations++;throw new InvalidOperationException("A retired inventory operation activated the global facade.");}));
        var payload=new Beep.OilandGas.Models.Data.Inventory.CreateInventoryTransactionRequest{InventoryItemId="item",TransactionType="Receipt",Quantity=10m,UnitCost=1m,Description="Explicit request",TransactionDate=new DateTime(2026,1,1)};
        foreach(var property in payload.GetType().GetProperties().Where(property=>property.CanWrite && property.PropertyType==typeof(string)))if(property.GetValue(payload) is null)property.SetValue(payload,"Explicit legacy value");
        using var response=await host.Client.PostAsJsonAsync("api/accounting/traditional/inventory/transactions?connectionName=foreign",payload);
        Assert.Equal(authenticate?HttpStatusCode.Gone:HttpStatusCode.Unauthorized,response.StatusCode);Assert.Equal(0,activations);Assert.Equal(0,host.Context.Reads);
    }
    [Theory][InlineData("header")][InlineData("lines")][InlineData("ledger")][InlineData("balance")][InlineData("post")][InlineData("reverse")][InlineData("reference")]
    public async Task SelectedCompanyJournalCannotBorrowLegacyHeadersLedgerBalancesOrPosting(string operation)
    {
        var native=new CompanyGLAccountReviewTests.Fixture();var journal=new JournalEntryService(native.Native.Editor.Object,new CommonColumnHandler(),Mock.Of<IPPDM39DefaultsRepository>(),Mock.Of<IPPDMMetadataRepository>(),native.Service,NullLogger<JournalEntryService>.Instance,()=>Task.FromResult(native.Native.Connection));
        async Task Invoke(){switch(operation){case "header":await journal.GetEntryByIdAsync("journal");break;case "lines":await journal.GetEntryLineItemsAsync("journal");break;
            case "ledger":await journal.GetEntriesByAccountAsync("account",new DateTime(2026,1,1),new DateTime(2026,12,31),"foreign");break;case "balance":await journal.GetAccountBalanceAsync("account",new DateTime(2026,10,12),"foreign");break;
            case "post":await journal.PostEntryAsync("journal","actor");break;case "reverse":await journal.ReverseEntryAsync("journal","Explicit reversal reason","actor");break;case "reference":await journal.GetPostingEvidenceAsync("reference");break;}}
        await Assert.ThrowsAsync<InvalidOperationException>(Invoke);Assert.Equal(0,native.Reads);native.NoWrites();
    }
    [Theory][InlineData("GET","accounts")][InlineData("GET","accounts/account")][InlineData("POST","accounts")]
    [InlineData("POST","journal-entries")][InlineData("POST","journal-entries/journal/post")][InlineData("GET","journal-entries/journal")]
    public async Task RetiredRoutesReturnMigrationGuidanceWithoutActivatingGlobalFacade(string method,string route)
    {
        var activations=0;await using var host=await AccountingSourceHttpTests.Start(configureAccounting:(services,_)=>services.AddSingleton<ProductionAccountingService>(_=>{
            activations++;throw new InvalidOperationException("The retired route activated a global facade.");}));
        using var request=new HttpRequestMessage(new HttpMethod(method),"api/accounting/traditional/gl/"+route+"?connectionName=foreign-production");
        if(method=="POST")request.Content=JsonContent.Create(new{AccountNumber="legacy",AccountName="Legacy request",AccountType="ASSET",NormalBalance="DEBIT",Description="Explicit request",Lines=Array.Empty<object>()});
        using var response=await host.Client.SendAsync(request);Assert.Equal(HttpStatusCode.Gone,response.StatusCode);
        var body=await response.Content.ReadAsStringAsync();Assert.Contains("companyless ledger operation is retired",body);Assert.Contains("api/accounting/traditional/gl/company-accounts",body);
        Assert.DoesNotContain("posted successfully",body);Assert.Equal(0,activations);Assert.Equal(0,host.Context.Reads);Assert.Equal(0,host.Context.Calls);
    }
    [Theory][InlineData("GET","accounts")][InlineData("GET","accounts/account")][InlineData("POST","accounts")][InlineData("POST","journal-entries")][InlineData("POST","journal-entries/journal/post")][InlineData("GET","journal-entries/journal")]
    public async Task UnauthenticatedLegacyRoutesChallengeBeforeAnyFacadeActivation(string method,string route)
    {
        var activations=0;await using var host=await AccountingSourceHttpTests.Start(authenticate:false,configureAccounting:(services,_)=>services.AddSingleton<ProductionAccountingService>(_=>{
            activations++;throw new InvalidOperationException("Authentication did not gate facade activation.");}));
        using var request=new HttpRequestMessage(new HttpMethod(method),"api/accounting/traditional/gl/"+route);if(method=="POST")request.Content=JsonContent.Create(new{});
        using var response=await host.Client.SendAsync(request);Assert.Equal(HttpStatusCode.Unauthorized,response.StatusCode);Assert.Equal(0,activations);Assert.Equal(0,host.Context.Reads);
    }
}
#endif
