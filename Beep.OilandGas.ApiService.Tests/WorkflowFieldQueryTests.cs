using Beep.OilandGas.ApiService.Controllers.BusinessProcess;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class WorkflowFieldQueryTests
{
    [Fact]
    public async Task FieldListQueriesFieldIdAndIncludesChildEntityWorkflows()
    {
        var source = new Mock<IDataSource>(MockBehavior.Strict);
        source.Setup(x => x.GetEntityAsync("PROCESS_INSTANCE", It.Is<List<AppFilter>>(filters =>
            filters.Count == 2 && filters.Any(f => f.FieldName == "FIELD_ID" && f.FilterValue == "field" && f.Operator == "=") &&
            filters.Any(f => f.FieldName == "ACTIVE_IND" && f.FilterValue == "Y"))))
            .ReturnsAsync(new List<PROCESS_INSTANCE>
            {
                new() { PROCESS_INSTANCE_ID = "well-process", FIELD_ID = "field", ENTITY_ID = "well", ENTITY_TYPE = "WELL" },
                new() { PROCESS_INSTANCE_ID = "facility-process", FIELD_ID = "field", ENTITY_ID = "facility", ENTITY_TYPE = "FACILITY" },
                new() { PROCESS_INSTANCE_ID = "foreign-process", FIELD_ID = "foreign", ENTITY_ID = "field", ENTITY_TYPE = "FIELD" },
                new() { PROCESS_INSTANCE_ID = "inactive-process", FIELD_ID = "field", ACTIVE_IND = "N" }
            });
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        editor.Setup(x => x.GetDataSource("selected-module")).Returns(source.Object);
        var resolutions = 0;
        var bound = new BoundProcessService(() => { resolutions++; return Task.FromResult("selected-module"); },
            target => new PPDMProcessService(editor.Object, Mock.Of<ICommonColumnHandler>(),
                Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(), target));
        var field = new Mock<IFieldOrchestrator>();
        field.SetupGet(x => x.CurrentFieldId).Returns("field");
        var controller = new BusinessProcessController(field.Object, bound, NullLogger<BusinessProcessController>.Instance);
        var result = await controller.GetInstancesAsync();
        var rows = Assert.IsType<List<ProcessInstanceSummary>>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal(new[] { "well", "facility" }, rows.Select(x => x.EntityId));
        Assert.Equal(1, resolutions);
        source.VerifyAll();
        editor.Verify(x => x.GetDataSource("selected-module"), Times.Once);
        editor.VerifyNoOtherCalls();
    }
}
