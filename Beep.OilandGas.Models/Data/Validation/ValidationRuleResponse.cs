using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Validation
{
    public class ValidationRuleResponse : ModelEntityBase
    {
        private string RuleIdValue;

        public string RuleId

        {

            get { return this.RuleIdValue; }

            set { SetProperty(ref RuleIdValue, value); }

        }
        private string EntityTypeValue;

        public string EntityType

        {

            get { return this.EntityTypeValue; }

            set { SetProperty(ref EntityTypeValue, value); }

        }
        private string RuleNameValue;

        public string RuleName

        {

            get { return this.RuleNameValue; }

            set { SetProperty(ref RuleNameValue, value); }

        }
        private string RuleDefinitionValue;

        public string RuleDefinition

        {

            get { return this.RuleDefinitionValue; }

            set { SetProperty(ref RuleDefinitionValue, value); }

        }
        private string RuleTypeValue;

        public string RuleType

        {

            get { return this.RuleTypeValue; }

            set { SetProperty(ref RuleTypeValue, value); }

        }
        private bool IsActiveValue;

        public bool IsActive

        {

            get { return this.IsActiveValue; }

            set { SetProperty(ref IsActiveValue, value); }

        }
    }
}
