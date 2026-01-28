using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class ExportRequest : ModelEntityBase
    {
        private string TableNameValue = string.Empty;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private List<AppFilter>? FiltersValue;

        public List<AppFilter>? Filters

        {

            get { return this.FiltersValue; }

            set { SetProperty(ref FiltersValue, value); }

        }
        private string FormatValue = "csv";

        public string Format

        {

            get { return this.FormatValue; }

            set { SetProperty(ref FormatValue, value); }

        } // "csv" or "json"
        private bool IncludeHeadersValue = true;

        public bool IncludeHeaders

        {

            get { return this.IncludeHeadersValue; }

            set { SetProperty(ref IncludeHeadersValue, value); }

        }
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
    }
}
