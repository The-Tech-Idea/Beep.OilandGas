using System;
using System.Collections.Generic;

namespace Beep.OilandGas.ApiService.Models
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
        public string ConnectionName { get; set; } = string.Empty;
        public string DatabaseType { get; set; } = string.Empty;
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
}

