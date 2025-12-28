using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Beep.OilandGas.PPDM39.DataManagement.Tools
{
    public class ConsolidateSeedData
    {
        public static void Run(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("PPDM Seed Data Consolidation");
            Console.WriteLine("========================================");
            Console.WriteLine();

            // Get solution root (go up from Tools project to solution root)
            var solutionRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."));
            
            var csvDataPath = Path.Combine(solutionRoot, "Beep.OilandGas.PPDM39.DataManagement", "Core", "SeedData", "PPDMCSVData.json");
            var referenceDataPath = Path.Combine(solutionRoot, "Beep.OilandGas.PPDM39.DataManagement", "SeedData", "Templates", "PPDMReferenceData.json");
            
            // Allow override via command line arguments
            if (args.Length >= 1 && !string.IsNullOrWhiteSpace(args[0]))
            {
                csvDataPath = args[0];
            }
            if (args.Length >= 2 && !string.IsNullOrWhiteSpace(args[1]))
            {
                referenceDataPath = args[1];
            }

            Console.WriteLine("Configuration:");
            Console.WriteLine($"  CSV Data Path: {csvDataPath}");
            Console.WriteLine($"  Reference Data Path: {referenceDataPath}");
            Console.WriteLine();

            if (!File.Exists(csvDataPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: CSV data file not found: {csvDataPath}");
                Console.ResetColor();
                return;
            }
            
            if (!File.Exists(referenceDataPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: Reference data file not found: {referenceDataPath}");
                Console.ResetColor();
                return;
            }

            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Loading CSV data...");
                Console.ResetColor();
                var csvData = JsonSerializer.Deserialize<JsonObject>(File.ReadAllText(csvDataPath));
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Loading reference data...");
                Console.ResetColor();
                var referenceData = JsonSerializer.Deserialize<JsonObject>(File.ReadAllText(referenceDataPath));
                
                // Process CSV data
                Console.WriteLine("Processing CSV data...");
                var csvTables = ProcessCsvData(csvData);
                Console.WriteLine($"Found {csvTables.Count} unique tables in CSV data");
                
                // Get existing tables
                var existingTables = GetExistingTableNames(referenceData);
                Console.WriteLine($"Found {existingTables.Count} existing tables in reference data");
                Console.WriteLine();
                
                // Consolidate
                var newTables = new List<string>();
                var updatedTables = new List<string>();
                
                Console.WriteLine("Consolidating data...");
                foreach (var kvp in csvTables)
                {
                    var tableName = kvp.Key;
                    var csvItems = kvp.Value;
                    
                    if (existingTables.Contains(tableName))
                    {
                        // Add new items to existing table
                        var added = AddToExistingTable(referenceData, tableName, csvItems);
                        if (added > 0)
                        {
                            updatedTables.Add(tableName);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"  Added {added} new items to {tableName}");
                            Console.ResetColor();
                        }
                    }
                    else
                    {
                        // Add new table
                        AddNewTable(referenceData, tableName, csvItems);
                        newTables.Add(tableName);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"  Added new table {tableName} with {csvItems.Count} items");
                        Console.ResetColor();
                    }
                }
                
                // Update description
                if (referenceData["description"] != null)
                {
                    referenceData["description"] = "PPDM standard reference tables (RA_* tables) with comprehensive standard values. This file contains consolidated seed data from PPDMCSVData.json and additional priority tables.";
                }
                
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("========================================");
                Console.WriteLine("Consolidation Summary");
                Console.WriteLine("========================================");
                Console.ResetColor();
                Console.WriteLine($"  New tables added: {newTables.Count}");
                Console.WriteLine($"  Existing tables updated: {updatedTables.Count}");
                
                // Save
                Console.WriteLine();
                Console.WriteLine("Saving consolidated data...");
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(referenceDataPath, JsonSerializer.Serialize(referenceData, options));
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nSuccessfully saved to: {referenceDataPath}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("========================================");
                Console.WriteLine("ERROR: Consolidation failed!");
                Console.WriteLine("========================================");
                Console.ResetColor();
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine();
                Console.WriteLine("Stack trace:");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }
        
        private static Dictionary<string, List<JsonObject>> ProcessCsvData(JsonObject csvData)
        {
            var tables = new Dictionary<string, List<JsonObject>>();
            
            foreach (var entry in csvData)
            {
                if (entry.Value is not JsonObject entryObj) continue;
                
                var tableName = entryObj["tableName"]?.GetValue<string>();
                if (string.IsNullOrEmpty(tableName)) continue;
                
                // Map R_* to RA_*
                var mappedTableName = tableName.StartsWith("R_") ? "RA_" + tableName.Substring(2) : tableName;
                
                var headers = entryObj["headers"]?.AsArray().Select(h => h?.GetValue<string>()).ToArray();
                var rows = entryObj["rows"]?.AsArray();
                var fileName = entryObj["fileName"]?.GetValue<string>() ?? entry.Key;
                
                if (headers == null || rows == null) continue;
                
                var items = new List<JsonObject>();
                
                foreach (var rowNode in rows)
                {
                    if (rowNode is not JsonArray rowArray) continue;
                    
                    var row = rowArray.Select(r => r?.GetValue<string>() ?? "").ToArray();
                    if (row.Length != headers.Length) continue;
                    
                    // Skip header rows
                    if (row.Length > 0 && row[0].StartsWith("(")) continue;
                    
                    var item = ConvertRowToJson(row, headers, mappedTableName, fileName);
                    if (item != null)
                    {
                        items.Add(item);
                    }
                }
                
                if (items.Count > 0)
                {
                    if (!tables.ContainsKey(mappedTableName))
                    {
                        tables[mappedTableName] = new List<JsonObject>();
                    }
                    tables[mappedTableName].AddRange(items);
                }
            }
            
            return tables;
        }
        
        private static JsonObject ConvertRowToJson(string[] row, string[] headers, string tableName, string fileName)
        {
            var rowDict = new Dictionary<string, string>();
            for (int i = 0; i < headers.Length && i < row.Length; i++)
            {
                rowDict[headers[i]] = row[i] ?? "";
            }
            
            var name1 = rowDict.GetValueOrDefault("NAME1", "").Trim();
            if (string.IsNullOrEmpty(name1)) return null;
            
            var result = new JsonObject();
            
            // Determine primary field
            var primaryField = DeterminePrimaryField(tableName);
            result[primaryField] = name1;
            
            // Extract STATUS_TYPE from filename
            var statusType = ExtractStatusType(fileName);
            if (statusType != null && tableName.Contains("STATUS"))
            {
                result["STATUS_TYPE"] = statusType;
            }
            
            // ALIAS fields
            var alias = rowDict.GetValueOrDefault("ALIAS", "").Trim();
            var name4 = rowDict.GetValueOrDefault("NAME4", "").Trim();
            
            if (!string.IsNullOrEmpty(alias))
            {
                result["ALIAS_ID"] = alias;
                result["ALIAS_SHORT_NAME"] = alias;
            }
            else if (!string.IsNullOrEmpty(name4))
            {
                result["ALIAS_ID"] = name4;
                result["ALIAS_SHORT_NAME"] = name4;
            }
            
            if (!string.IsNullOrEmpty(name4))
            {
                result["ALIAS_LONG_NAME"] = name4;
            }
            else if (!string.IsNullOrEmpty(name1))
            {
                result["ALIAS_LONG_NAME"] = name1;
            }
            
            if (!result.ContainsKey("ALIAS_SHORT_NAME"))
            {
                result["ALIAS_SHORT_NAME"] = name1;
            }
            
            // ABBREVIATION
            var name2 = rowDict.GetValueOrDefault("NAME2", "").Trim();
            var name3 = rowDict.GetValueOrDefault("NAME3", "").Trim();
            if (!string.IsNullOrEmpty(name2))
            {
                result["ABBREVIATION"] = name2;
            }
            else if (!string.IsNullOrEmpty(name3))
            {
                result["ABBREVIATION"] = name3;
            }
            else if (!string.IsNullOrEmpty(alias))
            {
                result["ABBREVIATION"] = alias.Length > 4 ? alias.Substring(0, 4).ToUpper() : alias.ToUpper();
            }
            else
            {
                result["ABBREVIATION"] = name1.Length > 4 ? name1.Substring(0, 4).ToUpper() : name1.ToUpper();
            }
            
            // ACTIVE_IND
            var valueStatus = rowDict.GetValueOrDefault("VALUE_STATUS", "").Trim();
            result["ACTIVE_IND"] = valueStatus.ToLower().Contains("adopted") ? "Y" : 
                                  valueStatus.ToLower().Contains("deprecated") ? "N" : "Y";
            
            result["PREFERRED_IND"] = "Y";
            result["ORIGINAL_IND"] = "Y";
            
            // SOURCE
            var source = rowDict.GetValueOrDefault("SOURCE", "").Trim();
            result["SOURCE"] = string.IsNullOrEmpty(source) ? "PPDM" : source;
            
            return result;
        }
        
        private static string DeterminePrimaryField(string tableName)
        {
            var lower = tableName.ToLower();
            if (lower.Contains("status") && !lower.Contains("type"))
                return "STATUS";
            if (lower.Contains("cost_type"))
                return "COST_TYPE";
            if (lower.Contains("completion_type"))
                return "COMPLETION_TYPE";
            if (lower.Contains("completion_status_type"))
                return "COMPLETION_STATUS_TYPE";
            if (lower.Contains("property_type"))
                return "PROPERTY_TYPE";
            if (lower.Contains("lease_type"))
                return "LEASE_TYPE";
            if (lower.Contains("production_method"))
                return "PRODUCTION_METHOD";
            if (lower.Contains("production_status"))
                return "STATUS";
            if (lower.Contains("production_type"))
                return "PRODUCTION_TYPE";
            if (lower.Contains("equipment_type"))
                return "EQUIPMENT_TYPE";
            if (lower.Contains("equipment_status"))
                return "STATUS";
            if (lower.Contains("facility_type"))
                return "FACILITY_TYPE";
            if (lower.Contains("drilling_method"))
                return "DRILLING_METHOD";
            if (lower.Contains("drilling_type"))
                return "DRILLING_TYPE";
            if (lower.Contains("completion_method"))
                return "COMPLETION_METHOD";
            if (lower.Contains("reservoir_type"))
                return "RESERVOIR_TYPE";
            if (lower.Contains("formation_type"))
                return "FORMATION_TYPE";
            if (lower.Contains("lithology_type"))
                return "LITHOLOGY_TYPE";
            if (lower.Contains("allocation_type"))
                return "ALLOCATION_TYPE";
            if (lower.Contains("account_proc_type"))
                return "ACCOUNT_PROC_TYPE";
            if (lower.Contains("allowable_expense"))
                return "ALLOWABLE_EXPENSE";
            if (lower.Contains("activity_type"))
                return "ACTIVITY_TYPE";
            if (lower.Contains("activity_set_type"))
                return "ACTIVITY_SET_TYPE";
            if (lower.Contains("anl_method_set_type"))
                return "ANL_METHOD_SET_TYPE";
            if (lower.Contains("anl_confidence_type"))
                return "ANL_CONFIDENCE_TYPE";
            if (lower.Contains("anl_repeatability"))
                return "ANL_REPEATABILITY";
            if (lower.Contains("well_status_type"))
                return "STATUS_TYPE";
            if (lower.Contains("type"))
                return "TYPE";
            if (lower.Contains("uom") || lower.Contains("unit_of_measure"))
                return "UOM";
            if (lower.Contains("row_quality"))
                return "ROW_QUALITY";
            if (lower.Contains("source") && !lower.Contains("account"))
                return "SOURCE";
            if (lower.Contains("accounting_method"))
                return "ACCOUNTING_METHOD";
            return "NAME";
        }
        
        private static string ExtractStatusType(string fileName)
        {
            if (fileName.Contains("_PPDM"))
            {
                var parts = fileName.Split(new[] { "_PPDM" }, StringSplitOptions.None)[0].Split('_');
                if (parts.Length > 1)
                {
                    return string.Join("_", parts);
                }
            }
            return null;
        }
        
        private static HashSet<string> GetExistingTableNames(JsonObject referenceData)
        {
            var tables = new HashSet<string>();
            if (referenceData["tables"] is JsonArray tablesArray)
            {
                foreach (var tableNode in tablesArray)
                {
                    if (tableNode is JsonObject tableObj && tableObj["tableName"] != null)
                    {
                        tables.Add(tableObj["tableName"].GetValue<string>());
                    }
                }
            }
            return tables;
        }
        
        private static int AddToExistingTable(JsonObject referenceData, string tableName, List<JsonObject> newItems)
        {
            if (referenceData["tables"] is not JsonArray tablesArray) return 0;
            
            foreach (var tableNode in tablesArray)
            {
                if (tableNode is not JsonObject tableObj) continue;
                if (tableObj["tableName"]?.GetValue<string>() != tableName) continue;
                
                var existingData = tableObj["data"] as JsonArray ?? new JsonArray();
                var existingKeys = new HashSet<string>();
                
                // Get existing keys
                foreach (var itemNode in existingData)
                {
                    if (itemNode is not JsonObject itemObj) continue;
                    var primaryField = DeterminePrimaryField(tableName);
                    var primaryValue = itemObj[primaryField]?.GetValue<string>() ?? "";
                    var statusType = itemObj["STATUS_TYPE"]?.GetValue<string>() ?? "";
                    var key = string.IsNullOrEmpty(statusType) ? primaryValue : $"{statusType}:{primaryValue}";
                    if (!string.IsNullOrEmpty(key))
                    {
                        existingKeys.Add(key);
                    }
                }
                
                // Add new items that aren't duplicates
                int added = 0;
                foreach (var newItem in newItems)
                {
                    var primaryField = DeterminePrimaryField(tableName);
                    var primaryValue = newItem[primaryField]?.GetValue<string>() ?? "";
                    var statusType = newItem["STATUS_TYPE"]?.GetValue<string>() ?? "";
                    var key = string.IsNullOrEmpty(statusType) ? primaryValue : $"{statusType}:{primaryValue}";
                    
                    if (!string.IsNullOrEmpty(key) && !existingKeys.Contains(key))
                    {
                        existingData.Add(newItem);
                        existingKeys.Add(key);
                        added++;
                    }
                }
                
                tableObj["data"] = existingData;
                return added;
            }
            
            return 0;
        }
        
        private static void AddNewTable(JsonObject referenceData, string tableName, List<JsonObject> items)
        {
            if (referenceData["tables"] is not JsonArray tablesArray)
            {
                tablesArray = new JsonArray();
                referenceData["tables"] = tablesArray;
            }
            
            var dataArray = new JsonArray();
            foreach (var item in items)
            {
                dataArray.Add(item);
            }
            
            var newTable = new JsonObject
            {
                ["tableName"] = tableName,
                ["description"] = $"Reference data for {tableName}",
                ["data"] = dataArray
            };
            
            tablesArray.Add(newTable);
        }
    }
}

