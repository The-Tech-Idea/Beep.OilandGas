using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ProductionAccounting.Accounting
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
    /// Uses database access via IDataSource instead of in-memory dictionaries.
    /// </summary>
    public class ReceivableManager
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<ReceivableManager>? _logger;
        private readonly string _connectionName;
        private const string RECEIVABLE_TABLE = "RECEIVABLE";

        public ReceivableManager(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILoggerFactory loggerFactory,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = loggerFactory?.CreateLogger<ReceivableManager>();
            _connectionName = connectionName ?? "PPDM39";
        }

        /// <summary>
        /// Creates a receivable from a sales transaction.
        /// </summary>
        public Receivable CreateReceivable(SalesTransaction transaction, int paymentTermsDays = 30, string? connectionName = null)
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

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var entity = ConvertReceivableToEntity(receivable);
            _commonColumnHandler.SetCommonColumns(entity, _defaults, _metadata, connName);
            var result = dataSource.InsertEntity(RECEIVABLE_TABLE, entity);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create receivable {ReceivableId}: {Error}", receivable.ReceivableId, errorMessage);
                throw new InvalidOperationException($"Failed to save receivable: {errorMessage}");
            }

            _logger?.LogDebug("Created receivable {ReceivableId} for customer {Customer} in database", receivable.ReceivableId, receivable.Customer);
            return receivable;
        }

        /// <summary>
        /// Records a payment against a receivable.
        /// </summary>
        public void RecordPayment(string receivableId, decimal paymentAmount, DateTime paymentDate, string? connectionName = null)
        {
            if (paymentAmount <= 0)
                throw new ArgumentException("Payment amount must be greater than zero.", nameof(paymentAmount));

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var receivable = GetReceivable(receivableId, connName);
            if (receivable == null)
                throw new ArgumentException($"Receivable {receivableId} not found.", nameof(receivableId));

            receivable.AmountPaid += paymentAmount;

            if (receivable.OutstandingBalance <= 0)
                receivable.Status = ReceivableStatus.Paid;
            else if (receivable.AmountPaid > 0)
                receivable.Status = ReceivableStatus.PartiallyPaid;

            if (receivable.IsOverdue)
                receivable.Status = ReceivableStatus.Overdue;

            // Update in database
            var entity = ConvertReceivableToEntity(receivable);
            _commonColumnHandler.SetCommonColumns(entity, _defaults, _metadata, connName);
            var result = dataSource.UpdateEntity(RECEIVABLE_TABLE, entity);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to update receivable {ReceivableId}: {Error}", receivableId, errorMessage);
                throw new InvalidOperationException($"Failed to update receivable: {errorMessage}");
            }

            _logger?.LogDebug("Recorded payment {PaymentAmount} for receivable {ReceivableId}", paymentAmount, receivableId);
        }

        /// <summary>
        /// Gets a receivable by ID.
        /// </summary>
        public Receivable? GetReceivable(string receivableId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(receivableId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "RECEIVABLE_ID", Operator = "=", FilterValue = receivableId }
            };

            var results = dataSource.GetEntityAsync(RECEIVABLE_TABLE, filters).GetAwaiter().GetResult();
            var entity = results?.OfType<RECEIVABLE>().FirstOrDefault();
            
            if (entity == null)
                return null;

            return ConvertEntityToReceivable(entity);
        }

        /// <summary>
        /// Gets all receivables for a customer.
        /// </summary>
        public IEnumerable<Receivable> GetReceivablesByCustomer(string customer, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(customer))
                return Enumerable.Empty<Receivable>();

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "CUSTOMER", Operator = "=", FilterValue = customer }
            };

            var results = dataSource.GetEntityAsync(RECEIVABLE_TABLE, filters).GetAwaiter().GetResult();
            if (results == null || !results.Any())
                return Enumerable.Empty<Receivable>();

            return results.OfType<RECEIVABLE>().Select(ConvertEntityToReceivable).Where(r => r != null)!;
        }

        /// <summary>
        /// Gets overdue receivables.
        /// </summary>
        public IEnumerable<Receivable> GetOverdueReceivables(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var today = DateTime.Now;
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "DUE_DATE", Operator = "<", FilterValue = today.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "STATUS", Operator = "!=", FilterValue = ReceivableStatus.Paid.ToString() }
            };

            var results = dataSource.GetEntityAsync(RECEIVABLE_TABLE, filters).GetAwaiter().GetResult();
            if (results == null || !results.Any())
                return Enumerable.Empty<Receivable>();

            return results.OfType<RECEIVABLE>().Select(ConvertEntityToReceivable)
                .Where(r => r != null && r.IsOverdue)!;
        }

        /// <summary>
        /// Gets total outstanding receivables.
        /// </summary>
        public decimal GetTotalOutstanding(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "STATUS", Operator = "!=", FilterValue = ReceivableStatus.Paid.ToString() }
            };

            var results = dataSource.GetEntityAsync(RECEIVABLE_TABLE, filters).GetAwaiter().GetResult();
            if (results == null || !results.Any())
                return 0m;

            return results.OfType<RECEIVABLE>().Select(ConvertEntityToReceivable)
                .Where(r => r != null)
                .Sum(r => r.OutstandingBalance);
        }

        private RECEIVABLE ConvertReceivableToEntity(Receivable receivable)
        {
            var entity = new RECEIVABLE
            {
                RECEIVABLE_ID = receivable.ReceivableId,
                TRANSACTION_ID = receivable.TransactionId,
                CUSTOMER = receivable.Customer,
                INVOICE_NUMBER = receivable.InvoiceNumber,
                INVOICE_DATE = receivable.InvoiceDate,
                DUE_DATE = receivable.DueDate,
                ORIGINAL_AMOUNT = receivable.OriginalAmount,
                AMOUNT_PAID = receivable.AmountPaid,
                STATUS = receivable.Status.ToString()
            };
            return entity;
        }

        private Receivable ConvertEntityToReceivable(RECEIVABLE entity)
        {
            var receivable = new Receivable
            {
                ReceivableId = entity.RECEIVABLE_ID ?? string.Empty,
                TransactionId = entity.TRANSACTION_ID ?? string.Empty,
                Customer = entity.CUSTOMER ?? string.Empty,
                InvoiceNumber = entity.INVOICE_NUMBER,
                InvoiceDate = entity.INVOICE_DATE ?? DateTime.MinValue,
                DueDate = entity.DUE_DATE ?? DateTime.MinValue,
                OriginalAmount = entity.ORIGINAL_AMOUNT ?? 0m,
                AmountPaid = entity.AMOUNT_PAID ?? 0m
            };
            
            if (!string.IsNullOrEmpty(entity.STATUS) && Enum.TryParse<ReceivableStatus>(entity.STATUS, out var stat))
                receivable.Status = stat;
            
            return receivable;
        }
    }
}

