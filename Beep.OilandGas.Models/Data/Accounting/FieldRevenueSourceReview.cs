using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Models.Data.Accounting;

/// <summary>Recorded source rows only; no recognition, invoice, currency conversion or posting is inferred.</summary>
public sealed record FieldRevenueSourceReview(CompanyScope Company,string ActorUserId,string FieldId,DateTime From,DateTime Through,
    string BindingRevision,IReadOnlyList<REVENUE_TRANSACTION> Rows,string Revision)
{
    public static void RequireWindow(string field,DateTime from,DateTime through)
    {
        if(string.IsNullOrWhiteSpace(field) || field.Length>128 || field.Any(char.IsControl) || from==default || through==default ||
            from.TimeOfDay!=TimeSpan.Zero || through.TimeOfDay!=TimeSpan.Zero || through<from || through.Date==DateTime.MaxValue.Date)
            throw new ArgumentException("An exact FIELD and inclusive stored calendar-date window are required.");
    }
    public static void RequireRows(string field,DateTime from,DateTime through,IReadOnlyList<REVENUE_TRANSACTION> rows)
    {
        RequireWindow(field,from,through);
        if(rows is null || rows.Any(row=>row is null || string.IsNullOrWhiteSpace(row.REVENUE_TRANSACTION_ID) || row.REVENUE_TRANSACTION_ID.Length>128 || row.REVENUE_TRANSACTION_ID.Any(char.IsControl) ||
            row.ACTIVE_IND!="Y" || row.TRANSACTION_DATE is null || row.TRANSACTION_DATE.Value.Date<from || row.TRANSACTION_DATE.Value.Date>through ||
            row.FIELD_ID!=field && row.PROPERTY_ID!=field || !string.IsNullOrEmpty(row.FIELD_ID) && row.FIELD_ID!=field || !string.IsNullOrEmpty(row.PROPERTY_ID) && row.PROPERTY_ID!=field) ||
            rows.Select(row=>row.REVENUE_TRANSACTION_ID).Distinct(StringComparer.Ordinal).Count()!=rows.Count)
            throw new InvalidOperationException("Complete recorded revenue identity, FIELD membership or date window is unconfirmed.");
    }
    public void Require(CompanyScope company,string actor,string field,DateTime from,DateTime through)
    {
        PurchaseOrderOwnership.RequireScope(company);RequireRows(field,from,through,Rows);
        if(Company!=company || ActorUserId!=actor || string.IsNullOrWhiteSpace(actor) || FieldId!=field || From!=from || Through!=through ||
            string.IsNullOrWhiteSpace(BindingRevision) || BindingRevision.Length>512 || BindingRevision.Any(char.IsControl) || Revision!=Digest(Company,ActorUserId,FieldId,From,Through,BindingRevision,Rows))
            throw new InvalidOperationException("The original revenue source company, account, route, window, binding or revision changed.");
    }
    public static FieldRevenueSourceReview Create(CompanyScope company,string actor,string field,DateTime from,DateTime through,string binding,IReadOnlyList<REVENUE_TRANSACTION> rows)
    {
        rows=CompanyInventorySourceReview.Copy(rows.OrderBy(row=>row.REVENUE_TRANSACTION_ID,StringComparer.Ordinal).ToArray());
        var result=new FieldRevenueSourceReview(company,actor,field,from,through,binding,rows,Digest(company,actor,field,from,through,binding,rows));result.Require(company,actor,field,from,through);return result;
    }
    private static string Digest(CompanyScope company,string actor,string field,DateTime from,DateTime through,string binding,IReadOnlyList<REVENUE_TRANSACTION> rows)=>
        ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{company,actor,field,from,through,binding,Rows=rows.OrderBy(row=>row.REVENUE_TRANSACTION_ID,StringComparer.Ordinal)}));
}
