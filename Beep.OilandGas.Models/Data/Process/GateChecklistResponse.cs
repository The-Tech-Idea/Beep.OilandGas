using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class GateChecklistResponse : ModelEntityBase
    {
        private string GateIdValue = string.Empty;

        public string GateId
        {
            get { return this.GateIdValue; }
            set { SetProperty(ref GateIdValue, value); }
        }

        private List<string> RequiredDocumentsValue = new();

        public List<string> RequiredDocuments
        {
            get { return this.RequiredDocumentsValue; }
            set { SetProperty(ref RequiredDocumentsValue, value); }
        }
    }
}