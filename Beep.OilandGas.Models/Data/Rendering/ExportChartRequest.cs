using System;
using System.Collections.Generic;
using SkiaSharp;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Rendering
{
    public class ExportChartRequest : ModelEntityBase
    {
        private string ChartTypeValue;

        public string ChartType

        {

            get { return this.ChartTypeValue; }

            set { SetProperty(ref ChartTypeValue, value); }

        }
        private string ExportFormatValue;

        public string ExportFormat

        {

            get { return this.ExportFormatValue; }

            set { SetProperty(ref ExportFormatValue, value); }

        }
        private string FilePathValue;

        public string FilePath

        {

            get { return this.FilePathValue; }

            set { SetProperty(ref FilePathValue, value); }

        }
        private float WidthValue = 800f;

        public float Width

        {

            get { return this.WidthValue; }

            set { SetProperty(ref WidthValue, value); }

        }
        private float HeightValue = 600f;

        public float Height

        {

            get { return this.HeightValue; }

            set { SetProperty(ref HeightValue, value); }

        }
        public Dictionary<string, object>? ChartData { get; set; }
        private string? ConfigurationIdValue;

        public string? ConfigurationId

        {

            get { return this.ConfigurationIdValue; }

            set { SetProperty(ref ConfigurationIdValue, value); }

        }
    }
}
