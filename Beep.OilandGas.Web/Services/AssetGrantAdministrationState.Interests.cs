using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

public sealed partial class AssetGrantAdministrationState
{
    public string InterestFieldId { get; private set; } = "";
    public FieldInterestReview? InterestReview { get; private set; }
    public bool CanChooseInterestField => !disposed && !Busy && IsAdministrator;
    public bool CanReviewInterests => CanChooseInterestField && Identifier(InterestFieldId);
    public string? InterestFieldError => InterestFieldId.Length != 0 && !Identifier(InterestFieldId)
        ? "Use an exact FIELD ID of at most 128 characters without surrounding whitespace or control characters." : null;

    public void SelectInterestField(string? fieldId)
    {
        if (disposed || InterestFieldId == (fieldId ?? "")) return;
        Clear(); InterestFieldId = fieldId ?? ""; Changed?.Invoke();
    }
    public async Task LoadInterestReviewAsync()
    {
        if (disposed) return;
        Clear(); var scope = company.Snapshot; var field = InterestFieldId; var version = generation;
        if (scope.Selection is null || !IsAdministrator || !Identifier(field))
        {
            Error = "Select a company Administrator account and enter an exact FIELD ID to review its recorded company interests.";
            Changed?.Invoke(); return;
        }
        using var cancellation = new CancellationTokenSource(); pending = cancellation; Busy = true; Changed?.Invoke();
        bool Current() => !disposed && generation == version && company.Snapshot == scope && InterestFieldId == field;
        try
        {
            var result = await review.GetFieldInterestReviewAsync(field, cancellation.Token);
            if (!Current()) return;
            FieldInterestReviewValidation.RequireValid(result, scope.Selection.Scope, field);
            InterestReview = result;
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested || !Current()) { }
        catch (Exception failure)
        {
            if (!Current()) return;
            var sentence = explain(failure, "reviewing recorded FIELD interests", "FIELD interests could not be reviewed");
            if (Current()) Error = sentence;
        }
        finally
        {
            if (ReferenceEquals(pending, cancellation)) pending = null;
            if (Current()) { Busy = false; Changed?.Invoke(); }
        }
    }
}
