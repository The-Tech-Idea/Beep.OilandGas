using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.AccessControl
{
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
