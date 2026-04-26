using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class PriceScenario : ModelEntityBase
    {
        /// <summary>
        /// Commodity price for this scenario
        /// </summary>
        private double PriceValue;

        public double Price

        {

            get { return this.PriceValue; }

            set { SetProperty(ref PriceValue, value); }

        }

        /// <summary>
        /// Net Present Value at this price
        /// </summary>
        private double NPVValue;

        public double NPV

        {

            get { return this.NPVValue; }

            set { SetProperty(ref NPVValue, value); }

        }

        /// <summary>
        /// Internal Rate of Return at this price
        /// </summary>
        private double IRRValue;

        public double IRR

        {

            get { return this.IRRValue; }

            set { SetProperty(ref IRRValue, value); }

        }

        /// <summary>
        /// Indicates if this is the breakeven price point
        /// </summary>
        private bool IsBreakevenValue;

        public bool IsBreakeven

        {

            get { return this.IsBreakevenValue; }

            set { SetProperty(ref IsBreakevenValue, value); }

        }

        // ── Best-practice additions (SPE PRMS §6.2 sensitivity / price deck) ──────

        /// <summary>Scenario label: LOW / BASE / HIGH per PRMS §6.2 price-deck convention</summary>
        private string ScenarioNameValue = "BASE";
        public string ScenarioName
        {
            get { return this.ScenarioNameValue; }
            set { SetProperty(ref ScenarioNameValue, value); }
        }

        /// <summary>Commodity to which this price applies: OIL / GAS / NGL / CONDENSATE</summary>
        private string CommodityTypeValue = "OIL";
        public string CommodityType
        {
            get { return this.CommodityTypeValue; }
            set { SetProperty(ref CommodityTypeValue, value); }
        }

        /// <summary>Unit for the price (e.g. USD/BBL, USD/MCF, USD/MMBTU)</summary>
        private string PriceUnitValue = "USD/BBL";
        public string PriceUnit
        {
            get { return this.PriceUnitValue; }
            set { SetProperty(ref PriceUnitValue, value); }
        }
    }
}
