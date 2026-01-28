using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public partial class APPLICATION_ATTACHMENT : ModelEntityBase {
        private String APPLICATION_ATTACHMENT_IDValue;
        public String APPLICATION_ATTACHMENT_ID
        {
            get { return this.APPLICATION_ATTACHMENT_IDValue; }
            set { SetProperty(ref APPLICATION_ATTACHMENT_IDValue, value); }
        }

        private String PERMIT_APPLICATION_IDValue;
        public String PERMIT_APPLICATION_ID
        {
            get { return this.PERMIT_APPLICATION_IDValue; }
            set { SetProperty(ref PERMIT_APPLICATION_IDValue, value); }
        }

        private String FILE_NAMEValue;
        public String FILE_NAME
        {
            get { return this.FILE_NAMEValue; }
            set { SetProperty(ref FILE_NAMEValue, value); }
        }

        private String FILE_TYPEValue;
        public String FILE_TYPE
        {
            get { return this.FILE_TYPEValue; }
            set { SetProperty(ref FILE_TYPEValue, value); }
        }

        private Int64? FILE_SIZEValue;
        public Int64? FILE_SIZE
        {
            get { return this.FILE_SIZEValue; }
            set { SetProperty(ref FILE_SIZEValue, value); }
        }

        private DateTime? UPLOAD_DATEValue;
        public DateTime? UPLOAD_DATE
        {
            get { return this.UPLOAD_DATEValue; }
            set { SetProperty(ref UPLOAD_DATEValue, value); }
        }

        private String DESCRIPTIONValue;
        public String DESCRIPTION
        {
            get { return this.DESCRIPTIONValue; }
            set { SetProperty(ref DESCRIPTIONValue, value); }
        }

        private String DOCUMENT_TYPEValue;
        public String DOCUMENT_TYPE
        {
            get { return this.DOCUMENT_TYPEValue; }
            set { SetProperty(ref DOCUMENT_TYPEValue, value); }
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

    }
}
