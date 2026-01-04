namespace Beep.OilandGas.Models.DTOs.Accounting.Storage
{
    /// <summary>
    /// Request DTO for creating a storage facility
    /// </summary>
    public class CreateStorageFacilityRequest
    {
        public string? FacilityId { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        public string? Location { get; set; }
    }
}
