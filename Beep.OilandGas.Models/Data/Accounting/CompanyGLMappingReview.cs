using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Models.Data.Accounting;

/// <summary>Complete active mapping history and current account facts for one explicit company calendar date.</summary>
public sealed record CompanyGLMappingReview(CompanyGLAccountReview Accounts,DateTime AsOfDate,IReadOnlyList<GLAccountMapping> Records,string Revision)
{
    public const int MaximumMappings=512;
    public static CompanyGLMappingReview Create(CompanyGLAccountReview accounts,DateTime date,IEnumerable<GLAccountMapping> records)
    {
        ArgumentNullException.ThrowIfNull(accounts);ArgumentNullException.ThrowIfNull(records);var rows=records.Take(MaximumMappings+1).ToArray();
        if(rows.Length>MaximumMappings || rows.Any(row=>row is null))throw new InvalidOperationException("The complete bounded company account mapping history is unavailable.");
        var result=new CompanyGLMappingReview(accounts,date,rows.OrderBy(row=>row.GL_ACCOUNT_MAPPING_ID,StringComparer.Ordinal).ToArray(),string.Empty);
        result=result with{Revision=Digest(result)};result.Require(accounts.Company,accounts.ActorUserId,accounts.BookId,accounts.CurrencyCode,date);
        var frozen=CompanyInventorySourceReview.Copy(result);frozen.Require(accounts.Company,accounts.ActorUserId,accounts.BookId,accounts.CurrencyCode,date);return frozen;
    }
    public void Require(CompanyScope company,string actor,string book,string currency,DateTime date)
    {
        RequireDate(date);if(Accounts is null)throw new InvalidOperationException("The complete original account review is unavailable.");Accounts.Require(company,actor,book,currency);
        if(AsOfDate!=date || AsOfDate.Kind!=date.Kind || Records is null || Records.Count>MaximumMappings || Records.Any(row=>row is null) ||
            Records.Select(row=>row.GL_ACCOUNT_MAPPING_ID).Distinct(StringComparer.Ordinal).Count()!=Records.Count ||
            !Records.Select(row=>row.GL_ACCOUNT_MAPPING_ID).SequenceEqual(Records.Select(row=>row.GL_ACCOUNT_MAPPING_ID).Order(StringComparer.Ordinal)))throw new InvalidOperationException("The complete original company account mapping review is unconfirmed.");
        foreach(var row in Records)
        {
            BillingPolicyValues.Text(row.GL_ACCOUNT_MAPPING_ID,128);BillingPolicyValues.Text(row.MAPPING_KEY,128);BillingPolicyValues.Text(row.GL_ACCOUNT_ID!,128);BillingPolicyValues.Text(row.GL_ACCOUNT_NUMBER,128);
            CompanyGLAccountReview.RequireAudit(row);
            if(row.OWNER_TENANT_ID!=company.TenantId || row.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || row.BOOK_ID!=book || row.CURRENCY_CODE!=currency || row.ACTIVE_IND!="Y" || row.EFFECTIVE_DATE is not {} start)
                throw new InvalidOperationException("A mapping lacks exact company/book/currency ownership or an explicit effective date.");
            RequireDate(start);if(row.EXPIRY_DATE is {} end){RequireDate(end);if(end<start)throw new InvalidOperationException("The recorded mapping date range is invalid.");}
            var account=Accounts.Accounts.SingleOrDefault(account=>account.GL_ACCOUNT_ID==row.GL_ACCOUNT_ID);
            if(account is not null && account.ACCOUNT_NUMBER!=row.GL_ACCOUNT_NUMBER || Effective(row,date) && account is null)
                throw new InvalidOperationException("An effective mapping must resolve its actual native account ID and recorded number in the original company book/currency.");
        }
        foreach(var group in Records.GroupBy(row=>row.MAPPING_KEY,StringComparer.Ordinal))
        {
            var ordered=group.OrderBy(row=>row.EFFECTIVE_DATE).ToArray();
            for(var index=1;index<ordered.Length;index++)
                if(ordered[index-1].EXPIRY_DATE is not {} end || end>=ordered[index].EFFECTIVE_DATE)
                    throw new InvalidOperationException("Recorded mapping versions overlap; an account cannot be selected by order or default.");
        }
        if(Revision!=Digest(this))throw new InvalidOperationException("The complete recorded account/mapping review changed.");
    }
    public GL_ACCOUNT Resolve(string key,string accountType,string normalBalance)
    {
        if(Accounts is null)throw new InvalidOperationException("The complete original account review is unavailable.");
        Require(Accounts.Company,Accounts.ActorUserId,Accounts.BookId,Accounts.CurrencyCode,AsOfDate);BillingPolicyValues.Text(key,128);
        if(accountType is not ("ASSET" or "LIABILITY" or "EQUITY" or "REVENUE" or "EXPENSE") || normalBalance is not ("DEBIT" or "CREDIT"))throw new InvalidOperationException("Explicit posting account type and normal balance are required.");
        var mapping=Records.SingleOrDefault(row=>row.MAPPING_KEY==key && Effective(row,AsOfDate)) ??throw new InvalidOperationException("The explicit effective company/book/currency mapping is unavailable.");
        var account=Accounts.Accounts.Single(row=>row.GL_ACCOUNT_ID==mapping.GL_ACCOUNT_ID);
        if(account.ACCOUNT_TYPE!=accountType || account.NORMAL_BALANCE!=normalBalance)throw new InvalidOperationException("The recorded mapping account is incompatible with the explicit posting purpose.");
        return CompanyInventorySourceReview.Copy(account);
    }
    public static bool Effective(GLAccountMapping row,DateTime date)=>row.EFFECTIVE_DATE<=date && (row.EXPIRY_DATE is null || row.EXPIRY_DATE>=date);
    public static void RequireDate(DateTime date){if(date==default || date.Kind!=DateTimeKind.Unspecified || date.TimeOfDay!=TimeSpan.Zero)throw new InvalidOperationException("An explicit company calendar date is required without a timezone conversion.");}
    public static bool HasCompanyMarkers(GLAccountMapping row)=>row.OWNER_TENANT_ID is not null || row.OWNER_COMPANY_BA_ID is not null || row.BOOK_ID is not null || row.CURRENCY_CODE is not null || row.GL_ACCOUNT_ID is not null;
    private static string Digest(CompanyGLMappingReview value)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(value with{Revision=string.Empty}));
}
