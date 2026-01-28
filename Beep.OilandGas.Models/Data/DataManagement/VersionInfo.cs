using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class VersionInfo : ModelEntityBase
    {
        private string VersionIdValue = string.Empty;

        public string VersionId

        {

            get { return this.VersionIdValue; }

            set { SetProperty(ref VersionIdValue, value); }

        }
        private DateTime CreatedAtValue;

        public DateTime CreatedAt

        {

            get { return this.CreatedAtValue; }

            set { SetProperty(ref CreatedAtValue, value); }

        }
        private string CreatedByValue = string.Empty;

        public string CreatedBy

        {

            get { return this.CreatedByValue; }

            set { SetProperty(ref CreatedByValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        public Dictionary<string, object>? EntityData { get; set; }
    }
}
