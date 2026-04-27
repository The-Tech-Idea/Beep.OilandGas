using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionOperations;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.ProductionOperations.Services;

/// <summary>
/// Facility lifecycle workflows using PPDM39 entities directly (no parallel DTO layer).
/// </summary>
public interface IFacilityManagementService
{
    Task<IReadOnlyList<FACILITY>> ListFacilitiesAsync(string? primaryFieldId, CancellationToken cancellationToken = default);

    Task<FACILITY?> GetFacilityAsync(string facilityId, string? facilityType, CancellationToken cancellationToken = default);

    Task<FACILITY> CreateFacilityAsync(FACILITY facility, string userId, CancellationToken cancellationToken = default);

    Task UpdateFacilityAsync(FACILITY facility, string userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FACILITY_CLASS>> ListFacilityClassesAsync(string facilityId, string? facilityType, CancellationToken cancellationToken = default);

    Task<FACILITY_CLASS> AddFacilityClassAsync(string facilityId, string? facilityType, string facilityClassType, string userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FACILITY_COMPONENT>> ListFacilityComponentsAsync(string facilityId, string? facilityType, CancellationToken cancellationToken = default);

    Task<FACILITY_COMPONENT> AddFacilityComponentAsync(FACILITY_COMPONENT component, string userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FACILITY_STATUS>> ListFacilityStatusHistoryAsync(string facilityId, string? facilityType, CancellationToken cancellationToken = default);

    Task<FACILITY_STATUS> AddFacilityStatusAsync(FACILITY_STATUS status, string userId, bool enforceActiveLicenseForOperationalStatus = true, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FACILITY_RATE>> ListFacilityRatesAsync(string facilityId, string? facilityType, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FACILITY_EQUIPMENT>> ListFacilityEquipmentAsync(string facilityId, string? facilityType, CancellationToken cancellationToken = default);

    Task<FACILITY_EQUIPMENT> LinkEquipmentToFacilityAsync(string facilityId, string? facilityType, string equipmentId, string userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FACILITY_MAINTAIN>> ListFacilityMaintenanceAsync(string facilityId, string? facilityType, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);

    Task<FACILITY_MAINTAIN> CreateFacilityMaintenanceAsync(FACILITY_MAINTAIN maintenance, string userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WORK_ORDER>> ListFacilityWorkOrdersAsync(string facilityId, string? facilityType, CancellationToken cancellationToken = default);

    Task<WORK_ORDER> CreateFacilityWorkOrderAsync(WORK_ORDER workOrder, string facilityId, string facilityType, string userId, CancellationToken cancellationToken = default);

    Task<string> EnsureFacilityPdenAsync(string facilityId, string? facilityType, string userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PDEN_VOL_SUMMARY>> ListFacilityProductionVolumesAsync(string facilityId, string? facilityType, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    Task<PDEN_VOL_SUMMARY> RecordFacilityProductionVolumeAsync(PDEN_VOL_SUMMARY volume, string userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FACILITY_LICENSE>> ListFacilityLicensesAsync(string facilityId, string? facilityType, CancellationToken cancellationToken = default);

    Task<FACILITY_LICENSE> CreateFacilityLicenseAsync(FACILITY_LICENSE license, string userId, CancellationToken cancellationToken = default);

    Task<bool> FacilityHasActiveLicenseAsync(string facilityId, string? facilityType, DateTime? asOf, CancellationToken cancellationToken = default);

    Task<(int MaintenanceEvents, int WorkOrders, decimal? EstimatedAvailabilityPercent)> GetFacilityReliabilityMetricsAsync(
        string facilityId, string? facilityType, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FACILITY_MEASUREMENT>> ListFacilityMeasurementsAsync(
        string facilityId,
        string? facilityType,
        string? equipmentId,
        string? measurementType,
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken = default);

    Task<FACILITY_MEASUREMENT> RecordFacilityMeasurementAsync(
        FACILITY_MEASUREMENT measurement,
        string userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FACILITY_EQUIPMENT_ACTIVITY>> ListEquipmentActivityAsync(
        string facilityId,
        string? facilityType,
        string equipmentId,
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken = default);

    Task<FACILITY_EQUIPMENT_ACTIVITY> RecordEquipmentActivityAsync(
        FACILITY_EQUIPMENT_ACTIVITY activity,
        string userId,
        CancellationToken cancellationToken = default);
}
