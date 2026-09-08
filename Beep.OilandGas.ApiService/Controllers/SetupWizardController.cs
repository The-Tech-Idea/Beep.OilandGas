using System.Security.Claims;
using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ApiService.Controllers;

[ApiController]
[Route("api/setup/wizard")]
[Authorize(Roles = "Admin,Administrator")]
public class SetupWizardController(IDMEEditor editor, SetupWizardCoordinator coordinator) : ControllerBase
{
    private readonly IDMEEditor _editor = editor;
        [HttpGet("preflight")]
        public IActionResult Preflight([FromQuery]string? connectionName=null)
        {
            var dsService=new TheTechIdea.Beep.Services.DatasourceManagement.DatasourceManagementService(_editor);
            var connections=dsService.GetAllDatasources();
            var connList=connections.Select(c=>new{name=c.ConnectionName,type=c.DatabaseType.ToString(),server=c.Host,category=c.Category.ToString()}).ToList();

            // Check if target datasource exists and probe for WELL table
            int tableCount=0;bool hasExistingTables=false;
            var target=string.IsNullOrWhiteSpace(connectionName)?connections.FirstOrDefault():connections.FirstOrDefault(c=>string.Equals(c.ConnectionName,connectionName,StringComparison.OrdinalIgnoreCase));
            if(target!=null){try{var ds=_editor.GetDataSource(target.ConnectionName);if(ds!=null){var well=ds.GetEntity("WELL",new List<TheTechIdea.Beep.Report.AppFilter>());hasExistingTables=well!=null&&((System.Collections.IEnumerable)well).Cast<object>().Any();tableCount=hasExistingTables?1:0;}}catch{}}

            var drivers=_editor.ConfigEditor?.DataDriversClasses?.Select(d=>new{name=d.DatasourceType.ToString(),loaded=!string.IsNullOrEmpty(d.classHandler),package=d.PackageName}).ToList()??new();

            return Ok(new{
                isFirstRun=connList.Count==0,
                hasExistingDatasource=connList.Count>0,
                datasources=connList,
                hasExistingTables,existingTableCount=tableCount,
                installedDrivers=drivers,
                connectionName=connectionName??"PPDM39"
            });
        }


    [HttpPost("start")]
    public IActionResult StartWizard([FromQuery] string connectionName = "PPDM39")
    {
        if (string.IsNullOrWhiteSpace(connectionName)) return BadRequest(new { message = "Connection name is required." });
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        if (!coordinator.TryStart(connectionName, userId))
            return Conflict(new { message = "Setup is already active or the worker cannot accept work." });
        return Accepted(new { message = "Setup wizard queued", statusEndpoint = "/api/setup/wizard/status" });
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        var s = coordinator.GetStatus();
        var stepStatus = s.State switch
        {
            "Completed" => "completed", "Failed" => "failed", "Cancelled" => "cancelled",
            "Running" or "Cancelling" => "running", _ => "pending"
        };
        return Ok(new
        {
            state = s.State, currentStep = "PPDM Modules", message = s.Message,
            percent = s.State == "Completed" ? 100 : s.Total > 0 ? (int)Math.Clamp(100L * s.Done / s.Total, 0, 100) : 0,
            totalSteps = 1, currentStepIndex = s.State == "Completed" ? 1 : 0,
            steps = new[] { new { id = "ppdm-modules", name = "PPDM Modules", description = "PPDM domain modules", status = stepStatus } },
            elapsedSeconds = s.State == "Idle" ? 0 : (int)((s.FinishedAt ?? DateTime.UtcNow) - s.StartedAt).TotalSeconds
        });
    }

    [HttpPost("cancel")]
    public IActionResult CancelWizard() => coordinator.Cancel()
        ? Ok(new { message = "Cancellation requested" })
        : NotFound(new { message = "No wizard is running" });
}
