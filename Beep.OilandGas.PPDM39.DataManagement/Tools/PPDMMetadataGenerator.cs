using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Beep.OilandGas.PPDM39.Core.Metadata;

namespace Beep.OilandGas.PPDM39.DataManagement.Tools
{
    /// <summary>
    /// Tool to parse PPDM SQL scripts and generate metadata as C# class or JSON
    /// Run this ONCE to extract metadata, then use the generated file
    /// </summary>
    public class PPDMMetadataGenerator
    {
        private readonly string _tabScriptPath;
        private readonly string _pkScriptPath;
        private readonly string _fkScriptPath;

        public PPDMMetadataGenerator(string tabScriptPath, string pkScriptPath, string fkScriptPath)
        {
            _tabScriptPath = tabScriptPath;
            _pkScriptPath = pkScriptPath;
            _fkScriptPath = fkScriptPath;
        }

        /// <summary>
        /// Generates metadata from SQL scripts and saves as JSON
        /// </summary>
        public void GenerateJsonMetadata(string outputPath)
        {
            var metadata = ParseAllScripts();
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(metadata, options);
            File.WriteAllText(outputPath, json);
            Console.WriteLine($"Metadata saved to: {outputPath}");
            Console.WriteLine($"Total tables: {metadata.Count}");
        }

        /// <summary>
        /// Generates metadata from SQL scripts and saves as C# class
        /// </summary>
        public void GenerateCSharpMetadata(string outputPath)
        {
            var metadata = ParseAllScripts();
            var csharpCode = GenerateCSharpClass(metadata);
            File.WriteAllText(outputPath, csharpCode);
            Console.WriteLine($"C# metadata class saved to: {outputPath}");
            Console.WriteLine($"Total tables: {metadata.Count}");
        }

        /// <summary>
        /// Parses all SQL scripts and returns metadata dictionary
        /// </summary>
        private Dictionary<string, PPDMTableMetadata> ParseAllScripts()
        {
            var metadata = new Dictionary<string, PPDMTableMetadata>(StringComparer.OrdinalIgnoreCase);

            // Step 1: Parse TAB.sql for table definitions
            if (File.Exists(_tabScriptPath))
            {
                Console.WriteLine($"Parsing TAB.sql: {_tabScriptPath}");
                var tabScript = File.ReadAllText(_tabScriptPath);
                ParseTables(tabScript, metadata);
                Console.WriteLine($"Found {metadata.Count} tables");
            }

            // Step 2: Parse PK.sql for primary keys
            if (File.Exists(_pkScriptPath))
            {
                Console.WriteLine($"Parsing PK.sql: {_pkScriptPath}");
                var pkScript = File.ReadAllText(_pkScriptPath);
                ParsePrimaryKeys(pkScript, metadata);
                Console.WriteLine($"Parsed primary keys");
            }

            // Step 3: Parse FK.sql for foreign keys
            if (File.Exists(_fkScriptPath))
            {
                Console.WriteLine($"Parsing FK.sql: {_fkScriptPath}");
                var fkScript = File.ReadAllText(_fkScriptPath);
                ParseForeignKeys(fkScript, metadata);
                Console.WriteLine($"Parsed foreign keys");
            }

            return metadata;
        }

