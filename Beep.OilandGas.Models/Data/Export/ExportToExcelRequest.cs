using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Export
{
    public class ExportToExcelRequest : ModelEntityBase
    {
        private string EntityTypeValue;

        public string EntityType

        {

            get { return this.EntityTypeValue; }

            set { SetProperty(ref EntityTypeValue, value); }

        }
        private List<string>? EntityIdsValue;

        public List<string>? EntityIds

        {

            get { return this.EntityIdsValue; }

            set { SetProperty(ref EntityIdsValue, value); }

        }
        private string FilePathValue;

        public string FilePath

        {

            get { return this.FilePathValue; }

            set { SetProperty(ref FilePathValue, value); }

        }
        public Dictionary<string, object>? Filters { get; set; }
        private string? SheetNameValue;

        public string? SheetName

        {

            get { return this.SheetNameValue; }

            set { SetProperty(ref SheetNameValue, value); }

        }
    }
}
