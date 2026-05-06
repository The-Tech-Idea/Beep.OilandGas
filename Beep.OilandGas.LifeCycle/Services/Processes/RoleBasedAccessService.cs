using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Models.Core.Interfaces;

namespace Beep.OilandGas.LifeCycle.Services.Processes
{
    public interface IRoleBasedAccessService
    {
        Task<bool> CanStartProcessAsync(string processId, string userId, IEnumerable<string> userRoles);
        Task<bool> CanExecuteStepAsync(string processId, string stepId, string userId, IEnumerable<string> userRoles);
        Task<bool> CanApproveStepAsync(string processId, string stepId, string userId, IEnumerable<string> userRoles);
        Task<IEnumerable<string>> GetRequiredRolesForProcessAsync(string processId);
        Task<IEnumerable<string>> GetRequiredRolesForStepAsync(string processId, string stepId);
        Task<Dictionary<string, IEnumerable<string>>> GetProcessRoleMatrixAsync(string processId);
    }

    public class RoleBasedAccessService : IRoleBasedAccessService
    {
        private readonly IProcessService _processService;

        public RoleBasedAccessService(IProcessService processService)
        {
            _processService = processService;
        }

        public async Task<bool> CanStartProcessAsync(string processId, string userId, IEnumerable<string> userRoles)
        {
            var processDef = await _processService.GetProcessDefinitionAsync(processId);
            if (processDef == null) return false;

            var requiredRoles = GetProcessRequiredRoles(processDef);
            if (requiredRoles.Count == 0) return true;

            return userRoles.Intersect(requiredRoles, StringComparer.OrdinalIgnoreCase).Any();
        }

        public async Task<bool> CanExecuteStepAsync(string processId, string stepId, string userId, IEnumerable<string> userRoles)
        {
            var processDef = await _processService.GetProcessDefinitionAsync(processId);
            if (processDef == null) return false;

            var step = processDef.Steps.FirstOrDefault(s => s.StepId == stepId);
            if (step == null) return false;

            var requiredRoles = GetStepRequiredRoles(step);
            if (requiredRoles.Count == 0) return true;

            return userRoles.Intersect(requiredRoles, StringComparer.OrdinalIgnoreCase).Any();
        }

        public async Task<bool> CanApproveStepAsync(string processId, string stepId, string userId, IEnumerable<string> userRoles)
        {
            var processDef = await _processService.GetProcessDefinitionAsync(processId);
            if (processDef == null) return false;

            var step = processDef.Steps.FirstOrDefault(s => s.StepId == stepId);
            if (step == null) return false;

            var approverRoles = GetStepApproverRoles(step);
            if (approverRoles.Count == 0) return true;

            return userRoles.Intersect(approverRoles, StringComparer.OrdinalIgnoreCase).Any();
        }

        public async Task<IEnumerable<string>> GetRequiredRolesForProcessAsync(string processId)
        {
            var processDef = await _processService.GetProcessDefinitionAsync(processId);
            if (processDef == null) return Enumerable.Empty<string>();

            return GetProcessRequiredRoles(processDef);
        }

        public async Task<IEnumerable<string>> GetRequiredRolesForStepAsync(string processId, string stepId)
        {
            var processDef = await _processService.GetProcessDefinitionAsync(processId);
            if (processDef == null) return Enumerable.Empty<string>();

            var step = processDef.Steps.FirstOrDefault(s => s.StepId == stepId);
            if (step == null) return Enumerable.Empty<string>();

            return GetStepRequiredRoles(step);
        }

        public async Task<Dictionary<string, IEnumerable<string>>> GetProcessRoleMatrixAsync(string processId)
        {
            var processDef = await _processService.GetProcessDefinitionAsync(processId);
            if (processDef == null) return new Dictionary<string, IEnumerable<string>>();

            var matrix = new Dictionary<string, IEnumerable<string>>();
            foreach (var step in processDef.Steps)
            {
                var roles = GetStepRequiredRoles(step);
                matrix[step.StepId] = roles;
            }

            return matrix;
        }

        private static HashSet<string> GetProcessRequiredRoles(ProcessDefinition processDef)
        {
            var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (processDef.Configuration != null && processDef.Configuration.TryGetValue("RequiredRoles", out var rolesObj))
            {
                if (rolesObj is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    foreach (var item in jsonElement.EnumerateArray())
                    {
                        if (item.ValueKind == System.Text.Json.JsonValueKind.String)
                        {
                            roles.Add(item.GetString() ?? string.Empty);
                        }
                    }
                }
                else if (rolesObj is IEnumerable<string> roleEnumerable)
                {
                    foreach (var role in roleEnumerable)
                    {
                        roles.Add(role);
                    }
                }
            }

            return roles;
        }

        private static HashSet<string> GetStepRequiredRoles(ProcessStepDefinition step)
        {
            var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (step.RequiredRoles != null)
            {
                roles.UnionWith(step.RequiredRoles);
            }

            if (step.StepConfiguration != null && step.StepConfiguration.TryGetValue("RequiredRoles", out var rolesObj))
            {
                if (rolesObj is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    foreach (var item in jsonElement.EnumerateArray())
                    {
                        if (item.ValueKind == System.Text.Json.JsonValueKind.String)
                        {
                            roles.Add(item.GetString() ?? string.Empty);
                        }
                    }
                }
                else if (rolesObj is IEnumerable<string> roleEnumerable)
                {
                    foreach (var role in roleEnumerable)
                    {
                        roles.Add(role);
                    }
                }
            }

            return roles;
        }

        private static HashSet<string> GetStepApproverRoles(ProcessStepDefinition step)
        {
            var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (step.StepConfiguration != null && step.StepConfiguration.TryGetValue("ApproverRoles", out var rolesObj))
            {
                if (rolesObj is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    foreach (var item in jsonElement.EnumerateArray())
                    {
                        if (item.ValueKind == System.Text.Json.JsonValueKind.String)
                        {
                            roles.Add(item.GetString() ?? string.Empty);
                        }
                    }
                }
                else if (rolesObj is IEnumerable<string> roleEnumerable)
                {
                    foreach (var role in roleEnumerable)
                    {
                        roles.Add(role);
                    }
                }
            }

            if (roles.Count == 0)
            {
                roles.UnionWith(GetStepRequiredRoles(step));
            }

            return roles;
        }
    }
}
