using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.ProductionAccounting;

namespace Beep.OilandGas.ProductionAccounting.Reporting
{
    /// <summary>
    /// Manages report generation and storage.
    /// Uses database access via IDataSource instead of in-memory dictionaries.
    /// </summary>
    public class ReportManager
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<ReportManager>? _logger;
        private readonly string _connectionName;
        private const string REPORT_TABLE = "REPORT";

        public ReportManager(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<ReportManager>? logger = null,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
            _connectionName = connectionName ?? "PPDM39";
        }

        /// <summary>
        /// Generates and stores an operational report.
        /// </summary>
        public OperationalReport GenerateOperationalReport(
            DateTime periodStart,
            DateTime periodEnd,
            List<Production.RunTicket> runTickets,
            List<Inventory.CrudeOilInventory> inventories,
            List<Allocation.AllocationResult> allocations,
            List<Measurement.MeasurementRecord> measurements,
            List<SalesTransaction> transactions)
        {
            var report = ReportGenerator.GenerateOperationalReport(
                periodStart,
                periodEnd,
                runTickets,
                inventories,
                allocations,
                measurements,
                transactions);

            // Save to database (optional - reports may be generated on-demand)
            var connName = _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource != null)
            {
                var reportEntity = new OPERATIONAL_REPORT
                {
                    OPERATIONAL_REPORT_ID = report.ReportId,
                    REPORT_TYPE = report.ReportType.ToString(),
                    REPORT_PERIOD_START = report.ReportPeriodStart,
                    REPORT_PERIOD_END = report.ReportPeriodEnd,
                    GENERATION_DATE = report.GeneratedDate,
                    GENERATED_BY = report.GeneratedBy
                };
                _commonColumnHandler.PrepareForInsert(reportEntity, report.GeneratedBy);
                dataSource.InsertEntity("OPERATIONAL_REPORT", reportEntity);
                _logger?.LogDebug("Saved operational report {ReportId} to database", report.ReportId);
            }

