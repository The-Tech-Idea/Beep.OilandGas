using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data;
namespace TheTechIdea.Data.OilGas;

/// <summary>Append-only original work-order reservation; never physical issue or stock consumption.</summary>
public sealed class WORK_ORDER_MATERIAL_ALLOCATION : ModelEntityBase
{
    [Key,Required,MaxLength(128)] public string ALLOCATION_ID {get;set;}="";
    [Required,MaxLength(64)] public string OWNER_TENANT_ID {get;set;}="";
    [Required,MaxLength(128)] public string OWNER_COMPANY_BA_ID {get;set;}="";
    [Required,MaxLength(128)] public string INVENTORY_ITEM_ID {get;set;}="";
    [Required,MaxLength(128)] public string FIELD_ID {get;set;}="";
    [Required,MaxLength(128)] public string WORK_ORDER_ID {get;set;}="";
    [Required,MaxLength(128)] public string PROCESS_INSTANCE_ID {get;set;}="";
    [Required,MaxLength(64)] public string QUANTITY_UNIT {get;set;}="";
    [Required,MaxLength(128)] public string WAREHOUSE_ID {get;set;}="";
    [Required,MaxLength(128)] public string LOCATION_ID {get;set;}="";
    public decimal QUANTITY {get;set;}
    public DateTime VALID_FROM_UTC {get;set;}
    public DateTime VALID_TO_UTC {get;set;}
    [Required,MaxLength(2097152)] public string COMMAND_JSON {get;set;}="";
    [Required,MaxLength(64)] public string RECORDED_REVISION {get;set;}="";
}

