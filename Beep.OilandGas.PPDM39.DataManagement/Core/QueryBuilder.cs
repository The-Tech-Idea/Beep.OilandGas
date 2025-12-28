using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheTechIdea.Beep;

namespace Beep.OilandGas.PPDM39.DataManagement.Core
{
    /// <summary>
    /// Utility class for building parameterized SQL queries using IDataSource delimiters
    /// Ensures database compatibility across SQL Server, Oracle, PostgreSQL, MySQL, etc.
    /// </summary>
    public static class QueryBuilder
    {
        /// <summary>
        /// Builds a parameterized SELECT query with WHERE clause
        /// </summary>
        /// <param name="dataSource">IDataSource instance to get delimiters from</param>
        /// <param name="tableName">Table name to query</param>
        /// <param name="columns">Column names to select (null or empty = SELECT *)</param>
        /// <param name="filters">Dictionary of field names and values for WHERE clause</param>
        /// <param name="parameters">Output dictionary of parameter names and values</param>
        /// <returns>Parameterized SQL query string</returns>
        public static string BuildSelectQuery(
            IDataSource dataSource,
            string tableName,
            out Dictionary<string, object> parameters,
            IEnumerable<string>? columns = null,
            Dictionary<string, object>? filters = null)
        {
            if (dataSource == null)
                throw new ArgumentNullException(nameof(dataSource));
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));

            var paramDelim = dataSource.ParameterDelimiter ?? "@";
            var columnDelim = dataSource.ColumnDelimiter ?? "";
            var columnDelimEnd = string.IsNullOrEmpty(columnDelim) ? "" : columnDelim;

            parameters = new Dictionary<string, object>();

            // Build SELECT clause
            string selectClause;
            if (columns == null || !columns.Any())
            {
                selectClause = "SELECT *";
            }
            else
            {
                var delimitedColumns = columns.Select(col => 
                    string.IsNullOrEmpty(columnDelim) ? col : $"{columnDelim}{col}{columnDelimEnd}");
                selectClause = $"SELECT {string.Join(", ", delimitedColumns)}";
            }

            // Apply column delimiters to table name
            var delimitedTableName = string.IsNullOrEmpty(columnDelim)
                ? tableName
                : $"{columnDelim}{tableName}{columnDelimEnd}";

            var sql = new StringBuilder($"{selectClause} FROM {delimitedTableName}");

            // Build WHERE clause with parameters
            if (filters != null && filters.Count > 0)
            {
                var whereClauses = new List<string>();
                var paramIndex = 0;

                foreach (var filter in filters)
                {
                    if (string.IsNullOrWhiteSpace(filter.Key))
                        continue;

                    var paramName = $"param{paramIndex++}";
                    var delimitedFieldName = string.IsNullOrEmpty(columnDelim)
                        ? filter.Key
                        : $"{columnDelim}{filter.Key}{columnDelimEnd}";

                    whereClauses.Add($"{delimitedFieldName} = {paramDelim}{paramName}");
                    parameters[paramName] = filter.Value ?? DBNull.Value;
                }

                if (whereClauses.Count > 0)
                {
                    sql.Append(" WHERE ").Append(string.Join(" AND ", whereClauses));
                }
            }

