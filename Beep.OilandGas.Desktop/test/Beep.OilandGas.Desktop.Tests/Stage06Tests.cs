using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Common;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Eor = Beep.OilandGas.EnhancedRecovery.Data;
using Lc = Beep.OilandGas.Models.Data.LifeCycle;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// The stage-level check for STG-06 (SG-06-8): every calculation this stage saves records its inputs, its model and the
/// actor who saved it. The check is structural on purpose - it fails if a future change drops one of those columns or
/// stops writing it - and each feature's own gateway test proves the values that are written.
/// </summary>
public sealed class Stage06Tests
{
    [Fact]
    public void Every_saved_calculation_records_its_inputs_its_model_and_its_actor()
    {
        // RES-01: the typed inputs, the converted pair the engine used, both correlations and the actor.
        Carries<GAS_PROPERTY_RUN>("RUN_ID", "PRESSURE_INPUT", "PRESSURE_PSIA", "TEMPERATURE_INPUT",
            "TEMPERATURE_R", "Z_CORRELATION", "VISCOSITY_CORRELATION", "ACTOR", "RECORDED_AT");

        // RES-02: the inputs, the correlation and the actor.
        Carries<OIL_PROPERTY_RESULT>("CALCULATION_ID", "API_GRAVITY", "GAS_SPECIFIC_GRAVITY", "PRESSURE",
            "TEMPERATURE", "SOLUTION_GOR", "CORRELATION_METHOD", "ROW_CREATED_BY", "CALCULATION_DATE");

        // RES-03: the model, the feed, the iteration count and the actor.
        Carries<FLASH_CALCULATION_RESULT>("FLASH_CALCULATION_RESULT_ID", "PRESSURE", "TEMPERATURE",
            "FEED_COMPOSITION_JSON", "EOS_CODE", "ITERATIONS", "STATUS", "ROW_CREATED_BY", "CALCULATION_DATE");

        // RES-04: the pool, the inputs, the counts and the actor; each candidate keeps the rule that decided it.
        Carries<Eor.EOR_SCREENING_RUN>("RUN_ID", "POOL_ID", "API_GRAVITY", "VISCOSITY_CP",
            "DEPTH_FT", "CHOSEN_METHOD", "ACTOR", "RECORDED_AT");
        Carries<Eor.EOR_SCREENING_CANDIDATE>("CANDIDATE_ID", "RUN_ID", "METHOD_CODE", "STATE",
            "DECIDING_RULE", "INCREMENTAL_RECOVERY");

        // RES-05: the pool, the category, the volumes with their unit, the revision, the classifier and the actor.
        Carries<Lc.RESERVE_ENTRY>("ENTRY_ID", "POOL_ID", "CATEGORY", "GROSS_VOLUME", "NET_VOLUME",
            "VOLUME_OUOM", "EFFECTIVE_DATE", "REVISION", "CLASSIFIER", "ACTOR", "RECORDED_AT");

        // RES-06: the well, the fluid, the model, the parameters, the exclusions and the actor.
        Carries<DECLINE_FIT>("FIT_ID", "WELL_ID", "FLUID", "MODEL", "QI", "DI_PER_DAY", "B",
            "POINTS_USED", "POINTS_EXCLUDED", "ACTOR", "RECORDED_AT");

        // RES-07: the entity, the method, the source fit, the version and the actor.
        Carries<PRODUCTION_FORECAST>("PRODUCTION_FORECAST_ID", "WELL_UWI", "FIELD_ID", "FORECAST_TYPE",
            "SOURCE_FIT_ID", "STATUS", "VERSION", "ACTOR", "RECORDED_AT");
        Carries<PRODUCTION_FORECAST_POINT>("FORECAST_POINT_ID", "FORECAST_ID", "FORECAST_DATE", "OIL_RATE",
            "GAS_RATE", "WATER_RATE", "MONTHLY_VOLUME", "CUMULATIVE_VOLUME");
    }

    [Fact]
    public void Every_data_service_this_stage_added_writes_the_actor_on_its_save_path()
    {
        // SG-06-8 as a guard on the code, not only on the model: the actor column exists and the service fills it.
        var expectations = new (string File, string ActorColumn)[]
        {
            ("OgGasProperties.cs", "ACTOR = actor.Trim()"),
            ("OgOilProperties.cs", "ROW_CREATED_BY = actor.Trim()"),
            ("OgFlash.cs", "ROW_CREATED_BY = actor.Trim()"),
            ("OgEorScreening.cs", "ACTOR = actor.Trim()"),
            ("OgReserves.cs", "ACTOR = actor.Trim()"),
            ("OgDecline.cs", "ACTOR = actor.Trim()"),
            ("OgForecast.cs", "ACTOR = actor.Trim()"),
        };

        var sources = SourceRoot.AllSources();

        foreach (var (file, actorColumn) in expectations)
        {
            var path = sources.Single(candidate => Path.GetFileName(candidate) == file);
            var text = File.ReadAllText(path);

            Assert.Contains(actorColumn, text, StringComparison.Ordinal);
            Assert.Contains("actor", text, StringComparison.Ordinal);
            Assert.Contains("CodeActorRequired", text, StringComparison.Ordinal);
        }
    }

    private static void Carries<T>(params string[] properties)
    {
        var missing = properties
            .Where(property => typeof(T).GetProperty(property) is null)
            .ToList();

        Assert.True(
            missing.Count == 0,
            $"{typeof(T).Name} does not carry {string.Join(", ", missing)}, so its saved record cannot be reproduced");
    }
}
