using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.Models.Data.WorkOrder;

public sealed record CompanyWorkOrderMaterialReadinessQuery(CompanyWorkOrderExecutionPermitReference Permit,Guid AllocationIntentId)
{
    public void Require(){if(Permit is null || AllocationIntentId==Guid.Empty)throw new ArgumentException("Select the original pending permit and recorded material allocation intent.");Permit.Require();}
}
/// <summary>Validated public reservation facts; private original command and rationale are not returned to another actor.</summary>
public sealed record CompanyWorkOrderMaterialReservationFacts(CompanyScope Company,CompanyMaintenanceReceivingQuery Task,Guid IntentId,string AllocationId,string Revision,string ReservedBy,string ItemId,decimal Quantity,string Unit,string Warehouse,string Location,DateTime ValidFromUtc,DateTime ValidToUtc)
{
    public static string IdFor(CompanyScope company,Guid id)=>"wo-material-"+ProcessDefinitionRevision.FromJson(System.Text.Json.JsonSerializer.Serialize(new{Company=company,IntentId=id}));
    public static CompanyWorkOrderMaterialReservationFacts From(WORK_ORDER_MATERIAL_ALLOCATION row)
    {
        var original=CompanyWorkOrderMaterialAllocationCommand.Read(row);original.RequireRecorded(row);var source=original.Review.Source;
        return new(source.Context.Company,source.Query.Task,original.IntentId,row.ALLOCATION_ID,row.RECORDED_REVISION,row.ROW_CREATED_BY,row.INVENTORY_ITEM_ID,row.QUANTITY,row.QUANTITY_UNIT,row.WAREHOUSE_ID,row.LOCATION_ID,row.VALID_FROM_UTC,row.VALID_TO_UTC);
    }
    public void Require(CompanyScope company,CompanyMaintenanceReceivingQuery task,Guid intent)
    {
        task.Require();if(Company!=company || Task!=task || IntentId!=intent || intent==Guid.Empty || AllocationId!=IdFor(company,intent) || !ProcessDefinitionRevision.IsValid(Revision) || Quantity<=0 || new[]{ReservedBy,ItemId,Unit,Warehouse,Location}.Any(value=>string.IsNullOrWhiteSpace(value) || value.Any(char.IsControl)) || ValidFromUtc.Kind!=DateTimeKind.Utc || ValidToUtc.Kind!=DateTimeKind.Utc || ValidToUtc<=ValidFromUtc)throw new InvalidOperationException("Original public reservation owner/task/item/quantity/unit/location/UTC evidence is unavailable.");
    }
}
/// <summary>Current reservation and physical inventory comparison; never permit approval or stock issue.</summary>
public sealed record CompanyWorkOrderMaterialReadinessReview(CompanyWorkOrderMaterialReadinessQuery Query,CompanyWorkOrderExecutionPermitApprovalReview PermitReview,CompanyWorkOrderMaterialReservationFacts Reservation,CompanyWorkOrderMaterialAllocationReview CurrentMaterial,DateTime ComparedAtUtc,IReadOnlyList<string> Blockers)
{
    public static string[] GetBlockers(CompanyWorkOrderExecutionPermitApprovalReview permit,CompanyWorkOrderMaterialReservationFacts reservation,CompanyWorkOrderMaterialAllocationReview material,DateTime utc)
    {
        if(utc==default || utc.Kind!=DateTimeKind.Utc || utc<material.ComparedAtUtc || utc<permit.ComparedAtUtc)throw new InvalidOperationException("A current UTC readiness comparison is required.");var result=new List<string>();
        if(permit.Proposal.MaterialReference!=reservation.AllocationId)result.Add("The pending permit material reference does not identify this exact recorded reservation.");
        if(reservation.ValidFromUtc>permit.Proposal.ValidFromUtc || reservation.ValidToUtc<permit.Proposal.ValidToUtc)result.Add("The reservation validity does not cover the complete reported permit window.");
        if(utc<reservation.ValidFromUtc)result.Add("The material reservation validity has not started.");if(utc>=reservation.ValidToUtc)result.Add("The material reservation has expired.");
        if(material.Releases?.Any(r=>r.AllocationId==reservation.AllocationId && r.ReleasedAtUtc<=utc)==true)result.Add("The original material reservation has been released.");
        if(material.Source.Inventory.Item.QUANTITY_ON_HAND<material.ReservedQuantity)result.Add("Recorded on-hand stock does not cover complete current outstanding reservations.");
        return result.ToArray();
    }
    public void Require(CompanyScope company,string actor,CompanyWorkOrderMaterialReadinessQuery query)
    {
        query.Require();if(Query!=query || PermitReview is null || Reservation is null || CurrentMaterial is null || Blockers is null)throw new InvalidOperationException("The complete current material readiness comparison is unavailable.");PermitReview.Require(company,actor,query.Permit);Reservation.Require(company,query.Permit.Task,query.AllocationIntentId);CurrentMaterial.Require();var source=CurrentMaterial.Source;source.Require(company,actor,new(query.Permit.Task,Reservation.ItemId,Reservation.Quantity,Reservation.Unit));
        var selected=CurrentMaterial.Allocations.SingleOrDefault(r=>r.Id==Reservation.AllocationId);
        if(source.WorkOrder.SnapshotRevision!=PermitReview.CurrentSource.SnapshotRevision || selected is null || selected.Revision!=Reservation.Revision || selected.Quantity!=Reservation.Quantity || selected.Unit!=Reservation.Unit || selected.Warehouse!=Reservation.Warehouse || selected.Location!=Reservation.Location || selected.ValidFromUtc!=Reservation.ValidFromUtc || selected.ValidToUtc!=Reservation.ValidToUtc || !Blockers.SequenceEqual(GetBlockers(PermitReview,Reservation,CurrentMaterial,ComparedAtUtc),StringComparer.Ordinal))throw new InvalidOperationException("Current permit/order/reservation/material membership or complete blockers changed.");
    }
}