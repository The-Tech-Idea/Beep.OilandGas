using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.Models.Data.Reporting;
public sealed record ExecutiveAsset(string FieldId,string? FieldName,string? LifecyclePhase=null,int? ActiveWells=null);
public sealed record ExecutivePortfolioReview(CompanyScope Company,string Actor,string Source,DateTimeOffset RetrievedAtUtc,IReadOnlyList<ExecutiveAsset> Assets);
public static class ExecutivePortfolioChecks
{
    public static void Require(ExecutivePortfolioReview? review,CompanyScope company,string actor)
    {
        if(review is null || review.Company!=company || review.Actor!=actor || review.Source!="FIELD" || review.RetrievedAtUtc==default || review.RetrievedAtUtc.Offset!=TimeSpan.Zero || review.Assets is null ||
            review.Assets.Any(x=>x is null || string.IsNullOrWhiteSpace(x.FieldId) || x.ActiveWells<0) || review.Assets.Select(x=>x.FieldId).Distinct(StringComparer.Ordinal).Count()!=review.Assets.Count)
            throw new InvalidOperationException("Executive portfolio evidence does not match the original company/account and recorded FIELD source.");
    }
}
