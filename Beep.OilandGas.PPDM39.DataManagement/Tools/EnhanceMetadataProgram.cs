using System;
using System.IO;

namespace Beep.OilandGas.PPDM39.DataManagement.Tools
{
    class EnhanceMetadataProgram
    {
        static void Main(string[] args)
        {
            try
            {
                var solutionRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", ".."));
                
                var metadataPath = args.Length > 0 
                    ? args[0] 
                    : Path.Combine(solutionRoot, "Beep.OilandGas.PPDM39.DataManagement", "Core", "Metadata", "PPDM39Metadata.json");
                
                var descriptionsPath = args.Length > 1 
                    ? args[1] 
                    : Path.Combine(solutionRoot, "Beep.OilandGas.PPDM39.DataManagement", "Core", "Metadata", "ppdm38tabledescr.json");

                var outputPath = args.Length > 2 ? args[2] : metadataPath;

                // Resolve absolute paths
                metadataPath = Path.GetFullPath(metadataPath);
                descriptionsPath = Path.GetFullPath(descriptionsPath);
                outputPath = Path.GetFullPath(outputPath);

                Console.WriteLine($"Metadata: {metadataPath}");
                Console.WriteLine($"Descriptions: {descriptionsPath}");
                Console.WriteLine($"Output: {outputPath}");

                EnhanceMetadataWithDescriptions.EnhanceMetadata(metadataPath, descriptionsPath, outputPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            }
        }
    }
}

