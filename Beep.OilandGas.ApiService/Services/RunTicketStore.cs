using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ApiService.Services;

public sealed class RunTicketStore(
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
            typeof(RUN_TICKET), connection, "RUN_TICKET");
    }

    public async Task<List<RUN_TICKET>> ListAsync(DateTime start, DateTime end, string? leaseId = null)
    {
        if (end < start) throw new ArgumentException("End date must not precede start date.");
        var repository = await RepositoryAsync();
        var filters = new List<AppFilter>
        {
            new() { FieldName = "TICKET_DATE_TIME", Operator = ">=", FilterValue = start.ToString("yyyy-MM-ddTHH:mm:ss") },
            new() { FieldName = "TICKET_DATE_TIME", Operator = "<=", FilterValue = end.ToString("yyyy-MM-ddTHH:mm:ss") }
        };
        if (!string.IsNullOrWhiteSpace(leaseId))
            filters.Add(new AppFilter { FieldName = "LEASE_ID", Operator = "=", FilterValue = leaseId });
        var rows = await repository.GetAsync(filters);
        return rows?.Cast<RUN_TICKET>().ToList() ?? [];
    }

    public async Task<RUN_TICKET?> GetAsync(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var repository = await RepositoryAsync();
        if (await repository.GetByIdAsync(id) is RUN_TICKET ticket) return ticket;
        var rows = await repository.GetAsync(new List<AppFilter>
        {
            new() { FieldName = "RUN_TICKET_NUMBER", Operator = "=", FilterValue = id }
        });
        return rows?.Cast<RUN_TICKET>().FirstOrDefault();
    }

    public async Task<RUN_TICKET> CreateAsync(CreateRunTicketRequest request, string actor)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(actor);
        var repository = await RepositoryAsync();
        var id = Guid.NewGuid().ToString();
        var ticket = new RUN_TICKET
        {
            RUN_TICKET_ID = id,
            RUN_TICKET_NUMBER = $"RT-{Guid.NewGuid():N}",
            LEASE_ID = request.LEASE_ID,
            WELL_ID = request.WELL_ID,
            TANK_BATTERY_ID = request.TANK_BATTERY_ID,
            TICKET_DATE_TIME = request.TICKET_DATE_TIME ?? DateTime.UtcNow,
            GROSS_VOLUME = request.GROSS_VOLUME,
            BSW_PERCENTAGE = request.BSWPERCENTAGE,
            NET_VOLUME = request.GROSS_VOLUME * (1m - request.BSWPERCENTAGE / 100m),
            TEMPERATURE = request.TEMPERATURE,
            API_GRAVITY = request.API_GRAVITY ?? 0m,
            DISPOSITION_TYPE = request.DISPOSITION_TYPE,
            PURCHASER = request.PURCHASER,
            ACTIVE_IND = defaults.GetActiveIndicatorYes(),
            PPDM_GUID = Guid.NewGuid().ToString(),
            ROW_CREATED_BY = actor,
            ROW_CREATED_DATE = DateTime.UtcNow
        };
        await repository.InsertAsync(ticket, actor);
        return ticket;
    }
}
