using System;
using System.IO;

namespace Beep.OilandGas.PPDM39.DataManagement.Tools
{
    class ParseTCMProgram
    {
        static void Main(string[] args)
        {
            try
            {
                // Get paths
                var solutionRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", ".."));
                var tcmSqlPath = args.Length > 0 
                    ? args[0] 
                    : Path.Combine(solutionRoot, "..", "..", "..", "OneDrive", "SimpleInfoapps", "PPDM", "html", "TCM.sql");
                
                var outputPath = args.Length > 1 
                    ? args[1] 
                    : Path.Combine(solutionRoot, "Beep.OilandGas.PPDM39.DataManagement", "Core", "Metadata", "ppdm38tabledescr.json");

                // Resolve absolute paths
                tcmSqlPath = Path.GetFullPath(tcmSqlPath);
                outputPath = Path.GetFullPath(outputPath);

                Console.WriteLine($"Reading TCM.sql from: {tcmSqlPath}");
                Console.WriteLine($"Writing JSON to: {outputPath}");

                ParseTCMDescriptions.ParseAndCreateJson(tcmSqlPath, outputPath);
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

