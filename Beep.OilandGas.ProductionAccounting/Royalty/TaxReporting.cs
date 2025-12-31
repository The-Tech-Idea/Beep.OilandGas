namespace Beep.OilandGas.ProductionAccounting.Royalty
{
    /// <summary>
    /// Represents 1099 reporting information.
    /// </summary>
    public class Form1099Info
    {
        /// <summary>
        /// Gets or sets the 1099 identifier.
        /// </summary>
        public string Form1099Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the tax year.
        /// </summary>
        public int TaxYear { get; set; }

        /// <summary>
        /// Gets or sets the royalty owner identifier.
        /// </summary>
        public string RoyaltyOwnerId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the tax identification number.
        /// </summary>
        public string TaxId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the total royalty payments.
        /// </summary>
        public decimal TotalRoyaltyPayments { get; set; }

        /// <summary>
        /// Gets or sets the total federal tax withheld.
        /// </summary>
        public decimal TotalFederalTaxWithheld { get; set; }

        /// <summary>
        /// Gets or sets the total state tax withheld.
        /// </summary>
        public decimal TotalStateTaxWithheld { get; set; }

        /// <summary>
        /// Gets or sets whether the tax ID is valid.
        /// </summary>
        public bool IsTaxIdValid { get; set; } = true;
    }

    /// <summary>
    /// Provides tax reporting functionality.
    /// </summary>
    public static class TaxReporting
    {
        /// <summary>
        /// Validates a tax identification number.
        /// </summary>
        public static bool ValidateTaxId(string taxId)
        {
            if (string.IsNullOrEmpty(taxId))
                return false;

            // Remove dashes and spaces
            string cleanTaxId = taxId.Replace("-", "").Replace(" ", "");

            // SSN: 9 digits, EIN: 9 digits
            if (cleanTaxId.Length != 9)
                return false;

            // Check if all digits
            return cleanTaxId.All(char.IsDigit);
        }

        /// <summary>
        /// Calculates invalid tax ID withholding (typically 24% backup withholding).
        /// </summary>
        public static decimal CalculateInvalidTaxIdWithholding(decimal paymentAmount)
        {
            const decimal backupWithholdingRate = 0.24m; // 24% backup withholding
            return paymentAmount * backupWithholdingRate;
        }

        /// <summary>
        /// Calculates out of state withholding.
        /// </summary>
        public static decimal CalculateOutOfStateWithholding(
            decimal paymentAmount,
            string ownerState,
            string productionState,
            decimal withholdingRate)
        {
            if (ownerState == productionState)
                return 0;

            return paymentAmount * withholdingRate;
        }

        /// <summary>
        /// Calculates alien withholding (typically 30% for non-resident aliens).
        /// </summary>
        public static decimal CalculateAlienWithholding(decimal paymentAmount, bool isResidentAlien = false)
        {
            if (isResidentAlien)
                return 0;

            const decimal nonResidentAlienRate = 0.30m; // 30% for non-resident aliens
            return paymentAmount * nonResidentAlienRate;
        }

        /// <summary>
        /// Creates 1099 information for a royalty owner.
        /// </summary>
        public static Form1099Info CreateForm1099(
            string royaltyOwnerId,
            string taxId,
            int taxYear,
            List<RoyaltyPayment> payments)
        {
            if (string.IsNullOrEmpty(royaltyOwnerId))
                throw new ArgumentException("Royalty owner ID cannot be null or empty.", nameof(royaltyOwnerId));

            if (string.IsNullOrEmpty(taxId))
                throw new ArgumentException("Tax ID cannot be null or empty.", nameof(taxId));

            var paymentsInYear = payments
                .Where(p => p.PaymentDate.Year == taxYear)
                .ToList();

            var form1099 = new Form1099Info
            {
                Form1099Id = Guid.NewGuid().ToString(),
                TaxYear = taxYear,
                RoyaltyOwnerId = royaltyOwnerId,
                TaxId = taxId,
                TotalRoyaltyPayments = paymentsInYear.Sum(p => p.RoyaltyAmount),
                TotalFederalTaxWithheld = paymentsInYear.Sum(p => 
                    p.TaxWithholdings.Where(t => t.WithholdingType == TaxWithholdingType.BackupWithholding || 
                                                 t.WithholdingType == TaxWithholdingType.Alien)
                     .Sum(t => t.Amount)),
                TotalStateTaxWithheld = paymentsInYear.Sum(p => 
                    p.TaxWithholdings.Where(t => t.WithholdingType == TaxWithholdingType.OutOfState)
                     .Sum(t => t.Amount)),
                IsTaxIdValid = ValidateTaxId(taxId)
            };

            return form1099;
        }
    }
}
