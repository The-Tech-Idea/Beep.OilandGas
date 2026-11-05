using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Process;
namespace Beep.OilandGas.Models.Data.Company;

/// <summary>Closed discovery of the existing company handoff for an exact recorded native preparation.</summary>
public sealed record CompanyNativeDecisionHandoffReview(CompanyScope Company,string ActorUserId,string EntityType,string EntityId,string PreparationRevision,
    string BindingRevision,string CandidateSetRevision,CompanyAccountingProcessStart? Start,string? ProcessRevision,string? Status)
{
    public void Require(CompanyScope company,string actor,CompanyAccountingProcessStartCatalogQuery query)
    {
        query.Require();APInvoiceOwnership.RequireScope(company);
        if(!CompanyAccountingSourceTypes.IsNativeDecision(EntityType) || EntityType!=query.EntityType || Company!=company || ActorUserId!=actor || EntityId!=query.EntityId || PreparationRevision!=query.ExpectedSourceRevision ||
            string.IsNullOrWhiteSpace(actor) || actor.Length>128 || actor.Any(char.IsControl) ||
            !ProcessDefinitionRevision.IsValid(PreparationRevision) || !ProcessDefinitionRevision.IsValid(BindingRevision) || !ProcessDefinitionRevision.IsValid(CandidateSetRevision))
            throw new InvalidOperationException("The original company/account/native preparation discovery is required.");
        if(Start is null)
        {if(ProcessRevision is not null || Status is not null)throw new InvalidOperationException("Absent handoff evidence cannot contain a process outcome.");return;}
        Start.Require();
        if(Start.Company!=company || Start.EntityType!=EntityType || Start.EntityId!=EntityId || Start.SourceRevision!=PreparationRevision || Start.BindingRevision!=BindingRevision ||
            !ProcessDefinitionRevision.IsValid(ProcessRevision) || Status is not ("IN_PROGRESS" or "COMPLETED" or "CANCELLED" or "FAILED" or "SUSPENDED"))
            throw new InvalidOperationException("The discovered handoff differs from its exact original preparation/binding.");
    }
}
