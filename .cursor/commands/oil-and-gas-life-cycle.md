# oil-and-gas-life-cycle
follow plans in C:\Users\f_ald\source\repos\The-Tech-Idea\Beep.OilandGas\Beep.OilandGas.LifeCycle\plans

// Bad: Using ExecuteSql for SELECT queries
var sql = "SELECT * FROM TABLE WHERE ID = @id";
dataSource.ExecuteSql(sql); // WRONG - ExecuteSql is for DDL only

// Bad: Using GetDataTable
var dataTable = dataSource.GetDataTable(sql); // WRONG - should use RunQuery

// BEST: Use GetEntityAsync with AppFilter for simple single-table queries (PREFERRED)
var dataSource = _editor.GetDataSource(connectionName);
var filters = new List<AppFilter> 
{ 
    new AppFilter { FieldName = "ID", Operator = "=", FilterValue = id } 
};
var results = await dataSource.GetEntityAsync("TABLE", filters); // Returns IEnumerable<object>

// Good: Use GetScalar for scalar queries (COUNT, SUM, EXISTS)
var paramDelim = dataSource.ParameterDelimiter;
var sql = $"SELECT COUNT(*) FROM TABLE WHERE ID = {paramDelim}id";
var count = dataSource.GetScalar(sql); // Returns double

// Use RunQuery only for complex multi-table queries or custom SQL
var sql2 = $"SELECT t1.*, t2.name FROM TABLE1 t1 JOIN TABLE2 t2 ON t1.id = t2.id";
var results2 = dataSource.RunQuery(sql2); // Returns IEnumerable<object>

// Good: ExecuteSql ONLY for DDL (CREATE, ALTER, DROP)
var createSql = "CREATE TABLE ...";
dataSource.ExecuteSql(createSql); // CORRECT - DDL doesn't return data

// Better: Use QueryBuilder for parameterized queries
var sql = QueryBuilder.BuildSelectQuery(dataSource, "TABLE", null, 
    new Dictionary<string, object> { { "ID", id } }, out var parameters);
var results = dataSource.RunQuery(sql);

Key Principles
Use GetEntityAsync with AppFilter for simple single-table queries - PREFERRED - Clean, maintainable, handles delimiters automatically
Use GetScalar/GetScalarAsync for scalar queries - Returns double, use for single-value queries (COUNT, SUM, EXISTS checks)
Use RunQuery for complex multi-table queries - Returns IEnumerable<object>, use only when GetEntityAsync is insufficient
Use ExecuteSql ONLY for DDL - CREATE, ALTER, DROP statements that don't return data
Never hardcode @ in SQL queries - Always use IDataSource.ParameterDelimiter (not needed with GetEntityAsync + AppFilter)
Use PPDMGenericRepository when possible - It handles delimiters automatically and uses GetEntityAsync internally
Use QueryBuilder for custom queries - Provides database-agnostic query construction (only when needed)
Access IDataSource via IDMEEditor: _editor.GetDataSource(connectionName)
Column delimiters: Use IDataSource.ColumnDelimiter for column names if needed (handled automatically by GetEntityAsync)
Files to Modify

Key Principle
Entity objects are passed directly to IDataSource.InsertEntity(tableName, entity) and IDataSource.UpdateEntity(tableName, entity)
Entity objects are returned directly from IDataSource.GetEntityAsync(tableName, filters) 
NO Dictionary<string, object> conversions - remove all Convert*ToDictionary and ConvertDictionaryTo* methods
Use DTOs for API input/output, convert DTOs ↔ Entity for database operations
