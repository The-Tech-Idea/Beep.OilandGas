namespace Beep.OilandGas.Models.Data
{
    /// <summary>
    /// Describes what database operations a provider supports.
    /// Used by the setup wizard to gate actions and surface accurate choices
    /// rather than relying on hardcoded provider-specific conditions.
    /// </summary>
    public class ProviderCapabilityInfo : ModelEntityBase
    {
        private string providerTypeValue = string.Empty;
        /// <summary>Provider type string (e.g. "SqlLite", "SqlServer", "Postgre").</summary>
        public string ProviderType { get => providerTypeValue; set => SetProperty(ref providerTypeValue, value); }

        private bool supportsLocalFileCreateValue;
        /// <summary>Provider creates a local file (e.g. SQLite). Enables file-path wizard step.</summary>
        public bool SupportsLocalFileCreate { get => supportsLocalFileCreateValue; set => SetProperty(ref supportsLocalFileCreateValue, value); }

        private bool supportsSafeAdditiveMigrationValue;
        /// <summary>Provider supports additive schema changes (CREATE TABLE, ADD COLUMN) without data loss.</summary>
        public bool SupportsSafeAdditiveMigration { get => supportsSafeAdditiveMigrationValue; set => SetProperty(ref supportsSafeAdditiveMigrationValue, value); }

        private bool supportsDestructiveOperationsValue;
        /// <summary>Provider supports DROP TABLE, TRUNCATE, and other destructive DDL in protected mode.</summary>
        public bool SupportsDestructiveOperations { get => supportsDestructiveOperationsValue; set => SetProperty(ref supportsDestructiveOperationsValue, value); }

        private bool supportsCheckpointResumeValue;
        /// <summary>Provider supports checkpoint/resume semantics in the migration pipeline.</summary>
        public bool SupportsCheckpointResume { get => supportsCheckpointResumeValue; set => SetProperty(ref supportsCheckpointResumeValue, value); }

        private bool supportsDryRunArtifactsValue;
        /// <summary>Provider supports generating dry-run SQL artifacts before execution.</summary>
        public bool SupportsDryRunArtifacts { get => supportsDryRunArtifactsValue; set => SetProperty(ref supportsDryRunArtifactsValue, value); }

        private bool supportsSchemaNamespaceValue;
        /// <summary>Provider supports named schemas (SQL Server: dbo, Oracle: user/schema).</summary>
        public bool SupportsSchemaNamespace { get => supportsSchemaNamespaceValue; set => SetProperty(ref supportsSchemaNamespaceValue, value); }

        private bool requiresNetworkHostValue;
        /// <summary>Provider requires a network host/port (not a local file).</summary>
        public bool RequiresNetworkHost { get => requiresNetworkHostValue; set => SetProperty(ref requiresNetworkHostValue, value); }

        private bool isDriverInstalledValue;
        /// <summary>Whether the driver assembly is loaded in the current BeepDM runtime.</summary>
        public bool IsDriverInstalled { get => isDriverInstalledValue; set => SetProperty(ref isDriverInstalledValue, value); }

        private string? nuGetPackageValue;
        /// <summary>NuGet package required to enable this provider.</summary>
        public string? NuGetPackage { get => nuGetPackageValue; set => SetProperty(ref nuGetPackageValue, value); }
    }
}
