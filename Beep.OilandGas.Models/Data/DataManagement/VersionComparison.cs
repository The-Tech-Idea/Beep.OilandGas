using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class VersionComparison : ModelEntityBase
    {
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
        private int Version1Value;

        public int Version1

        {

            get { return this.Version1Value; }

            set { SetProperty(ref Version1Value, value); }

        }
        private int Version2Value;

        public int Version2

        {

            get { return this.Version2Value; }

            set { SetProperty(ref Version2Value, value); }

        }
        private List<FieldDifference> DifferencesValue = new List<FieldDifference>();

        public List<FieldDifference> Differences

        {

            get { return this.DifferencesValue; }

            set { SetProperty(ref DifferencesValue, value); }

        }
        private bool HasDifferencesValue;

        public bool HasDifferences

        {

            get { return this.HasDifferencesValue; }

            set { SetProperty(ref HasDifferencesValue, value); }

        }
    }
}
