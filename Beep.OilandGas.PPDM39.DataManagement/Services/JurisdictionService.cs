using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.UOW;
using TheTechIdea.Beep.Report;
using static Beep.OilandGas.PPDM39.Repositories.IPPDM39DefaultsRepository;
using JurisdictionConfig = Beep.OilandGas.PPDM39.Repositories.JurisdictionConfig;

namespace Beep.OilandGas.PPDM39.DataManagement.Services
{
    /// <summary>
    /// Service for managing jurisdiction-specific configurations
    /// Handles currency, units, validation rules, and defaults per jurisdiction
    /// </summary>
    public class JurisdictionService : IJurisdictionService
    {
        private readonly IDMEEditor _editor;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly string _connectionName;
        private string? _currentJurisdiction;
        private bool _jurisdictionsLoadedFromDatabase = false;
        private readonly object _jurisdictionLock = new object();

        // Default jurisdiction configurations (instance variable to allow database updates)
        private readonly Dictionary<string, JurisdictionConfig> _jurisdictionConfigs = new Dictionary<string, JurisdictionConfig>(StringComparer.OrdinalIgnoreCase);

        public JurisdictionService(
            IDMEEditor editor = null,
            IPPDM39DefaultsRepository defaults = null,
            string connectionName = "PPDM39")
        {
            _editor = editor;
            _defaults = defaults;
            _connectionName = connectionName;
            InitializeDefaultJurisdictions();
        }

