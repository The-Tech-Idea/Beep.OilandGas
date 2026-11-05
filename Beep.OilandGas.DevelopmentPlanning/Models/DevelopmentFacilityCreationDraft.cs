using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Beep.OilandGas.DevelopmentPlanning.Models;

/// <summary>An unsaved facility proposal for one reviewed company and FIELD.</summary>
public sealed record DevelopmentFacilityCreationDraft(
    [property:JsonRequired] FieldSelectionReview Context,
    [property:JsonRequired] string CreatorRole,
    [property:JsonRequired] string FacilityId,
    [property:JsonRequired] string FacilityType,
    [property:JsonRequired] string OperatorBusinessAssociateId,
    [property:JsonRequired] string LongName,
    [property:JsonRequired] string? Description,
    [property:JsonRequired] string Reason)
{
    private static bool Text(string? value,int max,bool required)=>value is null?!required:value.Length<=max && value==value.Trim() && (!required || value.Length>0) && !value.Any(c=>char.IsControl(c) && c is not ('\r' or '\n' or '\t'));
    public void Require()
    {
        if(Context is null)throw new InvalidOperationException("A reviewed company and FIELD are required.");
        FieldSelectionReviewValidator.RequireReview(Context,Context.Company,Context.ActorUserId);
        if(!LeaseSummaryValidator.Identifier(Context.Company.TenantId) || !LeaseSummaryValidator.Identifier(Context.Company.BusinessAssociateId) || !LeaseSummaryValidator.Identifier(Context.ActorUserId) ||
           !LeaseSummaryValidator.Identifier(Context.FieldId) || !LeaseSummaryValidator.Identifier(CreatorRole) || !LeaseSummaryValidator.Identifier(FacilityId) || !LeaseSummaryValidator.Identifier(FacilityType) || !LeaseSummaryValidator.Identifier(OperatorBusinessAssociateId) ||
           Context.Company.TenantId.Length>128 || Context.Company.BusinessAssociateId.Length>128 || Context.ActorUserId.Length>128 || Context.FieldId!.Length>128 || CreatorRole.Length>128 || FacilityId.Length>128 || FacilityType.Length>128 || OperatorBusinessAssociateId.Length>128 ||
           !Text(LongName,255,true) || !Text(Description,4000,false) || !Text(Reason,1600,true) || JsonSerializer.SerializeToUtf8Bytes(this).Length>262144)
            throw new InvalidOperationException("An exact facility identity, current role, name and reason are required.");
    }
}
