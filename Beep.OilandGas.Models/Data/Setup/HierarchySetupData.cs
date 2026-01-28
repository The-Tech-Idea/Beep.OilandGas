using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Setup
{
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
}
