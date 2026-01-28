using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class ArchiveResult : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private int ArchivedRecordCountValue;

        public int ArchivedRecordCount

        {

            get { return this.ArchivedRecordCountValue; }

            set { SetProperty(ref ArchivedRecordCountValue, value); }

        }
        private List<object> ArchivedRecordIdsValue = new List<object>();

        public List<object> ArchivedRecordIds

        {

            get { return this.ArchivedRecordIdsValue; }

            set { SetProperty(ref ArchivedRecordIdsValue, value); }

        }
        private string ArchiveLocationValue;

        public string ArchiveLocation

        {

            get { return this.ArchiveLocationValue; }

            set { SetProperty(ref ArchiveLocationValue, value); }

        }
        private List<string> MessagesValue = new List<string>();

        public List<string> Messages

        {

            get { return this.MessagesValue; }

            set { SetProperty(ref MessagesValue, value); }

        }
    }
}
