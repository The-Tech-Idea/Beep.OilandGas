using System;
using System.Collections.Generic;
using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class DatabaseCreationOptions : ModelEntityBase
    {
        private string DatabaseTypeValue = string.Empty;

        public string DatabaseType

        {

            get { return this.DatabaseTypeValue; }

            set { SetProperty(ref DatabaseTypeValue, value); }

        } // SqlServer, Oracle, etc.
        private string ScriptsPathValue = string.Empty;

        public string ScriptsPath

        {

            get { return this.ScriptsPathValue; }

            set { SetProperty(ref ScriptsPathValue, value); }

        }
        private List<string>? CategoriesValue;

        public List<string>? Categories

        {

            get { return this.CategoriesValue; }

            set { SetProperty(ref CategoriesValue, value); }

        }
        private List<string>? ScriptTypesValue;

        public List<string>? ScriptTypes

        {

            get { return this.ScriptTypesValue; }

            set { SetProperty(ref ScriptTypesValue, value); }

        } // TAB, PK, FK, etc.
        private bool EnableLoggingValue = true;

        public bool EnableLogging

        {

            get { return this.EnableLoggingValue; }

            set { SetProperty(ref EnableLoggingValue, value); }

        }
        private string? LogFilePathValue;

        public string? LogFilePath

        {

            get { return this.LogFilePathValue; }

            set { SetProperty(ref LogFilePathValue, value); }

        }
        private bool ContinueOnErrorValue = false;

        public bool ContinueOnError

        {

            get { return this.ContinueOnErrorValue; }

            set { SetProperty(ref ContinueOnErrorValue, value); }

        }
        private bool EnableRollbackValue = false;

        public bool EnableRollback

        {

            get { return this.EnableRollbackValue; }

            set { SetProperty(ref EnableRollbackValue, value); }

        }
        private bool ExecuteConsolidatedScriptsValue = true;

        public bool ExecuteConsolidatedScripts

        {

            get { return this.ExecuteConsolidatedScriptsValue; }

            set { SetProperty(ref ExecuteConsolidatedScriptsValue, value); }

        }
        private bool ExecuteIndividualScriptsValue = true;

        public bool ExecuteIndividualScripts

        {

            get { return this.ExecuteIndividualScriptsValue; }

            set { SetProperty(ref ExecuteIndividualScriptsValue, value); }

        }
        private bool ExecuteOptionalScriptsValue = false;

        public bool ExecuteOptionalScripts

        {

            get { return this.ExecuteOptionalScriptsValue; }

            set { SetProperty(ref ExecuteOptionalScriptsValue, value); }

        }
        private bool ValidateDependenciesValue = true;

        public bool ValidateDependencies

        {

            get { return this.ValidateDependenciesValue; }

            set { SetProperty(ref ValidateDependenciesValue, value); }

        }
        private bool EnableParallelExecutionValue = false;

        public bool EnableParallelExecution

        {

            get { return this.EnableParallelExecutionValue; }

            set { SetProperty(ref EnableParallelExecutionValue, value); }

        }
        private int? MaxParallelTasksValue;

        public int? MaxParallelTasks

        {

            get { return this.MaxParallelTasksValue; }

            set { SetProperty(ref MaxParallelTasksValue, value); }

        }
        private string? ExecutionIdValue;

        public string? ExecutionId

        {

            get { return this.ExecutionIdValue; }

            set { SetProperty(ref ExecutionIdValue, value); }

        }
    }
}
