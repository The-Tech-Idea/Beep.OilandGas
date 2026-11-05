using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace TheTechIdea.Data.OilGas;

/// <summary>Private original physical-use intent. Valuation and journal posting are separate operations.</summary>
public sealed class WORK_ORDER_MATERIAL_USAGE : ModelEntityBase
{
    [Key, Required, MaxLength(128)] public string USAGE_ID { get; set; } = "";
    [Required, MaxLength(64)] public string OWNER_TENANT_ID { get; set; } = "";
    [Required, MaxLength(128)] public string OWNER_COMPANY_BA_ID { get; set; } = "";
    [Required, MaxLength(128)] public string FIELD_ID { get; set; } = "";
    [Required, MaxLength(128)] public string WORK_ORDER_ID { get; set; } = "";
    [Required, MaxLength(128)] public string PROCESS_INSTANCE_ID { get; set; } = "";
    [Required, MaxLength(128)] public string INVENTORY_ITEM_ID { get; set; } = "";
    [Required, MaxLength(128)] public string ALLOCATION_ID { get; set; } = "";
    [Required, MaxLength(128)] public string START_ID { get; set; } = "";
    public decimal QUANTITY { get; set; }
    public DateTime USED_AT_UTC { get; set; }
    [Required, MaxLength(8 * 1024 * 1024)] public string COMMAND_JSON { get; set; } = "";
    [Required, MaxLength(64)] public string RECORDED_REVISION { get; set; } = "";
}

/// <summary>Public stock provenance without retained command, rationale or private start evidence.</summary>
public sealed record CompanyWorkOrderMaterialUsageFacts(string UsageId, Guid IntentId, CompanyScope Company,
    CompanyMaintenanceReceivingQuery Task, string ItemId, string AllocationId, string AllocationRevision,
    string StartId, string StartRevision, string ExecutorUserId, decimal Quantity, string Unit,
    string Warehouse, string Location, DateTime UsedAtUtc, DateTime RecordedAtUtc)
{
    public static string IdFor(CompanyScope company, Guid intent) => "wo-use-" +
        ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new { Company = company, IntentId = intent }));

    public void Require()
    {
        if(Company is null || Task is null)throw new InvalidOperationException("Original material usage company and task are required.");
        PurchaseOrderOwnership.RequireScope(Company); Task.Require();
        if (IntentId == Guid.Empty || UsageId != IdFor(Company, IntentId) || Quantity <= 0 ||
            new[] { ItemId, AllocationId, StartId, ExecutorUserId, Unit, Warehouse, Location }.Any(value =>
                string.IsNullOrWhiteSpace(value) || value.Length > 128 || value.Any(char.IsControl)) ||
            Unit.Length > 64 || !ProcessDefinitionRevision.IsValid(AllocationRevision) ||
            !ProcessDefinitionRevision.IsValid(StartRevision) || UsedAtUtc == default ||
            UsedAtUtc.Kind != DateTimeKind.Utc || RecordedAtUtc.Kind != DateTimeKind.Utc || RecordedAtUtc < UsedAtUtc)
            throw new InvalidOperationException("Original material use requires exact reservation/start, positive quantity/unit/location and UTC provenance.");
    }

    public static CompanyWorkOrderMaterialUsageFacts From(INVENTORY_TRANSACTION movement)
    {
        if (movement?.WORK_ORDER_MATERIAL_USAGE_JSON is not { Length: > 0 and <= 16384 } json)
            throw new InvalidOperationException("Original material movement provenance is unavailable.");
        CompanyWorkOrderMaterialUsageFacts facts;
        try { facts = JsonSerializer.Deserialize<CompanyWorkOrderMaterialUsageFacts>(json) ?? throw new InvalidOperationException("Material use facts are unavailable."); }
        catch (JsonException error) { throw new InvalidOperationException("Material use facts are malformed.", error); }
        facts.Require();
        if (movement.INVENTORY_TRANSACTION_ID != facts.UsageId || movement.INVENTORY_ITEM_ID != facts.ItemId ||
            movement.TRANSACTION_TYPE != "WORK_ORDER_MATERIAL_USE" || movement.QUANTITY != -facts.Quantity ||
            movement.TRANSACTION_DATE != facts.UsedAtUtc || movement.REFERENCE_NUMBER != facts.AllocationId ||
            movement.ACTIVE_IND != "Y" || movement.ROW_CREATED_BY != facts.ExecutorUserId ||
            movement.ROW_CHANGED_BY != facts.ExecutorUserId || movement.ROW_CREATED_DATE != facts.RecordedAtUtc ||
            movement.ROW_CHANGED_DATE != facts.RecordedAtUtc || movement.UNIT_COST is not null ||
            movement.TOTAL_COST is not null || movement.PURCHASE_RECEIPT_STOCK_JSON is not null ||
            movement.SOURCE != "OILGAS_WORK_ORDER_MATERIAL_USE" || movement.NOTES is not null ||
            movement.WORK_ORDER_MATERIAL_USAGE_JSON != JsonSerializer.Serialize(facts))
            throw new InvalidOperationException("The physical stock movement differs from its original public usage provenance.");
        return facts;
    }
}

