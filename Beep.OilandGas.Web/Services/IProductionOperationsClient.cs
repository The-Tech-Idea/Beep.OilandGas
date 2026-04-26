using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Web.Services;

/// <summary>
/// Typed client for production operations domain (well monitoring, equipment, facility ops).
/// </summary>
public interface IProductionOperationsClient
{
    Task<List<WellProductionRecord>> GetWellProductionAsync(string wellUwi, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<WellUptimeSummary> GetWellUptimeAsync(string wellUwi, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<List<EquipmentMaintenanceRecord>> GetEquipmentMaintenanceAsync(string equipmentId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<List<FacilityProductionRecord>> GetFacilityProductionAsync(string facilityId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}

public class WellProductionRecord
{
    public string WellUwi { get; set; } = string.Empty;
    public DateTime ProductionDate { get; set; }
    public decimal OilVolume { get; set; }
    public decimal GasVolume { get; set; }
    public decimal WaterVolume { get; set; }
    public decimal ChokeSize { get; set; }
    public decimal TubingPressure { get; set; }
    public decimal CasingPressure { get; set; }
}

public class WellUptimeSummary
{
    public string WellUwi { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal UptimePercentage { get; set; }
    public int TotalDowntimeHours { get; set; }
    public List<DowntimeEvent> DowntimeEvents { get; set; } = new();
}

public class DowntimeEvent
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int DurationHours { get; set; }
}

public class EquipmentMaintenanceRecord
{
    public string EquipmentId { get; set; } = string.Empty;
    public string EquipmentType { get; set; } = string.Empty;
    public DateTime MaintenanceDate { get; set; }
    public string MaintenanceType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
}

public class FacilityProductionRecord
{
    public string FacilityId { get; set; } = string.Empty;
    public DateTime ProductionDate { get; set; }
    public decimal OilThroughput { get; set; }
    public decimal GasThroughput { get; set; }
    public decimal WaterHandling { get; set; }
    public string Status { get; set; } = string.Empty;
}
