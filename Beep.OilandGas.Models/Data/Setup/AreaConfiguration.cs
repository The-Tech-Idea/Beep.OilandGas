using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Setup
{
    public class AreaConfiguration : ModelEntityBase
    {
        private string AreaIdValue = string.Empty;

        [Required]
        public string AreaId

        {

            get { return this.AreaIdValue; }

            set { SetProperty(ref AreaIdValue, value); }

        }

        private string AreaNameValue = string.Empty;


        [Required]
        public string AreaName


        {


            get { return this.AreaNameValue; }


            set { SetProperty(ref AreaNameValue, value); }


        }

        private string? AreaTypeValue;


        public string? AreaType


        {


            get { return this.AreaTypeValue; }


            set { SetProperty(ref AreaTypeValue, value); }


        }

        private string? ParentAreaIdValue;


        public string? ParentAreaId


        {


            get { return this.ParentAreaIdValue; }


            set { SetProperty(ref ParentAreaIdValue, value); }


        }

        private int HierarchyLevelValue;


        public int HierarchyLevel


        {


            get { return this.HierarchyLevelValue; }


            set { SetProperty(ref HierarchyLevelValue, value); }


        }

        private bool ActiveValue = true;


        public bool Active


        {


            get { return this.ActiveValue; }


            set { SetProperty(ref ActiveValue, value); }


        }
    }
}
