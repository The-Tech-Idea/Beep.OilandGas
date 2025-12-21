using System;

namespace Beep.OilandGas.Accounting.Operational.Revenue
{
    /// <summary>
    /// Receivable status enumeration.
    /// </summary>
    public enum ReceivableStatus
    {
        /// <summary>
        /// Open (not yet paid).
        /// </summary>
        Open,

        /// <summary>
        /// Partially paid.
        /// </summary>
        PartiallyPaid,

        /// <summary>
        /// Paid in full.
        /// </summary>
        Paid,

        /// <summary>
        /// Overdue.
        /// </summary>
        Overdue,

        /// <summary>
        /// Written off.
        /// </summary>
        WrittenOff
    }

    /// <summary>
    /// Represents a receivable.
    /// </summary>
    public class Receivable
    {
        /// <summary>
        /// Gets or sets the receivable identifier.
        /// </summary>
        public string ReceivableId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the transaction reference.
        /// </summary>
        public string TransactionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the customer (purchaser).
        /// </summary>
        public string Customer { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the invoice number.
        /// </summary>
        public string? InvoiceNumber { get; set; }

        /// <summary>
        /// Gets or sets the invoice date.
        /// </summary>
        public DateTime InvoiceDate { get; set; }

        /// <summary>
        /// Gets or sets the due date.
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Gets or sets the original amount.
        /// </summary>
        public decimal OriginalAmount { get; set; }

        /// <summary>
        /// Gets or sets the amount paid.
        /// </summary>
        public decimal AmountPaid { get; set; }

        /// <summary>
        /// Gets the outstanding balance.
        /// </summary>
        public decimal OutstandingBalance => OriginalAmount - AmountPaid;

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public ReceivableStatus Status { get; set; } = ReceivableStatus.Open;

        /// <summary>
        /// Gets whether the receivable is overdue.
        /// </summary>
        public bool IsOverdue => DateTime.Now > DueDate && OutstandingBalance > 0;

        /// <summary>
        /// Gets the days past due.
        /// </summary>
        public int DaysPastDue => IsOverdue ? (DateTime.Now - DueDate).Days : 0;
    }

    /// <summary>
    /// Manages receivables.
    /// </summary>
    public class ReceivableManager
    {
        private readonly Dictionary<string, Receivable> receivables = new();

        /// <summary>
        /// Creates a receivable from a sales transaction.
        /// </summary>
        public Receivable CreateReceivable(SalesTransaction transaction, int paymentTermsDays = 30)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            var receivable = new Receivable
            {
                ReceivableId = Guid.NewGuid().ToString(),
                TransactionId = transaction.TransactionId,
                Customer = transaction.Purchaser,
                InvoiceDate = transaction.TransactionDate,
                DueDate = transaction.TransactionDate.AddDays(paymentTermsDays),
                OriginalAmount = transaction.TotalValue,
                Status = ReceivableStatus.Open
            };

            receivables[receivable.ReceivableId] = receivable;
            return receivable;
        }

        /// <summary>
        /// Records a payment against a receivable.
        /// </summary>
        public void RecordPayment(string receivableId, decimal paymentAmount, DateTime paymentDate)
        {
            if (!receivables.TryGetValue(receivableId, out var receivable))
                throw new ArgumentException($"Receivable {receivableId} not found.", nameof(receivableId));

            if (paymentAmount <= 0)
                throw new ArgumentException("Payment amount must be greater than zero.", nameof(paymentAmount));

            receivable.AmountPaid += paymentAmount;

            if (receivable.OutstandingBalance <= 0)
                receivable.Status = ReceivableStatus.Paid;
            else if (receivable.AmountPaid > 0)
                receivable.Status = ReceivableStatus.PartiallyPaid;

            if (receivable.IsOverdue)
                receivable.Status = ReceivableStatus.Overdue;
        }

        /// <summary>
        /// Gets a receivable by ID.
        /// </summary>
        public Receivable? GetReceivable(string receivableId)
        {
            return receivables.TryGetValue(receivableId, out var receivable) ? receivable : null;
        }

        /// <summary>
        /// Gets all receivables for a customer.
        /// </summary>
        public IEnumerable<Receivable> GetReceivablesByCustomer(string customer)
        {
            return receivables.Values.Where(r => r.Customer == customer);
        }

        /// <summary>
        /// Gets overdue receivables.
        /// </summary>
        public IEnumerable<Receivable> GetOverdueReceivables()
        {
            return receivables.Values.Where(r => r.IsOverdue);
        }

        /// <summary>
        /// Gets total outstanding receivables.
        /// </summary>
        public decimal GetTotalOutstanding()
        {
            return receivables.Values.Sum(r => r.OutstandingBalance);
        }
    }
}

