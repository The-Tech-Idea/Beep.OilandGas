using System.Data;
using System.Text.Json;
using Beep.OilandGas.DataManager.Core.Exceptions;
using Beep.OilandGas.DataManager.Core.Models;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.DataManager.Core.State
{
    /// <summary>
    /// Database-based implementation of execution state store
    /// </summary>
    public class DatabaseExecutionStateStore : IExecutionStateStore
    {
        private readonly IDataSource _dataSource;
        private readonly ILogger<DatabaseExecutionStateStore>? _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private const string TableName = "EXECUTION_STATE";

        public DatabaseExecutionStateStore(IDataSource dataSource, ILogger<DatabaseExecutionStateStore>? logger = null)
        {
            _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            EnsureTableExists();
        }

        public async Task SaveStateAsync(ExecutionState state)
        {
            try
            {
                var json = JsonSerializer.Serialize(state, _jsonOptions);
                var paramDelim = _dataSource.ParameterDelimiter;

                // Check if state exists
                var checkSql = $"SELECT COUNT(*) FROM {TableName} WHERE EXECUTION_ID = {paramDelim}executionId";
                var exists = _dataSource.GetScalar(checkSql) > 0;

                if (exists)
                {
                    // Update existing state
                    var updateSql = $@"
                        UPDATE {TableName} 
                        SET MODULE_NAME = {paramDelim}moduleName,
                            DATABASE_TYPE = {paramDelim}databaseType,
                            CONNECTION_NAME = {paramDelim}connectionName,
                            START_TIME = {paramDelim}startTime,
                            LAST_CHECKPOINT = {paramDelim}lastCheckpoint,
                            STATE_JSON = {paramDelim}stateJson,
                            IS_COMPLETED = {paramDelim}isCompleted,
                            ERROR_MESSAGE = {paramDelim}errorMessage,
                            UPDATED_DATE = {paramDelim}updatedDate
                        WHERE EXECUTION_ID = {paramDelim}executionId";

                    var updateParams = new Dictionary<string, object>
                    {
                        { "executionId", state.ExecutionId },
                        { "moduleName", state.ModuleName },
                        { "databaseType", state.DatabaseType },
                        { "connectionName", state.ConnectionName },
                        { "startTime", state.StartTime },
                        { "lastCheckpoint", state.LastCheckpoint ?? (object)DBNull.Value },
                        { "stateJson", json },
                        { "isCompleted", state.IsCompleted ? 1 : 0 },
                        { "errorMessage", state.ErrorMessage ?? (object)DBNull.Value },
                        { "updatedDate", DateTime.UtcNow }
                    };

                    _dataSource.ExecuteSql(updateSql);
                }
                else
                {
                    // Insert new state
                    var insertSql = $@"
                        INSERT INTO {TableName} 
                        (EXECUTION_ID, MODULE_NAME, DATABASE_TYPE, CONNECTION_NAME, START_TIME, LAST_CHECKPOINT, STATE_JSON, IS_COMPLETED, ERROR_MESSAGE, CREATED_DATE, UPDATED_DATE)
                        VALUES 
                        ({paramDelim}executionId, {paramDelim}moduleName, {paramDelim}databaseType, {paramDelim}connectionName, 
                         {paramDelim}startTime, {paramDelim}lastCheckpoint, {paramDelim}stateJson, {paramDelim}isCompleted, 
                         {paramDelim}errorMessage, {paramDelim}createdDate, {paramDelim}updatedDate)";

                    var insertParams = new Dictionary<string, object>
                    {
                        { "executionId", state.ExecutionId },
                        { "moduleName", state.ModuleName },
                        { "databaseType", state.DatabaseType },
                        { "connectionName", state.ConnectionName },
                        { "startTime", state.StartTime },
                        { "lastCheckpoint", state.LastCheckpoint ?? (object)DBNull.Value },
                        { "stateJson", json },
                        { "isCompleted", state.IsCompleted ? 1 : 0 },
                        { "errorMessage", state.ErrorMessage ?? (object)DBNull.Value },
                        { "createdDate", DateTime.UtcNow },
                        { "updatedDate", DateTime.UtcNow }
                    };

                    _dataSource.ExecuteSql(insertSql);
                }

                _logger?.LogDebug("Saved execution state to database: {ExecutionId}", state.ExecutionId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to save execution state to database: {ExecutionId}", state.ExecutionId);
                throw new DataManagerException($"Failed to save execution state to database: {ex.Message}", ex);
            }
        }

        public async Task<ExecutionState?> LoadStateAsync(string executionId)
        {
            try
            {
                var paramDelim = _dataSource.ParameterDelimiter;
                var sql = $"SELECT STATE_JSON FROM {TableName} WHERE EXECUTION_ID = {paramDelim}executionId";
                var result = _dataSource.RunQuery(sql);

                var row = result.FirstOrDefault();
                if (row == null)
                {
                    _logger?.LogDebug("Execution state not found in database: {ExecutionId}", executionId);
                    return null;
                }

                // Extract JSON from result (assuming it's in a property or dictionary)
                string json;
                if (row is Dictionary<string, object> dict && dict.ContainsKey("STATE_JSON"))
                {
                    json = dict["STATE_JSON"]?.ToString() ?? string.Empty;
                }
                else
                {
                    // Try to get by index or reflection
                    var props = row.GetType().GetProperties();
                    var jsonProp = props.FirstOrDefault(p => p.Name.Equals("STATE_JSON", StringComparison.OrdinalIgnoreCase));
                    json = jsonProp?.GetValue(row)?.ToString() ?? string.Empty;
                }

                if (string.IsNullOrEmpty(json))
                {
                    return null;
                }

                var state = JsonSerializer.Deserialize<ExecutionState>(json, _jsonOptions);
                _logger?.LogDebug("Loaded execution state from database: {ExecutionId}", executionId);
                return state;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to load execution state from database: {ExecutionId}", executionId);
                throw new DataManagerException($"Failed to load execution state from database: {ex.Message}", ex);
            }
        }

        public async Task DeleteStateAsync(string executionId)
        {
            try
            {
                var paramDelim = _dataSource.ParameterDelimiter;
                var sql = $"DELETE FROM {TableName} WHERE EXECUTION_ID = {paramDelim}executionId";
                _dataSource.ExecuteSql(sql);
                _logger?.LogDebug("Deleted execution state from database: {ExecutionId}", executionId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to delete execution state from database: {ExecutionId}", executionId);
                throw new DataManagerException($"Failed to delete execution state from database: {ex.Message}", ex);
            }
        }

        public async Task<List<ExecutionState>> GetIncompleteExecutionsAsync()
        {
            var incompleteStates = new List<ExecutionState>();

            try
            {
                var sql = $"SELECT STATE_JSON FROM {TableName} WHERE IS_COMPLETED = 0";
                var results = _dataSource.RunQuery(sql);

                foreach (var row in results)
                {
                    try
                    {
                        string json;
                        if (row is Dictionary<string, object> dict && dict.ContainsKey("STATE_JSON"))
                        {
                            json = dict["STATE_JSON"]?.ToString() ?? string.Empty;
                        }
                        else
                        {
                            var props = row.GetType().GetProperties();
                            var jsonProp = props.FirstOrDefault(p => p.Name.Equals("STATE_JSON", StringComparison.OrdinalIgnoreCase));
                            json = jsonProp?.GetValue(row)?.ToString() ?? string.Empty;
                        }

                        if (!string.IsNullOrEmpty(json))
                        {
                            var state = JsonSerializer.Deserialize<ExecutionState>(json, _jsonOptions);
                            if (state != null)
                            {
                                incompleteStates.Add(state);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Failed to deserialize execution state from database");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to get incomplete executions from database");
                throw new DataManagerException($"Failed to get incomplete executions from database: {ex.Message}", ex);
            }

            return incompleteStates;
        }

        private void EnsureTableExists()
        {
            try
            {
                var paramDelim = _dataSource.ParameterDelimiter;
                var checkTableSql = GetCheckTableExistsSql();
                var tableExists = _dataSource.GetScalar(checkTableSql) > 0;

                if (!tableExists)
                {
                    var createTableSql = GetCreateTableSql();
                    _dataSource.ExecuteSql(createTableSql);
                    _logger?.LogInformation("Created EXECUTION_STATE table");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Could not ensure EXECUTION_STATE table exists. It may need to be created manually.");
            }
        }

        private string GetCheckTableExistsSql()
        {

            return _dataSource.Dataconnection.ConnectionProp.DatabaseType switch
            {
                 TheTechIdea.Beep.Utilities.DataSourceType.SqlServer or TheTechIdea.Beep.Utilities.DataSourceType.SqlLite => "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EXECUTION_STATE'",
                TheTechIdea.Beep.Utilities.DataSourceType.Postgre => "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = 'execution_state'",
                TheTechIdea.Beep.Utilities.DataSourceType.Oracle=> "SELECT COUNT(*) FROM user_tables WHERE table_name = 'EXECUTION_STATE'",
               TheTechIdea.Beep.Utilities.DataSourceType.Mysql or  TheTechIdea.Beep.Utilities.DataSourceType.MariaDB => "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = 'EXECUTION_STATE'",
                _ => "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EXECUTION_STATE'"
            };
        }

        private string GetCreateTableSql()
        {
            return _dataSource.Dataconnection.ConnectionProp.DatabaseType switch
            {
               TheTechIdea.Beep.Utilities.DataSourceType.SqlServer => @"
                    CREATE TABLE EXECUTION_STATE (
                        EXECUTION_ID NVARCHAR(100) PRIMARY KEY,
                        MODULE_NAME NVARCHAR(100) NOT NULL,
                        DATABASE_TYPE NVARCHAR(50) NOT NULL,
                        CONNECTION_NAME NVARCHAR(100) NOT NULL,
                        START_TIME DATETIME NOT NULL,
                        LAST_CHECKPOINT DATETIME,
                        STATE_JSON NVARCHAR(MAX) NOT NULL,
                        IS_COMPLETED BIT NOT NULL DEFAULT 0,
                        ERROR_MESSAGE NVARCHAR(MAX),
                        CREATED_DATE DATETIME NOT NULL DEFAULT GETDATE(),
                        UPDATED_DATE DATETIME NOT NULL DEFAULT GETDATE()
                    )",
                TheTechIdea.Beep.Utilities.DataSourceType.SqlLite => @"
                    CREATE TABLE EXECUTION_STATE (
                        EXECUTION_ID TEXT PRIMARY KEY,
                        MODULE_NAME TEXT NOT NULL,
                        DATABASE_TYPE TEXT NOT NULL,
                        CONNECTION_NAME TEXT NOT NULL,
                        START_TIME TEXT NOT NULL,
                        LAST_CHECKPOINT TEXT,
                        STATE_JSON TEXT NOT NULL,
                        IS_COMPLETED INTEGER NOT NULL DEFAULT 0,
                        ERROR_MESSAGE TEXT,
                        CREATED_DATE TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        UPDATED_DATE TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
                    )",
                TheTechIdea.Beep.Utilities.DataSourceType.Postgre => @"
                    CREATE TABLE execution_state (
                        execution_id VARCHAR(100) PRIMARY KEY,
                        module_name VARCHAR(100) NOT NULL,
                        database_type VARCHAR(50) NOT NULL,
                        connection_name VARCHAR(100) NOT NULL,
                        start_time TIMESTAMP NOT NULL,
                        last_checkpoint TIMESTAMP,
                        state_json TEXT NOT NULL,
                        is_completed BOOLEAN NOT NULL DEFAULT FALSE,
                        error_message TEXT,
                        created_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        updated_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
                    )",
               TheTechIdea.Beep.Utilities.DataSourceType.Oracle => @"
                    CREATE TABLE execution_state (
                        execution_id VARCHAR2(100) PRIMARY KEY,
                        module_name VARCHAR2(100) NOT NULL,
                        database_type VARCHAR2(50) NOT NULL,
                        connection_name VARCHAR2(100) NOT NULL,
                        start_time DATE NOT NULL,
                        last_checkpoint DATE,
                        state_json CLOB NOT NULL,
                        is_completed NUMBER(1) NOT NULL DEFAULT 0,
                        error_message CLOB,
                        created_date DATE NOT NULL DEFAULT SYSDATE,
                        updated_date DATE NOT NULL DEFAULT SYSDATE
                    )",
                TheTechIdea.Beep.Utilities.DataSourceType.Mysql or TheTechIdea.Beep.Utilities.DataSourceType.MariaDB => @"
                    CREATE TABLE EXECUTION_STATE (
                        EXECUTION_ID VARCHAR(100) PRIMARY KEY,
                        MODULE_NAME VARCHAR(100) NOT NULL,
                        DATABASE_TYPE VARCHAR(50) NOT NULL,
                        CONNECTION_NAME VARCHAR(100) NOT NULL,
                        START_TIME DATETIME NOT NULL,
                        LAST_CHECKPOINT DATETIME,
                        STATE_JSON TEXT NOT NULL,
                        IS_COMPLETED TINYINT(1) NOT NULL DEFAULT 0,
                        ERROR_MESSAGE TEXT,
                        CREATED_DATE DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        UPDATED_DATE DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
                    )",
                _ => @"
                    CREATE TABLE EXECUTION_STATE (
                        EXECUTION_ID NVARCHAR(100) PRIMARY KEY,
                        MODULE_NAME NVARCHAR(100) NOT NULL,
                        DATABASE_TYPE NVARCHAR(50) NOT NULL,
                        CONNECTION_NAME NVARCHAR(100) NOT NULL,
                        START_TIME DATETIME NOT NULL,
                        LAST_CHECKPOINT DATETIME,
                        STATE_JSON NVARCHAR(MAX) NOT NULL,
                        IS_COMPLETED BIT NOT NULL DEFAULT 0,
                        ERROR_MESSAGE NVARCHAR(MAX),
                        CREATED_DATE DATETIME NOT NULL,
                        UPDATED_DATE DATETIME NOT NULL
                    )"
            };
        }
    }
}
