using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// A test that needs a real database and is therefore opt-in.
/// </summary>
/// <remarks>
/// <para>
/// The previous pattern was a <c>[Fact]</c> whose body began with <c>if (env != "1") return;</c>. xUnit reports that as
/// **passed**, so a suite of 1,620 passing tests could hide a hundred that never ran - exactly how a broken reference
/// table survived. A conditional skip is reported as **skipped**, which is the truth, and the default run's numbers
/// say how many were not run.
/// </para>
/// <para>
/// Run them with <c>OG_RUN_SETUP_INTEGRATION=1 dotnet test …</c>.
/// </para>
/// </remarks>
/// <summary>
/// A test that runs the whole catalogue's module apply, which is currently blocked inside the migration engine.
/// </summary>
/// <remarks>
/// It needs both <c>OG_RUN_SETUP_INTEGRATION=1</c> and <c>OG_RUN_SETUP_FULL=1</c>, and is reported as **skipped** until
/// then - a body that returns early would report passed, which is how the gate it is meant to prove went unnoticed.
/// </remarks>
public sealed class FullSetupFactAttribute : FactAttribute
{
    /// <summary>The variable that turns the full-catalogue run on.</summary>
    public const string Variable = "OG_RUN_SETUP_FULL";

    public FullSetupFactAttribute()
    {
        if (Environment.GetEnvironmentVariable(IntegrationFactAttribute.Variable) != "1")
        {
            Skip = $"Needs a real database: run with {IntegrationFactAttribute.Variable}=1.";
            return;
        }

        if (Environment.GetEnvironmentVariable(Variable) != "1")
        {
            Skip = $"Full-catalogue apply, currently blocked by the migration engine: run with {Variable}=1 to attempt it.";
        }
    }
}

public sealed class IntegrationFactAttribute : FactAttribute
{
    /// <summary>The variable that turns the database-backed tests on.</summary>
    public const string Variable = "OG_RUN_SETUP_INTEGRATION";

    public IntegrationFactAttribute()
    {
        if (Environment.GetEnvironmentVariable(Variable) != "1")
        {
            Skip = $"Needs a real database: run with {Variable}=1.";
        }
    }
}
