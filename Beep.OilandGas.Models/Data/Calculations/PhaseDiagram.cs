using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class PhaseDiagram : ModelEntityBase
    {
        private string DiagramIdValue = string.Empty;

        public string DiagramId

        {

            get { return this.DiagramIdValue; }

            set { SetProperty(ref DiagramIdValue, value); }

        }
        private string CompositionIdValue = string.Empty;

        public string CompositionId

        {

            get { return this.CompositionIdValue; }

            set { SetProperty(ref CompositionIdValue, value); }

        }
        private DateTime GenerationDateValue;

        public DateTime GenerationDate

        {

            get { return this.GenerationDateValue; }

            set { SetProperty(ref GenerationDateValue, value); }

        }
        private decimal MinPressureValue;

        public decimal MinPressure

        {

            get { return this.MinPressureValue; }

            set { SetProperty(ref MinPressureValue, value); }

        }
        private decimal MaxPressureValue;

        public decimal MaxPressure

        {

            get { return this.MaxPressureValue; }

            set { SetProperty(ref MaxPressureValue, value); }

        }
        private decimal MinTemperatureValue;

        public decimal MinTemperature

        {

            get { return this.MinTemperatureValue; }

            set { SetProperty(ref MinTemperatureValue, value); }

        }
        private decimal MaxTemperatureValue;

        public decimal MaxTemperature

        {

            get { return this.MaxTemperatureValue; }

            set { SetProperty(ref MaxTemperatureValue, value); }

        }
        private List<PhaseRegion> PhaseRegionsValue = new();

        public List<PhaseRegion> PhaseRegions

        {

            get { return this.PhaseRegionsValue; }

            set { SetProperty(ref PhaseRegionsValue, value); }

        }
    }
}
