using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.DTOs.Setup
{
    /// <summary>
    /// Data class for BA_ORGANIZATION setup during initial organization/company setup
    /// Maps to BA_ORGANIZATION table in PPDM39 schema
    /// </summary>
    public class OrganizationSetupData
    {
        [Required]
        public string BusinessAssociateId { get; set; } = string.Empty;

        [Required]
        public string OrganizationId { get; set; } = string.Empty;

        [Required]
        public int OrganizationSeqNo { get; set; } = 1;

        [Required]
        public string OrganizationName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? OrganizationType { get; set; }

        public string? AreaId { get; set; }

        public string? AreaType { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? EffectiveDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public string? Remark { get; set; }

        public string? Source { get; set; }

        public string ActiveInd { get; set; } = "Y";

        public string? PPDMGuid { get; set; }
    }
}
