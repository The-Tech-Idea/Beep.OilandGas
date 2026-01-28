using System;
using System.Collections.Generic;
using SkiaSharp;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Rendering
{
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
}
