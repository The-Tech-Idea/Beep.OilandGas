using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class ProgressUpdate : ModelEntityBase
    {
        private string OperationIdValue = string.Empty;

        public string OperationId

        {

            get { return this.OperationIdValue; }

            set { SetProperty(ref OperationIdValue, value); }

        }
        private string OperationTypeValue = string.Empty;

        public string OperationType

        {

            get { return this.OperationTypeValue; }

            set { SetProperty(ref OperationTypeValue, value); }

        } // "ScriptExecution", "CopyDatabase", "CreateSchema", etc.
        private int ProgressPercentageValue;

        public int ProgressPercentage

        {

            get { return this.ProgressPercentageValue; }

            set { SetProperty(ref ProgressPercentageValue, value); }

        }
        private string CurrentStepValue = string.Empty;

        public string CurrentStep

        {

            get { return this.CurrentStepValue; }

            set { SetProperty(ref CurrentStepValue, value); }

        }
        private string StatusMessageValue = string.Empty;

        public string StatusMessage

        {

            get { return this.StatusMessageValue; }

            set { SetProperty(ref StatusMessageValue, value); }

        }
        private long? ItemsProcessedValue;

        public long? ItemsProcessed

        {

            get { return this.ItemsProcessedValue; }

            set { SetProperty(ref ItemsProcessedValue, value); }

        }
        private long? TotalItemsValue;

        public long? TotalItems

        {

            get { return this.TotalItemsValue; }

            set { SetProperty(ref TotalItemsValue, value); }

        }
        private bool IsCompleteValue;

        public bool IsComplete

        {

            get { return this.IsCompleteValue; }

            set { SetProperty(ref IsCompleteValue, value); }

        }
        private bool HasErrorValue;

        public bool HasError

        {

            get { return this.HasErrorValue; }

            set { SetProperty(ref HasErrorValue, value); }

        }
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        private DateTime TimestampValue = DateTime.UtcNow;

        public DateTime Timestamp

        {

            get { return this.TimestampValue; }

            set { SetProperty(ref TimestampValue, value); }

        }
    }
}
