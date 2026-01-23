using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    /// <summary>
    /// DTO for gas lift design.
    /// </summary>
    public class GasLiftDesign : ModelEntityBase
    {
        private string DesignIdValue = string.Empty;

        public string DesignId

        {

            get { return this.DesignIdValue; }

            set { SetProperty(ref DesignIdValue, value); }

        }
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private DateTime DesignDateValue;

        public DateTime DesignDate

        {

            get { return this.DesignDateValue; }

            set { SetProperty(ref DesignDateValue, value); }

        }
        private decimal GasInjectionPressureValue;

        public decimal GasInjectionPressure

        {

            get { return this.GasInjectionPressureValue; }

            set { SetProperty(ref GasInjectionPressureValue, value); }

        }
        private int NumberOfValvesValue;

        public int NumberOfValves

        {

            get { return this.NumberOfValvesValue; }

            set { SetProperty(ref NumberOfValvesValue, value); }

        }
        private List<GasLiftValve> ValvesValue = new();

        public List<GasLiftValve> Valves

        {

            get { return this.ValvesValue; }

            set { SetProperty(ref ValvesValue, value); }

        }
        private decimal TotalGasInjectionRateValue;

        public decimal TotalGasInjectionRate

        {

            get { return this.TotalGasInjectionRateValue; }

            set { SetProperty(ref TotalGasInjectionRateValue, value); }

        }
        private decimal ExpectedProductionRateValue;

        public decimal ExpectedProductionRate

        {

            get { return this.ExpectedProductionRateValue; }

            set { SetProperty(ref ExpectedProductionRateValue, value); }

        }
        private decimal SystemEfficiencyValue;

        public decimal SystemEfficiency

        {

            get { return this.SystemEfficiencyValue; }

            set { SetProperty(ref SystemEfficiencyValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }

    /// <summary>
    /// DTO for gas lift valve.
    /// </summary>
    public class GasLiftValve : ModelEntityBase
    {
        private string ValveIdValue = string.Empty;

        public string ValveId

        {

            get { return this.ValveIdValue; }

            set { SetProperty(ref ValveIdValue, value); }

        }
        private decimal DepthValue;

        public decimal Depth

        {

            get { return this.DepthValue; }

            set { SetProperty(ref DepthValue, value); }

        }
        private decimal PortSizeValue;

        public decimal PortSize

        {

            get { return this.PortSizeValue; }

            set { SetProperty(ref PortSizeValue, value); }

        }
        private decimal OpeningPressureValue;

        public decimal OpeningPressure

        {

            get { return this.OpeningPressureValue; }

            set { SetProperty(ref OpeningPressureValue, value); }

        }
        private decimal ClosingPressureValue;

        public decimal ClosingPressure

        {

            get { return this.ClosingPressureValue; }

            set { SetProperty(ref ClosingPressureValue, value); }

        }
        private decimal GasInjectionRateValue;

        public decimal GasInjectionRate

        {

            get { return this.GasInjectionRateValue; }

            set { SetProperty(ref GasInjectionRateValue, value); }

        }
    }

    /// <summary>
    /// DTO for gas lift performance.
    /// </summary>
    public class GasLiftPerformance : ModelEntityBase
    {
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private DateTime PerformanceDateValue;

        public DateTime PerformanceDate

        {

            get { return this.PerformanceDateValue; }

            set { SetProperty(ref PerformanceDateValue, value); }

        }
        private decimal GasInjectionRateValue;

        public decimal GasInjectionRate

        {

            get { return this.GasInjectionRateValue; }

            set { SetProperty(ref GasInjectionRateValue, value); }

        }
        private decimal ProductionRateValue;

        public decimal ProductionRate

        {

            get { return this.ProductionRateValue; }

            set { SetProperty(ref ProductionRateValue, value); }

        }
        private decimal GasLiquidRatioValue;

        public decimal GasLiquidRatio

        {

            get { return this.GasLiquidRatioValue; }

            set { SetProperty(ref GasLiquidRatioValue, value); }

        }
        private decimal EfficiencyValue;

        public decimal Efficiency

        {

            get { return this.EfficiencyValue; }

            set { SetProperty(ref EfficiencyValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }
}







