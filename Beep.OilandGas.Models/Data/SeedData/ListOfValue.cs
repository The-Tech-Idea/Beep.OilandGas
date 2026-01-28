using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class ListOfValue : ModelEntityBase
    {
        private string ListOfValueIdValue = string.Empty;

        public string ListOfValueId

        {

            get { return this.ListOfValueIdValue; }

            set { SetProperty(ref ListOfValueIdValue, value); }

        }
        private string ValueTypeValue = string.Empty;

        public string ValueType

        {

            get { return this.ValueTypeValue; }

            set { SetProperty(ref ValueTypeValue, value); }

        }
        private string ValueCodeValue = string.Empty;

        public string ValueCode

        {

            get { return this.ValueCodeValue; }

            set { SetProperty(ref ValueCodeValue, value); }

        }
        private string ValueNameValue = string.Empty;

        public string ValueName

        {

            get { return this.ValueNameValue; }

            set { SetProperty(ref ValueNameValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private string? CategoryValue;

        public string? Category

        {

            get { return this.CategoryValue; }

            set { SetProperty(ref CategoryValue, value); }

        }
        private string? ModuleValue;

        public string? Module

        {

            get { return this.ModuleValue; }

            set { SetProperty(ref ModuleValue, value); }

        }
        private int? SortOrderValue;

        public int? SortOrder

        {

            get { return this.SortOrderValue; }

            set { SetProperty(ref SortOrderValue, value); }

        }
        private string? ParentValueIdValue;

        public string? ParentValueId

        {

            get { return this.ParentValueIdValue; }

            set { SetProperty(ref ParentValueIdValue, value); }

        }
        private string? IsDefaultValue;

        public string? IsDefault

        {

            get { return this.IsDefaultValue; }

            set { SetProperty(ref IsDefaultValue, value); }

        }
        private string ActiveIndValue = string.Empty;

        public string ActiveInd

        {

            get { return this.ActiveIndValue; }

            set { SetProperty(ref ActiveIndValue, value); }

        }

        private List<ListOfValue>? ChildrenValue;

        public List<ListOfValue>? Children

        {

            get { return this.ChildrenValue; }

            set { SetProperty(ref ChildrenValue, value); }

        }
    }
}
