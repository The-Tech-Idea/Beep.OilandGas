using System;
using System.IO;

namespace Beep.OilandGas.PPDM39.DataManagement.Tools
{
    /// <summary>
    /// Console program to update PPDM39Metadata.json with correct mappings
    /// </summary>
    public class UpdateMetadataProgram
    {
        public static void Main(string[] args)
        {
            try
            {
                // Get the path to the metadata JSON file
                var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
                var jsonPath = Path.Combine(solutionRoot, "Beep.OilandGas.PPDM39.DataManagement", "Core", "Metadata", "PPDM39Metadata.json");

                if (args.Length > 0)
                {
                    jsonPath = args[0];
                }

                if (!File.Exists(jsonPath))
                {
                    Console.WriteLine($"Error: File not found: {jsonPath}");
                    Console.WriteLine("Usage: UpdateMetadataProgram.exe [path-to-PPDM39Metadata.json]");
                    return;
                }

                Console.WriteLine("PPDM 3.9 Metadata Mapper");
                Console.WriteLine("========================");
                Console.WriteLine();

                UpdateMetadataMappings.UpdateMetadataFile(jsonPath);

                Console.WriteLine();
                Console.WriteLine("Update completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}

