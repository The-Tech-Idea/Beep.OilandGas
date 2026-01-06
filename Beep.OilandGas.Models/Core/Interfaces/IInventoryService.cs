using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.DTOs.Inventory;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for inventory operations.
    /// </summary>
    public interface IInventoryService
    {
        /// <summary>
        /// Creates a new inventory item.
        /// </summary>
        Task<INVENTORY_ITEM> CreateItemAsync(CreateInventoryItemRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets an inventory item by ID.
        /// </summary>
        Task<INVENTORY_ITEM?> GetItemAsync(string itemId, string? connectionName = null);
        
        /// <summary>
        /// Gets all inventory items.
        /// </summary>
        Task<List<INVENTORY_ITEM>> GetItemsAsync(string? connectionName = null);
        
        /// <summary>
        /// Updates an inventory item.
        /// </summary>
        Task<INVENTORY_ITEM> UpdateItemAsync(UpdateInventoryItemRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Creates an inventory transaction.
        /// </summary>
        Task<INVENTORY_TRANSACTION> CreateTransactionAsync(CreateInventoryTransactionRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets transactions by inventory item.
        /// </summary>
        Task<List<INVENTORY_TRANSACTION>> GetTransactionsByItemAsync(string itemId, DateTime? startDate, DateTime? endDate, string? connectionName = null);
        
        /// <summary>
        /// Creates an inventory adjustment.
        /// </summary>
        Task<INVENTORY_ADJUSTMENT> CreateAdjustmentAsync(CreateInventoryAdjustmentRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Calculates inventory valuation.
        /// </summary>
        Task<INVENTORY_VALUATION> CalculateValuationAsync(string itemId, ValuationMethod method, DateTime valuationDate, string userId, string? connectionName = null);
        
        /// <summary>
        /// Reconciles inventory (book vs physical).
        /// </summary>
        Task<InventoryReconciliationResult> ReconcileInventoryAsync(string itemId, DateTime reconciliationDate, decimal physicalQuantity, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets inventory summary.
        /// </summary>
        Task<InventorySummary> GetInventorySummaryAsync(string? itemId, string? connectionName = null);
        
        /// <summary>
        /// Gets items requiring reconciliation.
        /// </summary>
        Task<List<INVENTORY_ITEM>> GetItemsRequiringReconciliationAsync(string? connectionName = null);
    }
}




