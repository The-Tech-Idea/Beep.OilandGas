using System.Threading.Tasks;using Beep.OilandGas.ApiService.Services;using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;
namespace Beep.OilandGas.ApiService.Controllers
{[ApiController][Route("api/hse/engineer")][Authorize]public class HseEngineerController:ControllerBase{private readonly HseEngineerAggregationService _svc;public HseEngineerController(HseEngineerAggregationService svc)=>_svc=svc;[HttpGet("kpi")]public async Task<IActionResult> GetKpi()=>Ok(await _svc.GetKpiAsync());}}
