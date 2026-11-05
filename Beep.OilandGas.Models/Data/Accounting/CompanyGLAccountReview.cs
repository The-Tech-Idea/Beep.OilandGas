using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Data.Accounting;

/// <summary>Recorded company/book/currency account facts; legacy balances do not establish posted evidence.</summary>
public sealed record CompanyGLAccountReview(CompanyScope Company,string ActorUserId,string BookId,string CurrencyCode,IReadOnlyList<GL_ACCOUNT> Accounts,string Revision)
{
    public const int MaximumAccounts=512;
    public static CompanyGLAccountReview Create(CompanyScope company,string actor,string book,string currency,IEnumerable<GL_ACCOUNT> accounts)
    {
        ArgumentNullException.ThrowIfNull(accounts);
        var original=accounts.Take(MaximumAccounts+1).ToArray();
        if(original.Length>MaximumAccounts || original.Any(row=>row is null))throw new InvalidOperationException("The complete bounded company ledger account set is unavailable.");
        original=original.OrderBy(row=>row.GL_ACCOUNT_ID,StringComparer.Ordinal).ToArray();
        var result=new CompanyGLAccountReview(company,actor,book,currency,original,string.Empty);
        result=result with{Revision=Digest(result)};result.Require(company,actor,book,currency);
        var frozen=CompanyInventorySourceReview.Copy(result);frozen.Require(company,actor,book,currency);return frozen;
    }
    public void Require(CompanyScope company,string actor,string book,string currency)
    {
        PurchaseOrderOwnership.RequireScope(company);BillingPolicyValues.Text(actor,128);BillingPolicyValues.Text(book,128);BillingPolicyValues.Text(currency,16);
        if(Company!=company || ActorUserId!=actor || BookId!=book || CurrencyCode!=currency || Accounts is null || Accounts.Count>MaximumAccounts || Accounts.Any(row=>row is null) ||
            Accounts.Select(row=>row.GL_ACCOUNT_ID).Distinct(StringComparer.Ordinal).Count()!=Accounts.Count || Accounts.Select(row=>row.ACCOUNT_NUMBER).Distinct(StringComparer.Ordinal).Count()!=Accounts.Count ||
            !Accounts.Select(row=>row.GL_ACCOUNT_ID).SequenceEqual(Accounts.Select(row=>row.GL_ACCOUNT_ID).Order(StringComparer.Ordinal)))throw new InvalidOperationException("The complete bounded company ledger account review is unconfirmed.");
        foreach(var row in Accounts)
        {
            BillingPolicyValues.Text(row.GL_ACCOUNT_ID,128);BillingPolicyValues.Text(row.ACCOUNT_NUMBER,128);BillingPolicyValues.Text(row.ACCOUNT_NAME,256);RequireAudit(row);
            if(row.OWNER_TENANT_ID!=company.TenantId || row.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || row.BOOK_ID!=book || row.CURRENCY_CODE!=currency || row.ACTIVE_IND!="Y" ||
                row.ACCOUNT_TYPE is not ("ASSET" or "LIABILITY" or "EQUITY" or "REVENUE" or "EXPENSE") || row.NORMAL_BALANCE is not ("DEBIT" or "CREDIT"))throw new InvalidOperationException("A ledger account has unavailable company/book/currency or type facts.");
        }
        if(Revision!=Digest(this))throw new InvalidOperationException("The complete recorded ledger account review changed.");
    }
    public static bool HasCompanyMarkers(GL_ACCOUNT row)=>row.OWNER_TENANT_ID is not null || row.OWNER_COMPANY_BA_ID is not null || row.BOOK_ID is not null || row.CURRENCY_CODE is not null;
    public static void RequireAudit(global::Beep.OilandGas.Models.Data.ModelEntityBase row)
    {
        BillingPolicyValues.Text(row.ROW_CREATED_BY,128);
        if(row.ROW_CREATED_DATE is not {} created || created==default || created.Kind!=DateTimeKind.Utc || string.IsNullOrWhiteSpace(row.ROW_CHANGED_BY)!=(row.ROW_CHANGED_DATE is null))
            throw new InvalidOperationException("The recorded ledger configuration creation/change audit is unconfirmed.");
        if(row.ROW_CHANGED_DATE is {} changed){BillingPolicyValues.Text(row.ROW_CHANGED_BY,128);if(changed==default || changed.Kind!=DateTimeKind.Utc || changed<created)throw new InvalidOperationException("The recorded ledger configuration change audit is unconfirmed.");}
    }
    private static string Digest(CompanyGLAccountReview value)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(value with{Revision=string.Empty}));
}
