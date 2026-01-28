using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class OperationProgress : ModelEntityBase
    {
        private string StepNameValue = string.Empty;

        public string StepName

        {

            get { return this.StepNameValue; }

            set { SetProperty(ref StepNameValue, value); }

        }
        private string StepIdValue = string.Empty;

        public string StepId

        {

            get { return this.StepIdValue; }

            set { SetProperty(ref StepIdValue, value); }

        }
        private string OperationIdValue = string.Empty;

        public string OperationId

        {

            get { return this.OperationIdValue; }

            set { SetProperty(ref OperationIdValue, value); }

        }
        private int ProgressPercentageValue;

        public int ProgressPercentage

        {

            get { return this.ProgressPercentageValue; }

            set { SetProperty(ref ProgressPercentageValue, value); }

        }
        private string StatusMessageValue = string.Empty;

        public string StatusMessage

        {

            get { return this.StatusMessageValue; }

            set { SetProperty(ref StatusMessageValue, value); }

        }
        private OperationStatus StatusValue = OperationStatus.Pending;

        public OperationStatus Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

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
        private DateTime? StartedAtValue;

        public DateTime? StartedAt

        {

            get { return this.StartedAtValue; }

            set { SetProperty(ref StartedAtValue, value); }

        }
        private DateTime? CompletedAtValue;

        public DateTime? CompletedAt

        {

            get { return this.CompletedAtValue; }

            set { SetProperty(ref CompletedAtValue, value); }

        }
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        private int WeightValue = 1;

        public int Weight

        {

            get { return this.WeightValue; }

            set { SetProperty(ref WeightValue, value); }

        } // Weight for progress aggregation (default 1)
    }
}
