using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Interfaces.Security;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.UserManagement.Contracts.Services;
using Beep.OilandGas.UserManagement.Models.Requests.UserManagement;
using Beep.OilandGas.UserManagement.Models.Scope;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.UserManagement.Services
{
    /// <summary>
    /// Row-level security service implementation.
    /// Enforces field, asset, and organization scope-based access control.
    /// </summary>
    public class RowLevelSecurityService : IRowLevelSecurityService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly ILogger<RowLevelSecurityService> _logger;
        private readonly IUserService _userService;

        public RowLevelSecurityService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName,
            ILogger<RowLevelSecurityService> logger,
            IUserService userService)
        {
            _editor = editor;
            _commonColumnHandler = commonColumnHandler;
            _defaults = defaults;
            _metadata = metadata;
            _connectionName = connectionName;
            _logger = logger;
            _userService = userService;
        }

        private PPDMGenericRepository GetRepo<T>(string tableName)
        {
            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(T), _connectionName, tableName, null);
        }

        public async Task<RowAccessResult> CheckRowAccessAsync(CheckRowAccessRequest request)
        {
            try
            {
                var user = await _userService.GetByIdAsync(request.USER_ID);
                if (user == null)
                {
                    return new RowAccessResult { HasAccess = false, Reason = "User not found." };
                }

                var scopes = await GetUserScopesAsync(request.USER_ID);
                if (!scopes.Any())
                {
                    return new RowAccessResult { HasAccess = false, Reason = "No scope assignments found." };
                }

                var matchingScopes = scopes
                    .Where(s => s.SCOPE_TYPE == "FIELD" || s.SCOPE_TYPE == "ORGANIZATION" || s.SCOPE_TYPE == "ASSET")
                    .Select(s => s.SCOPE_VALUE)
                    .ToArray();

                return new RowAccessResult
                {
                    HasAccess = matchingScopes.Any(),
                    ScopeType = scopes.FirstOrDefault()?.SCOPE_TYPE,
                    MatchingScopes = matchingScopes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check row access for user {UserId}", request.USER_ID);
                return new RowAccessResult { HasAccess = false, Reason = "Error checking access." };
            }
        }

        public async Task<RowFilterResult> ApplyRowFiltersAsync(ApplyRowFiltersRequest request)
        {
            try
            {
                var scopes = await GetUserScopesAsync(request.USER_ID);
                var fieldScopes = scopes.Where(s => s.SCOPE_TYPE == "FIELD").Select(s => s.SCOPE_VALUE).ToArray();
                var orgScopes = scopes.Where(s => s.SCOPE_TYPE == "ORGANIZATION").Select(s => s.SCOPE_VALUE).ToArray();
                var assetScopes = scopes.Where(s => s.SCOPE_TYPE == "ASSET").Select(s => s.SCOPE_VALUE).ToArray();

                var filters = new List<string>();

                if (fieldScopes.Any())
                {
                    filters.Add($"FIELD_ID IN ({string.Join(",", fieldScopes.Select(f => $"'{f}'"))})");
                }

                if (orgScopes.Any())
                {
                    filters.Add($"ORGANIZATION_ID IN ({string.Join(",", orgScopes.Select(o => $"'{o}'"))})");
                }

                if (assetScopes.Any())
                {
                    filters.Add($"ASSET_ID IN ({string.Join(",", assetScopes.Select(a => $"'{a}'"))})");
                }

                return new RowFilterResult
                {
                    FilterExpressions = filters.ToArray(),
                    ScopeType = scopes.FirstOrDefault()?.SCOPE_TYPE ?? "NONE",
                    FilterCount = filters.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to apply row filters for user {UserId}", request.USER_ID);
                return new RowFilterResult { FilterExpressions = Array.Empty<string>() };
            }
        }

        public async Task<AccessResult> CheckSourceAccessAsync(CheckSourceAccessRequest request)
        {
            try
            {
                var scopes = await GetUserScopesAsync(request.USER_ID);
                var hasAccess = scopes.Any(s => s.SCOPE_VALUE == request.TARGET_SOURCE || s.SCOPE_TYPE == "GLOBAL");

                return new AccessResult
                {
                    HasAccess = hasAccess,
                    Reason = hasAccess ? null : "User does not have access to the specified source."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check source access for user {UserId}", request.USER_ID);
                return new AccessResult { HasAccess = false, Reason = "Error checking access." };
            }
        }

        public async Task<AccessResult> CheckDatabaseAccessAsync(CheckDatabaseAccessRequest request)
        {
            try
            {
                var scopes = await GetUserScopesAsync(request.USER_ID);
                var hasAccess = scopes.Any(s => s.SCOPE_VALUE == request.DATABASE_NAME || s.SCOPE_TYPE == "GLOBAL");

                return new AccessResult
                {
                    HasAccess = hasAccess,
                    Reason = hasAccess ? null : "User does not have access to the specified database."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check database access for user {UserId}", request.USER_ID);
                return new AccessResult { HasAccess = false, Reason = "Error checking access." };
            }
        }

        public async Task<AccessResult> CheckDataSourceAccessAsync(CheckDataSourceAccessRequest request)
        {
            try
            {
                var scopes = await GetUserScopesAsync(request.USER_ID);
                var hasAccess = scopes.Any(s => s.SCOPE_VALUE == request.DATASOURCE_NAME || s.SCOPE_TYPE == "GLOBAL");

                return new AccessResult
                {
                    HasAccess = hasAccess,
                    Reason = hasAccess ? null : "User does not have access to the specified data source."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check data source access for user {UserId}", request.USER_ID);
                return new AccessResult { HasAccess = false, Reason = "Error checking access." };
            }
        }

        public async Task<string[]> GetUserAccessibleFieldsAsync(string userId)
        {
            var scopes = await GetUserScopesAsync(userId);
            return scopes.Where(s => s.SCOPE_TYPE == "FIELD").Select(s => s.SCOPE_VALUE).Distinct().ToArray();
        }

        public async Task<string[]> GetUserAccessibleAssetsAsync(string userId)
        {
            var scopes = await GetUserScopesAsync(userId);
            return scopes.Where(s => s.SCOPE_TYPE == "ASSET").Select(s => s.SCOPE_VALUE).Distinct().ToArray();
        }

        public async Task<string[]> GetUserAccessibleOrganizationsAsync(string userId)
        {
            var scopes = await GetUserScopesAsync(userId);
            return scopes.Where(s => s.SCOPE_TYPE == "ORGANIZATION").Select(s => s.SCOPE_VALUE).Distinct().ToArray();
        }

        private async Task<List<UserScopeAssignment>> GetUserScopesAsync(string userId)
        {
            var scopeRepo = GetRepo<UserScopeAssignment>("USER_SCOPE_ASSIGNMENT");
            var scopes = (await scopeRepo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "USER_ID", Operator = "=", FilterValue = userId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            })).Cast<UserScopeAssignment>().ToList();

            return scopes;
        }
    }
}
