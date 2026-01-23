using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Setup
{
    /// <summary>
    /// Data class for AREA_HIERARCHY configuration during setup
    /// Maps to AREA_HIERARCHY and AREA tables in PPDM39 schema
    /// </summary>
    public class HierarchySetupData : ModelEntityBase
    {
        private string OrganizationIdValue = string.Empty;

        [Required]
        public string OrganizationId

        {

            get { return this.OrganizationIdValue; }

            set { SetProperty(ref OrganizationIdValue, value); }

        }

        private List<HierarchyLevelConfig> HierarchyLevelsValue = new List<HierarchyLevelConfig>();


        public List<HierarchyLevelConfig> HierarchyLevels


        {


            get { return this.HierarchyLevelsValue; }


            set { SetProperty(ref HierarchyLevelsValue, value); }


        }

        private List<AreaConfiguration> AreaConfigurationsValue = new List<AreaConfiguration>();


        public List<AreaConfiguration> AreaConfigurations


        {


            get { return this.AreaConfigurationsValue; }


            set { SetProperty(ref AreaConfigurationsValue, value); }


        }
    }

    /// <summary>
    /// Configuration for a single hierarchy level
    /// </summary>
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

    /// <summary>
    /// Area configuration for hierarchy
    /// </summary>
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







