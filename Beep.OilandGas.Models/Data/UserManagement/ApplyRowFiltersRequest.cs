using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.UserManagement
{
    public class ApplyRowFiltersRequest : ModelEntityBase
    {
        private string UserIdValue = string.Empty;

        [Required(ErrorMessage = "UserId is required")]
        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }

        private string TableNameValue = string.Empty;


        [Required(ErrorMessage = "TableName is required")]
        public string TableName


        {


            get { return this.TableNameValue; }


            set { SetProperty(ref TableNameValue, value); }


        }

        private List<AppFilter>? ExistingFiltersValue;


        public List<AppFilter>? ExistingFilters


        {


            get { return this.ExistingFiltersValue; }


            set { SetProperty(ref ExistingFiltersValue, value); }


        }
    }
}