public sealed record CompanyWorkOrderMaterialUsageReview(CompanyWorkOrderRecordedStart Start,
    CompanyWorkOrderMaterialReservationFacts Reservation, CompanyWorkOrderMaterialAllocationReview Material,
    DateTime ValidToUtc, DateTime ComparedAtUtc)
{
    public void Require()
    {
        if (Start is null || Reservation is null || Material is null) throw new InvalidOperationException("Original start/reservation and complete current stock evidence are required.");
        Start.Require(Start.Company, Start.Permit); Reservation.Require(Start.Company, Start.Permit.Task, Reservation.IntentId);
        Material.Require(); var source = Material.Source;
        source.Require(Start.Company, Start.ExecutorUserId, new(Start.Permit.Task, Reservation.ItemId, Reservation.Quantity, Reservation.Unit));
        var selected = Material.Allocations.SingleOrDefault(row => row.Id == Reservation.AllocationId);
        if (selected is null || selected.Revision != Reservation.Revision || selected.Quantity != Reservation.Quantity ||
            selected.Unit != Reservation.Unit || selected.Warehouse != Reservation.Warehouse || selected.Location != Reservation.Location ||
            selected.ValidFromUtc != Reservation.ValidFromUtc || selected.ValidToUtc != Reservation.ValidToUtc ||
            ComparedAtUtc.Kind != DateTimeKind.Utc || ComparedAtUtc < Material.ComparedAtUtc || ComparedAtUtc < Start.RecordedAtUtc ||
            ComparedAtUtc < Reservation.ValidFromUtc || ValidToUtc.Kind != DateTimeKind.Utc ||
            ValidToUtc > Reservation.ValidToUtc || ComparedAtUtc >= ValidToUtc ||
            Material.Releases?.Any(row => row.AllocationId == Reservation.AllocationId && row.ReleasedAtUtc <= ComparedAtUtc) == true)
            throw new InvalidOperationException("Current company/executor/start/reservation/location/window or release evidence changed.");
    }
    public decimal RemainingQuantity()
    {
        Require(); return Reservation.Quantity - (Material.Usages ?? []).Where(row => row.AllocationId == Reservation.AllocationId).Sum(row => row.Quantity);
    }
    public string SourceRevision() => ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new {
        Start, Reservation, Material.Source.Context, Material.Source.Query, Material.Source.WorkOrder.SnapshotRevision,
        InventoryRevision = Material.Source.Inventory.Revision, Material.Allocations, Material.Releases, Material.Usages,
        Material.ReservedQuantity, Material.AvailableQuantity, ValidToUtc }));
}

