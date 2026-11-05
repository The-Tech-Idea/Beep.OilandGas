using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Beep.OilandGas.ProductionOperations.Models;
public enum CompanyMaterialReservationStatus {Active,Upcoming,Expired,Released,Consumed}
public sealed record CompanyMaterialReservationQueueEntry(CompanyWorkOrderMaterialReservationFacts Reservation,CompanyWorkOrderMaterialReleaseReference? Release)
{
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public IReadOnlyList<CompanyWorkOrderMaterialUsageFacts>? Usages{get;init;}
    [JsonIgnore] public decimal? ConsumedQuantity=>Usages?.Sum(row=>row.Quantity);
    public decimal? RemainingReservedAt(DateTime utc)=>Release is {} release && release.ReleasedAtUtc<=utc || utc>=Reservation.ValidToUtc?0m:
        Usages is null?null:Reservation.Quantity-Usages.Where(row=>row.RecordedAtUtc<=utc).Sum(row=>row.Quantity);
    public CompanyMaterialReservationStatus StatusAt(DateTime utc)=>Release is {} released && released.ReleasedAtUtc<=utc?CompanyMaterialReservationStatus.Released:
        Usages is not null && Usages.Where(row=>row.RecordedAtUtc<=utc).Sum(row=>row.Quantity)==Reservation.Quantity?CompanyMaterialReservationStatus.Consumed:
        utc>=Reservation.ValidToUtc?CompanyMaterialReservationStatus.Expired:utc<Reservation.ValidFromUtc?CompanyMaterialReservationStatus.Upcoming:CompanyMaterialReservationStatus.Active;
}
/// <summary>Complete bounded authorized recorded reservations for the selected FIELD; no inferred physical availability.</summary>
public sealed record CompanyMaterialReservationQueue(FieldSelectionReview Context,IReadOnlyList<CompanyMaterialReservationQueueEntry> Rows,DateTime ComparedAtUtc,string Revision)
{
    public int Count(CompanyMaterialReservationStatus? status=null)=>status is null?Rows.Count:Enum.IsDefined(status.Value)?Rows.Count(row=>row.StatusAt(ComparedAtUtc)==status):throw new ArgumentOutOfRangeException(nameof(status));
    public static CompanyMaterialReservationQueue Create(FieldSelectionReview context,IReadOnlyList<CompanyMaterialReservationQueueEntry> rows,DateTime utc)
    {
        FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);if(string.IsNullOrWhiteSpace(context.FieldId) || rows is null || rows.Count>CompanyWorkOrderMaterialAllocationReview.MaximumAllocations || utc==default || utc.Kind!=DateTimeKind.Utc || rows.Any(row=>row?.Reservation is null) || rows.Select(row=>row.Reservation.AllocationId).Distinct(StringComparer.Ordinal).Count()!=rows.Count)throw new InvalidOperationException("Complete bounded current FIELD reservation membership and UTC comparison are required; no records are truncated.");
        var ordered=rows.OrderBy(row=>row.Reservation.AllocationId,StringComparer.Ordinal).ToArray();foreach(var row in ordered)
        {
            row.Reservation.Require(context.Company,row.Reservation.Task,row.Reservation.IntentId);if(row.Reservation.Task.FieldId!=context.FieldId)throw new InvalidOperationException("Reservation belongs to another selected FIELD.");if(row.Release is {} release && (release.AllocationId!=row.Reservation.AllocationId || !ProcessDefinitionRevision.IsValid(release.Revision) || release.Quantity!=row.Reservation.Quantity || release.ReleasedAtUtc.Kind!=DateTimeKind.Utc || release.ReleasedAtUtc==default))throw new InvalidOperationException("Original release quantity/membership is unconfirmed.");
            if(row.Usages is not {} usages)continue;
            if(usages.Count>CompanyWorkOrderMaterialAllocationReview.MaximumAllocations || usages.Any(usage=>usage is null) ||
                usages.Select(usage=>usage.UsageId).Distinct(StringComparer.Ordinal).Count()!=usages.Count || usages.Sum(usage=>usage.Quantity)>row.Reservation.Quantity)
                throw new InvalidOperationException("Complete unique bounded usage facts for each reservation are required.");
            foreach(var usage in usages)
            {
                usage.Require();var original=row.Reservation;
                if(usage.Company!=context.Company || usage.Task!=original.Task || usage.AllocationId!=original.AllocationId || usage.AllocationRevision!=original.Revision ||
                    usage.ItemId!=original.ItemId || usage.Unit!=original.Unit || usage.Warehouse!=original.Warehouse || usage.Location!=original.Location ||
                    usage.UsedAtUtc<original.ValidFromUtc || usage.UsedAtUtc>=original.ValidToUtc || usage.RecordedAtUtc>utc ||
                    row.Release is {} released && released.ReleasedAtUtc<=usage.RecordedAtUtc)
                    throw new InvalidOperationException("Original queue usage owner/task/reservation/unit/location/UTC/release proof differs.");
            }
        }
        if(ordered.Sum(row=>row.Usages?.Count??0)>CompanyWorkOrderMaterialAllocationReview.MaximumAllocations)
            throw new InvalidOperationException("Complete FIELD usage membership exceeds bounded queue capacity; no evidence is truncated.");
        var revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Context=context,Rows=ordered,ComparedAtUtc=utc}));return new(context,ordered,utc,revision);
    }
    public void Require(CompanyScope company,string actor,string field)
    {
        if(Context is null || Rows is null || Context.Company!=company || Context.ActorUserId!=actor || Context.FieldId!=field)throw new InvalidOperationException("Original company/account/FIELD queue context changed.");var expected=Create(Context,Rows,ComparedAtUtc);if(Revision!=expected.Revision)throw new InvalidOperationException("Complete original reservation queue evidence changed.");
    }
}
public interface ICompanyMaterialReservationQueueReader
{
    Task<CompanyMaterialReservationQueue> ReadMaterialReservationQueueAsync(System.Security.Claims.ClaimsPrincipal user);
}
