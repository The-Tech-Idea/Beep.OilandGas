namespace Beep.OilandGas.Models.DTOs.Accounting
{
    public class AmortizationRecordDto
    {
        public string AmortizationRecordId { get; set; }
        public string PropertyId { get; set; }
        public string CostCenterId { get; set; }
        public DateTime? PeriodStartDate { get; set; }
        public DateTime? PeriodEndDate { get; set; }
        public decimal? NetCapitalizedCosts { get; set; }
        public decimal? TotalReservesBOE { get; set; }
        public decimal? ProductionBOE { get; set; }
        public decimal? AmortizationAmount { get; set; }
        public string AccountingMethod { get; set; }
    }

    public class CreateAmortizationRecordRequest
    {
        public string PropertyId { get; set; }
        public string CostCenterId { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public decimal NetCapitalizedCosts { get; set; }
        public decimal TotalReservesBOE { get; set; }
        public decimal ProductionBOE { get; set; }
        public string AccountingMethod { get; set; }
    }

    public class AmortizationRecordResponse : AmortizationRecordDto
    {
    }
}

