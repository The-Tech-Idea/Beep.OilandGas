using Beep.OilandGas.ApiService.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.Importing;
using Xunit;

namespace Beep.OilandGas.PPDM39.Tests;

public class BeepSyncAdapterTests
{
    private static BeepSyncService Create() => new(Mock.Of<IDMEEditor>(), NullLogger<BeepSyncService>.Instance);

    [Fact]
    public void MapsCurrentSchemaContractAndReplacesDuplicateId()
    {
        using var service = Create();
        var name = "TEST_" + Guid.NewGuid().ToString("N");
        var schema = service.CreateEntitySyncSchema(name, "source", "destination", SyncMode.Upsert,
            fieldMappings: [new() { SourceField = "ID", DestField = "DEST_ID", IsKey = true }]);
        Assert.Equal(name, schema.DestinationEntityName);
        Assert.Equal("destination", schema.DestinationDataSourceName);
        Assert.Equal("Upsert", schema.SyncType);
        Assert.Equal("OneWay", schema.SyncDirection);
        Assert.Equal("ID", schema.SourceKeyField);
        Assert.Equal("DEST_ID", schema.DestinationSyncDataField);
        Assert.Equal("DEST_ID", Assert.Single(schema.MappedFields).DestinationField);
        service.CreateEntitySyncSchema(name, "source", "destination");
        Assert.Single(service.GetSchemas().Where(s => s.Id == schema.Id));
    }

    [Fact]
    public async Task UnknownSchemaDoesNotReportSuccess()
    {
        using var service = Create();
        var result = await service.SyncEntityAsync(Guid.NewGuid().ToString());
        Assert.False(result.Success);
        Assert.NotEmpty(result.ErrorMessage!);
        Assert.Null(service.GetLastReconciliation());
    }

    [Fact]
    public async Task InvalidSchemaReturnsFailureWithoutReport()
    {
        using var service = Create();
        var schema = service.CreateEntitySyncSchema("TEST_" + Guid.NewGuid().ToString("N"), "missing-source", "missing-destination",
            fieldMappings: [new() { SourceField = "ID", DestField = "ID", IsKey = true }]);
        var result = await service.SyncEntityAsync(schema.Id);
        Assert.False(result.Success);
        Assert.Equal(0, result.RecordsInserted);
        Assert.Null(result.Reconciliation);
    }

    [Fact]
    public async Task CancellationIsNotConvertedIntoSuccessOrOrdinaryFailure()
    {
        using var service = Create();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.SyncEntityAsync("unused", token: new(true)));
    }

    [Fact]
    public void RejectsUnsupportedDirectionAndCompositeKeys()
    {
        using var service = Create();
        Assert.Throws<ArgumentException>(() => service.CreateEntitySyncSchema("TEST", "source", "dest", direction: "Bidirectional"));
        Assert.Throws<ArgumentException>(() => service.CreateEntitySyncSchema("TEST", "source", "dest", fieldMappings:
            [new() { SourceField = "A", DestField = "A", IsKey = true }, new() { SourceField = "B", DestField = "B", IsKey = true }]));
    }

    [Fact]
    public void IncrementalRequiresWatermarkAndFullSyncUsesKeyFields()
    {
        using var service = Create();
        var name = "TEST_" + Guid.NewGuid().ToString("N");
        List<SyncFieldMapping> mappings = [new() { SourceField = "ID", DestField = "ID", IsKey = true }];
        Assert.Throws<ArgumentException>(() => service.CreateEntitySyncSchema(name, "source", "dest", SyncMode.Incremental, fieldMappings: mappings));
        var full = service.CreateEntitySyncSchema(name, "source", "dest", fieldMappings: mappings);
        Assert.Equal("ID", full.DestinationSyncDataField);
        mappings.Add(new() { SourceField = "CHANGED", DestField = "UPDATED", IsWatermark = true });
        var incremental = service.CreateEntitySyncSchema(name, "source", "dest", SyncMode.Incremental, fieldMappings: mappings);
        Assert.Equal("CHANGED", incremental.SourceSyncDataField);
        Assert.Equal("UPDATED", incremental.DestinationSyncDataField);
    }
}
