using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.Accounting;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Models.Data.Accounting;

/// <summary>An exact recorded sales-contract match, not approval or recognition authority.</summary>
public sealed record RevenueSalesContractReview(FieldRevenueSourceReview Source,string TransactionId,SALES_CONTRACT Contract,string Revision)
{
    public void Require(CompanyScope company,string actor,string field,DateTime from,DateTime through,string transaction,string sourceRevision)
    {
        Source.Require(company,actor,field,from,through);
        var row=Source.Rows.SingleOrDefault(row=>row.REVENUE_TRANSACTION_ID==transaction);
        if(TransactionId!=transaction || Source.Revision!=sourceRevision || row is null || string.IsNullOrWhiteSpace(row.CONTRACT_ID) || Contract is null ||
            Contract.SALES_CONTRACT_ID!=row.CONTRACT_ID || Contract.SELLER_BA_ID!=company.BusinessAssociateId || Contract.ACTIVE_IND!="Y" ||
            Revision!=Digest(Source,TransactionId,Contract))throw new InvalidOperationException("Original revenue and owned sales contract evidence is unconfirmed.");
    }
    public static RevenueSalesContractReview Create(FieldRevenueSourceReview source,string transaction,SALES_CONTRACT contract)
    {
        source=CompanyInventorySourceReview.Copy(source);contract=CompanyInventorySourceReview.Copy(contract);
        var review=new RevenueSalesContractReview(source,transaction,contract,Digest(source,transaction,contract));
        review.Require(source.Company,source.ActorUserId,source.FieldId,source.From,source.Through,transaction,source.Revision);return review;
    }
    private static string Digest(FieldRevenueSourceReview source,string transaction,SALES_CONTRACT contract)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{source,transaction,contract}));
}
