using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class AssetHierarchyNode : ModelEntityBase
    {
        public string? LevelName { get; set; }
        /// <summary>Recorded order relationships; these do not grant access to the referenced assets.</summary>
        public List<Beep.OilandGas.Models.Data.WorkOrder.WorkOrderComponentReference> WorkOrderComponents { get; set; } = [];
        private string AssetIdValue = string.Empty;

        public string AssetId

        {

            get { return this.AssetIdValue; }

            set { SetProperty(ref AssetIdValue, value); }

        }
        private string AssetTypeValue = string.Empty;

        public string AssetType

        {

            get { return this.AssetTypeValue; }

            set { SetProperty(ref AssetTypeValue, value); }

        }
        /// <summary>Completes the FACILITY_ID key; required only for FACILITY nodes.</summary>
        public string? FacilityType { get; set; }

        private string? AssetNameValue;

        public string? AssetName

        {

            get { return this.AssetNameValue; }

            set { SetProperty(ref AssetNameValue, value); }

        }
        private string? ParentAssetIdValue;

        public string? ParentAssetId

        {

            get { return this.ParentAssetIdValue; }

            set { SetProperty(ref ParentAssetIdValue, value); }

        }
        private string? ParentAssetTypeValue;

        public string? ParentAssetType

        {

            get { return this.ParentAssetTypeValue; }

            set { SetProperty(ref ParentAssetTypeValue, value); }

        }
        private List<AssetHierarchyNode> ChildrenValue = new List<AssetHierarchyNode>();

        public List<AssetHierarchyNode> Children

        {

            get { return this.ChildrenValue; }

            set { SetProperty(ref ChildrenValue, value); }

        }
        private bool UserHasAccessValue;

        public bool UserHasAccess

        {

            get { return this.UserHasAccessValue; }

            set { SetProperty(ref UserHasAccessValue, value); }

        }
    }
}
