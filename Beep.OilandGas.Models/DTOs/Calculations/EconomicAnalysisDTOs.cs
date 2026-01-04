using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.EconomicAnalysis;

namespace Beep.OilandGas.Models.DTOs.Calculations
{
    /// <summary>
    /// Request for calculating Net Present Value (NPV)
    /// </summary>
    public class CalculateNPVRequest
    {
        /// <summary>
        /// Array of cash flows
        /// </summary>
        [Required(ErrorMessage = "CashFlows are required")]
        [MinLength(1, ErrorMessage = "At least one cash flow is required")]
        public CashFlow[] CashFlows { get; set; } = Array.Empty<CashFlow>();

        /// <summary>
        /// Discount rate (as decimal, e.g., 0.10 for 10%)
        /// </summary>
        [Required]
        [Range(0, 1, ErrorMessage = "DiscountRate must be between 0 and 1")]
        public double DiscountRate { get; set; }
    }

    /// <summary>
    /// Request for calculating Internal Rate of Return (IRR)
    /// </summary>
    public class CalculateIRRRequest
    {
        /// <summary>
        /// Array of cash flows
        /// </summary>
        [Required(ErrorMessage = "CashFlows are required")]
        [MinLength(1, ErrorMessage = "At least one cash flow is required")]
        public CashFlow[] CashFlows { get; set; } = Array.Empty<CashFlow>();

        /// <summary>
        /// Initial guess for IRR calculation (default: 0.1 or 10%)
        /// </summary>
        [Range(-1, 1, ErrorMessage = "InitialGuess must be between -1 and 1")]
        public double InitialGuess { get; set; } = 0.1;
    }

    /// <summary>
    /// Request for comprehensive economic analysis
    /// </summary>
    public class AnalyzeRequest
    {
        /// <summary>
        /// Array of cash flows
        /// </summary>
        [Required(ErrorMessage = "CashFlows are required")]
        [MinLength(1, ErrorMessage = "At least one cash flow is required")]
        public CashFlow[] CashFlows { get; set; } = Array.Empty<CashFlow>();

        /// <summary>
        /// Discount rate (as decimal, e.g., 0.10 for 10%)
        /// </summary>
        [Required]
        [Range(0, 1, ErrorMessage = "DiscountRate must be between 0 and 1")]
        public double DiscountRate { get; set; }

        /// <summary>
        /// Finance rate (as decimal, e.g., 0.10 for 10%)
        /// </summary>
        [Range(0, 1, ErrorMessage = "FinanceRate must be between 0 and 1")]
        public double FinanceRate { get; set; } = 0.1;

        /// <summary>
        /// Reinvestment rate (as decimal, e.g., 0.10 for 10%)
        /// </summary>
        [Range(0, 1, ErrorMessage = "ReinvestRate must be between 0 and 1")]
        public double ReinvestRate { get; set; } = 0.1;
    }

    /// <summary>
    /// Request for generating NPV profile
    /// </summary>
    public class GenerateNPVProfileRequest
    {
        /// <summary>
        /// Array of cash flows
        /// </summary>
        [Required(ErrorMessage = "CashFlows are required")]
        [MinLength(1, ErrorMessage = "At least one cash flow is required")]
        public CashFlow[] CashFlows { get; set; } = Array.Empty<CashFlow>();

        /// <summary>
        /// Minimum discount rate for profile
        /// </summary>
        [Range(0, 1, ErrorMessage = "MinRate must be between 0 and 1")]
        public double MinRate { get; set; } = 0.0;

        /// <summary>
        /// Maximum discount rate for profile
        /// </summary>
        [Range(0, 1, ErrorMessage = "MaxRate must be between 0 and 1")]
        public double MaxRate { get; set; } = 1.0;

        /// <summary>
        /// Number of points in the profile
        /// </summary>
        [Range(2, 1000, ErrorMessage = "Points must be between 2 and 1000")]
        public int Points { get; set; } = 50;
    }

    /// <summary>
    /// Request for saving economic analysis result
    /// </summary>
    public class SaveAnalysisResultRequest
    {
        /// <summary>
        /// Analysis identifier
        /// </summary>
        [Required(ErrorMessage = "AnalysisId is required")]
        public string AnalysisId { get; set; } = string.Empty;

        /// <summary>
        /// Economic analysis result to save
        /// </summary>
        [Required(ErrorMessage = "Result is required")]
        public EconomicResult Result { get; set; } = null!;
    }
}
