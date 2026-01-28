using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class ProvedReserves : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the property identifier.
        /// </summary>
        private string PropertyIdValue = string.Empty;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the proved developed oil reserves in barrels.
        /// </summary>
        private decimal ProvedDevelopedOilReservesValue;

        public decimal ProvedDevelopedOilReserves

        {

            get { return this.ProvedDevelopedOilReservesValue; }

            set { SetProperty(ref ProvedDevelopedOilReservesValue, value); }

        }

        /// <summary>
        /// Gets or sets the proved undeveloped oil reserves in barrels.
        /// </summary>
        private decimal ProvedUndevelopedOilReservesValue;

        public decimal ProvedUndevelopedOilReserves

        {

            get { return this.ProvedUndevelopedOilReservesValue; }

            set { SetProperty(ref ProvedUndevelopedOilReservesValue, value); }

        }

        /// <summary>
        /// Gets the total proved oil reserves in barrels.
        /// </summary>
        public decimal TotalProvedOilReserves => ProvedDevelopedOilReserves + ProvedUndevelopedOilReserves;

        /// <summary>
        /// Gets or sets the proved developed gas reserves in MCF.
        /// </summary>
        private decimal ProvedDevelopedGasReservesValue;

        public decimal ProvedDevelopedGasReserves

        {

            get { return this.ProvedDevelopedGasReservesValue; }

            set { SetProperty(ref ProvedDevelopedGasReservesValue, value); }

        }

        /// <summary>
        /// Gets or sets the proved undeveloped gas reserves in MCF.
        /// </summary>
        private decimal ProvedUndevelopedGasReservesValue;

        public decimal ProvedUndevelopedGasReserves

        {

            get { return this.ProvedUndevelopedGasReservesValue; }

            set { SetProperty(ref ProvedUndevelopedGasReservesValue, value); }

        }

        /// <summary>
        /// Gets the total proved gas reserves in MCF.
        /// </summary>
        public decimal TotalProvedGasReserves => ProvedDevelopedGasReserves + ProvedUndevelopedGasReserves;

        /// <summary>
        /// Gets or sets the reserve date.
        /// </summary>
        private DateTime ReserveDateValue;

        public DateTime ReserveDate

        {

            get { return this.ReserveDateValue; }

            set { SetProperty(ref ReserveDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the oil price used for reserve valuation ($/barrel).
        /// </summary>
        private decimal OilPriceValue;

        public decimal OilPrice

        {

            get { return this.OilPriceValue; }

            set { SetProperty(ref OilPriceValue, value); }

        }

        /// <summary>
        /// Gets or sets the gas price used for reserve valuation ($/MCF).
        /// </summary>
        private decimal GasPriceValue;

        public decimal GasPrice

        {

            get { return this.GasPriceValue; }

            set { SetProperty(ref GasPriceValue, value); }

        }
        private object TotalProvedReservesBOEValue;

        public object TotalProvedReservesBOE

        {

            get { return this.TotalProvedReservesBOEValue; }

            set { SetProperty(ref TotalProvedReservesBOEValue, value); }

        }
    }
}
