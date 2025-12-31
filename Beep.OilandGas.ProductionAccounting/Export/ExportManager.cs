
using System.IO;
using System.Text;
using Beep.OilandGas.ProductionAccounting.Reporting;
using Beep.OilandGas.ProductionAccounting.Production;
using Beep.OilandGas.ProductionAccounting.Accounting;
using Beep.OilandGas.ProductionAccounting.Royalty;

namespace Beep.OilandGas.ProductionAccounting.Export
{
    /// <summary>
    /// Export format enumeration.
    /// </summary>
    public enum ExportFormat
    {
        /// <summary>
        /// Comma-separated values.
        /// </summary>
        CSV,

        /// <summary>
        /// Excel format.
        /// </summary>
        Excel,

        /// <summary>
        /// JSON format.
        /// </summary>
        JSON,

        /// <summary>
        /// XML format.
        /// </summary>
        XML,

        /// <summary>
        /// PDF format.
        /// </summary>
        PDF
    }

    /// <summary>
    /// Manages data export functionality.
    /// </summary>
    public class ExportManager
    {
        /// <summary>
        /// Exports run tickets to CSV.
        /// </summary>
        public void ExportRunTicketsToCsv(List<RunTicket> tickets, string filePath)
        {
            if (tickets == null || tickets.Count == 0)
                throw new ArgumentException("Tickets list cannot be null or empty.", nameof(tickets));

            var csv = new StringBuilder();
            csv.AppendLine("RunTicketNumber,Date,LeaseId,WellId,GrossVolume,BSWVolume,NetVolume,BSWPercentage,PricePerBarrel,TotalValue,Purchaser");

            foreach (var ticket in tickets)
            {
                csv.AppendLine($"{ticket.RunTicketNumber}," +
                    $"{ticket.TicketDateTime:yyyy-MM-dd}," +
                    $"{ticket.LeaseId}," +
                    $"{ticket.WellId ?? ""}," +
                    $"{ticket.GrossVolume}," +
                    $"{ticket.BSWVolume}," +
                    $"{ticket.NetVolume}," +
                    $"{ticket.BSWPercentage}," +
                    $"{ticket.PricePerBarrel?.ToString() ?? ""}," +
                    $"{ticket.TotalValue?.ToString() ?? ""}," +
                    $"{ticket.Purchaser}");
            }

            File.WriteAllText(filePath, csv.ToString());
        }

        /// <summary>
        /// Exports sales transactions to CSV.
        /// </summary>
        public void ExportSalesTransactionsToCsv(List<SalesTransaction> transactions, string filePath)
        {
            if (transactions == null || transactions.Count == 0)
                throw new ArgumentException("Transactions list cannot be null or empty.", nameof(transactions));

            var csv = new StringBuilder();
            csv.AppendLine("TransactionId,Date,Purchaser,NetVolume,PricePerBarrel,TotalValue,TotalCosts,TotalTaxes,NetRevenue");

            foreach (var transaction in transactions)
            {
                csv.AppendLine($"{transaction.TransactionId}," +
                    $"{transaction.TransactionDate:yyyy-MM-dd}," +
                    $"{transaction.Purchaser}," +
                    $"{transaction.NetVolume}," +
                    $"{transaction.PricePerBarrel}," +
                    $"{transaction.TotalValue}," +
                    $"{transaction.Costs.TotalCosts}," +
                    $"{transaction.Taxes.Sum(t => t.Amount)}," +
                    $"{transaction.NetRevenue}");
            }

            File.WriteAllText(filePath, csv.ToString());
        }

        /// <summary>
        /// Exports royalty payments to CSV.
        /// </summary>
        public void ExportRoyaltyPaymentsToCsv(List<RoyaltyPayment> payments, string filePath)
        {
            if (payments == null || payments.Count == 0)
                throw new ArgumentException("Payments list cannot be null or empty.", nameof(payments));

            var csv = new StringBuilder();
            csv.AppendLine("PaymentId,RoyaltyOwnerId,PropertyOrLeaseId,PeriodStart,PeriodEnd,RoyaltyAmount,PaymentDate,Status,NetPaymentAmount");

            foreach (var payment in payments)
            {
                csv.AppendLine($"{payment.PaymentId}," +
                    $"{payment.RoyaltyOwnerId}," +
                    $"{payment.PropertyOrLeaseId}," +
                    $"{payment.PaymentPeriodStart:yyyy-MM-dd}," +
                    $"{payment.PaymentPeriodEnd:yyyy-MM-dd}," +
                    $"{payment.RoyaltyAmount}," +
                    $"{payment.PaymentDate:yyyy-MM-dd}," +
                    $"{payment.Status}," +
                    $"{payment.NetPaymentAmount}");
            }

            File.WriteAllText(filePath, csv.ToString());
        }

        /// <summary>
        /// Exports report to JSON.
        /// </summary>
        public void ExportReportToJson(Report report, string filePath)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));

            var json = new StringBuilder();
            json.AppendLine("{");
            json.AppendLine($"  \"ReportId\": \"{report.ReportId}\",");
            json.AppendLine($"  \"ReportType\": \"{report.ReportType}\",");
            json.AppendLine($"  \"PeriodStart\": \"{report.ReportPeriodStart:yyyy-MM-dd}\",");
            json.AppendLine($"  \"PeriodEnd\": \"{report.ReportPeriodEnd:yyyy-MM-dd}\",");
            json.AppendLine($"  \"GenerationDate\": \"{report.GenerationDate:yyyy-MM-dd HH:mm:ss}\"");
            json.AppendLine("}");

            File.WriteAllText(filePath, json.ToString());
        }
    }
}
