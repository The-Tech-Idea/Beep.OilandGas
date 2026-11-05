using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
namespace Beep.OilandGas.PermitsAndApplications.Models;

/// <summary>Explicit mutable draft header only; identity, ownership, linked assets and lifecycle evidence are never client edits.</summary>
public sealed record PermitApplicationDraftChanges(
    [property: JsonRequired] PermitApplicationType ApplicationType,
    [property: JsonRequired] RegulatoryAuthority Authority,
    [property: JsonRequired] Country Country,
    [property: JsonRequired] StateProvince Region,
    [property: JsonRequired] string Applicant,
    [property: JsonRequired] string? Description)
{
    public void RequireValues()
    {
        if(!Enum.IsDefined(ApplicationType) || !Enum.IsDefined(Authority) || !Enum.IsDefined(Country) || Country==Country.Other || !Enum.IsDefined(Region) ||
            !LeaseSummaryValidator.Identifier(Applicant) || Applicant.Length>128 || Description is {Length:>4096} ||
            Description is not null && (Description!=Description.Trim() || Description.Any(c=>char.IsControl(c) && c is not ('\r' or '\n'))))
            throw new InvalidOperationException("Choose explicit supported draft classifications, an applicant and a bounded description.");
    }
    public void Require(PERMIT_APPLICATION source)
    {
        RequireValues();
        if(ApplicationType==source.APPLICATION_TYPE && Authority==source.REGULATORY_AUTHORITY && Country==source.COUNTRY && Region==source.STATE_PROVINCE &&
            Applicant==source.APPLICANT_ID && Description==source.REMARKS)
            throw new InvalidOperationException("The proposed draft is unchanged.");
    }
    public bool ChangesClassification(PERMIT_APPLICATION source)=>ApplicationType!=source.APPLICATION_TYPE || Authority!=source.REGULATORY_AUTHORITY || Country!=source.COUNTRY || Region!=source.STATE_PROVINCE;
    public void Apply(PERMIT_APPLICATION row)
    {row.APPLICATION_TYPE=ApplicationType;row.REGULATORY_AUTHORITY=Authority;row.COUNTRY=Country;row.STATE_PROVINCE=Region;row.APPLICANT_ID=Applicant;row.REMARKS=Description;}
}
