using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Models.Data.WorkOrder;

/// <summary>Recorded physical consumption for financial valuation, without executor command/rationale/recovery intent.</summary>
public sealed record CompanyWorkOrderMaterialCostSource(CompanyScope Company,AssetDatabaseScope Database,
    string WorkOrderId,string FieldId,string UsageId,string ItemId,string AllocationId,string StartId,
    string ExecutorUserId,decimal Quantity,string Unit,string Warehouse,string Location,
    DateTime UsedAtUtc,DateTime RecordedAtUtc,string Revision)
{
    public void Require()
    {
        PurchaseOrderOwnership.RequireScope(Company);
        if(Database is null || !LeaseSummaryValidator.Identifier(Database.ConnectionName) || !LeaseSummaryValidator.Identifier(Database.Fingerprint))throw new InvalidOperationException("Original material PRODUCTION binding is required.");
        if(new[]{WorkOrderId,FieldId,UsageId,ItemId,AllocationId,StartId,ExecutorUserId,Unit,Warehouse,Location}.Any(value=>!LeaseSummaryValidator.Identifier(value) || value.Length>128) ||
            Unit.Length>64 || Quantity<=0 || UsedAtUtc.Kind!=DateTimeKind.Utc || UsedAtUtc==default || RecordedAtUtc.Kind!=DateTimeKind.Utc || RecordedAtUtc<UsedAtUtc || !ProcessDefinitionRevision.IsValid(Revision))
            throw new InvalidOperationException("Original material consumption quantity/unit/location/time and native source audit are required.");
    }
    public static CompanyWorkOrderMaterialCostSource From(WORK_ORDER_MATERIAL_USAGE row,INVENTORY_TRANSACTION movement)
    {
        var command=CompanyWorkOrderMaterialUsageCommand.Read(row);command.RequireRecorded(row);
        if(JsonSerializer.Serialize(movement)!=JsonSerializer.Serialize(command.PrepareMovement(row.ROW_CREATED_DATE!.Value)))throw new InvalidOperationException("Original physical consumption movement and command differ.");
        var facts=CompanyWorkOrderMaterialUsageFacts.From(movement);
        var result=new CompanyWorkOrderMaterialCostSource(facts.Company,command.Review.Material.Source.Context.Database,facts.Task.WorkOrderId,facts.Task.FieldId,
            facts.UsageId,facts.ItemId,facts.AllocationId,facts.StartId,facts.ExecutorUserId,facts.Quantity,facts.Unit,facts.Warehouse,facts.Location,facts.UsedAtUtc,facts.RecordedAtUtc,row.RECORDED_REVISION);
        result.Require();return result;
    }
}
