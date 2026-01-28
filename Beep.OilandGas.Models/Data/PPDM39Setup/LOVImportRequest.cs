using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.Models.Data.DataManagement;

namespace Beep.OilandGas.Models.Data
{
    public class LOVImportRequest : ModelEntityBase
    {
        private string FilePathValue = string.Empty;

        [Required(ErrorMessage = "FilePath is required")]
        public string FilePath

        {

            get { return this.FilePathValue; }

            set { SetProperty(ref FilePathValue, value); }

        }
        private string? TargetTableValue;

        public string? TargetTable

        {

            get { return this.TargetTableValue; }

            set { SetProperty(ref TargetTableValue, value); }

        }
        public Dictionary<string, string>? ColumnMapping { get; set; }
        private bool? SkipExistingValue;

        public bool? SkipExisting

        {

            get { return this.SkipExistingValue; }

            set { SetProperty(ref SkipExistingValue, value); }

        }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
    }
}
