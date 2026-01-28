
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public enum ExchangeContractType
    {
        /// <summary>
        /// Physical exchange (crude for crude).
        /// </summary>
        PhysicalExchange,

        /// <summary>
        /// Buy/sell exchange (buy at one location, sell at another).
        /// </summary>
        BuySellExchange,

        /// <summary>
        /// Multi-party exchange.
        /// </summary>
        MultiPartyExchange,

        /// <summary>
        /// Time exchange (exchange volumes across time periods).
        /// </summary>
        TimeExchange
    }
}
