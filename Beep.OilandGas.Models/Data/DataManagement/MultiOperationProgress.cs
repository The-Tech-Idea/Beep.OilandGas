using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class MultiOperationProgress : ProgressUpdate
    {
        public Dictionary<string, ProgressUpdate> Operations { get; set; } = new Dictionary<string, ProgressUpdate>();
        private int TotalOperationsValue;

        public int TotalOperations

        {

            get { return this.TotalOperationsValue; }

            set { SetProperty(ref TotalOperationsValue, value); }

        }
        private int CompletedOperationsValue;

        public int CompletedOperations

        {

            get { return this.CompletedOperationsValue; }

            set { SetProperty(ref CompletedOperationsValue, value); }

        }
        private int FailedOperationsValue;

        public int FailedOperations

        {

            get { return this.FailedOperationsValue; }

            set { SetProperty(ref FailedOperationsValue, value); }

        }
        private int RunningOperationsValue;

        public int RunningOperations

        {

            get { return this.RunningOperationsValue; }

            set { SetProperty(ref RunningOperationsValue, value); }

        }
        private string GroupNameValue = string.Empty;

        public string GroupName

        {

            get { return this.GroupNameValue; }

            set { SetProperty(ref GroupNameValue, value); }

        }
        private int OverallProgressValue;

        public int OverallProgress

        {

            get { return this.OverallProgressValue; }

            set { SetProperty(ref OverallProgressValue, value); }

        }
    }
}
