using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.Models.Data.Process;

public sealed record CompanyAccountingProcessStartCatalogQuery(string EntityType,string EntityId,string ExpectedSourceRevision)
{
    public void Require()
    {
        if(EntityType is not ("AP_INVOICE" or "PURCHASE_ORDER" or "SALES_CONTRACT" or "RUN_TICKET" or "INVOICE" or "AR_INVOICE" or "JOURNAL_ENTRY" or "AR_PAYMENT") || string.IsNullOrWhiteSpace(EntityId) || EntityId.Length>128 || EntityId.Any(char.IsControl) ||
            !ProcessDefinitionRevision.IsValid(ExpectedSourceRevision))throw new ArgumentException("The displayed accounting source and its recorded revision are required.");
    }
}

public sealed record CompanyAccountingProcessStartCatalog(CompanyScope Company,string ActorUserId,string EntityType,string EntityId,
    string SourceRevision,string BindingRevision,IReadOnlyList<CompanyAccountingProcessStartReview> Choices)
{
    public void Require(CompanyScope company,string actor,CompanyAccountingProcessStartCatalogQuery query)
    {
        query.Require();APInvoiceOwnership.RequireScope(company);
        if(Company!=company || ActorUserId!=actor || string.IsNullOrWhiteSpace(actor) || EntityType!=query.EntityType || EntityId!=query.EntityId ||
            SourceRevision!=query.ExpectedSourceRevision || !ProcessDefinitionRevision.IsValid(BindingRevision) || Choices is null || Choices.Count>128 ||
            Choices.Any(choice=>choice is null) || Choices.Select(choice=>choice.ProcessId).Distinct(StringComparer.Ordinal).Count()!=Choices.Count)
            throw new InvalidOperationException("The recorded workflow choices do not match the original displayed company source.");
        foreach(var choice in Choices)
        {
            choice.Require();
            if(choice.Company!=company || choice.ActorUserId!=actor || choice.EntityType!=EntityType || choice.EntityId!=EntityId || choice.SourceRevision!=SourceRevision || choice.BindingRevision!=BindingRevision)
                throw new InvalidOperationException("A workflow choice belongs to another company, actor, source or binding.");
        }
    }
}
