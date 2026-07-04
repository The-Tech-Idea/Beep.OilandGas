using System.Threading.Tasks;using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers
{
    [ApiController][Route("api/production/engineer")][Authorize]
    public class ProductionEngineerController:ControllerBase
    {private readonly ProductionEngineerAggregationService _svc;public ProductionEngineerController(ProductionEngineerAggregationService svc)=>_svc=svc;
    [HttpGet("kpi")]public async Task<IActionResult> GetKpi([FromQuery]string?fieldId=null)=>Ok(await _svc.GetKpiAsync(fieldId));}
}
