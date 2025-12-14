using System;
using System.IO;

namespace Beep.OilandGas.PPDM39.DataManagement.Tools
{
    /// <summary>
    /// Console application to generate PPDM metadata from SQL scripts
    /// Run this ONCE to extract metadata and save as C# class or JSON
    /// </summary>
    class GenerateMetadata
    {
        static void Main(string[] args)
        {
            // Paths to SQL scripts
            var scriptFolder = @"C:\Users\f_ald\OneDrive\SimpleInfoapps\PPDM\ppdm39_SqlServerScript\ms";
            var tabScript = Path.Combine(scriptFolder, "TAB.sql");
            var pkScript = Path.Combine(scriptFolder, "PK.sql");
            var fkScript = Path.Combine(scriptFolder, "FK.sql");

            // Output paths
            var outputFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", 
                "Beep.OilandGas.PPDM39.DataManagement", "Core", "Metadata");
            var jsonOutput = Path.Combine(outputFolder, "PPDM39Metadata.json");
            var csharpOutput = Path.Combine(outputFolder, "PPDM39Metadata.Generated.cs");

            Console.WriteLine("PPDM39 Metadata Generator");
            Console.WriteLine("========================");
            Console.WriteLine($"TAB.sql: {tabScript}");
            Console.WriteLine($"PK.sql: {pkScript}");
            Console.WriteLine($"FK.sql: {fkScript}");
            Console.WriteLine();

            try
            {
                var generator = new PPDMMetadataGenerator(tabScript, pkScript, fkScript);

                // Generate JSON
                Console.WriteLine("Generating JSON metadata...");
                generator.GenerateJsonMetadata(jsonOutput);
                Console.WriteLine();

                // Generate C# class
                Console.WriteLine("Generating C# metadata class...");
                generator.GenerateCSharpMetadata(csharpOutput);
                Console.WriteLine();

                Console.WriteLine("Metadata generation complete!");
                Console.WriteLine($"JSON: {jsonOutput}");
                Console.WriteLine($"C#: {csharpOutput}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}

