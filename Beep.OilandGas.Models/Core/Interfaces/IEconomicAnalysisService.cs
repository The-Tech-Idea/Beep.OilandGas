using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.EconomicAnalysis;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for economic analysis operations.
    /// Provides comprehensive economic evaluation including NPV, IRR, scenario management, and risk analysis.
    /// </summary>
    public interface IEconomicAnalysisService
    {
        /// <summary>
        /// Calculates Net Present Value for cash flows.
        /// </summary>
        /// <param name="cashFlows">Array of cash flows</param>
        /// <param name="discountRate">Discount rate (e.g., 0.10 for 10%)</param>
        /// <returns>NPV value</returns>
        double CalculateNPV(CashFlow[] cashFlows, double discountRate);

        /// <summary>
        /// Calculates Internal Rate of Return for cash flows.
        /// </summary>
        /// <param name="cashFlows">Array of cash flows</param>
        /// <param name="initialGuess">Initial guess for IRR (default 0.1)</param>
        /// <returns>IRR value</returns>
        double CalculateIRR(CashFlow[] cashFlows, double initialGuess = 0.1);

        /// <summary>
        /// Performs comprehensive economic analysis.
        /// </summary>
        /// <param name="cashFlows">Array of cash flows</param>
        /// <param name="discountRate">Discount rate</param>
        /// <param name="financeRate">Finance rate for MIRR (default 0.1)</param>
        /// <param name="reinvestRate">Reinvestment rate for MIRR (default 0.1)</param>
        /// <returns>Economic analysis result</returns>
        EconomicResult Analyze(CashFlow[] cashFlows, double discountRate, double financeRate = 0.1, double reinvestRate = 0.1);

        /// <summary>
        /// Generates NPV profile for cash flows.
        /// </summary>
        /// <param name="cashFlows">Array of cash flows</param>
        /// <param name="minRate">Minimum discount rate (default 0.0)</param>
        /// <param name="maxRate">Maximum discount rate (default 1.0)</param>
        /// <param name="points">Number of points to generate (default 50)</param>
        /// <returns>List of NPV profile points</returns>
        List<NPVProfilePoint> GenerateNPVProfile(CashFlow[] cashFlows, double minRate = 0.0, double maxRate = 1.0, int points = 50);

        /// <summary>
        /// Saves economic analysis result to database.
        /// </summary>
        /// <param name="analysisId">Analysis identifier</param>
        /// <param name="result">Economic analysis result</param>
        /// <param name="userId">User ID for audit</param>
        /// <returns>Task</returns>
        Task SaveAnalysisResultAsync(string analysisId, EconomicResult result, string userId);

        /// <summary>
        /// Gets economic analysis result from database.
        /// </summary>
        /// <param name="analysisId">Analysis identifier</param>
        /// <returns>Economic analysis result, or null if not found</returns>
        Task<EconomicResult?> GetAnalysisResultAsync(string analysisId);
    }
}




