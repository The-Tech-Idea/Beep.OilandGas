using System.Text.Json;
using TheTechIdea.Data.OilGas;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public sealed class CompanyMaintenanceStatusMutationBoundaryTests
{
    public static IEnumerable<object[]> RawMutations()
    {
        foreach (var kind in new[] { "facility", "equipment", "facility-alias", "equipment-alias", "facility-table", "equipment-table" })
        foreach (var operation in new[] { "insert", "update", "delete", "soft-delete", "parent-insert", "batch", "update-batch", "sized-batch", "sized-update", "delete-batch", "parent-batch", "upsert", "uow" })
            yield return new object[] { kind, operation };
    }

    private static (Type Type, string Table, object Row) Target(string kind) => kind switch
    {
        "facility-table" => (typeof(FIELD), "facility_status", new FIELD { FIELD_ID = "A" }),
        "equipment-table" => (typeof(FIELD), "equipment_status", new FIELD { FIELD_ID = "A" }),
        "facility-alias" => (typeof(FACILITY_STATUS), "alias", new FACILITY_STATUS { FACILITY_ID = "plant", FACILITY_TYPE = "gas", STATUS_ID = "original" }),
        "equipment-alias" => (typeof(EQUIPMENT_STATUS), "alias", new EQUIPMENT_STATUS { EQUIPMENT_ID = "pump", EQUIP_STATUS_ID = "original" }),
        "facility" => (typeof(FACILITY_STATUS), "FACILITY_STATUS", new FACILITY_STATUS { FACILITY_ID = "plant", FACILITY_TYPE = "gas", STATUS_ID = "original" }),
        _ => (typeof(EQUIPMENT_STATUS), "EQUIPMENT_STATUS", new EQUIPMENT_STATUS { EQUIPMENT_ID = "pump", EQUIP_STATUS_ID = "original" })
    };

    [Theory, MemberData(nameof(RawMutations))]
    public async Task RawStatusWritesRefuseEvenInAnOwnedSerializableTransaction(string kind, string operation)
    {
        using var f = await CompanyPPDMRowAccessTests.Fixture.Create();
        f.EnableCorrectiveActionTransactions();
        var target = Target(kind);
        var repo = new PPDMGenericRepository(f.Editor.Object, f.Columns.Object, f.Defaults.Object,
            f.Metadata.Object, target.Type, "selected-tenant", target.Table);
        var body = JsonSerializer.Serialize(target.Row, target.Type);
        var reads = f.Source.Invocations.Count(call => call.Method.Name == "GetEntityAsync");
        async Task Attempt()
        {
            switch (operation)
            {
                case "insert": await repo.InsertAsync(target.Row, "actor"); break;
                case "update": await repo.UpdateAsync(target.Row, "actor"); break;
                case "delete": await repo.DeleteAsync(target.Row); break;
                case "soft-delete": await repo.SoftDeleteAsync("original", "actor"); break;
                case "parent-insert": await repo.InsertWithParentKeysAsync(target.Row, "actor", new() { ["FIELD_ID"] = "A" }); break;
                case "batch": await repo.InsertBatchAsync([target.Row], "actor"); break;
                case "update-batch": await repo.UpdateBatchAsync([target.Row], "actor"); break;
                case "sized-batch": await repo.InsertBatchWithSizeAsync([target.Row], "actor", 1); break;
                case "sized-update": await repo.UpdateBatchAsync([target.Row], "actor", 1); break;
                case "delete-batch": await repo.DeleteBatchAsync(["original"], "actor", false, 1); break;
                case "parent-batch": await repo.InsertBatchWithParentKeysAsync([target.Row], "actor", new() { ["FIELD_ID"] = "A" }); break;
                case "upsert": await repo.UpsertBatchAsync([target.Row], "actor", 1); break;
                case "uow": _ = repo.UnitOfWork; break;
            }
        }
        await Assert.ThrowsAsync<RefusalException>(Attempt);
        await PPDMDataSourceTransaction.ExecuteAsync(f.Source.Object,
            async () => await Assert.ThrowsAsync<RefusalException>(Attempt));
        Assert.Empty(f.Writes);
        Assert.Equal(reads, f.Source.Invocations.Count(call => call.Method.Name == "GetEntityAsync"));
        Assert.Equal(body, JsonSerializer.Serialize(target.Row, target.Type));
    }

    [Theory]
    [InlineData("facility", PPDMRowMutation.Insert)] [InlineData("facility", PPDMRowMutation.Update)] [InlineData("facility", PPDMRowMutation.Delete)]
    [InlineData("equipment", PPDMRowMutation.Insert)] [InlineData("equipment", PPDMRowMutation.Update)] [InlineData("equipment", PPDMRowMutation.Delete)]
    [InlineData("facility-table", PPDMRowMutation.Insert)] [InlineData("facility-table", PPDMRowMutation.Update)] [InlineData("facility-table", PPDMRowMutation.Delete)]
    [InlineData("equipment-table", PPDMRowMutation.Insert)] [InlineData("equipment-table", PPDMRowMutation.Update)] [InlineData("equipment-table", PPDMRowMutation.Delete)]
    public async Task CompanyAssetGrantsDoNotAuthorizeRawStatusWrites(string kind, PPDMRowMutation operation)
    {
        using var f = await CompanyMaintenanceQualifiedAssetTests.ReviewFixture.Create();
        var target = Target(kind);
        var reads = f.Native.Source.Invocations.Count(call => call.Method.Name == "GetEntityAsync");
        await Assert.ThrowsAsync<RefusalException>(() => f.Native.Policy.ValidateMutationAsync(target.Type,
            target.Table, target.Row, operation == PPDMRowMutation.Insert ? null : target.Row, operation, "actor"));
        Assert.Empty(f.Native.Writes);
        Assert.Equal(reads, f.Native.Source.Invocations.Count(call => call.Method.Name == "GetEntityAsync"));
    }

    [Theory]
    [InlineData("Active", true)] [InlineData("Active", false)]
    [InlineData("INACTIVE", true)] [InlineData("INACTIVE", false)]
    [InlineData("Operating", true)] [InlineData("Operating", false)]
    [InlineData("Shut Down", true)] [InlineData("Shut Down", false)]
    public async Task LegacyFacilitySetterCannotUseCodeInferenceOrSkipLicenseFlagToBypassReturn(string code, bool enforce)
    {
        using var f = await CompanyMaintenanceQualifiedAssetTests.ReviewFixture.Create();
        var row = new FACILITY_STATUS { FACILITY_ID = "plant", FACILITY_TYPE = "gas", STATUS = code,
            STATUS_TYPE = "OPERATIONAL", ACTIVE_IND = "Y", PERCENT_CAPABILITY = 100m };
        var body = JsonSerializer.Serialize(row);
        var reads = f.Native.Source.Invocations.Count(call => call.Method.Name == "GetEntityAsync");
        await Assert.ThrowsAsync<RefusalException>(() => f.Owner.AddFacilityStatusAsync(row, "actor", enforce));
        Assert.Empty(f.Native.Writes);
        Assert.Equal(reads, f.Native.Source.Invocations.Count(call => call.Method.Name == "GetEntityAsync"));
        Assert.Equal(body, JsonSerializer.Serialize(row));
    }
}
