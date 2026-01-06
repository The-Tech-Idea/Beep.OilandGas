namespace Beep.OilandGas.Models.EconomicAnalysis
{
    /// <summary>
    /// Represents a cash flow for a specific period in economic analysis
    /// DTO for calculations - Entity class: ECONOMIC_CASH_FLOW
    /// </summary>
    public class CashFlow
    {
        /// <summary>
        /// Time period (0, 1, 2, ...)
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// Cash flow amount (positive for inflows, negative for outflows)
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Optional description of the cash flow
        /// </summary>
        public string? Description { get; set; }
    }
}



