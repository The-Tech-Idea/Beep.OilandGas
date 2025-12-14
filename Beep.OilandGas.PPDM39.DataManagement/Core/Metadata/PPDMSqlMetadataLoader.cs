using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Beep.OilandGas.PPDM39.Core.Metadata;

namespace Beep.OilandGas.PPDM39.DataManagement.Core.Metadata
{
    /// <summary>
    /// Loads PPDM table metadata from SQL scripts
    /// Parses CREATE TABLE, ALTER TABLE, and other DDL statements to extract:
    /// - Table names
    /// - Primary keys
    /// - Foreign keys
    /// - Relationships
    /// </summary>
    public class PPDMSqlMetadataLoader
    {
        /// <summary>
        /// Loads metadata from PPDM SQL script files (TAB.sql, PK.sql, FK.sql)
        /// </summary>
        public Dictionary<string, PPDMTableMetadata> LoadFromPpdmScripts(string tabScriptPath, string pkScriptPath, string fkScriptPath)
        {
            var metadata = new Dictionary<string, PPDMTableMetadata>(StringComparer.OrdinalIgnoreCase);

            // Step 1: Load tables from TAB.sql
            if (System.IO.File.Exists(tabScriptPath))
            {
                var tabScript = System.IO.File.ReadAllText(tabScriptPath);
                var statements = SplitSqlStatements(tabScript);
                
                foreach (var statement in statements)
                {
                    if (IsCreateTableStatement(statement))
                    {
                        var tableMeta = ParseCreateTable(statement);
                        if (tableMeta != null)
                        {
                            metadata[tableMeta.TableName.ToUpper()] = tableMeta;
                        }
                    }
                }
            }

            // Step 2: Load primary keys from PK.sql
            if (System.IO.File.Exists(pkScriptPath))
            {
                var pkScript = System.IO.File.ReadAllText(pkScriptPath);
                var statements = SplitSqlStatements(pkScript);
                
                foreach (var statement in statements)
                {
                    ParsePrimaryKeyConstraint(statement, metadata);
                }
            }

            // Step 3: Load foreign keys from FK.sql
            if (System.IO.File.Exists(fkScriptPath))
            {
                var fkScript = System.IO.File.ReadAllText(fkScriptPath);
                var statements = SplitSqlStatements(fkScript);
                
                foreach (var statement in statements)
                {
                    if (IsAlterTableStatement(statement))
                    {
                        ParseAlterTable(statement, metadata);
                    }
                }
            }

            return metadata;
        }

        /// <summary>
        /// Loads metadata from SQL script content (single file)
        /// </summary>
        public Dictionary<string, PPDMTableMetadata> LoadFromSqlScript(string sqlScript)
        {
            var metadata = new Dictionary<string, PPDMTableMetadata>(StringComparer.OrdinalIgnoreCase);

            // Split script into statements
            var statements = SplitSqlStatements(sqlScript);

            // First pass: Parse CREATE TABLE statements
            foreach (var statement in statements)
            {
                if (IsCreateTableStatement(statement))
                {
                    var tableMeta = ParseCreateTable(statement);
                    if (tableMeta != null)
                    {
                        metadata[tableMeta.TableName.ToUpper()] = tableMeta;
                    }
                }
            }

            // Second pass: Parse ALTER TABLE statements for foreign keys
            foreach (var statement in statements)
            {
                if (IsAlterTableStatement(statement))
                {
                    ParseAlterTable(statement, metadata);
                }
            }

            // Third pass: Parse primary key constraints
            foreach (var statement in statements)
            {
                if (IsPrimaryKeyConstraint(statement))
                {
                    ParsePrimaryKeyConstraint(statement, metadata);
                }
            }

            return metadata;
        }

        /// <summary>
        /// Loads metadata from SQL script file
        /// </summary>
        public Dictionary<string, PPDMTableMetadata> LoadFromSqlFile(string filePath)
        {
            var sqlScript = System.IO.File.ReadAllText(filePath);
            return LoadFromSqlScript(sqlScript);
        }

