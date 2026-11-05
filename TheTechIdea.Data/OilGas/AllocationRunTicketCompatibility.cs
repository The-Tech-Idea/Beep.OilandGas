using System;
using System.Collections.Generic;
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

        [System.Text.Json.Serialization.JsonIgnore]
        public string? Method
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
        public string? EntityId
        {
            get => ENTITY_ID;
            set => ENTITY_ID = value;
        }

        public string? EntityName
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
    public partial class RUN_TICKET
    {
        public string? RunTicketNumber
        {
            get => RUN_TICKET_NUMBER;
            set => RUN_TICKET_NUMBER = value;
        }

        public DateTime? TicketDateTime
        {
            get => TICKET_DATE_TIME;
            set => TICKET_DATE_TIME = value;
        }

        public string? LeaseId
        {
            get => LEASE_ID;
            set => LEASE_ID = value;
        }

        public string? WellId
        {
            get => WELL_ID;
            set => WELL_ID = value;
        }

        public string? TankBatteryId
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

        [System.Text.Json.Serialization.JsonIgnore]
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

        public string? DispositionType
        {
            get => DISPOSITION_TYPE;
            set => DISPOSITION_TYPE = value;
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public string? Purchaser
        {
            get => PURCHASER;
            set => PURCHASER = value;
        }
    }
}
