using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Metadata;

namespace Beep.OilandGas.PPDM39.DataManagement.Core.Metadata
{
    /// <summary>
    /// Helper class to load PPDM metadata from SQL scripts
    /// </summary>
    /// <remarks>
    /// [Obsolete] This static helper class is deprecated. Use PPDMMetadataService instead which provides better DI support and caching.
    /// </remarks>
    [Obsolete("Use PPDMMetadataService instead. This class will be removed in a future version.")]
    public static class PPDMMetadataLoader
    {
        /// <summary>
        /// Loads metadata from SQL script file
        /// </summary>
        public static async Task<IPPDMMetadataRepository> LoadFromFileAsync(string sqlScriptPath)
        {
            if (!File.Exists(sqlScriptPath))
                throw new FileNotFoundException($"SQL script file not found: {sqlScriptPath}");

            var loader = new PPDMSqlMetadataLoader();
            var metadata = loader.LoadFromSqlFile(sqlScriptPath);
            
            return new PPDMMetadataRepository(metadata);
        }

        /// <summary>
        /// Loads metadata from SQL script content (string)
        /// </summary>
        public static IPPDMMetadataRepository LoadFromContent(string sqlScriptContent)
        {
            if (string.IsNullOrWhiteSpace(sqlScriptContent))
                throw new ArgumentException("SQL script content cannot be null or empty", nameof(sqlScriptContent));

            var loader = new PPDMSqlMetadataLoader();
            var metadata = loader.LoadFromSqlScript(sqlScriptContent);
            
            return new PPDMMetadataRepository(metadata);
        }

        /// <summary>
        /// Loads metadata from SQL script file (synchronous)
        /// </summary>
        public static IPPDMMetadataRepository LoadFromFile(string sqlScriptPath)
        {
            if (!File.Exists(sqlScriptPath))
                throw new FileNotFoundException($"SQL script file not found: {sqlScriptPath}");

            var loader = new PPDMSqlMetadataLoader();
            var metadata = loader.LoadFromSqlFile(sqlScriptPath);
            
            return new PPDMMetadataRepository(metadata);
        }
    }
}

