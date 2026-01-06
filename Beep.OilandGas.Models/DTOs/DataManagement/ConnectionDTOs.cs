using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.DTOs.DataManagement
{
    /// <summary>
    /// Connection information model
    /// </summary>
    public class ConnectionInfo
    {
        public string ConnectionName { get; set; } = string.Empty;
        public string DatabaseType { get; set; } = string.Empty;
        public string Server { get; set; } = string.Empty;
        public string? Database { get; set; }
        public int? Port { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// Connection test request
    /// </summary>
    public class TestConnectionRequest
    {
        [Required(ErrorMessage = "ConnectionName is required")]
        public string ConnectionName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Connection test result
    /// </summary>
    public class ConnectionTestResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorDetails { get; set; }
    }

    /// <summary>
    /// Set current connection request
    /// </summary>
    public class SetCurrentConnectionRequest
    {
        [Required(ErrorMessage = "ConnectionName is required")]
        public string ConnectionName { get; set; } = string.Empty;
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Set current connection result
    /// </summary>
    public class SetCurrentConnectionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorDetails { get; set; }
    }

    /// <summary>
    /// Current connection response
    /// </summary>
    public class CurrentConnectionResponse
    {
        public string? ConnectionName { get; set; }
    }

    /// <summary>
    /// Create connection request
    /// </summary>
    public class CreateConnectionRequest
    {
        [Required(ErrorMessage = "ConnectionName is required")]
        public string ConnectionName { get; set; } = string.Empty;

        [Required(ErrorMessage = "DatabaseType is required")]
        public string DatabaseType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Server is required")]
        public string Server { get; set; } = string.Empty;

        public string? Database { get; set; }
        public int? Port { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool CreateDatabase { get; set; }
        public string? SchemaName { get; set; }
    }

    /// <summary>
    /// Create connection result
    /// </summary>
    public class CreateConnectionResult
    {
        public bool Success { get; set; }
        public string ConnectionName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ErrorDetails { get; set; }
    }

    /// <summary>
    /// Seed data request
    /// </summary>
    public class SeedDataRequest
    {
        [Required(ErrorMessage = "ConnectionName is required")]
        public string ConnectionName { get; set; } = string.Empty;

        public List<string>? TableNames { get; set; }
        public bool SkipExisting { get; set; } = true;
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Seed data response
    /// </summary>
    public class SeedDataResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorDetails { get; set; }
        public int TablesSeeded { get; set; }
        public int RecordsInserted { get; set; }
    }

    /// <summary>
    /// Connection configuration for API requests
    /// </summary>
    public class ConnectionConfig
    {
        public string DatabaseType { get; set; } = string.Empty;
        public string ConnectionName { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Database { get; set; } = string.Empty;
        public string? Schema { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Converts to ConnectionProperties for internal use
        /// </summary>
        public TheTechIdea.Beep.ConfigUtil.ConnectionProperties ToConnectionProperties()
        {
            var cp = new TheTechIdea.Beep.ConfigUtil.ConnectionProperties
            {
                ConnectionName = this.ConnectionName,
                Host = this.Host,
                Port = this.Port,
                Database = this.Database,
                UserID = this.Username ?? string.Empty,
                Password = this.Password ?? string.Empty,
                ConnectionString = this.ConnectionString ?? string.Empty
            };

            // Note: DatabaseType enum parsing commented out due to external dependency issues
            // Set database type based on string value if available through other means
            
            return cp;
        }
    }
}



