using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
[Collection("CompanySalesJournalSource")]
public sealed class CompanySalesJournalSourceTests
{
    internal sealed class Fixture
    {
        public readonly CompanySalesReceivableDecisionTests.Fixture Receivable=new();
        public PersonaTaskRoutingTests.Fixture Native=>Receivable.Data.Invoice.Data.Data.Native;
        public JournalEntryService Journal=null!;
        public GLAccountService Accounts=null!;
        public AccountMappingService Mappings=null!;
        public PeriodClosingService Periods=null!;
        public CompanySalesReceivableReviewSource Current=null!;
        public CompanySalesJournalMappings Selection=new("company-ar","company-sales","company-charges","company-tax");
        public DateTime Date;
        public int Reads,AuthorityCalls,RevokeAt;
        public string? MappingConnection,PeriodConnection;
        public Action<string>? OnRead;
        public Func<Task<CompanySalesReceivableReviewSource>>? ReadCurrentOverride;
        public COMPANY_LEDGER_PERIOD Period=null!;
        public async Task Prepare(bool approve=true,bool taxAndCharges=false)
        {
            if(taxAndCharges){Receivable.Data.Invoice.ExplicitTax=2.345m;Receivable.Data.Invoice.ExplicitCharges=1.234m;}
            await Receivable.Prepare();if(approve)await Receivable.Execute();Current=await Receivable.Current();Date=Current.Original.Receivable.INVOICE_DATE!.Value;
            var company=Current.Company;var currency=Current.Original.Receivable.CURRENCY_CODE!;var columns=new CommonColumnHandler();var defaults=Mock.Of<IPPDM39DefaultsRepository>();var metadataMock=new Mock<IPPDMMetadataRepository>();metadataMock.Setup(value=>value.GetPrimaryKeyColumnsAsync(It.IsAny<string>())).ReturnsAsync((string table)=>new List<string>{table+"_ID"});var metadata=metadataMock.Object;
            Accounts=new(Native.Editor.Object,columns,defaults,metadata,NullLogger<GLAccountService>.Instance,()=>Task.FromResult(Native.Connection));
            var bookReview=CompanyLedgerBookReview.Create(company,"administrator",[]);var bookCommand=CompanyLedgerBookTests.Command(bookReview) with{FunctionalCurrency=currency,CalendarId="company-calendar",CurrencyDecimals=Current.CurrentInvoice.Original.Intent.Restore(company,Current.CurrentInvoice.Original.Intent.ROW_CREATED_BY).Review.ReviewAmounts(Current.CurrentInvoice.Original.Intent.Restore(company,Current.CurrentInvoice.Original.Intent.ROW_CREATED_BY).Facts).Rules.CurrencyDecimals};
            var bookRow=bookCommand.CreateRow();bookRow.ROW_CREATED_BY="administrator";bookRow.ROW_CREATED_DATE=DateTime.UtcNow;bookRow.CONFIGURATION_JSON=CompanyLedgerBookConfigurationProof.Create(bookCommand,bookRow);Native.Rows.Add(bookRow);
            Mappings=new(Native.Editor.Object,columns,defaults,metadata,NullLogger<AccountMappingService>.Instance,()=>Task.FromResult(MappingConnection??Native.Connection),Accounts);
            Journal=new(Native.Editor.Object,columns,defaults,metadata,Accounts,NullLogger<JournalEntryService>.Instance,()=>Task.FromResult(Native.Connection));var basis=new AccountingBasisPostingService(Journal);
            var ap=new APInvoiceService(Native.Editor.Object,columns,defaults,metadata,basis,NullLogger<APInvoiceService>.Instance,resolveConnection:()=>Task.FromResult(Native.Connection));
            var trial=new TrialBalanceService(Native.Editor.Object,columns,defaults,metadata,Accounts,NullLogger<TrialBalanceService>.Instance);
            Periods=new(Native.Editor.Object,columns,defaults,metadata,trial,Journal,basis,ap,Receivable.Data.Owner,NullLogger<PeriodClosingService>.Instance,resolveConnection:()=>Task.FromResult(PeriodConnection??Native.Connection));
            var from=new DateTime(Date.Year,Date.Month,1);var through=from.AddMonths(1).AddDays(-1);var empty=CompanyLedgerPeriodReview.Create(company,"administrator","statutory",currency,"company-calendar",[]);
            var command=new CompanyLedgerPeriodCreation(company,"administrator","statutory",currency,"company-calendar",Guid.NewGuid().ToString("N"),"explicit-period",from,through,Guid.NewGuid().ToString("N"),"Reviewed original period",empty.Revision,new string('B',64));
            Period=command.CreateRow();Period.ROW_CREATED_BY="administrator";Period.ROW_CREATED_DATE=DateTime.UtcNow;Period.AUTHORITY_JSON=CompanyLedgerPeriodAuthority.Create(command,Period);Native.Rows.Add(Period);
            void Account(string key,string id,string number,string type,string normal)
            {
                Native.Rows.Add(new GL_ACCOUNT{GL_ACCOUNT_ID=id,ACCOUNT_NUMBER=number,ACCOUNT_NAME=key,ACCOUNT_TYPE=type,NORMAL_BALANCE=normal,OWNER_TENANT_ID=company.TenantId,OWNER_COMPANY_BA_ID=company.BusinessAssociateId,BOOK_ID="statutory",CURRENCY_CODE=currency,ACTIVE_IND="Y",ROW_CREATED_BY="administrator",ROW_CREATED_DATE=DateTime.UtcNow});
                Native.Rows.Add(new GLAccountMapping{GL_ACCOUNT_MAPPING_ID="mapping-"+id,MAPPING_KEY=key,GL_ACCOUNT_ID=id,GL_ACCOUNT_NUMBER=number,OWNER_TENANT_ID=company.TenantId,OWNER_COMPANY_BA_ID=company.BusinessAssociateId,BOOK_ID="statutory",CURRENCY_CODE=currency,ACTIVE_IND="Y",EFFECTIVE_DATE=from,ROW_CREATED_BY="administrator",ROW_CREATED_DATE=DateTime.UtcNow});
            }
            Account(Selection.ReceivableKey,"ar-account","1110","ASSET","DEBIT");Account(Selection.RevenueKey,"revenue-account","4110","REVENUE","CREDIT");Account(Selection.ChargesKey!,"charges-account","4120","REVENUE","CREDIT");Account(Selection.TaxKey!,"tax-account","2110","LIABILITY","CREDIT");
            var types=Native.Rows.Select(row=>row.GetType()).Concat([typeof(JOURNAL_ENTRY),typeof(JOURNAL_ENTRY_LINE)]).Distinct().ToDictionary(type=>type==typeof(GLAccountMapping)?"GL_ACCOUNT_MAPPING":type.Name,type=>type);
            Native.Source.Setup(source=>source.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure request,bool refresh)=>new ClassCreator(Native.Editor.Object).ConvertToEntityStructure(types[request.EntityName]));
            Native.Source.Setup(source=>source.GetEntityAsync(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>{
                Reads++;OnRead?.Invoke(table);return Native.Rows.Where(row=>(row is GLAccountMapping?"GL_ACCOUNT_MAPPING":row.GetType().Name)==table && (table=="REVENUE_TRANSACTION"?filters.Where(filter=>filter.FieldName is "FIELD_ID" or "PROPERTY_ID"):filters).All(filter=>table=="FX_RATE"?CompanyFxRateTests.Matches(row,filter):row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).Select(row=>JsonSerializer.Deserialize(JsonSerializer.Serialize(row),row.GetType())!).ToArray();});
            Native.Writes=0;Native.Transactions.Clear();Reads=AuthorityCalls=0;
        }
        public Task Guard(){AuthorityCalls++;if(AuthorityCalls==RevokeAt)throw new UnauthorizedAccessException();return Receivable.Guard();}
        public Task<CompanySalesJournalSourceReview> Read()=>Journal.GetCompanySalesJournalSourceAsync(Current.Company,Current.ActorUserId,Current.ReceivableId,"statutory",Current.Original.Receivable.CURRENCY_CODE!,"company-calendar",Date,Selection,Receivable.Data.Owner,Mappings,Periods,ReadCurrentOverride??Receivable.Current,Guard,_=>Guard());
        public void NoWrites(){Assert.Equal(0,Native.Writes);Assert.Empty(Native.Transactions);Assert.DoesNotContain(Native.Source.Invocations,call=>call.Method.Name is "ExecuteSql" or "RunQuery");}
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task CompleteApprovedSourceProducesExactlyBalancedPurposeLinesAtOriginalCurrencyPrecision(bool taxAndCharges)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var fixture=new Fixture();await fixture.Prepare(taxAndCharges:taxAndCharges);var source=await fixture.Read();source.Require(fixture.Current.Company,fixture.Current.ActorUserId,fixture.Current.ReceivableId,"statutory","KWD","company-calendar",fixture.Date,fixture.Selection);
        Assert.Equal(taxAndCharges?4:2,source.Lines.Count);Assert.Equal("ar-account",source.Lines[0].AccountId);Assert.Equal(source.Receivable.Receivables.Single().ProposedTotal,source.Lines.Sum(line=>line.Debit));Assert.Equal(source.Lines.Sum(line=>line.Debit),source.Lines.Sum(line=>line.Credit));
        if(taxAndCharges){Assert.Equal(1.234m,source.Lines.Single(line=>line.Purpose=="SALES_CHARGES").Credit);Assert.Equal(2.345m,source.Lines.Single(line=>line.Purpose=="SALES_TAX").Credit);Assert.Equal("LIABILITY",source.Lines.Single(line=>line.Purpose=="SALES_TAX").AccountType);}
        fixture.NoWrites();Assert.DoesNotContain(fixture.Native.Rows,row=>row.GetType().Name is "JOURNAL_ENTRY" or "GL_ENTRY");
    }
    [Theory][InlineData("draft")][InlineData("period")][InlineData("mapping")][InlineData("account")][InlineData("currency")][InlineData("date")][InlineData("calendar")][InlineData("tax")][InlineData("missing-book")][InlineData("book-precision")]
    public async Task UnapprovedOrUnconfirmedOriginalFinancialConfigurationCannotSupplyJournalSource(string fault)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var fixture=new Fixture();await fixture.Prepare(approve:fault!="draft",taxAndCharges:fault=="tax");
        if(fault=="period")fixture.Native.Rows.Remove(fixture.Period);if(fault=="mapping")fixture.Selection=fixture.Selection with{RevenueKey="missing"};if(fault=="tax")fixture.Selection=fixture.Selection with{TaxKey=null};
        if(fault=="account")fixture.Native.Rows.OfType<GL_ACCOUNT>().Single(row=>row.GL_ACCOUNT_ID=="revenue-account").ACCOUNT_TYPE="LIABILITY";if(fault=="currency")fixture.Native.Rows.OfType<GL_ACCOUNT>().Single(row=>row.GL_ACCOUNT_ID=="ar-account").CURRENCY_CODE="USD";
        if(fault=="date")fixture.Date=DateTime.SpecifyKind(fixture.Date,DateTimeKind.Utc);if(fault=="calendar")fixture.Period.CALENDAR_ID="another-calendar";
        if(fault=="missing-book")fixture.Native.Rows.RemoveAll(row=>row is COMPANY_LEDGER_BOOK);
        if(fault=="book-precision"){var row=fixture.Native.Rows.OfType<COMPANY_LEDGER_BOOK>().Single();var command=CompanyLedgerBookConfigurationProof.Require(row).Command with{CurrencyDecimals=2};row.CURRENCY_DECIMALS=2;row.CONFIGURATION_JSON=CompanyLedgerBookConfigurationProof.Create(command,row);}
        await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Read());fixture.NoWrites();
    }
    [Theory][InlineData("mapping")][InlineData("period")]
    public async Task ASecondProductionBindingCannotLendItsAccountsOrPeriods(string owner)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var fixture=new Fixture();await fixture.Prepare();if(owner=="mapping")fixture.MappingConnection="other-production";else fixture.PeriodConnection="other-production";
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Read());Assert.Equal(0,fixture.Reads);fixture.NoWrites();
    }
    [Theory][InlineData("period")][InlineData("account")][InlineData("authority")]
    public async Task ClosingConfigurationOrAuthorityChangeCannotPublishOriginalPostingSource(string fault)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var fixture=new Fixture();await fixture.Prepare();var periods=0;fixture.OnRead=table=>{if(table!="COMPANY_LEDGER_PERIOD" || ++periods!=3)return;if(fault=="period")fixture.Period.PERIOD_CODE="changed";if(fault=="account")fixture.Native.Rows.OfType<GL_ACCOUNT>().Single(row=>row.GL_ACCOUNT_ID=="revenue-account").ACCOUNT_NAME="changed";if(fault=="authority")fixture.RevokeAt=fixture.AuthorityCalls+1;};
        await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Read());fixture.NoWrites();
    }
    [Fact] public async Task TransportTamperingCannotChangePurposeAmountsOrOriginalReceiptEvidence()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var fixture=new Fixture();await fixture.Prepare();var source=await fixture.Read();var lines=source.Lines.ToArray();lines[0]=lines[0] with{Debit=lines[0].Debit+.001m};var changed=source with{Lines=lines};
        Assert.Throws<InvalidOperationException>(()=>changed.Require(source.Company,source.ActorUserId,source.ReceivableId,source.BookId,source.CurrencyCode,source.CalendarId,source.PostingDate,source.Selection));source=source with{Receivable=source.Receivable with{Receivables=[source.Receivable.Receivables.Single() with{Status="POSTED"}]}};
        Assert.ThrowsAny<Exception>(()=>source.Require(source.Company,source.ActorUserId,source.ReceivableId,source.BookId,source.CurrencyCode,source.CalendarId,source.PostingDate,source.Selection));fixture.NoWrites();
    }
    [Theory][InlineData("SALES_RECEIVABLE_INTENT")][InlineData("SALES_RECEIVABLE_OUTCOME")][InlineData("SALES_RECEIVABLE_REVIEW_INTENT")]
    public async Task CachedValidCallbackCannotLendAMissingActualNativePreparationOutcomeOrReviewerArtifact(string table)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var fixture=new Fixture();await fixture.Prepare();fixture.ReadCurrentOverride=()=>Task.FromResult(CompanyInventorySourceReview.Copy(fixture.Current));fixture.Native.Rows.RemoveAll(row=>row.GetType().Name==table);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Read());fixture.NoWrites();
    }
    [Fact] public async Task MissingBookAndMismatchedFunctionalPrecisionCannotLendAnApprovedPostingSource()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var fixture=new Fixture();await fixture.Prepare();var row=fixture.Native.Rows.OfType<COMPANY_LEDGER_BOOK>().Single();fixture.Native.Rows.Remove(row);await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Read());
        var command=CompanyLedgerBookConfigurationProof.Require(row).Command with{CurrencyDecimals=2};row.CURRENCY_DECIMALS=2;row.CONFIGURATION_JSON=CompanyLedgerBookConfigurationProof.Create(command,row);fixture.Native.Rows.Add(row);await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Read());fixture.NoWrites();
    }
}
#endif


