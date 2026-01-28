using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.Models.Data.DataManagement;

namespace Beep.OilandGas.Models.Data
{
    public class RecreateDatabaseRequest : ModelEntityBase
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
        private bool BackupFirstValue = false;

        public bool BackupFirst

        {

            get { return this.BackupFirstValue; }

            set { SetProperty(ref BackupFirstValue, value); }

        }
        private string? BackupPathValue;

        public string? BackupPath

        {

            get { return this.BackupPathValue; }

            set { SetProperty(ref BackupPathValue, value); }

        }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }
}
