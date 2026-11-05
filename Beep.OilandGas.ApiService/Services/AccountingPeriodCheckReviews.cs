using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
namespace Beep.OilandGas.ApiService.Services;
public static class AccountingPeriodCheckReviews
{
    public static AccountingPeriodCheckReview Create(CompanyScope company,string actor,string field,string book,DateTime end,PeriodCloseChecklist source)
    {
        if(string.IsNullOrWhiteSpace(actor) || string.IsNullOrWhiteSpace(field) || string.IsNullOrWhiteSpace(book) || end==default ||
            source.PeriodEndDate!=end || source.ChecklistGeneratedAt==default || source.ChecklistGeneratedAt.Kind!=DateTimeKind.Utc || source.Items is null ||
            source.Items.Any(row=>row is null || string.IsNullOrWhiteSpace(row.RuleId) || string.IsNullOrWhiteSpace(row.Name) || string.IsNullOrWhiteSpace(row.Module)) ||
            source.Items.Select(row=>row.RuleId).Distinct(StringComparer.Ordinal).Count()!=source.Items.Count ||
            !new[]{"GL_BALANCE","AP_CLOSE","AR_CLOSE"}.All(id=>source.Items.Any(row=>row.RuleId==id && row.IsMandatory)))
            throw new InvalidOperationException("Executed accounting period checks cannot be confirmed.");
        // AP/AR checks in this owner are company database-wide; do not label them as book/FIELD-filtered checks.
        var checks=source.Items.Select(row=>new AccountingPeriodCheck(row.RuleId,row.Name,row.Description,row.IsMandatory,row.IsComplete,row.Module,
            row.RuleId=="GL_BALANCE"?"BOOK":"COMPANY_DATABASE")).ToArray();
        return new(company,actor,field,book,end,source.ChecklistGeneratedAt,Array.AsReadOnly(checks));
    }
}
