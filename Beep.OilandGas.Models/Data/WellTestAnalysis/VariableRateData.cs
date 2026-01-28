using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    public class VariableRateData : ModelEntityBase
    {
        private List<ProductionHistory> ProductionHistoryValue = new();

        public List<ProductionHistory> ProductionHistory

        {

            get { return this.ProductionHistoryValue; }

            set { SetProperty(ref ProductionHistoryValue, value); }

        }
        private List<PRESSURE_TIME_POINT> PressureDataValue = new();

        public List<PRESSURE_TIME_POINT> PressureData

        {

            get { return this.PressureDataValue; }

            set { SetProperty(ref PressureDataValue, value); }

        }
    }
}