public sealed record CompanyWorkOrderMaterialAllocationReference(string Id,string Revision,CompanyScope Company,string ItemId,string Unit,string Warehouse,string Location,decimal Quantity,DateTime ValidFromUtc,DateTime ValidToUtc)
{
    public static CompanyWorkOrderMaterialAllocationReference From(WORK_ORDER_MATERIAL_ALLOCATION row)
    {var command=CompanyWorkOrderMaterialAllocationCommand.Read(row);command.RequireRecorded(row);return new(row.ALLOCATION_ID,row.RECORDED_REVISION,new(row.OWNER_TENANT_ID,row.OWNER_COMPANY_BA_ID),row.INVENTORY_ITEM_ID,row.QUANTITY_UNIT,row.WAREHOUSE_ID,row.LOCATION_ID,row.QUANTITY,row.VALID_FROM_UTC,row.VALID_TO_UTC);}
}
public sealed record CompanyWorkOrderMaterialAllocationReview(CompanyWorkOrderMaterialSourceReview Source,
    IReadOnlyList<CompanyWorkOrderMaterialAllocationReference> Allocations,DateTime ComparedAtUtc,decimal ReservedQuantity,decimal AvailableQuantity,string Revision)
{
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public IReadOnlyList<CompanyWorkOrderMaterialReleaseReference>? Releases {get;init;}
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public IReadOnlyList<CompanyWorkOrderMaterialUsageFacts>? Usages {get;init;}
    public const int MaximumAllocations=4096;
    public static CompanyWorkOrderMaterialAllocationReview Create(CompanyWorkOrderMaterialSourceReview source,IReadOnlyList<WORK_ORDER_MATERIAL_ALLOCATION> rows,DateTime utc)
    {
        if(rows is null || rows.Count>MaximumAllocations || rows.Any(row=>row is null))throw new InvalidOperationException("Complete bounded allocation rows are required; no rows are truncated.");var references=rows.Select(CompanyWorkOrderMaterialAllocationReference.From).ToArray();return CreateReferences(source,references,utc);
    }
    public static CompanyWorkOrderMaterialAllocationReview Create(CompanyWorkOrderMaterialSourceReview source,IReadOnlyList<WORK_ORDER_MATERIAL_ALLOCATION> rows,IReadOnlyList<WORK_ORDER_MATERIAL_RELEASE> releases,DateTime utc)
    {
        if(releases is null || releases.Count>MaximumAllocations || releases.Any(r=>r is null) || releases.Select(r=>r.ALLOCATION_ID).Distinct(StringComparer.Ordinal).Count()!=releases.Count)throw new InvalidOperationException("Complete unique release membership is required; no rows are truncated.");
        if(rows is null || rows.Count>MaximumAllocations || rows.Any(row=>row is null))throw new InvalidOperationException("Complete bounded original allocation rows are required.");var allocationReferences=rows.Select(CompanyWorkOrderMaterialAllocationReference.From).ToArray();var references=releases.OrderBy(r=>r.ALLOCATION_ID,StringComparer.Ordinal).Select(row=>CompanyWorkOrderMaterialReleaseReference.From(row,rows.SingleOrDefault(a=>a.ALLOCATION_ID==row.ALLOCATION_ID)??throw new InvalidOperationException("An original allocation for the release is unavailable."))).ToArray();
        return CreateReferences(source,allocationReferences,utc,references.Length==0?null:references);
    }
    private static CompanyWorkOrderMaterialAllocationReview CreateReferences(CompanyWorkOrderMaterialSourceReview source,IReadOnlyList<CompanyWorkOrderMaterialAllocationReference> rows,DateTime utc,IReadOnlyList<CompanyWorkOrderMaterialReleaseReference>? releases=null)
    {
        source.Require(source.Context.Company,source.Context.ActorUserId,source.Query);
        if(utc==default || utc.Kind!=DateTimeKind.Utc || rows is null || rows.Count>MaximumAllocations)throw new InvalidOperationException("Complete bounded allocation evidence and UTC comparison are required; no rows are truncated.");
        var item=source.Inventory.Item;
        if(item.ACTIVE_IND!="Y" || item.QUANTITY_ON_HAND is null or <0 || string.IsNullOrWhiteSpace(item.WAREHOUSE_ID) || string.IsNullOrWhiteSpace(item.LOCATION_ID) || item.UNIT_OF_MEASURE!=source.Query.QuantityUnit)throw new InvalidOperationException("Original stock quantity, unit and location must be confirmed before allocation.");
        var ordered=rows.OrderBy(r=>r?.Id,StringComparer.Ordinal).ToArray();
        if(ordered.Any(r=>r is null) || ordered.Select(r=>r.Id).Distinct(StringComparer.Ordinal).Count()!=ordered.Length)throw new InvalidOperationException("Allocation identities are unavailable or ambiguous.");
        foreach(var row in ordered)
        {
            if(string.IsNullOrWhiteSpace(row.Id) || !ProcessDefinitionRevision.IsValid(row.Revision) || row.Quantity<=0 || row.ValidFromUtc.Kind!=DateTimeKind.Utc || row.ValidToUtc.Kind!=DateTimeKind.Utc || row.ValidToUtc<=row.ValidFromUtc)throw new InvalidOperationException("Recorded allocation reference is malformed.");
            if(row.Company!=source.Context.Company || row.ItemId!=item.INVENTORY_ITEM_ID || row.Unit!=item.UNIT_OF_MEASURE || row.Warehouse!=item.WAREHOUSE_ID || row.Location!=item.LOCATION_ID)throw new InvalidOperationException("Complete current item allocation ownership, unit or location changed.");
        }
        if(releases is not null && (releases.Count>MaximumAllocations || releases.Any(r=>r is null) || releases.Select(r=>r.AllocationId).Distinct(StringComparer.Ordinal).Count()!=releases.Count))throw new InvalidOperationException("Original release references are ambiguous.");
        foreach(var release in releases??[]){var allocation=ordered.SingleOrDefault(r=>r.Id==release.AllocationId);if(allocation is null || !ProcessDefinitionRevision.IsValid(release.Revision) || release.Quantity!=allocation.Quantity || release.ReleasedAtUtc==default || release.ReleasedAtUtc.Kind!=DateTimeKind.Utc)throw new InvalidOperationException("Release reference quantity/time/membership is unconfirmed.");}
        // Physical issues already reduced on-hand. Deduct only the unconsumed reservation balance.
        // Complete movement membership comes from the same inventory source as the recorded on-hand quantity.
        var usages=source.Inventory.Transactions.Where(row=>row.TRANSACTION_TYPE=="WORK_ORDER_MATERIAL_USE" || row.WORK_ORDER_MATERIAL_USAGE_JSON is not null)
            .Select(CompanyWorkOrderMaterialUsageFacts.From).OrderBy(row=>row.UsageId,StringComparer.Ordinal).ToArray();
        if(usages.Length>MaximumAllocations || usages.Select(row=>row.UsageId).Distinct(StringComparer.Ordinal).Count()!=usages.Length)
            throw new InvalidOperationException("Complete unique bounded material usage membership is required; no rows are truncated.");
        foreach(var usage in usages)
        {
            var allocation=ordered.SingleOrDefault(row=>row.Id==usage.AllocationId);
            if(allocation is null || usage.Company!=allocation.Company || usage.ItemId!=allocation.ItemId || usage.AllocationRevision!=allocation.Revision ||
                usage.Unit!=allocation.Unit || usage.Warehouse!=allocation.Warehouse || usage.Location!=allocation.Location || usage.UsedAtUtc<allocation.ValidFromUtc ||
                usage.UsedAtUtc>=allocation.ValidToUtc || usage.RecordedAtUtc>utc || usage.UsedAtUtc>utc ||
                (releases??[]).Any(release=>release.AllocationId==usage.AllocationId && release.ReleasedAtUtc<=usage.RecordedAtUtc))
                throw new InvalidOperationException("Original material usage reservation/company/unit/location/window/release membership differs.");
        }
        foreach(var allocation in ordered)
            if(usages.Where(row=>row.AllocationId==allocation.Id).Sum(row=>row.Quantity)>allocation.Quantity)
                throw new InvalidOperationException("Recorded physical usage exceeds its original reservation.");
        var reserved=ordered.Where(r=>r.ValidToUtc>utc && !(releases??[]).Any(release=>release.AllocationId==r.Id && release.ReleasedAtUtc<=utc))
            .Sum(r=>r.Quantity-usages.Where(usage=>usage.AllocationId==r.Id).Sum(usage=>usage.Quantity));var available=item.QUANTITY_ON_HAND.Value-reserved;
        if(available<0)throw new InvalidOperationException("Recorded outstanding reservations exceed on-hand stock.");
        var revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Source=source.WorkOrder.SnapshotRevision,Inventory=source.Inventory.Revision,Context=source.Context,Query=source.Query,Rows=ordered,ComparedAtUtc=utc,Reserved=reserved,Available=available}));
        if(releases is {Count:>0})revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Original=revision,Releases=releases}));
        if(usages.Length>0)revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Original=revision,Usages=usages}));
        return new(CompanyInventorySourceReview.Copy(source),ordered,utc,reserved,available,revision){Releases=releases is {Count:>0}?releases:null,Usages=usages.Length>0?usages:null};
    }
    public void Require()
    {
        if(Source is null || Allocations is null)throw new InvalidOperationException("Original allocation source is required.");var expected=CreateReferences(Source,Allocations,ComparedAtUtc,Releases);
        if(ReservedQuantity!=expected.ReservedQuantity || AvailableQuantity!=expected.AvailableQuantity || Revision!=expected.Revision || JsonSerializer.Serialize(Usages)!=JsonSerializer.Serialize(expected.Usages))throw new InvalidOperationException("The complete original allocation review changed.");
    }
}