public sealed record CompanyWorkOrderMaterialUsageCommand(CompanyWorkOrderMaterialUsageReview Review, Guid IntentId,
    decimal Quantity, string Unit, DateTime UsedAtUtc, string Reason)
{
    public const int MaximumBodyLength = 8 * 1024 * 1024;
    public void Require()
    {
        if (Review is null) throw new ArgumentException("Original current material-use review is required.");
        Review.Require();
        if (IntentId == Guid.Empty || Quantity <= 0 || Quantity > Review.RemainingQuantity() || Unit != Review.Reservation.Unit ||
            UsedAtUtc.Kind != DateTimeKind.Utc || UsedAtUtc < Review.ComparedAtUtc || UsedAtUtc >= Review.ValidToUtc ||
            string.IsNullOrWhiteSpace(Reason) || Reason.Length > 4096 || Reason.Any(char.IsControl) ||
            JsonSerializer.Serialize(this).Length > MaximumBodyLength)
            throw new ArgumentException("Retain the exact usage intent, available reserved quantity/unit, explicit UTC and rationale.");
    }
    public string UsageId() => CompanyWorkOrderMaterialUsageFacts.IdFor(Review.Start.Company, IntentId);
    public void RequireCurrent(CompanyWorkOrderMaterialUsageReview current)
    {
        Require(); current.Require();
        if (current.ComparedAtUtc < Review.ComparedAtUtc || current.SourceRevision() != Review.SourceRevision())
            throw new InvalidOperationException("Original start/reservation/current stock population changed before material use.");
    }
    public WORK_ORDER_MATERIAL_USAGE Prepare(DateTime utc)
    {
        Require(); if (utc.Kind != DateTimeKind.Utc || utc < UsedAtUtc || utc >= Review.ValidToUtc)
            throw new InvalidOperationException("Material use must be recorded within the confirmed original window.");
        var start = Review.Start; var task = start.Permit.Task;
        var row = new WORK_ORDER_MATERIAL_USAGE { USAGE_ID = UsageId(), OWNER_TENANT_ID = start.Company.TenantId,
            OWNER_COMPANY_BA_ID = start.Company.BusinessAssociateId, FIELD_ID = task.FieldId, WORK_ORDER_ID = task.WorkOrderId,
            PROCESS_INSTANCE_ID = task.InstanceId, INVENTORY_ITEM_ID = Review.Reservation.ItemId,
            ALLOCATION_ID = Review.Reservation.AllocationId, START_ID = start.StartId, QUANTITY = Quantity, USED_AT_UTC = UsedAtUtc,
            COMMAND_JSON = JsonSerializer.Serialize(this), ACTIVE_IND = "Y", SOURCE = "OILGAS_WORK_ORDER_MATERIAL_USE",
            PPDM_GUID = IntentId.ToString("D"), ROW_CREATED_BY = start.ExecutorUserId, ROW_CHANGED_BY = start.ExecutorUserId,
            ROW_CREATED_DATE = utc, ROW_CHANGED_DATE = utc };
        row.RECORDED_REVISION = Digest(row); return row;
    }
    public INVENTORY_TRANSACTION PrepareMovement(DateTime utc)
    {
        _ = Prepare(utc); var start = Review.Start; var reservation = Review.Reservation;
        var facts = new CompanyWorkOrderMaterialUsageFacts(UsageId(), IntentId, start.Company, start.Permit.Task,
            reservation.ItemId, reservation.AllocationId, reservation.Revision, start.StartId, start.RecordedRevision,
            start.ExecutorUserId, Quantity, Unit, reservation.Warehouse, reservation.Location, UsedAtUtc, utc);
        var movement = new INVENTORY_TRANSACTION { INVENTORY_TRANSACTION_ID = UsageId(), INVENTORY_ITEM_ID = reservation.ItemId,
            TRANSACTION_TYPE = "WORK_ORDER_MATERIAL_USE", TRANSACTION_DATE = UsedAtUtc, QUANTITY = -Quantity,
            REFERENCE_NUMBER = reservation.AllocationId, WORK_ORDER_MATERIAL_USAGE_JSON = JsonSerializer.Serialize(facts),
            ACTIVE_IND = "Y", SOURCE = "OILGAS_WORK_ORDER_MATERIAL_USE", PPDM_GUID = IntentId.ToString("D"),
            ROW_CREATED_BY = start.ExecutorUserId, ROW_CHANGED_BY = start.ExecutorUserId, ROW_CREATED_DATE = utc, ROW_CHANGED_DATE = utc };
        _ = CompanyWorkOrderMaterialUsageFacts.From(movement); return movement;
    }
    public static string Digest(WORK_ORDER_MATERIAL_USAGE row)
    { var copy = CompanyInventorySourceReview.Copy(row); copy.RECORDED_REVISION = ""; return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy)); }
    public static CompanyWorkOrderMaterialUsageCommand Read(WORK_ORDER_MATERIAL_USAGE row)
    {
        if (row?.COMMAND_JSON is not { Length: > 0 and <= MaximumBodyLength } body) throw new InvalidOperationException("Original private usage command is unavailable.");
        try { return JsonSerializer.Deserialize<CompanyWorkOrderMaterialUsageCommand>(body) ?? throw new InvalidOperationException("Original usage command is unavailable."); }
        catch (JsonException error) { throw new InvalidOperationException("Original usage command is malformed.", error); }
    }
    public void RequireRecorded(WORK_ORDER_MATERIAL_USAGE row)
    {
        Require(); if (row?.ROW_CREATED_DATE is not {} utc || JsonSerializer.Serialize(row) != JsonSerializer.Serialize(Prepare(utc)))
            throw new InvalidOperationException("Original private material usage company/task/body/audit changed.");
    }
}

