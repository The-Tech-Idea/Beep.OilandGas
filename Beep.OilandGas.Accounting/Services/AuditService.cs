using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Accounting.Interfaces;
using Beep.OilandGas.Accounting.Models; // Assuming models exist
using Beep.PPDM39.Models;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;

namespace Beep.OilandGas.Accounting.Services
{
    public class AuditService
    {
        private readonly IDMEEditor _editor;
        private readonly IAppManager _appManager;
        private readonly ILogger _logger;
        private readonly IRuleDefaults _defaults;
        private readonly AnomalyDetectionService _anomalyService;

        public string ConnectionName { get; set; } = "OilandGasDB";

        public AuditService(
            IDMEEditor editor, 
            IAppManager appManager, 
            ILogger logger, 
            IRuleDefaults defaults,
            AnomalyDetectionService anomalyService)
        {
            _editor = editor;
            _appManager = appManager;
            _logger = logger;
            _defaults = defaults;
            _anomalyService = anomalyService;
        }

        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName, string? connectionName)
        {
            var cn = connectionName ?? ConnectionName;
            var ds = _editor.GetDataSource(cn);
            if (ds == null) throw new InvalidOperationException($"Connection {cn} not found.");
            
            var CommonColumnHandler = new PPDMCommonColumnHandler(_defaults);
            var metadata = await ds.GetEntitesListAsync(); 
            
            return new PPDMGenericRepository(
                _editor, CommonColumnHandler, _defaults, ds,
                 typeof(T), cn, tableName);
        }

        public async Task<List<AuditLogEntry>> GetUserActivityAsync(
            string userId, 
            DateTime startDate, 
            DateTime endDate, 
            string? connectionName = null)
        {
            var cn = connectionName ?? ConnectionName;
            var logs = new List<AuditLogEntry>();

            // 1. Check Journal Entries
            var jeRepo = await GetRepoAsync<JOURNAL_ENTRY>("JOURNAL_ENTRY", cn);
            var jeFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ROW_CREATED_DATE", Operator = ">=", FilterValue = startDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ROW_CREATED_DATE", Operator = "<=", FilterValue = endDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ROW_CREATED_BY", Operator = "=", FilterValue = userId }
            };
            
            // Note: Filter logic in AppFilter might need proper date handling, assuming string format works or repo handles it.
            var jes = (await jeRepo.GetAsync(jeFilters)).Cast<JOURNAL_ENTRY>();
            
            foreach (var je in jes)
            {
                logs.Add(new AuditLogEntry
                {
                    Timestamp = je.ROW_CREATED_DATE ?? DateTime.MinValue,
                    UserId = userId,
                    Action = "Created",
                    EntityName = "JOURNAL_ENTRY",
                    RecordId = je.JOURNAL_ENTRY_ID,
                    Details = $"Posted Journal: {je.JOURNAL_DESCRIPTION}",
                    IsAnomaly = false
                });
            }

            // 2. Check Tax Transactions
            var taxRepo = await GetRepoAsync<TAX_TRANSACTION>("TAX_TRANSACTION", cn);
            var taxFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ROW_CREATED_DATE", Operator = ">=", FilterValue = startDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ROW_CREATED_DATE", Operator = "<=", FilterValue = endDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ROW_CREATED_BY", Operator = "=", FilterValue = userId }
            };

            var taxes = (await taxRepo.GetAsync(taxFilters)).Cast<TAX_TRANSACTION>();
             foreach (var tx in taxes)
            {
                logs.Add(new AuditLogEntry
                {
                    Timestamp = tx.ROW_CREATED_DATE ?? DateTime.MinValue,
                    UserId = userId,
                    Action = "Created",
                    EntityName = "TAX_TRANSACTION",
                    RecordId = tx.TAX_TRANSACTION_ID,
                    Details = $"Tax Provision: {tx.TAX_TYPE} - {tx.TAX_AMOUNT}",
                    IsAnomaly = false
                });
            }

            return logs.OrderByDescending(l => l.Timestamp).ToList();
        }

        public async Task<List<AuditLogEntry>> GetAnomalyReportAsync(
            DateTime startDate, 
            DateTime endDate, 
            string? connectionName = null)
        {
            var logs = new List<AuditLogEntry>();

            // 1. Unusual Postings
            var unusuals = await _anomalyService.DetectUnusualPostingsAsync(startDate, endDate, 1000m, connectionName);
            foreach (var u in unusuals)
            {
                logs.Add(new AuditLogEntry
                {
                    Timestamp = u.JournalDate,
                    UserId = "SYSTEM_AUDIT",
                    Action = "Anomaly Detected",
                    EntityName = "JOURNAL_ENTRY",
                    RecordId = u.JournalEntryId,
                    Details = $"Unusual Activity: {u.Reason}",
                    IsAnomaly = true,
                    AnomalyScore = 0.8 // High confidence it's unusual
                });
            }

            // 2. Benford Analysis (Sample on a key account, e.g., '1000' Cash)
            // In a real viewer, user would select account
            var benford = await _anomalyService.BenfordLawAnalysisAsync("1000", startDate, endDate, connectionName);
            foreach (var b in benford.Where(x => x.IsAnomaly))
            {
                logs.Add(new AuditLogEntry
                {
                    Timestamp = DateTime.UtcNow, // Report time
                    UserId = "SYSTEM_AUDIT",
                    Action = "Anomaly Detected",
                    EntityName = "GL_ACCOUNT",
                    RecordId = "1000",
                    Details = $"Benford Violation: Digit {b.Digit} freq {b.ObservedFrequency:P1} vs expected {b.ExpectedFrequency:P1}",
                    IsAnomaly = true,
                    AnomalyScore = Math.Abs(b.ObservedFrequency - b.ExpectedFrequency) * 10 
                });
            }

            return logs;
        }
    }
}
