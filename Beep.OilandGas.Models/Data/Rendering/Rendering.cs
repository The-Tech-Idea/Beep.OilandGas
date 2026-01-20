using System;
using System.Collections.Generic;
using SkiaSharp;

namespace Beep.OilandGas.Models.Data.Rendering
{
    public class RenderChartRequest : ModelEntityBase
    {
        public string ChartType { get; set; }
        public float Width { get; set; } = 800f;
        public float Height { get; set; } = 600f;
        public Dictionary<string, object>? ChartData { get; set; }
        public string? ConfigurationId { get; set; }
    }

    public class RenderChartResult : ModelEntityBase
    {
        public string ChartId { get; set; }
        public string ChartType { get; set; }
        public byte[]? ImageData { get; set; }
        public string? ImageFormat { get; set; }
        public DateTime RenderDate { get; set; } = DateTime.UtcNow;
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class SaveChartConfigurationRequest : ModelEntityBase
    {
        public string ChartType { get; set; }
        public string ConfigurationName { get; set; }
        public string ConfigurationData { get; set; }
        public bool IsDefault { get; set; } = false;
    }

    public class ChartConfigurationResponse : ModelEntityBase
    {
        public string ConfigurationId { get; set; }
        public string ChartType { get; set; }
        public string ConfigurationName { get; set; }
        public string ConfigurationData { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }

    public class ExportChartRequest : ModelEntityBase
    {
        public string ChartType { get; set; }
        public string ExportFormat { get; set; }
        public string FilePath { get; set; }
        public float Width { get; set; } = 800f;
        public float Height { get; set; } = 600f;
        public Dictionary<string, object>? ChartData { get; set; }
        public string? ConfigurationId { get; set; }
    }

    public class ExportChartResult : ModelEntityBase
    {
        public string ChartId { get; set; }
        public string ChartType { get; set; }
        public string ExportFormat { get; set; }
        public string FilePath { get; set; }
        public DateTime ExportDate { get; set; } = DateTime.UtcNow;
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }
}





