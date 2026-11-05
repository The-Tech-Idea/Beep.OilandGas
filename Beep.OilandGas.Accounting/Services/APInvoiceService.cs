using Beep.OilandGas.Models.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Data.OilGas;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// AP (Accounts Payable) Invoice Service
    /// Manages vendor invoices, expense recognition, and payment tracking
    /// Uses: AP_INVOICE entity
    /// GL Posting: Debit Expense (6001), Credit AP (2000) on bill receipt
    ///            Debit AP (2000), Credit Cash (1000) on payment
    /// </summary>
    public partial class APInvoiceService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<APInvoiceService> _logger;
        private readonly Func<Task<string>>? _resolveConnection;

        public APInvoiceService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<APInvoiceService> logger,
            IAccountMappingService? accountMapping = null,
            Func<Task<string>>? resolveConnection = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _basisPosting = basisPosting ?? throw new ArgumentNullException(nameof(basisPosting));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountMapping = accountMapping;
            _resolveConnection = resolveConnection;
        }

        /// <summary>
        /// Create a new vendor bill (DRAFT status)
        /// </summary>
        public async Task<AP_INVOICE> CreateBillAsync(
            CompanyScope company,string vendorBaId,
            decimal invoiceAmount,
            DateTime invoiceDate,
            string? invoiceNumber = null,
            string? description = null,
            DateTime? dueDate = null,
            string userId = "SYSTEM")
        {
            APInvoiceOwnership.RequireScope(company);
            if (string.IsNullOrWhiteSpace(vendorBaId))
                throw new ArgumentNullException(nameof(vendorBaId));
            if (invoiceAmount <= 0)
                throw RefusalException.Invalid("Invoice amount must be greater than zero.");
            if (dueDate.HasValue && dueDate.Value.Date < invoiceDate.Date)
                throw RefusalException.Invalid("Due date cannot be earlier than the invoice date.");

            _logger?.LogInformation("Creating AP invoice for vendor {VendorId}, amount {Amount:C}",
                vendorBaId, invoiceAmount);

            try
            {
                var bill = new AP_INVOICE
                {
                    AP_INVOICE_ID = Guid.NewGuid().ToString(),
                    OWNER_TENANT_ID=company.TenantId,OWNER_COMPANY_BA_ID=company.BusinessAssociateId,
                    INVOICE_NUMBER = invoiceNumber ?? await GenerateInvoiceNumberAsync(),
                    VENDOR_BA_ID = vendorBaId,
                    INVOICE_DATE = invoiceDate,
                    DUE_DATE = dueDate,
                    TOTAL_AMOUNT = invoiceAmount,
                    PAID_AMOUNT = 0m,
                    BALANCE_DUE = invoiceAmount,
                    STATUS = "DRAFT",
                    ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow,
                    REMARK = description
                };

                var repo = await GetRepoAsync<AP_INVOICE>("AP_INVOICE");

                await repo.InsertAsync(bill, userId);
                _logger?.LogInformation("AP invoice {InvoiceNumber} created (DRAFT)", bill.INVOICE_NUMBER);
                return bill;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating bill");
                throw;
            }
        }

        /// <summary>
        /// Receive vendor bill (DRAFT -> RECEIVED)
        /// Posts GL entries: Debit Expense (6001), Credit AP (2000)
        /// </summary>
        public async Task<AP_INVOICE> ReceiveBillAsync(CompanyScope company,string billId, string userId = "SYSTEM")
        {
            APInvoiceOwnership.RequireScope(company);
            if (string.IsNullOrWhiteSpace(billId))
                throw new ArgumentNullException(nameof(billId));

            _logger?.LogInformation("Receiving AP invoice {BillId}", billId);

            try
            {
                var bill = await GetBillByIdAsync(company,billId);
                if (bill == null)
                    throw RefusalException.NotFound($"Bill {billId} was not found.");

                if (bill.STATUS != "DRAFT")
                    throw RefusalException.Conflict($"Only DRAFT bills can be received; this bill is {bill.STATUS}.");

                // Create GL journal entry: Debit Expense, Credit AP
                var lineItems = new List<JOURNAL_ENTRY_LINE>
                {
                    new JOURNAL_ENTRY_LINE
                    {
                        GL_ACCOUNT_ID = GetAccountId(AccountMappingKeys.OperatingExpense, DefaultGlAccounts.OperatingExpense),
                        DEBIT_AMOUNT = bill.TOTAL_AMOUNT,
                        CREDIT_AMOUNT = 0m,
                        DESCRIPTION = $"Bill {bill.INVOICE_NUMBER} - Vendor {bill.VENDOR_BA_ID}"
                    },
                    new JOURNAL_ENTRY_LINE
                    {
                        GL_ACCOUNT_ID = GetAccountId(AccountMappingKeys.AccountsPayable, DefaultGlAccounts.AccountsPayable),
                        DEBIT_AMOUNT = 0m,
                        CREDIT_AMOUNT = bill.TOTAL_AMOUNT,
                        DESCRIPTION = $"Liability for bill {bill.INVOICE_NUMBER}"
                    }
                };

                await _basisPosting.PostEntryAsync(
                    bill.INVOICE_DATE ?? DateTime.UtcNow,
                    $"Bill receipt: {bill.INVOICE_NUMBER}",
                    lineItems,
                    userId,
                    bill.INVOICE_NUMBER,
                    "AP");

                // Update bill status
                bill.STATUS = AccountingReferenceCodes.InvoiceStatusCodes.Received;
                bill.ROW_CHANGED_BY = userId;
                bill.ROW_CHANGED_DATE = DateTime.UtcNow;

                var repo = await GetRepoAsync<AP_INVOICE>("AP_INVOICE");

                await repo.UpdateAsync(bill, userId);
                _logger?.LogInformation("AP invoice {InvoiceNumber} received and posted to GL", bill.INVOICE_NUMBER);
                return bill;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error receiving bill {BillId}", billId);
                throw;
            }
        }

        /// <summary>
        /// Record a payment against a bill
        /// Posts GL entry: Debit AP (2000), Credit Cash (1000)
        /// </summary>
        public async Task<AP_INVOICE> RecordPaymentAsync(
            CompanyScope company,string billId,
            decimal paymentAmount,
            DateTime paymentDate,
            string userId = "SYSTEM")
        {
            APInvoiceOwnership.RequireScope(company);
            if (string.IsNullOrWhiteSpace(billId))
                throw new ArgumentNullException(nameof(billId));
            if (paymentAmount <= 0)
                throw RefusalException.Invalid("Payment amount must be greater than zero.");

            _logger?.LogInformation("Recording payment for bill {BillId}: {Amount:C}", billId, paymentAmount);

            try
            {
                var bill = await GetBillByIdAsync(company,billId);
                if (bill == null)
                    throw RefusalException.NotFound($"Bill {billId} was not found.");

                if (bill.STATUS != "RECEIVED" && bill.STATUS != InvoiceStatuses.PartiallyPaid)
                    throw RefusalException.Conflict($"A payment can be recorded only against a RECEIVED or PARTIALLY_PAID bill; this bill is {bill.STATUS}.");

                decimal currentBalance = bill.BALANCE_DUE ?? 0m;
                if (paymentAmount > currentBalance)
                    throw RefusalException.Conflict($"Payment {paymentAmount:C} exceeds the bill's balance of {currentBalance:C}.");

                // Create GL journal entry: Debit AP, Credit Cash
                var lineItems = new List<JOURNAL_ENTRY_LINE>
                {
                    new JOURNAL_ENTRY_LINE
                    {
                        GL_ACCOUNT_ID = GetAccountId(AccountMappingKeys.AccountsPayable, DefaultGlAccounts.AccountsPayable),
                        DEBIT_AMOUNT = paymentAmount,
                        CREDIT_AMOUNT = 0m,
                        DESCRIPTION = $"Payment for bill {bill.INVOICE_NUMBER}"
                    },
                    new JOURNAL_ENTRY_LINE
                    {
                        GL_ACCOUNT_ID = GetAccountId(AccountMappingKeys.Cash, DefaultGlAccounts.Cash),
                        DEBIT_AMOUNT = 0m,
                        CREDIT_AMOUNT = paymentAmount,
                        DESCRIPTION = $"Bill {bill.INVOICE_NUMBER} payment"
                    }
                };

                await _basisPosting.PostEntryAsync(
                    paymentDate,
                    $"Payment for bill {bill.INVOICE_NUMBER}",
                    lineItems,
                    userId,
                    $"PAY-{bill.INVOICE_NUMBER}",
                    "AP");

                // Update bill balance and status
                decimal newBalance = currentBalance - paymentAmount;
                bill.PAID_AMOUNT = (bill.PAID_AMOUNT ?? 0m) + paymentAmount;
                bill.BALANCE_DUE = newBalance;

                if (newBalance <= 0.01m)
                {
                    bill.STATUS = AccountingReferenceCodes.InvoiceStatusCodes.Paid;
                }
                else
                {
                    bill.STATUS = InvoiceStatuses.PartiallyPaid;
                }

                bill.ROW_CHANGED_BY = userId;
                bill.ROW_CHANGED_DATE = DateTime.UtcNow;

                var repo = await GetRepoAsync<AP_INVOICE>("AP_INVOICE");

                await repo.UpdateAsync(bill, userId);

                _logger?.LogInformation("Payment recorded for bill {InvoiceNumber}: {Amount:C}, New Status: {Status}",
                    bill.INVOICE_NUMBER, paymentAmount, bill.STATUS);

                return bill;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recording payment for bill {BillId}", billId);
                throw;
            }
        }

        /// <summary>
        /// Get bill by ID
        /// </summary>
        public async Task<AP_INVOICE?> GetBillByIdAsync(CompanyScope company,string billId)
        {
            APInvoiceOwnership.RequireScope(company);
            if (string.IsNullOrWhiteSpace(billId))
                return null;

            try
            {
                var repo = await GetRepoAsync<AP_INVOICE>("AP_INVOICE");

                var results=await repo.GetAsync(new List<AppFilter>{
                    new(){FieldName="AP_INVOICE_ID",Operator="=",FilterValue=billId},
                    new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},
                    new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId}})
                    ?? throw new InvalidOperationException("The AP invoice read was unavailable.");
                var rows=results.ToArray();if(rows.Length==0)return null;
                if(rows.Length!=1 || rows[0] is not AP_INVOICE invoice)throw new InvalidOperationException("The AP invoice identity is ambiguous or incompatible.");
                APInvoiceOwnership.Require(company,billId,invoice);return invoice;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting bill {BillId}", billId);
                throw;
            }
        }

        /// <summary>
        /// Get all bills for a vendor
        /// </summary>
        public async Task<List<AP_INVOICE>> GetVendorBillsAsync(CompanyScope company,string vendorBaId)
        {
            APInvoiceOwnership.RequireScope(company);
            if(string.IsNullOrWhiteSpace(vendorBaId))return new List<AP_INVOICE>();
            return await GetCompanyBillsAsync(company,vendorBaId);
        }

        /// <summary>Recorded active invoices in the selected company; optional exact supplier restriction.</summary>
        public async Task<List<AP_INVOICE>> GetCompanyBillsAsync(CompanyScope company,string? vendorBaId=null)
        {
            APInvoiceOwnership.RequireScope(company);
            if(vendorBaId is not null && string.IsNullOrWhiteSpace(vendorBaId))
                throw new InvalidOperationException("A supplied AP supplier restriction cannot be empty.");
            var repo=await GetRepoAsync<AP_INVOICE>("AP_INVOICE");
            var filters=new List<AppFilter>{
                new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},
                new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},
                new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}};
            if(vendorBaId is not null)filters.Add(new(){FieldName="VENDOR_BA_ID",Operator="=",FilterValue=vendorBaId});
            var bills=await repo.GetAsync(filters);
            if(bills is null || bills.Any(row=>row is not AP_INVOICE))throw new InvalidOperationException("The recorded AP source list is unavailable.");
            var rows=bills.Cast<AP_INVOICE>().ToList();
            APInvoiceOwnership.RequireRows(company,rows);
            if(vendorBaId is not null)APInvoiceOwnership.RequireVendorRows(company,vendorBaId,rows);
            return rows;
        }

        /// <summary>
        /// Get AP Aging Report
        /// Groups unpaid/partially paid bills by age categories
        /// </summary>
        public Task<List<(string Category, decimal Total, List<AP_INVOICE> Bills)>> GetAPAgingAsync(CompanyScope company,DateTime? asOfDate = null)
        {
            APInvoiceOwnership.RequireScope(company);
            throw new InvalidOperationException("Reviewed currency-bound AP aging and exact contributing records are not implemented.");
        }

        public async Task<APInvoiceSourceReview?> GetSourceReviewAsync(CompanyScope company,string invoiceId,string actor)
        {
            var invoice=await GetBillByIdAsync(company,invoiceId);
            if(invoice is null)return null;
            var lines=await GetLinesForInvoiceAsync(company,invoiceId);
            return APInvoiceSourceReview.Create(company,actor,invoice,lines);
        }
        public async Task<List<AP_INVOICE_LINE>> GetLinesForInvoiceAsync(CompanyScope company,string invoiceId)
        {
            _=await GetBillByIdAsync(company,invoiceId) ?? throw RefusalException.NotFound("The owning AP invoice is unavailable.");
            var repo=await GetRepoAsync<AP_INVOICE_LINE>("AP_INVOICE_LINE");
            var raw=await repo.GetAsync(new List<AppFilter>{new(){FieldName="AP_INVOICE_ID",Operator="=",FilterValue=invoiceId}})
                ?? throw new InvalidOperationException("The recorded AP lines are unavailable.");
            var rows=raw.ToArray();if(rows.Any(row=>row is not AP_INVOICE_LINE))throw new InvalidOperationException("The AP line source is incompatible.");
            var lines=rows.Cast<AP_INVOICE_LINE>().ToList();APInvoiceOwnership.RequireLines(invoiceId,lines);return lines;
        }

        /// <summary>Recorded allocation history; owning AP header and exact invoice-line membership are mandatory.</summary>
        public async Task<List<AP_RECEIPT_ALLOCATION>> GetReceiptAllocationsForLineAsync(CompanyScope company,string invoiceId,string lineId)
        {
            APInvoiceOwnership.RequireScope(company);
            if(string.IsNullOrWhiteSpace(invoiceId) || string.IsNullOrWhiteSpace(lineId))throw new InvalidOperationException("Explicit invoice and line identities are required.");
            var lines=await GetLinesForInvoiceAsync(company,invoiceId);
            if(!lines.Any(line=>line.AP_INVOICE_LINE_ID==lineId))throw new InvalidOperationException("The invoice line is unavailable.");
            var repo=await GetRepoAsync<AP_RECEIPT_ALLOCATION>("AP_RECEIPT_ALLOCATION");
            var raw=await repo.GetAsync(new List<AppFilter>{
                new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},
                new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},
                new(){FieldName="AP_INVOICE_ID",Operator="=",FilterValue=invoiceId},
                new(){FieldName="AP_INVOICE_LINE_ID",Operator="=",FilterValue=lineId}})
                ?? throw new InvalidOperationException("The allocation source is unavailable.");
            var records=raw.ToArray();if(records.Any(row=>row is not AP_RECEIPT_ALLOCATION))throw new InvalidOperationException("The allocation source is incompatible.");
            var rows=records.Cast<AP_RECEIPT_ALLOCATION>().ToList();APReceiptAllocationReview.RequireOwnership(company,rows);APReceiptAllocationReview.RequireRows(invoiceId,lineId,rows);return rows;
        }

        /// <summary>All recorded contributors to an exact receipt. Foreign or missing invoice evidence refuses the whole read.</summary>
        public async Task<APReceiptConsumptionReview> GetReceiptConsumptionAsync(CompanyScope company,string actor,
            APLinePurchaseSourceReview selectedSource,string receiptId,Func<Task> requireAuthority)
        {
            // Validate receipt membership before opening the provider, including the confirmed-empty case.
            _=APReceiptConsumptionReview.Create(company,actor,selectedSource,receiptId,[]);
            await requireAuthority();
            var repo=await GetRepoAsync<AP_RECEIPT_ALLOCATION>("AP_RECEIPT_ALLOCATION");
            // Do not restrict to the selected invoice or active rows: all competing references and history must be checked.
            var raw=await repo.GetAsync(new List<AppFilter>{
                new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},
                new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},
                new(){FieldName="PO_RECEIPT_ID",Operator="=",FilterValue=receiptId}})
                ?? throw new InvalidOperationException("The recorded receipt contributors are unavailable.");
            var records=raw.ToArray();if(records.Any(row=>row is not AP_RECEIPT_ALLOCATION))
                throw new InvalidOperationException("The recorded receipt contributors are incompatible.");
            var rows=records.Cast<AP_RECEIPT_ALLOCATION>().ToArray();
            APReceiptAllocationReview.RequireOwnership(company,rows);
            var contributions=new List<APReceiptAllocationReview>();
            foreach(var group in rows.GroupBy(row=>(row.AP_INVOICE_ID,row.AP_INVOICE_LINE_ID)))
            {
                APReceiptAllocationReview.RequireRows(group.Key.AP_INVOICE_ID,group.Key.AP_INVOICE_LINE_ID,group.ToArray());
                await requireAuthority();
                var invoice=await GetSourceReviewAsync(company,group.Key.AP_INVOICE_ID,actor)
                    ?? throw new InvalidOperationException("An owning invoice for a receipt contributor is unavailable.");
                contributions.Add(APReceiptAllocationReview.Create(company,actor,group.Key.AP_INVOICE_ID,group.Key.AP_INVOICE_LINE_ID,
                    invoice.Revision,new(invoice,group.Key.AP_INVOICE_LINE_ID,selectedSource.Order),group.ToArray()));
            }
            await requireAuthority();
            return APReceiptConsumptionReview.Create(company,actor,selectedSource,receiptId,contributions);
        }

        private async Task<string> GenerateInvoiceNumberAsync()
        {
            try
            {
                return $"BILL-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString().Substring(0, 8)}";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating invoice number");
                throw;
            }
        }

        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName)
        {
            if(_resolveConnection is null)throw new InvalidOperationException("A reviewed request-bound PRODUCTION resolver is required.");
            var connection = await _resolveConnection();
            if (string.IsNullOrWhiteSpace(connection)) throw new InvalidOperationException("A PRODUCTION database binding is required.");
            var source=_editor.GetDataSource(connection) ?? throw new InvalidOperationException("The AP datasource is unavailable.");
            var schema=source.GetEntityStructure(new EntityStructure{EntityName=tableName,DatasourceEntityName=tableName},true);
            var keys=schema?.Fields?.Where(field=>field.IsKey).ToArray();
            if(keys is not {Length:1} || !string.Equals(keys[0].FieldName,tableName+"_ID",StringComparison.OrdinalIgnoreCase) || keys[0].AllowDBNull)
                throw new InvalidOperationException("The AP invoice identity key is unconfirmed.");
            var required=tableName=="AP_RECEIPT_ALLOCATION" ? new[]{"OWNER_TENANT_ID","OWNER_COMPANY_BA_ID","AP_INVOICE_ID","AP_INVOICE_LINE_ID","PURCHASE_ORDER_ID","PO_LINE_ITEM_ID","PO_RECEIPT_ID","ALLOCATED_QUANTITY","QUANTITY_UOM"} : tableName=="AP_INVOICE" ? new[]{"OWNER_TENANT_ID","OWNER_COMPANY_BA_ID","CURRENCY_CODE"} :
                new[]{"AP_INVOICE_ID","LINE_NUMBER","ITEM_DESCRIPTION","QUANTITY","QUANTITY_UOM","UNIT_PRICE","LINE_TOTAL","CURRENCY_CODE","PURCHASE_ORDER_ID","PO_LINE_ITEM_ID"};
            foreach(var column in required)
                if(schema?.Fields is null || schema.Fields.Count(field=>string.Equals(field.FieldName,column,StringComparison.OrdinalIgnoreCase))!=1)
                    throw new InvalidOperationException("The AP ownership or currency schema is unavailable. Review the PRODUCTION Beep migration.");
            // Use strongly typed class directly
            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(T), connection, tableName);
        }

        private string GetAccountId(string key, string fallback)
        {
            return _accountMapping?.GetAccountId(key) ?? fallback;
        }
    }
}
