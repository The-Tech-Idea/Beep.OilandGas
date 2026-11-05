using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
namespace TheTechIdea.Data.OilGas;

public sealed record PermitApplicationInboxAuthority(PermitApplicationDataScope Context,bool CanAuthor,bool CanReview,string FieldRevision);

/// <summary>Frozen returned-record work queues. Counts are not provider inventory completeness or permission to execute a command.</summary>
public sealed record PermitApplicationInboxReview(
    [property: JsonRequired] PermitApplicationInboxAuthority Authority,
    [property: JsonRequired] DateTimeOffset ReadAtUtc,
    [property: JsonRequired] string ApplicationsJson)
{
    public static PermitApplicationInboxReview Capture(PermitApplicationInboxAuthority authority,DateTimeOffset readAt,IReadOnlyList<PermitApplicationSummary> applications)
    {
        var result=new PermitApplicationInboxReview(authority,readAt,JsonSerializer.Serialize(applications));result.Require();return result;
    }
    public void Require()
    {
        if(Authority is null || Authority.Context is null || !ProcessDefinitionRevision.IsValid(Authority.FieldRevision) || ReadAtUtc==default || ReadAtUtc.Offset!=TimeSpan.Zero ||
            ApplicationsJson is not{Length:>0 and <=4194304})throw new InvalidOperationException("The permit inbox evidence is absent or exceeds its bounds.");
        Authority.Context.Require();var rows=JsonSerializer.Deserialize<PermitApplicationSummary[]>(ApplicationsJson);
        if(rows is null || rows.Length>4096 || rows.Any(row=>row is null || !LeaseSummaryValidator.Identifier(row.PermitApplicationId) || row.FieldId!=Authority.Context.Field.FieldId ||
            !Enum.GetNames<PermitApplicationStatus>().Contains(row.Status,StringComparer.Ordinal)) || rows.Select(row=>row.PermitApplicationId).Distinct(StringComparer.Ordinal).Count()!=rows.Length)
            throw new InvalidOperationException("The returned permit records are incomplete, ambiguous or belong to another FIELD.");
    }
    public void RequireFor(FieldSelectionReview expected){PermitApplicationSourceReview.RequireTarget("permit-inbox",expected);Require();if(Authority.Context.Field!=expected)throw new InvalidOperationException("The permit inbox belongs to another company/account/FIELD selection.");}
    public IReadOnlyList<PermitApplicationSummary> ReadApplications(){Require();return JsonSerializer.Deserialize<PermitApplicationSummary[]>(ApplicationsJson)!;}
    public static string Queue(PermitApplicationSummary row,string actor,bool author,bool reviewer)=>row.Status switch
    {
        nameof(PermitApplicationStatus.Draft) when author=>"Drafts",
        nameof(PermitApplicationStatus.AdditionalInformationRequired) when author=>"Corrections",
        nameof(PermitApplicationStatus.Submitted) or nameof(PermitApplicationStatus.UnderReview) when reviewer && LeaseSummaryValidator.Identifier(row.SubmittedByUserId) && row.SubmittedByUserId!=actor=>"Independent review",
        _=>"Other returned records"
    };
}
