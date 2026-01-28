using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class SampleInfo : ModelEntityBase
    {
        private string SampleIdValue = string.Empty;

        public string SampleId

        {

            get { return this.SampleIdValue; }

            set { SetProperty(ref SampleIdValue, value); }

        }
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private string FieldNameValue = string.Empty;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        private decimal DepthValue;

        public decimal Depth

        {

            get { return this.DepthValue; }

            set { SetProperty(ref DepthValue, value); }

        }
        private decimal ReservoirTemperatureValue;

        public decimal ReservoirTemperature

        {

            get { return this.ReservoirTemperatureValue; }

            set { SetProperty(ref ReservoirTemperatureValue, value); }

        }
        private decimal ReservoirPressureValue;

        public decimal ReservoirPressure

        {

            get { return this.ReservoirPressureValue; }

            set { SetProperty(ref ReservoirPressureValue, value); }

        }
        private string FluidTypeValue = string.Empty;

        public string FluidType

        {

            get { return this.FluidTypeValue; }

            set { SetProperty(ref FluidTypeValue, value); }

        }
        private DateTime SamplingDateValue;

        public DateTime SamplingDate

        {

            get { return this.SamplingDateValue; }

            set { SetProperty(ref SamplingDateValue, value); }

        }
        private string SamplingMethodValue = string.Empty;

        public string SamplingMethod

        {

            get { return this.SamplingMethodValue; }

            set { SetProperty(ref SamplingMethodValue, value); }

        }
        private string ContainerTypeValue = string.Empty;

        public string ContainerType

        {

            get { return this.ContainerTypeValue; }

            set { SetProperty(ref ContainerTypeValue, value); }

        }
        private string PreservationMethodValue = string.Empty;

        public string PreservationMethod

        {

            get { return this.PreservationMethodValue; }

            set { SetProperty(ref PreservationMethodValue, value); }

        }
        private List<string> AnalysisRequiredValue = new();

        public List<string> AnalysisRequired

        {

            get { return this.AnalysisRequiredValue; }

            set { SetProperty(ref AnalysisRequiredValue, value); }

        }
    }
}
