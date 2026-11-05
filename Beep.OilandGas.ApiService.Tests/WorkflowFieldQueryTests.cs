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
    [Theory][InlineData("type")][InlineData("retag-prefix")][InlineData("retag-payload")]
    public async Task GenericFieldReaderCannotProjectUnownedCompletionReview(string marker)
    {
        var id=marker=="retag-prefix"?TheTechIdea.Data.OilGas.DrillingCompletionReviewStart.InstancePrefix+"unqualified":"completion-review";
        var source=new Beep.OilandGas.Models.Processes.ProcessInstance{InstanceId=id,FieldId="field",EntityId="reported-claim",EntityType=marker=="type"?"DRILLING_DAILY_REPORT":"WELL"};
        if(marker=="retag-payload")source.ProcessData=new(){Data=new(){[TheTechIdea.Data.OilGas.DrillingCompletionReviewStart.PayloadKey]=new{unqualified=true}}};
        var process=new Mock<IProcessService>(MockBehavior.Strict);
        process.Setup(value=>value.GetProcessInstanceAsync(id)).ReturnsAsync(source);
        var resources=new Mock<IAccessControlService>(MockBehavior.Strict);
        var reader=new Beep.OilandGas.ApiService.Services.CompanyProcessReadService(resources.Object);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>reader.ReadAsync(process.Object,"actor","field",id,new("tenant","company")));
        resources.VerifyNoOtherCalls();process.Verify(value=>value.GetProcessInstanceAsync(id),Times.Once);
    }
    [Fact]
    public async Task GenericFieldListCannotExposeSourceBoundInvestmentAssignmentsOrRetaggedMarkers()
    {
        var field=new Mock<IFieldOrchestrator>();field.SetupGet(value=>value.CurrentFieldId).Returns("field");var process=new Mock<IProcessService>(MockBehavior.Strict);
        process.Setup(value=>value.GetProcessInstancesForFieldAsync("field")).ReturnsAsync([
            new(){InstanceId="well-process",FieldId="field",EntityId="well",EntityType="WELL"},
            new(){InstanceId="owned-investment",FieldId="field",EntityId="private-afe",EntityType="AFE"},
            new(){InstanceId="unowned-completion",FieldId="field",EntityId="private-completion-claim",EntityType="DRILLING_DAILY_REPORT"},
            new(){InstanceId=TheTechIdea.Data.OilGas.DrillingCompletionReviewStart.InstancePrefix+"retagged",FieldId="field",EntityId="private-completion",EntityType="WELL"},
            new(){InstanceId="retagged-completion-payload",FieldId="field",EntityId="private-completion",EntityType="WELL",ProcessData=new(){Data=new(){[TheTechIdea.Data.OilGas.DrillingCompletionReviewStart.PayloadKey]=new{privateCompany=true}}}},
            new(){InstanceId=TheTechIdea.Data.OilGas.DevelopmentInvestmentHandoff.InstancePrefix+"retagged",FieldId="field",EntityId="private-afe",EntityType="WELL"},
            new(){InstanceId="retagged-payload",FieldId="field",EntityId="private-afe",EntityType="WELL",ProcessData=new(){Data=new(){[TheTechIdea.Data.OilGas.DevelopmentInvestmentHandoff.PayloadKey]=new{privateCompany=true}}}}
        ]);
        var controller=new BusinessProcessController(field.Object,process.Object,NullLogger<BusinessProcessController>.Instance,Mock.Of<IAccessControlService>(),Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),Mock.Of<IJournalEntryService>(),Mock.Of<IWorkOrderService>(),Mock.Of<IInspectionService>());
        var result=await controller.GetInstancesAsync();var rows=Assert.IsType<List<ProcessInstanceSummary>>(Assert.IsType<OkObjectResult>(result.Result).Value);Assert.Equal("well",Assert.Single(rows).EntityId);
        process.Verify(value=>value.GetProcessInstancesForFieldAsync("field"),Times.Once);process.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task FieldListQueriesFieldIdAndIncludesChildEntityWorkflows()
    {
        var source = new Mock<IDataSource>(MockBehavior.Strict);
        source.SetupGet(value => value.ErrorObject).Returns(new TheTechIdea.Beep.ConfigUtil.ErrorsInfo { Flag = TheTechIdea.Beep.ConfigUtil.Errors.Ok });
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
        var controller = new BusinessProcessController(field.Object, bound, NullLogger<BusinessProcessController>.Instance, new Mock<IAccessControlService>().Object, new Mock<TheTechIdeaWeb.Diagnostics.IFailureReporter>().Object, new Mock<Beep.OilandGas.Models.Core.Interfaces.IJournalEntryService>().Object, new Mock<IWorkOrderService>().Object, new Mock<IInspectionService>().Object);
        var result = await controller.GetInstancesAsync();
        var rows = Assert.IsType<List<ProcessInstanceSummary>>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal(new[] { "well", "facility" }, rows.Select(x => x.EntityId));
        Assert.Equal(1, resolutions);
        source.VerifyAll();
        editor.Verify(x => x.GetDataSource("selected-module"), Times.Once);
        editor.VerifyNoOtherCalls();
    }
}