        /// <summary>
        /// Splits SQL script into individual statements
        /// </summary>
        private List<string> SplitSqlStatements(string sqlScript)
        {
            // Remove comments
            sqlScript = RemoveComments(sqlScript);

            // Split by semicolon, but preserve strings
            var statements = new List<string>();
            var currentStatement = new System.Text.StringBuilder();
            bool inString = false;
            char stringChar = '\0';

            foreach (char c in sqlScript)
            {
                if ((c == '\'' || c == '"') && !inString)
                {
                    inString = true;
                    stringChar = c;
                }
                else if (c == stringChar && inString)
                {
                    inString = false;
                    stringChar = '\0';
                }

                currentStatement.Append(c);

                if (c == ';' && !inString)
                {
                    var statement = currentStatement.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(statement))
                    {
                        statements.Add(statement);
                    }
                    currentStatement.Clear();
                }
            }

            // Add last statement if exists
            var lastStatement = currentStatement.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(lastStatement))
            {
                statements.Add(lastStatement);
            }

            return statements;
        }

        /// <summary>
        /// Removes SQL comments from script
        /// </summary>
        private string RemoveComments(string sql)
        {
            // Remove single-line comments (--)
            sql = Regex.Replace(sql, @"--.*?$", "", RegexOptions.Multiline);

            // Remove multi-line comments (/* */)
            sql = Regex.Replace(sql, @"/\*.*?\*/", "", RegexOptions.Singleline);

            return sql;
        }

        /// <summary>
        /// Checks if statement is CREATE TABLE
        /// </summary>
        private bool IsCreateTableStatement(string statement)
        {
            return Regex.IsMatch(statement, @"^\s*CREATE\s+TABLE", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        }

        /// <summary>
        /// Checks if statement is ALTER TABLE
        /// </summary>
        private bool IsAlterTableStatement(string statement)
        {
            return Regex.IsMatch(statement, @"^\s*ALTER\s+TABLE", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        }

        /// <summary>
        /// Checks if statement is primary key constraint
        /// </summary>
        private bool IsPrimaryKeyConstraint(string statement)
        {
            return Regex.IsMatch(statement, @"PRIMARY\s+KEY|CONSTRAINT.*PRIMARY", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Parses CREATE TABLE statement
        /// </summary>
        private PPDMTableMetadata ParseCreateTable(string statement)
        {
            // Extract table name
            var tableMatch = Regex.Match(statement, @"CREATE\s+TABLE\s+(?:\[?(\w+)\]?\.)?\[?(\w+)\]?", RegexOptions.IgnoreCase);
            if (!tableMatch.Success)
                return null;

            var tableName = tableMatch.Groups[2].Value;
            var metadata = new PPDMTableMetadata
            {
                TableName = tableName,
                EntityTypeName = tableName,
                PrimaryKeyColumn = null, // Will be set from constraints
                Module = InferModuleFromTableName(tableName),
                CommonColumns = new List<string>()
            };

            // Check for common columns
            if (Regex.IsMatch(statement, @"ACTIVE_IND", RegexOptions.IgnoreCase))
                metadata.CommonColumns.Add("ACTIVE_IND");
            if (Regex.IsMatch(statement, @"ROW_CREATED_BY", RegexOptions.IgnoreCase))
                metadata.CommonColumns.Add("ROW_CREATED_BY");
            if (Regex.IsMatch(statement, @"ROW_CREATED_DATE", RegexOptions.IgnoreCase))
                metadata.CommonColumns.Add("ROW_CREATED_DATE");
            if (Regex.IsMatch(statement, @"ROW_CHANGED_BY", RegexOptions.IgnoreCase))
                metadata.CommonColumns.Add("ROW_CHANGED_BY");
            if (Regex.IsMatch(statement, @"ROW_CHANGED_DATE", RegexOptions.IgnoreCase))
                metadata.CommonColumns.Add("ROW_CHANGED_DATE");
            if (Regex.IsMatch(statement, @"PPDM_GUID", RegexOptions.IgnoreCase))
                metadata.CommonColumns.Add("PPDM_GUID");

            return metadata;
        }

        /// <summary>
        /// Parses ALTER TABLE statement for foreign keys
        /// </summary>
        private void ParseAlterTable(string statement, Dictionary<string, PPDMTableMetadata> metadata)
        {
            // Pattern: ALTER TABLE [schema.]table ADD CONSTRAINT ... FOREIGN KEY (column) REFERENCES referenced_table (referenced_column)
            var fkPattern = @"ALTER\s+TABLE\s+(?:\[?\w+\]?\.)?\[?(\w+)\]?\s+ADD\s+(?:CONSTRAINT\s+\w+\s+)?FOREIGN\s+KEY\s*\(\[?(\w+)\]?\)\s+REFERENCES\s+(?:\[?\w+\]?\.)?\[?(\w+)\]?\s*\(\[?(\w+)\]?\)";
            var match = Regex.Match(statement, fkPattern, RegexOptions.IgnoreCase);

            if (match.Success)
            {
                var tableName = match.Groups[1].Value.ToUpper();
                var fkColumn = match.Groups[2].Value;
                var referencedTable = match.Groups[3].Value.ToUpper();
                var referencedColumn = match.Groups[4].Value;

                if (metadata.ContainsKey(tableName))
                {
                    var fk = new PPDMForeignKey
                    {
                        ForeignKeyColumn = fkColumn,
                        ReferencedTable = referencedTable,
                        ReferencedPrimaryKey = referencedColumn,
                        RelationshipType = "OneToMany"
                    };

                    metadata[tableName].ForeignKeys.Add(fk);
                }
            }
        }

        /// <summary>
        /// Parses primary key constraint
        /// </summary>
        private void ParsePrimaryKeyConstraint(string statement, Dictionary<string, PPDMTableMetadata> metadata)
        {
            // Pattern 1: PRIMARY KEY (column) in CREATE TABLE
            var pkPattern1 = @"PRIMARY\s+KEY\s*\(\[?(\w+)\]?\)";
            var match1 = Regex.Match(statement, pkPattern1, RegexOptions.IgnoreCase);
            if (match1.Success)
            {
                var pkColumn = match1.Groups[1].Value;
                // Find which table this belongs to
                var tableMatch = Regex.Match(statement, @"CREATE\s+TABLE\s+(?:\[?\w+\]?\.)?\[?(\w+)\]?", RegexOptions.IgnoreCase);
                if (tableMatch.Success)
                {
                    var tableName = tableMatch.Groups[1].Value.ToUpper();
                    if (metadata.ContainsKey(tableName))
                    {
                        metadata[tableName].PrimaryKeyColumn = pkColumn;
                    }
                }
            }

            // Pattern 2: ALTER TABLE ... ADD PRIMARY KEY (column)
            var pkPattern2 = @"ALTER\s+TABLE\s+(?:\[?\w+\]?\.)?\[?(\w+)\]?\s+ADD\s+(?:CONSTRAINT\s+\w+\s+)?PRIMARY\s+KEY\s*\(\[?(\w+)\]?\)";
            var match2 = Regex.Match(statement, pkPattern2, RegexOptions.IgnoreCase);
            if (match2.Success)
            {
                var tableName = match2.Groups[1].Value.ToUpper();
                var pkColumn = match2.Groups[2].Value;
                if (metadata.ContainsKey(tableName))
                {
                    metadata[tableName].PrimaryKeyColumn = pkColumn;
                }
            }
        }

        /// <summary>
        /// Infers module name from table name
        /// </summary>
        private string InferModuleFromTableName(string tableName)
        {
            // Common PPDM39 patterns
            if (tableName.StartsWith("STRAT_", StringComparison.OrdinalIgnoreCase))
                return "Stratigraphy";
            
            if (tableName.StartsWith("WELL_", StringComparison.OrdinalIgnoreCase) || 
                tableName.Equals("WELL", StringComparison.OrdinalIgnoreCase))
                return "Well";
            
            if (tableName.StartsWith("PDEN", StringComparison.OrdinalIgnoreCase) ||
                tableName.StartsWith("PROD", StringComparison.OrdinalIgnoreCase))
                return "Production";
            
            if (tableName.StartsWith("DRILL", StringComparison.OrdinalIgnoreCase))
                return "Drilling";
            
            if (tableName.StartsWith("FACILITY", StringComparison.OrdinalIgnoreCase))
                return "Facility";
            
            if (tableName.StartsWith("AREA", StringComparison.OrdinalIgnoreCase))
                return "Area";
            
            // Default
            return "General";
        }
    }
}

