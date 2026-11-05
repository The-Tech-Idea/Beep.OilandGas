using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.DevelopmentPlanning.Models;

public sealed record DevelopmentAfeFacts(
    [property:JsonRequired]string AfeId,[property:JsonRequired]string Number,[property:JsonRequired]string Name,[property:JsonRequired]string BaselineId,
    [property:JsonRequired]string BookId,[property:JsonRequired]string CurrencyCode,[property:JsonRequired]int CurrencyDecimals,
    [property:JsonRequired]decimal RequestedAmount,[property:JsonRequired]DateOnly EffectiveDate,[property:JsonRequired]string Status,
    [property:JsonRequired]string CreatedBy,[property:JsonRequired]DateTimeOffset CreatedAtUtc,[property:JsonRequired]string RecordedRevision)
{
    public void Require()
    {
        if(new[]{AfeId,Number,BaselineId,BookId,CreatedBy}.Any(value=>!LeaseSummaryValidator.Identifier(value)) || string.IsNullOrWhiteSpace(Name) || Name!=Name.Trim() || Name.Any(char.IsControl) || Name.Length>256 ||
            CurrencyCode is not {Length:3} || CurrencyCode.Any(c=>c is <'A' or >'Z') || CurrencyDecimals is <0 or >8 || RequestedAmount<0 ||
            decimal.Round(RequestedAmount,CurrencyDecimals)!=RequestedAmount || EffectiveDate.Year<1900 || Status!="DRAFT" || CreatedAtUtc==default || CreatedAtUtc.Offset!=TimeSpan.Zero || !ProcessDefinitionRevision.IsValid(RecordedRevision))
            throw new InvalidOperationException("Complete recorded draft AFE identity, explicit book/currency/precision, calendar date and actual audit are required.");
    }
}
public sealed record DevelopmentAfeReview(
    [property:JsonRequired]DevelopmentPlanBaselineReview Source,[property:JsonRequired]AssetDatabaseScope BudgetDatabase,
    [property:JsonRequired]CompanyLedgerBookReview Books,[property:JsonRequired]IReadOnlyList<DevelopmentAfeFacts> Afes,
    [property:JsonRequired]string Revision,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public const int MaximumAfes=128;
    public void Require()
    {
        if(Source is null || Source.Baseline is null || Source.Review is null || Books is null || BudgetDatabase is null ||
            !LeaseSummaryValidator.Identifier(BudgetDatabase.ConnectionName) || !LeaseSummaryValidator.Identifier(BudgetDatabase.Fingerprint) || Afes is null || Afes.Count>MaximumAfes || Afes.Any(row=>row is null) ||
            Afes.Select(row=>row.AfeId).Distinct(StringComparer.Ordinal).Count()!=Afes.Count || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("An approved FDP baseline, configured company books and complete bounded AFE population are required.");
        Source.Require(Source.Review);Books.Require(Source.Review.Scope.Company,Source.Review.Scope.ActorUserId);
        foreach(var row in Afes){row.Require();if(row.BaselineId!=Source.Baseline.FDP_ID)throw new InvalidOperationException("An AFE belongs to another approved FDP baseline.");}
        if(!ProcessDefinitionRevision.IsValid(Revision) || Revision!=ContentRevision())throw new InvalidOperationException("The approved source, books or AFE population changed.");
    }
    public string ContentRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{SourceScope=Source.Review.Scope,SourceEvidence=DevelopmentPlanRelease.ReviewEvidenceRevision(Source.Review),Source.Baseline,BudgetDatabase,Books=Books.Books,Afes=Afes.OrderBy(row=>row.AfeId,StringComparer.Ordinal)}));
    public DevelopmentAfeReview Without(string afeId)
    {
        Require();var copy=this with{Afes=Afes.Where(row=>row.AfeId!=afeId).ToArray()};return copy with{Revision=copy.ContentRevision()};
    }
}
public sealed record DevelopmentAfeCommand(
    [property:JsonRequired]DevelopmentAfeReview Review,[property:JsonRequired]Guid IntentId,[property:JsonRequired]string BookId,
    [property:JsonRequired]string Number,[property:JsonRequired]string Name,[property:JsonRequired]decimal Amount,
    [property:JsonRequired]DateOnly EffectiveDate,[property:JsonRequired]string Rationale)
{
    public const int MaximumBodyLength=4*1024*1024;
    public CompanyLedgerBookFacts Book()=>Review.Books.Books.Single(book=>book.BookId==BookId);
    public void Require()
    {
        if(Review is null)throw new InvalidOperationException("The original reviewed approved FDP and company books are required.");Review.Require();
        var book=Review.Books.Books.SingleOrDefault(row=>row.BookId==BookId);
        if(IntentId==Guid.Empty || book is null || new[]{Number,Name}.Any(value=>string.IsNullOrWhiteSpace(value) || value!=value.Trim() || value.Any(char.IsControl)) || Number.Length>128 || Name.Length>256 ||
            Amount<0 || decimal.Round(Amount,book.CurrencyDecimals)!=Amount || EffectiveDate.Year<1900 || string.IsNullOrWhiteSpace(Rationale) || Rationale!=Rationale.Trim() || Rationale.Length>4096 ||
            Rationale.Any(c=>char.IsControl(c) && c is not ('\r' or '\n' or '\t')) || Review.Afes.Any(row=>row.BookId==BookId) || JsonSerializer.Serialize(this).Length>MaximumBodyLength)
            throw new InvalidOperationException("Choose an exact company book and enter the first baseline/book AFE number, name, amount at recorded precision, calendar date and rationale.");
    }
    public string AfeId(){Require();return IdFor(Review.Source.Review.Scope.Company,Review.Source.Baseline!.FDP_ID,BookId);}
    public static string IdFor(CompanyScope company,string baseline,string book)=>"fdp-afe-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=company,Baseline=baseline,Book=book}));
    public string Revision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this));}
    public void RequireCurrent(DevelopmentAfeReview current)
    {
        Require();current.Require();if(current.Without(AfeId()).Revision!=Review.Revision)throw new InvalidOperationException("The original approved baseline, company books or AFE membership changed. Recover the original request.");
    }
    public AFE ToRecord(DateTime utc)
    {
        Require();if(utc==default || utc.Kind!=DateTimeKind.Utc)throw new InvalidOperationException("Actual AFE request producer UTC is required.");var scope=Review.Source.Review.Scope;
        var row=new AFE{AFE_ID=AfeId(),AFE_NUMBER=Number,AFE_NAME=Name,OWNER_TENANT_ID=scope.Company.TenantId,OWNER_COMPANY_BA_ID=scope.Company.BusinessAssociateId,
            FIELD_ID=scope.FieldId,FDP_BASELINE_ID=Review.Source.Baseline!.FDP_ID,BOOK_ID=BookId,CURRENCY_CODE=Book().FunctionalCurrency,ESTIMATED_COST=Amount,STATUS="DRAFT",DESCRIPTION=Rationale,
            ROW_EFFECTIVE_DATE=EffectiveDate.ToDateTime(TimeOnly.MinValue),ACTIVE_IND="Y",SOURCE="COMPANY_DEVELOPMENT_BUDGET",PPDM_GUID=IntentId.ToString("D"),ROW_CREATED_BY=scope.ActorUserId,
            ROW_CHANGED_BY=scope.ActorUserId,ROW_CREATED_DATE=utc,ROW_CHANGED_DATE=utc,DEVELOPMENT_BUDGET_JSON=JsonSerializer.Serialize(this)};
        row.DEVELOPMENT_BUDGET_REVISION=Digest(row);return row;
    }
    public static string Digest(AFE row){var copy=JsonSerializer.Deserialize<AFE>(JsonSerializer.Serialize(row))!;copy.DEVELOPMENT_BUDGET_REVISION=null;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static DevelopmentAfeCommand FromRecord(AFE row)
    {
        if(row?.DEVELOPMENT_BUDGET_JSON is not {Length:>0 and <=MaximumBodyLength} json)throw new InvalidOperationException("The bounded original development AFE proof is unavailable.");
        var original=JsonSerializer.Deserialize<DevelopmentAfeCommand>(json)??throw new InvalidOperationException("The original development AFE request is invalid.");original.Require();
        var actual=JsonSerializer.Deserialize<AFE>(JsonSerializer.Serialize(row))!;
        if(actual.ROW_CREATED_DATE is null || actual.ROW_CHANGED_DATE is null || actual.ROW_EFFECTIVE_DATE is null ||
            actual.ROW_CREATED_DATE.Value.Kind==DateTimeKind.Local || actual.ROW_CHANGED_DATE.Value.Kind==DateTimeKind.Local || actual.ROW_EFFECTIVE_DATE.Value.Kind==DateTimeKind.Local || actual.ROW_EFFECTIVE_DATE.Value.TimeOfDay!=TimeSpan.Zero)
            throw new InvalidOperationException("The original AFE UTC audit and exact calendar date are unavailable.");
        actual.ROW_CREATED_DATE=DateTime.SpecifyKind(actual.ROW_CREATED_DATE.Value,DateTimeKind.Utc);actual.ROW_CHANGED_DATE=DateTime.SpecifyKind(actual.ROW_CHANGED_DATE.Value,DateTimeKind.Utc);
        actual.ROW_EFFECTIVE_DATE=DateTime.SpecifyKind(actual.ROW_EFFECTIVE_DATE.Value,DateTimeKind.Unspecified);
        // Digest the normalized native audit representation before matching the original complete row.
        actual.DEVELOPMENT_BUDGET_REVISION=Digest(actual);
        var expected=original.ToRecord(actual.ROW_CREATED_DATE.Value);
        if(row.DEVELOPMENT_BUDGET_REVISION!=expected.DEVELOPMENT_BUDGET_REVISION || JsonSerializer.Serialize(actual)!=JsonSerializer.Serialize(expected))
            throw new InvalidOperationException("The canonical development AFE differs from its original source/book/currency/amount/intent/audit.");return original;
    }
    public static DevelopmentAfeFacts Facts(AFE row)
    {
        var original=FromRecord(row);var facts=new DevelopmentAfeFacts(row.AFE_ID,row.AFE_NUMBER,row.AFE_NAME,row.FDP_BASELINE_ID!,row.BOOK_ID!,row.CURRENCY_CODE!,original.Book().CurrencyDecimals,
            row.ESTIMATED_COST!.Value,original.EffectiveDate,row.STATUS,row.ROW_CREATED_BY!,new DateTimeOffset(DateTime.SpecifyKind(row.ROW_CREATED_DATE!.Value,DateTimeKind.Utc)),row.DEVELOPMENT_BUDGET_REVISION!);facts.Require();return facts;
    }
}
public sealed record DevelopmentAfeReceipt([property:JsonRequired]string IntentRevision,[property:JsonRequired]AFE? Recorded,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public void Require(DevelopmentAfeCommand original)
    {
        original.Require();if(IntentRevision!=original.Revision() || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero || Recorded is not null && DevelopmentAfeCommand.FromRecord(Recorded).Revision()!=original.Revision())
            throw new InvalidOperationException("The development AFE outcome does not confirm its retained original request.");
    }
}
