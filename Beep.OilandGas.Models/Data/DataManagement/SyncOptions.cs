using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class SyncOptions : ModelEntityBase
    {
        private SyncDirection DirectionValue = SyncDirection.Both;

        public SyncDirection Direction

        {

            get { return this.DirectionValue; }

            set { SetProperty(ref DirectionValue, value); }

        }
        private List<string> KeyFieldsValue = new List<string>();

        public List<string> KeyFields

        {

            get { return this.KeyFieldsValue; }

            set { SetProperty(ref KeyFieldsValue, value); }

        }
        private List<string> SyncFieldsValue = new List<string>();

        public List<string> SyncFields

        {

            get { return this.SyncFieldsValue; }

            set { SetProperty(ref SyncFieldsValue, value); }

        }
        private ConflictResolutionStrategy DefaultConflictStrategyValue = ConflictResolutionStrategy.SourceWins;

        public ConflictResolutionStrategy DefaultConflictStrategy

        {

            get { return this.DefaultConflictStrategyValue; }

            set { SetProperty(ref DefaultConflictStrategyValue, value); }

        }
        private bool ValidateBeforeSyncValue = true;

        public bool ValidateBeforeSync

        {

            get { return this.ValidateBeforeSyncValue; }

            set { SetProperty(ref ValidateBeforeSyncValue, value); }

        }
        private bool CreateMissingRecordsValue = true;

        public bool CreateMissingRecords

        {

            get { return this.CreateMissingRecordsValue; }

            set { SetProperty(ref CreateMissingRecordsValue, value); }

        }
        private bool UpdateExistingRecordsValue = true;

        public bool UpdateExistingRecords

        {

            get { return this.UpdateExistingRecordsValue; }

            set { SetProperty(ref UpdateExistingRecordsValue, value); }

        }
    }
}
