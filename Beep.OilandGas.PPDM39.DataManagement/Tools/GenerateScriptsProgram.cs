using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation;
using Beep.OilandGas.PPDM39.DataManagement.Tools;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.PPDM39.DataManagement.Tools
{
    /// <summary>
    /// Command-line tool for generating SQL scripts from entity classes
    /// </summary>
    public class GenerateScriptsProgram
    {
        private readonly string _outputPath;
        private readonly List<DatabaseTypeMapper.DatabaseType> _databaseTypes;
        private readonly List<string>? _specificEntities;
        private readonly bool _generateAll;
        private readonly IPPDMMetadataRepository? _metadata;

        public GenerateScriptsProgram(
            string outputPath,
            List<DatabaseTypeMapper.DatabaseType>? databaseTypes = null,
            List<string>? specificEntities = null,
            bool generateAll = true,
            IPPDMMetadataRepository? metadata = null)
        {
            _outputPath = outputPath ?? throw new ArgumentNullException(nameof(outputPath));
            _databaseTypes = databaseTypes ?? new List<DatabaseTypeMapper.DatabaseType>
            {
                DatabaseTypeMapper.DatabaseType.SqlServer,
                DatabaseTypeMapper.DatabaseType.Oracle,
                DatabaseTypeMapper.DatabaseType.PostgreSQL,
                DatabaseTypeMapper.DatabaseType.MySQL,
                DatabaseTypeMapper.DatabaseType.MariaDB,
                DatabaseTypeMapper.DatabaseType.SQLite
            };
            _specificEntities = specificEntities;
            _generateAll = generateAll;
            _metadata = metadata;
        }

        /// <summary>
        /// Generates scripts for all entities or specific entities
        /// </summary>
        public async Task<int> GenerateScriptsAsync()
        {
            try
            {
                Console.WriteLine("Starting SQL script generation...");
                Console.WriteLine($"Output path: {_outputPath}");
                Console.WriteLine($"Database types: {string.Join(", ", _databaseTypes)}");
                Console.WriteLine();

                // Get all entity types from Beep.OilandGas.Models.Data
                var entityTypes = GetEntityTypes();

                if (_specificEntities != null && _specificEntities.Count > 0)
                {
                    entityTypes = entityTypes.Where(et => _specificEntities.Contains(et.Name, StringComparer.OrdinalIgnoreCase)).ToList();
                    Console.WriteLine($"Generating scripts for {entityTypes.Count} specific entities: {string.Join(", ", _specificEntities)}");
                }
                else
                {
                    Console.WriteLine($"Found {entityTypes.Count} entity types to process");
                }

                Console.WriteLine();

                // Check which scripts already exist
                var existingScripts = GetExistingScripts();
                var scriptsToGenerate = new List<(Type entityType, DatabaseTypeMapper.DatabaseType dbType, ScriptType scriptType)>();

                foreach (var entityType in entityTypes)
                {
                    var tableName = entityType.Name;
                    foreach (var dbType in _databaseTypes)
                    {
                        var dbTypeName = GetDatabaseTypeFolderName(dbType);
                        var scriptsPath = Path.Combine(_outputPath, dbTypeName);

                        // Check if scripts exist
                        var tabExists = existingScripts.Contains(Path.Combine(scriptsPath, $"{tableName}_TAB.sql"));
                        var pkExists = existingScripts.Contains(Path.Combine(scriptsPath, $"{tableName}_PK.sql"));
                        var fkExists = existingScripts.Contains(Path.Combine(scriptsPath, $"{tableName}_FK.sql"));

                        if (!tabExists) scriptsToGenerate.Add((entityType, dbType, ScriptType.TAB));
                        if (!pkExists) scriptsToGenerate.Add((entityType, dbType, ScriptType.PK));
                        if (!fkExists) scriptsToGenerate.Add((entityType, dbType, ScriptType.FK));
                    }
                }

                Console.WriteLine($"Scripts to generate: {scriptsToGenerate.Count}");
                Console.WriteLine();

                // Generate scripts
                int generated = 0;
                int skipped = 0;
                int errors = 0;

                foreach (var (entityType, dbType, scriptType) in scriptsToGenerate)
                {
                    try
                    {
                        var generator = new PPDMScriptGenerator(dbType, _metadata);
                        var tableName = entityType.Name;
                        var dbTypeName = GetDatabaseTypeFolderName(dbType);
                        var scriptsPath = Path.Combine(_outputPath, dbTypeName);

                        // Ensure directory exists
                        Directory.CreateDirectory(scriptsPath);

                        string scriptContent;
                        string fileName;

                        switch (scriptType)
                        {
                            case ScriptType.TAB:
                                scriptContent = await generator.GenerateTableScriptAsync(entityType);
                                fileName = $"{tableName}_TAB.sql";
                                break;
                            case ScriptType.PK:
                                scriptContent = await generator.GeneratePrimaryKeyScriptAsync(entityType);
                                fileName = $"{tableName}_PK.sql";
                                break;
                            case ScriptType.FK:
                                scriptContent = await generator.GenerateForeignKeyScriptsAsync(entityType);
                                fileName = $"{tableName}_FK.sql";
                                break;
                            default:
                                continue;
                        }

                        if (string.IsNullOrWhiteSpace(scriptContent))
                        {
                            skipped++;
                            continue;
                        }

                        var filePath = Path.Combine(scriptsPath, fileName);
                        await File.WriteAllTextAsync(filePath, scriptContent);
                        generated++;

                        Console.WriteLine($"✓ Generated: {dbTypeName}/{fileName}");
                    }
                    catch (Exception ex)
                    {
                        errors++;
                        Console.WriteLine($"✗ Error generating {entityType.Name} {scriptType} for {dbType}: {ex.Message}");
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Script generation complete!");
                Console.WriteLine($"Generated: {generated}");
                Console.WriteLine($"Skipped: {skipped}");
                Console.WriteLine($"Errors: {errors}");

                return errors > 0 ? 1 : 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return 1;
            }
        }

        /// <summary>
        /// Gets all entity types from Beep.OilandGas.Models.Data namespace
        /// </summary>
        private List<Type> GetEntityTypes()
        {
            var entityAssembly = typeof(AFE).Assembly; // Use AFE as reference to get the assembly
            var entityTypes = entityAssembly.GetTypes()
                .Where(t => 
                    t.Namespace == "Beep.OilandGas.Models.Data" &&
                    t.IsClass &&
                    !t.IsAbstract &&
                    t.BaseType != null &&
                    t.BaseType.Name == "Entity")
                .OrderBy(t => t.Name)
                .ToList();

            return entityTypes;
        }

        /// <summary>
        /// Gets existing script files to avoid regenerating
        /// </summary>
        private HashSet<string> GetExistingScripts()
        {
            var existingScripts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!Directory.Exists(_outputPath))
                return existingScripts;

            foreach (var dbType in _databaseTypes)
            {
                var dbTypeName = GetDatabaseTypeFolderName(dbType);
                var scriptsPath = Path.Combine(_outputPath, dbTypeName);
                
                if (Directory.Exists(scriptsPath))
                {
                    var sqlFiles = Directory.GetFiles(scriptsPath, "*.sql", SearchOption.TopDirectoryOnly);
                    foreach (var file in sqlFiles)
                    {
                        existingScripts.Add(file);
                    }
                }
            }

            return existingScripts;
        }

        /// <summary>
        /// Gets folder name for database type
        /// </summary>
        private string GetDatabaseTypeFolderName(DatabaseTypeMapper.DatabaseType dbType)
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

        /// <summary>
        /// Main entry point for command-line usage
        /// </summary>
        public static async Task<int> Main(string[] args)
        {
            try
            {
                // Parse command-line arguments
                var outputPath = "Scripts";
                var databaseTypes = new List<DatabaseTypeMapper.DatabaseType>();
                var specificEntities = new List<string>();
                var generateAll = true;

                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i].ToLower())
                    {
                        case "--output":
                        case "-o":
                            if (i + 1 < args.Length)
                            {
                                outputPath = args[++i];
                            }
                            break;
                        case "--db-types":
                        case "-d":
                            if (i + 1 < args.Length)
                            {
                                var dbTypes = args[++i].Split(',');
                                foreach (var dbType in dbTypes)
                                {
                                    if (Enum.TryParse<DatabaseTypeMapper.DatabaseType>(dbType.Trim(), true, out var parsedType))
                                    {
                                        databaseTypes.Add(parsedType);
                                    }
                                }
                            }
                            break;
                        case "--entity":
                        case "-e":
                            if (i + 1 < args.Length)
                            {
                                specificEntities.Add(args[++i]);
                                generateAll = false;
                            }
                            break;
                        case "--help":
                        case "-h":
                            PrintHelp();
                            return 0;
                    }
                }

                if (databaseTypes.Count == 0)
                {
                    databaseTypes = new List<DatabaseTypeMapper.DatabaseType>
                    {
                        DatabaseTypeMapper.DatabaseType.SqlServer,
                        DatabaseTypeMapper.DatabaseType.Oracle,
                        DatabaseTypeMapper.DatabaseType.PostgreSQL,
                        DatabaseTypeMapper.DatabaseType.MySQL,
                        DatabaseTypeMapper.DatabaseType.MariaDB,
                        DatabaseTypeMapper.DatabaseType.SQLite
                    };
                }

                var program = new GenerateScriptsProgram(
                    outputPath,
                    databaseTypes,
                    specificEntities.Count > 0 ? specificEntities : null,
                    generateAll);

                return await program.GenerateScriptsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return 1;
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine("SQL Script Generator");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  GenerateScriptsProgram [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --output, -o <path>     Output directory for scripts (default: Scripts)");
            Console.WriteLine("  --db-types, -d <types>  Comma-separated database types (SqlServer, Oracle, PostgreSQL, MySQL, MariaDB, SQLite)");
            Console.WriteLine("  --entity, -e <name>     Generate scripts for specific entity (can be used multiple times)");
            Console.WriteLine("  --help, -h              Show this help message");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  GenerateScriptsProgram --output Scripts");
            Console.WriteLine("  GenerateScriptsProgram --entity AFE --db-types SqlServer,Oracle");
            Console.WriteLine("  GenerateScriptsProgram --output Scripts --db-types SqlServer");
        }
    }
}

