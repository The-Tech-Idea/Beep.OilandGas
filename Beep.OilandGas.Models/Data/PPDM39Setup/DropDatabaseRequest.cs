using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.Models.Data.DataManagement;

namespace Beep.OilandGas.Models.Data
{
    public class DropDatabaseRequest : ModelEntityBase
    {
        private string ConnectionNameValue = string.Empty;

        [Required]
        public string ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private string? SchemaNameValue;

        public string? SchemaName

        {

            get { return this.SchemaNameValue; }

            set { SetProperty(ref SchemaNameValue, value); }

        }
        private bool DropIfExistsValue = true;

        public bool DropIfExists

        {

            get { return this.DropIfExistsValue; }

            set { SetProperty(ref DropIfExistsValue, value); }

        }
        private bool ForceValue = false;

        public bool Force

        {

            get { return this.ForceValue; }

            set { SetProperty(ref ForceValue, value); }

        }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }
}
