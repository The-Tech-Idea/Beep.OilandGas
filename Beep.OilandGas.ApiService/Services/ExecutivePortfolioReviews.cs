using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Reporting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.Models;
namespace Beep.OilandGas.ApiService.Services;
public static class ExecutivePortfolioReviews
{
    public static ExecutivePortfolioReview Create(IEnumerable<object>? source,CompanyScope company,string actor,DateTimeOffset retrieved)
    {
        if(source is null)throw new InvalidOperationException("Recorded FIELD portfolio was not returned.");
        var rows=source.ToArray();if(rows.Any(x=>x is not FIELD))throw new InvalidOperationException("Portfolio source is not recorded FIELD data.");
        var assets=rows.Cast<FIELD>().Select(x=>new ExecutiveAsset(x.FIELD_ID,x.FIELD_NAME)).OrderBy(x=>x.FieldId,StringComparer.Ordinal).ToArray();
        var review=new ExecutivePortfolioReview(company,actor,"FIELD",retrieved,assets);ExecutivePortfolioChecks.Require(review,company,actor);return review;
    }
}
