using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class LabTestResult : ModelEntityBase
    {
        private string TestIdValue = string.Empty;

        public string TestId

        {

            get { return this.TestIdValue; }

            set { SetProperty(ref TestIdValue, value); }

        }
        private string SampleIdValue = string.Empty;

        public string SampleId

        {

            get { return this.SampleIdValue; }

            set { SetProperty(ref SampleIdValue, value); }

        }
        private string TestTypeValue = string.Empty;

        public string TestType

        {

            get { return this.TestTypeValue; }

            set { SetProperty(ref TestTypeValue, value); }

        }
        private string TestMethodValue = string.Empty;

        public string TestMethod

        {

            get { return this.TestMethodValue; }

            set { SetProperty(ref TestMethodValue, value); }

        }
        private DateTime TestDateValue;

        public DateTime TestDate

        {

            get { return this.TestDateValue; }

            set { SetProperty(ref TestDateValue, value); }

        }
        private string LabNameValue = string.Empty;

        public string LabName

        {

            get { return this.LabNameValue; }

            set { SetProperty(ref LabNameValue, value); }

        }
        private string AnalystValue = string.Empty;

        public string Analyst

        {

            get { return this.AnalystValue; }

            set { SetProperty(ref AnalystValue, value); }

        }
        private List<TestMeasurement> MeasurementsValue = new();

        public List<TestMeasurement> Measurements

        {

            get { return this.MeasurementsValue; }

            set { SetProperty(ref MeasurementsValue, value); }

        }
        private string QualityControlValue = string.Empty;

        public string QualityControl

        {

            get { return this.QualityControlValue; }

            set { SetProperty(ref QualityControlValue, value); }

        }
        private List<string> NotesValue = new();

        public List<string> Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }
    }
}
