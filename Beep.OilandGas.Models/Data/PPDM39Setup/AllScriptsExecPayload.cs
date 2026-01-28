using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.Models.Data.DataManagement;

namespace Beep.OilandGas.Models.Data
{
    public class AllScriptsExecPayload : ModelEntityBase
    {
        /// <summary>
        /// Connection properties to use for the operation
        /// </summary>
        private ConnectionProperties? ConnectionValue;

        [Required]
        public ConnectionProperties? Connection

        {

            get { return this.ConnectionValue; }

            set { SetProperty(ref ConnectionValue, value); }

        }

        /// <summary>
        /// Execute all discovered scripts
        /// </summary>
        private bool ExecuteAllValue = true;

        public bool ExecuteAll

        {

            get { return this.ExecuteAllValue; }

            set { SetProperty(ref ExecuteAllValue, value); }

        }

        /// <summary>
        /// Optional subset of script names to execute instead of all
        /// </summary>
        private List<string>? ScriptNamesValue;

        public List<string>? ScriptNames

        {

            get { return this.ScriptNamesValue; }

            set { SetProperty(ref ScriptNamesValue, value); }

        }

        /// <summary>
        /// Execute consolidated scripts (TAB, PK, etc.)
        /// </summary>
        private bool ExecuteConsolidatedScriptsValue = true;

        public bool ExecuteConsolidatedScripts

        {

            get { return this.ExecuteConsolidatedScriptsValue; }

            set { SetProperty(ref ExecuteConsolidatedScriptsValue, value); }

        }

        /// <summary>
        /// Execute individual table scripts
        /// </summary>
        private bool ExecuteIndividualScriptsValue = true;

        public bool ExecuteIndividualScripts

        {

            get { return this.ExecuteIndividualScriptsValue; }

            set { SetProperty(ref ExecuteIndividualScriptsValue, value); }

        }

        /// <summary>
        /// Execute optional/supporting scripts
        /// </summary>
        private bool ExecuteOptionalScriptsValue = false;

        public bool ExecuteOptionalScripts

        {

            get { return this.ExecuteOptionalScriptsValue; }

            set { SetProperty(ref ExecuteOptionalScriptsValue, value); }

        }

        /// <summary>
        /// Continue execution when a script fails
        /// </summary>
        private bool ContinueOnErrorValue = false;

        public bool ContinueOnError

        {

            get { return this.ContinueOnErrorValue; }

            set { SetProperty(ref ContinueOnErrorValue, value); }

        }

        /// <summary>
        /// Enable rollback on failure if supported
        /// </summary>
        private bool EnableRollbackValue = false;

        public bool EnableRollback

        {

            get { return this.EnableRollbackValue; }

            set { SetProperty(ref EnableRollbackValue, value); }

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
