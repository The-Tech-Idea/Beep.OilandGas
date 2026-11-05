using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.Accounting;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Models.Data.Accounting;

/// <summary>Exact native allocation/run-ticket evidence; certification is not delivery approval or billing authorization.</summary>
public sealed record RevenueDeliverySourceReview(RevenueSalesContractReview Source,ALLOCATION_RESULT Allocation,RUN_TICKET Ticket,string Revision)
{
    [JsonIgnore] public string TicketId=>Ticket.RUN_TICKET_ID;
    [JsonIgnore] public decimal? NetVolume=>Ticket.NET_VOLUME;
    [JsonIgnore] public string? VolumeUnit=>Ticket.VOLUME_OUOM;
    [JsonIgnore] public string? TicketStatus=>Ticket.TICKET_STATUS;
    [JsonIgnore] public DateTime? TicketDate=>Ticket.TICKET_DATE_TIME;
    [JsonIgnore] public bool HasCertifiedMeasuredQuantity=>TicketStatus=="CERTIFIED" && NetVolume>0 && !string.IsNullOrWhiteSpace(VolumeUnit);
    public void Require(RevenueSalesContractReview original)
    {
        var source=original.Source;
        original.Require(source.Company,source.ActorUserId,source.FieldId,source.From,source.Through,original.TransactionId,source.Revision);
        if(JsonSerializer.Serialize(Source)!=JsonSerializer.Serialize(original) || Allocation is null || Ticket is null)
            throw new InvalidOperationException("The original delivery source changed.");
        var revenue=source.Rows.Single(row=>row.REVENUE_TRANSACTION_ID==original.TransactionId);
        if(string.IsNullOrWhiteSpace(revenue.ALLOCATION_RESULT_ID) || Allocation.ALLOCATION_RESULT_ID!=revenue.ALLOCATION_RESULT_ID || Allocation.ACTIVE_IND!="Y" ||
            string.IsNullOrWhiteSpace(TicketId) || TicketId.Length>128 || TicketId.Any(char.IsControl) || Allocation.ALLOCATION_REQUEST_ID!=TicketId || Ticket.ACTIVE_IND!="Y" || Ticket.FIELD_ID!=source.FieldId ||
            Revision!=Digest(Source,Allocation,Ticket))throw new InvalidOperationException("Exact recorded allocation and FIELD run-ticket membership is unconfirmed.");
    }
    public static RevenueDeliverySourceReview Create(RevenueSalesContractReview source,ALLOCATION_RESULT allocation,RUN_TICKET ticket)
    {
        source=CompanyInventorySourceReview.Copy(source);allocation=CompanyInventorySourceReview.Copy(allocation);ticket=CompanyInventorySourceReview.Copy(ticket);
        var result=new RevenueDeliverySourceReview(source,allocation,ticket,Digest(source,allocation,ticket));result.Require(source);return result;
    }
    private static string Digest(RevenueSalesContractReview source,ALLOCATION_RESULT allocation,RUN_TICKET ticket)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{source,allocation,ticket}));
}
