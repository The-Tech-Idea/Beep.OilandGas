using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class ArchiveCriteria : ModelEntityBase
    {
        private List<AppFilter> FiltersValue = new List<AppFilter>();

        public List<AppFilter> Filters

        {

            get { return this.FiltersValue; }

            set { SetProperty(ref FiltersValue, value); }

        }
        private DateTime? OlderThanDateValue;

        public DateTime? OlderThanDate

        {

            get { return this.OlderThanDateValue; }

            set { SetProperty(ref OlderThanDateValue, value); }

        }
        private string StatusFieldValue;

        public string StatusField

        {

            get { return this.StatusFieldValue; }

            set { SetProperty(ref StatusFieldValue, value); }

        }
        private string StatusValueValue;

        public string StatusValue

        {

            get { return this.StatusValueValue; }

            set { SetProperty(ref StatusValueValue, value); }

        }
        private bool ArchiveInactiveOnlyValue;

        public bool ArchiveInactiveOnly

        {

            get { return this.ArchiveInactiveOnlyValue; }

            set { SetProperty(ref ArchiveInactiveOnlyValue, value); }

        }
    }
}
