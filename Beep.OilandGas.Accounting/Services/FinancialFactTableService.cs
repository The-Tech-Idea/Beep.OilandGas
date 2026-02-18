using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Accounting.Interfaces;
using Beep.OilandGas.Accounting.Models; // Assuming this namespace exists for models
using Beep.PPDM39.Models;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;

namespace Beep.OilandGas.Accounting.Services
{
    public class FinancialFactTableService
    {
        private readonly IDMEEditor _editor;
        private readonly IAppManager _appManager;
        private readonly ILogger _logger;
        private readonly IRuleDefaults _defaults;

        public string ConnectionName { get; set; } = "OilandGasDB";

        public FinancialFactTableService(IDMEEditor editor, IAppManager appManager, ILogger logger, IRuleDefaults defaults)
        {
            _editor = editor;
            _appManager = appManager;
            _logger = logger;
            _defaults = defaults;
        }

        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName, string? connectionName)
        {
            var cn = connectionName ?? ConnectionName;
            var ds = _editor.GetDataSource(cn);
            if (ds == null) throw new InvalidOperationException($"Connection {cn} not found.");
            
            var CommonColumnHandler = new PPDMCommonColumnHandler(_defaults);
            var metadata = await ds.GetEntitesListAsync(); // Ensure metadata is loaded
            
            return new PPDMGenericRepository(
                _editor, CommonColumnHandler, _defaults, ds,
                 typeof(T), cn, tableName);
        }

        public async Task<List<FactJournalLine>> ExportToStarSchemaAsync(
            DateTime startDate, 
            DateTime endDate, 
            string? connectionName = null)
        {
            var cn = connectionName ?? ConnectionName;
            
            // 1. Fetch Data
            // We need JOURNAL_ENTRY (Header) and JOURNAL_ENTRY_LINE (Details)
            // Ideally we join them, but with repository pattern we might fetch separately and join in memory for ETL
            // Warning: Large datasets might require pagination or streaming. For now we assume fetch-all fits in memory or we'd use a reader.
            
            var headerRepo = await GetRepoAsync<JOURNAL_ENTRY>("JOURNAL_ENTRY", cn);
            var lineRepo = await GetRepoAsync<JOURNAL_ENTRY_LINE>("JOURNAL_ENTRY_LINE", cn);
            
            var headerFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "JOURNAL_DATE", Operator = ">=", FilterValue = startDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "JOURNAL_DATE", Operator = "<=", FilterValue = endDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "POSTED_IND", Operator = "=", FilterValue = "Y" } // Only posted entries
            };

            var headers = (await headerRepo.GetAsync(headerFilters)).Cast<JOURNAL_ENTRY>().ToList();
            if (!headers.Any()) return new List<FactJournalLine>();

            // Optimizing: Fetch all lines for these headers. 
            // In a real ETL, we'd probably do this via SQL Query, but here we sticking to repo pattern.
            // Getting all lines might be heavy. Let's iterate or filter by ID list if possible, or just date range if lines have dates (usually they don't, only headers do).
            // We will fetch lines by Header IDs. 
            
            var factList = new List<FactJournalLine>();

            foreach (var header in headers)
            {
                var lineFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "JOURNAL_ENTRY_ID", Operator = "=", FilterValue = header.JOURNAL_ENTRY_ID }
                };
                var lines = (await lineRepo.GetAsync(lineFilters)).Cast<JOURNAL_ENTRY_LINE>();

                foreach (var line in lines)
                {
                    factList.Add(TransformToFact(header, line));
                }
            }

            return factList;
        }

        private FactJournalLine TransformToFact(JOURNAL_ENTRY header, JOURNAL_ENTRY_LINE line)
        {
            // Resolve Dimension Keys (In a real Data Warehouse, these would be Surrogate Keys)
            // Here we use Natural Keys or DTOs
            
            decimal amount = 0;
            string direction = "";
            if ((line.DEBIT_AMOUNT ?? 0) > 0)
            {
                amount = line.DEBIT_AMOUNT.Value;
                direction = "DEBIT";
            }
            else
            {
                amount = line.CREDIT_AMOUNT ?? 0;
                direction = "CREDIT";
            }

            return new FactJournalLine
            {
                FactId = Guid.NewGuid().ToString(),
                DateKey = header.JOURNAL_DATE.HasValue ? Convert.ToInt32(header.JOURNAL_DATE.Value.ToString("yyyyMMdd")) : 0,
                TransactionDate = header.JOURNAL_DATE ?? DateTime.MinValue,
                JournalEntryId = header.JOURNAL_ENTRY_ID,
                LineId = line.JOURNAL_ENTRY_LINE_ID,
                
                // Dimensions
                AccountCode = line.GL_ACCOUNT_ID, // Assuming ID is the code or we map it
                CostCenterCode = line.COST_CENTER_ID,
                CurrencyCode = line.CURRENCY_CODE,
                SourceSystem = header.SOURCE_SYSTEM,
                TransactionType = header.JOURNAL_TYPE,
                
                // Measures
                Amount = amount,
                Direction = direction, // "DEBIT" or "CREDIT"
                SignedAmount = direction == "DEBIT" ? amount : -amount, // Useful for aggregation
                ExchangeRate = line.EXCHANGE_RATE ?? 1.0m,
                ReportingAmount = (line.SHADOW_DEBIT_AMOUNT ?? 0) - (line.SHADOW_CREDIT_AMOUNT ?? 0) // Or calculated
            };
        }
    }

    // --- DTOs for Star Schema ---

    public class FactJournalLine
    {
        public string FactId { get; set; }
        public int DateKey { get; set; } // yyyyMMdd
        public DateTime TransactionDate { get; set; }
        public string JournalEntryId { get; set; }
        public string LineId { get; set; }
        
        // Dimensions
        public string AccountCode { get; set; }
        public string CostCenterCode { get; set; }
        public string CurrencyCode { get; set; }
        public string SourceSystem { get; set; }
        public string TransactionType { get; set; }
        
        // Measures
        public decimal Amount { get; set; }
        public string Direction { get; set; }
        public decimal SignedAmount { get; set; } // + for Debit, - for Credit
        public decimal ExchangeRate { get; set; }
        public decimal ReportingAmount { get; set; } // Converted to Functional Currency
    }

    public class DimAccount
    {
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public string AccountType { get; set; } // Asset, Liability
        public string AccountGroup { get; set; } // Current, Non-Current
    }

    public class DimCostCenter
    {
        public string CostCenterCode { get; set; }
        public string CostCenterName { get; set; }
        public string Department { get; set; }
    }
}
