using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class SyncConflict : ModelEntityBase
    {
        private string ConflictIdValue;

        public string ConflictId

        {

            get { return this.ConflictIdValue; }

            set { SetProperty(ref ConflictIdValue, value); }

        }
        private string TableNameValue;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private object EntityIdValue;

        public object EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private object SourceValueValue;

        public object SourceValue

        {

            get { return this.SourceValueValue; }

            set { SetProperty(ref SourceValueValue, value); }

        }
        private object TargetValueValue;

        public object TargetValue

        {

            get { return this.TargetValueValue; }

            set { SetProperty(ref TargetValueValue, value); }

        }
        private List<FieldDifference> FieldDifferencesValue = new List<FieldDifference>();

        public List<FieldDifference> FieldDifferences

        {

            get { return this.FieldDifferencesValue; }

            set { SetProperty(ref FieldDifferencesValue, value); }

        }
        private string ConflictReasonValue;

        public string ConflictReason

        {

            get { return this.ConflictReasonValue; }

            set { SetProperty(ref ConflictReasonValue, value); }

        }
    }
}
