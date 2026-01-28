using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class ImportRequest : ModelEntityBase
    {
        private string TableNameValue = string.Empty;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private string FileNameValue = string.Empty;

        public string FileName

        {

            get { return this.FileNameValue; }

            set { SetProperty(ref FileNameValue, value); }

        }
        private string ContentTypeValue = "text/csv";

        public string ContentType

        {

            get { return this.ContentTypeValue; }

            set { SetProperty(ref ContentTypeValue, value); }

        } // "text/csv" or "application/json"
        public Dictionary<string, string>? ColumnMapping { get; set; }
        private bool SkipHeaderRowValue = true;

        public bool SkipHeaderRow

        {

            get { return this.SkipHeaderRowValue; }

            set { SetProperty(ref SkipHeaderRowValue, value); }

        }
        private bool ValidateForeignKeysValue = true;

        public bool ValidateForeignKeys

        {

            get { return this.ValidateForeignKeysValue; }

            set { SetProperty(ref ValidateForeignKeysValue, value); }

        }
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }
}
