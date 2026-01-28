using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class FileImportError : ModelEntityBase
    {
        private int RowNumberValue;

        public int RowNumber

        {

            get { return this.RowNumberValue; }

            set { SetProperty(ref RowNumberValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
        private string? ColumnNameValue;

        public string? ColumnName

        {

            get { return this.ColumnNameValue; }

            set { SetProperty(ref ColumnNameValue, value); }

        }
    }
}
