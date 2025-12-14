using System.Threading.Tasks;

namespace Beep.OilandGas.PPDM39.DataManagement.SeedData
{
    /// <summary>
    /// Interface for PPDM39 data seeders
    /// </summary>
    public interface IPPDMSeeder
    {
        /// <summary>
        /// Seeds data from CSV file
        /// </summary>
        /// <param name="csvFilePath">Path to CSV file</param>
        /// <param name="userId">User ID for audit columns</param>
        /// <returns>Number of records seeded</returns>
        Task<int> SeedAsync(string csvFilePath, string userId = "SYSTEM");
    }
}

