using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Setup
{
    public class HierarchyLevelConfig : ModelEntityBase
    {
        private int HierarchyLevelValue;

        [Required]
        public int HierarchyLevel

        {

            get { return this.HierarchyLevelValue; }

            set { SetProperty(ref HierarchyLevelValue, value); }

        }

        private string LevelNameValue = string.Empty;


        [Required]
        public string LevelName


        {


            get { return this.LevelNameValue; }


            set { SetProperty(ref LevelNameValue, value); }


        }

        private string AssetTypeValue = string.Empty;


        [Required]
        public string AssetType


        {


            get { return this.AssetTypeValue; }


            set { SetProperty(ref AssetTypeValue, value); }


        }

        private int? ParentLevelValue;


        public int? ParentLevel


        {


            get { return this.ParentLevelValue; }


            set { SetProperty(ref ParentLevelValue, value); }


        }

        private bool ActiveValue = true;


        public bool Active


        {


            get { return this.ActiveValue; }


            set { SetProperty(ref ActiveValue, value); }


        }
    }
}
