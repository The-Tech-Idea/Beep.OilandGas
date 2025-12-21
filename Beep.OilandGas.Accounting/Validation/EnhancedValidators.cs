using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Accounting.Models;
using Beep.OilandGas.Accounting.Operational.Production;
using Beep.OilandGas.Accounting.Operational.Revenue;
using Beep.OilandGas.Accounting.Exceptions;

namespace Beep.OilandGas.Accounting.Validation
{
    /// <summary>
    /// Provides enhanced validation beyond basic data validation.
    /// </summary>
    public static class EnhancedValidators
    {
        /// <summary>
        /// Validates run ticket for completeness and accuracy.
        /// </summary>
        public static ValidationResult ValidateRunTicket(RunTicket ticket)
        {
            var result = new ValidationResult();

            if (ticket == null)
            {
                result.AddError("RunTicket", "Run ticket cannot be null.");
                return result;
            }

            // Validate volumes
            if (ticket.GrossVolume < 0)
                result.AddError("GrossVolume", "Gross volume cannot be negative.");

            if (ticket.BSWVolume < 0)
                result.AddError("BSWVolume", "BS&W volume cannot be negative.");

            if (ticket.BSWVolume > ticket.GrossVolume)
                result.AddError("BSWVolume", "BS&W volume cannot exceed gross volume.");

            // Validate BS&W percentage
            decimal calculatedBSW = ticket.GrossVolume > 0 
                ? (ticket.BSWVolume / ticket.GrossVolume) * 100m 
                : 0;

            if (Math.Abs(calculatedBSW - ticket.BSWPercentage) > 0.1m)
                result.AddWarning("BSWPercentage", $"BS&W percentage ({ticket.BSWPercentage}%) does not match calculated value ({calculatedBSW:F2}%).");

            // Validate net volume
            decimal calculatedNet = ticket.GrossVolume - ticket.BSWVolume;
            if (Math.Abs(calculatedNet - ticket.NetVolume) > 0.01m)
                result.AddError("NetVolume", $"Net volume ({ticket.NetVolume}) does not match calculated value ({calculatedNet}).");

            // Validate pricing
            if (ticket.PricePerBarrel.HasValue && ticket.PricePerBarrel.Value < 0)
                result.AddError("PricePerBarrel", "Price per barrel cannot be negative.");

            if (ticket.PricePerBarrel.HasValue && ticket.TotalValue.HasValue)
            {
                decimal calculatedValue = ticket.NetVolume * ticket.PricePerBarrel.Value;
                if (Math.Abs(calculatedValue - ticket.TotalValue.Value) > 0.01m)
                    result.AddWarning("TotalValue", $"Total value ({ticket.TotalValue}) does not match calculated value ({calculatedValue:F2}).");
            }

            return result;
        }

        /// <summary>
        /// Validates sales transaction for accounting accuracy.
        /// </summary>
        public static ValidationResult ValidateSalesTransaction(SalesTransaction transaction)
        {
            var result = new ValidationResult();

            if (transaction == null)
            {
                result.AddError("SalesTransaction", "Sales transaction cannot be null.");
                return result;
            }

            // Validate volumes
            if (transaction.NetVolume < 0)
                result.AddError("NetVolume", "Net volume cannot be negative.");

            // Validate pricing
            if (transaction.PricePerBarrel < 0)
                result.AddError("PricePerBarrel", "Price per barrel cannot be negative.");

            // Validate total value
            decimal calculatedValue = transaction.NetVolume * transaction.PricePerBarrel;
            if (Math.Abs(calculatedValue - transaction.TotalValue) > 0.01m)
                result.AddError("TotalValue", $"Total value ({transaction.TotalValue}) does not match calculated value ({calculatedValue:F2}).");

            // Validate net revenue
            decimal calculatedNetRevenue = transaction.TotalValue - transaction.Costs.TotalCosts - transaction.Taxes.Sum(t => t.Amount);
            if (Math.Abs(calculatedNetRevenue - transaction.NetRevenue) > 0.01m)
                result.AddError("NetRevenue", $"Net revenue ({transaction.NetRevenue}) does not match calculated value ({calculatedNetRevenue:F2}).");

            // Validate costs
            if (transaction.Costs.TotalCosts < 0)
                result.AddError("TotalCosts", "Total costs cannot be negative.");

            // Validate taxes
            foreach (var tax in transaction.Taxes)
            {
                if (tax.Amount < 0)
                    result.AddError($"Tax_{tax.TaxId}", $"Tax amount for {tax.TaxType} cannot be negative.");
            }

            return result;
        }

