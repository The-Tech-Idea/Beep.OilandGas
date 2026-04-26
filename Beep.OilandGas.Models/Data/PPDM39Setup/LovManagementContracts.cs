using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    /// <summary>A single LOV (List Of Values) entry from an R_* reference table.</summary>
    public class LovValueItem : ModelEntityBase
    {
        private string idValue = string.Empty;
        public string Id { get => idValue; set => SetProperty(ref idValue, value); }

        private string valueTypeValue = string.Empty;
        public string ValueType { get => valueTypeValue; set => SetProperty(ref valueTypeValue, value); }

        private string valueValue = string.Empty;
        public string Value { get => valueValue; set => SetProperty(ref valueValue, value); }

        private string descriptionValue = string.Empty;
        public string Description { get => descriptionValue; set => SetProperty(ref descriptionValue, value); }

        private string? categoryValue;
        public string? Category { get => categoryValue; set => SetProperty(ref categoryValue, value); }

        private string? sourceValue;
        public string? Source { get => sourceValue; set => SetProperty(ref sourceValue, value); }

        private string? moduleValue;
        public string? Module { get => moduleValue; set => SetProperty(ref moduleValue, value); }

        private bool isActiveValue = true;
        public bool IsActive { get => isActiveValue; set => SetProperty(ref isActiveValue, value); }
    }

    /// <summary>A parent node in a hierarchical LOV tree.</summary>
    public class LovHierarchyItem : ModelEntityBase
    {
        private string idValue = string.Empty;
        public string Id { get => idValue; set => SetProperty(ref idValue, value); }

        private string valueTypeValue = string.Empty;
        public string ValueType { get => valueTypeValue; set => SetProperty(ref valueTypeValue, value); }

        private string valueValue = string.Empty;
        public string Value { get => valueValue; set => SetProperty(ref valueValue, value); }

        private string descriptionValue = string.Empty;
        public string Description { get => descriptionValue; set => SetProperty(ref descriptionValue, value); }

        private List<LovValueItem> childrenValue = new();
        public List<LovValueItem> Children { get => childrenValue; set => SetProperty(ref childrenValue, value); }
    }

    /// <summary>Request to create a new LOV entry.</summary>
    public class LovCreateRequest : ModelEntityBase
    {
        private string valueTypeValue = string.Empty;
        public string ValueType { get => valueTypeValue; set => SetProperty(ref valueTypeValue, value); }

        private string valueValue = string.Empty;
        public string Value { get => valueValue; set => SetProperty(ref valueValue, value); }

        private string descriptionValue = string.Empty;
        public string Description { get => descriptionValue; set => SetProperty(ref descriptionValue, value); }

        private string? categoryValue;
        public string? Category { get => categoryValue; set => SetProperty(ref categoryValue, value); }

        private string? sourceValue;
        public string? Source { get => sourceValue; set => SetProperty(ref sourceValue, value); }

        private string? moduleValue;
        public string? Module { get => moduleValue; set => SetProperty(ref moduleValue, value); }
    }

    /// <summary>Request to update an existing LOV entry.</summary>
    public class LovUpdateRequest : ModelEntityBase
    {
        private string descriptionValue = string.Empty;
        public string Description { get => descriptionValue; set => SetProperty(ref descriptionValue, value); }

        private string? categoryValue;
        public string? Category { get => categoryValue; set => SetProperty(ref categoryValue, value); }

        private bool isActiveValue = true;
        public bool IsActive { get => isActiveValue; set => SetProperty(ref isActiveValue, value); }
    }

    /// <summary>Result of a seed-data validation check against a connection.</summary>
    public class SeedingValidationResult : ModelEntityBase
    {
        private bool isValidValue;
        public bool IsValid { get => isValidValue; set => SetProperty(ref isValidValue, value); }

        private string messageValue = string.Empty;
        public string Message { get => messageValue; set => SetProperty(ref messageValue, value); }

        private List<string> warningsValue = new();
        public List<string> Warnings { get => warningsValue; set => SetProperty(ref warningsValue, value); }

        private List<string> errorsValue = new();
        public List<string> Errors { get => errorsValue; set => SetProperty(ref errorsValue, value); }
    }
}
