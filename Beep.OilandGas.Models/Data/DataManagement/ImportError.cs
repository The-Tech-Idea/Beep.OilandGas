using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class ImportError : ModelEntityBase
    {
        private int RowNumberValue;

        public int RowNumber

        {

            get { return this.RowNumberValue; }

            set { SetProperty(ref RowNumberValue, value); }

        }
        private string ErrorMessageValue;

        public string ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        public Dictionary<string, object> RowData { get; set; } = new Dictionary<string, object>();
    }
}
