namespace Beep.OilandGas.Models.Data.Accounting.Storage
{
    /// <summary>
    /// Request DTO for creating a storage facility
    /// </summary>
    public class CreateStorageFacilityRequest : ModelEntityBase
    {
        public string? FacilityId { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        public string? Location { get; set; }
    }
}




