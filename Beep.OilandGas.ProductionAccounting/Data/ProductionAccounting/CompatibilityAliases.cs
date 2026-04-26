using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Trading;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class ALLOCATION_RESULT
    {
        public string AllocationId
        {
            get => ALLOCATION_ID ?? ALLOCATION_RESULT_ID;
            set
            {
                ALLOCATION_ID = value;
                ALLOCATION_RESULT_ID = value;
            }
        }

        public DateTime? AllocationDate
        {
            get => ALLOCATION_DATE;
            set => ALLOCATION_DATE = value;
        }

        public string Method
        {
            get => !string.IsNullOrWhiteSpace(ALLOCATION_METHOD) ? ALLOCATION_METHOD : METHOD.ToString();
            set => ALLOCATION_METHOD = value;
        }

        public decimal? TotalVolume
        {
            get => TOTAL_VOLUME;
            set => TOTAL_VOLUME = value;
        }

        public decimal? AllocatedVolume
        {
            get => ALLOCATED_VOLUME;
            set => ALLOCATED_VOLUME = value;
        }

        public decimal? AllocationVariance
        {
            get => ALLOCATION_VARIANCE;
            set => ALLOCATION_VARIANCE = value;
        }

        public List<ALLOCATION_DETAIL> Details { get; set; } = new();
    }

    public partial class ALLOCATION_DETAIL
    {
        public string EntityId
        {
            get => ENTITY_ID;
            set => ENTITY_ID = value;
        }

        public string EntityName
        {
            get => ENTITY_NAME;
            set => ENTITY_NAME = value;
        }

        public decimal? AllocatedVolume
        {
            get => ALLOCATED_VOLUME;
            set => ALLOCATED_VOLUME = value;
        }

        public decimal? AllocationPercentage
        {
            get => ALLOCATION_PERCENTAGE;
            set => ALLOCATION_PERCENTAGE = value;
        }

        public decimal? WorkingInterest { get; set; }

        public decimal? NetRevenueInterest { get; set; }
    }

    public partial class MEASUREMENT_RECORD
    {
        public string MeasurementId
        {
            get => MEASUREMENT_ID;
            set => MEASUREMENT_ID = value;
        }

        public DateTime? MeasurementDateTime
        {
            get => MEASUREMENT_DATETIME ?? MEASUREMENT_DATE_TIME;
            set
            {
                MEASUREMENT_DATETIME = value;
                MEASUREMENT_DATE_TIME = value ?? default;
            }
        }

        public MeasurementMethod Method
        {
            get => METHOD;
            set => METHOD = value;
        }

        public MeasurementStandard Standard
        {
            get => STANDARD;
            set => STANDARD = value;
        }

        public decimal? GrossVolume
        {
            get => GROSS_VOLUME;
            set => GROSS_VOLUME = value;
        }

        public decimal? Temperature
        {
            get => TEMPERATURE;
            set => TEMPERATURE = value ?? 0m;
        }

        public decimal? ApiGravity
        {
            get => API_GRAVITY;
            set => API_GRAVITY = value ?? 0m;
        }
    }

    public partial class RUN_TICKET
    {
        public string RunTicketNumber
        {
            get => RUN_TICKET_NUMBER;
            set => RUN_TICKET_NUMBER = value;
        }

        public DateTime? TicketDateTime
        {
            get => TICKET_DATE_TIME;
            set => TICKET_DATE_TIME = value;
        }

        public string LeaseId
        {
            get => LEASE_ID;
            set => LEASE_ID = value;
        }

        public string WellId
        {
            get => WELL_ID;
            set => WELL_ID = value;
        }

        public string TankBatteryId
        {
            get => TANK_BATTERY_ID;
            set => TANK_BATTERY_ID = value;
        }

        public decimal? GrossVolume
        {
            get => GROSS_VOLUME;
            set => GROSS_VOLUME = value;
        }

        public decimal? BSWVolume
        {
            get => BSW_VOLUME;
            set => BSW_VOLUME = value;
        }

        public decimal? BSWPercentage
        {
            get => BSW_PERCENTAGE;
            set => BSW_PERCENTAGE = value;
        }

        public decimal? NetVolume
        {
            get => NET_VOLUME;
            set => NET_VOLUME = value;
        }

        public decimal? Temperature
        {
            get => TEMPERATURE;
            set => TEMPERATURE = value ?? 0m;
        }

        public decimal? ApiGravity
        {
            get => API_GRAVITY;
            set => API_GRAVITY = value ?? 0m;
        }

        public string DispositionType
        {
            get => DISPOSITION_TYPE;
            set => DISPOSITION_TYPE = value;
        }

        public string Purchaser
        {
            get => PURCHASER;
            set => PURCHASER = value;
        }
    }

    public partial class OWNER_INFORMATION
    {
        public string OwnerId
        {
            get => OWNER_ID;
            set => OWNER_ID = value;
        }

        public string OwnerName
        {
            get => OWNER_NAME;
            set => OWNER_NAME = value;
        }

        public string TaxId
        {
            get => TAX_ID;
            set => TAX_ID = value;
        }
    }

    public partial class ROYALTY_PAYMENT
    {
        public string PaymentId
        {
            get => PAYMENT_ID;
            set => PAYMENT_ID = value;
        }

        public string RoyaltyOwnerId
        {
            get => ROYALTY_OWNER_ID;
            set => ROYALTY_OWNER_ID = value;
        }

        public string OwnerName
        {
            get => OWNER_NAME;
            set => OWNER_NAME = value;
        }

        public DateTime? PaymentDate
        {
            get => PAYMENT_DATE;
            set => PAYMENT_DATE = value;
        }

        public decimal RoyaltyAmount
        {
            get => ROYALTY_AMOUNT;
            set => ROYALTY_AMOUNT = value;
        }

        public decimal NetPayment
        {
            get => NET_PAYMENT;
            set => NET_PAYMENT = value;
        }

        public string Status
        {
            get => STATUS;
            set => STATUS = value;
        }
    }

    public partial class EXCHANGE_CONTRACT
    {
        public string ContractId
        {
            get => CONTRACT_ID;
            set => CONTRACT_ID = value;
        }

        public string ContractName
        {
            get => CONTRACT_NAME;
            set => CONTRACT_NAME = value;
        }

        public ExchangeContractType ContractType
        {
            get
            {
                return Enum.TryParse<ExchangeContractType>(CONTRACT_TYPE, true, out var parsed)
                    ? parsed
                    : ExchangeContractType.PhysicalExchange;
            }
            set => CONTRACT_TYPE = value.ToString();
        }

        public DateTime EffectiveDate
        {
            get => EFFECTIVE_DATE ?? DateTime.UtcNow;
            set => EFFECTIVE_DATE = value;
        }

        public DateTime? ExpirationDate
        {
            get => EXPIRY_DATE ?? EXPIRATION_DATE;
            set
            {
                EXPIRY_DATE = value;
                EXPIRATION_DATE = value;
            }
        }

        public new DateTime? EFFECTIVE_DATE { get; set; }

        public new DateTime? EXPIRY_DATE { get; set; }
    }

    public partial class RUN_TICKET_VALUATION
    {
        public string ValuationId
        {
            get => VALUATION_ID;
            set => VALUATION_ID = value;
        }

        public string RunTicketNumber
        {
            get => RUN_TICKET_NUMBER;
            set => RUN_TICKET_NUMBER = value;
        }

        public DateTime? ValuationDate
        {
            get => VALUATION_DATE;
            set => VALUATION_DATE = value;
        }

        public decimal BasePrice
        {
            get => BASE_PRICE;
            set => BASE_PRICE = value;
        }

        public decimal TotalAdjustments
        {
            get => TOTAL_ADJUSTMENTS;
            set => TOTAL_ADJUSTMENTS = value;
        }

        public decimal AdjustedPrice
        {
            get => ADJUSTED_PRICE;
            set => ADJUSTED_PRICE = value;
        }

        public decimal NetVolume
        {
            get => NET_VOLUME;
            set => NET_VOLUME = value;
        }

        public decimal TotalValue
        {
            get => TOTAL_VALUE;
            set => TOTAL_VALUE = value;
        }
    }

    public partial class OPERATIONAL_REPORT
    {
        public string ReportId
        {
            get => REPORT_ID;
            set => REPORT_ID = value;
        }

        public DateTime? ReportPeriodStart
        {
            get => REPORT_PERIOD_START;
            set => REPORT_PERIOD_START = value;
        }

        public DateTime? ReportPeriodEnd
        {
            get => REPORT_PERIOD_END;
            set => REPORT_PERIOD_END = value;
        }

        public DateTime? GeneratedDate
        {
            get => GENERATION_DATE;
            set => GENERATION_DATE = value;
        }

        public DateTime? GenerationDate
        {
            get => GENERATION_DATE;
            set => GENERATION_DATE = value;
        }
    }

    public partial class LEASE_REPORT
    {
        public string ReportId
        {
            get => REPORT_ID;
            set => REPORT_ID = value;
        }

        public string LeaseId
        {
            get => LEASE_ID;
            set => LEASE_ID = value;
        }

        public string LeaseName
        {
            get => LEASE_NAME;
            set => LEASE_NAME = value;
        }

        public DateTime? ReportPeriodStart
        {
            get => REPORT_PERIOD_START;
            set => REPORT_PERIOD_START = value;
        }

        public DateTime? ReportPeriodEnd
        {
            get => REPORT_PERIOD_END;
            set => REPORT_PERIOD_END = value;
        }

        public DateTime? GeneratedDate
        {
            get => GENERATION_DATE;
            set => GENERATION_DATE = value;
        }

        public DateTime? GenerationDate
        {
            get => GENERATION_DATE;
            set => GENERATION_DATE = value;
        }
    }

    public partial class ROYALTY_CALCULATION
    {
        public string? FIELD_ID { get; set; }

        public string? POOL_ID { get; set; }

        public decimal? GROSS_OIL_VOLUME { get; set; }

        public decimal? GROSS_GAS_VOLUME { get; set; }

        public decimal? OIL_ROYALTY_RATE { get; set; }

        public decimal? GAS_ROYALTY_RATE { get; set; }

        public decimal? OIL_PRICE { get; set; }

        public decimal? GAS_PRICE { get; set; }

        public DateTime? CREATE_DATE { get; set; }

        public string? CREATE_USER { get; set; }
    }
}