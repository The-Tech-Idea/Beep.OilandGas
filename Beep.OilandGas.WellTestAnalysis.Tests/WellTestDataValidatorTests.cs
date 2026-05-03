using Beep.OilandGas.Models.Data.WellTestAnalysis;
using Beep.OilandGas.WellTestAnalysis.Exceptions;
using Beep.OilandGas.WellTestAnalysis.Validation;
using Xunit;

namespace Beep.OilandGas.WellTestAnalysis.Tests;

public class WellTestDataValidatorTests
{
    private static WELL_TEST_DATA MinimalValidData() => new()
    {
        FLOW_RATE = 1000m,
        WELLBORE_RADIUS = 0.25m,
        FORMATION_THICKNESS = 50m,
        POROSITY = 0.20m,
        TOTAL_COMPRESSIBILITY = 1e-5m,
        OIL_VISCOSITY = 1.5m,
        OIL_FORMATION_VOLUME_FACTOR = 1.2m,
        TEST_TYPE = WellTestType.BuildUp.ToString(),
        PRODUCTION_TIME = 720m,
        RESERVOIR_TEMPERATURE = 200m,
        Time = [0.01, 0.1, 0.5, 1, 2, 5, 10, 20, 50, 100],
        Pressure = [3000, 2990, 2980, 2970, 2960, 2950, 2940, 2930, 2920, 2910]
    };

    [Fact]
    public void Validate_accepts_minimal_build_up_dataset()
    {
        var ex = Record.Exception(() => WellTestDataValidator.Validate(MinimalValidData()));
        Assert.Null(ex);
    }

    [Theory]
    [InlineData("BuildUp")]
    [InlineData(WellTestAnalysisWellKnown.AnalysisClassification.BuildUp)]
    [InlineData("buildup")]
    public void Validate_accepts_case_insensitive_build_up_test_type_for_production_time_rule(string testType)
    {
        var data = MinimalValidData();
        data.TEST_TYPE = testType;

        var ex = Record.Exception(() => WellTestDataValidator.Validate(data));
        Assert.Null(ex);
    }

    [Fact]
    public void Validate_throws_when_wellbore_radius_null()
    {
        var data = MinimalValidData();
        data.WELLBORE_RADIUS = null;

        var ex = Assert.Throws<InvalidWellTestDataException>(() => WellTestDataValidator.Validate(data));
        Assert.Equal(nameof(data.WELLBORE_RADIUS), ex.ParameterName);
    }

    [Fact]
    public void Validate_throws_when_time_not_strictly_increasing()
    {
        var data = MinimalValidData();
        data.Time = [1, 2, 2, 4];

        var ex = Assert.Throws<InvalidWellTestDataException>(() => WellTestDataValidator.Validate(data));
        Assert.Equal(nameof(data.Time), ex.ParameterName);
    }

    [Fact]
    public void Validate_throws_when_build_up_and_production_time_missing()
    {
        var data = MinimalValidData();
        data.PRODUCTION_TIME = null;

        var ex = Assert.Throws<InvalidWellTestDataException>(() => WellTestDataValidator.Validate(data));
        Assert.Equal(nameof(data.PRODUCTION_TIME), ex.ParameterName);
    }

    [Fact]
    public void Validate_throws_when_build_up_and_production_time_non_positive()
    {
        var data = MinimalValidData();
        data.PRODUCTION_TIME = 0m;

        var ex = Assert.Throws<InvalidWellTestDataException>(() => WellTestDataValidator.Validate(data));
        Assert.Equal(nameof(data.PRODUCTION_TIME), ex.ParameterName);
    }
}
