using System;
using Beep.OilandGas.CompressorAnalysis.Calculations;
using Beep.OilandGas.CompressorAnalysis.Constants;
using Beep.OilandGas.CompressorAnalysis.Exceptions;
using Beep.OilandGas.CompressorAnalysis.Validation;
using Beep.OilandGas.Models.Data.CompressorAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CompressorController : ControllerBase
    {
        private readonly ILogger<CompressorController> _logger;

        public CompressorController(ILogger<CompressorController> logger)
        {
            _logger = logger;
        }

        [HttpPost("analyze")]
        public ActionResult<COMPRESSOR_POWER_RESULT> Analyze([FromBody] COMPRESSOR_OPERATING_CONDITIONS request)
        {
            return ExecuteWithHandling("analyze", () =>
            {
                var conditions = NormalizeOperatingConditions(request, CompressorConstants.StandardPolytropicEfficiency);
                var properties = BuildDefaultCentrifugalProperties(conditions);
                CompressorValidator.ValidateCentrifugalCompressorProperties(properties);
                return StampResult(CentrifugalCompressorCalculator.CalculatePower(properties), conditions);
            });
        }

        [HttpPost("power")]
        public ActionResult<COMPRESSOR_POWER_RESULT> CalculatePower([FromBody] COMPRESSOR_OPERATING_CONDITIONS request)
        {
            return ExecuteWithHandling("power", () =>
            {
                var conditions = NormalizeOperatingConditions(request, CompressorConstants.StandardPolytropicEfficiency);
                var properties = BuildDefaultCentrifugalProperties(conditions);
                CompressorValidator.ValidateCentrifugalCompressorProperties(properties);
                return StampResult(CentrifugalCompressorCalculator.CalculatePower(properties), conditions);
            });
        }

        [HttpPost("design/centrifugal")]
        public ActionResult<COMPRESSOR_POWER_RESULT> DesignCentrifugal([FromBody] CENTRIFUGAL_COMPRESSOR_PROPERTIES request)
        {
            return ExecuteWithHandling("design/centrifugal", () =>
            {
                var properties = NormalizeCentrifugalProperties(request);
                CompressorValidator.ValidateCentrifugalCompressorProperties(properties);
                return StampResult(CentrifugalCompressorCalculator.CalculatePower(properties), properties.OPERATING_CONDITIONS);
            });
        }

        [HttpPost("design/reciprocating")]
        public ActionResult<COMPRESSOR_POWER_RESULT> DesignReciprocating([FromBody] RECIPROCATING_COMPRESSOR_PROPERTIES request)
        {
            return ExecuteWithHandling("design/reciprocating", () =>
            {
                var properties = NormalizeReciprocatingProperties(request);
                CompressorValidator.ValidateReciprocatingCompressorProperties(properties);
                return StampResult(ReciprocatingCompressorCalculator.CalculatePower(properties), properties.OPERATING_CONDITIONS);
            });
        }

        private ActionResult<COMPRESSOR_POWER_RESULT> ExecuteWithHandling(string operation, Func<COMPRESSOR_POWER_RESULT> action)
        {
            try
            {
                return Ok(action());
            }
            catch (CompressorException ex)
            {
                _logger.LogWarning(ex, "Legacy compressor compatibility route {Operation} rejected invalid input", operation);
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Legacy compressor compatibility route {Operation} received invalid arguments", operation);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Legacy compressor compatibility route {Operation} failed", operation);
                return StatusCode(500, "Compressor calculation failed");
            }
        }

        private static COMPRESSOR_OPERATING_CONDITIONS NormalizeOperatingConditions(
            COMPRESSOR_OPERATING_CONDITIONS request,
            decimal defaultCompressorEfficiency)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return new COMPRESSOR_OPERATING_CONDITIONS
            {
                COMPRESSOR_OPERATING_CONDITIONS_ID = string.IsNullOrWhiteSpace(request.COMPRESSOR_OPERATING_CONDITIONS_ID)
                    ? Guid.NewGuid().ToString("N")
                    : request.COMPRESSOR_OPERATING_CONDITIONS_ID,
                SUCTION_PRESSURE = request.SUCTION_PRESSURE,
                DISCHARGE_PRESSURE = request.DISCHARGE_PRESSURE,
                SUCTION_TEMPERATURE = request.SUCTION_TEMPERATURE,
                DISCHARGE_TEMPERATURE = request.DISCHARGE_TEMPERATURE > 0
                    ? request.DISCHARGE_TEMPERATURE
                    : request.SUCTION_TEMPERATURE,
                GAS_FLOW_RATE = request.GAS_FLOW_RATE,
                GAS_SPECIFIC_GRAVITY = request.GAS_SPECIFIC_GRAVITY,
                GAS_MOLECULAR_WEIGHT = request.GAS_MOLECULAR_WEIGHT > 0
                    ? request.GAS_MOLECULAR_WEIGHT
                    : request.GAS_SPECIFIC_GRAVITY * CompressorConstants.AirMolecularWeight,
                COMPRESSOR_EFFICIENCY = request.COMPRESSOR_EFFICIENCY > 0 && request.COMPRESSOR_EFFICIENCY <= 1
                    ? request.COMPRESSOR_EFFICIENCY
                    : defaultCompressorEfficiency,
                MECHANICAL_EFFICIENCY = request.MECHANICAL_EFFICIENCY > 0 && request.MECHANICAL_EFFICIENCY <= 1
                    ? request.MECHANICAL_EFFICIENCY
                    : CompressorConstants.StandardMECHANICAL_EFFICIENCY
            };
        }

        private static CENTRIFUGAL_COMPRESSOR_PROPERTIES BuildDefaultCentrifugalProperties(COMPRESSOR_OPERATING_CONDITIONS conditions)
        {
            return new CENTRIFUGAL_COMPRESSOR_PROPERTIES
            {
                COMPRESSOR_OPERATING_CONDITIONS_ID = conditions.COMPRESSOR_OPERATING_CONDITIONS_ID,
                OPERATING_CONDITIONS = conditions,
                POLYTROPIC_EFFICIENCY = conditions.COMPRESSOR_EFFICIENCY,
                SPECIFIC_HEAT_RATIO = CompressorConstants.StandardSpecificHeatRatio,
                NUMBER_OF_STAGES = 1,
                SPEED = 10000m
            };
        }

        private static CENTRIFUGAL_COMPRESSOR_PROPERTIES NormalizeCentrifugalProperties(CENTRIFUGAL_COMPRESSOR_PROPERTIES request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var conditions = NormalizeOperatingConditions(request.OPERATING_CONDITIONS, CompressorConstants.StandardPolytropicEfficiency);
            return new CENTRIFUGAL_COMPRESSOR_PROPERTIES
            {
                CENTRIFUGAL_COMPRESSOR_PROPERTIES_ID = string.IsNullOrWhiteSpace(request.CENTRIFUGAL_COMPRESSOR_PROPERTIES_ID)
                    ? Guid.NewGuid().ToString("N")
                    : request.CENTRIFUGAL_COMPRESSOR_PROPERTIES_ID,
                COMPRESSOR_OPERATING_CONDITIONS_ID = conditions.COMPRESSOR_OPERATING_CONDITIONS_ID,
                OPERATING_CONDITIONS = conditions,
                POLYTROPIC_EFFICIENCY = request.POLYTROPIC_EFFICIENCY > 0 && request.POLYTROPIC_EFFICIENCY <= 1
                    ? request.POLYTROPIC_EFFICIENCY
                    : conditions.COMPRESSOR_EFFICIENCY,
                SPECIFIC_HEAT_RATIO = request.SPECIFIC_HEAT_RATIO > 1m
                    ? request.SPECIFIC_HEAT_RATIO
                    : CompressorConstants.StandardSpecificHeatRatio,
                NUMBER_OF_STAGES = request.NUMBER_OF_STAGES > 0
                    ? request.NUMBER_OF_STAGES
                    : 1,
                SPEED = request.SPEED > 0
                    ? request.SPEED
                    : 10000m
            };
        }

        private static RECIPROCATING_COMPRESSOR_PROPERTIES NormalizeReciprocatingProperties(RECIPROCATING_COMPRESSOR_PROPERTIES request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var conditions = NormalizeOperatingConditions(request.OPERATING_CONDITIONS, CompressorConstants.StandardReciprocatingEfficiency);
            return new RECIPROCATING_COMPRESSOR_PROPERTIES
            {
                RECIPROCATING_COMPRESSOR_PROPERTIES_ID = string.IsNullOrWhiteSpace(request.RECIPROCATING_COMPRESSOR_PROPERTIES_ID)
                    ? Guid.NewGuid().ToString("N")
                    : request.RECIPROCATING_COMPRESSOR_PROPERTIES_ID,
                COMPRESSOR_OPERATING_CONDITIONS_ID = conditions.COMPRESSOR_OPERATING_CONDITIONS_ID,
                OPERATING_CONDITIONS = conditions,
                CYLINDER_DIAMETER = request.CYLINDER_DIAMETER > 0
                    ? request.CYLINDER_DIAMETER
                    : 10m,
                STROKE_LENGTH = request.STROKE_LENGTH > 0
                    ? request.STROKE_LENGTH
                    : 12m,
                ROTATIONAL_SPEED = request.ROTATIONAL_SPEED > 0
                    ? request.ROTATIONAL_SPEED
                    : 300m,
                NUMBER_OF_CYLINDERS = request.NUMBER_OF_CYLINDERS > 0
                    ? request.NUMBER_OF_CYLINDERS
                    : 2,
                VOLUMETRIC_EFFICIENCY = request.VOLUMETRIC_EFFICIENCY > 0 && request.VOLUMETRIC_EFFICIENCY <= 1
                    ? request.VOLUMETRIC_EFFICIENCY
                    : CompressorConstants.StandardVolumetricEfficiency,
                CLEARANCE_FACTOR = request.CLEARANCE_FACTOR >= 0 && request.CLEARANCE_FACTOR <= 1
                    ? request.CLEARANCE_FACTOR
                    : CompressorConstants.StandardClearanceFactor
            };
        }

        private static COMPRESSOR_POWER_RESULT StampResult(
            COMPRESSOR_POWER_RESULT result,
            COMPRESSOR_OPERATING_CONDITIONS conditions)
        {
            result.COMPRESSOR_OPERATING_CONDITIONS_ID = conditions.COMPRESSOR_OPERATING_CONDITIONS_ID;
            if (string.IsNullOrWhiteSpace(result.COMPRESSOR_POWER_RESULT_ID))
            {
                result.COMPRESSOR_POWER_RESULT_ID = Guid.NewGuid().ToString("N");
            }

            return result;
        }
    }
}