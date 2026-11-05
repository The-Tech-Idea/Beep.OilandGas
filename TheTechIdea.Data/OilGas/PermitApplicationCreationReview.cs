using System.Text.Json;
using System.Text.Json.Serialization;
namespace TheTechIdea.Data.OilGas;

/// <summary>Explicit draft values and complete optional native resource identities; no saved application or write authorization.</summary>
public sealed record PermitApplicationCreationProposal(
    [property: JsonRequired] PermitApplicationDraftChanges Header,
    [property: JsonRequired] string? WellId,
    [property: JsonRequired] string? FacilityId,
    [property: JsonRequired] string? FacilityType)
{
    public void Require()
    {
        if(Header is null)throw new InvalidOperationException("Explicit draft values are required.");
        Header.RequireValues();
        if(WellId is not null && (!LeaseSummaryValidator.Identifier(WellId) || WellId.Length>128) ||
            FacilityId is not null && (!LeaseSummaryValidator.Identifier(FacilityId) || FacilityId.Length>128) ||
            FacilityType is not null && (!LeaseSummaryValidator.Identifier(FacilityType) || FacilityType.Length>128) ||
            (FacilityId is null)!=(FacilityType is null))
            throw new InvalidOperationException("Supply an exact optional well identity and both facility identity and type, or omit the resource.");
    }
}

/// <summary>Frozen server-reviewed creation proposal and native linked evidence, not a persisted draft or regulator clearance.</summary>
public sealed record PermitApplicationCreationReview(
    [property: JsonRequired] PermitApplicationDataScope Context,
    [property: JsonRequired] PermitApplicationCreationProposal Proposal,
    [property: JsonRequired] string LinkedSourcesJson,
    [property: JsonRequired] string Revision)
{
    private static string Fingerprint(PermitApplicationDataScope context,PermitApplicationCreationProposal proposal,string sources)=>
        ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Context=context,Proposal=proposal,Sources=sources}));
    public static PermitApplicationCreationReview Capture(PermitApplicationDataScope context,PermitApplicationCreationProposal proposal,
        string fieldSource,string? wellSource,string? facilitySource)
    {
        var sources=JsonSerializer.Serialize(new{Field=fieldSource,Well=wellSource,Facility=facilitySource});
        var review=new PermitApplicationCreationReview(context,proposal,sources,Fingerprint(context,proposal,sources));review.Require();return review;
    }
    public void Require()
    {
        if(Context is null || Proposal is null || LinkedSourcesJson is not{Length:>0 and <=1048576} || !ProcessDefinitionRevision.IsValid(Revision))
            throw new InvalidOperationException("The creation review is absent or exceeds its bounds.");
        Context.Require();Proposal.Require();
        using var document=JsonDocument.Parse(LinkedSourcesJson);var root=document.RootElement;
        if(root.ValueKind!=JsonValueKind.Object || root.EnumerateObject().Count()!=3 ||
            !root.TryGetProperty("Field",out var field) || field.ValueKind!=JsonValueKind.String || string.IsNullOrWhiteSpace(field.GetString()) ||
            !root.TryGetProperty("Well",out var well) || !root.TryGetProperty("Facility",out var facility) ||
            !ResourceMatches(well,Proposal.WellId) || !ResourceMatches(facility,Proposal.FacilityId) || Revision!=Fingerprint(Context,Proposal,LinkedSourcesJson))
            throw new InvalidOperationException("The creation proposal, selection or reviewed native resources changed.");
    }
    public global::Beep.OilandGas.PermitsAndApplications.Data.PermitTables.PERMIT_APPLICATION BuildDraft(Guid intentId)
    {
        Require();
        if(intentId==Guid.Empty)throw new InvalidOperationException("Retain a nonempty creation intent before transport.");
        var row=new global::Beep.OilandGas.PermitsAndApplications.Data.PermitTables.PERMIT_APPLICATION
        {
            PERMIT_APPLICATION_ID="PA-"+intentId.ToString("N"),OWNER_TENANT_ID=Context.Field.Company.TenantId,
            OWNER_COMPANY_ID=Context.Field.Company.BusinessAssociateId,FIELD_ID=Context.Field.FieldId!,ACTIVE_IND="Y",
            STATUS=global::Beep.OilandGas.Models.Data.PermitsAndApplications.PermitApplicationStatus.Draft,
            SUBMISSION_COMPLETE_IND="N",WELL_ID=Proposal.WellId,FACILITY_ID=Proposal.FacilityId,FACILITY_TYPE=Proposal.FacilityType
        };
        Proposal.Header.Apply(row);return row;
    }
    public PermitApplicationCommand PrepareOriginal(Guid intentId,string reason)
    {
        var source=PermitApplicationSourceReview.Capture(Context,BuildDraft(intentId),[]);
        var original=new PermitApplicationCommand(intentId,source,"CREATE_DRAFT",reason){CreationReview=this};original.Require();return original;
    }
    private static bool ResourceMatches(JsonElement source,string? id)=>id is null?source.ValueKind==JsonValueKind.Null:
        source.ValueKind==JsonValueKind.String && !string.IsNullOrWhiteSpace(source.GetString());
    public static void RequireTarget(PermitApplicationCreationProposal proposal,FieldSelectionReview expected)
    {
        ArgumentNullException.ThrowIfNull(proposal);proposal.Require();
        if(expected is null || expected.Company is null)throw new InvalidOperationException("A current company/account/FIELD selection is required.");
        FieldSelectionReviewValidator.RequireReview(expected,expected.Company,expected.ActorUserId);
        if(!LeaseSummaryValidator.Identifier(expected.FieldId))throw new InvalidOperationException("An exact FIELD is required.");
    }
    public void RequireFor(PermitApplicationCreationProposal proposal,FieldSelectionReview expected)
    {
        RequireTarget(proposal,expected);Require();
        if(Context.Field!=expected || Proposal!=proposal)
            throw new InvalidOperationException("The creation review belongs to another proposal or company/account/FIELD selection.");
    }
}
