using Beep.OilandGas.HSE.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.BusinessProcess;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Processes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;
public sealed class HseReportJurisdictionTests
{
    [Theory][InlineData("2026-10-27T15:30+03:00",12,30)][InlineData("2026-10-27T08:30-04:00",12,30)][InlineData("2026-10-27T12:30Z",12,30)]
    public void ExplicitOffsetsConvertToTheSameUtcInstant(string text,int hour,int minute)
    {
        Assert.True(IncidentReportTime.TryParseIncidentTime(text,out var value));Assert.Equal(new DateTime(2026,10,27,hour,minute,0,DateTimeKind.Utc),value);
    }
    [Theory][InlineData("2026-10-27T12:30")][InlineData("2026-10-27")][InlineData(" 2026-10-27T12:30Z")][InlineData("")]
    public void MissingOrAmbiguousOffsetIsRejected(string text)=>Assert.False(IncidentReportTime.TryParseIncidentTime(text,out _));
    [Theory][InlineData("single")][InlineData("none")]
    public async Task IncidentListUsesCompanyWorkflowReader(string mode)
    {
        var hse=new Mock<IFieldHSEService>();hse.Setup(x=>x.GetIncidentsAsync(null)).ReturnsAsync([new HSEIncidentRecord{IncidentId="incident",FieldId="field"}]);
        var field=new Mock<IFieldOrchestrator>();field.SetupGet(x=>x.CurrentFieldId).Returns("field");field.Setup(x=>x.GetHSEService()).Returns(hse.Object);
        var process=new ProcessInstance{InstanceId="process",EntityId="incident",EntityType="HSE_INCIDENT",FieldId="field",ProcessId="HSE_INCIDENT_REPORTING"};
        var reader=new Mock<ICompanyHseWorkflowReader>();reader.Setup(x=>x.ReadAsync(It.IsAny<ClaimsPrincipal>(),"incident")).ReturnsAsync(mode=="none"?null:process);
        var controller=new HSEProcessController(field.Object,reader.Object,NullLogger<HSEProcessController>.Instance);
        var result=await controller.GetIncidentsAsync();var rows=Assert.IsType<List<ProcessInstanceSummary>>(Assert.IsType<OkObjectResult>(result.Result).Value);Assert.Equal(mode=="single"?"process":"",Assert.Single(rows).InstanceId);
    }
}
#endif
