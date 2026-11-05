using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ProductionOperations.Models;
namespace Beep.OilandGas.Web.Services;
public sealed partial class InventoryWorkspaceState
{
    private int reservationVersion;
    public CompanyMaterialReservationQueue? MaterialReservations {get;private set;}
    public CompanyMaterialReservationStatus? ReservationStatus {get;private set;}
    public int? ReservationCount(CompanyMaterialReservationStatus? status=null)=>MaterialReservations?.Count(status);
    public IReadOnlyList<CompanyMaterialReservationQueueEntry>? VisibleReservations=>MaterialReservations is {} queue?Array.AsReadOnly(queue.Rows.Where(row=>ReservationStatus is null || row.StatusAt(queue.ComparedAtUtc)==ReservationStatus).ToArray()):null;
    public void FilterReservations(CompanyMaterialReservationStatus? status){if(status is not null && !Enum.IsDefined(status.Value))throw new ArgumentOutOfRangeException(nameof(status));ReservationStatus=status;}
    public void ClearReservations(){reservationVersion++;MaterialReservations=null;ReservationStatus=null;}
    public async Task LoadMaterialReservationsAsync(string field,Func<string,Task<CompanyMaterialReservationQueue>> read)
    {
        ClearReservations();var version=reservationVersion;var snapshot=company.Snapshot;if(disposed || snapshot.Selection is not {} selected)return;
        var result=await read(field);if(disposed || version!=reservationVersion || snapshot!=company.Snapshot)return;
        result.Require(selected.Scope,selected.UserId,field);MaterialReservations=CompanyInventorySourceReview.Copy(result);
    }
}