public sealed record CompanyWorkOrderMaterialUsageReference(CompanyWorkOrderExecutionStartReference Start, Guid IntentId)
{
    public void Require() { Start.Require(); if (IntentId == Guid.Empty) throw new ArgumentException("Retain the original material usage reference."); }
}

/// <summary>Current execution authority from the original native start; future submission inspections may remain unperformed.</summary>
public sealed record CompanyWorkOrderMaterialUseExecutionProof(CompanyWorkOrderSubmissionInspectionReview Inspections,
    CompanyWorkOrderMaterialReservationFacts Reservation)
{
    public void Require(CompanyScope company, string actor, CompanyWorkOrderExecutionStartReference reference)
    {
        Inspections.Require(company, actor, reference); Reservation.Require(company, reference.Permit.Task, Reservation.IntentId);
        if (Inspections.Current.Blockers.Count != 0 || Inspections.ComparedAtUtc >= Inspections.Current.Approval.ValidToUtc ||
            Inspections.ComparedAtUtc < Reservation.ValidFromUtc || Inspections.ComparedAtUtc >= Reservation.ValidToUtc)
            throw new InvalidOperationException("Current original order/pre-start requirements, reservation and permit window must permit physical material use.");
    }
}
public sealed record CompanyWorkOrderMaterialUsageRecovery(CompanyWorkOrderMaterialUsageCommand Command,
    WORK_ORDER_MATERIAL_USAGE Recorded, INVENTORY_TRANSACTION Movement)
{
    public void Require(CompanyScope company, string actor, CompanyWorkOrderMaterialUsageReference reference)
    {
        reference.Require(); Command.Require(); var start = Command.Review.Start;
        if (start.Company != company || start.ExecutorUserId != actor || Command.IntentId != reference.IntentId ||
            start.Permit != reference.Start.Permit || start.IntentId != reference.Start.IntentId)
            throw new UnauthorizedAccessException("The original private usage belongs to another company/account/start/reference.");
        Command.RequireRecorded(Recorded);
        if (JsonSerializer.Serialize(Movement) != JsonSerializer.Serialize(Command.PrepareMovement(Recorded.ROW_CREATED_DATE!.Value)))
            throw new InvalidOperationException("Original stock movement differs from its private usage intent.");
    }
}
