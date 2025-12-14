using System;
using System.IO;
using System.Linq;
using Beep.OilandGas.PPDM39.DataManagement.Tools;

namespace Beep.OilandGas.PPDM39.DataManagement.Tools
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("PPDM39 Metadata Generator");
            Console.WriteLine("========================================");
            Console.WriteLine();

            // Default paths to SQL scripts
            var scriptFolder = @"C:\Users\f_ald\OneDrive\SimpleInfoapps\PPDM\ppdm39_SqlServerScript\ms";
            
            // Allow override via command line arguments
            if (args.Length >= 1 && !string.IsNullOrWhiteSpace(args[0]))
            {
                scriptFolder = args[0];
            }

            var tabScript = Path.Combine(scriptFolder, "TAB.sql");
            var pkScript = Path.Combine(scriptFolder, "PK.sql");
            var fkScript = Path.Combine(scriptFolder, "FK.sql");

            // Output paths (relative to solution root)
            var solutionRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."));
            var outputFolder = Path.Combine(solutionRoot, "Beep.OilandGas.PPDM39.DataManagement", "Core", "Metadata");
            
            // Create output folder if it doesn't exist
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            var jsonOutput = Path.Combine(outputFolder, "PPDM39Metadata.json");
            var csharpOutput = Path.Combine(outputFolder, "PPDM39Metadata.Generated.cs");

            Console.WriteLine("Configuration:");
            Console.WriteLine($"  Script Folder: {scriptFolder}");
            Console.WriteLine($"  TAB.sql: {tabScript}");
            Console.WriteLine($"  PK.sql: {pkScript}");
            Console.WriteLine($"  FK.sql: {fkScript}");
            Console.WriteLine($"  Output Folder: {outputFolder}");
            Console.WriteLine();

            // Validate input files
            var errors = new System.Collections.Generic.List<string>();
            if (!File.Exists(tabScript))
                errors.Add($"TAB.sql not found: {tabScript}");
            if (!File.Exists(pkScript))
                errors.Add($"PK.sql not found: {pkScript}");
            if (!File.Exists(fkScript))
                errors.Add($"FK.sql not found: {fkScript}");

            if (errors.Any())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: Missing required files:");
                foreach (var error in errors)
                {
                    Console.WriteLine($"  - {error}");
                }
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("Usage: Beep.OilandGas.PPDM39.DataManagement.Tools.exe [scriptFolder]");
                Console.WriteLine("Example: Beep.OilandGas.PPDM39.DataManagement.Tools.exe \"C:\\Path\\To\\Scripts\"");
                Environment.Exit(1);
                return;
            }

            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("All SQL scripts found. Starting metadata generation...");
                Console.ResetColor();
                Console.WriteLine();

                var generator = new PPDMMetadataGenerator(tabScript, pkScript, fkScript);

                // Generate JSON
                Console.WriteLine("Step 1: Generating JSON metadata...");
                Console.ForegroundColor = ConsoleColor.Yellow;
                generator.GenerateJsonMetadata(jsonOutput);
                Console.ResetColor();
                Console.WriteLine();

                // Generate C# class
                Console.WriteLine("Step 2: Generating C# metadata class...");
                Console.ForegroundColor = ConsoleColor.Yellow;
                generator.GenerateCSharpMetadata(csharpOutput);
                Console.ResetColor();
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("========================================");
                Console.WriteLine("Metadata generation complete!");
                Console.WriteLine("========================================");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("Generated files:");
                Console.WriteLine($"  - {jsonOutput}");
                Console.WriteLine($"  - {csharpOutput}");
                Console.WriteLine();
                Console.WriteLine("Next steps:");
                Console.WriteLine("  1. Review generated files");
                Console.WriteLine("  2. Use PPDMMetadataRepository.FromGeneratedClass() in your code");
                Console.WriteLine("  3. Commit generated files to source control");
                Console.WriteLine();
                
                // Generate CSV data JSON
                Console.WriteLine("Step 3: Generating CSV seed data JSON...");
                var csvFolder = @"C:\Users\f_ald\OneDrive\SimpleInfoapps\PPDM\PPDM_DATA\CSV";
                
                // Allow override via command line arguments (second argument)
                if (args.Length >= 2 && !string.IsNullOrWhiteSpace(args[1]))
                {
                    csvFolder = args[1];
                }
                
                if (Directory.Exists(csvFolder))
                {
                    var csvDataOutputFolder = Path.Combine(solutionRoot, "Beep.OilandGas.PPDM39.DataManagement", "Core", "SeedData");
                    if (!Directory.Exists(csvDataOutputFolder))
                    {
                        Directory.CreateDirectory(csvDataOutputFolder);
                    }
                    
                    var csvDataJsonOutput = Path.Combine(csvDataOutputFolder, "PPDMCSVData.json");
                    
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    PPDMCSVDataGenerator.GenerateJsonFromCsvFiles(csvFolder, csvDataJsonOutput);
                    Console.ResetColor();
                    Console.WriteLine();
                    Console.WriteLine("CSV data JSON generated:");
                    Console.WriteLine($"  - {csvDataJsonOutput}");
                    Console.WriteLine();
                    Console.WriteLine("Next steps for CSV data:");
                    Console.WriteLine("  1. Set PPDMCSVData.json as 'Embedded Resource' in project properties");
                    Console.WriteLine("  2. Use PPDMCSVDataLoader.LoadFromEmbeddedResource() in your seeder");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"CSV folder not found: {csvFolder}");
                    Console.WriteLine("Skipping CSV data generation. To generate CSV data, provide CSV folder path as second argument.");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("========================================");
                Console.WriteLine("ERROR: Generation failed!");
                Console.WriteLine("========================================");
                Console.ResetColor();
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine();
                Console.WriteLine("Stack trace:");
                Console.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
