using Beep.OilandGas.OilProperties.Constants;
using Xunit;

namespace Beep.OilandGas.OilProperties.Tests;

public class OilPropertyUnitsTests
{
    [Fact]
    public void RankineToFahrenheit_520R_is_about_60F()
    {
        decimal f = OilPropertyUnits.RankineToFahrenheit(520m);
        Assert.Equal(60.33m, decimal.Round(f, 2));
    }
}
