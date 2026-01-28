using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class ConnectionConfig : ModelEntityBase
    {
        private string DatabaseTypeValue = string.Empty;

        public string DatabaseType

        {

            get { return this.DatabaseTypeValue; }

            set { SetProperty(ref DatabaseTypeValue, value); }

        }
        private string ConnectionNameValue = string.Empty;

        public string ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private string HostValue = string.Empty;

        public string Host

        {

            get { return this.HostValue; }

            set { SetProperty(ref HostValue, value); }

        }
        private int PortValue;

        public int Port

        {

            get { return this.PortValue; }

            set { SetProperty(ref PortValue, value); }

        }
        private string DatabaseValue = string.Empty;

        public string Database

        {

            get { return this.DatabaseValue; }

            set { SetProperty(ref DatabaseValue, value); }

        }
        private string? SchemaValue;

        public string? Schema

        {

            get { return this.SchemaValue; }

            set { SetProperty(ref SchemaValue, value); }

        }
        private string? UsernameValue;

        public string? Username

        {

            get { return this.UsernameValue; }

            set { SetProperty(ref UsernameValue, value); }

        }
        private string? PasswordValue;

        public string? Password

        {

            get { return this.PasswordValue; }

            set { SetProperty(ref PasswordValue, value); }

        }
        private string? ConnectionStringValue;

        public string? ConnectionString

        {

            get { return this.ConnectionStringValue; }

            set { SetProperty(ref ConnectionStringValue, value); }

        }

        /// <summary>
        /// Converts to ConnectionProperties for internal use
        /// </summary>
        public TheTechIdea.Beep.ConfigUtil.ConnectionProperties ToConnectionProperties()
        {
            var cp = new TheTechIdea.Beep.ConfigUtil.ConnectionProperties
            {
                ConnectionName = this.ConnectionName,
                Host = this.Host,
                Port = this.Port,
                Database = this.Database,
                UserID = this.Username ?? string.Empty,
                Password = this.Password ?? string.Empty,
                ConnectionString = this.ConnectionString ?? string.Empty
            };

            // Note: DatabaseType enum parsing commented out due to external dependency issues
            // Set database type based on string value if available through other means
            
            return cp;
        }
    }
}
