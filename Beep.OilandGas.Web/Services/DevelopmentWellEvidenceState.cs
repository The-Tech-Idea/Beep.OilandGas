using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

/// <summary>Recorded WELL evidence for the existing design route; WELL status is not a design approval.</summary>
public sealed class DevelopmentWellEvidenceState : IDisposable
{
    private readonly CompanyClientContext company;
    private readonly IFieldSelectionContext fields;
    private readonly IDevelopmentServiceClient development;
    private readonly Func<Exception, string, string, string> explain;
    private long generation;
    private bool disposed;
    private WELL[] rows = [];
    private WELL? selected;
    public IReadOnlyList<WELL> Rows => Copy(rows);
    public WELL? Selected => selected is null ? null : Copy(selected);
    public FieldSelectionReview? Review { get; private set; }
    public string? FieldName { get; private set; }
    public DateTimeOffset? LoadedAtUtc { get; private set; }
    public bool Loading { get; private set; }
    public bool Loaded { get; private set; }
    public bool NeedsCompany => company.Selection is null;
    public bool NeedsField { get; private set; }
    public bool NotFound { get; private set; }
    public string? Error { get; private set; }
    public event Action? Changed;

    public DevelopmentWellEvidenceState(CompanyClientContext company, IFieldSelectionContext fields,
        IDevelopmentServiceClient development, Func<Exception, string, string, string> explain)
    {
        this.company=company; this.fields=fields; this.development=development; this.explain=explain;
        company.Changed+=Invalidate; fields.CurrentFieldChanged+=FieldChanged;
    }
    private static T Copy<T>(T value) => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value))!;
    private void FieldChanged(string _) => Invalidate();
    public void Clear() { if(!disposed) Invalidate(); }
    private void Invalidate()
    {
        ++generation; rows=[]; selected=null; Review=null; FieldName=null; LoadedAtUtc=null;
        Loading=false; Loaded=false; NeedsField=false; NotFound=false; Error=null; Changed?.Invoke();
    }
    public async Task LoadAsync(string? wellId = null)
    {
        if(disposed) return;
        Invalidate(); var version=generation; var captured=company.Snapshot;
        if(captured.Selection is null) return;
        bool Current() => !disposed && version==generation && captured==company.Snapshot;
        Loading=true; Changed?.Invoke();
        try
        {
            var before=await fields.GetCurrentFieldSelectionAsync();
            if(!Current()) return;
            FieldSelectionReviewValidator.RequireCurrent(before,captured.Selection.Scope,captured.Selection.UserId);
            if(!before.SelectionAvailable) { NeedsField=true; return; }
            var review=before.SelectionContext!;
            var fieldName=before.FieldName;
            WELL[] result;
            if(wellId is null)
                result=(await development.GetWellsAsync(fieldId:review.FieldId) ?? throw new InvalidOperationException("Development well evidence was unavailable.")).ToArray();
            else
            {
                if(string.IsNullOrWhiteSpace(wellId)) throw new InvalidOperationException("A recorded well identity is required.");
                var well=await development.GetWellAsync(wellId,fieldId:review.FieldId);
                result=well is null ? [] : [well];
            }
            if(!Current()) return;
            result=Copy(result);
            if(result.Any(row=>row is null || string.IsNullOrWhiteSpace(row.UWI) || row.ASSIGNED_FIELD!=review.FieldId ||
                row.ACTIVE_IND!="Y" || row.CURRENT_CLASS!="DEVELOPMENT" || wellId is not null && row.UWI!=wellId) ||
                result.Select(row=>row.UWI).Distinct(StringComparer.Ordinal).Count()!=result.Length)
                throw new InvalidOperationException("The development well identities or FIELD evidence could not be confirmed.");
            var after=await fields.GetCurrentFieldSelectionAsync();
            if(!Current()) return;
            FieldSelectionReviewValidator.RequireCurrent(after,captured.Selection.Scope,captured.Selection.UserId);
            if(!after.SelectionAvailable || after.SelectionContext!=review || after.FieldId!=before.FieldId)
                throw new InvalidOperationException("The company FIELD selection changed. Reload well evidence.");
            rows=result; selected=wellId is null ? null : result.SingleOrDefault();
            Review=review; FieldName=fieldName; LoadedAtUtc=DateTimeOffset.UtcNow;
            NotFound=wellId is not null && selected is null; Loaded=true;
        }
        catch(Exception error)
        {
            var sentence=explain(error,"loading development well evidence","Development well evidence could not be confirmed");
            if(Current()) Error=sentence;
        }
        finally { if(Current()) { Loading=false; Changed?.Invoke(); } }
    }
    public void Dispose()
    {
        disposed=true; company.Changed-=Invalidate; fields.CurrentFieldChanged-=FieldChanged; Invalidate();
    }
}
