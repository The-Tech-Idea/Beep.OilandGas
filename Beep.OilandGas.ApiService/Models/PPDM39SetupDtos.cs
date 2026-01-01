using System;
using System.Collections.Generic;
using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation;

namespace Beep.OilandGas.ApiService.Models
{
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
        // Added ConnectionString to support direct mapping from ConnectionProperties
        public string? ConnectionString { get; set; }
        public ConnectionProperties ToConnectionProperties()
        {
            // Map API DTO to internal ConnectionProperties used by editor/data layer
            var cp = new ConnectionProperties
            {
                ConnectionName = this.ConnectionName,
                Host = this.Host,
                Port = this.Port,
                Database = this.Database,
                UserID = this.Username ?? string.Empty,
                Password = this.Password ?? string.Empty,
                ConnectionString = this.ConnectionString ?? string.Empty
            };

            // Try to set DatabaseType enum if possible
            if (!string.IsNullOrWhiteSpace(this.DatabaseType))
            {
                // Try parse common names
                if (Enum.TryParse(typeof(TheTechIdea.Beep.ConfigUtil.DataSourceType), this.DatabaseType, true, out var ds))
                {
                    cp.DatabaseType = (TheTechIdea.Beep.ConfigUtil.DataSourceType)ds!;
                }
                else
                {
                    // fallback: map known strings
                    cp.DatabaseType = this.DatabaseType.ToLowerInvariant() switch
                    {
                        "sqlserver" => TheTechIdea.Beep.ConfigUtil.DataSourceType.SqlServer,
                        "postgresql" or "postgre" => TheTechIdea.Beep.ConfigUtil.DataSourceType.Postgre,
                        "mysql" or "mariadb" => TheTechIdea.Beep.ConfigUtil.DataSourceType.Mysql,
                        "oracle" => TheTechIdea.Beep.ConfigUtil.DataSourceType.Oracle,
                        "sqlite" => TheTechIdea.Beep.ConfigUtil.DataSourceType.Sqlite,
                        _ => TheTechIdea.Beep.ConfigUtil.DataSourceType.SqlServer
                    };
                }
            }

            return cp;
        }
    }

    // NOTE: ConnectionTestResult is defined in ConnectionModels.cs to avoid duplicate type declarations.
    // The class was intentionally removed from this file to prevent ambiguity during build.
}
