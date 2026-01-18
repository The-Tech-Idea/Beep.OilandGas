#nullable enable

namespace Beep.OilandGas.Models.Models.LeaseManagement
{
    /// <summary>
    /// Represents a lease agreement in the oil and gas domain.
    /// </summary>
    public class LeaseAgreement
    {
        public string? LeaseAgreementId { get; set; }
        public string? LeaseId { get; set; }
        public string? AgreementNumber { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public decimal? RoyaltyRate { get; set; }
        public string? OperatorName { get; set; }
        public string? AgreementType { get; set; }
        public string? Status { get; set; }
    }
}
