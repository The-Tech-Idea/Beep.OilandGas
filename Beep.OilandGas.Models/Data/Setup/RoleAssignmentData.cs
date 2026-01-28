using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Setup
{
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
