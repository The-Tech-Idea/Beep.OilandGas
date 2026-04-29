namespace Beep.OilandGas.ProductionAccounting.Constants
{
    /// <summary>User-facing validation text for reserve and reserve-cashflow operations.</summary>
    public static class ReserveAccountingServiceExceptionMessages
    {
        public const string PropertyIdRequiredForReserves = "PROPERTY_ID is required for reserves";
        public const string PropertyIdRequired = "PROPERTY_ID is required";
        public const string ReserveDateRequired = "RESERVE_DATE is required";
        public const string PropertyIdRequiredForCashflow = "PROPERTY_ID is required for cashflow";
        public const string PeriodEndDateRequiredForCashflow = "PERIOD_END_DATE is required for cashflow";
    }

    /// <summary>Simplified NPV helpers when <c>RESERVE_CASHFLOW.DISCOUNT_RATE</c> is missing (not a market curve).</summary>
    public static class ReserveCashflowPresentValueDefaults
    {
        /// <summary>Default annual discount rate as unit fraction when row omits <c>DISCOUNT_RATE</c> (10%).</summary>
        public const decimal DefaultAnnualDiscountRateFraction = 0.10m;

        /// <summary>Days per year for rough year-fraction from period spacing (calendar simplification).</summary>
        public const decimal DaysPerYearForYearFraction = 365m;
    }
}
