using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class ConnectionInfo : ModelEntityBase
    {
        private string ConnectionNameValue = string.Empty;

        public string ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private string DatabaseTypeValue = string.Empty;

        public string DatabaseType

        {

            get { return this.DatabaseTypeValue; }

            set { SetProperty(ref DatabaseTypeValue, value); }

        }
        private string ServerValue = string.Empty;

        public string Server

        {

            get { return this.ServerValue; }

            set { SetProperty(ref ServerValue, value); }

        }
        private string? DatabaseValue;

        public string? Database

        {

            get { return this.DatabaseValue; }

            set { SetProperty(ref DatabaseValue, value); }

        }
        private int? PortValue;

        public int? Port

        {

            get { return this.PortValue; }

            set { SetProperty(ref PortValue, value); }

        }
        private bool IsActiveValue;

        public bool IsActive

        {

            get { return this.IsActiveValue; }

            set { SetProperty(ref IsActiveValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }
}
