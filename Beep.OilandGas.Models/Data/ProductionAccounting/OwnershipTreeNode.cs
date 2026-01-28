using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class OwnershipTreeNode : ModelEntityBase
    {
        private string NodeIdValue = string.Empty;

        public string NodeId

        {

            get { return this.NodeIdValue; }

            set { SetProperty(ref NodeIdValue, value); }

        }
        private string NodeNameValue = string.Empty;

        public string NodeName

        {

            get { return this.NodeNameValue; }

            set { SetProperty(ref NodeNameValue, value); }

        }
        private string NodeTypeValue = string.Empty;

        public string NodeType

        {

            get { return this.NodeTypeValue; }

            set { SetProperty(ref NodeTypeValue, value); }

        }
        private decimal WorkingInterestValue;

        public decimal WorkingInterest

        {

            get { return this.WorkingInterestValue; }

            set { SetProperty(ref WorkingInterestValue, value); }

        }
        private decimal NetRevenueInterestValue;

        public decimal NetRevenueInterest

        {

            get { return this.NetRevenueInterestValue; }

            set { SetProperty(ref NetRevenueInterestValue, value); }

        }
        private List<OwnershipTreeNode> ChildrenValue = new();

        public List<OwnershipTreeNode> Children

        {

            get { return this.ChildrenValue; }

            set { SetProperty(ref ChildrenValue, value); }

        }
    }
}
