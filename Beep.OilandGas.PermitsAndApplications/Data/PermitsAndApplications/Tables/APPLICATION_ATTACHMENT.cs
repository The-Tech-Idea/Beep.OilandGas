using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.PermitsAndApplications.Data.PermitTables
{
    public partial class APPLICATION_ATTACHMENT : ModelEntityBase
    {
        private string _applicationAttachmentId = string.Empty;
        public string APPLICATION_ATTACHMENT_ID { get => _applicationAttachmentId; set => SetProperty(ref _applicationAttachmentId, value); }

        private string _permitApplicationId = string.Empty;
        public string PERMIT_APPLICATION_ID { get => _permitApplicationId; set => SetProperty(ref _permitApplicationId, value); }

        private string _fileName = string.Empty;
        public string FILE_NAME { get => _fileName; set => SetProperty(ref _fileName, value); }

        private string? _fileType;
        public string? FILE_TYPE { get => _fileType; set => SetProperty(ref _fileType, value); }

        private string? _description;
        public string? DESCRIPTION { get => _description; set => SetProperty(ref _description, value); }

        private string? _filePath;
        public string? FILE_PATH { get => _filePath; set => SetProperty(ref _filePath, value); }

        private DateTime _uploadDate;
        public DateTime UPLOAD_DATE { get => _uploadDate; set => SetProperty(ref _uploadDate, value); }

        private string _uploadedBy = string.Empty;
        public string UPLOADED_BY { get => _uploadedBy; set => SetProperty(ref _uploadedBy, value); }

        private long? _fileSize;
        public long? FILE_SIZE { get => _fileSize; set => SetProperty(ref _fileSize, value); }

        private string? _documentType;
        public string? DOCUMENT_TYPE { get => _documentType; set => SetProperty(ref _documentType, value); }
    }
}
