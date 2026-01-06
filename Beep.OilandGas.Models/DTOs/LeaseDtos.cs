using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs
{
    /// <summary>
    /// DTO for lease information.
    /// </summary>
    public class LeaseDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public string LeaseNumber { get; set; } = string.Empty;
        public string? LessorName { get; set; }
        public string? LessorId { get; set; }
        public string? LesseeName { get; set; }
        public string? LesseeId { get; set; }
        public DateTime? LeaseDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public decimal? LeaseArea { get; set; }
        public string? AreaUnit { get; set; }
        public string? Location { get; set; }
        public string? LegalDescription { get; set; }
        public decimal? RoyaltyRate { get; set; }
        public decimal? BonusPayment { get; set; }
        public decimal? AnnualRental { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }
        public List<LandRightDto> LandRights { get; set; } = new();
        public List<MineralRightDto> MineralRights { get; set; } = new();
    }

    /// <summary>
    /// DTO for land rights.
    /// </summary>
    public class LandRightDto
    {
        public string LandRightId { get; set; } = string.Empty;
        public string LeaseId { get; set; } = string.Empty;
        public string RightType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for mineral rights.
    /// </summary>
    public class MineralRightDto
    {
        public string MineralRightId { get; set; } = string.Empty;
        public string LeaseId { get; set; } = string.Empty;
        public string OwnerId { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public decimal? WorkingInterest { get; set; }
        public decimal? NetRevenueInterest { get; set; }
        public decimal? RoyaltyInterest { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string? Status { get; set; }
    }

    /// <summary>
    /// DTO for surface agreements.
    /// </summary>
    public class SurfaceAgreementDto
    {
        public string AgreementId { get; set; } = string.Empty;
        public string LeaseId { get; set; } = string.Empty;
        public string AgreementType { get; set; } = string.Empty;
        public string? SurfaceOwnerId { get; set; }
        public string? SurfaceOwnerName { get; set; }
        public DateTime? AgreementDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public decimal? CompensationAmount { get; set; }
        public string? CompensationType { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for royalty information.
    /// </summary>
    public class RoyaltyDto
    {
        public string RoyaltyId { get; set; } = string.Empty;
        public string LeaseId { get; set; } = string.Empty;
        public string OwnerId { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public decimal RoyaltyRate { get; set; }
        public string? RoyaltyType { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string? Status { get; set; }
    }

    /// <summary>
    /// DTO for creating a new lease.
    /// </summary>
    public class CreateLeaseDto
    {
        public string LeaseNumber { get; set; } = string.Empty;
        public string? LessorId { get; set; }
        public string? LesseeId { get; set; }
        public DateTime? LeaseDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public decimal? LeaseArea { get; set; }
        public string? AreaUnit { get; set; }
        public string? Location { get; set; }
        public string? LegalDescription { get; set; }
        public decimal? RoyaltyRate { get; set; }
        public decimal? BonusPayment { get; set; }
        public decimal? AnnualRental { get; set; }
    }

    /// <summary>
    /// DTO for updating a lease.
    /// </summary>
    public class UpdateLeaseDto
    {
        public string? Status { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public decimal? AnnualRental { get; set; }
        public string? Remarks { get; set; }
    }
}




