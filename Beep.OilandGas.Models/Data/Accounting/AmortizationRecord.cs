namespace Beep.OilandGas.Models.Data.Accounting
{
    public class AmortizationRecord : ModelEntityBase
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

    public class CreateAmortizationRecordRequest : ModelEntityBase
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

    public class AmortizationRecordResponse : AmortizationRecord
    {
    }
}





