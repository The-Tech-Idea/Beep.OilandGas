using System.Collections.Generic;

namespace Beep.OilandGas.PermitsAndApplications.Constants;

public static class PermitsReferenceCodeSeed
{
    public readonly record struct SeedRow(string ReferenceSet, string ReferenceCode, string LongName, string ActiveInd = "Y");

    public static IEnumerable<SeedRow> GetAllSeedRows()
    {
        yield return new(PermitsReferenceSets.PermitStatus, PermitsReferenceDefaults.Draft, "Draft");
        yield return new(PermitsReferenceSets.PermitStatus, PermitsReferenceDefaults.Submitted, "Submitted");
        yield return new(PermitsReferenceSets.PermitStatus, PermitsReferenceDefaults.UnderReview, "Under Review");
        yield return new(PermitsReferenceSets.PermitStatus, PermitsReferenceDefaults.AdditionalInfoRequired, "Additional Information Required");
        yield return new(PermitsReferenceSets.PermitStatus, PermitsReferenceDefaults.Approved, "Approved");
        yield return new(PermitsReferenceSets.PermitStatus, PermitsReferenceDefaults.Rejected, "Rejected");
        yield return new(PermitsReferenceSets.PermitStatus, PermitsReferenceDefaults.Expired, "Expired");
        yield return new(PermitsReferenceSets.PermitStatus, PermitsReferenceDefaults.Renewed, "Renewed");
        yield return new(PermitsReferenceSets.PermitStatus, PermitsReferenceDefaults.Withdrawn, "Withdrawn");

        yield return new(PermitsReferenceSets.PermitAuthorityCategory, "STATE", "State Regulator");
        yield return new(PermitsReferenceSets.PermitAuthorityCategory, "FEDERAL", "Federal Regulator");
        yield return new(PermitsReferenceSets.PermitAuthorityCategory, "LOCAL", "Local Authority");
        yield return new(PermitsReferenceSets.PermitAuthorityCategory, "INDIGENOUS", "Indigenous Authority");

        yield return new(PermitsReferenceSets.ComplianceOutcome, "PASS", "Compliant");
        yield return new(PermitsReferenceSets.ComplianceOutcome, "WARN", "Warning");
        yield return new(PermitsReferenceSets.ComplianceOutcome, "FAIL", "Non-Compliant");

        yield return new(PermitsReferenceSets.ComplianceStatus, "PENDING", "Pending");
        yield return new(PermitsReferenceSets.ComplianceStatus, "IN_REVIEW", "In Review");
        yield return new(PermitsReferenceSets.ComplianceStatus, "CLOSED", "Closed");

        yield return new(PermitsReferenceSets.FormRequirementType, "FORM", "Regulatory Form");
        yield return new(PermitsReferenceSets.FormRequirementType, "ATTACHMENT", "Supporting Attachment");
        yield return new(PermitsReferenceSets.FormRequirementType, "CERTIFICATE", "Certificate");
        yield return new(PermitsReferenceSets.FormRequirementType, "ASSESSMENT", "Assessment");
    }
}
