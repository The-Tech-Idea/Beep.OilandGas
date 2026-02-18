using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.FieldOrchestrator
{
    public class Field : ModelEntityBase
    {
        public string FieldId { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Basin { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public DateTime? DiscoveryDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public decimal? AreaSize { get; set; }
        public string AreaUnit { get; set; } = "Acres";
    }
}
