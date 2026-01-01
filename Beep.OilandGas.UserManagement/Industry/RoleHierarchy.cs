namespace Beep.OilandGas.UserManagement.Industry
{
    /// <summary>
    /// Defines role hierarchy and inheritance for Oil & Gas roles
    /// Higher roles inherit permissions from lower roles
    /// </summary>
    public static class RoleHierarchy
    {
        /// <summary>
        /// Gets the role hierarchy levels (higher number = higher privilege)
        /// </summary>
        public static Dictionary<string, int> RoleLevels { get; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { OilGasRoles.Viewer, 1 },
            { OilGasRoles.FieldOperator, 2 },
            { OilGasRoles.DataAnalyst, 3 },
            { OilGasRoles.Geologist, 4 },
            { OilGasRoles.RegulatorySpecialist, 4 },
            { OilGasRoles.EnvironmentalSpecialist, 4 },
            { OilGasRoles.SafetySpecialist, 4 },
            { OilGasRoles.Accountant, 5 },
            { OilGasRoles.LandManager, 5 },
            { OilGasRoles.DrillingEngineer, 6 },
            { OilGasRoles.CompletionsEngineer, 6 },
            { OilGasRoles.FacilitiesEngineer, 6 },
            { OilGasRoles.ProductionEngineer, 6 },
            { OilGasRoles.ReservoirEngineer, 7 },
            { OilGasRoles.PetroleumEngineer, 7 },
            { OilGasRoles.Manager, 8 },
            { OilGasRoles.Administrator, 9 }
        };

        /// <summary>
        /// Gets roles that inherit from a given role (roles at same or higher level)
        /// </summary>
        public static IEnumerable<string> GetInheritingRoles(string roleName)
        {
            if (!RoleLevels.TryGetValue(roleName, out var roleLevel))
            {
                return Enumerable.Empty<string>();
            }

            return RoleLevels
                .Where(kvp => kvp.Value >= roleLevel)
                .Select(kvp => kvp.Key)
                .ToList();
        }

        /// <summary>
        /// Gets roles that are inherited by a given role (roles at same or lower level)
        /// </summary>
        public static IEnumerable<string> GetInheritedRoles(string roleName)
        {
            if (!RoleLevels.TryGetValue(roleName, out var roleLevel))
            {
                return Enumerable.Empty<string>();
            }

            return RoleLevels
                .Where(kvp => kvp.Value <= roleLevel)
                .Select(kvp => kvp.Key)
                .ToList();
        }

        /// <summary>
        /// Checks if a role is higher than another role
        /// </summary>
        public static bool IsHigherThan(string role1, string role2)
        {
            if (!RoleLevels.TryGetValue(role1, out var level1) ||
                !RoleLevels.TryGetValue(role2, out var level2))
            {
                return false;
            }

            return level1 > level2;
        }

        /// <summary>
        /// Checks if a role is lower than another role
        /// </summary>
        public static bool IsLowerThan(string role1, string role2)
        {
            if (!RoleLevels.TryGetValue(role1, out var level1) ||
                !RoleLevels.TryGetValue(role2, out var level2))
            {
                return false;
            }

            return level1 < level2;
        }

        /// <summary>
        /// Gets the highest role from a list of roles
        /// </summary>
        public static string? GetHighestRole(IEnumerable<string> roles)
        {
            var roleList = roles?.ToList() ?? new List<string>();
            if (!roleList.Any())
            {
                return null;
            }

            var highestRole = roleList
                .Where(r => RoleLevels.ContainsKey(r))
                .OrderByDescending(r => RoleLevels[r])
                .FirstOrDefault();

            return highestRole;
        }

        /// <summary>
        /// Gets the lowest role from a list of roles
        /// </summary>
        public static string? GetLowestRole(IEnumerable<string> roles)
        {
            var roleList = roles?.ToList() ?? new List<string>();
            if (!roleList.Any())
            {
                return null;
            }

            var lowestRole = roleList
                .Where(r => RoleLevels.ContainsKey(r))
                .OrderBy(r => RoleLevels[r])
                .FirstOrDefault();

            return lowestRole;
        }
    }
}
