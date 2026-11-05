namespace TheTechIdea.Data.OilGas;

/// <summary>Facility native command owner; authority is rechecked by the caller throughout the operation.</summary>
public interface ICompanyFacilityEditOwner
{
    Task<FACILITY_EDIT_INTENT> RecordFacilityEditAsync(DevelopmentFacilityEdit original,Func<Task> authority);
    Task<FACILITY_EDIT_INTENT> RecoverFacilityEditAsync(DevelopmentFacilityEdit original,Func<Task> readAuthority);
}

public sealed record DevelopmentFacilityEquipmentItem(string EquipmentId, decimal InstallationObservation, string? Source);

public interface ICompanyFacilityEquipmentReader
{
    Task<IReadOnlyList<global::Beep.OilandGas.PPDM39.Models.FACILITY_EQUIPMENT>> ReadDevelopmentFacilityEquipmentAsync(FieldSelectionReview context,string facilityId,string facilityType,Func<Task> authority);
}

/// <summary>Unsaved request to inspect installation source evidence.</summary>
public sealed record DevelopmentEquipmentInstallationProposal(FieldSelectionReview Context,string FacilityId,string FacilityType,string EquipmentId,string EditorRole,string Reason)
{
    public void Require()
    {
        if(Context is null)throw new InvalidOperationException("A reviewed company and FIELD are required.");
        FieldSelectionReviewValidator.RequireReview(Context,Context.Company,Context.ActorUserId);
        if(!LeaseSummaryValidator.Identifier(FacilityId) || !LeaseSummaryValidator.Identifier(FacilityType) || !LeaseSummaryValidator.Identifier(EquipmentId) || !LeaseSummaryValidator.Identifier(EditorRole) ||
           FacilityId.Length>128 || FacilityType.Length>128 || EquipmentId.Length>128 || EditorRole.Length>128 || string.IsNullOrWhiteSpace(Reason) || Reason!=Reason.Trim() || Reason.Length>1600 || Reason.Any(char.IsControl))
            throw new InvalidOperationException("An exact facility, equipment, role and reason are required.");
    }
}
public sealed record DevelopmentEquipmentInstallationSource(string EquipmentOwner,int RecordedRows,int ActiveRows,string Revision);
public sealed record DevelopmentEquipmentInstallationReview(DevelopmentEquipmentInstallationProposal Proposal,DevelopmentEquipmentInstallationSource Source,string AuthorityRevision,DateTimeOffset CheckedAtUtc)
{
    public void Require()
    {
        if(Proposal is null || Source is null)throw new InvalidOperationException("The original proposal and source evidence are required.");
        Proposal.Require();
        if(Source.EquipmentOwner!=Proposal.Context.Company.BusinessAssociateId || Source.RecordedRows<0 || Source.ActiveRows<0 || Source.ActiveRows>Source.RecordedRows || !ProcessDefinitionRevision.IsValid(Source.Revision) || !ProcessDefinitionRevision.IsValid(AuthorityRevision) || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero || CheckedAtUtc>DateTimeOffset.UtcNow)
            throw new InvalidOperationException("The original equipment source or authority is invalid.");
    }
}
public interface ICompanyEquipmentInstallationReader
{
    Task<DevelopmentEquipmentInstallationSource> ReviewDevelopmentEquipmentInstallationAsync(DevelopmentEquipmentInstallationProposal proposal,Func<Task> authority);
}