        /// <summary>
        /// Initialize default jurisdiction configurations
        /// Called during construction or when loading from database
        /// </summary>
        private void InitializeDefaultJurisdictions()
        {
            lock (_jurisdictionLock)
            {
                if (_jurisdictionConfigs.Count > 0)
                    return; // Already initialized

                // North America
                _jurisdictionConfigs["US"] = new Beep.OilandGas.PPDM39.Repositories.JurisdictionConfig
                {
                    JurisdictionId = "US",
                    Currency = "USD",
                    CurrencySymbol = "$",
                    DepthUnit = "FT",
                    TemperatureUnit = "F",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["CA"] = new JurisdictionConfig
                {
                    JurisdictionId = "CA",
                    Currency = "CAD",
                    CurrencySymbol = "$",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "KPA",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "M3" }, { "GAS", "E3M3" }, { "WATER", "M3" } }
                };
                _jurisdictionConfigs["MX"] = new JurisdictionConfig
                {
                    JurisdictionId = "MX",
                    Currency = "MXN",
                    CurrencySymbol = "$",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "KPA",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };

                // Europe
                _jurisdictionConfigs["GB"] = new JurisdictionConfig
                {
                    JurisdictionId = "GB",
                    Currency = "GBP",
                    CurrencySymbol = "£",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "BAR",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["NO"] = new JurisdictionConfig
                {
                    JurisdictionId = "NO",
                    Currency = "NOK",
                    CurrencySymbol = "kr",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "BAR",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "SM3" }, { "GAS", "SM3" }, { "WATER", "SM3" } }
                };
                _jurisdictionConfigs["NL"] = new JurisdictionConfig
                {
                    JurisdictionId = "NL",
                    Currency = "EUR",
                    CurrencySymbol = "€",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "BAR",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "SM3" }, { "GAS", "SM3" }, { "WATER", "SM3" } }
                };
                _jurisdictionConfigs["DK"] = new JurisdictionConfig
                {
                    JurisdictionId = "DK",
                    Currency = "DKK",
                    CurrencySymbol = "kr",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "BAR",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "SM3" }, { "GAS", "SM3" }, { "WATER", "SM3" } }
                };
                _jurisdictionConfigs["RU"] = new JurisdictionConfig
                {
                    JurisdictionId = "RU",
                    Currency = "RUB",
                    CurrencySymbol = "₽",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "ATM",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "M3" }, { "GAS", "E3M3" }, { "WATER", "M3" } }
                };

                // Middle East
                _jurisdictionConfigs["SA"] = new JurisdictionConfig
                {
                    JurisdictionId = "SA",
                    Currency = "SAR",
                    CurrencySymbol = "﷼",
                    DepthUnit = "FT",
                    TemperatureUnit = "F",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["AE"] = new JurisdictionConfig
                {
                    JurisdictionId = "AE",
                    Currency = "AED",
                    CurrencySymbol = "د.إ",
                    DepthUnit = "FT",
                    TemperatureUnit = "F",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["KW"] = new JurisdictionConfig
                {
                    JurisdictionId = "KW",
                    Currency = "KWD",
                    CurrencySymbol = "د.ك",
                    DepthUnit = "FT",
                    TemperatureUnit = "F",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["QA"] = new JurisdictionConfig
                {
                    JurisdictionId = "QA",
                    Currency = "QAR",
                    CurrencySymbol = "﷼",
                    DepthUnit = "FT",
                    TemperatureUnit = "F",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["OM"] = new JurisdictionConfig
                {
                    JurisdictionId = "OM",
                    Currency = "OMR",
                    CurrencySymbol = "﷼",
                    DepthUnit = "FT",
                    TemperatureUnit = "F",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["IQ"] = new JurisdictionConfig
                {
                    JurisdictionId = "IQ",
                    Currency = "IQD",
                    CurrencySymbol = "ع.د",
                    DepthUnit = "FT",
                    TemperatureUnit = "F",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["IR"] = new JurisdictionConfig
                {
                    JurisdictionId = "IR",
                    Currency = "IRR",
                    CurrencySymbol = "﷼",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "BAR",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };

                // Asia Pacific
                _jurisdictionConfigs["AU"] = new JurisdictionConfig
                {
                    JurisdictionId = "AU",
                    Currency = "AUD",
                    CurrencySymbol = "$",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "KPA",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["CN"] = new JurisdictionConfig
                {
                    JurisdictionId = "CN",
                    Currency = "CNY",
                    CurrencySymbol = "¥",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "MPA",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "M3" }, { "GAS", "E3M3" }, { "WATER", "M3" } }
                };
                _jurisdictionConfigs["IN"] = new JurisdictionConfig
                {
                    JurisdictionId = "IN",
                    Currency = "INR",
                    CurrencySymbol = "₹",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "KPA",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["MY"] = new JurisdictionConfig
                {
                    JurisdictionId = "MY",
                    Currency = "MYR",
                    CurrencySymbol = "RM",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["SG"] = new JurisdictionConfig
                {
                    JurisdictionId = "SG",
                    Currency = "SGD",
                    CurrencySymbol = "$",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["TH"] = new JurisdictionConfig
                {
                    JurisdictionId = "TH",
                    Currency = "THB",
                    CurrencySymbol = "฿",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["ID"] = new JurisdictionConfig
                {
                    JurisdictionId = "ID",
                    Currency = "IDR",
                    CurrencySymbol = "Rp",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["JP"] = new JurisdictionConfig
                {
                    JurisdictionId = "JP",
                    Currency = "JPY",
                    CurrencySymbol = "¥",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "MPA",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };

                // South America
                _jurisdictionConfigs["BR"] = new JurisdictionConfig
                {
                    JurisdictionId = "BR",
                    Currency = "BRL",
                    CurrencySymbol = "R$",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "KPA",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["AR"] = new JurisdictionConfig
                {
                    JurisdictionId = "AR",
                    Currency = "ARS",
                    CurrencySymbol = "$",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "BAR",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["CO"] = new JurisdictionConfig
                {
                    JurisdictionId = "CO",
                    Currency = "COP",
                    CurrencySymbol = "$",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["VE"] = new JurisdictionConfig
                {
                    JurisdictionId = "VE",
                    Currency = "VES",
                    CurrencySymbol = "Bs",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["EC"] = new JurisdictionConfig
                {
                    JurisdictionId = "EC",
                    Currency = "USD",
                    CurrencySymbol = "$",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };

                // Africa
                _jurisdictionConfigs["NG"] = new JurisdictionConfig
                {
                    JurisdictionId = "NG",
                    Currency = "NGN",
                    CurrencySymbol = "₦",
                    DepthUnit = "FT",
                    TemperatureUnit = "F",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["ZA"] = new JurisdictionConfig
                {
                    JurisdictionId = "ZA",
                    Currency = "ZAR",
                    CurrencySymbol = "R",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "BAR",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["AO"] = new JurisdictionConfig
                {
                    JurisdictionId = "AO",
                    Currency = "AOA",
                    CurrencySymbol = "Kz",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "BAR",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["DZ"] = new JurisdictionConfig
                {
                    JurisdictionId = "DZ",
                    Currency = "DZD",
                    CurrencySymbol = "د.ج",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "BAR",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["LY"] = new JurisdictionConfig
                {
                    JurisdictionId = "LY",
                    Currency = "LYD",
                    CurrencySymbol = "ل.د",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "BAR",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["EG"] = new JurisdictionConfig
                {
                    JurisdictionId = "EG",
                    Currency = "EGP",
                    CurrencySymbol = "£",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
            }
        }

        private JurisdictionConfig GetJurisdictionConfig(string jurisdictionId)
        {
            // Ensure defaults are initialized
            if (_jurisdictionConfigs.Count == 0)
            {
                InitializeDefaultJurisdictions();
            }

            if (string.IsNullOrEmpty(jurisdictionId))
                jurisdictionId = _currentJurisdiction ?? "US";

            return _jurisdictionConfigs.TryGetValue(jurisdictionId, out var config) 
                ? config 
                : _jurisdictionConfigs["US"]; // Default to US
        }

        public string GetCurrencyForJurisdiction(string jurisdictionId)
        {
            return GetJurisdictionConfig(jurisdictionId).Currency;
        }

        public string GetCurrencySymbolForJurisdiction(string jurisdictionId)
        {
            return GetJurisdictionConfig(jurisdictionId).CurrencySymbol;
        }

        public Dictionary<string, string> GetVolumeUnitsForJurisdiction(string jurisdictionId)
        {
            return new Dictionary<string, string>(GetJurisdictionConfig(jurisdictionId).VolumeUnits);
        }

        public string GetDepthUnitForJurisdiction(string jurisdictionId)
        {
            return GetJurisdictionConfig(jurisdictionId).DepthUnit;
        }

        public string GetTemperatureUnitForJurisdiction(string jurisdictionId)
        {
            return GetJurisdictionConfig(jurisdictionId).TemperatureUnit;
        }

        public string GetPressureUnitForJurisdiction(string jurisdictionId)
        {
            return GetJurisdictionConfig(jurisdictionId).PressureUnit;
        }

        public Dictionary<string, string> GetAllUnitsForJurisdiction(string jurisdictionId)
        {
            var config = GetJurisdictionConfig(jurisdictionId);
            var units = new Dictionary<string, string>
            {
                { "DEPTH", config.DepthUnit },
                { "TEMPERATURE", config.TemperatureUnit },
                { "PRESSURE", config.PressureUnit }
            };

            foreach (var kvp in config.VolumeUnits)
            {
                units[$"VOLUME_{kvp.Key}"] = kvp.Value;
            }

            return units;
        }

        public async Task<List<JurisdictionValidationRule>> GetValidationRulesForJurisdictionAsync(string jurisdictionId, string? entityType = null)
        {
            await Task.CompletedTask;
            
            // Return empty list - can be extended to load from database or configuration
            var rules = new List<JurisdictionValidationRule>();

            // Example rules that could be jurisdiction-specific
            if (jurisdictionId.Equals("CA", StringComparison.OrdinalIgnoreCase))
            {
                rules.Add(new JurisdictionValidationRule
                {
                    RuleId = "CA_UWI_FORMAT",
                    JurisdictionId = "CA",
                    EntityType = "WELL",
                    FieldName = "UWI",
                    RuleType = "Pattern",
                    RuleValue = @"^\d{2}-\d{2}-\d{3}-\d{2}W\d{1}$",
                    ErrorMessage = "UWI must match Canadian format (XX-XX-XXX-XXW#)",
                    Severity = "Error",
                    IsActive = true
                });
            }

            if (!string.IsNullOrEmpty(entityType))
            {
                rules = rules.Where(r => r.EntityType == null || r.EntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return rules;
        }

        public async Task<JurisdictionValidationResult> ValidateValueForJurisdictionAsync(string jurisdictionId, string fieldName, object value, string? entityType = null)
        {
            var result = new JurisdictionValidationResult
            {
                IsValid = true,
                FieldName = fieldName
            };

            var rules = await GetValidationRulesForJurisdictionAsync(jurisdictionId, entityType);
            var applicableRules = rules.Where(r => r.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase) && r.IsActive).ToList();

            foreach (var rule in applicableRules)
            {
                var isValid = ValidateRule(rule, value);
                if (!isValid)
                {
                    result.IsValid = false;
                    result.RuleId = rule.RuleId;

                    if (rule.Severity == "Error")
                        result.ErrorMessages.Add(rule.ErrorMessage);
                    else if (rule.Severity == "Warning")
                        result.WarningMessages.Add(rule.ErrorMessage);
                }
            }

            return result;
        }

        private bool ValidateRule(JurisdictionValidationRule rule, object value)
        {
            if (value == null)
                return rule.RuleType != "Required";

            var stringValue = value.ToString() ?? string.Empty;

            return rule.RuleType switch
            {
                "Required" => !string.IsNullOrWhiteSpace(stringValue),
                "Pattern" when !string.IsNullOrEmpty(rule.RuleValue) => 
                    System.Text.RegularExpressions.Regex.IsMatch(stringValue, rule.RuleValue),
                "Range" when !string.IsNullOrEmpty(rule.RuleValue) => ValidateRange(stringValue, rule.RuleValue),
                _ => true
            };
        }

        private bool ValidateRange(string value, string rangeSpec)
        {
            if (!decimal.TryParse(value, out var numValue))
                return false;

            var parts = rangeSpec.Split(',');
            if (parts.Length != 2)
                return false;

            var hasMin = decimal.TryParse(parts[0], out var min);
            var hasMax = decimal.TryParse(parts[1], out var max);

            if (hasMin && numValue < min) return false;
            if (hasMax && numValue > max) return false;

            return true;
        }

        public async Task<Dictionary<string, object>> GetJurisdictionDefaultsForTableAsync(string jurisdictionId, string tableName)
        {
            await Task.CompletedTask;

            var defaults = new Dictionary<string, object>();
            var config = GetJurisdictionConfig(jurisdictionId);

            // Add jurisdiction-specific defaults based on table
            switch (tableName.ToUpperInvariant())
            {
                case "WELL":
                case "WELLBORE":
                    defaults["DEPTH_OUOM"] = config.DepthUnit;
                    break;

                case "PRODUCTION":
                    defaults["OIL_VOLUME_OUOM"] = config.VolumeUnits.GetValueOrDefault("OIL", "BBL");
                    defaults["GAS_VOLUME_OUOM"] = config.VolumeUnits.GetValueOrDefault("GAS", "MSCF");
                    defaults["WATER_VOLUME_OUOM"] = config.VolumeUnits.GetValueOrDefault("WATER", "BBL");
                    break;

                case "POOL":
                    defaults["PRESSURE_OUOM"] = config.PressureUnit;
                    defaults["TEMPERATURE_OUOM"] = config.TemperatureUnit;
                    break;
            }

            return defaults;
        }

        public void SetCurrentJurisdiction(string jurisdictionId)
        {
            // Ensure defaults are initialized
            if (_jurisdictionConfigs.Count == 0)
            {
                InitializeDefaultJurisdictions();
            }

            if (!string.IsNullOrEmpty(jurisdictionId) && _jurisdictionConfigs.ContainsKey(jurisdictionId))
            {
                _currentJurisdiction = jurisdictionId;
            }
            else
            {
                _currentJurisdiction = "US"; // Default to US
            }
        }

        public string? GetCurrentJurisdiction()
        {
            return _currentJurisdiction;
        }

        public async Task<List<string>> GetAvailableJurisdictionsAsync()
        {
            // Ensure defaults are initialized
            if (_jurisdictionConfigs.Count == 0)
            {
                InitializeDefaultJurisdictions();
            }

            // Optionally load from database if not already loaded
            if (!_jurisdictionsLoadedFromDatabase && _editor != null)
            {
                await LoadJurisdictionsFromBusinessAssociateAsync();
            }

            return _jurisdictionConfigs.Keys.OrderBy(k => k).ToList();
        }

        public async Task LoadJurisdictionsFromBusinessAssociateAsync()
        {
            if (_editor == null)
            {
                return; // Cannot load from database without editor
            }

            try
            {
                lock (_jurisdictionLock)
                {
                    if (_jurisdictionsLoadedFromDatabase)
                        return; // Already loaded
                }

                // Ensure defaults are initialized first
                if (_jurisdictionConfigs.Count == 0)
                {
                    InitializeDefaultJurisdictions();
                }

                var uow = UnitOfWorkFactory.CreateUnitOfWork(
                    typeof(object),
                    _editor,
                    _connectionName,
                    "BUSINESS_ASSOCIATE",
                    "BUSINESS_ASSOCIATE_ID");

                // Get distinct jurisdictions from BUSINESS_ASSOCIATE table
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ACTIVE_IND", FilterValue = _defaults?.GetActiveIndicatorYes() ?? "Y", Operator = "=" }
                };

                var queryResult = await uow.Get(filters);
                var businessAssociates = ConvertToDynamicList(queryResult);

                // Extract unique jurisdictions and build configurations
                var jurisdictionsFromDb = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var ba in businessAssociates)
                {
                    if (ba is System.Dynamic.ExpandoObject expando)
                    {
                        var dict = (IDictionary<string, object>)expando;
                        
                        // Check for JURISDICTION field
                        if (dict.ContainsKey("JURISDICTION") && dict["JURISDICTION"] != null)
                        {
                            var jurisdictionId = dict["JURISDICTION"].ToString();
                            if (!string.IsNullOrWhiteSpace(jurisdictionId) && !jurisdictionsFromDb.Contains(jurisdictionId))
                            {
                                jurisdictionsFromDb.Add(jurisdictionId);

                                // If this jurisdiction doesn't exist in defaults, create a config from BUSINESS_ASSOCIATE data
                                if (!_jurisdictionConfigs.ContainsKey(jurisdictionId))
                                {
                                    var config = CreateJurisdictionConfigFromBusinessAssociate(dict, jurisdictionId);
                                    if (config != null)
                                    {
                                        _jurisdictionConfigs[jurisdictionId] = config;
                                    }
                                }
                            }
                        }

                        // Also check for COUNTRY_CODE or COUNTRY fields
                        var countryField = dict.ContainsKey("COUNTRY_CODE") ? "COUNTRY_CODE" :
                                          dict.ContainsKey("COUNTRY") ? "COUNTRY" : null;

                        if (countryField != null && dict[countryField] != null)
                        {
                            var countryCode = dict[countryField].ToString();
                            if (!string.IsNullOrWhiteSpace(countryCode) && !jurisdictionsFromDb.Contains(countryCode))
                            {
                                jurisdictionsFromDb.Add(countryCode);

                                if (!_jurisdictionConfigs.ContainsKey(countryCode))
                                {
                                    var config = CreateJurisdictionConfigFromBusinessAssociate(dict, countryCode);
                                    if (config != null)
                                    {
                                        _jurisdictionConfigs[countryCode] = config;
                                    }
                                }
                            }
                        }
                    }
                }

                lock (_jurisdictionLock)
                {
                    _jurisdictionsLoadedFromDatabase = true;
                }
            }
            catch (Exception)
            {
                // Silently fail - defaults will be used instead
                // Could log this in production
            }
        }

        private List<dynamic> ConvertToDynamicList(dynamic result)
        {
            var list = new List<dynamic>();
            if (result == null) return list;

            if (result is System.Collections.IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    list.Add(item);
                }
            }
            else
            {
                list.Add(result);
            }

            return list;
        }

        /// <summary>
        /// Creates a jurisdiction configuration from BUSINESS_ASSOCIATE record
        /// Uses defaults and infers from common patterns if fields are not available
        /// </summary>
        private Beep.OilandGas.PPDM39.Repositories.JurisdictionConfig? CreateJurisdictionConfigFromBusinessAssociate(IDictionary<string, object> dict, string jurisdictionId)
        {
            // Try to extract currency and unit information from BUSINESS_ASSOCIATE
            // Many fields may not exist, so we'll use intelligent defaults based on jurisdiction ID
            
            var config = new JurisdictionConfig
            {
                JurisdictionId = jurisdictionId
            };

            // Try to get currency from BUSINESS_ASSOCIATE fields
            if (dict.ContainsKey("CURRENCY_CODE") && dict["CURRENCY_CODE"] != null)
            {
                config.Currency = dict["CURRENCY_CODE"].ToString() ?? "USD";
            }
            else if (dict.ContainsKey("CURRENCY") && dict["CURRENCY"] != null)
            {
                config.Currency = dict["CURRENCY"].ToString() ?? "USD";
            }
            else
            {
                // Infer currency from jurisdiction ID (common country codes)
                config.Currency = GetCurrencyByCountryCode(jurisdictionId);
            }

            // Set currency symbol based on currency
            config.CurrencySymbol = GetCurrencySymbol(config.Currency);

            // Try to get unit preferences from BUSINESS_ASSOCIATE
            // If not available, infer based on jurisdiction (metric vs imperial)
            var useMetric = IsMetricJurisdiction(jurisdictionId);
            
            config.DepthUnit = useMetric ? "M" : "FT";
            config.TemperatureUnit = useMetric ? "C" : "F";
            config.PressureUnit = useMetric ? (config.Currency == "EUR" || config.Currency == "GBP" ? "BAR" : "KPA") : "PSI";
            
            // Volume units - typically BBL for oil regardless, but gas can vary
            config.VolumeUnits = new Dictionary<string, string>
            {
                { "OIL", useMetric && (jurisdictionId == "NO" || jurisdictionId == "NL") ? "SM3" : "BBL" },
                { "GAS", useMetric ? (jurisdictionId == "NO" || jurisdictionId == "NL" ? "SM3" : "E3M3") : "MSCF" },
                { "WATER", useMetric && (jurisdictionId == "NO" || jurisdictionId == "NL") ? "SM3" : "BBL" }
            };

            return config;
        }

        /// <summary>
        /// Gets currency code based on country/jurisdiction code
        /// </summary>
        private string GetCurrencyByCountryCode(string countryCode)
        {
            // Map common country codes to currencies
            return countryCode.ToUpperInvariant() switch
            {
                "US" => "USD",
                "CA" => "CAD",
                "MX" => "MXN",
                "GB" => "GBP",
                "NO" => "NOK",
                "DK" => "DKK",
                "NL" => "EUR",
                "DE" => "EUR",
                "FR" => "EUR",
                "IT" => "EUR",
                "ES" => "EUR",
                "SA" => "SAR",
                "AE" => "AED",
                "KW" => "KWD",
                "QA" => "QAR",
                "AU" => "AUD",
                "CN" => "CNY",
                "IN" => "INR",
                "JP" => "JPY",
                "BR" => "BRL",
                "AR" => "ARS",
                "NG" => "NGN",
                "ZA" => "ZAR",
                _ => "USD" // Default to USD
            };
        }

        /// <summary>
        /// Gets currency symbol based on currency code
        /// </summary>
        private string GetCurrencySymbol(string currencyCode)
        {
            return currencyCode.ToUpperInvariant() switch
            {
                "USD" => "$",
                "CAD" => "$",
                "MXN" => "$",
                "AUD" => "$",
                "SGD" => "$",
                "GBP" => "£",
                "EUR" => "€",
                "NOK" => "kr",
                "DKK" => "kr",
                "SAR" => "﷼",
                "AED" => "د.إ",
                "CNY" => "¥",
                "JPY" => "¥",
                "INR" => "₹",
                "BRL" => "R$",
                "RUB" => "₽",
                _ => "$" // Default
            };
        }

        /// <summary>
        /// Determines if a jurisdiction typically uses metric units
        /// </summary>
        private bool IsMetricJurisdiction(string jurisdictionId)
        {
            // US and a few others use imperial
            var imperialJurisdictions = new[] { "US", "SA", "AE", "KW", "QA", "OM", "IQ", "NG" };
            return !imperialJurisdictions.Contains(jurisdictionId.ToUpperInvariant());
        }

        public async Task SaveJurisdictionConfigAsync(JurisdictionConfig config, string databaseId, string? userId = null)
        {
            if (_defaults == null)
                throw new InvalidOperationException("IPPDM39DefaultsRepository is required for database persistence.");

            await _defaults.SetDefaultValueAsync($"JURISDICTION.{config.JurisdictionId}.CURRENCY", config.Currency, databaseId, userId, "Jurisdiction", "String", $"Currency for {config.JurisdictionId}");
            await _defaults.SetDefaultValueAsync($"JURISDICTION.{config.JurisdictionId}.CURRENCY_SYMBOL", config.CurrencySymbol, databaseId, userId, "Jurisdiction", "String", $"Currency symbol for {config.JurisdictionId}");
            await _defaults.SetDefaultValueAsync($"JURISDICTION.{config.JurisdictionId}.DEPTH_UNIT", config.DepthUnit, databaseId, userId, "Jurisdiction", "String", $"Depth unit for {config.JurisdictionId}");
            await _defaults.SetDefaultValueAsync($"JURISDICTION.{config.JurisdictionId}.TEMPERATURE_UNIT", config.TemperatureUnit, databaseId, userId, "Jurisdiction", "String", $"Temperature unit for {config.JurisdictionId}");
            await _defaults.SetDefaultValueAsync($"JURISDICTION.{config.JurisdictionId}.PRESSURE_UNIT", config.PressureUnit, databaseId, userId, "Jurisdiction", "String", $"Pressure unit for {config.JurisdictionId}");

            foreach (var volumeUnit in config.VolumeUnits)
            {
                await _defaults.SetDefaultValueAsync($"JURISDICTION.{config.JurisdictionId}.VOLUME_UNIT.{volumeUnit.Key}", volumeUnit.Value, databaseId, userId, "Jurisdiction", "String", $"Volume unit for {volumeUnit.Key} in {config.JurisdictionId}");
            }
        }

        public async Task<JurisdictionConfig?> LoadJurisdictionConfigAsync(string jurisdictionId, string databaseId, string? userId = null)
        {
            if (_defaults == null)
                return GetJurisdictionConfig(jurisdictionId); // Fallback to in-memory config

            try
            {
                var currency = await _defaults.GetDefaultValueAsync($"JURISDICTION.{jurisdictionId}.CURRENCY", databaseId, userId);
                var currencySymbol = await _defaults.GetDefaultValueAsync($"JURISDICTION.{jurisdictionId}.CURRENCY_SYMBOL", databaseId, userId);
                var depthUnit = await _defaults.GetDefaultValueAsync($"JURISDICTION.{jurisdictionId}.DEPTH_UNIT", databaseId, userId);
                var temperatureUnit = await _defaults.GetDefaultValueAsync($"JURISDICTION.{jurisdictionId}.TEMPERATURE_UNIT", databaseId, userId);
                var pressureUnit = await _defaults.GetDefaultValueAsync($"JURISDICTION.{jurisdictionId}.PRESSURE_UNIT", databaseId, userId);

                // If no values found in database, return null to use in-memory fallback
                if (string.IsNullOrEmpty(currency))
                    return null;

                var config = new Beep.OilandGas.PPDM39.Repositories.JurisdictionConfig
                {
                    JurisdictionId = jurisdictionId,
                    Currency = currency ?? "USD",
                    CurrencySymbol = currencySymbol ?? "$",
                    DepthUnit = depthUnit ?? "FT",
                    TemperatureUnit = temperatureUnit ?? "F",
                    PressureUnit = pressureUnit ?? "PSI",
                    VolumeUnits = new Dictionary<string, string>()
                };

                // Load volume units
                var volumeUnits = await _defaults.GetDefaultsByCategoryAsync("Jurisdiction", databaseId, userId);
                foreach (var kvp in volumeUnits)
                {
                    if (kvp.Key.StartsWith($"JURISDICTION.{jurisdictionId}.VOLUME_UNIT.", StringComparison.OrdinalIgnoreCase))
                    {
                        var volumeType = kvp.Key.Substring($"JURISDICTION.{jurisdictionId}.VOLUME_UNIT.".Length);
                        config.VolumeUnits[volumeType] = kvp.Value;
                    }
                }

                return config;
            }
            catch
            {
                return GetJurisdictionConfig(jurisdictionId); // Fallback to in-memory config
            }
        }
    }
}
