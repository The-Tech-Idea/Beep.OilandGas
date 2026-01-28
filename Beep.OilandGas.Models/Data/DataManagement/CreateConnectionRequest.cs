using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class CreateConnectionRequest : ModelEntityBase
    {
        private string ConnectionNameValue = string.Empty;

        [Required(ErrorMessage = "ConnectionName is required")]
        public string ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }

        private string DatabaseTypeValue = string.Empty;


        [Required(ErrorMessage = "DatabaseType is required")]
        public string DatabaseType


        {


            get { return this.DatabaseTypeValue; }


            set { SetProperty(ref DatabaseTypeValue, value); }


        }

        private string ServerValue = string.Empty;


        [Required(ErrorMessage = "Server is required")]
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
        private bool CreateDatabaseValue;

        public bool CreateDatabase

        {

            get { return this.CreateDatabaseValue; }

            set { SetProperty(ref CreateDatabaseValue, value); }

        }
        private string? SchemaNameValue;

        public string? SchemaName

        {

            get { return this.SchemaNameValue; }

            set { SetProperty(ref SchemaNameValue, value); }

        }
    }
}
