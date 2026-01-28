using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class FieldQualityMetrics : ModelEntityBase
    {
        private string FieldNameValue;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        private int TotalValuesValue;

        public int TotalValues

        {

            get { return this.TotalValuesValue; }

            set { SetProperty(ref TotalValuesValue, value); }

        }
        private int NullValuesValue;

        public int NullValues

        {

            get { return this.NullValuesValue; }

            set { SetProperty(ref NullValuesValue, value); }

        }
        private int EmptyValuesValue;

        public int EmptyValues

        {

            get { return this.EmptyValuesValue; }

            set { SetProperty(ref EmptyValuesValue, value); }

        }
        private int ValidValuesValue;

        public int ValidValues

        {

            get { return this.ValidValuesValue; }

            set { SetProperty(ref ValidValuesValue, value); }

        }
        private double CompletenessValue;

        public double Completeness

        {

            get { return this.CompletenessValue; }

            set { SetProperty(ref CompletenessValue, value); }

        } // 0-100
        private List<string> InvalidValuesValue = new List<string>();

        public List<string> InvalidValues

        {

            get { return this.InvalidValuesValue; }

            set { SetProperty(ref InvalidValuesValue, value); }

        }
    }
}
