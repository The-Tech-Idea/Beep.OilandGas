using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.Setup
{
    /// <summary>
    /// Data class for initial user and role assignment during setup
    /// Maps to BA_EMPLOYEE and BA_AUTHORITY tables in PPDM39 schema
    /// </summary>
    public class UserRoleSetupData : ModelEntityBase
    {
        [Required]
        public string UserId { get; set; } = string.Empty; // BUSINESS_ASSOCIATE_ID

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string EmployerBaId { get; set; } = string.Empty; // Organization BUSINESS_ASSOCIATE_ID

        public List<string> Roles { get; set; } = new List<string>();

        public string? EmployeePosition { get; set; }

        public string? Status { get; set; }

        public DateTime? EffectiveDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public bool Active { get; set; } = true;
    }

    /// <summary>
    /// Role assignment data for a user
    /// Maps to BA_AUTHORITY table
    /// </summary>
    public class RoleAssignmentData : ModelEntityBase
    {
        [Required]
        public string BusinessAssociateId { get; set; } = string.Empty;

        [Required]
        public string AuthorityId { get; set; } = string.Empty; // Role ID

        public string? AuthorityType { get; set; }

        public DateTime? EffectiveDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public bool Active { get; set; } = true;
    }
}




