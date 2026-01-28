using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class AccessStatistics : ModelEntityBase
    {
        private string TableNameValue;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private DateTime FromDateValue;

        public DateTime FromDate

        {

            get { return this.FromDateValue; }

            set { SetProperty(ref FromDateValue, value); }

        }
        private DateTime ToDateValue;

        public DateTime ToDate

        {

            get { return this.ToDateValue; }

            set { SetProperty(ref ToDateValue, value); }

        }
        private int TotalAccessEventsValue;

        public int TotalAccessEvents

        {

            get { return this.TotalAccessEventsValue; }

            set { SetProperty(ref TotalAccessEventsValue, value); }

        }
        private int UniqueUsersValue;

        public int UniqueUsers

        {

            get { return this.UniqueUsersValue; }

            set { SetProperty(ref UniqueUsersValue, value); }

        }
        private int ReadOperationsValue;

        public int ReadOperations

        {

            get { return this.ReadOperationsValue; }

            set { SetProperty(ref ReadOperationsValue, value); }

        }
        private int WriteOperationsValue;

        public int WriteOperations

        {

            get { return this.WriteOperationsValue; }

            set { SetProperty(ref WriteOperationsValue, value); }

        }
        private int DeleteOperationsValue;

        public int DeleteOperations

        {

            get { return this.DeleteOperationsValue; }

            set { SetProperty(ref DeleteOperationsValue, value); }

        }
        public Dictionary<string, int> AccessByUser { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> AccessByType { get; set; } = new Dictionary<string, int>();
    }
}
