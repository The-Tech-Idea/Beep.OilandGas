using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using CreateRequest = Beep.OilandGas.Models.Data.Inventory.CreateTankInventoryRequest;

namespace Beep.OilandGas.ApiService.Services;

public sealed class TankInventoryStore(
    IDMEEditor editor,
    ICommonColumnHandler columns,
    IPPDM39DefaultsRepository defaults,
    IPPDMMetadataRepository metadata,
    Func<Task<string>> resolveConnection)
{
    private async Task<PPDMGenericRepository> RepositoryAsync()
    {
        var connection = await resolveConnection();
        if (string.IsNullOrWhiteSpace(connection))
            throw new InvalidOperationException("A PRODUCTION database binding is required.");
        return new PPDMGenericRepository(editor, columns, defaults, metadata,
            typeof(TANK_INVENTORY), connection, "TANK_INVENTORY");
    }

    public async Task<TANK_INVENTORY?> GetAsync(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var repository = await RepositoryAsync();
        return await repository.GetByIdAsync(id) as TANK_INVENTORY;
    }

    public async Task<TANK_INVENTORY> CreateAsync(CreateRequest request, string actor)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(actor);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.TankBatteryId);
        if (request.OpeningInventory < 0 || request.Receipts < 0 || request.Deliveries < 0 || request.ActualClosingInventory < 0)
            throw new ArgumentException("Inventory volumes cannot be negative.");
        var repository = await RepositoryAsync();
        var inventory = new TANK_INVENTORY
        {
            TANK_INVENTORY_ID = Guid.NewGuid().ToString(),
            TANK_BATTERY_ID = request.TankBatteryId,
            INVENTORY_DATE = request.InventoryDate ?? DateTime.UtcNow,
            OPENING_INVENTORY = request.OpeningInventory,
            RECEIPTS = request.Receipts,
            DELIVERIES = request.Deliveries,
            ACTUAL_CLOSING_INVENTORY = request.ActualClosingInventory,
            ACTIVE_IND = defaults.GetActiveIndicatorYes(),
            PPDM_GUID = Guid.NewGuid().ToString(),
            ROW_CREATED_BY = actor,
            ROW_CREATED_DATE = DateTime.UtcNow
        };
        await repository.InsertAsync(inventory, actor);
        return inventory;
    }
}
