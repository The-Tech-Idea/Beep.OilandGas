using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.DataManagement.Tools;

namespace Beep.OilandGas.PPDM39.DataManagement.Tools
{
    /// <summary>
    /// Test program to generate scripts for a few entities
    /// </summary>
    public class TestScriptGeneration
    {
        public static async Task TestAsync()
        {
            var scriptsPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..", "Beep.OilandGas.Models", "Scripts");

            scriptsPath = Path.GetFullPath(scriptsPath);
            Console.WriteLine($"Scripts path: {scriptsPath}");

            // Test with AFE entity
            var entityType = typeof(AFE);
            var dbType = DatabaseTypeMapper.DatabaseType.SqlServer;

            var generator = new PPDMScriptGenerator(dbType);
            
            Console.WriteLine("Generating scripts for AFE...");
            var tabScript = await generator.GenerateTableScriptAsync(entityType);
            var pkScript = await generator.GeneratePrimaryKeyScriptAsync(entityType);
            var fkScript = await generator.GenerateForeignKeyScriptsAsync(entityType);

            var dbPath = Path.Combine(scriptsPath, "Sqlserver");
            Directory.CreateDirectory(dbPath);

            await File.WriteAllTextAsync(Path.Combine(dbPath, "AFE_TAB.sql"), tabScript);
            await File.WriteAllTextAsync(Path.Combine(dbPath, "AFE_PK.sql"), pkScript);
            await File.WriteAllTextAsync(Path.Combine(dbPath, "AFE_FK.sql"), fkScript);

            Console.WriteLine("Scripts generated successfully!");
        }
    }
}

