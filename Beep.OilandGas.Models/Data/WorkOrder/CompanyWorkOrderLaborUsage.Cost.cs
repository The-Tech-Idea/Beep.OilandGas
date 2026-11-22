using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.WorkOrder;

/// <summary>
/// The cost columns of a labour report (WRK-02), added to the table that already holds the recorded hours.
/// </summary>
/// <remarks>
/// The owner's labour table records who worked, on what duty, from and to when, and the quantity - and its own remark says
/// "no wage, valuation, currency or journal posting is inferred". A work order's actual cost must equal the sum of its labour
/// and material lines (WRK-02 AC-2), so the rate a line is costed at has to be recorded somewhere: these two columns are the
/// plan's own contract, additive, leaving every recorded labour report readable as it was.
/// </remarks>
public partial class WORK_ORDER_LABOR_USAGE
{
    /// <summary>The rate one unit of this line is costed at, as recorded for the work order.</summary>
    public decimal? UNIT_COST
    {
        get => UnitCostValue;
        set => SetProperty(ref UnitCostValue, value);
    }

    private decimal? UnitCostValue;

    /// <summary>The currency of that rate - a cost without its currency is not an amount.</summary>
    [MaxLength(8)]
    public string? CURRENCY_CODE
    {
        get => CurrencyValue;
        set => SetProperty(ref CurrencyValue, value);
    }

    private string? CurrencyValue;
}
