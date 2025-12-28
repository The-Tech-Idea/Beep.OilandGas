using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;

namespace Beep.OilandGas.PPDM39.DataManagement.Tools
{
    /// <summary>
    /// Helper class to run script generation
    /// </summary>
    public static class RunScriptGenerator
    {
        /// <summary>
        /// Generates all scripts for all entities
        /// </summary>
        public static async Task<int> GenerateAllScriptsAsync(string outputPath = null)
        {
            try
            {
                // Determine output path
                var scriptsPath = outputPath ?? Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "..", "..", "..", "..", "Beep.OilandGas.Models", "Scripts");

                scriptsPath = Path.GetFullPath(scriptsPath);

                Console.WriteLine("SQL Script Generator");
                Console.WriteLine("===================");
                Console.WriteLine($"Output path: {scriptsPath}");
                Console.WriteLine();

                // Get all entity types
                var entityAssembly = typeof(AFE).Assembly;
                var entityTypes = entityAssembly.GetTypes()
                    .Where(t =>
                        t.Namespace == "Beep.OilandGas.Models.Data" &&
                        t.IsClass &&
                        !t.IsAbstract &&
                        t.BaseType != null &&
                        t.BaseType.Name == "Entity" &&
                        t.Name != "ppdm39SqlServer") // Exclude the base class
                    .OrderBy(t => t.Name)
                    .ToList();

                Console.WriteLine($"Found {entityTypes.Count} entity types");
                Console.WriteLine();

                // Database types to generate
                var databaseTypes = new List<DatabaseTypeMapper.DatabaseType>
                {
                    DatabaseTypeMapper.DatabaseType.SqlServer,
                    DatabaseTypeMapper.DatabaseType.Oracle,
                    DatabaseTypeMapper.DatabaseType.PostgreSQL,
                    DatabaseTypeMapper.DatabaseType.MySQL,
                    DatabaseTypeMapper.DatabaseType.MariaDB,
                    DatabaseTypeMapper.DatabaseType.SQLite
                };

                // Create output directories
                foreach (var dbType in databaseTypes)
                {
                    var dbTypeName = GetDatabaseTypeFolderName(dbType);
                    var dbPath = Path.Combine(scriptsPath, dbTypeName);
                    Directory.CreateDirectory(dbPath);
                }

                // Generate scripts
                int totalGenerated = 0;
                int totalSkipped = 0;
                int totalErrors = 0;

                IPPDMMetadataRepository? metadata = null;
                try
                {
                    metadata = PPDMMetadataRepository.FromGeneratedClass();
                }
                catch
                {
                    // Metadata not available, continue without it
                    Console.WriteLine("Warning: Metadata not available, generating scripts without metadata support");
                }

                foreach (var entityType in entityTypes)
                {
                    var tableName = entityType.Name;
                    Console.WriteLine($"Processing: {tableName}");

                    foreach (var dbType in databaseTypes)
                    {
                        try
                        {
                            var generator = new PPDMScriptGenerator(dbType, metadata);
                            var dbTypeName = GetDatabaseTypeFolderName(dbType);
                            var dbPath = Path.Combine(scriptsPath, dbTypeName);

                            // Generate TAB script
                            var tabScript = await generator.GenerateTableScriptAsync(entityType);
                            if (!string.IsNullOrWhiteSpace(tabScript))
                            {
                                var tabPath = Path.Combine(dbPath, $"{tableName}_TAB.sql");
                                await File.WriteAllTextAsync(tabPath, tabScript);
                                totalGenerated++;
                            }

                            // Generate PK script
                            var pkScript = await generator.GeneratePrimaryKeyScriptAsync(entityType);
                            if (!string.IsNullOrWhiteSpace(pkScript))
                            {
                                var pkPath = Path.Combine(dbPath, $"{tableName}_PK.sql");
                                await File.WriteAllTextAsync(pkPath, pkScript);
                                totalGenerated++;
                            }

                            // Generate FK script
                            var fkScript = await generator.GenerateForeignKeyScriptsAsync(entityType);
                            if (!string.IsNullOrWhiteSpace(fkScript))
                            {
                                var fkPath = Path.Combine(dbPath, $"{tableName}_FK.sql");
                                await File.WriteAllTextAsync(fkPath, fkScript);
                                totalGenerated++;
                            }

                            // Generate IX script (indexes)
                            var ixScript = await generator.GenerateIndexScriptsAsync(entityType);
                            if (!string.IsNullOrWhiteSpace(ixScript))
                            {
                                var ixPath = Path.Combine(dbPath, $"{tableName}_IX.sql");
                                await File.WriteAllTextAsync(ixPath, ixScript);
                                totalGenerated++;
                            }
                        }
                        catch (Exception ex)
                        {
                            totalErrors++;
                            Console.WriteLine($"  ✗ Error generating {tableName} for {dbType}: {ex.Message}");
                        }
                    }

                    Console.WriteLine($"  ✓ Completed {tableName}");
                }

                Console.WriteLine();
                Console.WriteLine("Script generation complete!");
                Console.WriteLine($"Total scripts generated: {totalGenerated}");
                Console.WriteLine($"Skipped: {totalSkipped}");
                Console.WriteLine($"Errors: {totalErrors}");

                return totalErrors > 0 ? 1 : 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return 1;
            }
        }

        private static string GetDatabaseTypeFolderName(DatabaseTypeMapper.DatabaseType dbType)
        {
            return dbType switch
            {
                DatabaseTypeMapper.DatabaseType.SqlServer => "Sqlserver",
                DatabaseTypeMapper.DatabaseType.Oracle => "Oracle",
                DatabaseTypeMapper.DatabaseType.PostgreSQL => "PostgreSQL",
                DatabaseTypeMapper.DatabaseType.MySQL => "MySQL",
                DatabaseTypeMapper.DatabaseType.MariaDB => "MariaDB",
                DatabaseTypeMapper.DatabaseType.SQLite => "SQLite",
                _ => dbType.ToString()
            };
        }
    }
}

