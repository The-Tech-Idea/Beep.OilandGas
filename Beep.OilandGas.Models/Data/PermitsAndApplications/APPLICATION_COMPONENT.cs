using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public partial class APPLICATION_COMPONENT : ModelEntityBase {
        private String APPLICATION_COMPONENT_IDValue;
        public String APPLICATION_COMPONENT_ID
        {
            get { return this.APPLICATION_COMPONENT_IDValue; }
            set { SetProperty(ref APPLICATION_COMPONENT_IDValue, value); }
        }

        private String PERMIT_APPLICATION_IDValue;
        public String PERMIT_APPLICATION_ID
        {
            get { return this.PERMIT_APPLICATION_IDValue; }
            set { SetProperty(ref PERMIT_APPLICATION_IDValue, value); }
        }

        private String COMPONENT_TYPEValue;
        public String COMPONENT_TYPE
        {
            get { return this.COMPONENT_TYPEValue; }
            set { SetProperty(ref COMPONENT_TYPEValue, value); }
        }

        private String DESCRIPTIONValue;
        public String DESCRIPTION
        {
            get { return this.DESCRIPTIONValue; }
            set { SetProperty(ref DESCRIPTIONValue, value); }
        }

        private String VALUEValue;
        public String VALUE
        {
            get { return this.VALUEValue; }
            set { SetProperty(ref VALUEValue, value); }
        }

        private Int32? SEQUENCE_NUMBERValue;
        public Int32? SEQUENCE_NUMBER
        {
            get { return this.SEQUENCE_NUMBERValue; }
            set { SetProperty(ref SEQUENCE_NUMBERValue, value); }
        }

        // Standard PPDM columns

        // Optional PPDM properties
        private String AREA_IDValue;
        public String AREA_ID
        {
            get { return this.AREA_IDValue; }
            set { SetProperty(ref AREA_IDValue, value); }
        }

        private String AREA_TYPEValue;
        public String AREA_TYPE
        {
            get { return this.AREA_TYPEValue; }
            set { SetProperty(ref AREA_TYPEValue, value); }
        }

        private String BUSINESS_ASSOCIATE_IDValue;
        public String BUSINESS_ASSOCIATE_ID
        {
            get { return this.BUSINESS_ASSOCIATE_IDValue; }
            set { SetProperty(ref BUSINESS_ASSOCIATE_IDValue, value); }
        }

        public object? Value { get; set; }
        public int SequenceNumber { get; set; }
        public object Description { get; set; }
        public string? ComponentType { get; set; }
        public string ComponentId { get; set; }
    }
}


