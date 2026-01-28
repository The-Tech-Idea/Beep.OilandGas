using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    public class TestDataValidationResult : ModelEntityBase
    {
        private bool IsValidValue;

        public bool IsValid

        {

            get { return this.IsValidValue; }

            set { SetProperty(ref IsValidValue, value); }

        }
        private List<string> ErrorsValue = new();

        public List<string> Errors

        {

            get { return this.ErrorsValue; }

            set { SetProperty(ref ErrorsValue, value); }

        }
        private List<string> WarningsValue = new();

        public List<string> Warnings

        {

            get { return this.WarningsValue; }

            set { SetProperty(ref WarningsValue, value); }

        }
        private double DataQualityScoreValue;

        public double DataQualityScore

        {

            get { return this.DataQualityScoreValue; }

            set { SetProperty(ref DataQualityScoreValue, value); }

        }
        private string DataQualityRatingValue = string.Empty;

        public string DataQualityRating

        {

            get { return this.DataQualityRatingValue; }

            set { SetProperty(ref DataQualityRatingValue, value); }

        }
    }
}
