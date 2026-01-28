using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class DatabaseOperationProgress : ProgressUpdate
    {
        private string ConnectionNameValue = string.Empty;

        public string ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private string OperationValue = string.Empty;

        public string Operation

        {

            get { return this.OperationValue; }

            set { SetProperty(ref OperationValue, value); }

        } // "Drop", "Recreate"
        public new string CurrentStep { get; set; } = string.Empty;
    }
}
