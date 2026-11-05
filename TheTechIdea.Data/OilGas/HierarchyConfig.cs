using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class HierarchyConfig : ModelEntityBase
    {
        /// <summary>Expected aggregate revision for updates. All rows in one snapshot carry the same revision.</summary>
        public string? Revision { get; set; }
        private string OrganizationIdValue = string.Empty;

        public string OrganizationId

        {

            get { return this.OrganizationIdValue; }

            set { SetProperty(ref OrganizationIdValue, value); }

        }
        private int HierarchyLevelValue;

        public int HierarchyLevel

        {

            get { return this.HierarchyLevelValue; }

            set { SetProperty(ref HierarchyLevelValue, value); }

        }
        private string? LevelNameValue;

        public string? LevelName

        {

            get { return this.LevelNameValue; }

            set { SetProperty(ref LevelNameValue, value); }

        }
        private string? AssetTypeValue;

        public string? AssetType

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
