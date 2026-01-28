using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class SeparatorSimulation : ModelEntityBase
    {
        private string SimulationIdValue = string.Empty;

        public string SimulationId

        {

            get { return this.SimulationIdValue; }

            set { SetProperty(ref SimulationIdValue, value); }

        }
        private string CompositionIdValue = string.Empty;

        public string CompositionId

        {

            get { return this.CompositionIdValue; }

            set { SetProperty(ref CompositionIdValue, value); }

        }
        private DateTime SimulationDateValue;

        public DateTime SimulationDate

        {

            get { return this.SimulationDateValue; }

            set { SetProperty(ref SimulationDateValue, value); }

        }
        private decimal InletPressureValue;

        public decimal InletPressure

        {

            get { return this.InletPressureValue; }

            set { SetProperty(ref InletPressureValue, value); }

        }
        private decimal InletTemperatureValue;

        public decimal InletTemperature

        {

            get { return this.InletTemperatureValue; }

            set { SetProperty(ref InletTemperatureValue, value); }

        }
        private decimal SeparatorPressureValue;

        public decimal SeparatorPressure

        {

            get { return this.SeparatorPressureValue; }

            set { SetProperty(ref SeparatorPressureValue, value); }

        }
        private decimal SeparatorTemperatureValue;

        public decimal SeparatorTemperature

        {

            get { return this.SeparatorTemperatureValue; }

            set { SetProperty(ref SeparatorTemperatureValue, value); }

        }
        private decimal GasOilRatioValue;

        public decimal GasOilRatio

        {

            get { return this.GasOilRatioValue; }

            set { SetProperty(ref GasOilRatioValue, value); }

        }
        private decimal LiquidRecoveryValue;

        public decimal LiquidRecovery

        {

            get { return this.LiquidRecoveryValue; }

            set { SetProperty(ref LiquidRecoveryValue, value); }

        }
        private decimal GasRecoveryValue;

        public decimal GasRecovery

        {

            get { return this.GasRecoveryValue; }

            set { SetProperty(ref GasRecoveryValue, value); }

        }
        private List<SeparatorStage> StagesValue = new();

        public List<SeparatorStage> Stages

        {

            get { return this.StagesValue; }

            set { SetProperty(ref StagesValue, value); }

        }
    }
}
