using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.Models.Data.DataManagement;

namespace Beep.OilandGas.Models.Data
{
    public class CopyDatabaseRequest : ModelEntityBase
    {
        private string SourceConnectionNameValue = string.Empty;

        [Required]
        public string SourceConnectionName

        {

            get { return this.SourceConnectionNameValue; }

            set { SetProperty(ref SourceConnectionNameValue, value); }

        }
        private string TargetConnectionNameValue = string.Empty;

        [Required]
        public string TargetConnectionName

        {

            get { return this.TargetConnectionNameValue; }

            set { SetProperty(ref TargetConnectionNameValue, value); }

        }
        private string? SourceSchemaValue;

        public string? SourceSchema

        {

            get { return this.SourceSchemaValue; }

            set { SetProperty(ref SourceSchemaValue, value); }

        }
        private string? TargetSchemaValue;

        public string? TargetSchema

        {

            get { return this.TargetSchemaValue; }

            set { SetProperty(ref TargetSchemaValue, value); }

        }
        private bool CopyStructureOnlyValue = false;

        public bool CopyStructureOnly

        {

            get { return this.CopyStructureOnlyValue; }

            set { SetProperty(ref CopyStructureOnlyValue, value); }

        }
        private bool CopyDataValue = true;

        public bool CopyData

        {

            get { return this.CopyDataValue; }

            set { SetProperty(ref CopyDataValue, value); }

        }
        private bool TruncateTargetTablesValue = false;

        public bool TruncateTargetTables

        {

            get { return this.TruncateTargetTablesValue; }

            set { SetProperty(ref TruncateTargetTablesValue, value); }

        }
        private List<string>? TableNamesValue;

        public List<string>? TableNames

        {

            get { return this.TableNamesValue; }

            set { SetProperty(ref TableNamesValue, value); }

        }
        private string? OperationIdValue;

        public string? OperationId

        {

            get { return this.OperationIdValue; }

            set { SetProperty(ref OperationIdValue, value); }

        }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }
}
