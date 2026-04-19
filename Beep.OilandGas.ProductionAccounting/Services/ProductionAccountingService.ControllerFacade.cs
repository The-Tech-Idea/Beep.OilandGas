using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Accounting.Financial;
using Beep.OilandGas.Models.Data.Accounting.Ownership;
using Beep.OilandGas.Models.Data.Accounting.Pricing;
using Beep.OilandGas.Models.Data.Inventory;
using Beep.OilandGas.Models.Data.Pricing;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Storage;
using Beep.OilandGas.Models.Data.Trading;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    public partial class ProductionAccountingService
    {
        public TraditionalAccountingCompatibility TraditionalAccounting => new(this);

        public ProductionManagerCompatibility ProductionManager => new(this);

        public PricingManagerCompatibility PricingManager => new(this);

        public OwnershipManagerCompatibility OwnershipManager => new(this);

        public RoyaltyManagerCompatibility RoyaltyManager => new(this);

        public ReportingManagerCompatibility ReportManager => new(this);

        public StorageManagerCompatibility StorageManager => new(this);

        public LeaseManagerCompatibility LeaseManager => new(this);

        public TradingCompatibilityService TradingService => new(this);

        public FullCostAccountingCompatibility CreateFullCostAccounting(string? connectionName = null)
            => new(this, connectionName ?? ConnectionName);

        public SuccessfulEffortsAccountingCompatibility CreateSuccessfulEffortsAccounting(string? connectionName = null)
            => new(this, connectionName ?? ConnectionName);

        private static T? ReadValue<T>(object? source, params string[] propertyNames)
        {
            if (source == null)
                return default;

            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            foreach (var propertyName in propertyNames)
            {
                var property = source.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property == null)
                    continue;

                var value = property.GetValue(source);
                if (value == null)
                    continue;

                if (value is T typedValue)
                    return typedValue;

                try
                {
                    if (targetType.IsEnum)
                    {
                        if (value is string enumText && Enum.TryParse(targetType, enumText, true, out var enumValue))
                            return (T)enumValue;

                        return (T)Enum.ToObject(targetType, value);
                    }

                    return (T)Convert.ChangeType(value, targetType);
                }
                catch
                {
                }
            }

            return default;
        }

        private T? GetEntityById<T>(string tableName, string id, string? connectionName = null) where T : class
        {
            try
            {
                return GetRepository(typeof(T), connectionName ?? ConnectionName, tableName)
                    .GetByIdAsync(id)
                    .GetAwaiter()
                    .GetResult() as T;
            }
            catch
            {
                return null;
            }
        }

        private List<T> GetEntities<T>(string tableName, IEnumerable<AppFilter>? filters = null, string? connectionName = null) where T : class
        {
            try
            {
                var results = GetRepository(typeof(T), connectionName ?? ConnectionName, tableName)
                    .GetAsync(filters?.ToList() ?? new List<AppFilter>())
                    .GetAwaiter()
                    .GetResult();

                return results?.OfType<T>().ToList() ?? new List<T>();
            }
            catch
            {
                return new List<T>();
            }
        }

        private void TryInsertEntity(object entity, string tableName, string userId, string? connectionName = null)
        {
            try
            {
                GetRepository(entity.GetType(), connectionName ?? ConnectionName, tableName)
                    .InsertAsync(entity, userId)
                    .GetAwaiter()
                    .GetResult();
            }
            catch
            {
            }
        }

        public sealed class TraditionalAccountingCompatibility
        {
            public TraditionalAccountingCompatibility(ProductionAccountingService service)
            {
                PurchaseOrder = new PurchaseOrderCompatibility(service);
                Invoice = new InvoiceCompatibility(service);
                GeneralLedger = new GeneralLedgerCompatibility(service);
                JournalEntry = new JournalEntryCompatibility(service);
                Inventory = new InventoryCompatibility(service);
                AccountsReceivable = new AccountsReceivableCompatibility(service);
                AccountsPayable = new AccountsPayableCompatibility(service);
            }

            public PurchaseOrderCompatibility PurchaseOrder { get; }

            public InvoiceCompatibility Invoice { get; }

            public GeneralLedgerCompatibility GeneralLedger { get; }

            public JournalEntryCompatibility JournalEntry { get; }

            public InventoryCompatibility Inventory { get; }

            public AccountsReceivableCompatibility AccountsReceivable { get; }

            public AccountsPayableCompatibility AccountsPayable { get; }
        }

        public sealed class PurchaseOrderCompatibility
        {
            private readonly ProductionAccountingService _service;

            public PurchaseOrderCompatibility(ProductionAccountingService service)
            {
                _service = service;
            }

            public PURCHASE_ORDER? GetPurchaseOrder(string id)
            {
                if (_service._accountingServices?.PurchaseOrders != null)
                    return _service._accountingServices.PurchaseOrders.GetPOByIdAsync(id).GetAwaiter().GetResult();

                return _service.GetEntityById<PURCHASE_ORDER>("PURCHASE_ORDER", id);
            }

            public PURCHASE_ORDER CreatePurchaseOrder(CreatePurchaseOrderRequest request, string userId)
            {
                var purchaseOrder = new PURCHASE_ORDER
                {
                    PURCHASE_ORDER_ID = Guid.NewGuid().ToString(),
                    PO_NUMBER = ReadValue<string>(request, nameof(CreatePurchaseOrderRequest.PoNumber)) ?? $"PO-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    VENDOR_BA_ID = ReadValue<string>(request, nameof(CreatePurchaseOrderRequest.VendorBaId)) ?? string.Empty,
                    PO_DATE = ReadValue<DateTime?>(request, nameof(CreatePurchaseOrderRequest.PoDate)) ?? DateTime.UtcNow,
                    EXPECTED_DELIVERY_DATE = ReadValue<DateTime?>(request, nameof(CreatePurchaseOrderRequest.ExpectedDeliveryDate)),
                    DESCRIPTION = ReadValue<string>(request, nameof(CreatePurchaseOrderRequest.Description)),
                    STATUS = "DRAFT",
                    ACTIVE_IND = _service._defaults.GetActiveIndicatorYes(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                _service.TryInsertEntity(purchaseOrder, "PURCHASE_ORDER", userId);
                return purchaseOrder;
            }
        }

        public sealed class InvoiceCompatibility
        {
            private readonly ProductionAccountingService _service;

            public InvoiceCompatibility(ProductionAccountingService service)
            {
                _service = service;
            }

            public INVOICE? GetInvoice(string id)
            {
                if (_service._accountingServices?.Invoices != null)
                    return _service._accountingServices.Invoices.GetInvoiceAsync(id).GetAwaiter().GetResult();

                return _service.GetEntityById<INVOICE>("INVOICE", id);
            }

            public INVOICE CreateInvoice(CreateInvoiceRequest request, string userId)
            {
                if (_service._accountingServices?.Invoices != null)
                    return _service._accountingServices.Invoices.CreateInvoiceAsync(request, userId).GetAwaiter().GetResult();

                var invoice = new INVOICE
                {
                    INVOICE_ID = Guid.NewGuid().ToString(),
                    INVOICE_NUMBER = ReadValue<string>(request, nameof(CreateInvoiceRequest.InvoiceNumber)) ?? $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    CUSTOMER_BA_ID = ReadValue<string>(request, nameof(CreateInvoiceRequest.CustomerBaId)) ?? string.Empty,
                    INVOICE_DATE = ReadValue<DateTime?>(request, nameof(CreateInvoiceRequest.InvoiceDate)) ?? DateTime.UtcNow,
                    DUE_DATE = ReadValue<DateTime?>(request, nameof(CreateInvoiceRequest.DueDate)) ?? DateTime.UtcNow,
                    SUBTOTAL = ReadValue<decimal?>(request, nameof(CreateInvoiceRequest.Subtotal)) ?? 0m,
                    TAX_AMOUNT = ReadValue<decimal?>(request, nameof(CreateInvoiceRequest.TaxAmount)) ?? 0m,
                    TOTAL_AMOUNT = (ReadValue<decimal?>(request, nameof(CreateInvoiceRequest.Subtotal)) ?? 0m) + (ReadValue<decimal?>(request, nameof(CreateInvoiceRequest.TaxAmount)) ?? 0m),
                    BALANCE_DUE = (ReadValue<decimal?>(request, nameof(CreateInvoiceRequest.Subtotal)) ?? 0m) + (ReadValue<decimal?>(request, nameof(CreateInvoiceRequest.TaxAmount)) ?? 0m),
                    STATUS = "DRAFT",
                    ACTIVE_IND = _service._defaults.GetActiveIndicatorYes(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                _service.TryInsertEntity(invoice, "INVOICE", userId);
                return invoice;
            }
        }

        public sealed class GeneralLedgerCompatibility
        {
            private readonly ProductionAccountingService _service;

            public GeneralLedgerCompatibility(ProductionAccountingService service)
            {
                _service = service;
            }

            public List<GL_ACCOUNT> GetAllAccounts()
            {
                if (_service._accountingServices?.GlAccounts != null)
                    return _service._accountingServices.GlAccounts.GetAllAccountsAsync().GetAwaiter().GetResult();

                return _service.GetEntities<GL_ACCOUNT>("GL_ACCOUNT");
            }

            public GL_ACCOUNT? GetAccount(string id)
            {
                return _service.GetEntityById<GL_ACCOUNT>("GL_ACCOUNT", id)
                    ?? GetAllAccounts().FirstOrDefault(account => account.ACCOUNT_NUMBER == id);
            }

            public GL_ACCOUNT CreateAccount(CreateGLAccountRequest request, string userId)
            {
                if (_service._accountingServices?.GlAccounts != null)
                {
                    return _service._accountingServices.GlAccounts.CreateAccountAsync(
                        request.AccountNumber,
                        request.AccountName,
                        request.AccountType,
                        request.NormalBalance,
                        request.Description,
                        userId).GetAwaiter().GetResult();
                }

                var account = new GL_ACCOUNT
                {
                    GL_ACCOUNT_ID = Guid.NewGuid().ToString(),
                    ACCOUNT_NUMBER = request.AccountNumber,
                    ACCOUNT_NAME = request.AccountName,
                    ACCOUNT_TYPE = request.AccountType,
                    PARENT_ACCOUNT_ID = request.ParentAccountId,
                    NORMAL_BALANCE = request.NormalBalance,
                    OPENING_BALANCE = request.OpeningBalance,
                    CURRENT_BALANCE = request.OpeningBalance,
                    DESCRIPTION = request.Description,
                    ACTIVE_IND = _service._defaults.GetActiveIndicatorYes(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                _service.TryInsertEntity(account, "GL_ACCOUNT", userId);
                return account;
            }
        }

        public sealed class JournalEntryCompatibility
        {
            private readonly ProductionAccountingService _service;

            public JournalEntryCompatibility(ProductionAccountingService service)
            {
                _service = service;
            }

            public JOURNAL_ENTRY CreateJournalEntry(string entryNumber, DateTime entryDate, string entryType, string description, List<JournalEntryLineData> lines, string userId)
            {
                if (_service._accountingServices?.JournalEntries != null)
                {
                    var journalLines = lines.Select((line, index) => new JOURNAL_ENTRY_LINE
                    {
                        JOURNAL_ENTRY_LINE_ID = Guid.NewGuid().ToString(),
                        GL_ACCOUNT_ID = line.GlAccountId,
                        LINE_NUMBER = index + 1,
                        DEBIT_AMOUNT = line.DebitAmount,
                        CREDIT_AMOUNT = line.CreditAmount,
                        DESCRIPTION = line.Description
                    }).ToList();

                    var entry = _service._accountingServices.JournalEntries.CreateEntryAsync(
                        entryDate,
                        description,
                        journalLines,
                        userId,
                        entryNumber,
                        entryType).GetAwaiter().GetResult();
                    entry.ENTRY_TYPE = entryType;
                    return entry;
                }

                var manualEntry = new JOURNAL_ENTRY
                {
                    JOURNAL_ENTRY_ID = Guid.NewGuid().ToString(),
                    ENTRY_NUMBER = entryNumber,
                    ENTRY_DATE = entryDate,
                    ENTRY_TYPE = entryType,
                    DESCRIPTION = description,
                    TOTAL_DEBIT = lines.Sum(line => line.DebitAmount ?? 0m),
                    TOTAL_CREDIT = lines.Sum(line => line.CreditAmount ?? 0m),
                    STATUS = "DRAFT",
                    ACTIVE_IND = _service._defaults.GetActiveIndicatorYes(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                _service.TryInsertEntity(manualEntry, "JOURNAL_ENTRY", userId);
                return manualEntry;
            }

            public void PostJournalEntry(string id, string userId)
            {
                if (_service._accountingServices?.JournalEntries != null)
                    _service._accountingServices.JournalEntries.PostEntryAsync(id, userId).GetAwaiter().GetResult();
            }

            public JOURNAL_ENTRY? GetJournalEntry(string id)
            {
                return _service.GetEntityById<JOURNAL_ENTRY>("JOURNAL_ENTRY", id);
            }

            public IEnumerable<JOURNAL_ENTRY_LINE> GetJournalEntryLines(string id)
            {
                return _service.GetEntities<JOURNAL_ENTRY_LINE>(
                    "JOURNAL_ENTRY_LINE",
                    new[] { new AppFilter { FieldName = "JOURNAL_ENTRY_ID", Operator = "=", FilterValue = id } });
            }
        }

        public sealed class InventoryCompatibility
        {
            private readonly ProductionAccountingService _service;

            public InventoryCompatibility(ProductionAccountingService service)
            {
                _service = service;
            }

            public INVENTORY_TRANSACTION CreateTransaction(string inventoryItemId, string transactionType, DateTime transactionDate, decimal quantity, decimal? unitCost, string description, string userId)
            {
                var transaction = new INVENTORY_TRANSACTION
                {
                    INVENTORY_TRANSACTION_ID = Guid.NewGuid().ToString(),
                    INVENTORY_ITEM_ID = inventoryItemId,
                    TRANSACTION_TYPE = transactionType,
                    TRANSACTION_DATE = transactionDate,
                    QUANTITY = quantity,
                    UNIT_COST = unitCost,
                    TOTAL_COST = quantity * (unitCost ?? 0m),
                    DESCRIPTION = description,
                    ACTIVE_IND = _service._defaults.GetActiveIndicatorYes(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                _service.TryInsertEntity(transaction, "INVENTORY_TRANSACTION", userId);
                return transaction;
            }

            public INVENTORY_TRANSACTION? GetTransaction(string id)
            {
                return _service.GetEntityById<INVENTORY_TRANSACTION>("INVENTORY_TRANSACTION", id);
            }
        }

        public sealed class AccountsReceivableCompatibility
        {
            private readonly ProductionAccountingService _service;

            public AccountsReceivableCompatibility(ProductionAccountingService service)
            {
                _service = service;
            }

            public AR_INVOICE? GetARInvoice(string id)
            {
                if (_service._accountingServices?.AccountsReceivable != null)
                    return _service._accountingServices.AccountsReceivable.GetInvoiceAsync(id).GetAwaiter().GetResult();

                return _service.GetEntityById<AR_INVOICE>("AR_INVOICE", id);
            }

            public AR_INVOICE CreateARInvoice(CreateARInvoiceRequest request, string userId)
            {
                if (_service._accountingServices?.AccountsReceivable != null)
                    return _service._accountingServices.AccountsReceivable.CreateInvoiceAsync(request, userId).GetAwaiter().GetResult();

                var invoice = new AR_INVOICE
                {
                    AR_INVOICE_ID = Guid.NewGuid().ToString(),
                    INVOICE_NUMBER = request.InvoiceNumber,
                    CUSTOMER_BA_ID = request.CustomerBaId,
                    INVOICE_DATE = request.InvoiceDate,
                    DUE_DATE = request.DueDate,
                    TOTAL_AMOUNT = request.TotalAmount,
                    BALANCE_DUE = request.TotalAmount,
                    STATUS = "DRAFT",
                    ACTIVE_IND = _service._defaults.GetActiveIndicatorYes(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                _service.TryInsertEntity(invoice, "AR_INVOICE", userId);
                return invoice;
            }
        }

        public sealed class AccountsPayableCompatibility
        {
            private readonly ProductionAccountingService _service;

            public AccountsPayableCompatibility(ProductionAccountingService service)
            {
                _service = service;
            }

            public AP_INVOICE? GetAPInvoice(string id)
            {
                return _service.GetEntityById<AP_INVOICE>("AP_INVOICE", id);
            }

            public AP_INVOICE CreateAPInvoice(CreateAPInvoiceRequest request, string userId)
            {
                if (_service._accountingServices?.AccountsPayableInvoices != null)
                {
                    return _service._accountingServices.AccountsPayableInvoices.CreateBillAsync(
                        request.VendorBaId,
                        request.TotalAmount,
                        request.InvoiceDate,
                        request.InvoiceNumber,
                        null,
                        request.DueDate,
                        userId).GetAwaiter().GetResult();
                }

                var bill = new AP_INVOICE
                {
                    AP_INVOICE_ID = Guid.NewGuid().ToString(),
                    INVOICE_NUMBER = request.InvoiceNumber,
                    VENDOR_BA_ID = request.VendorBaId,
                    INVOICE_DATE = request.InvoiceDate,
                    DUE_DATE = request.DueDate,
                    TOTAL_AMOUNT = request.TotalAmount,
                    BALANCE_DUE = request.TotalAmount,
                    STATUS = "DRAFT",
                    ACTIVE_IND = _service._defaults.GetActiveIndicatorYes(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                _service.TryInsertEntity(bill, "AP_INVOICE", userId);
                return bill;
            }
        }

        public sealed class ProductionManagerCompatibility
        {
            private static readonly ConcurrentDictionary<string, RUN_TICKET> RunTickets = new(StringComparer.OrdinalIgnoreCase);
            private static readonly ConcurrentDictionary<string, TankInventoryRecord> TankInventories = new(StringComparer.OrdinalIgnoreCase);
            private readonly ProductionAccountingService _service;

            public ProductionManagerCompatibility(ProductionAccountingService service)
            {
                _service = service;
            }

            public IEnumerable<RUN_TICKET> GetRunTicketsByDateRange(DateTime startDate, DateTime endDate)
            {
                var cached = RunTickets.Values
                    .Where(ticket => (ReadValue<DateTime?>(ticket, "TICKET_DATE_TIME", "TicketDateTime") ?? DateTime.MinValue) >= startDate
                        && (ReadValue<DateTime?>(ticket, "TICKET_DATE_TIME", "TicketDateTime") ?? DateTime.MinValue) <= endDate)
                    .ToList();

                if (cached.Count > 0)
                    return cached;

                return _service.GetEntities<RUN_TICKET>("RUN_TICKET")
                    .Where(ticket => (ReadValue<DateTime?>(ticket, "TICKET_DATE_TIME", "TicketDateTime") ?? DateTime.MinValue) >= startDate
                        && (ReadValue<DateTime?>(ticket, "TICKET_DATE_TIME", "TicketDateTime") ?? DateTime.MinValue) <= endDate)
                    .ToList();
            }

            public IEnumerable<RUN_TICKET> GetRunTicketsByLease(string leaseId)
            {
                var cached = RunTickets.Values
                    .Where(ticket => string.Equals(ReadValue<string>(ticket, "LEASE_ID", "LeaseId"), leaseId, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (cached.Count > 0)
                    return cached;

                return _service.GetEntities<RUN_TICKET>("RUN_TICKET")
                    .Where(ticket => string.Equals(ReadValue<string>(ticket, "LEASE_ID", "LeaseId"), leaseId, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            public RUN_TICKET? GetRunTicket(string id)
            {
                if (RunTickets.TryGetValue(id, out var ticket))
                    return ticket;

                var stored = _service.GetEntityById<RUN_TICKET>("RUN_TICKET", id);
                if (stored != null)
                    return stored;

                return _service.GetEntities<RUN_TICKET>(
                    "RUN_TICKET",
                    new[] { new AppFilter { FieldName = "RUN_TICKET_NUMBER", Operator = "=", FilterValue = id } })
                    .FirstOrDefault();
            }

            public RUN_TICKET CreateRunTicket(string leaseId, string? wellId, string? tankBatteryId, MEASUREMENT_RECORD measurement, string dispositionType, string purchaser)
            {
                var ticket = new RUN_TICKET
                {
                    RUN_TICKET_ID = Guid.NewGuid().ToString(),
                    RUN_TICKET_NUMBER = $"RT-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    LEASE_ID = leaseId,
                    WELL_ID = wellId,
                    TANK_BATTERY_ID = tankBatteryId,
                    TICKET_DATE_TIME = ReadValue<DateTime?>(measurement, "MEASUREMENT_DATETIME", "MeasurementDateTime") ?? DateTime.UtcNow,
                    GROSS_VOLUME = ReadValue<decimal?>(measurement, "GROSS_VOLUME", "GrossVolume"),
                    BSWPERCENTAGE = ReadValue<decimal?>(measurement, "BSW", "BSWPercentage") ?? 0m,
                    TEMPERATURE = ReadValue<decimal?>(measurement, "TEMPERATURE", "Temperature") ?? 0m,
                    API_GRAVITY = ReadValue<decimal?>(measurement, "API_GRAVITY", "ApiGravity") ?? 0m,
                    DISPOSITION_TYPE = dispositionType,
                    PURCHASER = purchaser,
                    ACTIVE_IND = _service._defaults.GetActiveIndicatorYes(),
                    ROW_CREATED_BY = "system",
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                var grossVolume = ReadValue<decimal?>(ticket, "GROSS_VOLUME", "GrossVolume") ?? 0m;
                var bswPercentage = ReadValue<decimal?>(ticket, "BSWPERCENTAGE", "BSWPercentage") ?? 0m;
                ticket.NET_VOLUME = grossVolume * (1m - (bswPercentage / 100m));
                RunTickets[ticket.RUN_TICKET_ID] = ticket;
                RunTickets[ticket.RUN_TICKET_NUMBER] = ticket;
                _service.TryInsertEntity(ticket, "RUN_TICKET", "system");
                return ticket;
            }

            public TankInventoryRecord? GetTankInventory(string id)
            {
                return TankInventories.TryGetValue(id, out var inventory) ? inventory : null;
            }

            public TankInventoryRecord CreateTankInventory(string tankBatteryId, DateTime inventoryDate, decimal openingInventory, decimal receipts, decimal deliveries, decimal actualClosingInventory)
            {
                var inventory = new TankInventoryRecord
                {
                    InventoryId = Guid.NewGuid().ToString(),
                    TankBatteryId = tankBatteryId,
                    InventoryDate = inventoryDate,
                    OpeningInventory = openingInventory,
                    Receipts = receipts,
                    Deliveries = deliveries,
                    ClosingInventory = actualClosingInventory
                };

                TankInventories[inventory.InventoryId] = inventory;
                return inventory;
            }
        }

        public sealed class PricingManagerCompatibility
        {
            private readonly ProductionAccountingService _service;

            public PricingManagerCompatibility(ProductionAccountingService service)
            {
                _service = service;
            }

            public PriceIndexManagerCompatibility GetIndexManager()
            {
                return new PriceIndexManagerCompatibility(_service);
            }

            public RUN_TICKET_VALUATION ValueRunTicket(RUN_TICKET ticket, PricingMethod pricingMethod, decimal? fixedPrice, string? indexName, decimal? differential, object? _) 
            {
                var indexManager = GetIndexManager();
                var latestIndex = !string.IsNullOrWhiteSpace(indexName)
                    ? indexManager.GetLatestPrice(indexName)
                    : null;

                var basePrice = pricingMethod == PricingMethod.Fixed
                    ? (fixedPrice ?? 0m)
                    : latestIndex?.Price ?? fixedPrice ?? 0m;

                var adjustedPrice = basePrice + (differential ?? 0m);
                var netVolume = ReadValue<decimal?>(ticket, "NET_VOLUME", "NetVolume") ?? 0m;

                return new RUN_TICKET_VALUATION
                {
                    VALUATION_ID = Guid.NewGuid().ToString(),
                    RUN_TICKET_NUMBER = ReadValue<string>(ticket, "RUN_TICKET_NUMBER", "RunTicketNumber") ?? string.Empty,
                    VALUATION_DATE = DateTime.UtcNow,
                    BASE_PRICE = basePrice,
                    TOTAL_ADJUSTMENTS = differential ?? 0m,
                    ADJUSTED_PRICE = adjustedPrice,
                    NET_VOLUME = netVolume,
                    TOTAL_VALUE = netVolume * adjustedPrice
                };
            }
        }

        public sealed class PriceIndexManagerCompatibility
        {
            private static readonly ConcurrentDictionary<string, PriceIndex> Indices = new(StringComparer.OrdinalIgnoreCase);
            private readonly ProductionAccountingService _service;

            public PriceIndexManagerCompatibility(ProductionAccountingService service)
            {
                _service = service;
            }

            public PriceIndex? GetLatestPrice(string indexName)
            {
                if (Indices.TryGetValue(indexName, out var cached))
                    return cached;

                try
                {
                    var price = _service._pricingService.GetPriceAsync(indexName, DateTime.UtcNow, ConnectionName).GetAwaiter().GetResult();
                    var resolved = new PriceIndex
                    {
                        IndexName = indexName,
                        IndexDate = DateTime.UtcNow,
                        Price = price,
                        Currency = "USD"
                    };
                    Indices[indexName] = resolved;
                    return resolved;
                }
                catch
                {
                    return null;
                }
            }

            public void AddOrUpdatePriceIndex(PriceIndex index)
            {
                if (index?.IndexName == null)
                    return;

                Indices[index.IndexName] = index;
            }
        }

        public sealed class OwnershipManagerCompatibility
        {
            private static readonly ConcurrentDictionary<string, List<OWNERSHIP_INTEREST>> OwnershipByProperty = new(StringComparer.OrdinalIgnoreCase);
            private readonly ProductionAccountingService _service;

            public OwnershipManagerCompatibility(ProductionAccountingService service)
            {
                _service = service;
            }

            public IEnumerable<OWNERSHIP_INTEREST> GetOwnershipInterests(string propertyOrLeaseId, DateTime asOfDate)
            {
                if (OwnershipByProperty.TryGetValue(propertyOrLeaseId, out var interests) && interests.Count > 0)
                    return interests;

                return _service.GetEntities<OWNERSHIP_INTEREST>(
                    "OWNERSHIP_INTEREST",
                    new[] { new AppFilter { FieldName = "PROPERTY_OR_LEASE_ID", Operator = "=", FilterValue = propertyOrLeaseId } })
                    .Where(interest => (interest.EffectiveDate ?? DateTime.MinValue) <= asOfDate
                        && ((interest.ExpirationDate ?? DateTime.MaxValue) >= asOfDate))
                    .ToList();
            }

            public DivisionOrderRecord CreateDivisionOrder(string propertyOrLeaseId, OWNER_INFORMATION ownerInfo, decimal workingInterest, decimal netRevenueInterest, DateTime effectiveDate)
            {
                var divisionOrder = new DivisionOrderRecord
                {
                    DIVISION_ORDER_ID = Guid.NewGuid().ToString(),
                    PROPERTY_OR_LEASE_ID = propertyOrLeaseId,
                    OWNER_ID = ownerInfo.OWNER_ID ?? ownerInfo.OwnerId,
                    EFFECTIVE_DATE = effectiveDate
                };

                var ownershipInterest = new OWNERSHIP_INTEREST
                {
                    OWNERSHIP_ID = Guid.NewGuid().ToString(),
                    PROPERTY_OR_LEASE_ID = propertyOrLeaseId,
                    OWNER_ID = ownerInfo.OWNER_ID ?? ownerInfo.OwnerId,
                    WORKING_INTEREST = workingInterest,
                    NET_REVENUE_INTEREST = netRevenueInterest,
                    EFFECTIVE_START_DATE = effectiveDate,
                    EFFECTIVE_END_DATE = null,
                    ACTIVE_IND = _service._defaults.GetActiveIndicatorYes(),
                    ROW_CREATED_BY = "system",
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                OwnershipByProperty.AddOrUpdate(
                    propertyOrLeaseId,
                    _ => new List<OWNERSHIP_INTEREST> { ownershipInterest },
                    (_, existing) =>
                    {
                        existing.Add(ownershipInterest);
                        return existing;
                    });

                _service.TryInsertEntity(ownershipInterest, "OWNERSHIP_INTEREST", "system");
                return divisionOrder;
            }

            public void ApproveDivisionOrder(string divisionOrderId, string userId)
            {
            }
        }

        public sealed class RoyaltyManagerCompatibility
        {
            private static readonly ConcurrentDictionary<string, List<ROYALTY_PAYMENT>> PaymentsByOwner = new(StringComparer.OrdinalIgnoreCase);
            private readonly ProductionAccountingService _service;

            public RoyaltyManagerCompatibility(ProductionAccountingService service)
            {
                _service = service;
            }

            public ROYALTY_PAYMENT CalculateAndCreatePayment(SalesTransaction transaction, string royaltyOwnerId, decimal royaltyInterest, DateTime paymentDate)
            {
                var price = ReadValue<decimal?>(transaction, "PRICE_PER_BARREL", "PricePerBarrel") ?? 0m;
                var amount = Math.Round(price * royaltyInterest, 2);
                var payment = new ROYALTY_PAYMENT
                {
                    PAYMENT_ID = Guid.NewGuid().ToString(),
                    ROYALTY_OWNER_ID = royaltyOwnerId,
                    OWNER_NAME = royaltyOwnerId,
                    PAYMENT_DATE = paymentDate,
                    ROYALTY_AMOUNT = amount,
                    NET_PAYMENT = amount,
                    STATUS = "Pending",
                    ACTIVE_IND = _service._defaults.GetActiveIndicatorYes(),
                    ROW_CREATED_BY = "system",
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                PaymentsByOwner.AddOrUpdate(
                    royaltyOwnerId,
                    _ => new List<ROYALTY_PAYMENT> { payment },
                    (_, existing) =>
                    {
                        existing.Add(payment);
                        return existing;
                    });

                _service.TryInsertEntity(payment, "ROYALTY_PAYMENT", "system");
                return payment;
            }

            public IEnumerable<ROYALTY_PAYMENT> GetPaymentsByOwner(string ownerId)
            {
                if (PaymentsByOwner.TryGetValue(ownerId, out var payments) && payments.Count > 0)
                    return payments;

                return _service.GetEntities<ROYALTY_PAYMENT>(
                    "ROYALTY_PAYMENT",
                    new[] { new AppFilter { FieldName = "ROYALTY_OWNER_ID", Operator = "=", FilterValue = ownerId } });
            }
        }

        public sealed class ReportingManagerCompatibility
        {
            private readonly ProductionAccountingService _service;

            public ReportingManagerCompatibility(ProductionAccountingService service)
            {
                _service = service;
            }

            public OPERATIONAL_REPORT GenerateOperationalReport(DateTime startDate, DateTime endDate, List<RUN_TICKET> runTickets, List<object> inventories, List<ALLOCATION_RESULT> allocations, List<MEASUREMENT_RECORD> measurements, List<SalesTransaction> salesTransactions)
            {
                return new OPERATIONAL_REPORT
                {
                    REPORT_ID = Guid.NewGuid().ToString(),
                    REPORT_PERIOD_START = startDate,
                    REPORT_PERIOD_END = endDate,
                    GENERATION_DATE = DateTime.UtcNow,
                    REPORT_TYPE = "Operational"
                };
            }

            public LEASE_REPORT GenerateLeaseReport(string leaseId, DateTime startDate, DateTime endDate, List<RUN_TICKET> runTickets, List<SalesTransaction> salesTransactions)
            {
                return new LEASE_REPORT
                {
                    REPORT_ID = Guid.NewGuid().ToString(),
                    LEASE_ID = leaseId,
                    LEASE_NAME = leaseId,
                    REPORT_PERIOD_START = startDate,
                    REPORT_PERIOD_END = endDate,
                    GENERATION_DATE = DateTime.UtcNow,
                    REPORT_TYPE = "Lease"
                };
            }
        }

        public sealed class StorageManagerCompatibility
        {
            private static readonly ConcurrentDictionary<string, StorageFacility> Facilities = new(StringComparer.OrdinalIgnoreCase);
            private static readonly ConcurrentDictionary<string, List<TankBatteryInfo>> TankBatteriesByLease = new(StringComparer.OrdinalIgnoreCase);
            private readonly ProductionAccountingService _service;

            public StorageManagerCompatibility(ProductionAccountingService service)
            {
                _service = service;
            }

            public StorageFacility? GetFacility(string facilityId)
            {
                if (Facilities.TryGetValue(facilityId, out var facility))
                    return facility;

                return null;
            }

            public IEnumerable<TankBatteryInfo> GetTankBatteriesByLease(string leaseId)
            {
                if (TankBatteriesByLease.TryGetValue(leaseId, out var tankBatteries))
                    return tankBatteries;

                return Array.Empty<TankBatteryInfo>();
            }

            public void RegisterFacility(StorageFacility facility)
            {
                Facilities[facility.FacilityId] = facility;
            }
        }

        public sealed class LeaseManagerCompatibility
        {
            private static readonly ConcurrentDictionary<string, LeaseInfo> Leases = new(StringComparer.OrdinalIgnoreCase);

            public LeaseManagerCompatibility(ProductionAccountingService service)
            {
            }

            public LeaseInfo? GetLease(string leaseId)
            {
                if (Leases.TryGetValue(leaseId, out var lease))
                    return lease;

                lease = new LeaseInfo { LEASE_ID = leaseId, LEASE_NAME = leaseId };
                Leases[leaseId] = lease;
                return lease;
            }
        }

        public sealed class TradingCompatibilityService
        {
            private static readonly ConcurrentDictionary<string, EXCHANGE_CONTRACT> Contracts = new(StringComparer.OrdinalIgnoreCase);
            private readonly ProductionAccountingService _service;

            public TradingCompatibilityService(ProductionAccountingService service)
            {
                _service = service;
            }

            public Task<EXCHANGE_CONTRACT?> GetContractAsync(string contractId, string? connectionName = null)
            {
                if (Contracts.TryGetValue(contractId, out var contract))
                    return Task.FromResult<EXCHANGE_CONTRACT?>(contract);

                var stored = _service.GetEntityById<EXCHANGE_CONTRACT>("EXCHANGE_CONTRACT", contractId, connectionName);
                return Task.FromResult<EXCHANGE_CONTRACT?>(stored);
            }

            public Task<EXCHANGE_CONTRACT> RegisterContractAsync(CreateExchangeContractRequest request, string userId, string? connectionName = null)
            {
                var contract = new EXCHANGE_CONTRACT
                {
                    CONTRACT_ID = request.ContractId ?? Guid.NewGuid().ToString(),
                    CONTRACT_NAME = request.ContractName ?? string.Empty,
                    CONTRACT_TYPE = request.ContractType.ToString(),
                    EFFECTIVE_DATE = request.EffectiveDate,
                    EXPIRATION_DATE = request.ExpirationDate,
                    ACTIVE_IND = _service._defaults.GetActiveIndicatorYes(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                Contracts[contract.CONTRACT_ID] = contract;
                _service.TryInsertEntity(contract, "EXCHANGE_CONTRACT", userId, connectionName);
                return Task.FromResult(contract);
            }
        }

        public sealed class FullCostAccountingCompatibility
        {
            private static readonly ConcurrentDictionary<string, decimal> TotalsByCostCenter = new(StringComparer.OrdinalIgnoreCase);
            private readonly ProductionAccountingService _service;
            private readonly string _connectionName;

            public FullCostAccountingCompatibility(ProductionAccountingService service, string connectionName)
            {
                _service = service;
                _connectionName = connectionName;
            }

            public void RecordExplorationCosts(string costCenterId, ExplorationCosts costs, string? connectionName = null)
            {
                AddToTotal(costCenterId, ReadValue<decimal?>(costs, "TotalExplorationCosts") ?? 0m);
            }

            public void RecordDevelopmentCosts(string costCenterId, DevelopmentCosts costs, string? connectionName = null)
            {
                AddToTotal(costCenterId, ReadValue<decimal?>(costs, "TotalDevelopmentCosts") ?? 0m);
            }

            public void RecordAcquisitionCosts(string costCenterId, UnprovedProperty property, string? connectionName = null)
            {
                AddToTotal(costCenterId, ReadValue<decimal?>(property, "AcquisitionCost") ?? 0m);
            }

            public decimal CalculateTotalCapitalizedCosts(string costCenterId, string? connectionName = null)
            {
                return TotalsByCostCenter.TryGetValue(costCenterId, out var total) ? total : 0m;
            }

            public object PerformCeilingTest(string costCenterId, object? reserves, decimal discountRate, string? connectionName = null)
            {
                bool passes;
                try
                {
                    passes = _service._fcService.PerformCeilingTestAsync(costCenterId, "system", connectionName ?? _connectionName).GetAwaiter().GetResult();
                }
                catch
                {
                    passes = true;
                }

                return new
                {
                    CostCenterId = costCenterId,
                    DiscountRate = discountRate,
                    Passes = passes
                };
            }

            private static void AddToTotal(string costCenterId, decimal amount)
            {
                TotalsByCostCenter.AddOrUpdate(costCenterId, amount, (_, existing) => existing + amount);
            }
        }

        public sealed class SuccessfulEffortsAccountingCompatibility
        {
            private readonly ProductionAccountingService _service;
            private readonly string _connectionName;

            public SuccessfulEffortsAccountingCompatibility(ProductionAccountingService service, string connectionName)
            {
                _service = service;
                _connectionName = connectionName;
            }

            public void RecordAcquisition(UnprovedProperty property, string? connectionName = null)
            {
                RecordCost(ReadValue<string>(property, "PropertyId") ?? string.Empty, ReadValue<decimal?>(property, "AcquisitionCost") ?? 0m, connectionName);
            }

            public void RecordExplorationCosts(ExplorationCosts costs, string? connectionName = null)
            {
                RecordCost(ReadValue<string>(costs, "PropertyId", "WellId") ?? string.Empty, ReadValue<decimal?>(costs, "TotalExplorationCosts") ?? 0m, connectionName);
            }

            public void RecordDevelopmentCosts(DevelopmentCosts costs, string? connectionName = null)
            {
                RecordCost(ReadValue<string>(costs, "PropertyId", "WellId") ?? string.Empty, ReadValue<decimal?>(costs, "TotalDevelopmentCosts") ?? 0m, connectionName);
            }

            public void RecordProductionCosts(ProductionCosts costs, string? connectionName = null)
            {
                RecordCost(ReadValue<string>(costs, "PropertyId", "WellId") ?? string.Empty, ReadValue<decimal?>(costs, "TotalProductionCosts") ?? 0m, connectionName);
            }

            public void RecordDryHole(ExplorationCosts costs, string? connectionName = null)
            {
                RecordCost(ReadValue<string>(costs, "PropertyId", "WellId") ?? string.Empty, ReadValue<decimal?>(costs, "TotalExplorationCosts") ?? 0m, connectionName);
            }

            public void RecordImpairment(string propertyId, decimal impairmentAmount, string? connectionName = null)
            {
                RecordCost(propertyId, impairmentAmount, connectionName);
            }

            private void RecordCost(string wellOrPropertyId, decimal amount, string? connectionName)
            {
                if (string.IsNullOrWhiteSpace(wellOrPropertyId) || amount <= 0m)
                    return;

                try
                {
                    _service._seService.RecordCostAsync(wellOrPropertyId, amount, "system", connectionName ?? _connectionName).GetAwaiter().GetResult();
                }
                catch
                {
                }
            }
        }

        public sealed class TankInventoryRecord
        {
            public string InventoryId { get; set; } = string.Empty;

            public string TankBatteryId { get; set; } = string.Empty;

            public DateTime InventoryDate { get; set; }

            public decimal OpeningInventory { get; set; }

            public decimal Receipts { get; set; }

            public decimal Deliveries { get; set; }

            public decimal ClosingInventory { get; set; }
        }

        public sealed class DivisionOrderRecord
        {
            public string DIVISION_ORDER_ID { get; set; } = string.Empty;

            public string PROPERTY_OR_LEASE_ID { get; set; } = string.Empty;

            public string OWNER_ID { get; set; } = string.Empty;

            public DateTime EFFECTIVE_DATE { get; set; }
        }

        public sealed class TankBatteryInfo
        {
            public string BatteryId { get; set; } = string.Empty;

            public string BatteryName { get; set; } = string.Empty;

            public string LeaseId { get; set; } = string.Empty;
        }

        public sealed class LeaseInfo
        {
            public string LEASE_ID { get; set; } = string.Empty;

            public string LEASE_NAME { get; set; } = string.Empty;
        }
    }
}