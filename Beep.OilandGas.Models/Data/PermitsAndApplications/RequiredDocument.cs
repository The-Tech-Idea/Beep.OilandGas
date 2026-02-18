namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public class RequiredDocument
    {
        public string DocumentType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsMandatory { get; set; }
    }
}
