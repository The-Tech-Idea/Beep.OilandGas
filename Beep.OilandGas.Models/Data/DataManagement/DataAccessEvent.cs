using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class DataAccessEvent : ModelEntityBase
    {
        private string EventIdValue;

        public string EventId

        {

            get { return this.EventIdValue; }

            set { SetProperty(ref EventIdValue, value); }

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
        private string UserIdValue;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
        private string AccessTypeValue;

        public string AccessType

        {

            get { return this.AccessTypeValue; }

            set { SetProperty(ref AccessTypeValue, value); }

        } // Read, Write, Delete, Export
        private DateTime AccessDateValue;

        public DateTime AccessDate

        {

            get { return this.AccessDateValue; }

            set { SetProperty(ref AccessDateValue, value); }

        }
        private string IpAddressValue;

        public string IpAddress

        {

            get { return this.IpAddressValue; }

            set { SetProperty(ref IpAddressValue, value); }

        }
        private string ApplicationNameValue;

        public string ApplicationName

        {

            get { return this.ApplicationNameValue; }

            set { SetProperty(ref ApplicationNameValue, value); }

        }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
}
