using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class RolePermission : ModelEntityBase
    {
        private string RoleIdValue = string.Empty;

        public string RoleId

        {

            get { return this.RoleIdValue; }

            set { SetProperty(ref RoleIdValue, value); }

        }
        private string PermissionIdValue = string.Empty;

        public string PermissionId

        {

            get { return this.PermissionIdValue; }

            set { SetProperty(ref PermissionIdValue, value); }

        }
        private string? OrganizationIdValue;

        public string? OrganizationId

        {

            get { return this.OrganizationIdValue; }

            set { SetProperty(ref OrganizationIdValue, value); }

        }
        private bool ActiveValue = true;

        public bool Active

        {

            get { return this.ActiveValue; }

            set { SetProperty(ref ActiveValue, value); }

        }
    }
}
