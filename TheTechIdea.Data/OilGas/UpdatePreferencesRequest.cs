using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.AccessControl
{
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
}
