using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.Accounting;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Beep.OilandGas.Models.Data.Accounting;

/// <summary>Explicit configured mapping purposes; no global account or logical key is inferred.</summary>
public sealed record CompanySalesJournalMappings(string ReceivableKey,string RevenueKey,string? ChargesKey,string? TaxKey)
{
    public void Require(decimal charges,decimal tax)
    {
        BillingPolicyValues.Text(ReceivableKey,128);BillingPolicyValues.Text(RevenueKey,128);
        if(charges>0)BillingPolicyValues.Text(ChargesKey!,128);else if(ChargesKey is not null)BillingPolicyValues.Text(ChargesKey,128);
        if(tax>0)BillingPolicyValues.Text(TaxKey!,128);else if(TaxKey is not null)BillingPolicyValues.Text(TaxKey,128);
    }
}
public sealed record CompanySalesJournalSourceLine(int Number,string Purpose,string MappingKey,string AccountId,string AccountNumber,string AccountType,string NormalBalance,decimal Debit,decimal Credit);

/// <summary>Bounded native contributors retain exact financial facts and complete receipt/source revisions without recursively embedding private command histories. This is not a journal or posting.</summary>
public sealed record CompanySalesJournalSourceReview(CompanySalesReceivableStatusReview Receivable,string ReceivableSourceRevision,string InvoiceSourceRevision,string PhysicalSourceRevision,
    CompanyBillingPolicyRecord Policy,decimal SalesAmount,decimal ChargesAmount,decimal TaxAmount,CompanyGLMappingReview Mappings,CompanyLedgerPeriodSource Periods,string PeriodAuthorityRevision,
    DateTime PostingDate,CompanySalesJournalMappings Selection,IReadOnlyList<CompanySalesJournalSourceLine> Lines,CompanyLedgerBookReview Books,[property:JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] CompanyFxConversion? Conversion,string Revision)
{
    public const int MaximumSourceLength=4194304;
    [JsonIgnore]public CompanyScope Company=>Receivable.Company;
    [JsonIgnore]public string ActorUserId=>Receivable.ActorUserId;
    [JsonIgnore]public string ReceivableId=>Receivable.Receivables.Single().Id;
    [JsonIgnore]public string FieldId=>Receivable.FieldId;
    [JsonIgnore]public string BindingRevision=>Receivable.BindingRevision;
    [JsonIgnore]public string BookId=>Mappings.Accounts.BookId;
    [JsonIgnore]public string CurrencyCode=>Mappings.Accounts.CurrencyCode;
    [JsonIgnore]public string CalendarId=>Periods.CalendarId;
    [JsonIgnore]public string SourceCurrencyCode=>Receivable.Invoice.Currency;
    [JsonIgnore]public decimal FunctionalTotal=>Conversion?.TargetTotal ?? Receivable.Receivables.Single().ProposedTotal;
    public static CompanySalesJournalSourceReview Create(CompanySalesReceivableReviewSource native,CompanyGLMappingReview mappings,CompanyLedgerPeriodReview periods,DateTime date,CompanySalesJournalMappings selection,CompanyLedgerBookReview books,CompanyFxRateReview? rates=null)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();ArgumentNullException.ThrowIfNull(native);ArgumentNullException.ThrowIfNull(mappings);ArgumentNullException.ThrowIfNull(periods);ArgumentNullException.ThrowIfNull(selection);
        native.Require(native.Company,native.ActorUserId,native.ReceivableId);
        if(native.Original.Receivable.STATUS!="APPROVED" || !native.HasCurrentOriginalSource || native.CurrentInvoice.Original.Invoice.STATUS!="APPROVED" || !native.CurrentInvoice.HasCurrentOriginalSource)throw new InvalidOperationException("Original independently approved receivable/invoice and current physical/policy evidence are required before journal preparation.");
        _=CompanySalesReceivableDecisionProof.Require(native.Company,native.Original.Receivable);
        var invoice=native.CurrentInvoice;var command=invoice.Original.Intent.Restore(native.Company,invoice.Original.Intent.ROW_CREATED_BY);var amounts=command.Review.ReviewAmounts(command.Facts);var row=native.Original.Receivable;
        if(row.CURRENCY_CODE!=amounts.Rules.CurrencyCode || row.TOTAL_AMOUNT!=amounts.Total || row.PAID_AMOUNT!=0 || row.BALANCE_DUE!=amounts.Total)throw new InvalidOperationException("Exact original unposted receivable money and currency are required; conversion and settled sources are unavailable.");
        var policy=CompanyBillingPolicyProof.Require(native.Company,invoice.Current.Policies.RequireApplicable(row.CURRENCY_CODE!,command.Facts.InvoiceDate)).Record;
        mappings.Require(native.Company,native.ActorUserId,mappings.Accounts.BookId,mappings.Accounts.CurrencyCode,date);periods.Require(native.Company,native.ActorUserId,mappings.Accounts.BookId,mappings.Accounts.CurrencyCode,periods.CalendarId);_=periods.RequireOpen(date);
        CompanyFxConversion? conversion=null;if(row.CURRENCY_CODE!=mappings.Accounts.CurrencyCode){if(rates is null)throw new InvalidOperationException("Foreign posting requires the actual recorded company rate source.");rates.Require(native.Company,native.ActorUserId,mappings.Accounts.BookId,row.CURRENCY_CODE!,date);if(JsonSerializer.Serialize(rates.Books)!=JsonSerializer.Serialize(books))throw new InvalidOperationException("The original functional book and rate source differ.");conversion=CompanyFxConversion.Create(rates,policy.Rules.CurrencyDecimals,row.TOTAL_AMOUNT!.Value,amounts.RoundedPrice,command.Facts.ChargesAmount!.Value,command.Facts.TaxAmount!.Value);}else if(rates is not null)throw new InvalidOperationException("Same-currency journals cannot borrow a foreign rate.");
        var header=CompanySalesReceivableStatusReview.Create(native.Company,native.ActorUserId,native.BindingRevision,invoice.Original,[native.Original]);
        var source=new CompanySalesJournalSourceReview(header,native.Revision,invoice.Revision,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(invoice.Current.Delivery)),CompanyInventorySourceReview.Copy(policy),amounts.RoundedPrice,command.Facts.ChargesAmount!.Value,command.Facts.TaxAmount!.Value,mappings,CompanyLedgerPeriodSource.FromNative(periods),periods.Revision,date,selection,[],CompanyInventorySourceReview.Copy(books),conversion,string.Empty);
        source=source with{Lines=source.ExpectedLines()};source=source with{Revision=Digest(source)};source.Require(source.Company,source.ActorUserId,source.ReceivableId,source.BookId,source.CurrencyCode,source.CalendarId,date,selection);var frozen=CompanyInventorySourceReview.Copy(source);frozen.Require(source.Company,source.ActorUserId,source.ReceivableId,source.BookId,source.CurrencyCode,source.CalendarId,date,selection);return frozen;
    }
    public void Require(CompanyScope company,string actor,string receivable,string book,string currency,string calendar,DateTime date,CompanySalesJournalMappings selection)
    {
        if(Receivable is null || Mappings?.Accounts is null || Periods is null || Policy is null || Books is null || Selection is null || Lines is null || Lines.Count is <2 or >4 || Lines.Any(line=>line is null))throw new InvalidOperationException("The complete approved sales journal source is unavailable.");
        Policy.Require();Books.Require(company,actor);var configuredBook=Books.RequireBook(book,currency,calendar);
        if(Receivable.Invoice.Currency==currency){if(Conversion is not null || configuredBook.CurrencyDecimals!=Policy.Rules.CurrencyDecimals)throw new InvalidOperationException("Same-currency book precision differs or borrows foreign conversion evidence.");}
        else{if(Conversion?.Rates is null)throw new InvalidOperationException("The original foreign-currency rate/conversion evidence is unavailable.");Conversion.Rates.Require(company,actor,book,Receivable.Invoice.Currency,date);if(JsonSerializer.Serialize(Conversion.Rates.Books)!=JsonSerializer.Serialize(Books))throw new InvalidOperationException("The functional book source changed within the conversion.");Conversion.Require(Conversion.Rates,Policy.Rules.CurrencyDecimals,Receivable.Receivables.Single().ProposedTotal,SalesAmount,ChargesAmount,TaxAmount);}
        Receivable.Require(company,actor,Receivable.Invoice.InvoiceId);Mappings.Require(company,actor,book,currency,date);Periods.Require(company,actor,book,currency,calendar);Policy.Require();CompanyGLMappingReview.RequireDate(date);
        foreach(var revision in new[]{ReceivableSourceRevision,InvoiceSourceRevision,PhysicalSourceRevision,PeriodAuthorityRevision})BillingPolicyValues.Revision(revision);
        if(Receivable.Receivables.Count!=1 || Receivable.Receivables[0].Id!=receivable || Receivable.Receivables[0].Status!="APPROVED" || Receivable.Invoice.Status!="APPROVED" || Policy.Company!=company || Policy.ApprovedBy is null || Policy.Rules.CurrencyCode!=Receivable.Invoice.Currency ||
            Receivable.Invoice.InvoiceDate<Policy.Rules.EffectiveFrom || Policy.Rules.EffectiveThrough is {} end && Receivable.Invoice.InvoiceDate>end || PostingDate!=date || Selection!=selection || JsonSerializer.Serialize(Lines)!=JsonSerializer.Serialize(ExpectedLines()) || Revision!=Digest(this))throw new InvalidOperationException("The complete original journal source, explicit purposes or balanced financial facts changed.");
        if(JsonSerializer.Serialize(this).Length>MaximumSourceLength)throw new InvalidOperationException("The bounded complete journal source exceeds its declared capacity.");
    }
    private IReadOnlyList<CompanySalesJournalSourceLine> ExpectedLines()
    {
        if(Receivable?.Invoice is null || Receivable.Receivables is not {Count:1} || Mappings?.Accounts is null || Periods is null || Policy is null || Selection is null)throw new InvalidOperationException("Complete source, explicit account purposes and recorded period facts are required.");
        var rules=Policy.Rules;rules.Require();rules.RequireExactAmount(SalesAmount);rules.RequireExactAmount(ChargesAmount);rules.RequireExactAmount(TaxAmount);var row=Receivable.Receivables.Single();rules.RequireExactAmount(row.ProposedTotal);
        var period=Periods.Periods.SingleOrDefault(value=>value.From<=PostingDate && value.Through>=PostingDate);if(period is null || period.Status!="OPEN")throw new InvalidOperationException("The recorded original company book period is not open.");
        Selection.Require(ChargesAmount,TaxAmount);var lines=new List<CompanySalesJournalSourceLine>();
        void Add(string purpose,string key,string type,string normal,decimal amount,bool debit)
        {if(amount<0 || debit && amount==0)throw new InvalidOperationException("Journal source amounts must be positive on their recorded side.");var account=Mappings.Resolve(key,type,normal);if(amount==0)return;lines.Add(new(lines.Count+1,purpose,key,account.GL_ACCOUNT_ID,account.ACCOUNT_NUMBER,account.ACCOUNT_TYPE,account.NORMAL_BALANCE,debit?amount:0,debit?0:amount));}
        decimal Functional(string purpose,decimal source)=>Conversion is null?source:Conversion.Components.Single(value=>value.Purpose==purpose).TargetAmount;
        Add("RECEIVABLE",Selection.ReceivableKey,"ASSET","DEBIT",FunctionalTotal,true);Add("SALES_REVENUE",Selection.RevenueKey,"REVENUE","CREDIT",Functional("SALES_REVENUE",SalesAmount),false);
        if(ChargesAmount>0)Add("SALES_CHARGES",Selection.ChargesKey!,"REVENUE","CREDIT",Functional("SALES_CHARGES",ChargesAmount),false);if(TaxAmount>0)Add("SALES_TAX",Selection.TaxKey!,"LIABILITY","CREDIT",Functional("SALES_TAX",TaxAmount),false);
        BigInteger Units(decimal value){var bits=decimal.GetBits(value);var coefficient=(BigInteger)(uint)bits[0]+((BigInteger)(uint)bits[1]<<32)+((BigInteger)(uint)bits[2]<<64);var scale=(bits[3]>>16)&255;
            var decimals=Books.RequireBook(BookId,CurrencyCode,CalendarId).CurrencyDecimals;if(scale<=decimals)return coefficient*BigInteger.Pow(10,decimals-scale);var divisor=BigInteger.Pow(10,scale-decimals);if(coefficient%divisor!=0)throw new InvalidOperationException("Journal currency amounts cannot be rounded implicitly.");return coefficient/divisor;}
        if(lines.Aggregate(BigInteger.Zero,(total,line)=>total+Units(line.Debit))!=lines.Aggregate(BigInteger.Zero,(total,line)=>total+Units(line.Credit)))throw new InvalidOperationException("Exact journal debits and credits must balance at the original currency precision.");return lines;
    }
    private static string Digest(CompanySalesJournalSourceReview source)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(source with{Revision=string.Empty}));
}
