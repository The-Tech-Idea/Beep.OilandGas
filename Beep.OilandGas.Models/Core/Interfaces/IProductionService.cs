using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.DTOs.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for production operations.
    /// </summary>
    public interface IProductionService
    {
        /// <summary>
        /// Creates a run ticket.
        /// </summary>
        Task<RUN_TICKET> CreateRunTicketAsync(
            CreateRunTicketRequest request,
            string userId,
            string? connectionName = null);
        
        /// <summary>
        /// Gets a run ticket by number.
        /// </summary>
        Task<RUN_TICKET?> GetRunTicketAsync(string runTicketNumber, string? connectionName = null);
        
        /// <summary>
        /// Gets all run tickets for a lease.
        /// </summary>
        Task<List<RUN_TICKET>> GetRunTicketsByLeaseAsync(string leaseId, string? connectionName = null);
        
        /// <summary>
        /// Gets run tickets by date range.
        /// </summary>
        Task<List<RUN_TICKET>> GetRunTicketsByDateRangeAsync(DateTime startDate, DateTime endDate, string? connectionName = null);
        
        /// <summary>
        /// Creates a tank inventory record.
        /// </summary>
        Task<TANK_INVENTORY> CreateTankInventoryAsync(
            CreateTankInventoryRequest request,
            string userId,
            string? connectionName = null);
        
        /// <summary>
        /// Gets tank inventory by ID.
        /// </summary>
        Task<TANK_INVENTORY?> GetTankInventoryAsync(string inventoryId, string? connectionName = null);
        
        /// <summary>
        /// Calculates total production for a lease in a date range.
        /// </summary>
        Task<decimal> CalculateTotalProductionAsync(string leaseId, DateTime startDate, DateTime endDate, string? connectionName = null);
        
        /// <summary>
        /// Calculates dispositions by type.
        /// </summary>
        Task<Dictionary<DispositionType, decimal>> CalculateDispositionsByTypeAsync(DateTime startDate, DateTime endDate, string? connectionName = null);
    }
}

