using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.ProductionAccounting.Constants;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// Inventory Service - Manages tank and pipeline inventory.
    /// Tracks volume levels, updates inventory, and validates amounts.
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<InventoryService> _logger;
        private const string ConnectionName = "PPDM39";

        public InventoryService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<InventoryService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        /// <summary>
        /// Updates tank inventory with additional volume.
        /// Formula: New Closing Balance = Current Balance + Receipts - Deliveries
        /// </summary>
        public async Task<TANK_INVENTORY> UpdateInventoryAsync(
            string tankId,
            decimal volume,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(tankId))
                throw new ArgumentNullException(nameof(tankId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            _logger?.LogInformation(
                "Updating inventory for tank {TankId} by volume {Volume} for user {UserId}",
                tankId, volume, userId);

            try
            {
                // Get metadata
                var metadata = await _metadata.GetTableMetadataAsync("TANK_INVENTORY");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(TANK_INVENTORY);

                // Create repository
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "TANK_INVENTORY");

                // Get current inventory
                var inventory = await repo.GetByIdAsync(tankId);

                if (inventory == null)
                {
                    _logger?.LogWarning("Tank inventory not found for tank {TankId}", tankId);
                    throw new InvalidOperationException($"Tank inventory not found for tank ID: {tankId}");
                }

                // Cast to TANK_INVENTORY for type safety
                var tankInventory = inventory as TANK_INVENTORY;
                if (tankInventory == null)
                {
                    _logger?.LogError("Failed to cast inventory to TANK_INVENTORY for tank {TankId}", tankId);
                    throw new InvalidOperationException("Inventory type mismatch");
                }

                // Calculate new closing inventory
                var currentClosing = tankInventory.ACTUAL_CLOSING_INVENTORY ?? 0;
                var newClosing = currentClosing + volume;

                // Validate new volume is not negative
                if (newClosing < 0)
                {
                    _logger?.LogError(
                        "Inventory update would result in negative closing inventory for tank {TankId}: {NewClosing}",
                        tankId, newClosing);
                    throw new InvalidOperationException(
                        $"Inventory update would result in negative balance. Current: {currentClosing}, Change: {volume}");
                }

                // Update closing inventory
                tankInventory.ACTUAL_CLOSING_INVENTORY = newClosing;

                // Save changes
                await repo.UpdateAsync(tankInventory, userId);

                _logger?.LogInformation(
                    "Tank inventory updated successfully. Tank: {TankId}, New Closing: {NewClosing}",
                    tankId, newClosing);

                return tankInventory;
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Error updating inventory for tank {TankId}: {ErrorMessage}",
                    tankId, ex.Message);
                throw new ProductionAccountingException(
                    $"Failed to update inventory for tank {tankId}", ex);
            }
        }

        /// <summary>
        /// Gets current inventory level for a tank.
        /// </summary>
        public async Task<TANK_INVENTORY?> GetInventoryAsync(
            string tankId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(tankId))
                throw new ArgumentNullException(nameof(tankId));

            _logger?.LogInformation("Retrieving inventory for tank {TankId}", tankId);

            try
            {
                // Get metadata
                var metadata = await _metadata.GetTableMetadataAsync("TANK_INVENTORY");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(TANK_INVENTORY);

                // Create repository
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "TANK_INVENTORY");

                // Get inventory
                var inventory = await repo.GetByIdAsync(tankId);

                if (inventory == null)
                {
                    _logger?.LogError("Tank inventory not found for tank {TankId}", tankId);
                    throw new ProductionAccountingException($"Tank inventory not found for tank {tankId}");
                }

                var tankInventory = inventory as TANK_INVENTORY;
                _logger?.LogInformation(
                    "Retrieved inventory for tank {TankId}: {ClosingVolume} units",
                    tankId, tankInventory?.ACTUAL_CLOSING_INVENTORY ?? 0);

                return tankInventory;
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Error retrieving inventory for tank {TankId}: {ErrorMessage}",
                    tankId, ex.Message);
                throw new ProductionAccountingException(
                    $"Failed to retrieve inventory for tank {tankId}", ex);
            }
        }

        /// <summary>
        /// Validates inventory data for business rule compliance.
        /// Checks: ACTUAL_CLOSING_INVENTORY >= 0, TANK_BATTERY_ID not empty, etc.
        /// </summary>
        public async Task<bool> ValidateAsync(
            TANK_INVENTORY inventory,
            string cn = "PPDM39")
        {
            if (inventory == null)
                throw new ArgumentNullException(nameof(inventory));

            _logger?.LogInformation(
                "Validating inventory for tank {TankBatteryId}",
                inventory.TANK_BATTERY_ID);

            try
            {
                // Tank Battery ID required
                if (string.IsNullOrWhiteSpace(inventory.TANK_BATTERY_ID))
                {
                    _logger?.LogWarning("Validation failed: TANK_BATTERY_ID is empty");
                    throw new InvalidOperationException("TANK_BATTERY_ID is required");
                }

                // Opening inventory should be non-negative if set
                if (inventory.OPENING_INVENTORY.HasValue && inventory.OPENING_INVENTORY < 0)
                {
                    _logger?.LogWarning(
                        "Validation failed: Negative opening inventory for tank {TankId}: {Volume}",
                        inventory.TANK_BATTERY_ID, inventory.OPENING_INVENTORY);
                    throw new InvalidOperationException(
                        $"OPENING_INVENTORY cannot be negative. Current value: {inventory.OPENING_INVENTORY}");
                }

                // Closing inventory must be non-negative if set
                if (inventory.ACTUAL_CLOSING_INVENTORY.HasValue && inventory.ACTUAL_CLOSING_INVENTORY < 0)
                {
                    _logger?.LogWarning(
                        "Validation failed: Negative closing inventory for tank {TankId}: {Volume}",
                        inventory.TANK_BATTERY_ID, inventory.ACTUAL_CLOSING_INVENTORY);
                    throw new InvalidOperationException(
                        $"ACTUAL_CLOSING_INVENTORY cannot be negative. Current value: {inventory.ACTUAL_CLOSING_INVENTORY}");
                }

                _logger?.LogInformation(
                    "Inventory validation passed for tank {TankBatteryId}",
                    inventory.TANK_BATTERY_ID);

                return true;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Error validating inventory for tank {TankBatteryId}: {ErrorMessage}",
                    inventory.TANK_BATTERY_ID, ex.Message);
                throw new ProductionAccountingException(
                    $"Failed to validate inventory for tank {inventory.TANK_BATTERY_ID}", ex);
            }
        }
    }
}
