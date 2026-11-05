namespace TheTechIdea.Data.OilGas;

/// <summary>Stable discovery of the exact version's owned native handoff, including confirmed absence.</summary>
public sealed record CompanyCommercialHandoffReview(CompanyScope Company,string ActorUserId,string ContractId,string PreparationRevision,
    string BindingRevision,string CandidateSetRevision,CompanyAccountingProcessStart? Start,string? ProcessRevision,string? Status)
{
    public void Require(CompanyScope company,string actor,CompanyAccountingProcessStartCatalogQuery query)
    {
        query.Require();APInvoiceOwnership.RequireScope(company);
        if(query.EntityType!="SALES_CONTRACT" || Company!=company || ActorUserId!=actor || ContractId!=query.EntityId || PreparationRevision!=query.ExpectedSourceRevision ||
            string.IsNullOrWhiteSpace(actor) || actor.Length>128 || actor.Any(char.IsControl) ||
            !ProcessDefinitionRevision.IsValid(PreparationRevision) || !ProcessDefinitionRevision.IsValid(BindingRevision) || !ProcessDefinitionRevision.IsValid(CandidateSetRevision))
            throw new InvalidOperationException("The original company/account/commercial version discovery is required.");
        if(Start is null)
        {if(ProcessRevision is not null || Status is not null)throw new InvalidOperationException("Absent handoff evidence cannot contain a process outcome.");return;}
        Start.Require();
        if(Start.Company!=company || Start.EntityType!="SALES_CONTRACT" || Start.EntityId!=ContractId || Start.SourceRevision!=PreparationRevision || Start.BindingRevision!=BindingRevision ||
            !ProcessDefinitionRevision.IsValid(ProcessRevision) || Status is not ("IN_PROGRESS" or "COMPLETED" or "CANCELLED" or "FAILED" or "SUSPENDED"))
            throw new InvalidOperationException("The discovered handoff differs from its exact original version/binding.");
    }
}
