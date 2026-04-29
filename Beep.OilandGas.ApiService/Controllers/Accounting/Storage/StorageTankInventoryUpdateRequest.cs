namespace Beep.OilandGas.ApiService.Controllers.Accounting.Storage
{
    /// <summary>Payload for service-backed storage tank inventory updates.</summary>
    public class StorageTankInventoryUpdateRequest
    {
        public decimal VolumeDelta { get; set; }
    }
}