public sealed record CompanyWorkOrderMaterialAllocationCommand(CompanyWorkOrderMaterialAllocationReview Review,Guid IntentId,DateTime ValidFromUtc,DateTime ValidToUtc,string Reason)
{
    public void Require()
    {
        if(Review is null)throw new ArgumentException("Original allocation review is required.");Review.Require();
        if(IntentId==Guid.Empty || ValidFromUtc.Kind!=DateTimeKind.Utc || ValidToUtc.Kind!=DateTimeKind.Utc || ValidFromUtc<Review.ComparedAtUtc || ValidToUtc<=ValidFromUtc || string.IsNullOrWhiteSpace(Reason) || Reason.Length>2048 || Reason.Any(char.IsControl) || Review.AvailableQuantity<Review.Source.Query.RequiredQuantity)throw new ArgumentException("Retain an original allocation identity, confirmed capacity, explicit increasing UTC effectivity and rationale.");
    }
    public string AllocationId()=>"wo-material-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=Review.Source.Context.Company,IntentId}));
    public WORK_ORDER_MATERIAL_ALLOCATION Prepare(DateTime utc)
    {
        Require();if(utc.Kind!=DateTimeKind.Utc || utc<Review.ComparedAtUtc || utc>=ValidToUtc)throw new InvalidOperationException("Allocation effectivity or recorded UTC time is stale.");
        var source=Review.Source;var item=source.Inventory.Item;var body=JsonSerializer.Serialize(this);if(body.Length>2097152)throw new InvalidOperationException("Original allocation evidence exceeds its retained capacity.");
        var row=new WORK_ORDER_MATERIAL_ALLOCATION{ALLOCATION_ID=AllocationId(),OWNER_TENANT_ID=source.Context.Company.TenantId,OWNER_COMPANY_BA_ID=source.Context.Company.BusinessAssociateId,INVENTORY_ITEM_ID=source.Query.ItemId,FIELD_ID=source.Query.Task.FieldId,WORK_ORDER_ID=source.Query.Task.WorkOrderId,PROCESS_INSTANCE_ID=source.Query.Task.InstanceId,QUANTITY=source.Query.RequiredQuantity,QUANTITY_UNIT=source.Query.QuantityUnit,WAREHOUSE_ID=item.WAREHOUSE_ID!,LOCATION_ID=item.LOCATION_ID!,VALID_FROM_UTC=ValidFromUtc,VALID_TO_UTC=ValidToUtc,COMMAND_JSON=body,PPDM_GUID=IntentId.ToString("D"),ACTIVE_IND="Y",ROW_CREATED_BY=source.Context.ActorUserId,ROW_CHANGED_BY=source.Context.ActorUserId,ROW_CREATED_DATE=utc,ROW_CHANGED_DATE=utc,SOURCE="OILGAS_WORK_ORDER_MATERIAL_RESERVATION"};row.RECORDED_REVISION=Digest(row);return row;
    }
    public static string Digest(WORK_ORDER_MATERIAL_ALLOCATION row){var copy=CompanyInventorySourceReview.Copy(row);copy.RECORDED_REVISION="";return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static CompanyWorkOrderMaterialAllocationCommand Read(WORK_ORDER_MATERIAL_ALLOCATION row)
    {
        if(row is null || string.IsNullOrWhiteSpace(row.COMMAND_JSON) || row.COMMAND_JSON.Length>2097152)throw new InvalidOperationException("Original retained allocation command is unavailable.");
        try{return JsonSerializer.Deserialize<CompanyWorkOrderMaterialAllocationCommand>(row.COMMAND_JSON)??throw new InvalidOperationException("Original allocation body is unavailable.");}catch(JsonException error){throw new InvalidOperationException("Original allocation evidence is malformed.",error);}
    }
    public void RequireRecorded(WORK_ORDER_MATERIAL_ALLOCATION row)
    {
        Require();if(row?.ROW_CREATED_DATE is not {} utc || utc.Kind!=DateTimeKind.Utc)throw new InvalidOperationException("Original allocation audit is unavailable.");var expected=Prepare(utc);
        if(JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("Original allocation owner/body/source/audit evidence changed.");
    }
}
