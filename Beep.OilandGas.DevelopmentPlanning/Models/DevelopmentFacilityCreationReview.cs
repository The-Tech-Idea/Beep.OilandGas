using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Beep.OilandGas.DevelopmentPlanning.Models;

/// <summary>Read-only source observations for an unsaved facility proposal.</summary>
public sealed record DevelopmentFacilityCreationReview(
    [property:JsonRequired] DevelopmentFacilityCreationDraft Draft,
    [property:JsonRequired] R_FACILITY_TYPE TypeSource,
    [property:JsonRequired] string OperatorRevision,
    [property:JsonRequired] string AuthorityRevision,
    [property:JsonRequired] string Revision,
    [property:JsonRequired] DateTimeOffset CheckedAtUtc)
{
    public string ContentRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Draft,TypeSource,OperatorRevision,AuthorityRevision}));
    public void Require()
    {
        if(Draft is null || TypeSource is null)throw new InvalidOperationException("The proposal and recorded facility type are required.");
        Draft.Require();
        if(TypeSource.FACILITY_TYPE!=Draft.FacilityType || TypeSource.ACTIVE_IND!="Y" || !ProcessDefinitionRevision.IsValid(OperatorRevision) || !ProcessDefinitionRevision.IsValid(AuthorityRevision) ||
           Revision!=ContentRevision() || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero || CheckedAtUtc>DateTimeOffset.UtcNow)
            throw new InvalidOperationException("The original proposal, recorded active type and authority must be unchanged.");
    }
}

public interface ICompanyFacilityCreationReader
{
    Task<DevelopmentFacilityCreationSource> ReadFacilityCreationSourceAsync(DevelopmentFacilityCreationDraft draft,Func<Task> authority);
}

public sealed record DevelopmentFacilityCreationSource(R_FACILITY_TYPE TypeSource,string OperatorRevision);
