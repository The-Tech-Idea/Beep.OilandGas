using System.Threading.Tasks;using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers
{[ApiController][Route("api/reservoir/engineer")][Authorize]
public class ReservoirEngineerController:ControllerBase
{private readonly ReservoirEngineerAggregationService _svc;public ReservoirEngineerController(ReservoirEngineerAggregationService svc)=>_svc=svc;
[HttpGet("kpi")]public async Task<IActionResult> GetKpi([FromQuery]string?fieldId=null)=>Ok(await _svc.GetKpiAsync(fieldId));}}
