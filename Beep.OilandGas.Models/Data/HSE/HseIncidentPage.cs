using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.Models.Data.HSE;

/// <summary>A logical page of authorized incident keys; counts are incidents, not assigned actions.</summary>
public sealed record HseIncidentPage(FieldSelectionReview Context, int PageNumber, int VisibleIncidentCount,
    IReadOnlyList<string> IncidentIds)
{
    public const int PageSize = 10;
    public bool HasNextPage => (long)PageNumber * PageSize < VisibleIncidentCount;
    public void Require()
    {
        FieldSelectionReviewValidator.RequireReview(Context, Context.Company, Context.ActorUserId);
        var offset = ((long)PageNumber - 1) * PageSize;
        if (PageNumber < 1 || VisibleIncidentCount is < 0 or > 256 || IncidentIds is null ||
            PageNumber > 1 && offset >= VisibleIncidentCount ||
            IncidentIds.Count != Math.Min(PageSize, Math.Max(0L, VisibleIncidentCount - offset)) ||
            IncidentIds.Any(id => !LeaseSummaryValidator.Identifier(id)) ||
            IncidentIds.Distinct(StringComparer.Ordinal).Count() != IncidentIds.Count ||
            !IncidentIds.SequenceEqual(IncidentIds.Order(StringComparer.Ordinal)))
            throw new InvalidOperationException("The authorized incident page is unavailable or inconsistent.");
    }
}
