using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Storage;
using Beep.OilandGas.Models.DTOs.Storage;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for storage operations.
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// Registers a storage facility.
        /// </summary>
        Task<STORAGE_FACILITY> RegisterFacilityAsync(CreateStorageFacilityRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets a storage facility by ID.
        /// </summary>
        Task<STORAGE_FACILITY?> GetFacilityAsync(string facilityId, string? connectionName = null);
        
        /// <summary>
        /// Gets all storage facilities.
        /// </summary>
        Task<List<STORAGE_FACILITY>> GetFacilitiesAsync(string? connectionName = null);
        
        /// <summary>
        /// Registers a tank battery.
        /// </summary>
        Task<TANK_BATTERY> RegisterTankBatteryAsync(CreateTankBatteryRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets a tank battery by ID.
        /// </summary>
        Task<TANK_BATTERY?> GetTankBatteryAsync(string tankBatteryId, string? connectionName = null);
        
        /// <summary>
        /// Gets tank batteries by facility.
        /// </summary>
        Task<List<TANK_BATTERY>> GetTankBatteriesByFacilityAsync(string facilityId, string? connectionName = null);
        
        /// <summary>
        /// Registers a service unit.
        /// </summary>
        Task<SERVICE_UNIT> RegisterServiceUnitAsync(CreateServiceUnitRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Registers a LACT unit.
        /// </summary>
        Task<LACT_UNIT> RegisterLACTUnitAsync(CreateLACTUnitRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets storage capacity summary.
        /// </summary>
        Task<StorageCapacitySummary> GetStorageCapacityAsync(string facilityId, string? connectionName = null);
        
        /// <summary>
        /// Gets storage utilization report.
        /// </summary>
        Task<StorageUtilizationReport> GetStorageUtilizationAsync(string facilityId, DateTime? asOfDate, string? connectionName = null);
        
        /// <summary>
        /// Gets facilities requiring maintenance.
        /// </summary>
        Task<List<STORAGE_FACILITY>> GetFacilitiesRequiringMaintenanceAsync(string? connectionName = null);
    }
}




