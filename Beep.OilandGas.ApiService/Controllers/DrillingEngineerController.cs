using System.Threading.Tasks;using Beep.OilandGas.ApiService.Services;using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;
namespace Beep.OilandGas.ApiService.Controllers
{[ApiController][Route("api/drilling/engineer")][Authorize]public class DrillingEngineerController:ControllerBase{private readonly DrillingEngineerAggregationService _svc;public DrillingEngineerController(DrillingEngineerAggregationService svc)=>_svc=svc;[HttpGet("kpi")]public async Task<IActionResult> GetKpi([FromQuery]string?fieldId=null)=>Ok(await _svc.GetKpiAsync(fieldId));}}