        /// <summary>
        /// Validates allocation result for accuracy.
        /// </summary>
        public static ValidationResult ValidateAllocationResult(Allocation.AllocationResult allocation)
        {
            var result = new ValidationResult();

            if (allocation == null)
            {
                result.AddError("AllocationResult", "Allocation result cannot be null.");
                return result;
            }

            // Validate total volume
            if (allocation.TotalVolume < 0)
                result.AddError("TotalVolume", "Total volume cannot be negative.");

            // Validate allocated volume matches total
            decimal totalAllocated = allocation.Details.Sum(d => d.AllocatedVolume);
            decimal variance = Math.Abs(allocation.TotalVolume - totalAllocated);

            if (variance > 0.01m)
                result.AddError("AllocatedVolume", $"Total allocated volume ({totalAllocated}) does not match total volume ({allocation.TotalVolume}). Variance: {variance}.");

            // Validate percentages sum to 100
            decimal totalPercentage = allocation.Details.Sum(d => d.AllocationPercentage);
            if (Math.Abs(totalPercentage - 100m) > 0.1m)
                result.AddWarning("AllocationPercentage", $"Total allocation percentage ({totalPercentage:F2}%) does not equal 100%.");

            // Validate individual allocations
            foreach (var detail in allocation.Details)
            {
                if (detail.AllocatedVolume < 0)
                    result.AddError($"Detail_{detail.EntityId}", $"Allocated volume for {detail.EntityId} cannot be negative.");

                if (detail.AllocationPercentage < 0 || detail.AllocationPercentage > 100)
                    result.AddError($"Detail_{detail.EntityId}", $"Allocation percentage for {detail.EntityId} must be between 0 and 100.");
            }

            return result;
        }

        /// <summary>
        /// Validates royalty calculation for accuracy.
        /// </summary>
        public static ValidationResult ValidateRoyaltyCalculation(Royalty.RoyaltyCalculation calculation)
        {
            var result = new ValidationResult();

            if (calculation == null)
            {
                result.AddError("RoyaltyCalculation", "Royalty calculation cannot be null.");
                return result;
            }

            // Validate net revenue
            decimal calculatedNetRevenue = calculation.GrossRevenue - calculation.Deductions.TotalDeductions;
            if (Math.Abs(calculatedNetRevenue - calculation.NetRevenue) > 0.01m)
                result.AddError("NetRevenue", $"Net revenue ({calculation.NetRevenue}) does not match calculated value ({calculatedNetRevenue:F2}).");

            // Validate royalty amount
            decimal calculatedRoyalty = calculation.NetRevenue * calculation.RoyaltyInterest;
            if (Math.Abs(calculatedRoyalty - calculation.RoyaltyAmount) > 0.01m)
                result.AddError("RoyaltyAmount", $"Royalty amount ({calculation.RoyaltyAmount}) does not match calculated value ({calculatedRoyalty:F2}).");

            // Validate interest
            if (calculation.RoyaltyInterest < 0 || calculation.RoyaltyInterest > 1)
                result.AddError("RoyaltyInterest", $"Royalty interest ({calculation.RoyaltyInterest}) must be between 0 and 1.");

            return result;
        }
    }

    /// <summary>
    /// Represents validation results with errors and warnings.
    /// </summary>
    public class ValidationResult
    {
        public List<ValidationIssue> Errors { get; set; } = new();
        public List<ValidationIssue> Warnings { get; set; } = new();

        public bool IsValid => Errors.Count == 0;
        public bool HasWarnings => Warnings.Count > 0;

        public void AddError(string field, string message)
        {
            Errors.Add(new ValidationIssue { Field = field, Message = message });
        }

        public void AddWarning(string field, string message)
        {
            Warnings.Add(new ValidationIssue { Field = field, Message = message });
        }

        public string GetSummary()
        {
            var summary = $"Validation Result: {(IsValid ? "Valid" : "Invalid")}";
            if (Errors.Count > 0)
                summary += $"\nErrors: {Errors.Count}";
            if (Warnings.Count > 0)
                summary += $"\nWarnings: {Warnings.Count}";
            return summary;
        }
    }

    /// <summary>
    /// Represents a validation issue.
    /// </summary>
    public class ValidationIssue
    {
        public string Field { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}

