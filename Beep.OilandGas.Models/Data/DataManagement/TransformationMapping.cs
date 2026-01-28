using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class TransformationMapping : ModelEntityBase
    {
        private Type SourceTypeValue;

        public Type SourceType

        {

            get { return this.SourceTypeValue; }

            set { SetProperty(ref SourceTypeValue, value); }

        }
        private Type TargetTypeValue;

        public Type TargetType

        {

            get { return this.TargetTypeValue; }

            set { SetProperty(ref TargetTypeValue, value); }

        }
        public Dictionary<string, string> FieldMapping { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, Func<object, object>> CustomTransformations { get; set; } = new Dictionary<string, Func<object, object>>();
        private List<string> FieldsToIncludeValue = new List<string>();

        public List<string> FieldsToInclude

        {

            get { return this.FieldsToIncludeValue; }

            set { SetProperty(ref FieldsToIncludeValue, value); }

        }
        private List<string> FieldsToExcludeValue = new List<string>();

        public List<string> FieldsToExclude

        {

            get { return this.FieldsToExcludeValue; }

            set { SetProperty(ref FieldsToExcludeValue, value); }

        }
    }
}
