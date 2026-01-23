using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Setup
{
    /// <summary>
    /// Data class for initial user and role assignment during setup
    /// Maps to BA_EMPLOYEE and BA_AUTHORITY tables in PPDM39 schema
    /// </summary>
    public class UserRoleSetupData : ModelEntityBase
    {
        private string UserIdValue = string.Empty;

        [Required]
        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        } // BUSINESS_ASSOCIATE_ID

        private string UserNameValue = string.Empty;


        [Required]
        public string UserName


        {


            get { return this.UserNameValue; }


            set { SetProperty(ref UserNameValue, value); }


        }

        private string EmailValue = string.Empty;


        [Required]
        [EmailAddress]
        public string Email


        {


            get { return this.EmailValue; }


            set { SetProperty(ref EmailValue, value); }


        }

        private string EmployerBaIdValue = string.Empty;


        [Required]
        public string EmployerBaId


        {


            get { return this.EmployerBaIdValue; }


            set { SetProperty(ref EmployerBaIdValue, value); }


        } // Organization BUSINESS_ASSOCIATE_ID

        private List<string> RolesValue = new List<string>();


        public List<string> Roles


        {


            get { return this.RolesValue; }


            set { SetProperty(ref RolesValue, value); }


        }

        private string? EmployeePositionValue;


        public string? EmployeePosition


        {


            get { return this.EmployeePositionValue; }


            set { SetProperty(ref EmployeePositionValue, value); }


        }

        private string? StatusValue;


        public string? Status


        {


            get { return this.StatusValue; }


            set { SetProperty(ref StatusValue, value); }


        }

        private DateTime? EffectiveDateValue;


        public DateTime? EffectiveDate


        {


            get { return this.EffectiveDateValue; }


            set { SetProperty(ref EffectiveDateValue, value); }


        }

        private DateTime? ExpiryDateValue;


        public DateTime? ExpiryDate


        {


            get { return this.ExpiryDateValue; }


            set { SetProperty(ref ExpiryDateValue, value); }


        }

        private bool ActiveValue = true;


        public bool Active


        {


            get { return this.ActiveValue; }


            set { SetProperty(ref ActiveValue, value); }


        }
    }

    /// <summary>
    /// Role assignment data for a user
    /// Maps to BA_AUTHORITY table
    /// </summary>
    public class RoleAssignmentData : ModelEntityBase
    {
        private string BusinessAssociateIdValue = string.Empty;

        [Required]
        public string BusinessAssociateId

        {

            get { return this.BusinessAssociateIdValue; }

            set { SetProperty(ref BusinessAssociateIdValue, value); }

        }

        private string AuthorityIdValue = string.Empty;


        [Required]
        public string AuthorityId


        {


            get { return this.AuthorityIdValue; }


            set { SetProperty(ref AuthorityIdValue, value); }


        } // Role ID

        private string? AuthorityTypeValue;


        public string? AuthorityType


        {


            get { return this.AuthorityTypeValue; }


            set { SetProperty(ref AuthorityTypeValue, value); }


        }

        private DateTime? EffectiveDateValue;


        public DateTime? EffectiveDate


        {


            get { return this.EffectiveDateValue; }


            set { SetProperty(ref EffectiveDateValue, value); }


        }

        private DateTime? ExpiryDateValue;


        public DateTime? ExpiryDate


        {


            get { return this.ExpiryDateValue; }


            set { SetProperty(ref ExpiryDateValue, value); }


        }

        private bool ActiveValue = true;


        public bool Active


        {


            get { return this.ActiveValue; }


            set { SetProperty(ref ActiveValue, value); }


        }
    }
}







