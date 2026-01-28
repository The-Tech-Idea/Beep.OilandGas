using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class AssetHierarchyNode : ModelEntityBase
    {
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
