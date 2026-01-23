using System.ComponentModel.DataAnnotations;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.AccessControl
{
    /// <summary>
    /// Request to update user preferences
    /// </summary>
    public class UpdatePreferencesRequest : ModelEntityBase
    {
        private string PreferencesJsonValue = string.Empty;

        [Required(ErrorMessage = "PreferencesJson is required")]
        public string PreferencesJson

        {

            get { return this.PreferencesJsonValue; }

            set { SetProperty(ref PreferencesJsonValue, value); }

        }
    }

    /// <summary>
    /// Request to update user's primary role
    /// </summary>
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

    /// <summary>
    /// Request to update user's preferred layout
    /// </summary>
    public class UpdatePreferredLayoutRequest : ModelEntityBase
    {
        private string PreferredLayoutValue = string.Empty;

        [Required(ErrorMessage = "PreferredLayout is required")]
        public string PreferredLayout

        {

            get { return this.PreferredLayoutValue; }

            set { SetProperty(ref PreferredLayoutValue, value); }

        }
    }
}






