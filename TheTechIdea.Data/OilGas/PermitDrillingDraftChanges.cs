using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
namespace TheTechIdea.Data.OilGas;

/// <summary>Typed supporting proposal; parent identity, filing state, linked well and audit cannot be edited.</summary>
public sealed record PermitDrillingDraftChanges(
    [property: JsonRequired] decimal ProposedDepth,
    [property: JsonRequired] string DepthUnit,
    [property: JsonRequired] string? TargetFormation,
    [property: JsonRequired] string? DrillingMethod,
    [property: JsonRequired] string? MudType,
    [property: JsonRequired] string? CasingProgram,
    [property: JsonRequired] string? LegalDescription,
    [property: JsonRequired] string? SurfaceOwnerNotified,
    [property: JsonRequired] string? EnvironmentalAssessmentRequired,
    [property: JsonRequired] string? EnvironmentalAssessmentReference,
    [property: JsonRequired] string? SpacingUnit,
    [property: JsonRequired] string? PermitType,
    [property: JsonRequired] string? Remarks)
{
    public static PermitDrillingDraftChanges From(DRILLING_PERMIT_APPLICATION row) => new(row.PROPOSED_DEPTH,row.PROPOSED_DEPTH_UNIT ?? "",
        row.TARGET_FORMATION,row.DRILLING_METHOD,row.MUD_TYPE,row.CASING_PROGRAM,row.LEGAL_DESCRIPTION,row.SURFACE_OWNER_NOTIFIED_IND,
        row.ENVIRONMENTAL_ASSESSMENT_REQUIRED_IND,row.ENVIRONMENTAL_ASSESSMENT_REFERENCE,row.SPACING_UNIT,row.PERMIT_TYPE,row.REMARKS);
    public void Require(PermitApplicationSourceReview review)
    {
        var parent=review.ReadApplication();
        if (parent.APPLICATION_TYPE!=PermitApplicationType.Drilling || !LeaseSummaryValidator.Identifier(parent.WELL_ID) ||
            parent.STATUS is not (PermitApplicationStatus.Draft or PermitApplicationStatus.AdditionalInformationRequired) ||
            ProposedDepth<=0 || !LeaseSummaryValidator.Identifier(DepthUnit) || DepthUnit.Length>128 ||
            new[]{TargetFormation,DrillingMethod,MudType,EnvironmentalAssessmentReference,SpacingUnit,PermitType}
                .Any(x=>x is not null && (x.Length>128 || x!=x.Trim() || x.Any(char.IsControl))) ||
            new[]{CasingProgram,LegalDescription,Remarks}.Any(x=>x is not null && (x.Length>4096 || x!=x.Trim() || x.Any(c=>char.IsControl(c) && c is not ('\r' or '\n')))) ||
            SurfaceOwnerNotified is not (null or "Y" or "N") || EnvironmentalAssessmentRequired is not (null or "Y" or "N"))
            throw new InvalidOperationException("Use a drilling draft with its recorded well, positive depth, explicit unit and bounded details; flags must be unknown, Y or N.");
        if(review.Supporting.Drilling is {} before && From(before)==this)
            throw new InvalidOperationException("The proposed drilling details are unchanged.");
    }
    public void Apply(DRILLING_PERMIT_APPLICATION row)
    {
        row.PROPOSED_DEPTH=ProposedDepth;row.PROPOSED_DEPTH_UNIT=DepthUnit;row.TARGET_FORMATION=TargetFormation;row.DRILLING_METHOD=DrillingMethod;
        row.MUD_TYPE=MudType;row.CASING_PROGRAM=CasingProgram;row.LEGAL_DESCRIPTION=LegalDescription;row.SURFACE_OWNER_NOTIFIED_IND=SurfaceOwnerNotified;
        row.ENVIRONMENTAL_ASSESSMENT_REQUIRED_IND=EnvironmentalAssessmentRequired;row.ENVIRONMENTAL_ASSESSMENT_REFERENCE=EnvironmentalAssessmentReference;
        row.SPACING_UNIT=SpacingUnit;row.PERMIT_TYPE=PermitType;row.REMARKS=Remarks;
    }
}
