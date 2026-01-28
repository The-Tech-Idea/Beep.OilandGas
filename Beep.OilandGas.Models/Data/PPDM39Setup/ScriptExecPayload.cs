using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.Models.Data.DataManagement;

namespace Beep.OilandGas.Models.Data
{
    public class ScriptExecPayload : ModelEntityBase
    {
        /// <summary>
        /// Connection properties to use for script execution. If null, the current configured connection will be used.
        /// </summary>
        private ConnectionProperties? ConnectionValue;

        [Required]
        public ConnectionProperties? Connection

        {

            get { return this.ConnectionValue; }

            set { SetProperty(ref ConnectionValue, value); }

        }

        /// <summary>
        /// Single script name to execute (if not executing multiple)
        /// </summary>
        private string? ScriptNameValue;

        public string? ScriptName

        {

            get { return this.ScriptNameValue; }

            set { SetProperty(ref ScriptNameValue, value); }

        }

        /// <summary>
        /// When true, execute all discovered scripts for the database type
        /// </summary>
        private bool ExecuteAllValue = false;

        public bool ExecuteAll

        {

            get { return this.ExecuteAllValue; }

            set { SetProperty(ref ExecuteAllValue, value); }

        }

        /// <summary>
        /// Optional explicit list of script file names to execute
        /// </summary>
        private List<string>? ScriptNamesValue;

        public List<string>? ScriptNames

        {

            get { return this.ScriptNamesValue; }

            set { SetProperty(ref ScriptNamesValue, value); }

        }

        /// <summary>
        /// If the operation originated from an existing connection name, set it here so logs can reference the original
        /// </summary>
        private string? OriginalConnectionNameValue;

        public string? OriginalConnectionName

        {

            get { return this.OriginalConnectionNameValue; }

            set { SetProperty(ref OriginalConnectionNameValue, value); }

        }

        /// <summary>
        /// Continue execution when one script fails
        /// </summary>
        private bool ContinueOnErrorValue = false;

        public bool ContinueOnError

        {

            get { return this.ContinueOnErrorValue; }

            set { SetProperty(ref ContinueOnErrorValue, value); }

        }

        /// <summary>
        /// Enable rollback on failure if supported by the provider
        /// </summary>
        private bool EnableRollbackValue = false;

        public bool EnableRollback

        {

            get { return this.EnableRollbackValue; }

            set { SetProperty(ref EnableRollbackValue, value); }

        }

        /// <summary>
        /// When true, drop objects if they exist before creating (where applicable)
        /// </summary>
        private bool DropIfExistsValue = false;

        public bool DropIfExists

        {

            get { return this.DropIfExistsValue; }

            set { SetProperty(ref DropIfExistsValue, value); }

        }

        /// <summary>
        /// Optional operation id for progress tracking
        /// </summary>
        private string? OperationIdValue;

        public string? OperationId

        {

            get { return this.OperationIdValue; }

            set { SetProperty(ref OperationIdValue, value); }

        }

        /// <summary>
        /// Optional user id performing the operation
        /// </summary>
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }
}
