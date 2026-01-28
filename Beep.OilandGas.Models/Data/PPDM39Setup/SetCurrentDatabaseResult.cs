using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.Models.Data.DataManagement;

namespace Beep.OilandGas.Models.Data
{
    public class SetCurrentDatabaseResult : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
        private string? ErrorDetailsValue;

        public string? ErrorDetails

        {

            get { return this.ErrorDetailsValue; }

            set { SetProperty(ref ErrorDetailsValue, value); }

        }

        /// <summary>
        /// If true, client must log out to apply the change
        /// </summary>
        private bool RequiresLogoutValue = false;

        public bool RequiresLogout

        {

            get { return this.RequiresLogoutValue; }

            set { SetProperty(ref RequiresLogoutValue, value); }

        }
    }
}
