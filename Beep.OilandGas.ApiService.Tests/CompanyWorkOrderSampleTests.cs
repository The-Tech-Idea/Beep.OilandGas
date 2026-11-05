#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanyWorkOrderSampleTests
{
    [Theory]
    [InlineData("valid")]
    [InlineData("foreign")]
    [InlineData("oversized")]
    [InlineData("missing-active")]
    [InlineData("missing-header-active")]
    public async Task NativeSampleUsesBoundedBeepPageAndRejectsInvalidProviderRows(string mode)
    {
        var native = new PersonaTaskRoutingTests.Fixture();
        var component = new WORK_ORDER_COMPONENT { WORK_ORDER_ID = "order", COMPONENT_ID = "component", FIELD_ID = "field", ACTIVE_IND = "Y" };
        native.Rows.Add(component); native.Rows.Add(new WORK_ORDER { WORK_ORDER_ID = "order", WORK_ORDER_NUMBER = "WO 1", ACTIVE_IND = "Y" });
        if (mode == "missing-header-active")
            native.Source.Setup(x => x.GetEntityAsync("WORK_ORDER", It.IsAny<List<AppFilter>>()))
                .ReturnsAsync(new object[] { new Dictionary<string, object> { ["WORK_ORDER_ID"] = "order", ["WORK_ORDER_NUMBER"] = "WO 1" } });
        native.Source.Setup(x => x.GetEntity("WORK_ORDER_COMPONENT", It.IsAny<List<AppFilter>>(), 1, 21))
            .Returns((string _, List<AppFilter> filters, int page, int size) =>
            {
                Assert.Contains(filters, filter => filter.FieldName == "FIELD_ID" && filter.FilterValue == "field");
                var rows = mode == "oversized" ? Enumerable.Range(0, 22).Select(i => new WORK_ORDER_COMPONENT { WORK_ORDER_ID = "order", COMPONENT_ID = i.ToString(), FIELD_ID = "field", ACTIVE_IND = "Y" }).ToArray() :
                    [new WORK_ORDER_COMPONENT { WORK_ORDER_ID = "order", COMPONENT_ID = "component", FIELD_ID = mode == "foreign" ? "other" : "field", ACTIVE_IND = "Y" }];
                object data = mode == "missing-active" ? new[] { new Dictionary<string, object> { ["WORK_ORDER_ID"] = "order", ["COMPONENT_ID"] = "component", ["FIELD_ID"] = "field" } } : rows;
                return new PagedResult(data, page, size, rows.Length);
            });
        var service = new WorkOrderService(native.Editor.Object, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(),
            Mock.Of<IInspectionService>(), () => Task.FromResult((new CompanyScope("tenant", "company"), "actor", new AssetDatabaseScope("lifecycle", "fingerprint"))),
            NullLogger<WorkOrderService>.Instance, (_, _, _, _) => Task.CompletedTask);
        if (mode == "valid") Assert.Equal("order", Assert.Single(await service.GetByFieldSampleAsync("field", 20)).InstanceId);
        else if (mode is "missing-active" or "missing-header-active") await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetByFieldSampleAsync("field", 20));
        else await Assert.ThrowsAsync<RefusalException>(() => service.GetByFieldSampleAsync("field", 20));
        native.Source.Verify(x => x.GetEntity("WORK_ORDER_COMPONENT", It.IsAny<List<AppFilter>>(), 1, 21), Times.Once);
        Assert.Equal(0, native.Writes);
    }
}
#endif