            return report;
        }

        /// <summary>
        /// Generates and stores a lease report.
        /// </summary>
        public async Task<LeaseReport> GenerateLeaseReportAsync(
            string leaseId,
            DateTime periodStart,
            DateTime periodEnd,
            List<Production.RunTicket> runTickets,
            List<SalesTransaction> transactions,
            string? connectionName = null)
        {
            var report = ReportGenerator.GenerateLeaseReport(
                leaseId,
                periodStart,
                periodEnd,
                runTickets,
                transactions);

            // Save to database (optional)
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource != null)
            {
                var reportEntity = new LEASE_REPORT
                {
                    LEASE_REPORT_ID = report.ReportId,
                    REPORT_TYPE = report.ReportType.ToString(),
                    REPORT_PERIOD_START = report.ReportPeriodStart,
                    REPORT_PERIOD_END = report.ReportPeriodEnd,
                    GENERATION_DATE = report.GeneratedDate,
                    GENERATED_BY = report.GeneratedBy,
                    LEASE_ID = leaseId
                };
                _commonColumnHandler.PrepareForInsert(reportEntity, report.GeneratedBy);
                dataSource.InsertEntity("LEASE_REPORT", reportEntity);
                _logger?.LogDebug("Saved lease report {ReportId} to database", report.ReportId);
            }

            return report;
        }

        /// <summary>
        /// Generates and stores a lease report (synchronous wrapper).
        /// </summary>
        public LeaseReport GenerateLeaseReport(
            string leaseId,
            DateTime periodStart,
            DateTime periodEnd,
            List<Production.RunTicket> runTickets,
            List<SalesTransaction> transactions)
        {
            return GenerateLeaseReportAsync(leaseId, periodStart, periodEnd, runTickets, transactions).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Generates and stores a governmental report.
        /// </summary>
        public async Task<GovernmentalReport> GenerateGovernmentalReportAsync(
            string reportingAgency,
            string reportFormat,
            DateTime periodStart,
            DateTime periodEnd,
            List<Production.RunTicket> runTickets,
            List<SalesTransaction> transactions,
            List<Royalty.RoyaltyPayment> royaltyPayments,
            string? connectionName = null)
        {
            var report = ReportGenerator.GenerateGovernmentalReport(
                reportingAgency,
                reportFormat,
                periodStart,
                periodEnd,
                runTickets,
                transactions,
                royaltyPayments);

            // Save to database (optional)
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource != null)
            {
                var reportEntity = new GOVERNMENTAL_REPORT
                {
                    GOVERNMENTAL_REPORT_ID = report.ReportId,
                    REPORT_TYPE = report.ReportType.ToString(),
                    REPORT_PERIOD_START = report.ReportPeriodStart,
                    REPORT_PERIOD_END = report.ReportPeriodEnd,
                    GENERATION_DATE = report.GeneratedDate,
                    GENERATED_BY = report.GeneratedBy,
                    REPORTING_AGENCY = reportingAgency,
                    REPORT_FORMAT = reportFormat
                };
                _commonColumnHandler.PrepareForInsert(reportEntity, report.GeneratedBy);
                dataSource.InsertEntity("GOVERNMENTAL_REPORT", reportEntity);
                _logger?.LogDebug("Saved governmental report {ReportId} to database", report.ReportId);
            }

            return report;
        }

        /// <summary>
        /// Generates and stores a governmental report (synchronous wrapper).
        /// </summary>
        public GovernmentalReport GenerateGovernmentalReport(
            string reportingAgency,
            string reportFormat,
            DateTime periodStart,
            DateTime periodEnd,
            List<Production.RunTicket> runTickets,
            List<SalesTransaction> transactions,
            List<Royalty.RoyaltyPayment> royaltyPayments)
        {
            return GenerateGovernmentalReportAsync(reportingAgency, reportFormat, periodStart, periodEnd, runTickets, transactions, royaltyPayments).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Generates and stores a joint interest statement.
        /// </summary>
        public async Task<JointInterestStatement> GenerateJointInterestStatementAsync(
            string jibId,
            string operatorName,
            DateTime periodStart,
            DateTime periodEnd,
            List<JIBParticipant> participants,
            List<JIBCharge> charges,
            List<JIBCredit> credits,
            string? connectionName = null)
        {
            var statement = ReportGenerator.GenerateJointInterestStatement(
                jibId,
                operatorName,
                periodStart,
                periodEnd,
                participants,
                charges,
                credits);

            // Save to database (optional)
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource != null)
            {
                var reportEntity = new JOINT_INTEREST_STATEMENT
                {
                    JOINT_INTEREST_STATEMENT_ID = statement.ReportId,
                    REPORT_TYPE = statement.ReportType.ToString(),
                    REPORT_PERIOD_START = statement.ReportPeriodStart,
                    REPORT_PERIOD_END = statement.ReportPeriodEnd,
                    GENERATION_DATE = statement.GeneratedDate,
                    GENERATED_BY = statement.GeneratedBy,
                    JIB_ID = jibId,
                    OPERATOR_NAME = operatorName
                };
                _commonColumnHandler.PrepareForInsert(reportEntity, statement.GeneratedBy);
                dataSource.InsertEntity("JOINT_INTEREST_STATEMENT", reportEntity);
                _logger?.LogDebug("Saved joint interest statement {ReportId} to database", statement.ReportId);
            }

            return statement;
        }

        /// <summary>
        /// Generates and stores a joint interest statement (synchronous wrapper).
        /// </summary>
        public JointInterestStatement GenerateJointInterestStatement(
            string jibId,
            string operatorName,
            DateTime periodStart,
            DateTime periodEnd,
            List<JIBParticipant> participants,
            List<JIBCharge> charges,
            List<JIBCredit> credits)
        {
            return GenerateJointInterestStatementAsync(jibId, operatorName, periodStart, periodEnd, participants, charges, credits).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets a report by ID.
        /// </summary>
        public async Task<Report?> GetReportAsync(string reportId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(reportId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                return null; // Reports may not be stored

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "REPORT_ID", Operator = "=", FilterValue = reportId }
            };

            var results = await dataSource.GetEntityAsync(REPORT_TABLE, filters);
            var reportData = results?.FirstOrDefault();
            
            if (reportData == null)
                return null;

            return reportData as Report;
        }

        /// <summary>
        /// Gets a report by ID (synchronous wrapper).
        /// </summary>
        public Report? GetReport(string reportId)
        {
            return GetReportAsync(reportId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets reports by type.
        /// </summary>
        public async Task<IEnumerable<Report>> GetReportsByTypeAsync(ReportType reportType, string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                return Enumerable.Empty<Report>();

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "REPORT_TYPE", Operator = "=", FilterValue = reportType.ToString() }
            };

            // Query all report tables and combine results
            var allReports = new List<Entity>();
            var reportTables = new[] { "OPERATIONAL_REPORT", "LEASE_REPORT", "GOVERNMENTAL_REPORT", "JOINT_INTEREST_STATEMENT" };
            foreach (var tableName in reportTables)
            {
                var results = await dataSource.GetEntityAsync(tableName, filters);
                if (results != null)
                    allReports.AddRange(results);
            }
            
            if (!allReports.Any())
                return Enumerable.Empty<Report>();

            // Note: Report is abstract, so we can't directly cast Entity to Report
            // This method may need to be refactored to return Entity types or DTOs
            return Enumerable.Empty<Report>();
        }

        /// <summary>
        /// Gets reports by type (synchronous wrapper).
        /// </summary>
        public IEnumerable<Report> GetReportsByType(ReportType reportType)
        {
            return GetReportsByTypeAsync(reportType).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets reports by date range.
        /// </summary>
        public async Task<IEnumerable<Report>> GetReportsByDateRangeAsync(DateTime startDate, DateTime endDate, string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                return Enumerable.Empty<Report>();

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "REPORT_PERIOD_START", Operator = ">=", FilterValue = startDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "REPORT_PERIOD_END", Operator = "<=", FilterValue = endDate.ToString("yyyy-MM-dd") }
            };

            var results = await dataSource.GetEntityAsync(REPORT_TABLE, filters);
            if (results == null || !results.Any())
                return Enumerable.Empty<Report>();

            return results.Cast<Report>()
                .Where(r => r != null && r.ReportPeriodStart >= startDate && r.ReportPeriodEnd <= endDate)!;
        }

        /// <summary>
        /// Gets reports by date range (synchronous wrapper).
        /// </summary>
        public IEnumerable<Report> GetReportsByDateRange(DateTime startDate, DateTime endDate)
        {
            return GetReportsByDateRangeAsync(startDate, endDate).GetAwaiter().GetResult();
        }

    }
}
