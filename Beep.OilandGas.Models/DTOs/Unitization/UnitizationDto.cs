using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.Unitization
{
    public class CreateUnitAgreementRequest
    {
        public string UnitName { get; set; }
        public string UnitNumber { get; set; }
        public string UnitOperatorBaId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string TermsAndConditions { get; set; }
    }

    public class CreateParticipatingAreaRequest
    {
        public string UnitAgreementId { get; set; }
        public string AreaName { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    public class CreateTractParticipationRequest
    {
        public string ParticipatingAreaId { get; set; }
        public string UnitAgreementId { get; set; }
        public string TractId { get; set; }
        public decimal ParticipationPercentage { get; set; }
        public decimal WorkingInterest { get; set; }
        public decimal NetRevenueInterest { get; set; }
        public decimal? TractAcreage { get; set; }
    }

    public class UnitApprovalResult
    {
        public string AgreementId { get; set; }
        public bool IsApproved { get; set; }
        public string ApproverId { get; set; }
        public DateTime ApprovalDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; }
        public string Comments { get; set; }
    }

    public class UnitOperationsSummary
    {
        public string UnitAgreementId { get; set; }
        public string UnitName { get; set; }
        public int ParticipatingAreaCount { get; set; }
        public int TractCount { get; set; }
        public decimal TotalParticipationPercentage { get; set; }
        public decimal TotalWorkingInterest { get; set; }
        public decimal TotalNetRevenueInterest { get; set; }
        public DateTime? LastProductionDate { get; set; }
        public decimal? TotalProductionVolume { get; set; }
    }
}