            return sql.ToString();
        }

        /// <summary>
        /// Builds a parameterized INSERT query
        /// </summary>
        /// <param name="dataSource">IDataSource instance to get delimiters from</param>
        /// <param name="tableName">Table name to insert into</param>
        /// <param name="columnValues">Dictionary of column names and values</param>
        /// <param name="parameters">Output dictionary of parameter names and values</param>
        /// <returns>Parameterized SQL INSERT statement</returns>
        public static string BuildInsertQuery(
            IDataSource dataSource,
            string tableName,
            Dictionary<string, object> columnValues,
            out Dictionary<string, object> parameters)
        {
            if (dataSource == null)
                throw new ArgumentNullException(nameof(dataSource));
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));
            if (columnValues == null || columnValues.Count == 0)
                throw new ArgumentException("Column values cannot be null or empty", nameof(columnValues));

            var paramDelim = dataSource.ParameterDelimiter ?? "@";
            var columnDelim = dataSource.ColumnDelimiter ?? "";
            var columnDelimEnd = string.IsNullOrEmpty(columnDelim) ? "" : columnDelim;

            parameters = new Dictionary<string, object>();

            var delimitedTableName = string.IsNullOrEmpty(columnDelim)
                ? tableName
                : $"{columnDelim}{tableName}{columnDelimEnd}";

            var columns = new List<string>();
            var paramNames = new List<string>();
            var paramIndex = 0;

            foreach (var kvp in columnValues)
            {
                var delimitedColumn = string.IsNullOrEmpty(columnDelim)
                    ? kvp.Key
                    : $"{columnDelim}{kvp.Key}{columnDelimEnd}";

                var paramName = $"param{paramIndex++}";
                columns.Add(delimitedColumn);
                paramNames.Add($"{paramDelim}{paramName}");
                parameters[paramName] = kvp.Value ?? DBNull.Value;
            }

            var sql = $"INSERT INTO {delimitedTableName} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", paramNames)})";
            return sql;
        }

        /// <summary>
        /// Builds a parameterized UPDATE query
        /// </summary>
        /// <param name="dataSource">IDataSource instance to get delimiters from</param>
        /// <param name="tableName">Table name to update</param>
        /// <param name="columnValues">Dictionary of column names and values to update</param>
        /// <param name="whereFilters">Dictionary of field names and values for WHERE clause</param>
        /// <param name="parameters">Output dictionary of parameter names and values</param>
        /// <returns>Parameterized SQL UPDATE statement</returns>
        public static string BuildUpdateQuery(
            IDataSource dataSource,
            string tableName,
            Dictionary<string, object> columnValues,
            out Dictionary<string, object> parameters,
            Dictionary<string, object>? whereFilters = null)
        {
            if (dataSource == null)
                throw new ArgumentNullException(nameof(dataSource));
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));
            if (columnValues == null || columnValues.Count == 0)
                throw new ArgumentException("Column values cannot be null or empty", nameof(columnValues));

            var paramDelim = dataSource.ParameterDelimiter ?? "@";
            var columnDelim = dataSource.ColumnDelimiter ?? "";
            var columnDelimEnd = string.IsNullOrEmpty(columnDelim) ? "" : columnDelim;

            parameters = new Dictionary<string, object>();

            var delimitedTableName = string.IsNullOrEmpty(columnDelim)
                ? tableName
                : $"{columnDelim}{tableName}{columnDelimEnd}";

            var setClauses = new List<string>();
            var paramIndex = 0;

            // Build SET clause
            foreach (var kvp in columnValues)
            {
                var delimitedColumn = string.IsNullOrEmpty(columnDelim)
                    ? kvp.Key
                    : $"{columnDelim}{kvp.Key}{columnDelimEnd}";

                var paramName = $"setParam{paramIndex++}";
                setClauses.Add($"{delimitedColumn} = {paramDelim}{paramName}");
                parameters[paramName] = kvp.Value ?? DBNull.Value;
            }

            var sql = new StringBuilder($"UPDATE {delimitedTableName} SET {string.Join(", ", setClauses)}");

            // Build WHERE clause
            if (whereFilters != null && whereFilters.Count > 0)
            {
                var whereClauses = new List<string>();

                foreach (var filter in whereFilters)
                {
                    if (string.IsNullOrWhiteSpace(filter.Key))
                        continue;

                    var delimitedFieldName = string.IsNullOrEmpty(columnDelim)
                        ? filter.Key
                        : $"{columnDelim}{filter.Key}{columnDelimEnd}";

                    var paramName = $"whereParam{paramIndex++}";
                    whereClauses.Add($"{delimitedFieldName} = {paramDelim}{paramName}");
                    parameters[paramName] = filter.Value ?? DBNull.Value;
                }

                if (whereClauses.Count > 0)
                {
                    sql.Append(" WHERE ").Append(string.Join(" AND ", whereClauses));
                }
            }

            return sql.ToString();
        }

        /// <summary>
        /// Builds a parameterized DELETE query
        /// </summary>
        /// <param name="dataSource">IDataSource instance to get delimiters from</param>
        /// <param name="tableName">Table name to delete from</param>
        /// <param name="whereFilters">Dictionary of field names and values for WHERE clause</param>
        /// <param name="parameters">Output dictionary of parameter names and values</param>
        /// <returns>Parameterized SQL DELETE statement</returns>
        public static string BuildDeleteQuery(
            IDataSource dataSource,
            string tableName,
            out Dictionary<string, object> parameters,
            Dictionary<string, object>? whereFilters = null)
        {
            if (dataSource == null)
                throw new ArgumentNullException(nameof(dataSource));
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));

            var paramDelim = dataSource.ParameterDelimiter ?? "@";
            var columnDelim = dataSource.ColumnDelimiter ?? "";
            var columnDelimEnd = string.IsNullOrEmpty(columnDelim) ? "" : columnDelim;

            parameters = new Dictionary<string, object>();

            var delimitedTableName = string.IsNullOrEmpty(columnDelim)
                ? tableName
                : $"{columnDelim}{tableName}{columnDelimEnd}";

            var sql = new StringBuilder($"DELETE FROM {delimitedTableName}");

            // Build WHERE clause
            if (whereFilters != null && whereFilters.Count > 0)
            {
                var whereClauses = new List<string>();
                var paramIndex = 0;

                foreach (var filter in whereFilters)
                {
                    if (string.IsNullOrWhiteSpace(filter.Key))
                        continue;

                    var delimitedFieldName = string.IsNullOrEmpty(columnDelim)
                        ? filter.Key
                        : $"{columnDelim}{filter.Key}{columnDelimEnd}";

                    var paramName = $"param{paramIndex++}";
                    whereClauses.Add($"{delimitedFieldName} = {paramDelim}{paramName}");
                    parameters[paramName] = filter.Value ?? DBNull.Value;
                }

                if (whereClauses.Count > 0)
                {
                    sql.Append(" WHERE ").Append(string.Join(" AND ", whereClauses));
                }
            }

            return sql.ToString();
        }

        /// <summary>
        /// Replaces hardcoded @ parameters in a SQL query with the appropriate parameter delimiter
        /// </summary>
        /// <param name="dataSource">IDataSource instance to get delimiters from</param>
        /// <param name="sql">SQL query with @ parameters</param>
        /// <returns>SQL query with correct parameter delimiters</returns>
        public static string ReplaceParameterDelimiters(IDataSource dataSource, string sql)
        {
            if (dataSource == null)
                throw new ArgumentNullException(nameof(dataSource));
            if (string.IsNullOrWhiteSpace(sql))
                return sql;

            var paramDelim = dataSource.ParameterDelimiter ?? "@";
            
            // If already using the correct delimiter, return as-is
            if (paramDelim == "@")
                return sql;

            // Replace @paramName with {paramDelim}paramName
            // Use regex to match @ followed by word characters
            var pattern = @"@(\w+)";
            var replacement = $"{paramDelim}$1";
            return System.Text.RegularExpressions.Regex.Replace(sql, pattern, replacement);
        }

        /// <summary>
        /// Applies column delimiters to column and table names in a SQL query
        /// </summary>
        /// <param name="dataSource">IDataSource instance to get delimiters from</param>
        /// <param name="sql">SQL query</param>
        /// <returns>SQL query with column delimiters applied</returns>
        public static string ApplyColumnDelimiters(IDataSource dataSource, string sql)
        {
            if (dataSource == null)
                throw new ArgumentNullException(nameof(dataSource));
            if (string.IsNullOrWhiteSpace(sql))
                return sql;

            var columnDelim = dataSource.ColumnDelimiter ?? "";
            if (string.IsNullOrEmpty(columnDelim))
                return sql; // No column delimiters needed

            var columnDelimEnd = columnDelim;

            // This is a simplified implementation
            // For production, you might want a more sophisticated SQL parser
            // For now, we'll handle common cases: table names after FROM, JOIN, column names in SELECT, WHERE, etc.
            
            // Note: Full SQL parsing is complex. This method provides a basic implementation.
            // For complex queries, consider using a SQL parser library or manual construction.
            
            return sql; // Placeholder - full implementation would require SQL parsing
        }
    }
}