        /// <summary>
        /// Parses CREATE TABLE statements
        /// </summary>
        private void ParseTables(string sqlScript, Dictionary<string, PPDMTableMetadata> metadata)
        {
            var statements = SplitSqlStatements(sqlScript);
            
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

        /// <summary>
        /// Parses PRIMARY KEY constraints
        /// </summary>
        private void ParsePrimaryKeys(string sqlScript, Dictionary<string, PPDMTableMetadata> metadata)
        {
            var statements = SplitSqlStatements(sqlScript);
            
            foreach (var statement in statements)
            {
                // Pattern: ALTER TABLE table_name ADD CONSTRAINT constraint_name PRIMARY KEY (column1, column2, ...)
                var pkPattern = @"ALTER\s+TABLE\s+(\w+)\s+ADD\s+CONSTRAINT\s+\w+\s+PRIMARY\s+KEY\s*\((.*?)\)";
                var match = Regex.Match(statement, pkPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                
                if (match.Success)
                {
                    var tableName = match.Groups[1].Value.ToUpper();
                    var pkColumns = match.Groups[2].Value;
                    
                    // Extract column names (handle multiple columns)
                    var columns = Regex.Matches(pkColumns, @"(\w+)")
                        .Cast<Match>()
                        .Select(m => m.Value.Trim())
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .ToList();
                    
                    if (metadata.ContainsKey(tableName) && columns.Any())
                    {
                        // Use first column as primary key (or composite key indicator)
                        metadata[tableName].PrimaryKeyColumn = columns[0];
                        
                        // If composite key, store all columns
                        if (columns.Count > 1)
                        {
                            metadata[tableName].PrimaryKeyColumn = string.Join(",", columns);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Parses FOREIGN KEY constraints
        /// </summary>
        private void ParseForeignKeys(string sqlScript, Dictionary<string, PPDMTableMetadata> metadata)
        {
            var statements = SplitSqlStatements(sqlScript);
            
            foreach (var statement in statements)
            {
                // Pattern: ALTER TABLE table_name ADD CONSTRAINT constraint_name FOREIGN KEY (fk_cols) REFERENCES ref_table (ref_cols)
                var fkPattern = @"ALTER\s+TABLE\s+(\w+)\s+ADD\s+CONSTRAINT\s+\w+\s+FOREIGN\s+KEY\s*\((.*?)\)\s+REFERENCES\s+(\w+)\s*\((.*?)\)";
                var match = Regex.Match(statement, fkPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                
                if (match.Success)
                {
                    var tableName = match.Groups[1].Value.ToUpper();
                    var fkColumns = match.Groups[2].Value;
                    var refTable = match.Groups[3].Value.ToUpper();
                    var refColumns = match.Groups[4].Value;
                    
                    // Extract column names
                    var fkColList = Regex.Matches(fkColumns, @"(\w+)")
                        .Cast<Match>()
                        .Select(m => m.Value.Trim())
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .ToList();
                    
                    var refColList = Regex.Matches(refColumns, @"(\w+)")
                        .Cast<Match>()
                        .Select(m => m.Value.Trim())
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .ToList();
                    
                    if (metadata.ContainsKey(tableName) && fkColList.Any() && refColList.Any())
                    {
                        // For now, use first column (can be extended for composite FKs)
                        var fk = new PPDMForeignKey
                        {
                            ForeignKeyColumn = fkColList[0],
                            ReferencedTable = refTable,
                            ReferencedPrimaryKey = refColList[0],
                            RelationshipType = "OneToMany"
                        };
                        
                        metadata[tableName].ForeignKeys.Add(fk);
                    }
                }
            }
        }

        /// <summary>
        /// Parses CREATE TABLE statement
        /// </summary>
        private PPDMTableMetadata ParseCreateTable(string statement)
        {
            // Extract table name (handle raiserror prefix)
            var tableMatch = Regex.Match(statement, @"CREATE\s+TABLE\s+(\w+)", RegexOptions.IgnoreCase);
            if (!tableMatch.Success)
                return null;

            var tableName = tableMatch.Groups[1].Value;
            var metadata = new PPDMTableMetadata
            {
                TableName = tableName,
                EntityTypeName = tableName.ToUpper(),
                PrimaryKeyColumn = null, // Will be set from PK.sql
                Module = InferModuleFromTableName(tableName),
                CommonColumns = new List<string>()
            };

            // Check for common columns
            if (Regex.IsMatch(statement, @"\bACTIVE_IND\b", RegexOptions.IgnoreCase))
                metadata.CommonColumns.Add("ACTIVE_IND");
            if (Regex.IsMatch(statement, @"\bROW_CREATED_BY\b", RegexOptions.IgnoreCase))
                metadata.CommonColumns.Add("ROW_CREATED_BY");
            if (Regex.IsMatch(statement, @"\bROW_CREATED_DATE\b", RegexOptions.IgnoreCase))
                metadata.CommonColumns.Add("ROW_CREATED_DATE");
            if (Regex.IsMatch(statement, @"\bROW_CHANGED_BY\b", RegexOptions.IgnoreCase))
                metadata.CommonColumns.Add("ROW_CHANGED_BY");
            if (Regex.IsMatch(statement, @"\bROW_CHANGED_DATE\b", RegexOptions.IgnoreCase))
                metadata.CommonColumns.Add("ROW_CHANGED_DATE");
            if (Regex.IsMatch(statement, @"\bPPDM_GUID\b", RegexOptions.IgnoreCase))
                metadata.CommonColumns.Add("PPDM_GUID");
            if (Regex.IsMatch(statement, @"\bROW_EFFECTIVE_DATE\b", RegexOptions.IgnoreCase))
                metadata.CommonColumns.Add("ROW_EFFECTIVE_DATE");
            if (Regex.IsMatch(statement, @"\bROW_EXPIRY_DATE\b", RegexOptions.IgnoreCase))
                metadata.CommonColumns.Add("ROW_EXPIRY_DATE");
            if (Regex.IsMatch(statement, @"\bROW_QUALITY\b", RegexOptions.IgnoreCase))
                metadata.CommonColumns.Add("ROW_QUALITY");

            return metadata;
        }

        /// <summary>
        /// Infers module from table name
        /// </summary>
        private string InferModuleFromTableName(string tableName)
        {
            var upper = tableName.ToUpper();
            
            if (upper.StartsWith("STRAT_"))
                return "Stratigraphy";
            if (upper.StartsWith("WELL_") || upper == "WELL")
                return "Well";
            if (upper.StartsWith("PDEN") || upper.StartsWith("PROD"))
                return "Production";
            if (upper.StartsWith("DRILL"))
                return "Drilling";
            if (upper.StartsWith("FACILITY") || upper.StartsWith("FAC_"))
                return "Facility";
            if (upper.StartsWith("AREA") || upper.StartsWith("FIELD"))
                return "Area";
            if (upper.StartsWith("ANL_") || upper.StartsWith("ANALYSIS"))
                return "Analysis";
            if (upper.StartsWith("R_"))
                return "Reference";
            if (upper.StartsWith("PPDM_"))
                return "PPDM";
            
            return "General";
        }

        /// <summary>
        /// Splits SQL script into statements
        /// </summary>
        private List<string> SplitSqlStatements(string sqlScript)
        {
            // Remove comments
            sqlScript = RemoveComments(sqlScript);

            var statements = new List<string>();
            var currentStatement = new StringBuilder();
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

            var lastStatement = currentStatement.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(lastStatement))
            {
                statements.Add(lastStatement);
            }

            return statements;
        }

        /// <summary>
        /// Removes SQL comments
        /// </summary>
        private string RemoveComments(string sql)
        {
            sql = Regex.Replace(sql, @"--.*?$", "", RegexOptions.Multiline);
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
        /// Generates C# class code from metadata
        /// </summary>
        private string GenerateCSharpClass(Dictionary<string, PPDMTableMetadata> metadata)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using Beep.OilandGas.PPDM39.Core.Metadata;");
            sb.AppendLine();
            sb.AppendLine("namespace Beep.OilandGas.PPDM39.DataManagement.Core.Metadata");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Auto-generated PPDM39 table metadata");
            sb.AppendLine("    /// Generated from SQL scripts - DO NOT EDIT MANUALLY");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static class PPDM39Metadata");
            sb.AppendLine("    {");
            sb.AppendLine("        public static Dictionary<string, PPDMTableMetadata> GetMetadata()");
            sb.AppendLine("        {");
            sb.AppendLine("            var metadata = new Dictionary<string, PPDMTableMetadata>(System.StringComparer.OrdinalIgnoreCase);");
            sb.AppendLine();

            foreach (var kvp in metadata.OrderBy(x => x.Key))
            {
                var table = kvp.Value;
                sb.AppendLine($"            // {table.TableName}");
                sb.AppendLine($"            metadata[\"{table.TableName.ToUpper()}\"] = new PPDMTableMetadata");
                sb.AppendLine("            {");
                sb.AppendLine($"                TableName = \"{table.TableName}\",");
                sb.AppendLine($"                EntityTypeName = \"{table.EntityTypeName}\",");
                sb.AppendLine($"                PrimaryKeyColumn = \"{table.PrimaryKeyColumn ?? "UNKNOWN"}\",");
                sb.AppendLine($"                Module = \"{table.Module}\",");
                
                if (table.CommonColumns.Any())
                {
                    sb.AppendLine("                CommonColumns = new List<string> { " + 
                        string.Join(", ", table.CommonColumns.Select(c => $"\"{c}\"")) + " },");
                }
                else
                {
                    sb.AppendLine("                CommonColumns = new List<string>(),");
                }
                
                if (table.ForeignKeys.Any())
                {
                    sb.AppendLine("                ForeignKeys = new List<PPDMForeignKey>");
                    sb.AppendLine("                {");
                    foreach (var fk in table.ForeignKeys)
                    {
                        sb.AppendLine("                    new PPDMForeignKey");
                        sb.AppendLine("                    {");
                        sb.AppendLine($"                        ForeignKeyColumn = \"{fk.ForeignKeyColumn}\",");
                        sb.AppendLine($"                        ReferencedTable = \"{fk.ReferencedTable}\",");
                        sb.AppendLine($"                        ReferencedPrimaryKey = \"{fk.ReferencedPrimaryKey}\",");
                        sb.AppendLine($"                        RelationshipType = \"{fk.RelationshipType}\"");
                        sb.AppendLine("                    },");
                    }
                    sb.AppendLine("                }");
                }
                else
                {
                    sb.AppendLine("                ForeignKeys = new List<PPDMForeignKey>()");
                }
                
                sb.AppendLine("            };");
                sb.AppendLine();
            }

            sb.AppendLine("            return metadata;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}

