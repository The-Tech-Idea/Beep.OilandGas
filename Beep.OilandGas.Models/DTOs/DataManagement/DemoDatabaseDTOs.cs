using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.DTOs.DataManagement
{
    /// <summary>
    /// Create demo database request
    /// </summary>
    public class CreateDemoDatabaseRequest
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; } = string.Empty;

        public string SeedDataOption { get; set; } = "reference-sample"; // none, reference-only, reference-sample, full-demo
        public string? ConnectionName { get; set; } // Optional, will be auto-generated if not provided
    }

    /// <summary>
    /// Create demo database response
    /// </summary>
    public class CreateDemoDatabaseResponse
    {
        public bool Success { get; set; }
        public string ConnectionName { get; set; } = string.Empty;
        public string DatabasePath { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ErrorDetails { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
    }

    /// <summary>
    /// Demo database metadata
    /// </summary>
    public class DemoDatabaseMetadata
    {
        public string ConnectionName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string DatabasePath { get; set; } = string.Empty;
        public string SeedDataOption { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsExpired => DateTime.UtcNow > ExpiryDate;
    }

    /// <summary>
    /// List demo databases response
    /// </summary>
    public class ListDemoDatabasesResponse
    {
        public List<DemoDatabaseMetadata> Databases { get; set; } = new();
        public int TotalCount { get; set; }
        public int ExpiredCount { get; set; }
    }

    /// <summary>
    /// Delete demo database response
    /// </summary>
    public class DeleteDemoDatabaseResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorDetails { get; set; }
    }

    /// <summary>
    /// Cleanup demo databases response
    /// </summary>
    public class CleanupDemoDatabasesResponse
    {
        public bool Success { get; set; }
        public int DeletedCount { get; set; }
        public List<string> DeletedDatabases { get; set; } = new();
        public string Message { get; set; } = string.Empty;
        public string? ErrorDetails { get; set; }
    }
}



