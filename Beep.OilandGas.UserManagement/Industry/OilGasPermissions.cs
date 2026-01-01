namespace Beep.OilandGas.UserManagement.Industry
{
    /// <summary>
    /// Standard Oil & Gas industry permission codes
    /// Uses hierarchical permission structure: Module:Resource:Action
    /// </summary>
    public static class OilGasPermissions
    {
        #region Module Permissions

        /// <summary>
        /// Production Accounting module permissions
        /// </summary>
        public static class ProductionAccounting
        {
            public const string View = "Module:ProductionAccounting:View";
            public const string Read = "Module:ProductionAccounting:Read";
            public const string Write = "Module:ProductionAccounting:Write";
            public const string Delete = "Module:ProductionAccounting:Delete";
            public const string Approve = "Module:ProductionAccounting:Approve";
            public const string Export = "Module:ProductionAccounting:Export";
        }

        /// <summary>
        /// Nodal Analysis module permissions
        /// </summary>
        public static class NodalAnalysis
        {
            public const string View = "Module:NodalAnalysis:View";
            public const string Read = "Module:NodalAnalysis:Read";
            public const string Execute = "Module:NodalAnalysis:Execute";
            public const string Export = "Module:NodalAnalysis:Export";
        }

        /// <summary>
        /// Reservoir Analysis module permissions
        /// </summary>
        public static class ReservoirAnalysis
        {
            public const string View = "Module:ReservoirAnalysis:View";
            public const string Read = "Module:ReservoirAnalysis:Read";
            public const string Write = "Module:ReservoirAnalysis:Write";
            public const string Execute = "Module:ReservoirAnalysis:Execute";
        }

        /// <summary>
        /// Well Test Analysis module permissions
        /// </summary>
        public static class WellTestAnalysis
        {
            public const string View = "Module:WellTestAnalysis:View";
            public const string Read = "Module:WellTestAnalysis:Read";
            public const string Write = "Module:WellTestAnalysis:Write";
            public const string Execute = "Module:WellTestAnalysis:Execute";
        }

        /// <summary>
        /// Gas Lift module permissions
        /// </summary>
        public static class GasLift
        {
            public const string View = "Module:GasLift:View";
            public const string Read = "Module:GasLift:Read";
            public const string Write = "Module:GasLift:Write";
            public const string Execute = "Module:GasLift:Execute";
        }

        /// <summary>
        /// Sucker Rod Pumping module permissions
        /// </summary>
        public static class SuckerRodPumping
        {
            public const string View = "Module:SuckerRodPumping:View";
            public const string Read = "Module:SuckerRodPumping:Read";
            public const string Write = "Module:SuckerRodPumping:Write";
            public const string Execute = "Module:SuckerRodPumping:Execute";
        }

        /// <summary>
        /// Production Forecasting module permissions
        /// </summary>
        public static class ProductionForecasting
        {
            public const string View = "Module:ProductionForecasting:View";
            public const string Read = "Module:ProductionForecasting:Read";
            public const string Write = "Module:ProductionForecasting:Write";
            public const string Execute = "Module:ProductionForecasting:Execute";
        }

        /// <summary>
        /// Decline Curve Analysis (DCA) module permissions
        /// </summary>
        public static class DCA
        {
            public const string View = "Module:DCA:View";
            public const string Read = "Module:DCA:Read";
            public const string Write = "Module:DCA:Write";
            public const string Execute = "Module:DCA:Execute";
        }

        /// <summary>
        /// Permits and Applications module permissions
        /// </summary>
        public static class PermitsAndApplications
        {
            public const string View = "Module:PermitsAndApplications:View";
            public const string Read = "Module:PermitsAndApplications:Read";
            public const string Write = "Module:PermitsAndApplications:Write";
            public const string Approve = "Module:PermitsAndApplications:Approve";
        }

        /// <summary>
        /// Lease Acquisition module permissions
        /// </summary>
        public static class LeaseAcquisition
        {
            public const string View = "Module:LeaseAcquisition:View";
            public const string Read = "Module:LeaseAcquisition:Read";
            public const string Write = "Module:LeaseAcquisition:Write";
        }

        /// <summary>
        /// Economic Analysis module permissions
        /// </summary>
        public static class EconomicAnalysis
        {
            public const string View = "Module:EconomicAnalysis:View";
            public const string Read = "Module:EconomicAnalysis:Read";
            public const string Write = "Module:EconomicAnalysis:Write";
            public const string Execute = "Module:EconomicAnalysis:Execute";
        }

        /// <summary>
        /// Data Management module permissions
        /// </summary>
        public static class DataManagement
        {
            public const string View = "Module:DataManagement:View";
            public const string Read = "Module:DataManagement:Read";
            public const string Write = "Module:DataManagement:Write";
            public const string Import = "Module:DataManagement:Import";
            public const string Export = "Module:DataManagement:Export";
        }

        /// <summary>
        /// Drawing module permissions
        /// </summary>
        public static class Drawing
        {
            public const string View = "Module:Drawing:View";
            public const string Read = "Module:Drawing:Read";
            public const string Write = "Module:Drawing:Write";
            public const string Delete = "Module:Drawing:Delete";
        }

        /// <summary>
        /// Heat Map module permissions
        /// </summary>
        public static class HeatMap
        {
            public const string View = "Module:HeatMap:View";
            public const string Read = "Module:HeatMap:Read";
            public const string Write = "Module:HeatMap:Write";
        }

        #endregion

        #region Data Source Permissions

        /// <summary>
        /// Creates a data source access permission
        /// </summary>
        public static string DataSourceAccess(string dataSourceName) => $"DataSource:{dataSourceName}:Access";

        /// <summary>
        /// Creates a data source read permission
        /// </summary>
        public static string DataSourceRead(string dataSourceName) => $"DataSource:{dataSourceName}:Read";

        /// <summary>
        /// Creates a data source write permission
        /// </summary>
        public static string DataSourceWrite(string dataSourceName) => $"DataSource:{dataSourceName}:Write";

        /// <summary>
        /// Creates a database access permission
        /// </summary>
        public static string DatabaseAccess(string databaseName) => $"Database:{databaseName}:Access";

        #endregion

        #region Asset Permissions

        /// <summary>
        /// Creates an asset access permission
        /// </summary>
        public static string AssetAccess(string assetType, string assetId, string action) => 
            $"Asset:{assetType}:{assetId}:{action}";

        /// <summary>
        /// Creates an asset type access permission
        /// </summary>
        public static string AssetTypeAccess(string assetType, string action) => 
            $"Asset:{assetType}:*:{action}";

        #endregion

        #region Row-Level Security Permissions

        /// <summary>
        /// Enable creator-based RLS (users see only their rows)
        /// </summary>
        public const string RLSCreatorEnabled = "RLS:Creator:Enabled";

        /// <summary>
        /// Disable creator-based RLS (users see all rows)
        /// </summary>
        public const string RLSCreatorDisabled = "RLS:Creator:Disabled";

        /// <summary>
        /// Creates a source-based RLS permission
        /// </summary>
        public static string RLSSourceAccess(string sourceName) => $"RLS:Source:{sourceName}:Access";

        /// <summary>
        /// Creates a tenant-based RLS permission
        /// </summary>
        public static string RLSTenantAccess(string tenantId) => $"RLS:Tenant:{tenantId}:Access";

        /// <summary>
        /// Creates a table RLS bypass permission (admin only)
        /// </summary>
        public static string RLSTableBypass(string tableName) => $"RLS:Table:{tableName}:Bypass";

        #endregion

        #region Function Permissions

        /// <summary>
        /// Creates a function execution permission
        /// </summary>
        public static string FunctionExecute(string functionName) => $"Function:{functionName}:Execute";

        #endregion

        /// <summary>
        /// Gets all standard module permissions
        /// </summary>
        public static IEnumerable<string> GetAllModulePermissions()
        {
            return new[]
            {
                // Production Accounting
                ProductionAccounting.View, ProductionAccounting.Read, ProductionAccounting.Write,
                ProductionAccounting.Delete, ProductionAccounting.Approve, ProductionAccounting.Export,
                
                // Nodal Analysis
                NodalAnalysis.View, NodalAnalysis.Read, NodalAnalysis.Execute, NodalAnalysis.Export,
                
                // Reservoir Analysis
                ReservoirAnalysis.View, ReservoirAnalysis.Read, ReservoirAnalysis.Write, ReservoirAnalysis.Execute,
                
                // Well Test Analysis
                WellTestAnalysis.View, WellTestAnalysis.Read, WellTestAnalysis.Write, WellTestAnalysis.Execute,
                
                // Gas Lift
                GasLift.View, GasLift.Read, GasLift.Write, GasLift.Execute,
                
                // Sucker Rod Pumping
                SuckerRodPumping.View, SuckerRodPumping.Read, SuckerRodPumping.Write, SuckerRodPumping.Execute,
                
                // Production Forecasting
                ProductionForecasting.View, ProductionForecasting.Read, ProductionForecasting.Write, ProductionForecasting.Execute,
                
                // DCA
                DCA.View, DCA.Read, DCA.Write, DCA.Execute,
                
                // Permits and Applications
                PermitsAndApplications.View, PermitsAndApplications.Read, PermitsAndApplications.Write, PermitsAndApplications.Approve,
                
                // Lease Acquisition
                LeaseAcquisition.View, LeaseAcquisition.Read, LeaseAcquisition.Write,
                
                // Economic Analysis
                EconomicAnalysis.View, EconomicAnalysis.Read, EconomicAnalysis.Write, EconomicAnalysis.Execute,
                
                // Data Management
                DataManagement.View, DataManagement.Read, DataManagement.Write, DataManagement.Import, DataManagement.Export,
                
                // Drawing
                Drawing.View, Drawing.Read, Drawing.Write, Drawing.Delete,
                
                // Heat Map
                HeatMap.View, HeatMap.Read, HeatMap.Write
            };
        }
    }
}
