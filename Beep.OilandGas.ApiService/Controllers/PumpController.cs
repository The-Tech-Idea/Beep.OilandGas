using Beep.OilandGas.Models.Data.PumpPerformance;
using Beep.OilandGas.PumpPerformance.Calculations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers
{
    [ApiController]
    [Route("api/pump")]
    [Authorize]
    public class PumpController : ControllerBase
    {
        private readonly ILogger<PumpController> _logger;

        public PumpController(ILogger<PumpController> logger)
        {
            _logger = logger;
        }

        [HttpPost("analyze")]
        public ActionResult<ESP_DESIGN_RESULT> AnalyzeAsync([FromBody] ESP_DESIGN_PROPERTIES request)
        {
            return ExecuteDesignOperation("analyze", request, DesignPump);
        }

        [HttpPost("system-curve")]
        public ActionResult<ESP_DESIGN_RESULT> GetSystemCurveAsync([FromBody] ESP_DESIGN_PROPERTIES request)
        {
            return ExecuteDesignOperation("system-curve", request, DesignPump);
        }

        [HttpPost("select")]
        public ActionResult<ESP_DESIGN_RESULT> SelectPumpAsync([FromBody] ESP_DESIGN_PROPERTIES request)
        {
            return ExecuteDesignOperation("select", request, DesignPump);
        }

        [HttpPost("optimize")]
        public ActionResult<ESP_DESIGN_RESULT> OptimizeAsync([FromBody] ESP_DESIGN_PROPERTIES request)
        {
            return ExecuteDesignOperation("optimize", request, OptimizePump);
        }

        private ActionResult<ESP_DESIGN_RESULT> ExecuteDesignOperation(
            string operation,
            ESP_DESIGN_PROPERTIES request,
            Func<ESP_DESIGN_PROPERTIES, ESP_DESIGN_RESULT> executor)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Pump design request is required." });
            }

            try
            {
                var normalized = NormalizeRequest(request);
                ValidateRequest(normalized);

                var result = executor(normalized);
                StampResult(normalized, result, operation);

                _logger.LogInformation(
                    "Pump performance {Operation} completed for area {AreaId} with desired flow {DesiredFlowRate}",
                    operation,
                    normalized.AREA_ID,
                    normalized.DESIRED_FLOW_RATE);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid pump performance {Operation} request", operation);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Pump performance {Operation} failed", operation);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        private static ESP_DESIGN_RESULT DesignPump(ESP_DESIGN_PROPERTIES request)
        {
            return ESPDesignCalculator.DesignESP(request, useSIUnits: false);
        }

        private static ESP_DESIGN_RESULT OptimizePump(ESP_DESIGN_PROPERTIES request)
        {
            var scenarios = BuildOptimizationCandidates(request);

            return scenarios
                .Select(candidate => ESPDesignCalculator.DesignESP(candidate, useSIUnits: false))
                .OrderByDescending(result => result.SYSTEM_EFFICIENCY ?? 0m)
                .ThenBy(result => result.POWER_CONSUMPTION ?? decimal.MaxValue)
                .First();
        }

        private static List<ESP_DESIGN_PROPERTIES> BuildOptimizationCandidates(ESP_DESIGN_PROPERTIES request)
        {
            var desiredFlowFactors = new[] { 0.9m, 1.0m, 1.1m };
            var settingDepthFactors = new[] { 0.95m, 1.0m, 1.05m };
            var pressureFactors = new[] { 0.9m, 1.0m, 1.1m };

            var scenarios = new List<ESP_DESIGN_PROPERTIES>();

            foreach (var flowFactor in desiredFlowFactors)
            {
                foreach (var settingDepthFactor in settingDepthFactors)
                {
                    foreach (var pressureFactor in pressureFactors)
                    {
                        var candidate = CloneRequest(request);
                        candidate.DESIRED_FLOW_RATE = Math.Max(50m, request.DESIRED_FLOW_RATE * flowFactor);
                        candidate.PUMP_SETTING_DEPTH = Math.Min(request.WELL_DEPTH, Math.Max(500m, request.PUMP_SETTING_DEPTH * settingDepthFactor));
                        candidate.WELLHEAD_PRESSURE = Math.Max(25m, request.WELLHEAD_PRESSURE * pressureFactor);
                        candidate.TOTAL_DYNAMIC_HEAD = Math.Max(250m, candidate.PUMP_SETTING_DEPTH + candidate.WELLHEAD_PRESSURE);
                        scenarios.Add(candidate);
                    }
                }
            }

            return scenarios;
        }

        private static ESP_DESIGN_PROPERTIES NormalizeRequest(ESP_DESIGN_PROPERTIES request)
        {
            var normalized = CloneRequest(request);

            normalized.DESIRED_FLOW_RATE = normalized.DESIRED_FLOW_RATE > 0 ? normalized.DESIRED_FLOW_RATE : 800m;
            normalized.WELL_DEPTH = normalized.WELL_DEPTH > 0 ? normalized.WELL_DEPTH : 8000m;
            normalized.CASING_DIAMETER = normalized.CASING_DIAMETER > 0 ? normalized.CASING_DIAMETER : 7m;
            normalized.TUBING_DIAMETER = normalized.TUBING_DIAMETER > 0 ? normalized.TUBING_DIAMETER : 2.875m;
            normalized.OIL_GRAVITY = normalized.OIL_GRAVITY > 0 ? normalized.OIL_GRAVITY : 35m;
            normalized.WATER_CUT = Math.Clamp(normalized.WATER_CUT, 0m, 1m);
            normalized.GAS_OIL_RATIO = normalized.GAS_OIL_RATIO > 0 ? normalized.GAS_OIL_RATIO : 500m;
            normalized.WELLHEAD_PRESSURE = normalized.WELLHEAD_PRESSURE > 0 ? normalized.WELLHEAD_PRESSURE : 100m;
            normalized.BOTTOM_HOLE_TEMPERATURE = normalized.BOTTOM_HOLE_TEMPERATURE > 0 ? normalized.BOTTOM_HOLE_TEMPERATURE : 580m;
            normalized.GAS_SPECIFIC_GRAVITY = normalized.GAS_SPECIFIC_GRAVITY > 0 ? normalized.GAS_SPECIFIC_GRAVITY : 0.65m;
            normalized.PUMP_SETTING_DEPTH = normalized.PUMP_SETTING_DEPTH > 0 ? normalized.PUMP_SETTING_DEPTH : normalized.WELL_DEPTH * 0.8m;
            normalized.PUMP_SETTING_DEPTH = Math.Min(normalized.PUMP_SETTING_DEPTH, normalized.WELL_DEPTH);
            normalized.TOTAL_DYNAMIC_HEAD = normalized.TOTAL_DYNAMIC_HEAD > 0 ? normalized.TOTAL_DYNAMIC_HEAD : normalized.PUMP_SETTING_DEPTH + normalized.WELLHEAD_PRESSURE;

            return normalized;
        }

        private static void ValidateRequest(ESP_DESIGN_PROPERTIES request)
        {
            if (request.DESIRED_FLOW_RATE <= 0)
                throw new ArgumentException("Desired flow rate must be positive.", nameof(request.DESIRED_FLOW_RATE));

            if (request.WELL_DEPTH <= 0)
                throw new ArgumentException("Well depth must be positive.", nameof(request.WELL_DEPTH));

            if (request.PUMP_SETTING_DEPTH <= 0 || request.PUMP_SETTING_DEPTH > request.WELL_DEPTH)
                throw new ArgumentException("Pump setting depth must be positive and cannot exceed well depth.", nameof(request.PUMP_SETTING_DEPTH));

            if (request.CASING_DIAMETER <= 0 || request.TUBING_DIAMETER <= 0)
                throw new ArgumentException("Tubing and casing diameters must be positive.");
        }

        private static void StampResult(ESP_DESIGN_PROPERTIES request, ESP_DESIGN_RESULT result, string operation)
        {
            var propertiesId = string.IsNullOrWhiteSpace(request.ESP_DESIGN_PROPERTIES_ID)
                ? Guid.NewGuid().ToString("N")
                : request.ESP_DESIGN_PROPERTIES_ID;
            var resultId = string.IsNullOrWhiteSpace(result.ESP_DESIGN_RESULT_ID)
                ? Guid.NewGuid().ToString("N")
                : result.ESP_DESIGN_RESULT_ID;

            request.ESP_DESIGN_PROPERTIES_ID = propertiesId;
            result.ESP_DESIGN_PROPERTIES_ID = propertiesId;
            result.ESP_DESIGN_RESULT_ID = resultId;
            result.AREA_ID = request.AREA_ID;
            result.AREA_TYPE = string.IsNullOrWhiteSpace(request.AREA_TYPE) ? "WELL" : request.AREA_TYPE;
            result.BUSINESS_ASSOCIATE_ID = request.BUSINESS_ASSOCIATE_ID;

            if (result.PERFORMANCE_POINTS == null)
            {
                result.PERFORMANCE_POINTS = new List<ESP_PUMP_POINT>();
            }

            for (var index = 0; index < result.PERFORMANCE_POINTS.Count; index++)
            {
                var point = result.PERFORMANCE_POINTS[index];
                point.ESP_PUMP_POINT_ID = string.IsNullOrWhiteSpace(point.ESP_PUMP_POINT_ID)
                    ? Guid.NewGuid().ToString("N")
                    : point.ESP_PUMP_POINT_ID;
                point.ESP_DESIGN_RESULT_ID = resultId;
                point.POINT_ORDER = index + 1;
                point.AREA_ID = request.AREA_ID;
                point.AREA_TYPE = result.AREA_TYPE;
                point.BUSINESS_ASSOCIATE_ID = request.BUSINESS_ASSOCIATE_ID;
            }

            result.ROW_CHANGED_BY = operation;
            result.ROW_CHANGED_DATE = DateTime.UtcNow;
        }

        private static ESP_DESIGN_PROPERTIES CloneRequest(ESP_DESIGN_PROPERTIES request)
        {
            return new ESP_DESIGN_PROPERTIES
            {
                ESP_DESIGN_PROPERTIES_ID = request.ESP_DESIGN_PROPERTIES_ID,
                DESIRED_FLOW_RATE = request.DESIRED_FLOW_RATE,
                TOTAL_DYNAMIC_HEAD = request.TOTAL_DYNAMIC_HEAD,
                WELL_DEPTH = request.WELL_DEPTH,
                CASING_DIAMETER = request.CASING_DIAMETER,
                TUBING_DIAMETER = request.TUBING_DIAMETER,
                OIL_GRAVITY = request.OIL_GRAVITY,
                WATER_CUT = request.WATER_CUT,
                GAS_OIL_RATIO = request.GAS_OIL_RATIO,
                WELLHEAD_PRESSURE = request.WELLHEAD_PRESSURE,
                BOTTOM_HOLE_TEMPERATURE = request.BOTTOM_HOLE_TEMPERATURE,
                GAS_SPECIFIC_GRAVITY = request.GAS_SPECIFIC_GRAVITY,
                PUMP_SETTING_DEPTH = request.PUMP_SETTING_DEPTH,
                AREA_ID = request.AREA_ID,
                AREA_TYPE = request.AREA_TYPE,
                BUSINESS_ASSOCIATE_ID = request.BUSINESS_ASSOCIATE_ID
            };
        }
    }
}