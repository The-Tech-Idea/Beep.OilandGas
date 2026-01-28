using System;
using System.Collections.Generic;
using SkiaSharp;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Rendering
{
    public class ExportChartResult : ModelEntityBase
    {
        private string ChartIdValue;

        public string ChartId

        {

            get { return this.ChartIdValue; }

            set { SetProperty(ref ChartIdValue, value); }

        }
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
        private DateTime ExportDateValue = DateTime.UtcNow;

        public DateTime ExportDate

        {

            get { return this.ExportDateValue; }

            set { SetProperty(ref ExportDateValue, value); }

        }
        private bool IsSuccessValue;

        public bool IsSuccess

        {

            get { return this.IsSuccessValue; }

            set { SetProperty(ref IsSuccessValue, value); }

        }
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
    }
}
