using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace TheTechIdea.Data.OilGas;

/// <summary>The recorded commercial source of one owned company process.</summary>
public sealed record CompanyCommercialProcessSourceReview(CompanyScope Company,string ActorUserId,string InstanceId,
    string ContractId,string PreparationRevision,string ProcessRevision,SalesContractTermsReadReview Terms,
    [property:System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] SALES_CONTRACT? CurrentNativeContract=null)
{
    public bool IsHistorical=>CurrentNativeContract is not null;
    public void Require(CompanyScope company,string actor,string instance)
    {
        APInvoiceOwnership.RequireScope(company);
        if(Company!=company || ActorUserId!=actor || InstanceId!=instance ||
            new[]{ActorUserId,InstanceId,ContractId}.Any(value=>string.IsNullOrWhiteSpace(value) || value.Length>128 || value.Any(char.IsControl)) ||
            !ProcessDefinitionRevision.IsValid(ProcessRevision) || !ProcessDefinitionRevision.IsValid(PreparationRevision) || Terms is null)
            throw new InvalidOperationException("The original company process and commercial source are required.");
        var source=Terms.Source.Source;
        Terms.Require(company,actor,source.FieldId,source.From,source.Through,Terms.Source.TransactionId);
        if(Terms.Source.Contract.SALES_CONTRACT_ID!=ContractId || Terms.Recorded is null ||
            CompanyAccountingSourceTypes.CommercialPreparationRevision(Terms.Recorded)!=PreparationRevision)
            throw new InvalidOperationException("The commercial source differs from the original process preparation.");
        if(CurrentNativeContract is not null)
        {
            if(CurrentNativeContract.SALES_CONTRACT_ID!=ContractId ||
                System.Text.Json.JsonSerializer.Serialize(Terms)!=System.Text.Json.JsonSerializer.Serialize(SalesContractTermsNativeProof.HistoricalReview(company,actor,CurrentNativeContract,PreparationRevision)))
                throw new InvalidOperationException("Historical commercial source must match its preserved native version exactly.");
        }
    }
}
