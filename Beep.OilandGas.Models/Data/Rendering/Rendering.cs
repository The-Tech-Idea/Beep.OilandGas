using System;
using System.Collections.Generic;
using SkiaSharp;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Rendering
{
    public class RenderChartRequest : ModelEntityBase
    {
        private string ChartTypeValue;

        public string ChartType

        {

            get { return this.ChartTypeValue; }

            set { SetProperty(ref ChartTypeValue, value); }

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

    public class RenderChartResult : ModelEntityBase
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
        private byte[]? ImageDataValue;

        public byte[]? ImageData

        {

            get { return this.ImageDataValue; }

            set { SetProperty(ref ImageDataValue, value); }

        }
        private string? ImageFormatValue;

        public string? ImageFormat

        {

            get { return this.ImageFormatValue; }

            set { SetProperty(ref ImageFormatValue, value); }

        }
        private DateTime RenderDateValue = DateTime.UtcNow;

        public DateTime RenderDate

        {

            get { return this.RenderDateValue; }

            set { SetProperty(ref RenderDateValue, value); }

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

    public class SaveChartConfigurationRequest : ModelEntityBase
    {
        private string ChartTypeValue;

        public string ChartType

        {

            get { return this.ChartTypeValue; }

            set { SetProperty(ref ChartTypeValue, value); }

        }
        private string ConfigurationNameValue;

        public string ConfigurationName

        {

            get { return this.ConfigurationNameValue; }

            set { SetProperty(ref ConfigurationNameValue, value); }

        }
        private string ConfigurationDataValue;

        public string ConfigurationData

        {

            get { return this.ConfigurationDataValue; }

            set { SetProperty(ref ConfigurationDataValue, value); }

        }
        private bool IsDefaultValue = false;

        public bool IsDefault

        {

            get { return this.IsDefaultValue; }

            set { SetProperty(ref IsDefaultValue, value); }

        }
    }

    public class ChartConfigurationResponse : ModelEntityBase
    {
        private string ConfigurationIdValue;

        public string ConfigurationId

        {

            get { return this.ConfigurationIdValue; }

            set { SetProperty(ref ConfigurationIdValue, value); }

        }
        private string ChartTypeValue;

        public string ChartType

        {

            get { return this.ChartTypeValue; }

            set { SetProperty(ref ChartTypeValue, value); }

        }
        private string ConfigurationNameValue;

        public string ConfigurationName

        {

            get { return this.ConfigurationNameValue; }

            set { SetProperty(ref ConfigurationNameValue, value); }

        }
        private string ConfigurationDataValue;

        public string ConfigurationData

        {

            get { return this.ConfigurationDataValue; }

            set { SetProperty(ref ConfigurationDataValue, value); }

        }
        private bool IsDefaultValue;

        public bool IsDefault

        {

            get { return this.IsDefaultValue; }

            set { SetProperty(ref IsDefaultValue, value); }

        }
        private DateTime CreatedDateValue;

        public DateTime CreatedDate

        {

            get { return this.CreatedDateValue; }

            set { SetProperty(ref CreatedDateValue, value); }

        }
        private string CreatedByValue;

        public string CreatedBy

        {

            get { return this.CreatedByValue; }

            set { SetProperty(ref CreatedByValue, value); }

        }
    }

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








