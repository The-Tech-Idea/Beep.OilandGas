using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class UserProfile : ModelEntityBase
    {
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
        private string? PrimaryRoleValue;

        public string? PrimaryRole

        {

            get { return this.PrimaryRoleValue; }

            set { SetProperty(ref PrimaryRoleValue, value); }

        }
        private string? PreferredLayoutValue;

        public string? PreferredLayout

        {

            get { return this.PreferredLayoutValue; }

            set { SetProperty(ref PreferredLayoutValue, value); }

        }
        private string? UserPreferencesValue;

        public string? UserPreferences

        {

            get { return this.UserPreferencesValue; }

            set { SetProperty(ref UserPreferencesValue, value); }

        } // JSON string
        private DateTime? LastLoginDateValue;

        public DateTime? LastLoginDate

        {

            get { return this.LastLoginDateValue; }

            set { SetProperty(ref LastLoginDateValue, value); }

        }
        private bool ActiveValue = true;

        public bool Active

        {

            get { return this.ActiveValue; }

            set { SetProperty(ref ActiveValue, value); }

        }
        private List<string> RolesValue = new List<string>();

        public List<string> Roles

        {

            get { return this.RolesValue; }

            set { SetProperty(ref RolesValue, value); }

        }
    }
}
