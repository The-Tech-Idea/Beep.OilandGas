namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional
{
    /// <summary>Payload for service-backed tank inventory updates.</summary>
    public class UpdateTankInventoryRequest
    {
        public decimal VolumeDelta { get; set; }
    }
}
