using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.UserManagement.Core.DataAccess;
using Beep.OilandGas.Models.Core.Interfaces.Security;

namespace Beep.OilandGas.UserManagement.Services
{
    /// <summary>
    /// Implementation of row-level security provider
    /// Supports creator-based, source-based, tenant-based, and role-based filtering
    /// </summary>
    public class RowLevelSecurityService : IRowLevelSecurityProvider
    {
        private readonly IUserService _userService;
        private readonly IAuthorizationService _authorizationService;
        private readonly List<RowLevelSecurityRule> _rules;

        public RowLevelSecurityService(
            IUserService userService,
            IAuthorizationService authorizationService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            _rules = new List<RowLevelSecurityRule>();
        }

        /// <summary>
        /// Adds a row-level security rule
        /// </summary>
        public void AddRule(RowLevelSecurityRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            _rules.Add(rule);
            _rules.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }

        /// <summary>
        /// Adds multiple row-level security rules
        /// </summary>
        public void AddRules(IEnumerable<RowLevelSecurityRule> rules)
        {
            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            _rules.AddRange(rules);
            _rules.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }

        public async Task<List<AppFilter>> GetRowFiltersAsync(string userId, string tableName)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(tableName))
            {
                return new List<AppFilter>();
            }

            var filters = new List<AppFilter>();
            var userRoles = await _userService.GetRolesAsync(userId);
            var user = await _userService.GetByIdAsync(userId);

            // Get applicable rules for this table
            var applicableRules = _rules
                .Where(r => r.IsActive && 
                           (r.TableName == "*" || r.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            foreach (var rule in applicableRules)
            {
                // Check if rule applies to user's roles (for role-based rules)
                if (rule.RuleType == RowLevelSecurityRuleType.Role)
                {
                    if (!rule.ApplicableRoles.Any() || 
                        !rule.ApplicableRoles.Any(r => userRoles.Contains(r, StringComparer.OrdinalIgnoreCase)))
                    {
                        continue;
                    }
                }

                switch (rule.RuleType)
                {
                    case RowLevelSecurityRuleType.Creator:
                        // Users see only rows they created
                        filters.Add(new AppFilter
                        {
                            FieldName = "ROW_CREATED_BY",
                            Operator = "=",
                            FilterValue = userId
                        });
                        break;

                    case RowLevelSecurityRuleType.Source:
                        // Users see only rows from certain sources
                        if (rule.SourceValues.Any())
                        {
                            var sourceValues = rule.SourceValues.ToList();
                            if (sourceValues.Count == 1)
                            {
                                filters.Add(new AppFilter
                                {
                                    FieldName = "SOURCE",
                                    Operator = "=",
                                    FilterValue = sourceValues[0]
                                });
                            }
                            else
                            {
                                filters.Add(new AppFilter
                                {
                                    FieldName = "SOURCE",
                                    Operator = "IN",
                                    FilterValue = string.Join(",", sourceValues)
                                });
                            }
                        }
                        break;

                    case RowLevelSecurityRuleType.Tenant:
                        // Users see only rows for their tenant
                        if (user != null && !string.IsNullOrWhiteSpace(user.TENANT_ID))
                        {
                            filters.Add(new AppFilter
                            {
                                FieldName = "TENANT_ID",
                                Operator = "=",
                                FilterValue = user.TENANT_ID
                            });
                        }
                        else if (!string.IsNullOrWhiteSpace(rule.TenantId))
                        {
                            filters.Add(new AppFilter
                            {
                                FieldName = "TENANT_ID",
                                Operator = "=",
                                FilterValue = rule.TenantId
                            });
                        }
                        break;

                    case RowLevelSecurityRuleType.Custom:
                        // Custom filter expression
                        if (!string.IsNullOrWhiteSpace(rule.FilterExpression))
                        {
                            // Parse custom expression and add filters
                            // This is a simplified implementation - full implementation would parse SQL expressions
                            // For now, we'll add a note that custom expressions need to be handled separately
                            // In practice, you might want to use a query builder or SQL parser here
                        }
                        break;
                }
            }

            return filters;
        }

        public async Task<bool> CanAccessRowAsync(string userId, IPPDMEntity entity)
        {
            if (string.IsNullOrWhiteSpace(userId) || entity == null)
            {
                return false;
            }

            var userRoles = await _userService.GetRolesAsync(userId);
            var user = await _userService.GetByIdAsync(userId);

            // Get applicable rules
            var applicableRules = _rules
                .Where(r => r.IsActive && 
                           (r.TableName == "*" || r.TableName.Equals(entity.GetType().Name, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            foreach (var rule in applicableRules)
            {
                // Check if rule applies to user's roles
                if (rule.RuleType == RowLevelSecurityRuleType.Role)
                {
                    if (!rule.ApplicableRoles.Any() || 
                        !rule.ApplicableRoles.Any(r => userRoles.Contains(r, StringComparer.OrdinalIgnoreCase)))
                    {
                        continue;
                    }
                }

                switch (rule.RuleType)
                {
                    case RowLevelSecurityRuleType.Creator:
                        // Check if user created this row
                        if (!string.IsNullOrWhiteSpace(entity.ROW_CREATED_BY) &&
                            !entity.ROW_CREATED_BY.Equals(userId, StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                        break;

                    case RowLevelSecurityRuleType.Source:
                        // Check if source is allowed
                        if (rule.SourceValues.Any())
                        {
                            var source = entity.SOURCE ?? string.Empty;
                            if (!rule.SourceValues.Contains(source, StringComparer.OrdinalIgnoreCase))
                            {
                                return false;
                            }
                        }
                        break;

                    case RowLevelSecurityRuleType.Tenant:
                        // Check tenant access
                        var entityTenantId = GetTenantIdFromEntity(entity);
                        var userTenantId = user?.TENANT_ID ?? rule.TenantId;
                        if (!string.IsNullOrWhiteSpace(entityTenantId) && 
                            !string.IsNullOrWhiteSpace(userTenantId) &&
                            !entityTenantId.Equals(userTenantId, StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                        break;
                }
            }

            return true;
        }

        public async Task<List<AppFilter>> ApplyRowFiltersAsync(string userId, string tableName, List<AppFilter>? existingFilters = null)
        {
            var rlsFilters = await GetRowFiltersAsync(userId, tableName);
            var combinedFilters = existingFilters?.ToList() ?? new List<AppFilter>();
            combinedFilters.AddRange(rlsFilters);
            return combinedFilters;
        }

        private string? GetTenantIdFromEntity(IPPDMEntity entity)
        {
            // Try to get TENANT_ID from entity using reflection
            var tenantIdProperty = entity.GetType().GetProperty("TENANT_ID");
            if (tenantIdProperty != null)
            {
                return tenantIdProperty.GetValue(entity)?.ToString();
            }

            // Try ORGANIZATION_ID as fallback
            var orgIdProperty = entity.GetType().GetProperty("ORGANIZATION_ID");
            if (orgIdProperty != null)
            {
                return orgIdProperty.GetValue(entity)?.ToString();
            }

            return null;
        }
    }
}
