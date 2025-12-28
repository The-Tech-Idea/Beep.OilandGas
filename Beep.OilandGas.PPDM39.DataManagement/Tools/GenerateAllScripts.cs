using System;
using System.IO;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.DataManagement.Tools;

namespace Beep.OilandGas.PPDM39.DataManagement.Tools
{
    /// <summary>
    /// Standalone program to generate all SQL scripts
    /// </summary>
    public class GenerateAllScripts
    {
        public static async Task<int> Main(string[] args)
        {
            try
            {
                // Determine output path - default to Beep.OilandGas.Models/Scripts
                var currentDir = AppDomain.CurrentDomain.BaseDirectory;
                var solutionRoot = Path.GetFullPath(Path.Combine(currentDir, "..", "..", "..", "..", ".."));
                var defaultOutput = Path.Combine(solutionRoot, "Beep.OilandGas.Models", "Scripts");

                var outputPath = args.Length > 0 ? args[0] : defaultOutput;
                outputPath = Path.GetFullPath(outputPath);

                Console.WriteLine("Generating SQL scripts for all entity classes...");
                Console.WriteLine($"Output directory: {outputPath}");
                Console.WriteLine();

                return await RunScriptGenerator.GenerateAllScriptsAsync(outputPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return 1;
            }
        }
    }
}

