using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.ProductionAccounting.Models;

/// <summary>The exact recorded preparation and current source of an admitted company delivery process.</summary>
public sealed record CompanyDeliveryProcessSourceReview(CompanyScope Company,string ActorUserId,string InstanceId,string TicketId,
    string PreparationRevision,string ProcessRevision,RevenueDeliveryAcceptanceReadReview Delivery)
{
    [System.Text.Json.Serialization.JsonIgnore] public bool IsHistorical=>Delivery.IsHistorical;
    public void Require(CompanyScope company,string actor,string instance)
    {
        APInvoiceOwnership.RequireScope(company);
        if(Company!=company || ActorUserId!=actor || InstanceId!=instance || Delivery is null ||
            new[]{ActorUserId,InstanceId,TicketId}.Any(value=>string.IsNullOrWhiteSpace(value) || value.Length>128 || value.Any(char.IsControl)) ||
            !ProcessDefinitionRevision.IsValid(PreparationRevision) || !ProcessDefinitionRevision.IsValid(ProcessRevision))
            throw new InvalidOperationException("The original company delivery process and preparation are required.");
        Delivery.Require();var source=Delivery.Source.Source.Source;
        if(source.Company!=company || source.ActorUserId!=actor || Delivery.Source.TicketId!=TicketId || Delivery.Recorded is null ||
            CompanyAccountingSourceTypes.DeliveryPreparationRevision(Delivery.Recorded)!=PreparationRevision)
            throw new InvalidOperationException("The delivery source differs from the original process preparation.");
    }
}
