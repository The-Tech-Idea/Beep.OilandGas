using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.AccessControl
{
    public class UpdatePrimaryRoleRequest : ModelEntityBase
    {
        private string PrimaryRoleValue = string.Empty;

        [Required(ErrorMessage = "PrimaryRole is required")]
        public string PrimaryRole

        {

            get { return this.PrimaryRoleValue; }

            set { SetProperty(ref PrimaryRoleValue, value); }

        }
    }
}
