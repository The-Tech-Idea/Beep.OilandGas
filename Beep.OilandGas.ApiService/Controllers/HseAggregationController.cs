using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers
{
    [ApiController][Route("api/hse/aggregation")][Authorize]
    public class HseAggregationController:ControllerBase
    {
        private readonly HseAggregationService _svc;
        public HseAggregationController(HseAggregationService svc)=>_svc=svc;
        [HttpGet("incidents")]public async Task<IActionResult> GetIncidents([FromQuery]string?fieldId=null,[FromQuery]DateTime?start=null,[FromQuery]DateTime?end=null)=>Ok(await _svc.GetIncidentSummaryAsync(fieldId,start,end));
    }
}
