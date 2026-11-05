using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.PPDM39.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Beep.OilandGas.Models.Data.DevelopmentPlanning;

/// <summary>Frozen source and authority evidence for preparing a facility edit; it is not a saved outcome.</summary>
public sealed record DevelopmentFacilityEditReview(
    [property:JsonRequired] FieldSelectionReview Context,
    [property:JsonRequired] FACILITY Original,
    [property:JsonRequired] string EditorRole,
    [property:JsonRequired] string SourceRevision,
    [property:JsonRequired] string AuthorityRevision,
    [property:JsonRequired] string Revision,
    [property:JsonRequired] DateTimeOffset CheckedAtUtc)
{
    public string ContentRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Context,Original,EditorRole,SourceRevision,AuthorityRevision}));
    public static string SourceDigest(FACILITY source)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(source));
    public void Require()
    {
        if(Context is null || Original is null)throw new InvalidOperationException("The original facility and current company/FIELD review are required.");
        FieldSelectionReviewValidator.RequireReview(Context,Context.Company,Context.ActorUserId);
        if(!LeaseSummaryValidator.Identifier(Context.Company.TenantId) || !LeaseSummaryValidator.Identifier(Context.Company.BusinessAssociateId) || !LeaseSummaryValidator.Identifier(Context.ActorUserId) ||
            !LeaseSummaryValidator.Identifier(Context.FieldId) || !LeaseSummaryValidator.Identifier(Original.FACILITY_ID) || !LeaseSummaryValidator.Identifier(Original.FACILITY_TYPE) ||
            Context.Company.TenantId.Length>128 || Context.Company.BusinessAssociateId.Length>128 || Context.ActorUserId.Length>128 || Context.FieldId!.Length>128 ||
            Original.FACILITY_ID.Length>128 || Original.FACILITY_TYPE.Length>128 || Original.PRIMARY_FIELD_ID!=Context.FieldId || !LeaseSummaryValidator.Identifier(EditorRole) || EditorRole.Length>128 ||
            !ProcessDefinitionRevision.IsValid(SourceRevision) || SourceRevision!=SourceDigest(Original) || !ProcessDefinitionRevision.IsValid(AuthorityRevision) ||
            Revision!=ContentRevision() || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero || CheckedAtUtc>DateTimeOffset.UtcNow)
            throw new InvalidOperationException("The original composite facility identity, FIELD source and editor review must be complete and unchanged.");
    }
}

/// <summary>Exact retained proposed descriptive edit. Persistence and recovery require an independently qualified native owner.</summary>
public sealed record DevelopmentFacilityEdit(
    [property:JsonRequired] DevelopmentFacilityEditReview Review,
    [property:JsonRequired] Guid IntentId,
    [property:JsonRequired] string LongName,
    [property:JsonRequired] string? Description,
    [property:JsonRequired] string Reason,
    [property:JsonRequired] bool Confirmed)
{
    private static bool Text(string? value,int maximum,bool required)=>value is null?!required:value.Length<=maximum && value==value.Trim() && (!required || value.Length>0) && !value.Any(c=>char.IsControl(c) && c is not ('\r' or '\n' or '\t'));
    public void Require()
    {
        if(Review is null)throw new InvalidOperationException("The original reviewed facility is required.");
        Review.Require();
        if(IntentId==Guid.Empty || !Confirmed || !Text(LongName,255,true) || !Text(Description,4000,false) || !Text(Reason,1600,true) ||
            LongName==Review.Original.FACILITY_LONG_NAME && Description==Review.Original.DESCRIPTION || JsonSerializer.SerializeToUtf8Bytes(this).Length>262144)
            throw new InvalidOperationException("An exact changed facility description, bounded reason and explicit confirmation are required.");
    }
    public string Revision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this));}
}
